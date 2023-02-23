using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Advertusement
{
    public class AndroidAppLovinMediationSystem : AppLovinMediationAdsSystem
    {
        private const string _androidGameId = "4734717";
        protected override string GameId => _androidGameId;
        protected override string AdsName => "Rewarded_Android";
    }
    
    public class AppLovinMediationAdsSystem : BaseAdsSystem, ITickable
    {
        protected override string GameId { get; }
        protected override string AdsName { get; }

        private string SDK_KEY =
            "-CaTixODxqx6-A4XBDbK89w_R02bzHIK1CfHCk477Tg1ZPBQIXlLxRX_yiUzaOI-A-vzSh6XDzOWTiAudyozQV";

        private string _adInterstitialUnitId = "95e115169cd6b87f";
        private string _adUnitId = "be6ee55152411cc8";
        private string bannerAdUnitId = "d24d200a13f5185c";
        private int _retryInterstitialAttempt;
        private int _retryRewardedAttempt;

        private bool _bannerShowed;

        protected bool _waitStartInterstitial;
        
        protected AppLovinMediationAdsSystem()
        {
            Init();
        }

        private void Init()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                InitializeInterstitialAds();
                InitializeRewardedAds();
                InitializeBannerAds();
            };

            MaxSdk.SetSdkKey(SDK_KEY);
            //MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
        }

#region Interstittial
    public void InitializeInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
    
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(_adInterstitialUnitId);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
            // Reset retry attempt
            _retryInterstitialAttempt = 0;
        }

        private async void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

            _retryInterstitialAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _retryInterstitialAttempt));
    
            //Invoke("LoadInterstitial", (float) retryDelay);
            await new WaitForSeconds((float)retryDelay);
            LoadInterstitial();
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad.
            LoadInterstitial();
        }
        
        public override bool ShowInterstitial()
        {
            if ( MaxSdk.IsInterstitialReady(_adInterstitialUnitId) )
            {
                MaxSdk.ShowInterstitial(_adInterstitialUnitId);
                return true;
            }

            return false;
        }
        
        public override void ShowStartInterstitial(bool needShow)
        {
            if (!needShow)
            {
                _startAdsShowed = true;
                return;
            }

            if (!_startAdsShowed)
            {
                if ( MaxSdk.IsInterstitialReady(_adInterstitialUnitId))
                {
                    MaxSdk.ShowInterstitial(_adInterstitialUnitId);
                    _startAdsShowed = true;
                }
                else
                {
                    _waitStartInterstitial = true;
                }
            }
        }
#endregion

#region Revarded
        public void InitializeRewardedAds()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
                    
            // Load the first rewarded ad
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(_adUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _retryRewardedAttempt = 0;
        }

        private async void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            _retryRewardedAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryRewardedAttempt));
            
            //Invoke("LoadRewardedAd", (float) retryDelay);
            await new WaitForSeconds((float)retryDelay);
            LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            OnShowRewardedAdsResult(_currentlyViewingAdType, false);
            LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            OnShowRewardedAdsResult(_currentlyViewingAdType, true);
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
        
        private bool _isRewardedShowingNow;
        public override async Task<bool> ShowAdsForWinScreen()
        {
            if (!MaxSdk.IsRewardedAdReady(_adUnitId))
            {
                OnNoAds();
                return false;
            }
            
            if (_isRewardedShowingNow)
                return false;
            _isRewardedShowingNow = true;

            _currentlyViewingAdType = AdsType.WinLevel;
            
            MaxSdk.ShowRewardedAd(_adUnitId);
            
            _isRewardedShowingNow = false;
            return false;
        }
        
        public override async Task<bool> ShowAdsForUpgrade(int upgradeId)
        {
            if (!MaxSdk.IsRewardedAdReady(_adUnitId))
            {
                OnNoAds();
                return false;
            }
            
            if (_isRewardedShowingNow)
                return false;
            _isRewardedShowingNow = true;

            _currentlyViewingAdType = AdsType.UpgradeCard;
            
            MaxSdk.ShowRewardedAd(_adUnitId);
            
            _isRewardedShowingNow = false;
            return false;
        }
        
        public override async Task<bool> ShowAdsForRevive()
        {
            if (!MaxSdk.IsRewardedAdReady(_adUnitId))
            {
                OnNoAds();
                return false;
            }
            
            if (_isRewardedShowingNow)
                return false;
            _isRewardedShowingNow = true;

            _currentlyViewingAdType = AdsType.Revive;
            
            MaxSdk.ShowRewardedAd(_adUnitId);
            
            _isRewardedShowingNow = false;
            return false;
        }
#endregion

#region Banner
        private void InitializeBannerAds()
        {
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, Color.white);

            // MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
            // MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
            // MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
            // MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            // MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
            // MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {}

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)  {}

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {}

        public override void ShowBanner()
        {
            if (_bannerShowed)
            {
                return;
            }

            _bannerShowed = true;
            MaxSdk.ShowBanner(bannerAdUnitId);
        }

        public override void HideBanner()
        {
            MaxSdk.HideBanner(bannerAdUnitId);
        }

#endregion

        private int _ticksCount = 0;
        public void Tick()
        {
            if (!_startAdsShowed && _waitStartInterstitial)
            {
                if (_ticksCount < 25)
                {
                    _ticksCount++;
                }
                else
                {
                    _ticksCount = 0;
                    ShowStartInterstitial(true);
                }
            }
        }
    }
}