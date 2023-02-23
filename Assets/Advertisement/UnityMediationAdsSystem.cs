#if false
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;

public class IOSAdsSystem : UnityMediationAdsSystem
{
    private const string _androidGameId = "4734716";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_iOS";
}

public class AndroidAdsSystem : UnityMediationAdsSystem 
{
    private const string _androidGameId = "4734717";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_Android";
}

public class UnityMediationAdsSystem :BaseAdsSystem
{
    protected override string GameId => "";
    protected override string AdsName => "Rewarded_Mobile";
    private IRewardedAd rewardedAd;
    
    protected UnityMediationAdsSystem()
    {
        Init();
    }

    private async void Init()
    {
        while (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await new WaitForSeconds(1f);
        }
        
        rewardedAd = MediationService.Instance.CreateRewardedAd(AdsName);
        //rewardedAd.OnLoaded += AdLoaded;
        rewardedAd.OnFailedLoad += AdFailedToLoad;

        // Subscribe callback methods to show events:
        rewardedAd.OnShowed += AdShown;
        rewardedAd.OnFailedShow += AdFailedToShow;
        rewardedAd.OnUserRewarded += UserRewarded;
        //rewardedAd.OnClosed += AdClosed;
        
        try
        {
            await rewardedAd.LoadAsync();
        }
        catch(Exception e)
        {
            // Here our load failed.
        }
    }
    
    

    private void UserRewarded(object sender, RewardEventArgs e)
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
        OnChanged();
    }

    private async void AdShown(object sender, EventArgs e)
    {
        if(rewardedAd.AdState != AdState.Loading || rewardedAd.AdState != AdState.Loaded)
        {
            await rewardedAd.LoadAsync();
        }
    }

    private async void AdFailedToShow(object sender, ShowErrorEventArgs e)
    {
        Debug.Log($"Error showing Ad Unit {AdsName}: {e.Message}");
        
        OnNoAds();
        if(rewardedAd.AdState != AdState.Loading || rewardedAd.AdState != AdState.Loaded)
        {
            await rewardedAd.LoadAsync();
        }
    }

    private async void AdFailedToLoad(object sender, LoadErrorEventArgs e)
    {
        Debug.Log($"Error loading Ad Unit: {AdsName} - {e.Message}");

        await rewardedAd.LoadAsync();
    }


    private bool _isEnergyShowingNow;
    public override async Task<bool> ShowAdsForEnergyMarket()
    {
        if (rewardedAd == null || rewardedAd.AdState != AdState.Loaded)
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
        var result = await ShowAd();
        _isEnergyShowingNow = false;
        return result;
    }

    private bool _isCoinsShowingNow;
    public override async Task<bool> ShowAdsForCoinsMarket()
    {
        if (rewardedAd == null || rewardedAd.AdState != AdState.Loaded)
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
        var result = await ShowAd();
        _isCoinsShowingNow = false;
        return result;
    }
    
    private bool _isBoxesShowingNow;
    public override async Task<bool> ShowAdsForBoxes()
    {
        if (rewardedAd == null || rewardedAd.AdState != AdState.Loaded)
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
        var result = await ShowAd();
        _isBoxesShowingNow = false;
        return result;
    }
    
    private async Task<bool> ShowAd()
    {
        if (rewardedAd.AdState == AdState.Loaded)
        {
            try
            {
                await rewardedAd.ShowAsync();
            }
            catch(Exception e)
            {
                return false;
            }
        }
        else
        {
            OnNoAds();
            return false;
        }

        return true;
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
        //throw new System.NotImplementedException();
    }
}
#endif
