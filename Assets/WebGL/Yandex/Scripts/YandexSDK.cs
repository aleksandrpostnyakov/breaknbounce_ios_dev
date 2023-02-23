using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Social;
using UnityEngine;

public class YandexSDK : IDisposable {
    private readonly WebGLProviderService _webGLProvider;
    private readonly SocialService _socialService;
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void GetUserData();
    [DllImport("__Internal")] private static extern void ShowFullscreenAd();
    [DllImport("__Internal")] private static extern int ShowRewardedAd(string placement);
    [DllImport("__Internal")] private static extern void AuthenticateUser(string direct);
    [DllImport("__Internal")] private static extern void InitPurchases();
    [DllImport("__Internal")] private static extern void Purchase(string id);
    [DllImport("__Internal")] private static extern void SetLeaderboard(string leaderboardId, int score);
    [DllImport("__Internal")] private static extern void GetLeaderboard(string leaderboardId);
    [DllImport("__Internal")] private static extern void CopyClipboard(string text);
    #else
    private static void GetUserData(){}

    private static void ShowFullscreenAd(){}

    private static int ShowRewardedAd(string placement)
    {
        return 0;}
    private static void AuthenticateUser(string direct){}
    private static void InitPurchases(){}
    private static void Purchase(string id){}
    private void SetLeaderboard(string leaderboardId, int score) {}
    private void GetLeaderboard(string leaderboardId){}
    private void CopyClipboard(string text){}
#endif

    public YandexSDK(WebGLProviderService webGLProvider, SocialService socialService)
    {
        _webGLProvider = webGLProvider;
        _webGLProvider.YaOnRewardedResult += OnRewarded;
        _webGLProvider.YaOnRewardedErrorResult += OnRewardedError;
        _webGLProvider.YaOnRewardedOpenResult += OnRewardedOpen;
        _webGLProvider.YaOnRewardedClosedResult += OnRewardedClose;
        _webGLProvider.YaOnGetProductsResult += OnGetProducts;
        _webGLProvider.YaOnPurchaseSuccessResult += OnPurchaseSuccess;
        _webGLProvider.YaOnPurchaseFailedResult += OnPurchaseFailed;
        _webGLProvider.YaOnGetLeaderboardScoreResult += OnGetLeaderboardScore;
        _webGLProvider.YaOnAuthResult += OnAuth;
        _webGLProvider.YaOnAuthErrorResult += OnAuthError;
        
        _socialService = socialService;
        _socialService.OnNeedYaAuthenficated += Authenticate;
        _socialService.OnCopyToClipboard += SocialServiceOnOnCopyToClipboard;
        
        _leaderboards = new Dictionary<string, int> {{"merge", -1}};

        foreach (var leaderboard in _leaderboards)
        {
            GetLeaderboardScore(leaderboard.Key);
        }
        
    }

    private void SocialServiceOnOnCopyToClipboard(string text)
    {
        CopyClipboard(text);
    }

    private UserData user;
    private readonly Dictionary<string, int> _leaderboards;
    
    public ProductsData[] Products { get; private set; }
    public event Action onUserDataReceived;

    public event Action onInterstitialShown;
    public event Action<string> onInterstitialFailed;
    public event Action<string> onRewardedAdOpened;
    public event Action<string> onRewardedAdReward;
    public event Action<string> onRewardedAdClosed;
    public event Action<string> onRewardedAdError;
    public event Action<string> onPurchaseSuccess;
    public event Action<string> onPurchaseFailed;

    public event Action onClose;

    public void Authenticate(bool direct) {
        AuthenticateUser(direct ? "direct" : "check");
    }

    public void ShowInterstitial() {
        ShowFullscreenAd();
    }

    public void ShowRewarded(string placement)
    {
        ShowRewardedAd(placement);
    }

    public void RequestUserData() {
        GetUserData();
    }
    
    public void InitializePurchases() {
        InitPurchases();
    }

    public void ProcessPurchase(string id) {
        Purchase(id);
    }
    
