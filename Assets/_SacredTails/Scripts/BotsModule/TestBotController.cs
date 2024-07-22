using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Timba.Games.SacredTails.Lobby;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Database;
using Timba.SacredTails.TournamentBehavior;
using UnityEngine;
using UnityEngine.Networking;

public static class EndpointsForBots
{
    public static string DELETE_MATCH = "https://sacredtailsserver.azurewebsites.net/api/BattleServer/DeleteMatch";
    public static string CREATE_MATCH = "https://sacredtailsserver.azurewebsites.net/api/BattleServer/CreateMatch";
    public static string SELECT_SHINSEIS = "https://sacredtailsserver.azurewebsites.net/api/BattleServer/SelectShinseis";
}

public class TestBotController : MonoBehaviour
{
    #region ----Fields-----
    [Header("Bot Settings")]
    public ChallengePlayerController challengePlayerController;
    public AuthController authController;
    public BotPlayfabIdsList botPlayfabIdsList;
    public int localBotIndex = 0;
    public int numberOfBots = 32;
    private bool shouldLoopMatches = false;
    private int startsFrom = 0;
    private static bool alreadyStarted = false;

    [Header("Test Tournament")]
    private bool testTournament = false;
    bool isCreationOfTournament = false;
    public SearchAndShow searchTournament;
    public TournamentCreationController tournamentCreationController;
    public float timeToWaitForTournamentCreation = 5;
    public const int MAX_NUMBER_OF_BOT = 64;

    [DllImport("user32.dll")]
    static extern int SetWindowText(IntPtr hWnd, string text);
    #endregion ----Fields-----

    #region ----Methods----
    #region Initialize
    public void Start()
    {
        if (alreadyStarted)
            return;
        alreadyStarted = true;

        PlayerDataManager.Singleton.isBot = true;
        SacredTailsLog.Init();

        List<string> arguments = Environment.GetCommandLineArgs().ToList();
        if (arguments.Count >= 3 && Regex.IsMatch(arguments[1], @"^\d+$") && Regex.IsMatch(arguments[2], @"^\d+$"))
        {
            startsFrom = Int32.Parse(arguments[1]);
            int newNumber = Int32.Parse(arguments[2]);

            if (newNumber + startsFrom <= MAX_NUMBER_OF_BOT)
                numberOfBots = newNumber;
            else
                numberOfBots = MAX_NUMBER_OF_BOT - startsFrom;

            timeToWaitForTournamentCreation = numberOfBots * 5;
            if (localBotIndex - startsFrom > 0)
                timeToWaitForTournamentCreation /= localBotIndex - startsFrom;
            else
                timeToWaitForTournamentCreation = 0;
            PlayerDataManager.Singleton.numberOfBots = numberOfBots;
        }

        if (arguments.Contains("RESTART"))
            PlayerPrefs.SetInt("currentBot", startsFrom);

        if (arguments.Contains("TEST_TOURNAMENT"))
        {
            testTournament = true;
            isCreationOfTournament = arguments.Contains("CREATE_TOURNAMENT");
            PlayerDataManager.Singleton.isBotCreatorOfTournaments = isCreationOfTournament;
        }

        if (arguments.Contains("LOOP"))
            shouldLoopMatches = true;

        challengePlayerController = this.GetComponent<ChallengePlayerController>();
        if (shouldLoopMatches)
            challengePlayerController.onChallengeAgain += StartNewMatch;
        localBotIndex = PlayerPrefs.GetInt("currentBot", startsFrom);

        Console.Title = "Bot-" + localBotIndex;
        // Log into playfab
        PlayfabManager.Singleton.OnLoginSucces.AddListener((result) =>
        {
            PlayerDataManager.Singleton.localPlayerData.playfabId = botPlayfabIdsList.playfabIdList[localBotIndex];

            var bracketsManager = ServiceLocator.Instance.GetService<IBracketsTournament>();
            if (isCreationOfTournament)
            {
                //On end tournament start again
                bracketsManager.OnTournamentEnded += () => CreateNewTournament(() => StartNewMatch());

                //Start first time
                CreateNewTournament(() => StartNewMatch());
            }
            else
            {
                if (testTournament)
                {
                    PlayerDataManager.Singleton.currentTournamentId = PlayerPrefs.GetString("currentBotTournamentId", "");
                    bracketsManager.OnTournamentEnded += () =>
                    {
                        alreadyOnTournament = false;
                        StartNewMatch();
                    };
                }
                else
                    SetChallengeData();

                StartNewMatch();
            }

            bool isLastBot = (localBotIndex + 1) == startsFrom + numberOfBots;
            int newBotIndex = !isLastBot ? localBotIndex + 1 : 0;

            PlayerPrefs.SetInt("currentBot", newBotIndex);


#if !UNITY_EDITOR
            if (!isLastBot)
            {
                Process process = new Process();
                process.StartInfo.FileName = Environment.CurrentDirectory + "\\SacredTails.exe";
                process.StartInfo.Arguments = $"{startsFrom} {numberOfBots} {(SacredTailsLog.isBot ? "IsBot" : "")} {(shouldLoopMatches ? "LOOP" : "")} {(testTournament ? "TEST_TOURNAMENT" : "")}";
                process.Start();
                Thread.Sleep(100);
                SetWindowText(process.MainWindowHandle, "Bot-" + localBotIndex);
            }
#endif
        });


        authController.SetLoginInfo($"bot{localBotIndex}@timba.co", "123456");
        authController.Login();

    }

