using System;
using Config;
using Sound;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class SettingsScreen : MonoBehaviour
    {
        [Inject] private ISoundManager _soundManager;
        [Inject] private PlayerInfo.PlayerGameInfo _playerGameInfo;
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        
        [SerializeField] private TopMenu _topMenu;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _ratePanel;
        [SerializeField] private GameObject _privacyPanel;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _passiveSprite;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;
        
        [Header("Setting Panel")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _vibroBtn;
        [SerializeField] private Button _soundBtn;
        [SerializeField] private Button _musicBtn;
        [SerializeField] private Button _rateUsBtn;
        [SerializeField] private Button _privacyBtn;
        [SerializeField] private Image _vibroBtnImage;
        [SerializeField] private Image _soundBtnImage;
        [SerializeField] private Image _musicBtnImage;
        
        [Header("Rate Panel")]
        [SerializeField] private Button _closeRateBtn;
        [SerializeField] private Button _rateBtn;
        [SerializeField] private Image _rateBtnImage;
        [SerializeField] private Button _starBtn1;
        [SerializeField] private Button _starBtn2;
        [SerializeField] private Button _starBtn3;
        [SerializeField] private Button _starBtn4;
        [SerializeField] private Button _starBtn5;
        [SerializeField] private Image _starBtnImage1;
        [SerializeField] private Image _starBtnImage2;
        [SerializeField] private Image _starBtnImage3;
        [SerializeField] private Image _starBtnImage4;
        [SerializeField] private Image _starBtnImage5;
        [SerializeField] private Sprite _activeStarSprite;
        [SerializeField] private Sprite _passiveStarSprite;
        [SerializeField] private Sprite _activeRateBtnSprite;
        [SerializeField] private Sprite _passiveRateBtnSprite;
        
        [Header("Privacy Panel")]
        [SerializeField] private Button _closePrivacyBtn;
        [SerializeField] private Button _acceptPrivacyBtn;
        [SerializeField] private Button _policyPrivacyBtn;
        [SerializeField] private Button _termsPrivacyBtn;
        
        public event Action OnExit;

        private int _currentStar;
        
        private void Awake()
        {
            var coef = _cameraHandler.GetAspect() / .5625f;
            float screenCoef = 1;
            if (coef != 1)
            {
                if (coef > 1)
                {
                    coef = 1 / coef;
                }

                var rt = _centralBlockTransform as RectTransform;
                var widthCoef = Screen.width / rt.rect.width;
                var heightCoef = Screen.height / rt.rect.height;

                if (widthCoef < 1 || heightCoef < 1)
                {
                    if (widthCoef < 1 && heightCoef < 1)
                    {
                        screenCoef = widthCoef < heightCoef ? widthCoef : heightCoef;
                    }
                    else if (widthCoef < 1)
                    {
                        screenCoef = widthCoef;
                    }
                    else
                    {
                        screenCoef = heightCoef;
                    }
                }
                else if (widthCoef > heightCoef)
                {
                    screenCoef = 1 / widthCoef;
                }
                else
                {
                    screenCoef = 1 / heightCoef;
                }
                
                var uiScale = _uiTransform.localScale.x;

                if (uiScale < 1)
                {
                    uiScale = 1 / uiScale;
                }

                screenCoef *= uiScale;
                _centralBlockTransform.localScale = new Vector3(screenCoef, screenCoef, 1);
            }
        }

        private void Start()
        {
            _closeBtn.onClick.AddListener(OnClose);
            _closeRateBtn.onClick.AddListener(OnClose);
            _closePrivacyBtn.onClick.AddListener(OnClose);
            
            _vibroBtn.onClick.AddListener(SwitchVibro);
            _soundBtn.onClick.AddListener(SwitchSounds);
            _musicBtn.onClick.AddListener(SwitchMusic);
            _rateUsBtn.onClick.AddListener(ShowRateUs);
            _privacyBtn.onClick.AddListener(ShowPrivacy);
            
            _acceptPrivacyBtn.onClick.AddListener(AcceptPrivacy);
            _policyPrivacyBtn.onClick.AddListener(OpenPrivacy);
            _termsPrivacyBtn.onClick.AddListener(ShowTerms);
            
            _starBtn1.onClick.AddListener(() => {ClickStar(1);});
            _starBtn2.onClick.AddListener(() => {ClickStar(2);});
            _starBtn3.onClick.AddListener(() => {ClickStar(3);});
            _starBtn4.onClick.AddListener(() => {ClickStar(4);});
            _starBtn5.onClick.AddListener(() => {ClickStar(5);});
            
            _rateBtn.onClick.AddListener(OnRate);
        }
       
        public void Show()
        {
            PrepareButton(_musicBtnImage, _soundManager.Settings.MusicOn);
            PrepareButton(_soundBtnImage, _soundManager.Settings.SoundsOn);
            PrepareButton(_vibroBtnImage, _soundManager.Settings.VibroOn);
            
            _currentStar = _playerGameInfo.SettingsData.RateStars;
            PrepareStars();
            
            _topMenu.Init();
            _settingsPanel.SetActive(true);
            _ratePanel.SetActive(false);
            _privacyPanel.SetActive(false);
            
            _privacyBtn.gameObject.SetActive(!_playerGameInfo.SettingsData.PrivacyAccepted);

            if (_playerGameInfo.SettingsData.NeedShowRateUs)
            {
                _playerGameInfo.SettingsData.NeedShowRateUs = false;
                ShowRateUs();
            }
            
            _canvas.enabled = true;
        }
        
        private void ShowRateUs()
        {
            _settingsPanel.SetActive(false);
            _ratePanel.SetActive(true);
            _privacyPanel.SetActive(false);
        }
        
        private void ShowPrivacy()
        {
            _settingsPanel.SetActive(false);
            _ratePanel.SetActive(false);
            _privacyPanel.SetActive(true);
        }
        
        public void Hide()
        {
            _canvas.enabled = false;
        }
        
        private void OnClose()
        {
            Hide();
            OnExit?.Invoke();
        }
        
        private void PrepareButton(Image btnImage, bool active)
        {
            btnImage.sprite = active ? _activeSprite : _passiveSprite;
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

        private void OnRate()
        {
            if (_currentStar < 5)
            {
                OnClose();
            }
            else
            {
                Application.OpenURL("market://details?id=" + Application.identifier);
                OnClose();
            }
        }
        
        private void PrepareStar(Image btnImage, int id)
        {
            btnImage.sprite = id <= _currentStar ? _activeStarSprite : _passiveStarSprite;
        }

        private void PrepareStars()
        {
            PrepareStar(_starBtnImage1, 1);
            PrepareStar(_starBtnImage2, 2);
            PrepareStar(_starBtnImage3, 3);
            PrepareStar(_starBtnImage4, 4);
            PrepareStar(_starBtnImage5, 5);
            
            _rateBtnImage.sprite = _currentStar > 0 ? _activeRateBtnSprite : _passiveRateBtnSprite;
            _rateBtn.enabled = _currentStar > 0;
        }

        private void ClickStar(int id)
        {
            _currentStar = id;
            PrepareStars();
            _playerGameInfo.Rate(id);
        }
        
        private void AcceptPrivacy()
        {
            _playerGameInfo.PrivacyAccept();
            OnClose();
        }
        
        private void ShowTerms()
        {
            Application.OpenURL("https://www.bonanzagames.llc/terms");
        }

        private void OpenPrivacy()
        {
            Application.OpenURL("https://www.bonanzagames.llc/privacy");
        }
        
        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveListener(OnClose);
            _closeRateBtn.onClick.RemoveListener(OnClose);
            _closePrivacyBtn.onClick.RemoveListener(OnClose);
            
            _vibroBtn.onClick.RemoveListener(SwitchVibro);
            _soundBtn.onClick.RemoveListener(SwitchSounds);
            _musicBtn.onClick.RemoveListener(SwitchMusic);
            
            _rateBtn.onClick.RemoveListener(OnRate);
            
            _starBtn1.onClick.RemoveAllListeners();
            _starBtn2.onClick.RemoveAllListeners();
            _starBtn3.onClick.RemoveAllListeners();
            _starBtn4.onClick.RemoveAllListeners();
            _starBtn5.onClick.RemoveAllListeners();
            
            _acceptPrivacyBtn.onClick.RemoveAllListeners();
            _policyPrivacyBtn.onClick.RemoveAllListeners();
            _termsPrivacyBtn.onClick.RemoveAllListeners();
        }
    }
}