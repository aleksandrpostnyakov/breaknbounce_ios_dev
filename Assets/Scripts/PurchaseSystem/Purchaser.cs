using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Purchasing;

public interface IPurchaser
    {
        PurchaserType GetPurchaserType { get; }
        void Initialize();
        event Action NeedUpdate;
        string GetLocalizedItemPrice(string itemId);
        void BuyDisableAds();
    }

    public enum PurchaserType
    {
        Google,
        Yandex,
        OK,
        VK,
        GameDistribution
    }

public class Purchaser : IStoreListener, IPurchaser
{
    private IStoreController _storeController;

    public event Action NeedUpdate;

    public PurchaserType GetPurchaserType => PurchaserType.Google;

    public string GetLocalizedItemPrice(string productId)
    {
        var product = _storeController.products.WithID(productId);
        return $"{product.metadata.localizedPrice:0.00} {product.metadata.isoCurrencyCode}";
    }

    public void BuyDisableAds()
    {
        BuyProductID("adsOff");
    }

    private void BuyProductID(string productId)
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
                Debug.Log(
                    "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
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

        builder.AddProduct("GoogleStoreKey", ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized => _storeController != null;

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
        return PurchaseProcessingResult.Complete;
    }
}