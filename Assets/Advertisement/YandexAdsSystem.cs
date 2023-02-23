using System;
using System.Threading.Tasks;
using Sound;
using UnityEngine;


namespace Advertusement
{
    public class YandexAdsSystem: BaseAdsSystem
    {
        //private readonly YandexSDK _yandexSDK;
        //private readonly ISoundManager _soundManager;
        
        protected override string GameId { get; }
        protected override string AdsName => "Rewarded_Yandex";

        public YandexAdsSystem(/*YandexSDK yandexSDK,*/ ISoundManager soundManager)
        {
            //_yandexSDK = yandexSDK;
            //_yandexSDK.onRewardedAdReward += YandexSDKOnonRewardedAdReward;
            //_yandexSDK.onRewardedAdError += YandexSDKOnonRewardedAdError;
            //_yandexSDK.onRewardedAdOpened += YandexSDKOnonRewardedAdOpen;
            //_yandexSDK.onRewardedAdClosed += YandexSDKOnonRewardedAdClose;
            //_soundManager = soundManager;
        }

        public override void SetModel(AdsModel newModel)
        {
            _model = newModel;
        }

        private void YandexSDKOnonRewardedAdError(string error)
        {
            AdsViewed?.Invoke(false);
            OnChanged();
        }
        
        private void YandexSDKOnonRewardedAdOpen(string error)
        {
            //_soundManager.SetSilent(true, true, "ya");
        }
        
        private void YandexSDKOnonRewardedAdClose(string error)
        {
            //_soundManager.SetSilent(false, true, "ya");
        }

        private void YandexSDKOnonRewardedAdReward(string placement)
        {
            AdsViewed?.Invoke(true);
            OnChanged();
        }
        
        private bool _isAdsShowed;
        public override async Task<bool> ShowAdsForWinScreen()
        {
            if (_isAdsShowed)
                return false;
            _isAdsShowed = true;
            _currentlyViewingAdType = AdsType.WinLevel;
            
            //_yandexSDK.ShowRewarded(AdsName);
            
            var result = await WaitForView();
            _isAdsShowed = false;
            return result;
        }

        public override bool ShowInterstitial()
        {
            //_yandexSDK.ShowInterstitial();
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
            }

            AdsViewed += Action;

            await tcs.Task;
            return tcs.Task.Result;
        }
    }
}