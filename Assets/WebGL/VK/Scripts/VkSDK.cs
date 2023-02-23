using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Social;
using UnityEngine;
using Zenject;

public class VkSDK : IDisposable, ITickable
{
    private readonly WebGLProviderService _webGLProvider;
    private readonly SocialService _socialService;
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void VkShowPayment(string itemId);
    [DllImport("__Internal")] private static extern void VkShowRewarderAd();
    [DllImport("__Internal")] private static extern void VkShowInterstitialAd();
    [DllImport("__Internal")] private static extern void VkAddToFavorites();
    [DllImport("__Internal")] private static extern void VkShowInviteBox();
    [DllImport("__Internal")] private static extern void VkShowWallPost(int id, int data);
    [DllImport("__Internal")] private static extern void VkAuth();
    [DllImport("__Internal")] private static extern void VkCopyClipboard(string text);
    #else
    private static void VkShowPayment(string itemId){}
    private static void VkShowRewarderAd(){}
    private static void VkShowInterstitialAd(){}
    private static void VkAddToFavorites(){}
    private static void VkShowInviteBox(){}
    private static void VkShowWallPost(int id, int data){}
    private static void VkAuth(){}
    private void VkCopyClipboard(string text){}
#endif

    public event Action OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action OnShowAd;
    public event Action OnErrorShowAd;
    public event Action OnErrorLoadAd;

    private string _userId = string.Empty;

    private DateTime _updateTime;
    
    public VkSDK(WebGLProviderService webGLProvider, SocialService socialService)
    {
        _webGLProvider = webGLProvider;
        _webGLProvider.VkOnPaymentResult += OnPayment;
        _webGLProvider.VkOnShowLoadedAdResult += OnShowLoadedAd;
        _webGLProvider.VkCheckAdsFailedResult += CheckAdsFailed;
        _webGLProvider.VkOnAuthResult += OnAuth;

        _socialService = socialService;
        _socialService.OnSocialAction += SocialServiceOnOnSocialAction;
        _socialService.OnInviteUsers += SocialServiceOnOnInviteUsers;
        _socialService.OnNeedVKAuthenficated += Auth;
        _socialService.OnCopyToClipboard += SocialServiceOnOnCopyToClipboard;
        
        _updateTime = DateTime.Now;
    }

    private void SocialServiceOnOnCopyToClipboard(string text)
    {
        VkCopyClipboard(text);
    }

    private void SocialServiceOnOnMerge()
    {
        //throw new NotImplementedException();
    }

    private void SocialServiceOnOnInviteUsers()
    {
        VkShowInviteBox();
    }

    private void SocialServiceOnOnSocialAction(int id)
    {
        if (id == 1)
        {
            VkAddToFavorites();
        }
    }

    public event Action onClose;
    
    private void Auth()
    {
        VkAuth();
    }
    
    public void Payment(string itemId)
    {
        VkShowPayment(itemId);
    }

    public void ShowRewarded()
    {
        VkShowRewarderAd();
    }
    
    public void ShowInterstitial()
    {
        VkShowInterstitialAd();
    }

    public void ShowWallPost(int id, int data)
    {
        VkShowWallPost(id, data);
    }
    
#region +++ CALLBACKS +++
    private void OnAuth(string data)
    {
        Debug.Log("SDK ON AUTH");
        _userId = data;
        _socialService.VkAuthenficated("vk-client-"+_userId);
    }

    private void OnPayment(string result)
    {
        if (result == "ok")
        {
            OnPurchaseSuccess?.Invoke();
        }
        else
        {
            OnPurchaseFailed?.Invoke("error");
        }
    }

    public void OnShowLoadedAd(string result)
    {
        if (result == "ok")
        {
            OnShowAd?.Invoke();
        }
        else
        {
            OnErrorShowAd?.Invoke();
        }
    }

    public void CheckAdsFailed()
    {
        OnErrorLoadAd?.Invoke();
    }

    public void OnClose() {
        onClose?.Invoke();
    }
    
#endregion

    public void Dispose()
    {
        _webGLProvider.VkOnPaymentResult -= OnPayment;
        _webGLProvider.VkOnShowLoadedAdResult -= OnShowLoadedAd;
        _webGLProvider.VkCheckAdsFailedResult -= CheckAdsFailed;
        
        _socialService.OnSocialAction -= SocialServiceOnOnSocialAction;
        _socialService.OnInviteUsers -= SocialServiceOnOnInviteUsers;
        _socialService.OnNeedVKAuthenficated -= Auth;
        _socialService.OnCopyToClipboard -= SocialServiceOnOnCopyToClipboard;
    }

    public void Tick()
    {
        if (_socialService.SocialWallpostData.Complete)
        {
            return;
        }

        var now = DateTime.Now;
        if ((now - _updateTime).TotalMinutes >= 5)
        {
            _updateTime = now;
            _socialService.SocialWallpostData.TimeCount++;
            
            if (_socialService.SocialWallpostData.TimeCount >= 6)
            {
                _socialService.SocialWallpostData.Complete = true;
                ShowWallPost(1, _socialService.SocialWallpostData.MergeCount);
            }

            _socialService.WallpostDataChanged();
        }
    }
}

