using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using Timba.SacredTails.Database;
using Timba.SacredTails.Arena;
using PlayFab.MultiplayerModels;
using System.Collections;
using UnityEngine.EventSystems;
using CoreRequestManager;
using Newtonsoft.Json;
using Timba.Games.SacredTails.PopupModule;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.UiHelpers;

public class PlayerUI : MonoBehaviour
{
    #region ---Fields---
    #region Matchmaking
    [Header("Matchmaking Fields")]
    [SerializeField] private Button seekFriendlyMatchBtn;
    [SerializeField] private Button seekRankedMatchBtn;
    [SerializeField] private Button cancelMatchBtn;
    [SerializeField] private GameObject matchmakingTimerPanel;
    [SerializeField] private TMP_Text timerLabel;
    [SerializeField] private GameObject matchTooltip;
    [SerializeField] private CanvasGroup UiCanvasGroup;
    private ThirdPersonController thirdPersonController;

    [Header("Matchmaking popup")]
    [SerializeField] private GameObject confirmMatchPopup;
    [SerializeField] private TMP_Text confirmMatchTimer;
    [SerializeField] private Button confirmMatchBtn;
    [SerializeField] private Button quitMatchBtn;

    private IDatabase database;
    private float waitTime = 0;
    private float confirmMatchTime = 40;
    private bool isWaitingMatch = false;
    private CreateMatchmakingTicketResult currentTicket;
    #endregion

    #region Display Name
    [Header("Player Display Name Fields")]
    [SerializeField] private Transform playerTagCanvas;
    private Vector3 originalDisplayNamePosition;
    private Quaternion originalDisplayNameRotation;

    [SerializeField] private GameObject displayNamePanel, battlePanel, backpackPanel, interactionPanel;
    [SerializeField] private TMP_Text displayNameLabel;
    [SerializeField] private TMP_Text profileNameLabel;
    string displayName;
    #endregion

    #endregion

    #region ---Methods---
    #region Init
    /// <summary>
    /// controlls the plyer UI events for the lobby interactions
    /// </summary>
    private void Start()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        if (!thirdPersonController.IsLocalPlayer)
            return;
        database = ServiceLocator.Instance.GetService<IDatabase>();
        PlayfabManager.Singleton.OnCreateTicketSuccess.AddListener(x => currentTicket = x);
        PlayfabManager.Singleton.OnMatchResultSuccess.AddListener(PromptMatchFoundPopUp);
        PlayfabManager.Singleton.OnMatchResultFailed.AddListener(() =>
        {
            CancelMatch();
            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("User is member of too many tickets, wait some time and try again later.");
        });

