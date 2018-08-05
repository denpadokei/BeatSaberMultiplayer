﻿using BeatSaberMultiplayer.Misc;
using BeatSaberMultiplayer.UI.ViewControllers.CreateRoomScreen;
using ServerHub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRUI;

namespace BeatSaberMultiplayer.UI.FlowCoordinators
{
    class RoomCreationFlowCoordinator : FlowCoordinator
    {
        RoomCreationServerHubsListViewController _serverHubsViewController;
        RoomCreationViewController _roomCreationViewController;

        ServerHubClient _selectedServerHub;
        RoomSettings _roomSettings;

        uint _createdRoomId;

        public void MainCreateRoomButtonPressed(VRUIViewController parentViewController, List<ServerHubClient> serverHubs)
        {
            if(_serverHubsViewController == null)
            {
                _serverHubsViewController = BeatSaberUI.CreateViewController<RoomCreationServerHubsListViewController>();
                _serverHubsViewController.selectedServerHub += ServerHubSelected;
            }

            parentViewController.PresentModalViewController(_serverHubsViewController, null);
            _serverHubsViewController.SetServerHubs(serverHubs);

        }

        public void ServerHubSelected(ServerHubClient serverHubClient)
        {
            _selectedServerHub = serverHubClient;

            if(_roomCreationViewController == null)
            {
                _roomCreationViewController = BeatSaberUI.CreateViewController<RoomCreationViewController>();
                _roomCreationViewController.CreatedRoom += CreateRoomPressed;
            }

            _serverHubsViewController.PresentModalViewController(_roomCreationViewController, null);
        }
        
        public void CreateRoomPressed(RoomSettings settings)
        {
            _roomSettings = settings;

            if (Client.instance == null)
            {
                Log.Info("Creating client");
                Client.CreateClient();
            }
            if(!Client.instance.Connected || (Client.instance.Connected && (Client.instance.ip != _selectedServerHub.ip || Client.instance.port != _selectedServerHub.port)))
            {
                Client.instance.Disconnect(true);
                Client.instance.Connect(_selectedServerHub.ip, _selectedServerHub.port);
                Client.instance.ConnectedToServerHub += ConnectedToServerHub;
            }
            else
            {
                ConnectedToServerHub();
            }
            
        }

        public void ConnectedToServerHub()
        {
            Client.instance.ConnectedToServerHub -= ConnectedToServerHub;
            Client.instance.CreateRoom(_roomSettings);
            Client.instance.PacketReceived += PacketReceived;
        }

        public void PacketReceived(BasePacket packet)
        {
            if(packet.commandType == CommandType.CreateRoom)
            {

                Client.instance.PacketReceived -= PacketReceived;
                _createdRoomId = BitConverter.ToUInt32(packet.additionalData, 0);
                _roomCreationViewController.DismissModalViewController(null, true);
                _serverHubsViewController.DismissModalViewController(null, true);

                PluginUI.instance.serverHubFlowCoordinator.JoinRoom(_selectedServerHub.ip, _selectedServerHub.port, _createdRoomId, _roomSettings.UsePassword, _roomSettings.Password);
            }
        }
    }
}