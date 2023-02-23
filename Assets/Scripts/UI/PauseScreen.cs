using System;
using Analytics;
using ByteBrewSDK;
using PlayerInfo;
using Sound;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class PauseScreen : MonoBehaviour
    {
        [Inject] private ISoundManager _soundManager;
        [Inject] private PlayerGameInfo _playerInfo;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private AnalyticsService _analytics;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _continueBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _vibroBtn;
        [SerializeField] private Button _soundBtn;
        [SerializeField] private Button _musicBtn;
        [SerializeField] private Image _vibroBtnImage;
        [SerializeField] private Image _soundBtnImage;
        [SerializeField] private Image _musicBtnImage;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _passiveSprite;

        public event Action OnExit;

        private void Start()
        {
            _closeBtn.onClick.AddListener(OnClose);
            _continueBtn.onClick.AddListener(OnClose);
            _homeBtn.onClick.AddListener(OnHome);
            
            _vibroBtn.onClick.AddListener(SwitchVibro);
            _soundBtn.onClick.AddListener(SwitchSounds);
            _musicBtn.onClick.AddListener(SwitchMusic);
        }
        
        public void Show()
        {
            PrepareButton(_musicBtnImage, _soundManager.Settings.MusicOn);
            PrepareButton(_soundBtnImage, _soundManager.Settings.SoundsOn);
            PrepareButton(_vibroBtnImage, _soundManager.Settings.VibroOn);
            
            _canvas.enabled = true;
        }

        private void SwitchMusic()
        {
            _soundManager.ToggleMusic();
            PrepareButton(_musicBtnImage, _soundManager.Settings.MusicOn);
        }

        private void SwitchSounds()
        {
            _soundManager.ToggleSounds();
            PrepareButton(_soundBtnImage, _soundManager.Settings.SoundsOn);
        }

        private void SwitchVibro()
        {
            _soundManager.ToggleVibro();
            PrepareButton(_vibroBtnImage, _soundManager.Settings.VibroOn);
        }

        private void PrepareButton(Image btnImage, bool active)
        {
            btnImage.sprite = active ? _activeSprite : _passiveSprite;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }

        private void OnClose()
        {
            Hide();
        }
        
        private void OnHome()
        {
            _analytics.ByteBrewLevelEvent(_playerInfo.CurrentLevel, "skip_level=1;");
            _analytics.ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes.Failed, _playerInfo.CurrentLevel, "exit_from_pause");

            OnExit?.Invoke();
            Hide();
            
            if (!_playerInfo.SettingsData.AdsDisabled && _playerInfo.CurrentLevel > 2 )
            {
                _adsSystem.ShowInterstitial();
            }
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveListener(OnClose);
            _continueBtn.onClick.RemoveListener(OnClose);
            _homeBtn.onClick.RemoveListener(OnHome);
            
            _vibroBtn.onClick.RemoveListener(SwitchVibro);
            _soundBtn.onClick.RemoveListener(SwitchSounds);
            _musicBtn.onClick.RemoveListener(SwitchMusic);
        }
    }
}