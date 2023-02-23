#if false
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Advertisements;

public class IOSAdsSystem : UnityAdsSystem
{
    private const string _androidGameId = "4734716";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_Android";
}

public class AndroidAdsSystem : UnityAdsSystem 
{
    private const string _androidGameId = "4734717";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_Android";
}

public class UnityAdsSystem : BaseAdsSystem, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
    protected override string GameId => "";
    protected override string AdsName => "Rewarded_Mobile";

    private bool _adLoaded;
    
    protected UnityAdsSystem()
    {
        Init();
    }

    private void Init()
    {
        Advertisement.Initialize(GameId, false,this);
    }
    
    public void OnInitializationComplete()
    {
        Debug.Log("UnityAds Initialization Complete");
        Advertisement.Load(AdsName, this);
    }
    
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
    
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
 
        if (adUnitId.Equals(AdsName))
        {
            _adLoaded = true;
        }
    }
 
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
        AdsViewed?.Invoke(false);
        _adLoaded = false;
        Advertisement.Load(AdsName, this);
    }
 
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
         AdsViewed?.Invoke(false);
         OnNoAds();
         if(_adLoaded)
         {
             _adLoaded = false;
             Advertisement.Load(AdsName, this);Advertisement.Load(AdsName, this);
         }
    }
    
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"ADS Show complete {showCompletionState}");
        
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            if (_currentlyViewingAdType == AdsType.EnergyMarket)
            {
                if(_model.EnergyAdViewsCounter == 0)
                {
                    _model.FirstEnergyViewTime = DateTime.Now;
                    RemainTimeForEnergy = _timerResetPeriod;

                    StartEnergyCountDownTimer();
                }

                ++_model.EnergyAdViewsCounter;
            }
            else if (_currentlyViewingAdType == AdsType.CoinsMarket)
            {
                if (_model.CoinsAdViewsCounter == 0)
                {
                    _model.FirstCoinViewTime = DateTime.Now;
                    RemainTimeForCoins = _timerResetPeriod;

                    StartCoinsCountDownTimer();
                }

                ++_model.CoinsAdViewsCounter;
            }
            else if (_currentlyViewingAdType == AdsType.Boxes)
            {
                if (_model.BoxesAdViewsCounter == 0)
                {
                    _model.FirstBoxViewTime = DateTime.Now;
                    RemainTimeForBoxes = _timerResetPeriod;

                    StartBoxesCountDownTimer();
                }

                ++_model.BoxesAdViewsCounter;
            }
            Advertisement.Load(AdsName, this);
            AdsViewed?.Invoke(true);
            OnChanged();
        }
    }
    
    private bool _isEnergyShowingNow;
    public override async Task<bool> ShowAdsForEnergyMarket()
    {
        Debug.Log("SHOW ENERGY ADS");
        if (!_adLoaded)
        {
            OnNoAds();
        }
        
        if (_isEnergyShowingNow)
            return false;
        _isEnergyShowingNow = true;
        if (_model.EnergyAdViewsCounter >= _maxAdViews)
        {
            _isEnergyShowingNow = false;
            return false;
        }
        _currentlyViewingAdType = AdsType.EnergyMarket;
        _adLoaded = false;
        Advertisement.Show(AdsName, this);
        var result =await WaitForView();
        _isEnergyShowingNow = false;
        return result;
    }

    private bool _isCoinsShowingNow;
    public override async Task<bool> ShowAdsForCoinsMarket()
    {
        if (!_adLoaded)
        {
            OnNoAds();
        }
        
        if (_isCoinsShowingNow)
            return false;
        _isCoinsShowingNow = true;
        if (_model.CoinsAdViewsCounter >= _maxAdViews)
        {
            _isCoinsShowingNow = false;
            return false;
        }
        _currentlyViewingAdType = AdsType.CoinsMarket;
        _adLoaded = false;
        Advertisement.Show(AdsName, this);
        var result =await WaitForView();
        _isCoinsShowingNow = false;
        return result;
    }
    
    private bool _isBoxesShowingNow;
    public override async Task<bool> ShowAdsForBoxes()
    {
        if (!_adLoaded)
        {
            OnNoAds();
        }
        
        if (_isBoxesShowingNow)
            return false;
        _isBoxesShowingNow = true;
        if (_model.BoxesAdViewsCounter >= _maxAdViews)
        {
            _isBoxesShowingNow = false;
            return false;
        }
        _currentlyViewingAdType = AdsType.Boxes;
        _adLoaded = false;
        Advertisement.Show(AdsName, this);
        var result = await WaitForView();
        _isBoxesShowingNow = false;
        return result;
    }

    public override async Task<bool> ShowAdsForStorageSlots(int currentUnlockedCount)
    {
        return false;
    }

    public override async Task<bool> ShowAdsForDoubleRewards()
    {
        return false;
    }
    
    public override void ShowInterstitial()
    {
        ClearInterstitialTime();
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

        AdsViewed -= Action;
        AdsViewed += Action;

        await tcs.Task;
        return tcs.Task.Result;
    }
}
#endif
