﻿using BeatSaberMultiplayerLite.Data;
using BeatSaberMultiplayerLite.Misc;
using SongCore.Utilities;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace BeatSaberMultiplayerLite
{
    public class PlayerInfoDisplay : MonoBehaviour
    {
        private PlayerScore _playerScore;

        public TextMeshPro playerNameText;
        public TextMeshPro playerScoreText;
        public TextMeshPro playerPlaceText;

        public Image playerSpeakerIcon;

        private uint _previousScore;
        private uint _currentScore;
        private float _progress;
        private bool _rainbowName;
        private HSBColor _color;

        void Update()
        {
            if (_playerScore != default)
            {
                _progress += Time.deltaTime * Client.Instance.tickrate;
                uint score = (uint)Mathf.Lerp(_previousScore, _currentScore, Mathf.Clamp01(_progress));

                playerScoreText.text = score.ToString();

                if (_rainbowName)
                {
                    playerNameText.color = HSBColor.ToColor(_color);
                    _color.h += 0.125f * Time.deltaTime;
                    if (_color.h >= 1f)
                    {
                        _color.h = 0f;
                    }
                }
            }
        }

        void Awake()
        {
            playerPlaceText = CustomExtensions.CreateWorldText(transform, "");
            playerPlaceText.rectTransform.anchoredPosition = new Vector2(42.5f, -47.5f);
            playerPlaceText.fontSize = 8f;

            playerNameText = CustomExtensions.CreateWorldText(transform, "");
            playerNameText.rectTransform.anchoredPosition = new Vector2(44f, -47.5f);
            playerNameText.fontSize = 7f;

            playerScoreText = CustomExtensions.CreateWorldText(transform, "");
            playerScoreText.rectTransform.anchoredPosition = new Vector2(55f, -47.5f);
            playerScoreText.fontSize = 8f;

            playerSpeakerIcon = new GameObject("Player Speaker Icon", typeof(Canvas), typeof(CanvasRenderer)).AddComponent<Image>();
            playerSpeakerIcon.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            playerSpeakerIcon.rectTransform.SetParent(transform);
            playerSpeakerIcon.rectTransform.localScale = new Vector3(0.008f, 0.008f, 1f);
            playerSpeakerIcon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            playerSpeakerIcon.rectTransform.anchoredPosition3D = new Vector3(-8.5f, 2f, 0f);
            playerSpeakerIcon.sprite = Sprites.speakerIcon;
        }

        public void UpdatePlayerInfo(PlayerScore _info, int _index)
        {
            _playerScore = _info;

            if (_playerScore != default && _info.valid)
            {
                playerPlaceText.text = (_index + 1).ToString();
                playerNameText.text = _playerScore.name;
                playerNameText.color = _playerScore.color;
                _previousScore = _currentScore;
                _currentScore = _playerScore.score;
                playerSpeakerIcon.gameObject.SetActive(InGameOnlineController.Instance.VoiceChatIsTalking(_playerScore.id));
                _progress = 0;

                if (_info.playerFlags.rainbowName && !_rainbowName)
                {
                    playerNameText.color = _info.color;
                    _color = HSBColor.FromColor(_info.color);
                }
                else if (!_info.playerFlags.rainbowName && playerNameText.color != _info.color)
                {
                    playerNameText.color = _info.color;
                }

                _rainbowName = _info.playerFlags.rainbowName;
            }
            else
            {
                playerPlaceText.text = "";
                playerNameText.text = "";
                playerScoreText.text = "";
                playerSpeakerIcon.gameObject.SetActive(false);
                _rainbowName = false;
            }
        }

    }
}
