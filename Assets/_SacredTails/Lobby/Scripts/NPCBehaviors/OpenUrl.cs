using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

public class OpenUrl : MonoBehaviour
{
    public string url;
    public void OpenUrlMethod()
    {
        //Waiting for opponent to accept popup
        Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
        buttons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => { 
            Application.OpenURL(url);
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
        });
        buttons.Add(PopupManager.ButtonType.BACK_BUTTON, () => {  });
        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Do you like redirect to sacred tails page?", buttons);
        
    }

}
