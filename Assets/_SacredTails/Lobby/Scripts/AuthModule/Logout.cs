using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using PlayFab;
using UnityEngine.SceneManagement;
using Timba.Games.SacredTails;
using Timba.Games.SacredTails.PopupModule;
using System;
using Timba.Patterns.ServiceLocator;

public class Logout : NetworkBehaviour
{
    public void CallLogout()
    {
        Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
        buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
        {
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
        });
        buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
        {
            Application.Quit();
        });

        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"Are you sure you want to quit the game?", buttonsAction);
    }
}
