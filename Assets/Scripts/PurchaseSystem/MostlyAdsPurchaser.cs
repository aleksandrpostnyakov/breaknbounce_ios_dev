using UnityEngine;
using UnityEngine.Purchasing;
using System;
using PlayerInfo;

namespace Funcraft.Merge.PurchaseSystem
{
    public class MostlyAdsPurchaser : IStoreListener, IPurchaser
    {
        private IStoreController _storeController;
        private readonly PlayerGameInfo _playerGameInfo;
        private readonly IAdsSystem _adsSystem;

        private string _removeAdsKey = "breaknbounce.remove_ads";
        
        public event Action NeedUpdate;

        public PurchaserType GetPurchaserType => PurchaserType.Google;

        public string GetLocalizedItemPrice(string productId)
        {
            var product = _storeController.products.WithID(productId);
            return $"{product.metadata.localizedPrice:0.00} {product.metadata.isoCurrencyCode}";
        }

        public MostlyAdsPurchaser(PlayerGameInfo playerGameInfo, IAdsSystem adsSystem)
        {
            _playerGameInfo = playerGameInfo;
            _adsSystem = adsSystem;
        }
        
        public void BuyDisableAds()
        {
            BuyProductID(_removeAdsKey);
        }

        private void  BuyProductID(string productId)
        {
            if (IsInitialized)
            {
                    var product = _storeController.products.WithID(productId);

                    if (product != null && product.availableToPurchase)
                    {
                        Debug.Log($"Purchasing product asychronously: '{product.definition.id}'");
                        _storeController.InitiatePurchase(product);
                    }
                    else
                    {
                        Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public void Initialize()
        {
            if (IsInitialized)
                return; 
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(_removeAdsKey, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsInitialized=> _storeController != null;

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"Inapp initialization failed :{error}");
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            Debug.LogError($"Inapp purchase {i} failed :{p}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            _playerGameInfo.AdsDisable();
            _adsSystem.HideBanner();
            NeedUpdate?.Invoke();
            return PurchaseProcessingResult.Complete;
        }
    }
}