using CoreRequestManager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

public class CheckTournamentStateController : MonoBehaviour
{
    public void CheckTournamentState(bool? isLocalPlayerWon = null, bool shouldCheckWinByDefault = false)
    {
        StartCoroutine(CheckTournamentStateCoroutine(isLocalPlayerWon, shouldCheckWinByDefault));
    }

    private IEnumerator CheckTournamentStateCoroutine(bool? isLocalPlayerWon, bool shouldCheckWinByDefault)
    {
        var tournamentReadyController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController;
        //if (shouldCheckWinByDefault)
        tournamentReadyController.gameObject.SetActive(false);
        bool hasResponse = false;
        bool hasAlreadyShownPending = false;

        while (!hasResponse)
        {
            //"As soon as the tournament ends you can see your end position at the backpack-Rank section-Tournament"
            if (PlayerDataManager.Singleton.currentTournamentId != "")
            {
                ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.CHECK_TOURNAMENT_STATE, (succesData) =>
                {
                    SacredTailsPSDto<CheckStateOfTournamentDto> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<CheckStateOfTournamentDto>>(succesData.FunctionResult.ToString());
                    var bracketsManager = ServiceLocator.Instance.GetService<IBracketsTournament>();

                    Debug.Log("REGRESO DEL COMBATE dto.code: " + dto.code);

                    if (dto.code == 777)
                    {
                        SacredTailsLog.LogMessage("Tournament has ended", true);
                        hasResponse = true;

                        if (PlayerDataManager.Singleton.isBot)
                            tournamentReadyController.ExitTournament("The tournament has ended");
                        else if (dto.success)
                            //tournamentReadyController.ExitTournament("You are the winner! \n 1st Place! \n Amazing! \n Please check the Backpack-Rank section for your prize");
                            tournamentReadyController.ExitTournament("You are the winner! \n 1st Place! \n Amazing! \n");
                        else
                            //tournamentReadyController.ExitTournament("You are awesome \n 2nd Place! \n Please check the Backpack-Rank section for your prize");
                            tournamentReadyController.ExitTournament("You are awesome \n 2nd Place! \n");
                    }
                    else
                    {
                        if ((isLocalPlayerWon.HasValue && isLocalPlayerWon.Value == false) || dto.code == 432)
                        {
                            if (!PlayerDataManager.Singleton.isBot)
                            {
                                hasResponse = true;
                                tournamentReadyController.ExitTournament("You have lost the tournament.");
                            }
                            else
                            {
                                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Please wait for the test tournament to start again");
                                if (!PlayerDataManager.Singleton.isBotCreatorOfTournaments)
                                    petitonTimeRate = 60;
                                else
                                    petitonTimeRate = 5;
                            }

                            SacredTailsLog.LogMessage("User lost match", true);
                        }
                        else
                        {
                            tournamentReadyController.hasCreatedMatch = false;
                            tournamentReadyController.gameObject.SetActive(true);

                            if (dto.code == 43 && !dto.data.alredyPlayedPlayers.Contains(PlayerDataManager.Singleton.localPlayerData.playfabId))
                            {
                                SacredTailsLog.LogMessage("New round has already started, user needs to get ready for match", true);
                                hasResponse = true;
                                PlayerDataManager.Singleton.currentTournamentStage = dto.data.currentStage;
                                DateTime currentStageInitTime = DateTime.Parse(dto.data.nextRoundInitTime).ToUniversalTime();

                                var nowTime = DateTime.UtcNow;
                                var substraction = currentStageInitTime.Subtract(nowTime);
                                tournamentReadyController.currentCountdownDate = currentStageInitTime.AddMinutes(5);

                                if (substraction.TotalSeconds + 300 > 0)
                                {
                                    tournamentReadyController.StartReadyButton((float)substraction.TotalSeconds + 300);
                                    Debug.Log("StartReadyButton 01: " + (float)substraction.TotalSeconds + 300);
                                }
                                else
                                {
                                    tournamentReadyController.StartReadyButton(0);
                                    Debug.Log("StartReadyButton 02");
                                }
                            }
                            else if (!hasAlreadyShownPending)
                            {
                                SacredTailsLog.LogMessage("Match ended, show waiting for next round", true);
                                hasAlreadyShownPending = true;
                                // Check if stage change
                                tournamentReadyController.ShowPendingTimeAfterMatch(dto.success && dto.data != null ? dto.data.nextRoundInitTime : null);
                            }
                        }
                    }
                });
                yield return new WaitForSeconds(petitonTimeRate);
            }
            else
                yield break;
        }

    }
    public float petitonTimeRate = 3;


    public IEnumerator WaitForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
}

public class CheckStateOfTournamentDto
{
    public string nextRoundInitTime;
    public int currentStage;
    public List<string> alredyPlayedPlayers;
}
