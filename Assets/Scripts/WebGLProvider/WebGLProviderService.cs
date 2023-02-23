using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WebGLProviderService{
    public event Action<bool> FrameOnVisibilityChange;
    
    
    public event Action<string> VkOnAuthResult;
    public event Action<string> VkOnPaymentResult;
    public event Action<string> VkOnShowLoadedAdResult;
    public event Action VkCheckAdsFailedResult;
    
    public event Action<string> YaOnAuthResult;
    public event Action<string> YaOnAuthErrorResult;
    public event Action<string> YaOnRewardedResult;
    public event Action<string> YaOnRewardedErrorResult;
    public event Action<string> YaOnRewardedOpenResult;
    public event Action<string> YaOnRewardedClosedResult;
    public event Action<string> YaOnGetProductsResult;
    public event Action<string> YaOnPurchaseSuccessResult;
    public event Action<string> YaOnPurchaseFailedResult;
    public event Action<string> YaOnGetLeaderboardScoreResult;
    
    public event Action<string> OKOnUserIdResult;
    public event Action<bool, string> OKOnPaymentResult;
    public event Action<bool, string> OKOnLoadAdResult;
    public event Action<bool, string> OKOnShowLoadedAdResult;
    public event Action<bool, string> OKOnShowInterstitialAdResult;
    
    public event Action GDOnRewardGameResult;
    public event Action GDOnFailShowAdResult;
    public event Action GDOnSucessShowAdResult;
    public event Action<int> GDOnLoadAdResult;
    
    public void OnVisibilityChange(string visibilityState) {
        //System.Console.WriteLine("[" + System.DateTime.Now + "] the game switched to " + (visibilityState == "visible" ? "foreground" : "background"));
        FrameOnVisibilityChange?.Invoke(visibilityState == "visible");
    }

#region +++ VK +++
    public void VkOnAuth(string data)
    {
        Debug.Log("PROVIDER SERVICE ON AUTH");
        VkOnAuthResult?.Invoke(data);
    }
    public void VkOnPayment(string result)
    {
        VkOnPaymentResult?.Invoke(result);
    }
    
    public void VkOnShowLoadedAd(string result)
    {
        VkOnShowLoadedAdResult?.Invoke(result);
    }
    
    public void VkCheckAdsFailed()
    {
        VkCheckAdsFailedResult?.Invoke();
    }
#endregion   --- VK ---

#region +++ YANDEX +++
    public void YaOnAuth(string data)
    {
        Debug.Log("PROVIDER SERVICE ON AUTH");
        YaOnAuthResult?.Invoke(data);
    }
    
    public void YaOnAuthError(string data)
    {
        Debug.Log("PROVIDER SERVICE ON AUTH ERROR");
        YaOnAuthErrorResult?.Invoke(data);
    }

    public void YaOnRewarded(string result)
    {
        YaOnRewardedResult?.Invoke(result);
    }

    public void YaOnRewardedError(string result)
    {
        YaOnRewardedErrorResult?.Invoke(result);
    }
    
    public void YaOnRewardedOpen(string result)
    {
        YaOnRewardedOpenResult?.Invoke(result);
    }
    
    public void YaOnRewardedClose(string result)
    {
        YaOnRewardedClosedResult?.Invoke(result);
    }

    public void YaOnGetProducts(string result)
    {
        YaOnGetProductsResult?.Invoke(result);
    }

    public void YaOnPurchaseSuccess(string result)
    {
        YaOnPurchaseSuccessResult?.Invoke(result);
    }

    public void YaOnPurchaseFailed(string result)
    {
        YaOnPurchaseFailedResult?.Invoke(result);
    }

    public void YaOnGetLeaderboardScore(string result)
    {
        YaOnGetLeaderboardScoreResult?.Invoke(result);
    }
#endregion   --- YANDEX ---

#region +++ OK +++
    public void OKOnUserId(string data)
    {
        OKOnUserIdResult?.Invoke(data);
    }

    public void OKOnPayment(bool result, string data)
    {
        OKOnPaymentResult?.Invoke(result, data);
    }

    public void OKOnLoadAd(bool result, string data)
    {
        OKOnLoadAdResult?.Invoke(result, data);
    }

    public void OKOnShowLoadedAd(bool result, string data)
    {
        OKOnShowLoadedAdResult?.Invoke(result, data);
    }

    public void OKOnShowInterstitialAd(bool result, string data)
    {
        OKOnShowInterstitialAdResult?.Invoke(result, data);
    }

#endregion   --- OK ---

#region +++ GameDistribution +++

    public void GDResumeGame()
    {
    }

    public void GDPauseGame()
    {
    }
    public void GDRewardGame()
    {
        GDOnRewardGameResult?.Invoke();
    }
    
    public void GDRewardedVideoSuccess()
    {
        GDOnSucessShowAdResult?.Invoke();
    }

    public void GDRewardedVideoFailure()
    {
        GDOnFailShowAdResult?.Invoke();
    }

    public void GDPreloadRewardedVideo(int loaded)
    {
        GDOnLoadAdResult?.Invoke(loaded);
    }
#endregion   --- GameDistribution ---
}
