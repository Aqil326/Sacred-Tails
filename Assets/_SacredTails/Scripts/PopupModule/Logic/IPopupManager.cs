using System;
using System.Collections.Generic;
using Timba.Patterns.ServiceLocator;

namespace Timba.Games.SacredTails.PopupModule
{
    public interface IPopupManager : IService
    {
        void ShowInfoPopup(string textInfo, Dictionary<PopupManager.ButtonType, Action> buttonsActionsPair = null);

        void HideInfoPopup();
    }
}