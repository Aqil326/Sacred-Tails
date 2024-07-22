using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

/// <summary>
/// Call all necesary actions to leave the tournament
/// </summary>
public class PendingVariableNPC : MonoBehaviour
{
    [SerializeField] List<GameObject> DependientNPC = new List<GameObject>();
    public CheckTournamentInscription CheckTournamentInscription;

    /// <summary>
    /// Change between two npc with different dialogs in the same place
    /// </summary>
    /// <param name="value">Index of npc</param>
    public void ShowVendor(int value)
    {
        foreach (var item in DependientNPC)
        {
            item.SetActive(false);
        }
        DependientNPC[value].gameObject.SetActive(true);
    }

    public void LeaveTournament()
    {
        PopupManager popupManager = ServiceLocator.Instance.GetService<PopupManager>();
        Dictionary<PopupManager.ButtonType, Action> mainButtons = new Dictionary<PopupManager.ButtonType, Action>();
        mainButtons.Add(PopupManager.ButtonType.BACK_BUTTON, () => { popupManager.HideInfoPopup(); });
        mainButtons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
        {
            ShowVendor(0);
            popupManager.HideInfoPopup();
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController.ExitTournament("You have leave your current tournament successfully");
        });
        popupManager.ShowInfoPopup("You have desided to not participate in the tournament \n it will cost 75% of your investment", mainButtons);
    }
}