    private void SetChallengeData()
    {
        UnityEngine.Debug.Log(startsFrom);
        UnityEngine.Debug.Log(numberOfBots);
        UnityEngine.Debug.Log(localBotIndex);

        int challengeBotIndex = ((startsFrom + numberOfBots) - 1) - (localBotIndex - startsFrom);
        UnityEngine.Debug.Log(challengeBotIndex);

        string playfabIdOtherBot = botPlayfabIdsList.playfabIdList[challengeBotIndex];
        challengePlayerController.isRecivingChallenge = localBotIndex >= startsFrom + (numberOfBots / 2);
        challengePlayerController.SetOtherPlayerInfo(playfabIdOtherBot, $"bot{challengeBotIndex}");
    }

    private static IntPtr FindWindowByProcessId(int processId)
    {
        IntPtr hWnd = IntPtr.Zero;
        foreach (Process pList in Process.GetProcesses())
        {
            if (pList.Id == processId)
            {
                hWnd = pList.MainWindowHandle;
                break;
            }
        }
        return hWnd;
    }
    #endregion Initialize

    #region Start Fights/Tournament
    bool alreadyOnTournament = false;
    public void CreateNewTournament(Action callback = null)
    {
        UnityEngine.Debug.Log("Create new tournament");
        tournamentCreationController.onTournamentCreation += (tournamentId) =>
        {
            PlayerPrefs.SetString("currentBotTournamentId", tournamentId);
            PlayerDataManager.Singleton.currentTournamentId = tournamentId;
            alreadyOnTournament = false;
            callback?.Invoke();
            UnityEngine.Debug.Log("Create new tournament success");
        };
        tournamentCreationController.CreateTournament();
    }

    public void StartNewMatch()
    {
        PlayerDataManager.Singleton.isBot = true;
        SacredTailsLog.LogMessageForBot($"Start new match bot");
        if (testTournament)
        {
            if (alreadyOnTournament)
                return;
            alreadyOnTournament = true;

            if (!isCreationOfTournament)
            {
                UnityEngine.Debug.Log("start new tournament !creation");
                StartCoroutine(WaitForSeconds(timeToWaitForTournamentCreation, () =>
                {
                    PlayerDataManager.Singleton.currentTournamentId = PlayerPrefs.GetString("currentBotTournamentId", "");
                    searchTournament.gameObject.SetActive(true);
                    searchTournament.SearchAndShowTournaments();
                }));
            }
            else
            {
                UnityEngine.Debug.Log("start new tournament creation");
                searchTournament.gameObject.SetActive(true);
                searchTournament.SearchAndShowTournaments();
            }

            return;
        }

        challengePlayerController.ResetAlreadyHasChallenge();
        challengePlayerController.bothConfirmed = false;
        challengePlayerController.checkCount = 0;
        SacredTailsLog.LogMessageForBot($"START NEW CASUAL MATCH, isRecieving: {challengePlayerController.isRecivingChallenge}");
        if (!challengePlayerController.isRecivingChallenge)
            StartCoroutine(DeleteMatch(challengePlayerController.GetMatchId(), () => challengePlayerController.CheckChallenge()));
        else
            challengePlayerController.CheckChallenge();


    }
    #endregion Start Fights/Tournament

    #region End
    IEnumerator DeleteMatch(string matchId, Action callback = null)
    {
        string _url = EndpointsForBots.DELETE_MATCH;
        string data = "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"5352E306ACAB3F9B\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\"\r\n    }\r\n  }\r\n}";

        UnityWebRequest request;
        request = UnityWebRequest.Post(_url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        SacredTailsLog.LogMessage(request.result.ToString());
        callback?.Invoke();
    }

    private void OnDestroy()
    {
        SacredTailsLog.OnEnd(localBotIndex);
        if (isCreationOfTournament)
            PlayerPrefs.SetString("currentBotTournamentId", "");
    }
    #endregion End

    #region Helpers
    IEnumerator WaitForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
    #endregion Helpers
    #endregion ----Methods----
}
