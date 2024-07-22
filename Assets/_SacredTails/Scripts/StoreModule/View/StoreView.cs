using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Database;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Games.SacredTails.StoreModule
{
    public class StoreView : MonoBehaviour
    {
        #region ----Fields----
        [SerializeField] private GameObject vendorUI;
        #endregion ----Fields----

        #region ----Methods----
        #region <<<Popup handling>>>
        public void ShowPopupPurchaseCard(int itemId, uint itemPrice, Action<int, uint> onPurchaseCard)
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, null);
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => onPurchaseCard?.Invoke(itemId, itemPrice));

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Do you want to buy this card?", buttonsAction);
        }

        public void ShowPopupPurchaseResult(string message)
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => HidePanelAfterPurchase());

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup(message, buttonsAction);
        }

        public void HidePopup()
        {
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
        }
        #endregion <<<Popup handling>>>

        #region <<<Hide panel>>>
        public void HidePanelAfterPurchase()
        {
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            vendorUI.SetActive(false);
        }

        public void HideStore(List<StoreItem> storeItems)
        {
            for (int i = 0; i < storeItems.Count; i++)
                storeItems[i].gameObject.SetActive(false);
        }
        #endregion <<<Hide panel>>>
        #endregion ----Methods----
    }
}