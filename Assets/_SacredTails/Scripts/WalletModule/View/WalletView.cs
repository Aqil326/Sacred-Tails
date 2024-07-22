using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Timba.Games.SacredTails.WalletModule
{
    public class WalletView : MonoBehaviour
    {
        public TMP_Text currencyText;
        public GameObject walletPanel;


        public int currency;

       
        public void ChangeCurrency(int currentCurrency)
        {
            currencyText.text = currentCurrency.ToString("0000");
        }
       
        public void ShowUserWallet()
        {
            walletPanel.SetActive(true);
        }

        public void HideUserWallet()
        {
            walletPanel.SetActive(false);
        }
    }
}