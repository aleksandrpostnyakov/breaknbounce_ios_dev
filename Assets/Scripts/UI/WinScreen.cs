using System;
using System.Collections;
using Config;
using Core;
using PlayerInfo;
using Sound;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class WinScreen : MonoBehaviour
    {
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        [Inject] private PlayerGameInfo _playerInfo;
        [Inject] private GameConfig _gameConfig;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private ISoundManager _soundManager;
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _playBtn;
        [SerializeField] private Leaderboard.Leaderboard _leaderboard;
        [SerializeField] private TopMenu _topMenu;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;
        
        [Header("LOTTERY")]
        [SerializeField] private Transform _arrowTransform;
        [SerializeField] private Button _lotteryBtn;
        [SerializeField] private TMP_Text _rewardTxt;
        [SerializeField] private TMP_Text _newRewardTxt;
        [SerializeField] private TMP_Text _mult1_1_Txt;
        [SerializeField] private TMP_Text _mult1_2_Txt;
        [SerializeField] private TMP_Text _mult2_1_Txt;
        [SerializeField] private TMP_Text _mult2_2_Txt;
        [SerializeField] private TMP_Text _mult3_1_Txt;
        
        public event Action OnPlay;
        public event Action<float> OnLottery;

        private float lotteryAngle = 0;
        private int lotteryDirection = -1;
        private float mult = 2;
        private Coroutine _angleRoutine;
        private Coroutine _pauseNoThanksRoutine;
        
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
            _playBtn.onClick.AddListener(OnPlayClick);
            _lotteryBtn.onClick.AddListener(OnLotteryClick);
        }
        
        public void Show(int coins)
        {
            _topMenu.Init(true);
            PrepareLottery(coins);
            _leaderboard.Init(_playerInfo.GetXP, _playerInfo.GetOldXP, 3);
            _playerInfo.UpdateOldXp();
            _soundManager.PlayUISound(UISoundId.Win);
            _soundManager.PlayBackgroundMusic(BackgroundSoundId.MusicMainMenu);
            
            _playBtn.gameObject.SetActive(false);
            
            _canvas.enabled = true;
            _pauseNoThanksRoutine = StartCoroutine(ShowNoThanks());
        }

        private IEnumerator ShowNoThanks()
        {
            yield return new WaitForSeconds(3f);
            _playBtn.gameObject.SetActive(true);
        }

        private void ResetRoutine()
        {
            if (_angleRoutine != null)
            {
                StopCoroutine(_angleRoutine);
                _angleRoutine = null;
            }
            
            if (_pauseNoThanksRoutine != null)
            {
                StopCoroutine(_pauseNoThanksRoutine);
                _pauseNoThanksRoutine = null;
            }
        }

        private void PrepareLottery(int coins)
        {
            _rewardTxt.text = "+" + coins;
            _angleRoutine = StartCoroutine(AngleCoroutine(coins));

            var config = _gameConfig.GetHudConfig;
            _mult1_1_Txt.text = "X" + config.RewardsMultiplier1;
            _mult1_2_Txt.text = "X" + config.RewardsMultiplier1;
            _mult2_1_Txt.text = "X" + config.RewardsMultiplier2;
            _mult2_2_Txt.text = "X" + config.RewardsMultiplier2;
            _mult3_1_Txt.text = "X" + config.RewardsMultiplier3;
            _lotteryBtn.enabled = true;
        }
        
        private IEnumerator AngleCoroutine(int coins)
        {
            var coef = _gameConfig.LotterySpeed * Time.deltaTime;
            var config = _gameConfig.GetHudConfig;
            
            while (true)
            {
                if (lotteryDirection > 0)
                {
                    lotteryAngle -= coef;
                    if (lotteryAngle <= -85)
                    {
                        lotteryDirection = -1;
                    }
                }
                else
                {
                    lotteryAngle += coef;
                    if (lotteryAngle >= 85)
                    {
                        lotteryDirection = 1;
                    }
                }

                
                mult = Math.Abs(lotteryAngle) switch
                {
                    > 52 => config.RewardsMultiplier1,
                    > 18 => config.RewardsMultiplier2,
                    _ => config.RewardsMultiplier3
                };
                
                var arrowTransformRotation = Quaternion.Euler(0, 0, lotteryAngle);
                _arrowTransform.localRotation = arrowTransformRotation;
                _newRewardTxt.text = ((int)(coins * mult)).ToString();
                yield return new WaitForEndOfFrame();
            }
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
        private void OnPlayClick()
        {
            CheckNeedRate();
            ResetRoutine();
            OnPlay?.Invoke();

            if (!_playerInfo.SettingsData.AdsDisabled && _playerInfo.CurrentLevel > 3 )
            {
                _adsSystem.ShowInterstitial();
            }
        }
        
        private void OnLotteryClick()
        {
            CheckNeedRate();
            ResetRoutine();
            _lotteryBtn.enabled = false;

            ShowAds();
        }

        private async void ShowAds()
        {
            Debug.Log("SHOW ADS " + _platformType);
            if (_platformType == PlatformType.AndroidAds)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                _adsSystem.OnShowRewardedAds += AdsSystemOnOnShowRewardedAds;
                await _adsSystem.ShowAdsForWinScreen();
            }
            else
            {
                var result = await _adsSystem.ShowAdsForWinScreen();
                if (result)
                {
                    OnLotteryReward();
                }
                else
                {
                    OnLottery?.Invoke(1);
                }
            }
        }

        private void AdsSystemOnOnShowRewardedAds(BaseAdsSystem.AdsType adsType, bool result)
        {
            if (adsType == BaseAdsSystem.AdsType.WinLevel)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                OnLottery?.Invoke(result ? mult : 1);
            }
        }

        private void OnLotteryReward()
        {
            OnLottery?.Invoke(mult);
        }

        private void CheckNeedRate()
        {
            if (_playerInfo.CurrentLevel == 11)
            {
                _playerInfo.SettingsData.NeedShowRateUs = true;
            }
        }

        private void OnDestroy()
        {
            _playBtn.onClick.RemoveAllListeners();
            _lotteryBtn.onClick.RemoveAllListeners();
        }
    }
}