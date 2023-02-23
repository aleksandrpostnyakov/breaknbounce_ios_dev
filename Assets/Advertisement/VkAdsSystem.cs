using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Advertusement
{
    public class VkAdsSystem : BaseAdsSystem
    {
        private readonly VkSDK _vkSDK;
        protected override string GameId => "vk";
        protected override string AdsName => "Rewarded_VK";
        
        public VkAdsSystem(VkSDK vkSDK)
        {
            _vkSDK = vkSDK;
            _vkSDK.OnErrorLoadAd += VkSDKOnOnErrorLoadAd;
            _vkSDK.OnShowAd += VkSDKOnOnShowAd;
            _vkSDK.OnErrorShowAd += VkSDKOnOnErrorShowAd;
        }

        private void VkSDKOnOnErrorShowAd()
        {
            AdsViewed?.Invoke(false);
            OnChanged();
        }

        private void VkSDKOnOnShowAd()
        {
            AdsViewed?.Invoke(true);
            OnChanged();
        }

        private void VkSDKOnOnErrorLoadAd()
        {
            AdsViewed?.Invoke(false);
            OnChanged();
            OnNoAds();
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

            _isAdsShowed = true;
            _currentlyViewingAdType = AdsType.WinLevel;
            
            _vkSDK.ShowRewarded();
            OnShowAdsFader(true);
            var result = await WaitForView();
            _isAdsShowed = false;
            return result;
        }

        public override bool ShowInterstitial()
        {
            _vkSDK.ShowInterstitial();
            ClearInterstitialTime();
            return true;
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