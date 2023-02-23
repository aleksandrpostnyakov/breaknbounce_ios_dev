#if false
using System;
using System.Linq;
using System.Threading.Tasks;
using ByteBrewSDK;
using Funcraft.Merge.Config;
using Unity.Services.Core;
using Unity.Services.Mediation;
using UnityEngine;
using Zenject;

public class IOSMostlyAdsSystem : UnityMediationMostlyAdsSystem
{
    private const string _androidGameId = "4734716";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_iOS";
}

public class AndroidMostlyAdsSystem : UnityMediationMostlyAdsSystem
{
    private const string _androidGameId = "4734717";
    protected override string GameId => _androidGameId;
    protected override string AdsName => "Rewarded_Android";
}

public class UnityMediationMostlyAdsSystem :BaseAdsSystem
{
    [Inject] private IBankConfig _bankConfig;
    protected override string GameId => "";
    protected override string AdsName => "Rewarded_Mobile";
    private string interstitialAdUnitId = "Interstitial_Android";
    private IRewardedAd rewardedAd;
    private IInterstitialAd interstitialAd;
    
    protected UnityMediationMostlyAdsSystem()
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
        interstitialAd = MediationService.Instance.CreateInterstitialAd(interstitialAdUnitId);
        
            
        //rewardedAd.OnLoaded += AdLoaded;
        rewardedAd.OnFailedLoad += AdFailedToLoad;

        // Subscribe callback methods to show events:
        rewardedAd.OnShowed += AdShown;
        rewardedAd.OnFailedShow += AdFailedToShow;
        rewardedAd.OnUserRewarded += UserRewarded;
        //rewardedAd.OnClosed += AdClosed;
        
        try
        {
            await interstitialAd.LoadAsync();
        }
        catch(Exception e)
        {
            // Here our load failed.
        }
        
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

        _currentlyViewingAdType = AdsType.EnergyMarket;
        var result = await ShowAd();
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_energyshop");
        }
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

        _currentlyViewingAdType = AdsType.CoinsMarket;
        var result = await ShowAd();
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_coinshop");
        }
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
 
        _currentlyViewingAdType = AdsType.Boxes;
        var result = await ShowAd();
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_boxpanel");
        }
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

    private bool _isSlotsShowingNow;
    public override async Task<bool> ShowAdsForStorageSlots(int currentUnlockedCount)
    {
        if (_isSlotsShowingNow)
            return false;
     
        _isSlotsShowingNow = true;
        _currentlyViewingAdType = AdsType.StorageSlot;
        var result = await ShowAd();
            
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_storage");
            
            var count = ++_model.SlotsCounter;
            result = count >= (_bankConfig.StorageAdsCount + currentUnlockedCount);

            if (result)
            {
                _model.SlotsCounter = 0;
            }

            await new WaitForSeconds(.2f);
            OnChanged();
        }
        _isSlotsShowingNow = false;
        return result;
    }
    
    private bool _isDoubleRewardShowingNow;
    public override async Task<bool> ShowAdsForDoubleRewards()
    {
        if (_isDoubleRewardShowingNow)
            return false;
            
        _isDoubleRewardShowingNow = true;
        _currentlyViewingAdType = AdsType.DoubleReward;
        var result = await ShowAd();
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_double");
        }
        _isDoubleRewardShowingNow = false;
        return result;
    }
    
    private string _isPackShowingNow = string.Empty;
    public override async Task<bool> ShowAdsForPacks(string packId)
    {
        if (_isPackShowingNow != string.Empty)
            return false;

        _isPackShowingNow = packId;
        _currentlyViewingAdType = AdsType.Pack;
        var result = await ShowAd();
        if (result)
        {
            ByteBrew.TrackAdEvent(ByteBrewAdTypes.Reward, "adsview_bank", packId);
            
            var count = packId switch
            {
                "merge.adventure.gems_pack_6" => ++_model.Pack1Counter,
                "merge.adventure.gems_pack_5" => ++_model.Pack2Counter,
                "merge.adventure.gems_pack_4" => ++_model.Pack3Counter,
                "merge.adventure.gems_pack_3" => ++_model.Pack4Counter,
                "merge.adventure.gems_pack_2" => ++_model.Pack5Counter,
                "merge.adventure.gems_pack_1" => ++_model.Pack6Counter,
                "gems_specialoffer" => ++_model.SpecialOfferCounter,
                _ => 0
            };

            result = CheckPackAds(packId, count);

            if (result)
            {
                switch (packId)
                {
                    case "merge.adventure.gems_pack_6":
                        _model.Pack1Counter = 0;
                        break;
                    case "merge.adventure.gems_pack_5":
                        _model.Pack2Counter = 0;
                        break;
                    case "merge.adventure.gems_pack_4":
                        _model.Pack3Counter = 0;
                        break;
                    case "merge.adventure.gems_pack_3":
                        _model.Pack4Counter = 0;
                        break;
                    case "merge.adventure.gems_pack_2":
                        _model.Pack5Counter = 0;
                        break;
                    case "merge.adventure.gems_pack_1":
                        _model.Pack6Counter = 0;
                        break;
                    case "gems_specialoffer":
                        _model.SpecialOfferCounter = 0;
                        break;
                }
            }

            await new WaitForSeconds(.2f);
            OnChanged();
        }
        _isPackShowingNow = string.Empty;
        return result;
    }

    private bool CheckPackAds(string packId, int currentCount)
    {
        GemBankItem config;
        if (packId == "gems_specialoffer")
        {
            config = _bankConfig.SpecialOffeItem;
        }
        else
        {
            config = _bankConfig.BankItems.FirstOrDefault(bankItem =>
                string.Equals(packId, bankItem.GoogleStoreKey, StringComparison.Ordinal));
        }
        if (config == null)
        {
            return false;
        }

        return currentCount >= config.PackAdsCost;
    }

    public override async void ShowInterstitial()
    {
        if (interstitialAd.AdState == AdState.Loaded)
        {
            try
            {
                await interstitialAd.ShowAsync();
                ByteBrew.TrackAdEvent(ByteBrewAdTypes.Interstitial, "game");
            }
            catch(Exception e)
            {
                // Here show failed.
            }
        }
    }
}
#endif
