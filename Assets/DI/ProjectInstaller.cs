using Advertusement;
using Analytics;
using Core;
using Funcraft.Merge.PurchaseSystem;
using Level;
using Save;
using Server;
using Social;
using Sound;
using UI;
using UnityEngine;
using UpgradeState;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private GameObject _soundManagerPrefab;
    ///[SerializeField] private GameObject _notificationServicePrefab;
    [SerializeField] private PlatformType _platformType;
    
    public override void InstallBindings()
    {
        //Container.BindInterfacesTo<Funcraft.Merge.Analytics.Analytics>().AsSingle();
        Container.BindInterfacesTo<SoundManager>().FromComponentInNewPrefab(_soundManagerPrefab).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UiService>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameLoader.GameLoader>().AsSingle();
        Container.Bind<UserUpgrader>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerInfo.PlayerGameInfo>().AsSingle();
        Container.Bind<SocialService>().AsSingle();
        Container.Bind<StateMashine.StateMashine>().AsSingle();
        Container.Bind<WebGLProviderService>().AsSingle();
        Container.BindInterfacesTo<UnityServicer>().AsSingle().NonLazy();
        Container.Bind<Tutorial>().AsSingle();
        Container.Bind<AnalyticsService>().AsSingle().NonLazy();
        
#if !UNITY_WEBGL
    //Container.BindInterfacesTo<NotificationService>().FromComponentInNewPrefab(_notificationServicePrefab).AsSingle().NonLazy();
#else
        if (_platformType == PlatformType.Yandex)
        {
            Container.BindInterfacesAndSelfTo<YandexSDK>().AsSingle();
            Container.BindInterfacesTo<YandexAdsSystem>().AsSingle();
            Container.BindInterfacesTo<YandexPurchaser>().AsSingle();
            Container.BindInstance(PlatformType.Yandex).WithId("PlatformType");
        }
        else if (_platformType == PlatformType.OK)
        {
            Container.BindInterfacesAndSelfTo<OkSDK>().AsSingle();
            Container.BindInterfacesTo<OkAdsSystem>().AsSingle();
            Container.BindInterfacesTo<OkPurchaser>().AsSingle();
            Container.BindInstance(PlatformType.OK).WithId("PlatformType");
        }
        else if (_platformType == PlatformType.VK)
        {
            Container.BindInterfacesAndSelfTo<VkSDK>().AsSingle();
            Container.BindInterfacesTo<VkAdsSystem>().AsSingle();
            Container.BindInterfacesTo<VkPurchaser>().AsSingle();
            Container.BindInstance(PlatformType.VK).WithId("PlatformType");
        }
        else if (_platformType == PlatformType.GameDistribution)
        {
            Container.BindInterfacesAndSelfTo<GameDistributionSDK>().AsSingle();
            Container.BindInterfacesTo<GameDistributionAdsSystem>().AsSingle();
            Container.BindInterfacesTo<GameDistributionPurchaser>().AsSingle();
            Container.BindInstance(PlatformType.GameDistribution).WithId("PlatformType");
        }
#endif

#if UNITY_ANDROID
        if (_platformType == PlatformType.AndroidAds)
        {
            //Container.BindInterfacesTo<AndroidMostlyAdsSystem>().AsSingle();
            Container.BindInterfacesTo<AndroidAppLovinMediationSystem>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MostlyAdsPurchaser>().AsSingle();
            Container.BindInstance(PlatformType.AndroidAds).WithId("PlatformType");
        }
        else
        {
            //Container.BindInterfacesTo<AndroidAdsSystem>().AsSingle();
            //Container.BindInterfacesTo<Purchaser>().AsSingle();
            Container.BindInstance(PlatformType.Android).WithId("PlatformType");
        }
#elif UNITY_IOS
        Container.BindInterfacesTo<AndroidAppLovinMediationSystem>().AsSingle().NonLazy();
        Container.BindInterfacesTo<MostlyAdsPurchaser>().AsSingle();
        Container.BindInstance(PlatformType.IOS).WithId("PlatformType");
#endif
        //Container.BindInterfacesTo<CloudSave>().AsSingle().NonLazy();
        Container.BindInterfacesTo<PlayerPrefsSave>().AsSingle().NonLazy();
        Container.BindInterfacesTo<NakamaServer>().AsSingle();
    }
}

