using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.CardStoreModule;
using Timba.SacredTails.Database;
using UnityEngine;

namespace Timba.Games.SacredTails.StoreModule
{
    public class StoreController : MonoBehaviour
    {
        #region ----Fields----
        #region <<<References>>>
        [SerializeField] private StoreView storeView;
        [SerializeField] private CardManagementController cardManagementController;
        #endregion <<<References>>>

        #region <<<Prefabs and transforms>>>
        [SerializeField] private StoreItem storeItemPrefab;
        [SerializeField] private Transform storeItemsRowPrefab;
        [SerializeField] private Transform storeContainer;
        #endregion <<<Prefabs and transforms>>>

        #region <<<Class fields>>>
        private List<CatalogItem> storeItemsInfo = new List<CatalogItem>();
        private List<StoreItem> storeItems = new List<StoreItem>();
        private Transform currentRow;
        private int amountToPool = -1;
        #endregion <<<Class fields>>>
        #endregion ----Fields----

        #region ----Methods----
        #region <<<Init and disable>>>
        public void Start()
        {
            SubscribeToPurchaseEvent();
        }

        public void SubscribeToPurchaseEvent()
        {
            PlayfabManager.Singleton.OnPurchaseCardsSuccess.AddListener((result) =>
            {
                ServiceLocator.Instance.GetService<IWallet>().UpdateUserWallet();
                storeView.ShowPopupPurchaseResult("Card successfully buyed!");
            });

            PlayfabManager.Singleton.OnPurchaseCardsFailed.AddListener((error) =>
                storeView.ShowPopupPurchaseResult($"Didn't buy card: {error.ErrorMessage}"));

            PlayfabManager.Singleton.OnGetCardsStoreSuccess.AddListener(SetCardStore);
        }

        public void RequestCardsStore()
        {
            PlayfabManager.Singleton.GetStoreCards();
        }
        #endregion Init and disable

        #region <<<Show Hide Store >>>
        public void SetCardStore(GetCatalogItemsResult result)
        {
            storeItemsInfo = result.Catalog;
            FillPool(storeItemsInfo);
        }
        public void HideStore()
        {
            storeItemsInfo = new List<CatalogItem>();
            storeView.HideStore(storeItems);
        }
        #endregion <<<Show Hide Store >>>

        #region <<< StoreItems Pool >>>
        public void CreatePool()
        {
            for (int i = storeItems.Count; i < amountToPool; i++)
            {
                if (i % 3 == 0)
                    currentRow = Instantiate(storeItemsRowPrefab, storeContainer);

                StoreItem newItem = Instantiate(storeItemPrefab, currentRow);
                storeItems.Add(newItem);
                newItem.gameObject.SetActive(false);
            }
        }

        public void FillPool(List<CatalogItem> items)
        {
            if (items.Count > amountToPool)
            {
                amountToPool = items.Count;
                CreatePool();
                FillPool(items);
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                storeItems[i].data = new StoreItemData()
                {
                    itemId = Int32.Parse(items[i].ItemId),
                    itemName = items[i].DisplayName,
                    priceCurrency = "SC",
                    itemPrice = items[i].VirtualCurrencyPrices["SC"],
                    itemSprite = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(Int32.Parse(items[i].ItemId)).cardImage
                };
                storeItems[i].SetCardButtonAction(i, (index) =>
                 storeView.ShowPopupPurchaseCard(storeItems[index].data.itemId, storeItems[index].data.itemPrice, BuyItem));

                storeItems[i].Init();
                storeItems[i].gameObject.SetActive(true);
            }
        }
        #endregion <<< StoreItems Pool >>>

        #region <<<Buy Store Item>>>
        public void BuyItem(int itemId, uint itemPrice)
        {
            PlayfabManager.Singleton.PurchaseCard(itemId, itemPrice);
            cardManagementController.AddCardInDeck(itemId);
            PlayerDataManager.Singleton.UpdatePlayerData();

            storeView.HidePopup();
        }
        #endregion <<<Buy Store Item>>>
        #endregion ----Methods----
    }
}