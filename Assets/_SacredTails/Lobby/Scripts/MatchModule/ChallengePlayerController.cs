using CoreRequestManager;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using UnityEngine;

public class ChallengePlayerController : MonoBehaviour
{
    #region ----Fields----
    public bool isRecivingChallenge;
    private ThirdPersonController otherPlayer;
    private string otherPlayerPlayfabId;
    private string otherPlayerDisplayName;
    private bool alreadyHasAChallenge;
    private bool alreadyCanceled;
    private string otherRandomMatchNumber;
    private List<Coroutine> currentCoroutines = new List<Coroutine>();

    private ILobbyNetworkManager lobbyNetworkingManager;
    private IPopupManager popupManager;
    private string matchId;
    public Action onChallengeAgain;

    #endregion ----Fields----

    #region ----Methods----

    #region Recieve Challenge
    public void RecieveChallenge(ThirdPersonController _otherPlayer, string _otherRandomMatchNumber)
    {
        otherPlayer = _otherPlayer;
        otherRandomMatchNumber = _otherRandomMatchNumber;
        CheckChallenge();
        otherPlayer = null;
    }
    #endregion Recieve Challenge

    #region Check Challenge Main Logic

    public void CheckChallenge()
    {
        SacredTailsLog.LogMessageForBot($"Check Challenge");
        if (String.IsNullOrEmpty(otherPlayerPlayfabId) || String.IsNullOrEmpty(otherPlayerDisplayName))
        {
            if (otherPlayer == null)
                otherPlayer = GetComponent<ThirdPersonController>();
            otherPlayerPlayfabId = otherPlayer.playfabId;
            otherPlayerDisplayName = otherPlayer.displayName;
        }
        SacredTailsLog.LogMessageForBot($"Check Challenge otherPlayerId: {otherPlayerPlayfabId} otherPlayerName: {otherPlayerDisplayName}");

        if (CheckChallengeVerifications())
            return;
        SacredTailsLog.LogMessageForBot($"Check Challenge Verification");

        alreadyHasAChallenge = true;
        if (!PlayerDataManager.Singleton.isBot)
        {
            string message = isRecivingChallenge ? $"{otherPlayerDisplayName} Challenges you to a duel" : $"Do you want to challenge player: {otherPlayerDisplayName}?";
            Dictionary<PopupManager.ButtonType, Action> mainButtons = new Dictionary<PopupManager.ButtonType, Action>();
            mainButtons.Add(PopupManager.ButtonType.BACK_BUTTON, () => BackButtonLogic(isRecivingChallenge));
            mainButtons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => ConfirmMatch(message));

            if (!isRecivingChallenge)
                popupManager.ShowInfoPopup(message, mainButtons);
            else
                StartCoroutine(TimingAcceptChallenge(message, mainButtons, true));
        }
        else
            ConfirmMatch("");
    }

    private bool CheckChallengeVerifications()
    {
        if (PlayerDataManager.Singleton.isOnTheTournament)
        {
            popupManager.ShowInfoPopup("You can not make battles if you are registered in a tournament");
            return true;
        }
        if (alreadyHasAChallenge || alreadyCanceled)
        {
            if (!isRecivingChallenge)
            {
                alreadyCanceled = false;
                popupManager.ShowInfoPopup("Player is already on a challenge right now");
            }
            SacredTailsLog.LogMessageForBot($"Challenge Dafuk");
            return true;
        }

        if (!PlayerDataManager.Singleton.isBot && !isRecivingChallenge && lobbyNetworkingManager.GetPlayerState(otherPlayerPlayfabId) == Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.COMBAT)
        {
            popupManager.ShowInfoPopup("Player is on a match, can not send challege right now");
            return true;
        }

        bool isAlreadyOnChallenge = !PlayerDataManager.Singleton.isBot && !isRecivingChallenge && lobbyNetworkingManager.CheckIfPlayerHasChallengeOrIsChallenging(otherPlayerPlayfabId);
        if (isAlreadyOnChallenge)
        {
            popupManager.ShowInfoPopup("Player is already on a challenge");
            return true;
        }

        return false;
    }

    public void ConfirmMatch(string message)
    {
        SacredTailsLog.LogMessageForBot($"Confirm match challenge player {PlayerDataManager.Singleton.isBot}");
        if (PlayerDataManager.Singleton.isBot)
        {
            string id = isRecivingChallenge ? PlayerDataManager.Singleton.localPlayerData.playfabId : otherPlayerPlayfabId;
            SacredTailsLog.LogMessageForBot($"id: {id}");

            string randomId = "";
            if (!isRecivingChallenge)
            {
                randomId = GenerateRandomMatchId();
                PlayerPrefs.SetString($"BotRandomId_{id}", randomId);
            }
            else
            {
                randomId = PlayerPrefs.GetString($"BotRandomId_{id}", "");
                if (randomId == PlayerDataManager.Singleton.botChallengeId)
                {
                    SacredTailsLog.LogMessageForBot($"Waiting for creator of challenge to create it, prev: {PlayerDataManager.Singleton.botChallengeId} get: {randomId}");
                    StartCoroutine(WaitForSeconds(3, () => ConfirmMatch(message)));
                    return;
                }
            }
            SacredTailsLog.LogMessageForBot($"challenge randomId created: {randomId}");

            PlayerDataManager.Singleton.botChallengeId = randomId;
            matchId = GetMatchId(randomId);
        }
        else
        {
            SetMatchIdAndChallengeInPlayerData(message);
            SacredTailsLog.LogMessageForBot($"challenge isnotbot");
        }

        SacredTailsLog.LogMessageForBot($"Create Match");
        PlayerPrefs.SetFloat("MatchSelectTime", -1);
        PlayfabManager.Singleton.BattleServerCreateMatch(matchId, (result) =>
        {
            if (!PlayerDataManager.Singleton.isBot)
            {
                Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
                buttons.Add(PopupManager.ButtonType.BACK_BUTTON, () => BackButtonLogic());
                //Waiting for opponent to accept popup
                if (isRecivingChallenge)
                    popupManager.ShowInfoPopup("Waiting for your opponent", buttons);
                else
                    StartCoroutine(TimingAcceptChallenge("Waiting for your opponent", buttons, isRecivingChallenge));
            }

            SacredTailsPSDto<bool?> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<bool?>>(result.FunctionResult.ToString());
            bool didCreateMatch = dto.success && dto.data.HasValue && dto.data.Value;

            if (PlayerDataManager.Singleton.isBot && !dto.success)
            {
                if ((isRecivingChallenge && dto.code == 21) || (isRecivingChallenge && didCreateMatch))
                    StartCoroutine(WaitForSeconds(2, () => ConfirmMatch(message)));
                else if (!isRecivingChallenge && !didCreateMatch)
                    StartCoroutine(WaitForSeconds(1, () => onChallengeAgain?.Invoke()));
                return;
            }


            currentCoroutines.Add(StartCoroutine(WaitforOpponentToConfirm(GetMatchResultPayload())));
        });
    }


    IEnumerator WaitForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }

    public string GenerateRandomMatchId()
    {
        string guidString = Guid.NewGuid().ToString();
        string uid = guidString.Replace("-", ""); ;
        return uid;
    }

    public void SetMatchIdAndChallengeInPlayerData(string popupMessage)
    {
        Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
        popupManager.HideInfoPopup();
        popupManager.ShowInfoPopup(popupMessage, buttons);

        string randomNumber;
        randomNumber = isRecivingChallenge ? otherRandomMatchNumber : GenerateRandomMatchId();
        matchId = GetMatchId(randomNumber);

        //Set currentChallenge in player Data
        if (!isRecivingChallenge)
        {
            PlayerDataManager.Singleton.localPlayerData.challengedPlayer = otherPlayerPlayfabId + "_" + randomNumber;
            lobbyNetworkingManager.TickCheckActivates();
        }
    }

    public void BackButtonLogic(bool waitCancel = true)
    {
        PlayerDataManager.Singleton.localPlayerData.challengedPlayer = isRecivingChallenge ? "CANCEL" : "";
        lobbyNetworkingManager.TickCheckActivates();
        popupManager.HideInfoPopup();
        alreadyCanceled = false;
        alreadyHasAChallenge = false;

        //Cancel Selection
        if (!String.IsNullOrEmpty(matchId))
            PlayfabManager.Singleton.BattleServerSelectShinseis(matchId, null, new List<int> { -1, -1, -1 }, null, null);
        StopAllCoroutines();
        StartCoroutine(MatchCanceledWait(waitCancel));
    }
    #endregion Check Challenge Main Logic

    #region Coroutines
    public IEnumerator TimingAcceptChallenge(string mainMessage, Dictionary<PopupManager.ButtonType, Action> mainButtons, bool isRecievingChallenge)
    {

        int maxSecondsBetweenPetition = Mathf.FloorToInt(lobbyNetworkingManager.CurrentSecondsBetweenPetition * 2);
        int counter = isRecievingChallenge ? 60 - maxSecondsBetweenPetition : 60;
        bool clickOnConfirm = false;
        if (mainButtons.ContainsKey(PopupManager.ButtonType.CONFIRM_BUTTON))
            mainButtons[PopupManager.ButtonType.CONFIRM_BUTTON] += () => { clickOnConfirm = true; counter = 0; };
        if (mainButtons.ContainsKey(PopupManager.ButtonType.BACK_BUTTON))
            mainButtons[PopupManager.ButtonType.BACK_BUTTON] += () => { counter = 0; };

        while (counter > 0)
        {
            if (!isRecievingChallenge)
            {
                lobbyNetworkingManager.TickCheckActivates();
                bool isAlreadyOnChallenge = lobbyNetworkingManager.CheckIfOtherPlayerHasChallengeForPlayfabId(PlayerDataManager.Singleton.localPlayerData.playfabId);
                if (isAlreadyOnChallenge)
                {
                    BackButtonLogic();
                    yield break;
                }
            }
            counter--;
            string counterText = counter < 10 ? $"0:0{counter}" : $"0:{counter}";
            popupManager.ShowInfoPopup($"{mainMessage}\r\n {counterText}", mainButtons);
            yield return new WaitForSeconds(1);
        }
        if (!isRecievingChallenge)
        {
            Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
            buttons.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
            {
                PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
                lobbyNetworkingManager.TickCheckActivates();
                popupManager.HideInfoPopup();
                StartCoroutine(MatchCanceledWait());
            });
            popupManager.ShowInfoPopup("Waiting for your opponent", buttons);
        }
        if (!alreadyCanceled && !clickOnConfirm && isRecivingChallenge)
            mainButtons[PopupManager.ButtonType.BACK_BUTTON]?.Invoke();
        yield return null;
    }

    public bool bothConfirmed = false;
    public int checkCount = 0;
    public IEnumerator WaitforOpponentToConfirm(GetMatchResult matchData)
    {
        bothConfirmed = false;
        checkCount = 0;

        Dictionary<PopupManager.ButtonType, Action> mainButtons = new Dictionary<PopupManager.ButtonType, Action>();
        mainButtons.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
        {
            PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
            lobbyNetworkingManager.TickCheckActivates();
            popupManager.HideInfoPopup();
            StartCoroutine(MatchCanceledWait());
        });
        var gameSceneManager = FindObjectOfType<GameSceneManager>();

        int closestSpawnPoint = 0;

        if (!PlayerDataManager.Singleton.isBot)
            closestSpawnPoint = gameSceneManager.GetClosestBattleSpawn();

        while (alreadyHasAChallenge && !alreadyCanceled && !bothConfirmed && checkCount < 20)
        {
            PlayfabManager.Singleton.BattleServerCheckMatchConfirm(
                matchData.MatchId,
                closestSpawnPoint,
                (result) =>
                {
                    SacredTailsPSDto<MatchConfirmedDto> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<MatchConfirmedDto>>(result.FunctionResult.ToString());
                    if (!dto.success)
                    {
                        if (!PlayerDataManager.Singleton.isBot)
                            checkCount++;
                        return;
                    }
                    if (bothConfirmed)
                        return;
                    bothConfirmed = true;

                    PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
                    ResetAlreadyHasChallenge();
                    lobbyNetworkingManager.TickCheckActivates();
                    gameSceneManager.SendBattle(matchData, null, dto.data.matchSpawnPoint, isBotFight: PlayerDataManager.Singleton.isBot);

                    popupManager.HideInfoPopup();
                    StartCoroutine(MatchCanceledWait(false));
                });
            yield return new WaitForSeconds(3);
        }
    }

    public IEnumerator MatchCanceledWait(bool waitPetitionTime = true)
    {
        if (alreadyCanceled)
            yield break;
        matchId = "";
        alreadyCanceled = true;
        currentCoroutines = currentCoroutines.Where(coroutine => coroutine != null).ToList();
        currentCoroutines.ForEach((coroutine) => StopCoroutine(coroutine));
        if (waitPetitionTime)
            yield return new WaitForSeconds(lobbyNetworkingManager.CurrentSecondsBetweenPetition + 1);
        ResetAlreadyHasChallenge();
    }
    #endregion Coroutines

    #region Helpers

    public string GetMatchId(string randomNumber = "")
    {
        string matchId;
        if (isRecivingChallenge)
            matchId = PlayerDataManager.Singleton.localPlayerData.playfabId + "_" + otherPlayerPlayfabId + "_" + randomNumber;
        else
            matchId = otherPlayerPlayfabId + "_" + PlayerDataManager.Singleton.localPlayerData.playfabId + "_" + randomNumber;
        return matchId;
    }

    public GetMatchResult GetMatchResultPayload()
    {
        GetMatchResult matchData = new GetMatchResult()
        {
            MatchId = matchId,
            Members = new List<MatchmakingPlayerWithTeamAssignment>()
                {
                        new MatchmakingPlayerWithTeamAssignment(){
                           Attributes= new MatchmakingPlayerAttributes  (){
                                DataObject =new CustomAtributes() {
                                    PlayerPlayfabId = otherPlayerPlayfabId,
                                    displayName =otherPlayerDisplayName
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
        return matchData;
    }

    public void SetOtherPlayerInfo(string otherPlayerId, string otherPlayerName)
    {
        otherPlayerDisplayName = otherPlayerName;
        otherPlayerPlayfabId = otherPlayerId;
    }

    public void ResetAlreadyHasChallenge()
    {
        alreadyHasAChallenge = false;
        alreadyCanceled = false;
    }

    public void MatchCanceledByChallenged(bool showOtherPlayerDeclinePopup)
    {
        popupManager.HideInfoPopup();
        StopAllCoroutines();
        StartCoroutine(MatchCanceledWait());
        if (!showOtherPlayerDeclinePopup)
            return;

        Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
        buttons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => popupManager.HideInfoPopup());
        popupManager.ShowInfoPopup("The other player didn't accept you match.", buttons);
    }
    #endregion Helpers

    #region UnityMethods
    private void OnEnable()
    {
        if (PlayerDataManager.Singleton != null)
            PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
        alreadyHasAChallenge = false;
        alreadyCanceled = false;
        lobbyNetworkingManager = ServiceLocator.Instance.GetService<ILobbyNetworkManager>();
        popupManager = ServiceLocator.Instance.GetService<IPopupManager>();
        popupManager.HideInfoPopup();
    }

    private void OnDestroy()
    {
        if (bothConfirmed || lobbyNetworkingManager == null)
            return;
        PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
        lobbyNetworkingManager.TickCheckActivates();
        popupManager.HideInfoPopup();
        StartCoroutine(MatchCanceledWait());
    }
    #endregion UnityMethods
    #endregion ----Methods----
}

public class MatchConfirmedDto
{
    public int matchSpawnPoint = -1;
}
