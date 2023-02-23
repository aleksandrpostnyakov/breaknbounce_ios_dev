using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Zenject;

namespace WebGLProvider
{
    public class WebGLProvider : MonoBehaviour
    {
#if UNITY_WEBGL
    #if  !UNITY_EDITOR
             [DllImport("__Internal")] private static extern void registerVisibilityChangeEvent();
    #else
        private static void registerVisibilityChangeEvent(){}
    #endif
        
        private void Start() {
            registerVisibilityChangeEvent();
        }
        
        [Inject] private WebGLProviderService _service;

        public void ConsoleLog(string log)
        {
            Debug.Log("CONSOLE:" + log);
        }
        
        public void OnVisibilityChange(string visibilityState)
        {
            _service.OnVisibilityChange(visibilityState);
        }
        
        public void VkOnAuth(string data)
        {
            Debug.Log("PROVIDER ON AUTH");
            _service.VkOnAuth(data);
        }
        
        public void VkOnPayment(string result)
        {
            _service.VkOnPayment(result);
        }
        
        public void VkCheckAdsFailed(string result)
        {
            _service.VkCheckAdsFailed();
        }
        
        public void VkOnShowLoadedAd(string result)
        {
            _service.VkOnShowLoadedAd(result);
        }
        
        public void YaOnAuth(string data)
        {
            Debug.Log("PROVIDER ON AUTH");
            _service.YaOnAuth(data);
        }
        
        public void YaOnAuthError(string data)
        {
            Debug.Log("YANDEX ON AUTH ERROR");
            _service.YaOnAuthError(data);
        }
        
        public void YaOnRewarded(string result)
        {
            _service.YaOnRewarded(result);
        }

        public void YaOnRewardedError(string result)
        {
            _service.YaOnRewardedError(result);
        }
        
        public void YaOnRewardOpen(string result)
        {
            _service.YaOnRewardedOpen(result);
        }
        
        public void YaOnRewardClose(string result)
        {
            _service.YaOnRewardedClose(result);
        }
        
        public void YaOnGetProducts(string result)
        {
            _service.YaOnGetProducts(result);
        }
        
        public void YaOnPurchaseSuccess(string result)
        {
            _service.YaOnPurchaseSuccess(result);
        }

        public void YaOnPurchaseFailed(string result)
        {
            _service.YaOnPurchaseFailed(result);
        }

        public void YaOnGetLeaderboardScore(string result)
        {
            _service.YaOnGetLeaderboardScore(result);
        }
        
        public void OKOnUserId(string data)
        {
            _service.OKOnUserId(data);
        }
        
        public void OKOnPaymentOK(string data)
        {
            _service.OKOnPayment(true, data);
        }

        public void OKOnPaymentError(string data)
        {
            _service.OKOnPayment(false, data);
        }

        public void OKOnLoadAdOk(string data)
        {
            _service.OKOnLoadAd(true, data);
        }

        public void OKOnLoadAdError(string data)
        {
            _service.OKOnLoadAd(false, data);
        }

        public void OKOnShowLoadedAdOk(string data)
        {
            _service.OKOnShowLoadedAd(true, data);
        }

        public void OKOnShowLoadedAdError(string data)
        {
            _service.OKOnShowLoadedAd(false, data);
        }

        public void OKOnShowInterstitialAdOk(string data)
        {
            _service.OKOnShowInterstitialAd(true, data);
        }

        public void OKOnShowInterstitialAdError(string data)
        {
            _service.OKOnShowInterstitialAd(false, data);
        }
        
        public void GDResumeGame()
        {
            _service.GDResumeGame();
        }

        public void GDPauseGame()
        {
            _service.GDPauseGame();
        }
        public void GDRewardGame()
        {
            _service.GDRewardGame();
        }

        public void GDRewardedVideoSuccess()
        {
            _service.GDRewardedVideoSuccess();
        }

        public void GDRewardedVideoFailure()
        {
            _service.GDRewardedVideoFailure();
        }

        public void GDPreloadRewardedVideo(int loaded)
        {
            _service.GDPreloadRewardedVideo(loaded);
        }
#endif
    }
}