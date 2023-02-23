using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Advertusement
{
    public class GameDistributionAdsSystem : BaseAdsSystem
    {
        private readonly GameDistributionSDK _gameDistributionSDK;
        
        protected override string GameId { get; }
        protected override string AdsName => "Rewarded_GD";
        private string _key => "b8c1e12f2bee40c0a22a3710f5e4e84e";
        private bool _loaded;
        private bool _needReward;
        
        public GameDistributionAdsSystem(GameDistributionSDK gameDistributionSDK)
        {
            _gameDistributionSDK = gameDistributionSDK;
            _gameDistributionSDK.OnLoadedAd += OkSDKOnOnLoadedAd;
            _gameDistributionSDK.OnErrorLoadAd += GDSDKOnOnErrorLoadAd;
            _gameDistributionSDK.OnShowAd += GDSDKOnOnShowAd;
            _gameDistributionSDK.OnErrorShowAd += GDSDKOnOnErrorShowAd;
            _gameDistributionSDK.OnRewardAd += GDSDKOnRewardAd;
            
            _gameDistributionSDK.Init(_key);
        }

        private void GDSDKOnRewardAd()
        {
            //_needReward = true;
        }

        private void GDSDKOnOnErrorShowAd(string error)
        {
            AdsViewed?.Invoke(false);
            OnChanged();
            _loaded = false;
            _gameDistributionSDK.PreloadAd();
        }

        private void GDSDKOnOnShowAd()
        {
            AdsViewed?.Invoke(true);
            OnChanged();
            _loaded = false;
            _gameDistributionSDK.PreloadAd();
        }

        private async void GDSDKOnOnErrorLoadAd()
        {
            _loaded = false;
            await new WaitForSeconds(1);
            _gameDistributionSDK.PreloadAd();
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
            
            _gameDistributionSDK.ShowAd("rewarded");
            OnShowAdsFader(true);
            _loaded = false;
            var result = await WaitForView();
            _isAdsShowed = false;
            return result;
        }

        public override bool ShowInterstitial()
        {
            _gameDistributionSDK.ShowAd("interstitial");
            ClearInterstitialTime();
            return true;
        }
        public override void LoadAds()
        {
            if (!_loaded)
            {
                _gameDistributionSDK.PreloadAd();
            }
        }
        

        private event Action<bool> AdsViewed;
        
        private async Task<bool> WaitForView()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void Action(bool complete)
            {
                tcs.SetResult(complete);
                _needReward = false;
                AdsViewed -= Action;
                OnShowAdsFader(false);
            }

            AdsViewed += Action;

            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}