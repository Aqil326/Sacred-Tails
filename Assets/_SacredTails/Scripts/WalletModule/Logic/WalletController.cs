using PlayFab;
using UnityEngine;
using PlayFab.ClientModels;
using System;
using Timba.Games.SacredTails.WalletModule;
using Timba.SacredTails.UiHelpers;

namespace Timba.Games.SacredTails
{
    public class WalletController : MonoBehaviour, IWallet
    {
        #region ----Fields----

        [SerializeField] private WalletView walletView;
        ///*private */ public int currentCurrency /*= -1*/;
        public int currentCurrency = 100;

        #endregion ----Fields----

        #region ----Methods----
        public void Start()
        {
          
            PlayfabManager.Singleton.OnUserCurrencyGetSuccess.AddListener(GetPlayerCurrency);
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            
            UIGroups.instance.NotifyDynamicPanel(canvasGroup,"planner");
            if (UIGroups.instance.lastActivate == "planner")
                canvasGroup.alpha = 1;
            else
                canvasGroup.alpha = 0;
               // AddCurrency(900);
        }

        #region Add/Sub New Currency
        public void AddCurrency(int amount)
        {
            
            var request = new AddUserVirtualCurrencyRequest
            {
                VirtualCurrency = "SC",
                Amount = currentCurrency + amount
            };
           
            PlayFabClientAPI.AddUserVirtualCurrency(request, OnAddCurrencySuccess, OnAddCurrencyFailure);
        }

        private void OnAddCurrencySuccess(ModifyUserVirtualCurrencyResult result)
        {
            currentCurrency = result.Balance;
            walletView.ChangeCurrency(currentCurrency);
            Debug.LogError("Currency added successfully. New Balance: " + result.Balance);
        }

        private void OnAddCurrencyFailure(PlayFabError error)
        {
            Debug.LogError("Error adding currency: " + error.GenerateErrorReport());
        }

        public void SubtractCurrency(int amount)
        {
            if (currentCurrency >= amount)
            {
                var request = new SubtractUserVirtualCurrencyRequest
                {
                    VirtualCurrency = "SC",
                    Amount = currentCurrency - amount
                };

                PlayFabClientAPI.SubtractUserVirtualCurrency(request, OnSubtractCurrencySuccess, OnSubtractCurrencyFailure);
            }
            else
            {
                Debug.LogError("Not enough currency to subtract.");
            }
        }

        private void OnSubtractCurrencySuccess(ModifyUserVirtualCurrencyResult result)
        {
            currentCurrency = result.Balance;
            walletView.ChangeCurrency(currentCurrency);
            Debug.Log("Currency subtracted successfully. New Balance: " + result.Balance);
        }

        private void OnSubtractCurrencyFailure(PlayFabError error)
        {
            Debug.LogError("Error subtracting currency: " + error.GenerateErrorReport());
        }

        #endregion

        public void GetPlayerCurrency(GetUserInventoryResult result)
        {
            currentCurrency = result.VirtualCurrency["SC"];
            walletView.ChangeCurrency(currentCurrency);

        }

        public int GetUserCurrentCoins()
        {
            return currentCurrency;
        }

        public void UpdateUserWallet()
        {
            PlayfabManager.Singleton.GetPlayerCurrency();
        }

        public void ShowUserWallet()
        {
            walletView.ShowUserWallet();
        }

        public void HideUserWallet()
        {
            walletView.HideUserWallet();
        }

        public bool IsReady()
        {
            if (currentCurrency == -1)
                return false;

            return true;
        }
        #endregion ----Methods----
    }
}