        seekFriendlyMatchBtn.onClick.AddListener(() => SearchMatch());
        cancelMatchBtn.onClick.AddListener(() => CancelMatch());
        quitMatchBtn.onClick.AddListener(() =>
        {
            CancelMatch();
            confirmMatchPopup.SetActive(false);
        });
        UIGroups.instance.NotifyDynamicPanel(UiCanvasGroup, "planner");
        if (UIGroups.instance.lastActivate == "planner")
            UiCanvasGroup.alpha = 1;
        else
            UiCanvasGroup.alpha = 0;
    }


    public void HideNameTag(bool reset = false)
    {
        playerTagCanvas.gameObject.SetActive(reset);
    }

    public void SearchMatch(bool initTimer = true)
    {
        isSearchingMatch = true;
        database.StartMatchmakingSequence();
        isWaitingMatch = true;
        cancelMatchBtn.gameObject.SetActive(true);
        if (initTimer)
            SearchMatchInitTimer();
    }

    public void CancelMatch(bool hideSearch = true)
    {
        if (currentTicket != null)
            PlayfabManager.Singleton.CancelMatchmaking(Constants.FRIENDLY_MATCH, currentTicket.TicketId);
        isSearchingMatch = false;
        isWaitingMatch = false;
        StopMatchmaking(hideSearch);
    }

    public void OnSpawn(string _displayName)
    {
        if (!string.IsNullOrEmpty(_displayName))
        {
            displayName = _displayName;
            SetPlayerNameLabel(displayName);
        }
    }

    public void SetPlayerNameLabel(string newValue)
    {
        displayNameLabel.text = newValue.ToString();
        profileNameLabel.text = newValue.ToString();
    }

    public void OnOffDisplayName(bool toggleState)
    {
        displayNamePanel.SetActive(toggleState);
    }
    #endregion

    #region Matchmaking

    bool isSearchingMatch = false;
    private void PromptMatchFoundPopUp(GetMatchResult getMatchResult)
    {
        if (!isSearchingMatch)
            return;
        isSearchingMatch = false;

        confirmMatchPopup.SetActive(true);
        StartCoroutine(MatchTimer(60, 0, true));
        confirmMatchBtn.onClick.RemoveAllListeners();
        confirmMatchBtn.onClick.AddListener(() =>
        {
            if (!accepted)
                accepted = true;
            confirmMatchBtn.onClick.RemoveAllListeners();
            confirmMatchPopup.SetActive(false);

            //Waiting for opponent to accept popup
            Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Waiting for opponent to accept", buttons);


            //Create server match
            PlayfabManager.Singleton.BattleServerCreateMatch(getMatchResult.MatchId, (result) =>
            {
                SacredTailsPSDto<object> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());

                StartCoroutine(WaitforOpponentToConfirm(getMatchResult));
            });

        });
    }
    public bool accepted = false;

    public string matchId = "TestMatch_2C332300EEA8B647";

    public void ViewMatch()
    {
        if (PlayerDataManager.Singleton.isOnTheTournament)
        {
            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You can not watch battles while you are registered in a tournament");
            return;
        }
        Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
        buttons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
        {
            FindObjectOfType<GameSceneManager>().SendBattle(matchId: GetComponent<ThirdPersonController>().currentMatchId, isViewing: true);
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
        });
        buttons.Add(PopupManager.ButtonType.BACK_BUTTON, () => ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup());
        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Do you want to see this player's match?", buttons);
    }

    IEnumerator MakeEndpointCall(string _url, string data, Action callback)
    {
        if (_url == "https://sacredtailsserver.azurewebsites.net/api/BattleServer/SelectShinseis")
        {
            Debug.Log("SelectShinseis: " + data);
        }

        UnityWebRequest request;
        request = UnityWebRequest.Post(_url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        callback?.Invoke();
    }

    private int checkCount = 0;
    private bool bothConfirmed = false;
    public IEnumerator WaitforOpponentToConfirm(GetMatchResult matchResult)
    {
        bothConfirmed = false;
        checkCount = 0;

        var gameSceneManager = FindObjectOfType<GameSceneManager>();
        int closestSpawnPoint = gameSceneManager.GetClosestBattleSpawn();
        while (!bothConfirmed || checkCount < 20)
        {
            PlayfabManager.Singleton.BattleServerCheckMatchConfirm(
                matchResult.MatchId,
                closestSpawnPoint,
                (result) =>
                {
                    SacredTailsPSDto<MatchConfirmedDto> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<MatchConfirmedDto>>(result.FunctionResult.ToString());
                    if (!dto.success)
                    {
                        checkCount++;
                        return;
                    }
                    if (bothConfirmed)
                        return;
                    bothConfirmed = true;

                    confirmMatchPopup.SetActive(false);
                    timerLabel.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(0);
                    confirmMatchTimer.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(0);
                    gameSceneManager.SendBattle(matchResult, null, dto.data.matchSpawnPoint);
                    CancelMatch();

                    ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
                });
            if (checkCount == 10)
            {
                Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
                buttons.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
                {
                    confirmMatchPopup.SetActive(false);
                    CancelMatch();
                    StopAllCoroutines();
                });
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Waiting for opponent to accept", buttons);
            }

            yield return new WaitForSeconds(3);
        }

        if (checkCount >= 20)
        {
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Opponent did not accept match");
            CancelMatch();
        }

    }
    public void DisplayMatchmakingOptions()
    {
        if (!isWaitingMatch)
            seekFriendlyMatchBtn.gameObject.SetActive(true);
    }

    private void StopMatchmaking(bool hideSearch)
    {
        if (hideSearch)
        {
            matchmakingTimerPanel.SetActive(false);
            cancelMatchBtn.gameObject.SetActive(false);
            timerLabel.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(0);
            confirmMatchTimer.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(0);
        }

        seekFriendlyMatchBtn.interactable = true;
        seekFriendlyMatchBtn.GetComponent<EventTrigger>().enabled = true;
    }

    public void SearchMatchInitTimer()
    {
        seekFriendlyMatchBtn.interactable = false;
        seekFriendlyMatchBtn.GetComponent<EventTrigger>().enabled = false;
        matchTooltip.SetActive(false);
        matchmakingTimerPanel.SetActive(true);
        //99:99 min
        StartCoroutine(MatchTimer(waitTime, 6039, false));
    }

    private IEnumerator MatchTimer(float startTime, float maxTime, bool isCountdown)
    {
        float localTime = Time.time;
        if (isCountdown)
        {
            while (startTime - (Time.time - localTime) > maxTime)
            {
                confirmMatchTimer.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(startTime - (Time.time - localTime));
                yield return null;
            }
            CancelMatch();
            confirmMatchPopup.SetActive(false);
        }
        else
        {
            float secondsPassed = startTime + (Time.time - localTime);
            while (secondsPassed < maxTime && isWaitingMatch)
            {
                secondsPassed = startTime + (Time.time - localTime);
                timerLabel.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(secondsPassed);

                if (Math.Floor(secondsPassed) != 0 && Math.Floor(secondsPassed) % 120 == 0)
                {
                    CancelMatch(hideSearch: false);
                    SearchMatch(initTimer: false);
                }
                yield return null;
            }

            if (secondsPassed >= maxTime)
            {
                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Exceded waiting time, please try again later.");
                CancelMatch();
            }
        }
        yield return null;
    }
    #endregion

    #region TEST

    public void TestMatchInit()
    {
        string matchId = "TestMatch_" + PlayerDataManager.Singleton.localPlayerData.playfabId;
        CleanPreviousMatch(matchId, () =>
        {
            PlayerPrefs.SetFloat("MatchSelectTime", -1);
            PlayfabManager.Singleton.BattleServerCreateMatch(
                matchId,
                (result) =>
                {
                    SacredTailsPSDto<object> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());
                    if (!dto.success)
                        return;

                    FindObjectOfType<GameSceneManager>().SendBattle(new GetMatchResult()
                    {
                        MatchId = "TestMatch_" + PlayerDataManager.Singleton.localPlayerData.playfabId,
                        Members = new List<MatchmakingPlayerWithTeamAssignment>() {
                        new MatchmakingPlayerWithTeamAssignment() {
                            Attributes = new MatchmakingPlayerAttributes(){ DataObject =  JsonUtility.ToJson(new CustomAtributes (){PlayerPlayfabId= "5352E306ACAB3F9B"}) }
                        },
                        new MatchmakingPlayerWithTeamAssignment() {
                            Attributes = new MatchmakingPlayerAttributes(){ DataObject =  JsonUtility.ToJson(new CustomAtributes (){PlayerPlayfabId= PlayerDataManager.Singleton.localPlayerData.playfabId} )}
                        },
                        }
                    });
                });
        });
    }


    public void CleanPreviousMatch(string matchId, Action callback)
    {
        StartCoroutine(MakeEndpointCall("https://sacredtailsserver.azurewebsites.net/api/BattleServer/DeleteMatch", "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"5352E306ACAB3F9B\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\"\r\n    }\r\n  }\r\n}",
        () =>
        {
            StartCoroutine(MakeEndpointCall("https://sacredtailsserver.azurewebsites.net/api/BattleServer/CreateMatch", "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"5352E306ACAB3F9B\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\"\r\n    }\r\n  }\r\n}",
            () =>
            {
                StartCoroutine(MakeEndpointCall("https://sacredtailsserver.azurewebsites.net/api/BattleServer/SelectShinseis", "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"5352E306ACAB3F9B\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\",\r\n      \"ShinseiIdList\": [0,1,2],\r\n      \"PlayerMatchData\": \"{\\r\\n  \\\"DisplayName\\\": \\\"jiufen\\\",\\r\\n  \\\"shinseisSelected\\\": true,\\r\\n  \\\"hasSurrender\\\": false,\\r\\n  \\\"confirmState\\\": true,\\r\\n  \\\"ShinseiParty\\\": [\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10090010011006003002100400000010120090031009001004009\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        39,\\r\\n        35,\\r\\n        28,\\r\\n        1\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 9,\\r\\n      \\\"shinseiRarity\\\": 2,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 46,\\r\\n        \\\"Attack\\\": 34,\\r\\n        \\\"Defence\\\": 30,\\r\\n        \\\"Luck\\\": 34,\\r\\n        \\\"Speed\\\": 43,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    },\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10060030011002001002100800200010050010031004002004002\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        29,\\r\\n        38,\\r\\n        15,\\r\\n        0\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 2,\\r\\n      \\\"shinseiRarity\\\": 2,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 34,\\r\\n        \\\"Attack\\\": 43,\\r\\n        \\\"Defence\\\": 38,\\r\\n        \\\"Luck\\\": 38,\\r\\n        \\\"Speed\\\": 34,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    },\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10100010011001000002100200000010080010031008003004008\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        7,\\r\\n        37,\\r\\n        19,\\r\\n        14\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 8,\\r\\n      \\\"shinseiRarity\\\": 1,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 34,\\r\\n        \\\"Attack\\\": 34,\\r\\n        \\\"Defence\\\": 30,\\r\\n        \\\"Luck\\\": 43,\\r\\n        \\\"Speed\\\": 30,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    }\\r\\n  ]\\r\\n}\"\r\n    }\r\n  }\r\n}",
                () =>
                {
                    callback?.Invoke();
                }));
            }));
        }));
    }
    #endregion TEST
    #endregion
}
