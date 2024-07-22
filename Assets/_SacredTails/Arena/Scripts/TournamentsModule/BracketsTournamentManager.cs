using CoreRequestManager;
using Newtonsoft.Json;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.TournamentBehavior;
using UnityEngine;

public interface IBracketsTournament : IService
{
    public void ShowPanelBracketsView(bool active);
    public void SetAlreadyConnection(bool state);
    public CheckTournamentInscription CheckTournamentInscription { get; }
    public CheckTournamentStateController CheckTournamentStateController { get; }
    Action OnTournamentEnded { get; set; }
}

public class BracketsTournamentManager : MonoBehaviour, IBracketsTournament
{
    #region ----Fields----
    public TournamentBracketsShowController tournamentBracketsShowController;


    public CheckTournamentInscription checkTournamentInscription;
    public CheckTournamentInscription CheckTournamentInscription { get => checkTournamentInscription; }


    public CheckTournamentStateController checkTournamentStateController;
    public CheckTournamentStateController CheckTournamentStateController { get => checkTournamentStateController; }

    public bool alreadyRecheckConnection = false;
    public int currentStage = 0;

    public Action onTournamentEnded;
    public Action OnTournamentEnded { get => onTournamentEnded; set => onTournamentEnded = value; }

    #endregion ----Fields----

    #region ----Methods----
    public bool IsReady() { return true; }

    public void Awake()
    {
        if (checkTournamentInscription != null)
            checkTournamentInscription.onCheckIfTournamentExist += RecheckConection;
    }

    public void SetAlreadyConnection(bool state)
    {
        alreadyRecheckConnection = state;
    }

    public void RecheckConection()
    {
        if (alreadyRecheckConnection)
            return;

        alreadyRecheckConnection = true;
        if (PlayerPrefs.GetInt("DisconFromTournament", 0) == 1)
        {
            PlayerPrefs.SetInt("DisconFromTournament", 0);
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController.ExitTournament("You have disconnected from the tournament match, lose by default. As soon as the tournament ends you can see your end position at the backpack-Rank section-Tournament");
            return;
        }

        if (PlayerDataManager.Singleton.currentTournamentId != "")
        {
            GetUserDataRequest request = new GetUserDataRequest() { PlayFabId = "7F1965D480D991B5", };
            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest("", TypeOfTournamentRequest.GET_TOURNAMENT_LIST, (succesData) =>
            {
                List<TournamentEntry> succesDataJson = JsonConvert.DeserializeObject<SacredTailsPSDto<List<TournamentEntry>>>(succesData.FunctionResult.ToString()).data;
                foreach (var tournament in succesDataJson)
                {
                    if (tournament.tournamentId == PlayerDataManager.Singleton.currentTournamentId)
                    {
                        DateTime tournamentDate = DateTime.Parse(tournament.initTimeStage_1).ToUniversalTime();
                        ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController.ShowTimerInitTournament(tournamentDate);

                        ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentInscription.DisableObjectsInTournament(false);
                    }

                }
            });
        }
    }

    public void ShowPanelBracketsView(bool active)
    {
        tournamentBracketsShowController.gameObject.SetActive(active);
    }
    #endregion ----Methods----
}
