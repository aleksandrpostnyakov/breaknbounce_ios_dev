using System;
using System.Linq;
using UnityEngine;

namespace Funcraft.Merge.PurchaseSystem
{
    public class YandexPurchaser : IPurchaser
    {
        //private readonly YandexSDK _yandexSDK;
        private readonly IAdsSystem _adsSystem;

        public event Action NeedUpdate;
        public PurchaserType GetPurchaserType => PurchaserType.Yandex;
        
        public YandexPurchaser(/*YandexSDK yandexSDK,*/ IAdsSystem adsSystem)
        {
            //_yandexSDK = yandexSDK;
            _adsSystem = adsSystem;
        }

        private bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                return;
            //_yandexSDK.InitializePurchases();
            //_yandexSDK.onPurchaseSuccess += YandexSDKOnonPurchaseSuccess;
            //_yandexSDK.onPurchaseFailed += YandexSDKOnonPurchaseFailed;
            //_yandexSDK.onClose += YandexSDKOnonClose;
            IsInitialized = true;
        }
        
        public void BuyDisableAds()
        {
            //BuyProductID("adsOff");
        }
        
        // private async void  BuyProductID(string productId)
        // {
        //     if (IsInitialized)
        //     {
        //         if (productId == "Pack_1")
        //         {
        //             var result = await _adsSystem.ShowAdsForPacks(productId);
        //             if (result)
        //             {
        //                 var bankItem = _bankConfig.BankItems.First(tbankItem =>
        //                     string.Equals(productId, tbankItem.YandexKey, StringComparison.Ordinal));
        //                 Purchased?.Invoke(bankItem);
        //                 _playerInf.AddGems( bankItem.Value );
        //             }
        //             else
        //             {
        //                 NeedUpdate?.Invoke();
        //             }
        //         }
        //         else
        //         {
        //             _yandexSDK.ProcessPurchase(productId);
        //         }
        //     }
        //     else
        //     {
        //         Debug.Log("BuyProductID FAIL. Not initialized.");
        //     }
        // }

        private void YandexSDKOnonPurchaseFailed(string error)
        {
            Debug.Log($"BuyProductID FAIL. Error:{error}");
        }

        private void YandexSDKOnonPurchaseSuccess(string id)
        {
           
        }
        
        private void YandexSDKOnonClose()
        {
            //_yandexSDK.onPurchaseSuccess -= YandexSDKOnonPurchaseSuccess;
            //_yandexSDK.onPurchaseFailed -= YandexSDKOnonPurchaseFailed;
            //_yandexSDK.onClose -= YandexSDKOnonClose;
        }
        
        public string GetLocalizedItemPrice(string itemId)
        {
            return  "ERROR";
        }

    }
}