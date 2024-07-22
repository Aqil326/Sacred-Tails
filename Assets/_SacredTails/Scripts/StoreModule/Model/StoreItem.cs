using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Patterns.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Games.SacredTails.StoreModule
{
    public class StoreItem : MonoBehaviour
    {
        #region ----Fields----
        public StoreItemData data;

        [SerializeField] private TMP_Text itemName;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemValue;
        [SerializeField] private Image itemCurrency;
        [SerializeField] private List<Sprite> currencySpriteList;

        [SerializeField] private Image lockedImage;
        public Button cardButton;
        #endregion ----Fields----

        #region ----Methods----
        public void Init()
        {
            itemName.text = data.itemName;
            itemImage.sprite = data.itemSprite;
            itemValue.text = data.itemPrice.ToString();
            itemCurrency.sprite = currencySpriteList[0];

            int currentUserCoins = ServiceLocator.Instance.GetService<IWallet>().GetUserCurrentCoins();
            if (currentUserCoins < data.itemPrice)
                lockedImage.gameObject.SetActive(true);
        }

        public void SetCardButtonAction(int index,Action<int> callback)
        {
            cardButton.onClick.AddListener(() => callback?.Invoke(index)); 
        }
        #endregion ----Methods----
    }
}