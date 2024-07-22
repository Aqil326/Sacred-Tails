using System;
using System.Collections.Generic;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using UnityEngine;
using static Timba.Games.SacredTails.PopupModule.PopupManager;

namespace Timba.Games.SacredTails
{
    public class DisconnectManager : MonoBehaviour
    {
        public Logout logout;
        bool hasLoggedIn = false;

        public void LogIn()
        {
            hasLoggedIn = true;
        }

        public void Update()
        {
            if (!hasLoggedIn)
                return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Dictionary<ButtonType, Action> buttons = new Dictionary<ButtonType, Action>();
                buttons.Add(ButtonType.CONFIRM_BUTTON, () =>
                {
                    logout.CallLogout();
                    hasLoggedIn = false;
                });
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You has disconnected. Going back to main menu",buttons);
            }
        }
    }
}