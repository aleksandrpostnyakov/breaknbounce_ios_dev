using System;
using System.Collections;
using Analytics;
using ByteBrewSDK;
using Config;
using Core;
using PlayerInfo;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class ReviveScreen: MonoBehaviour
    {
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private ISoundManager _soundManager;
        [Inject] private AnalyticsService _analytics;
        [Inject] private PlayerGameInfo _playerGameInfo;
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        
        [SerializeField] private TMP_Text _timerTxt;
        [SerializeField] private Button _ressurectionBtn;
        [SerializeField] private Button _noBtn;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;

        public Action OnNoBtnClick;
        public Action OnRessurectionBtnClick;

        private Coroutine _timerRoutine;
        private int _time;
        
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

        public void Start()
        {
            _ressurectionBtn.onClick.AddListener(OnRessurection);
            _noBtn.onClick.AddListener(ResetRoutine);
        }

        private void OnRessurection()
        {
            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
            }
            
            ShowAds();
        }
        
        private async void ShowAds()
        {
            if (_platformType == PlatformType.AndroidAds)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                _adsSystem.OnShowRewardedAds += AdsSystemOnOnShowRewardedAds;
                await _adsSystem.ShowAdsForRevive();
            }
            else
            {
                var result = await _adsSystem.ShowAdsForRevive();
                if (result)
                {
                    OnRevive();
                }
            }
        }

        private void AdsSystemOnOnShowRewardedAds(BaseAdsSystem.AdsType adsType, bool result)
        {
            if (adsType == BaseAdsSystem.AdsType.Revive)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                OnRevive();
            }
        }

        private void OnRevive()
        {
            Hide();
            OnRessurectionBtnClick?.Invoke();
        }

        private void ResetRoutine()
        {
            if (_timerRoutine != null)
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
            }
            
            _analytics.ByteBrewLevelEvent(_playerGameInfo.CurrentLevel, "failed_skip=1;");
            _analytics.ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes.Failed, _playerGameInfo.CurrentLevel, "skip_revive");

            Hide();
            OnNoBtnClick?.Invoke();
        }

        public void Show()
        {
            _canvas.enabled = true;
            _time = 15;
            _timerRoutine = StartCoroutine(TimeCoroutine());
            _soundManager.PlayUISound(UISoundId.Revive);
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }

        private IEnumerator TimeCoroutine()
        {
            while (_time > 0)
            {
                _timerTxt.text = _time.ToString();
                yield return new WaitForSeconds(1f);
                _time--;
            }
            
            ResetRoutine();
        }
    }
}