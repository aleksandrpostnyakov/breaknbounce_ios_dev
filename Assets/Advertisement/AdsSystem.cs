using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IAdsSystem
{
    Task<bool> ShowAdsForWinScreen();
    Task<bool> ShowAdsForUpgrade(int upgradeId);
    Task<bool> ShowAdsForRevive();
    bool ShowInterstitial();
    void ShowStartInterstitial(bool needShow);
    void ShowBanner();
    void HideBanner();
    AdsModel Model { get; }
    void SetModel(AdsModel newModel);
    void ClearInterstitialTime();
    void LoadAds();
    event Action Changed;
    event Action NoAvailableAds;
    event Action<bool> ShowAdsFader;
    event Action<BaseAdsSystem.AdsType, bool> OnShowRewardedAds;
}
