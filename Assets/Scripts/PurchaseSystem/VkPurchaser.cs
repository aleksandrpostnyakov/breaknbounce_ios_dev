using System;
using System.Linq;
using UnityEngine;

namespace Funcraft.Merge.PurchaseSystem
{
    public class VkPurchaser : IPurchaser
    {
        private readonly VkSDK _vkSDK;
        private readonly IAdsSystem _adsSystem;
        public PurchaserType GetPurchaserType => PurchaserType.VK;
        private bool IsInitialized { get; set; }
        private string _currentPurchase;
        public event Action NeedUpdate;
        
        public VkPurchaser(VkSDK vkSDK, IAdsSystem adsSystem)
        {
            _vkSDK = vkSDK;
            _adsSystem = adsSystem;
        }
        public void Initialize()
        {
            if (IsInitialized)
                return;
            
            _vkSDK.OnPurchaseSuccess += VkSDKOnOnPurchaseSuccess;
            _vkSDK.OnPurchaseFailed += VkSDKOnOnPurchaseFailed;
            
            IsInitialized = true;
        }

        private void VkSDKOnOnPurchaseFailed(string error)
        {
            _currentPurchase = string.Empty;
            Debug.Log($"BuyProductID FAIL. Error:{error}");
        }

        private void VkSDKOnOnPurchaseSuccess()
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
        //         if (bankItem.VkKey == "Pack_1")
        //         {
        //             var result = await _adsSystem.ShowAdsForPacks(bankItem.VkKey);
        //             if (result)
        //             {
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
        //             _currentPurchase = bankItem.VkKey;
        //             _vkSDK.Payment(bankItem.VkKey);
        //         }
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