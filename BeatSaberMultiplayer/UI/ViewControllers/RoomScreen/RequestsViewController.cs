﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMultiplayerLite.Data;
using IPA.Utilities;
using BeatSaberMultiplayerLite.Misc;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace BeatSaberMultiplayerLite.UI.ViewControllers.RoomScreen
{
    public class RequestsViewController : BSMLResourceViewController, TableView.IDataSource
    {
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        public event Action BackPressed;
        public event Action<SongInfo> SongSelected;
        public event Action<SongInfo> RemovePressed;

        [UIComponent("song-list")]
        CustomListTableData _songsTableView;

        [UIComponent("play-btn")]
        Button _playButton;
        [UIComponent("remove-btn")]
        Button _removeButton;

        private LevelListTableCell songListTableCellInstance;
        private AdditionalContentModel _additionalContentModel;
        private BeatmapLevelsModel _beatmapLevelsModel;

        List<SongInfo> requestedSongs = new List<SongInfo>();

        SongInfo _selectedSong;

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            if (firstActivation)
            {
                _songsTableView.tableView.didSelectCellWithIdxEvent += SongsTableView_DidSelectRow;
                _songsTableView.tableView.dataSource = this;

                _additionalContentModel = Resources.FindObjectsOfTypeAll<AdditionalContentModel>().First(); 
                _beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().First();
            }

            _selectedSong = null;
            _playButton.interactable = false;
            _removeButton.interactable = false;
        }

        public void SetSongs(List<SongInfo> songs)
        {
            requestedSongs = songs;
            
            _songsTableView.tableView.ReloadData();

            if (_selectedSong != null)
            {

                int index = requestedSongs.FindIndex(x => x.hash == _selectedSong.hash);

                if (index != -1)
                {
                    _songsTableView.tableView.ScrollToCellWithIdx(index, TableViewScroller.ScrollPositionType.Beginning, false);
                    _songsTableView.tableView.SelectCellWithIdx(index, false);
                }
                else
                {
                    _songsTableView.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);
                    _selectedSong = null;
                    _playButton.interactable = false;
                    _removeButton.interactable = false;
                }
            }
            else
            {
                _songsTableView.tableView.ScrollToCellWithIdx(0, TableViewScroller.ScrollPositionType.Beginning, false);
                _playButton.interactable = false;
                _removeButton.interactable = false;
            }

            Plugin.log.Debug($"Set list of {songs.Count} songs!");
        }

        [UIAction("back-pressed")]
        public void BackButtonPressed()
        {
            BackPressed?.Invoke();
        }

        [UIAction("play-pressed")]
        public void PlayButtonPressed()
        {
            SongSelected?.Invoke(_selectedSong);
        }

        [UIAction("remove-pressed")]
        public void RemoveButtonPressed()
        {
            RemovePressed?.Invoke(_selectedSong);
        }

        private void SongsTableView_DidSelectRow(TableView arg1, int arg2)
        {
            _selectedSong = requestedSongs[arg2];
            _playButton.interactable = true;
            _removeButton.interactable = true;
        }

        public float CellSize()
        {
            return 10f;
        }

        public int NumberOfCells()
        {
            return requestedSongs.Count;
        }

        public TableCell CellForIdx(TableView tableView, int idx)
        {
            LevelListTableCell tableCell = (LevelListTableCell)tableView.DequeueReusableCellForIdentifier(_songsTableView.reuseIdentifier);
            if (!tableCell)
            {
                if (songListTableCellInstance == null)
                    songListTableCellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                tableCell = Instantiate(songListTableCellInstance);
            }

            var level = _beatmapLevelsModel.allLoadedBeatmapLevelPackCollection.beatmapLevelPacks.SelectMany(x => x.beatmapLevelCollection.beatmapLevels).FirstOrDefault(x => x.levelID == requestedSongs[idx].levelId);

            if (level != null)
            {
                tableCell.SetDataFromLevelAsync(level, false);
                tableCell.RefreshAvailabilityAsync(_additionalContentModel, level.levelID);
            }
            else
            {
                tableCell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText").text = string.Format("{0} <size=80%>{1}</size>", level.songName, level.songSubName);
                tableCell.GetField<TextMeshProUGUI, LevelListTableCell>("_authorText").text = level.songAuthorName;

                var coverImage = tableCell.GetField<RawImage, LevelListTableCell>("_coverRawImage");
                coverImage.texture = null;
                coverImage.color = Color.clear;

                Image[] chars = tableCell.GetField<Image[], LevelListTableCell>("_beatmapCharacteristicImages");

                foreach(var img in chars)
                {
                    img.enabled = false;
                }

                SongDownloader.Instance.RequestSongByLevelID(requestedSongs[idx].hash, (info) =>
                {
                    tableCell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText").text = string.Format("{0} <size=80%>{1}</size>", info.songName, info.songSubName);
                    tableCell.GetField<TextMeshProUGUI, LevelListTableCell>("_authorText").text = info.songAuthorName;

                    StartCoroutine(LoadScripts.LoadSpriteCoroutine(info.coverURL, (cover) => { coverImage.texture = cover; }));
                });
            }

            tableCell.reuseIdentifier = _songsTableView.reuseIdentifier;
            return tableCell;
        }
    }
}
