using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using UnityEngine;

namespace Timba.Games.SacredTails.BattleModule
{
    public class UIHelper : MonoBehaviour
    {
        public TurnsController turnsController;
        public BattleGameMode battleGameMode;
        public void OpenSkipTurnPopup()
        {
            bool hasEnergy = false;
            float auxEnergy = battleGameMode.playerInfo.energybars[battleGameMode.playerInfo.currentShinseiIndex].currentValue;
            foreach (CardUI aux in turnsController.uiCards)
            {
                if(auxEnergy >= aux.cardEnergy)
                {
                    hasEnergy = true;
                    break;
                }
            }

            if(hasEnergy)
            {
                Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
                buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, null);
                buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
                {
                    turnsController.SendMyTurn(7);
                    ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
                });

                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Do you want to skip turn?", buttonsAction);
            } else
            {
                turnsController.SendMyTurn(7);
            }

            //battleGameMode.playerInfo.energybars[battleGameMode.playerInfo.currentShinseiIndex].currentValue;
            //turnsController.uiCards[0].cardEnergy;
        }

        public void OpenSurrenderPopup()
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, null);
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
            {
                turnsController.SendMyTurn(8);
                ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            });

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Are you sure you want to surrender?", buttonsAction);
        }
    }
}