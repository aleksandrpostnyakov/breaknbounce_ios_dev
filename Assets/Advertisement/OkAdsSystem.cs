using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Advertusement
{
    public class OkAdsSystem : BaseAdsSystem
    {
        private readonly OkSDK _okSDK;
        
        protected override string GameId { get; }
        protected override string AdsName => "Rewarded_OK";
        private bool _loaded;
        
        public OkAdsSystem(OkSDK okSDK)
        {
            _okSDK = okSDK;
            _okSDK.OnLoadedAd += OkSDKOnOnLoadedAd;
            _okSDK.OnErrorLoadAd += OkSDKOnOnErrorLoadAd;
            _okSDK.OnShowAd += OkSDKOnOnShowAd;
            _okSDK.OnErrorShowAd += OkSDKOnOnErrorShowAd;
        }

        private async void OkSDKOnOnErrorShowAd(string error)
        {
            AdsViewed?.Invoke(false);
            OnChanged();
            _loaded = false;

            await new WaitForSeconds(1);

            LoadAds();
        }

        private void OkSDKOnOnShowAd()
        {
            AdsViewed?.Invoke(true);
            OnChanged();
            _loaded = false;
            _okSDK.LoadRewarded();
        }

        private async void OkSDKOnOnErrorLoadAd()
        {
            _loaded = false;
            await new WaitForSeconds(3);
            if (!_loaded)
            {
                _okSDK.LoadRewarded();
            }
        }

        private void OkSDKOnOnLoadedAd()
        {
            _loaded = true;
        }

        public override void SetModel(AdsModel newModel)
        {
            _model = newModel;
        }
        private bool _isAdsShowed;
        public override async Task<bool> ShowAdsForWinScreen()
        {
            if (_isAdsShowed)
                return false;
             
            if (!_loaded)
            {
                OnNoAds();
                return false;
            }

            _isAdsShowed = true;
            _currentlyViewingAdType = AdsType.WinLevel;
            
            _okSDK.ShowRewarded();
            OnShowAdsFader(true);
            _loaded = false;
            var result = await WaitForView();
            _isAdsShowed = false;
            return result;
        }
        
        public override bool ShowInterstitial()
        {
            _okSDK.ShowInterstitial();
            ClearInterstitialTime();
            return true;
        }

        public override void LoadAds()
        {
            if (!_loaded)
            {
                _okSDK.LoadRewarded();
            }
        }

        private event Action<bool> AdsViewed;
        
        private async Task<bool> WaitForView()
        {
            var tcs = new TaskCompletionSource<bool>();

            void Action(bool complete)
            {
                tcs.SetResult(complete);
                AdsViewed -= Action;
                OnShowAdsFader(false);
            }

            AdsViewed += Action;

            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}