using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameDistributionSDK :IDisposable
{
    private readonly WebGLProviderService _webGLProvider;
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SDK_Init(string gameKey);
    [DllImport("__Internal")] private static extern void SDK_PreloadAd();
    [DllImport("__Internal")] private static extern void SDK_ShowAd(string type);
    #else
    private static void SDK_Init(string gameKey){}
    private static void SDK_PreloadAd(){}
    private static void SDK_ShowAd(string type){}
#endif

    public event Action OnLoadedAd;
    public event Action OnErrorLoadAd;
    public event Action OnRewardAd;
    public event Action OnShowAd;
    public event Action<string> OnErrorShowAd;
    
    public GameDistributionSDK(WebGLProviderService webGLProvider)
    {
        _webGLProvider = webGLProvider;
        _webGLProvider.GDOnRewardGameResult += OnRewardGame;
        _webGLProvider.GDOnSucessShowAdResult += OnRewardedVideoSucess;
        _webGLProvider.GDOnFailShowAdResult += OnRewardedVideoFailure;
        _webGLProvider.GDOnLoadAdResult += OnLoadAd;
    }
 
    public void Init(string key)
    {
        SDK_Init(key);
    }
    
    public void PreloadAd()
    {
        SDK_PreloadAd();
    }
    
    public void ShowAd(string type)
    {
        SDK_ShowAd(type);
    }

    #region +++ CALLBACKS +++

    private void OnRewardGame()
    {
        OnRewardAd?.Invoke();
    }

    private void OnLoadAd(int loaded)
    {
        if (loaded == 1)
        {
            OnLoadedAd?.Invoke();
        }
        else
        {
            OnErrorLoadAd?.Invoke();
        }
    }
    
    private void OnRewardedVideoSucess()
    {
        OnShowAd?.Invoke();
    }

    private void OnRewardedVideoFailure()
    {
        OnErrorShowAd?.Invoke("error");
    }
    
#endregion

public void Dispose()
{
    _webGLProvider.GDOnRewardGameResult -= OnRewardGame;
    _webGLProvider.GDOnFailShowAdResult -= OnRewardedVideoFailure;
    _webGLProvider.GDOnLoadAdResult -= OnLoadAd;
}
}

