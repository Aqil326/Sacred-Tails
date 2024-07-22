using CoreRequestManager;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.TournamentBehavior
{
    public class TournamentReadyController : MonoBehaviour
    {
        #region ----Fields----
        Coroutine TryToPlayRoutine;
        Coroutine timerCoroutine;
        [SerializeField] GameObject tournamentFlag;
        [SerializeField] GameObject tournamentSeeBracketsFlag;
        [SerializeField] Animator tournamentAnim;
        [SerializeField] public Button readyButton;
        [SerializeField] public TMP_Text labelTimer;
        [SerializeField] public TMP_Text timerText;
        public DateTime currentStageTime;

        public float timerLeft = 0;
        public DateTime? currentCountdownDate;
        public bool hasCreatedMatch = false;
        bool isReadyTimerOn = false;
        #endregion ----Fields----

        #region ----Methods----
        #region >> Init tournament <<
        private void Start()
        {
            readyButton.onClick.AddListener(() =>
            {
                var bracketsManager = ServiceLocator.Instance.GetService<IBracketsTournament>();
                bracketsManager.CheckTournamentInscription.DisableObjectsInTournament(false);
            });
        }

        public void Enter()
        {
            TryToPlayRoutine = StartCoroutine(TryToPlay());
        }

        public void ShowTimerInitTournament(DateTime initTimeTournament)
        {

            currentStageTime = initTimeTournament;
            EnableReadyButton(false);
            labelTimer.text = "Tournament starts in:";

            var nowTime = DateTime.UtcNow;
            var substraction = initTimeTournament.Subtract(nowTime);
            if (substraction.TotalSeconds > 0)
            {
                currentCountdownDate = initTimeTournament;
                timerLeft = (float)substraction.TotalSeconds;
                PlayerPrefs.SetInt("isReadyForRound", 0);
                ShowTimer(true, () =>
                {
                    // Init tournament by time
                    ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.JOIN_TOURNAMENT, null);
                    StartReadyButton();
                });

                Debug.Log("RETOMO 01");
            }
            else
            {
                ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.CHECK_READY_STATE, (succesResult) =>
                    {
                        // Init tournament by time
                        ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.JOIN_TOURNAMENT, null);
                        SacredTailsPSDto<BracketsData> response = JsonConvert.DeserializeObject<SacredTailsPSDto<BracketsData>>(succesResult.FunctionResult.ToString());

                        // Tournament hasn't started yet
                        if (response.code == 4)
                            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.JOIN_TOURNAMENT, null);

                        if (response.code == 3)
                        {
                            currentCountdownDate = initTimeTournament;
                            StartReadyButton((float)initTimeTournament.Subtract(DateTime.UtcNow).TotalSeconds + 300);
                        }
                        else
                            StartReadyButton(-10);

                        Debug.Log("RETOMO 02, response.code: " + response.code);
                    }, false);
            }

        }
        #endregion >> Init tournament <<

        #region >> Ready confirm <<
        IEnumerator TryToPlay()
        {
            hasCreatedMatch = false;
            while (isReadyTimerOn)
            {
                MarkAsReady();
                yield return new WaitForSeconds(4);
            }

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You enemy didn't get ready for match, you won this match.");
            ShowPendingTimeAfterMatch();
        }

        public void StartReadyButton(float _timerLeft = -1)
        {
            SacredTailsLog.LogMessage("Show ready button", true);
            EnableReadyButton(true);

            labelTimer.text = "Get ready time limit:";

            if (_timerLeft < -1)
            {
                currentCountdownDate = null;
                ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentStateController.CheckTournamentState();
                return;
            }
            else if (_timerLeft != -1)
                timerLeft = _timerLeft;
            else
            {
                currentCountdownDate = null;
                timerLeft = 300; //5 min
            }

            isReadyTimerOn = true;
            Debug.Log("PlayerDataManager.Singleton.isBot: " + PlayerDataManager.Singleton.isBot);
            if (PlayerDataManager.Singleton.isBot)
                Enter();
            ShowTimer(false, () =>
            {
                isReadyTimerOn = false;
                /*if (PlayerPrefs.GetInt("isReadyForRound", 0) != 1)
                    ExitTournament("You didn't get ready for your match, you have been disqualified from the tournament.");
                else
                    ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentStateController.CheckTournamentState();*/
                ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentStateController.CheckTournamentState();

                PlayerPrefs.SetInt("isReadyForRound", 0);
            });
        }

        public void MarkAsReady(bool isMarkingReady = true)
        {
            Debug.Log("MarkAsReady Run");
            EnableReadyButton(false);
            SacredTailsLog.LogMessage("Marked as ready init", true);
            //TODO take the ID of the tournament from player data
            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.CHECK_READY_STATE, (succesResult) =>
            {
                SacredTailsPSDto<BracketsData> response = JsonConvert.DeserializeObject<SacredTailsPSDto<BracketsData>>(succesResult.FunctionResult.ToString());
                BracketsData bracketsData = response.data;

                if (CheckIfWinByDefault(bracketsData, response))
                {
                    SacredTailsLog.LogMessage("Win by default", true);
                    return;
                }

                if (PlayerPrefs.GetInt("isReadyForRound", 0) != 1)
                    PlayerPrefs.SetInt("isReadyForRound", 1);
                if (!response.success)
                {
                    SacredTailsLog.LogMessage("Response did not succed, trying again...", true);
                    tournamentFlag.SetActive(true);
                    tournamentFlag.transform.GetChild(1).GetComponent<Animator>().Play("open");
                    return;
                }

                if (!hasCreatedMatch)
                    CreateMatch(response.data);
                if (TryToPlayRoutine != null)
                {
                    StopCoroutine(TryToPlayRoutine);
                    TryToPlayRoutine = null;
                }

                GetMatchResult matchData = new GetMatchResult()
                {
                    MatchId = bracketsData.matchId,
                    Members = new List<MatchmakingPlayerWithTeamAssignment>()
                    {
                        new MatchmakingPlayerWithTeamAssignment(){
                           Attributes= new MatchmakingPlayerAttributes  (){
                                DataObject =new CustomAtributes() {
                                    PlayerPlayfabId = bracketsData.PID1 != PlayerDataManager.Singleton.localPlayerData.playfabId?bracketsData.PID1:bracketsData.PID2,
                                    displayName  = bracketsData.PID1 != PlayerDataManager.Singleton.localPlayerData.playfabId?bracketsData.displayName1:bracketsData.displayName2,
                                    }
                                }
                            },
                        new MatchmakingPlayerWithTeamAssignment(){
                           Attributes= new MatchmakingPlayerAttributes  (){
                                DataObject =new CustomAtributes() {
                                    PlayerPlayfabId = PlayerDataManager.Singleton.localPlayerData.playfabId,
                                    displayName = PlayerDataManager.Singleton.localPlayerData.playerName
                                    }
                                }
                            },
                    }
                };
                FindObjectOfType<GameSceneManager>().SendBattle(matchData, null, 0, false);
                PlayerPrefs.SetInt("isReadyForRound", 0);
            }, isMarkingReady);
        }

        public bool CheckIfWinByDefault(BracketsData bracketsData, SacredTailsPSDto<BracketsData> response)
        {
            try
            {
                if (bracketsData.PID1 == "WinByDefault" || bracketsData.PID2 == "WinByDefault")
                {
                    if (PlayerPrefs.GetInt("isReadyForRound", 0) != 1)
                    {
                        ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentStateController.CheckTournamentState(shouldCheckWinByDefault: response.code == 1);
                        //if (response.code == 1)
                        //{
                        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You won this match by default.");
                        ShowPendingTimeAfterMatch();
                        PlayerPrefs.SetInt("isReadyForRound", 1);
                        //}
                        StopAllCoroutines();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                SacredTailsLog.LogErrorMessage(JsonConvert.SerializeObject(response));
                return false;
            }
        }
        #endregion >> Ready confirm <<

        #region >> End stage time <<
        public void ShowPendingTimeAfterMatch(string nextStageTime = null)
        {
            SacredTailsLog.LogMessage("Show pending time to init next round", true);
            //Show ready button and ready timer
            EnableReadyButton(false);

            labelTimer.text = "Next round begin in:";
            GetTimerLeftForNextStage(nextStageTime);

            ShowTimer(false, () =>
            {
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Next stage of tournament has begun!");
                StartReadyButton();
            });
        }

        /// <summary>
        /// Set current timer to time left for next round
        /// </summary>
        /// <param name="nextStageTime"></param>
        public void GetTimerLeftForNextStage(string nextStageTime)
        {
            if (nextStageTime == null)
            {
                var nowTime = DateTime.UtcNow;
                var substraction = currentStageTime.Subtract(nowTime);

                currentCountdownDate = currentStageTime.AddMinutes(5).AddMinutes(15);
                if (substraction.TotalSeconds + 300 + 900 > 0)
                    timerLeft = (float)substraction.TotalSeconds + 300 + 900; //5 min + 15min
                else
                    timerLeft = 0;
            }
            else
            {
                DateTime newStageTimeDate = DateTime.Parse(nextStageTime).ToUniversalTime();

                var nowTime = DateTime.UtcNow;
                var substraction = newStageTimeDate.Subtract(nowTime);

                currentCountdownDate = newStageTimeDate;
                if (substraction.TotalSeconds > 0)
                    timerLeft = (float)substraction.TotalSeconds;
                else
                    timerLeft = 0;
            }
        }
        #endregion >> End stage time <<

        #region >> Helpers <<
        public void OnEnable()
        {
            SacredTailsLog.LogMessage("Gas gas gas");
        }

        public void OnDisable()
        {
            SacredTailsLog.LogMessage("Cool vibrations");
        }

        public void CreateMatch(BracketsData _bracketsData)
        {
            SacredTailsLog.LogMessageForBot("Creating tournament match");

            float auxTimerLeft = (timerLeft + 30) > 300 ? 300 : timerLeft + 30;
            PlayerPrefs.SetFloat("MatchSelectTime", auxTimerLeft);

            PlayfabManager.Singleton.BattleServerCreateMatch(_bracketsData.matchId, (result) =>
            {
                SacredTailsPSDto<object> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());
                if (!dto.success)
                    return;
                
                SacredTailsLog.LogMessageForBot("Tournament match created");
                hasCreatedMatch = true;

            });
        }

        public void ExitTournament(string message = null, Action finishCallback = null)
        {
            SacredTailsLog.LogMessageForBot("Exiting tournament");
            gameObject?.SetActive(false);
            if (message != null)
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup(message);

            var bracketsManager = ServiceLocator.Instance.GetService<IBracketsTournament>();
            bracketsManager?.SetAlreadyConnection(true);
            bracketsManager?.CheckTournamentInscription?.DisableObjectsInTournament(true);
            bracketsManager?.ShowPanelBracketsView(false);

            tournamentSeeBracketsFlag?.SetActive(false);
            tournamentFlag?.SetActive(false);

            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest(PlayerDataManager.Singleton.currentTournamentId, TypeOfTournamentRequest.CHECK_TOURNAMENT_STATE,
                (succesResult) =>
                {
                    PlayerDataManager.Singleton.currentTournamentId = "";
                    PlayfabManager.Singleton.RemoveUserData(new List<string>() { "CurrentTournament" }, (result) =>
                    {
                        SacredTailsLog.LogMessageForBot("Tournament exit successful");
                        ServiceLocator.Instance.GetService<IBracketsTournament>().OnTournamentEnded?.Invoke();
                        bracketsManager?.CheckTournamentInscription?.EnableNPCs();
                        finishCallback?.Invoke();
                    });
                });

        }

        public void EnableReadyButton(bool isReadyEnable)
        {
            readyButton.gameObject.SetActive(isReadyEnable);
        }

        public void ShowTimer(bool showHour, Action onEndTimer)
        {
            if (timerCoroutine != null)
                StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(ShowTimerCoroutine(showHour, onEndTimer));
        }

        private int timesUpdating = 0;
        public IEnumerator ShowTimerCoroutine(bool showHour, Action onEndTimer)
        {
            while (timerLeft > 0)
            {
                timerLeft--;

                if (currentCountdownDate.HasValue)
                {
                    timesUpdating++;
                    if (timesUpdating > 10)
                    {
                        timesUpdating = 0;
                        var substraction = currentCountdownDate.Value.Subtract(DateTime.UtcNow);
                        timerLeft = (float)substraction.TotalSeconds;

                        if (timerLeft < 0)
                            continue;
                    }
                }
                ShowTimer(timerLeft, showHour);
                yield return new WaitForSecondsRealtime(1);
            }

            onEndTimer?.Invoke();
        }

        public void ShowTimer(float timerLeft, bool showHour)
        {
            timerText.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(timerLeft, null, showHour);
        }
        #endregion >> Helpers <<
        #endregion ----Methods----
    }
}