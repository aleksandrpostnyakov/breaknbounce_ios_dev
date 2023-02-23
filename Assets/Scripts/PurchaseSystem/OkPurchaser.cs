using System;
using UnityEngine;

namespace Funcraft.Merge.PurchaseSystem
{
    public class OkPurchaser : IPurchaser
    {

        private readonly OkSDK _okSDK;
        private readonly IAdsSystem _adsSystem;

        private bool IsInitialized { get; set; }
        private string _currentPurchase;
        public event Action NeedUpdate;
        public PurchaserType GetPurchaserType => PurchaserType.OK;
        
        public OkPurchaser(OkSDK okSDK, IAdsSystem adsSystem)
        {
            _okSDK = okSDK;
            _adsSystem = adsSystem;
        }
        public void Initialize()
        {
            if (IsInitialized)
                return;
            
            _okSDK.OnPurchaseSuccess += OkSDKOnOnPurchaseSuccess;
            _okSDK.OnPurchaseFailed += OkSDKOnOnPurchaseFailed;
            
            IsInitialized = true;
        }

        private void OkSDKOnOnPurchaseFailed(string error)
        {
            _currentPurchase = string.Empty;
            Debug.Log($"BuyProductID FAIL. Error:{error}");
        }

        private void OkSDKOnOnPurchaseSuccess()
        {
            _currentPurchase = string.Empty;
        }

        public void BuyDisableAds()
        {
            //BuyProductID("adsOff");
        }
        
        // private async void BuyProductID(GemBankItem bankItem)
        // {
        //     if (IsInitialized)
        //     {
        //         _currentPurchase = bankItem.OkCode;
        //         _okSDK.Payment(bankItem.OkName, bankItem.OkDescription, bankItem.OkCode, bankItem.OkPrice);
        //     }
        //     else
        //     {
        //         Debug.Log("BuyProductID FAIL. Not initialized.");
        //     }
        // }


        public string GetLocalizedItemPrice(string itemId)
        {
            return $"ERROR";
        }
    }
}