using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Funcraft.Merge.PurchaseSystem
{
    public class GameDistributionPurchaser : IPurchaser
    {
        private readonly IAdsSystem _adsSystem;
        public PurchaserType GetPurchaserType => PurchaserType.GameDistribution;
        private bool IsInitialized { get; set; }
        public event Action NeedUpdate;
        
        public GameDistributionPurchaser(IAdsSystem adsSystem)
        {
            _adsSystem = adsSystem;
        }
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void BuyDisableAds()
        {
            //BuyProductID("adsOff");
        }

        // private async void BuyProductID(GemBankItem bankItem)
        // {
        //     if (IsInitialized)
        //     {
        //         var result = await _adsSystem.ShowAdsForPacks(bankItem.GameDistributionKey);
        //         if (result)
        //         {
        //             Purchased?.Invoke(bankItem);
        //             _playerInf.AddGems( bankItem.Value );
        //         }
        //         else
        //         {
        //             NeedUpdate?.Invoke();
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