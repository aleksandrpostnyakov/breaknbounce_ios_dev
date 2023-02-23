using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Social;
using UnityEngine;

public class OkSDK :IDisposable
{
    private readonly WebGLProviderService _webGLProvider;
    private readonly SocialService _socialService;
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void OkShowPayment(string name, string description, string code, int price);
    [DllImport("__Internal")] private static extern void OkLoadRewardedAd();
    [DllImport("__Internal")] private static extern void OkShowRewardedAd();
    [DllImport("__Internal")] private static extern void OkShowInterstitialAd();
    [DllImport("__Internal")] private static extern void OkInit();
    [DllImport("__Internal")] private static extern void CopyClipboard(string text);
    #else
    private static void OkShowPayment(string name, string description, string code, int price){}

    private static void OkLoadRewardedAd(){}
private static void OkShowRewardedAd(){}
private static void OkShowInterstitialAd(){}
private static void OkInit(){}
private static void CopyClipboard(string text){}
#endif

    public event Action OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action OnLoadedAd;
    public event Action OnErrorLoadAd;
    public event Action OnShowAd;
    public event Action<string> OnErrorShowAd;

    private string _userId;
    
    public OkSDK(WebGLProviderService webGLProvider, SocialService socialService)
    {
        _webGLProvider = webGLProvider;
        _webGLProvider.OKOnUserIdResult += OnUserId;
        _webGLProvider.OKOnPaymentResult += OnPayment;
        _webGLProvider.OKOnShowLoadedAdResult += OnShowLoadedAd;
        _webGLProvider.OKOnLoadAdResult += OnLoadAd;
        _webGLProvider.OKOnShowInterstitialAdResult += OnShowInterstitialAd;
        
        _socialService = socialService;
        _socialService.OnNeedOKAuthenficated += Auth;
        _socialService.OnCopyToClipboard += SocialServiceOnOnCopyToClipboard;
        
        OkInit();
    }

    public event Action onClose;
    
    private void SocialServiceOnOnCopyToClipboard(string text)
    {
        CopyClipboard(text);
    }

    private async void Auth()
    {
        while (_userId == null)
        {
            Debug.Log("userId000 ");
            await new WaitForSeconds(.5f);
        }
        
        Debug.Log("userId4 " + _userId);
        _socialService.OKAuthenficated("ok-client-" +_userId);
    }
    
    public void Payment(string name, string description, string code, int price)
    {
        OkShowPayment(name, description, code, price);
    }
    
    public void LoadRewarded()
    {
        OkLoadRewardedAd();
    }
    
    public void ShowRewarded()
    {
        OkShowRewardedAd();
    }
    
    public void ShowInterstitial()
    {
        OkShowInterstitialAd();
    }

    #region +++ CALLBACKS +++

    
    private void OnUserId(string id)
    {
        _userId = id;
    }

    private void OnPayment(bool result, string data)
    {
        if (result)
        {
            OnPurchaseSuccess?.Invoke();
        }
        else
        {
            OnPurchaseFailed?.Invoke(data);
        }
    }

    private void OnLoadAd(bool result, string data)
    {
        if (result)
        {
            OnLoadedAd?.Invoke();
        }
        else
        {
            OnErrorLoadAd?.Invoke();
        }
    }

    private void OnShowLoadedAd(bool result, string data)
    {
        if (result)
        {
            OnShowAd?.Invoke();
        }
        else
        {
            OnErrorShowAd?.Invoke(data);
        }
    }

    private void OnShowInterstitialAd(bool result, string data)
    {
        
    }

    public void OnClose() {
        onClose?.Invoke();
    }
    
#endregion

public void Dispose()
{
    _webGLProvider.OKOnUserIdResult -= OnUserId;
    _webGLProvider.OKOnPaymentResult -= OnPayment;
    _webGLProvider.OKOnShowLoadedAdResult -= OnShowLoadedAd;
    _webGLProvider.OKOnLoadAdResult -= OnLoadAd;
    _webGLProvider.OKOnShowInterstitialAdResult -= OnShowInterstitialAd;
    _socialService.OnNeedVKAuthenficated -= Auth;
    _socialService.OnCopyToClipboard -= SocialServiceOnOnCopyToClipboard;
}
}

