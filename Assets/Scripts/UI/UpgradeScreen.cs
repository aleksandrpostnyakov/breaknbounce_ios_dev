using System;
using Config;
using I2.Loc;
using Level;
using PlayerInfo;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class UpgradeScreen : MonoBehaviour
    {
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        [Inject] private PlayerGameInfo _playerInfo;
        [Inject] private IPurchaser _purchaser;
        [Inject] private Tutorial _tutorial;
        [Inject] private ISoundManager _soundManager;
        [Inject] private IAdsSystem _adsSystem;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _playBtn;
        [SerializeField] private Button _settingsBtn;
        [SerializeField] private Button _shareBtn;
        [SerializeField] private Button _adsBtn;
        [SerializeField] private Button _leaderboardBtn;
        [SerializeField] private TMP_Text _levelTxt;
        [SerializeField] private TopMenu _topMenu;
        [SerializeField] private UpgradesPanel _upgradesPanel;

        [SerializeField] private Canvas _coinsCanvas;
        [SerializeField] private Canvas _playBtnCanvas;
        [SerializeField] private Canvas _shieldCanvas;
        [SerializeField] private Canvas _pointerCanvas;
        [SerializeField] private Canvas _ballsCanvas;
        [SerializeField] private GameObject _tutorialHand;
        [SerializeField] private GameObject _tutorialFade;
        
        [SerializeField] private Transform _upgradesTransform;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;

        public event Action OnPlay;
        public event Action OnSettings;
        public event Action OnLeaderboard;

        private int _tutorialActiveOrder = 20;
        private int _tutorialHideOrder = 1;

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
                
                _shareBtn.transform.localScale = new Vector3(coef, coef, 0);
                _leaderboardBtn.transform.localScale = new Vector3(coef, coef, 0);
                _adsBtn.transform.localScale = new Vector3(coef, coef, 0);
                _settingsBtn.transform.localScale = new Vector3(coef, coef, 0);
            }
        }

        private void Start()
        {
            _playBtn.onClick.AddListener(OnPlayClick);
            _settingsBtn.onClick.AddListener(OnSettingsClick);
            _leaderboardBtn.onClick.AddListener(OnLeaderboardClick);
            _adsBtn.onClick.AddListener(OnAdsClick);
            _shareBtn.onClick.AddListener(() =>
            {
                if(LocalizationManager.TryGetTranslation("BnB UI/Share_decription", out var txt))
                {
                    Share(txt, string.Empty, "https://play.google.com/store/apps/details?id=com.bonanzagames.breaknbounce");
                }
            });
            
            _purchaser.Initialize();
            _purchaser.NeedUpdate += UpdateAdsButton;
        }

        public void Show()
        {
            // var coef = Mathf.Min(_cameraHandler.GetAspect() / .5625f, 1);
            // _upgradesTransform.localScale = new Vector3(coef, coef, 1);
            // var left = (coef - 1) * 450;
            // var rt = (RectTransform) _upgradesTransform;
            // rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            // LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
             if(LocalizationManager.TryGetTranslation("BnB UI/MainScreen_Level", out var txt))
             {
                 _levelTxt.text =  string.Format(txt, _playerInfo.CurrentLevel);
             }

            _topMenu.Init();
            _upgradesPanel.Init();

            
            
            UpdateAdsButton();
            _canvas.enabled = true;
            _coinsCanvas.enabled = true;
            _playBtnCanvas.enabled = true;
            _shieldCanvas.enabled = true;
            _pointerCanvas.enabled = true;
            _ballsCanvas.enabled = true;
            
            SetTutorial(_tutorial.GetStady() == TutorialStady.WinFirstLevel);
            _soundManager.PlayBackgroundMusic(BackgroundSoundId.MusicMainMenu);
            _soundManager.PlayAmbientMusic(AmbientSoundId.AmbientMainMenu);
            
            if (_playerInfo.CurrentLevel > 1 && !_playerInfo.SettingsData.AdsDisabled)
            {
                _adsSystem.ShowBanner();
            }
            
            if (_playerInfo.SettingsData.NeedShowRateUs)
            {
                OnSettingsClick();
            }
        }

        public void Hide()
        {
            _canvas.enabled = false;
            _coinsCanvas.enabled = false;
            _playBtnCanvas.enabled = false;
            _shieldCanvas.enabled = false;
            _pointerCanvas.enabled = false;
            _ballsCanvas.enabled = false;
            _tutorialFade.SetActive(false);
            _tutorialHand.SetActive(false);
        }
        private void OnPlayClick()
        {
            OnPlay?.Invoke();
        }
        
        private void OnSettingsClick()
        {
            OnSettings?.Invoke();
        }
        
        
        private void OnLeaderboardClick()
        {
            OnLeaderboard?.Invoke();
        }
        
        private void OnAdsClick()
        {
            _purchaser.BuyDisableAds();
        }
        
        private void UpdateAdsButton()
        {
            _adsBtn.gameObject.SetActive(!_playerInfo.SettingsData.AdsDisabled);
        }
        
        private void Share(string shareText, string imagePath, string url, string subject = "")
        {
             var intentClass = new AndroidJavaClass("android.content.Intent");
             var intentObject = new AndroidJavaObject("android.content.Intent");
            
             intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            
             if (imagePath != string.Empty)
             {
                 var uriClass = new AndroidJavaClass("android.net.Uri");
                 var uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + imagePath);
                 intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
                 intentObject.Call<AndroidJavaObject>("setType", "image/png");
             }
            
             intentObject.Call<AndroidJavaObject> ("setType", "text/plain");
             intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText + "\n" + url);
            
             var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
             var currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            
             var jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, subject);
             currentActivity.Call("startActivity", jChooser);
            
            
              
        }
        
        private void SetTutorial(bool isTutorial)
        {
            _settingsBtn.gameObject.SetActive(!isTutorial);
            _leaderboardBtn.gameObject.SetActive(!isTutorial);
            _adsBtn.gameObject.SetActive(!isTutorial);
            _shareBtn.gameObject.SetActive(!isTutorial);

            if (isTutorial)
            {
                _tutorial.SetStady(TutorialStady.Finish);
                
                _tutorialFade.SetActive(true);
                _coinsCanvas.sortingOrder = _tutorialActiveOrder;
                _shieldCanvas.sortingOrder = _tutorialActiveOrder;
                _tutorialHand.SetActive(true);
                _tutorialHand.transform.position = _upgradesPanel.GetCardBuyBtnPosition(1);
                _upgradesPanel.OnUpgradeCard += OnUpgradeCardTutorial;
            }
        }

        private void OnUpgradeCardTutorial(UpgradeType upgradeType)
        {
            if (upgradeType == UpgradeType.Shield)
            {
                _shieldCanvas.sortingOrder = _tutorialHideOrder;
                _pointerCanvas.sortingOrder = _tutorialActiveOrder;
                _tutorialHand.transform.position = _upgradesPanel.GetCardBuyBtnPosition(2);
            }
            else if (upgradeType == UpgradeType.Pointer)
            {
                _pointerCanvas.sortingOrder = _tutorialHideOrder;
                _ballsCanvas.sortingOrder = _tutorialActiveOrder;
                _tutorialHand.transform.position = _upgradesPanel.GetCardBuyBtnPosition(3);
            }
            else if (upgradeType == UpgradeType.Ball)
            {
                _ballsCanvas.sortingOrder = _tutorialHideOrder;
                _playBtnCanvas.sortingOrder = _tutorialActiveOrder;
                _tutorialHand.transform.position = _playBtn.transform.position;
                _upgradesPanel.OnUpgradeCard -= OnUpgradeCardTutorial;
            }
        }

        private void OnDestroy()
        {
            _playBtn.onClick.RemoveAllListeners();
            _shareBtn.onClick.RemoveAllListeners();
            _adsBtn.onClick.RemoveAllListeners();
            _settingsBtn.onClick.RemoveListener(OnSettingsClick);
            _leaderboardBtn.onClick.RemoveListener(OnLeaderboardClick);
            _purchaser.NeedUpdate -= UpdateAdsButton;
        }
    }
}