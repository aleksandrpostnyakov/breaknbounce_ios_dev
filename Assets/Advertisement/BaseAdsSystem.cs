using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsModel
{
    public int AdsCounter;
    public DateTime InterstitialTime;
    public int UpgradesLastId;
    public DateTime UpgradesTime;
    public bool UpgradesActive;
}

public abstract class BaseAdsSystem: IAdsSystem, IDisposable
{
    public enum AdsType
    {
        WinLevel,
        UpgradeCard,
        Revive
    }

    public AdsModel Model => _model;
    protected AdsModel _model = new();

    public event Action Changed;
    public event Action NoAvailableAds;
    public event Action<bool> ShowAdsFader;
    public event Action<AdsType, bool> OnShowRewardedAds;

    protected abstract string GameId { get; }
    protected abstract string AdsName { get; }

    protected AdsType _currentlyViewingAdType;
    protected bool _startAdsShowed;

    public virtual async Task<bool> ShowAdsForWinScreen()
    {
        return false;
    }
    
    public virtual async Task<bool> ShowAdsForUpgrade(int upgradeId)
    {
        return false;
    }
    
    public virtual async Task<bool> ShowAdsForRevive()
    {
        return false;
    }

    public abstract bool ShowInterstitial();

    public virtual void ShowStartInterstitial(bool needShow)
    {
        if (needShow && !_startAdsShowed)
        {
            ShowInterstitial();
        }
        _startAdsShowed = true;
    }
    
    public virtual void ShowBanner()
    {
    }
    
    public virtual void HideBanner()
    {
    }

    protected void OnChanged()
    {
        Changed?.Invoke();
    }
    
    protected void OnShowRewardedAdsResult(AdsType adsType, bool result)
    {
        OnShowRewardedAds?.Invoke(adsType, result);
    }
    
    protected void OnShowAdsFader(bool show)
    {
        ShowAdsFader?.Invoke(show);
    }

    protected void OnNoAds()
    {
        NoAvailableAds?.Invoke();
    }

    public virtual void SetModel(AdsModel newModel)
    {
        _model = newModel;
    }
    
    public void ClearInterstitialTime()
    {
        _model.InterstitialTime = DateTime.Now;
        OnChanged();
    }

    public virtual void LoadAds()
    {
    }

    public void Dispose()
    {
        
    }
}