    public void StoreUserData(string data) {
        user = JsonUtility.FromJson<UserData>(data);
        onUserDataReceived?.Invoke();
    }

    public void IncreaseLeaderboardScore(string leaderboardId)
    {
        if (_leaderboards.ContainsKey(leaderboardId) && _leaderboards[leaderboardId] != -1)
        {
            _leaderboards[leaderboardId]++;
            SetLeaderboard(leaderboardId, _leaderboards[leaderboardId]);
        }
    }
    
    private void GetLeaderboardScore(string leaderboardId)
    {
        GetLeaderboard(leaderboardId);
    }
    
#region +++ CALLBACKS +++
    private void OnAuth(string data)
    {
        Debug.Log("SDK ON AUTH");
        user = new UserData() {id = data};
        _socialService.YaAuthenficated("ya-" + ReplaceWrongSymbols(user.id));
    }
    
    private void OnAuthError(string data)
    {
        Debug.Log("SDK ON AUTH ERROR");
        _socialService.YaAuthenficatedError();
    }

    private string ReplaceWrongSymbols(string str)
    {
        const int blockLength = 2;
        const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var userName = string.Empty;
        for (var i = 0; i < str.Length; i += blockLength)
        {
            var block = str.Length - i > blockLength ? str.Substring(i, blockLength) : str.Substring(i, str.Length - i);
            var sum = block.Aggregate(0, (current, t) => current + t);
            userName += symbols[sum % symbols.Length];
        }

        return userName;
    }
    
    
    public void OnInterstitialShown() {
        onInterstitialShown?.Invoke();
    }
    
    public void OnInterstitialError(string error) {
        onInterstitialFailed?.Invoke(error);
    }

    private void OnRewarded(string placement) {
        onRewardedAdReward?.Invoke(placement);
    }


    private void OnRewardedError(string placement) {
        onRewardedAdError?.Invoke(placement);
    }

    private void OnRewardedOpen(string placement) {
        onRewardedAdOpened?.Invoke(placement);
    }

    private void OnRewardedClose(string placement) {
        onRewardedAdClosed?.Invoke(placement);
    }
    
    public void OnPurchaseSuccess(string id) {
        onPurchaseSuccess?.Invoke(id);
    }
    
    public void OnPurchaseFailed(string error) {
        onPurchaseFailed?.Invoke(error);
    }

    public void OnGetProducts(string data)
    {
        Products = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductsData[]>(data);
    }

    public void OnGetLeaderboardScore(string data)
    {
        var datas = data.Split('$');
        if (datas.Length == 2 && _leaderboards.ContainsKey(datas[0]))
        {
            if(int.TryParse(datas[1], out var score))
            {
                _leaderboards[datas[0]] = score;
            }
        }
    }
    
    public void OnClose() {
        onClose?.Invoke();
    }
    
#endregion

    public void Dispose()
    {
        _webGLProvider.YaOnAuthResult += OnAuth;
        _webGLProvider.YaOnAuthErrorResult -= OnAuthError;
        _webGLProvider.YaOnRewardedResult -= OnRewarded;
        _webGLProvider.YaOnRewardedErrorResult -= OnRewardedError;
        _webGLProvider.YaOnGetProductsResult -= OnGetProducts;
        _webGLProvider.YaOnPurchaseSuccessResult -= onPurchaseSuccess;
        _webGLProvider.YaOnPurchaseFailedResult -= onPurchaseFailed;
        _webGLProvider.YaOnGetLeaderboardScoreResult -= OnGetLeaderboardScore;
        _socialService.OnNeedYaAuthenficated -= Authenticate;
        _socialService.OnCopyToClipboard -= SocialServiceOnOnCopyToClipboard;
    }
}
public struct ProductsData
{
    public string id;
    public string title;
    public string description;
    public string imageURI;
    public string price;
    public string priceValue;
    public string priceCurrencyCode;
    
}

public struct UserData {
    public string id;
    public string name;
    public string avatarUrlSmall;
    public string avatarUrlMedium;
    public string avatarUrlLarge;
}
