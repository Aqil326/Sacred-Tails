using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.UiHelpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Timba.SacredTails.TournamentBehavior
{
    public class TournamentCreationController : MonoBehaviour
    {
        public GameObject loadingScreen;
        public CalendarController calendarController;
        public TMP_InputField tournamentName;
        public TMP_Dropdown maxPlayerInput;
        [SerializeField] private List<string> allowedPlayfabIds;
        public UnityEvent onLoginSuccess;

        public void CheckIfUserIsAllowed(LoginResult resultLogin)
        {
            if (allowedPlayfabIds.Contains(resultLogin.PlayFabId))
                onLoginSuccess?.Invoke();
            else
            {
                loadingScreen.SetActive(false);
                ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup("Unaouthorized account for tournament creation, closing tournament creator.");
                Application.Quit();
            }
        }
        public Action<string> onTournamentCreation;

        public void CreateTournament()
        {
            ExecuteFunctionRequest req = null;
            string tournamentId = null;
            if (!PlayerDataManager.Singleton.isBot)
            {
                loadingScreen?.SetActive(true);
                if (tournamentName.text.Length < 3)
                {
                    loadingScreen?.SetActive(false);
                    ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup("Tournament name must be at least 3 characters long");
                    return;
                }
                if (!calendarController.CalendarHasDate())
                {
                    loadingScreen?.SetActive(false);
                    ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup("Select an init date for the tournament.");
                    return;
                }

                tournamentId = tournamentName.text + "_" + GenerateTournamentId();
                req = new ExecuteFunctionRequest()
                {
                    FunctionName = "BracketsTournament_CreateTournament",
                    FunctionParameter = new
                    {
                        Keys = new
                        {
                            tournamentId,
                            tournamentName = tournamentName.text,
                            initTime = calendarController.CreateDateForTournament(),
                            maxPlayer = int.Parse(maxPlayerInput.options[maxPlayerInput.value].text)
                        }
                    },
                };
            }
            else
            {
                (string, ExecuteFunctionRequest) result = FillBotData();

                tournamentId = result.Item1;
                req = result.Item2;
            }


            PlayFabCloudScriptAPI.ExecuteFunction(req,
            (res) =>
            {
                SacredTailsLog.LogMessage($"TOURNAMENT CREATED: {tournamentId}");
                onTournamentCreation?.Invoke(tournamentId);
                loadingScreen?.SetActive(false);
                GUIUtility.systemCopyBuffer = tournamentId;
                ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup($"Tournament created successfully!\n Id: {tournamentId} \n Copied to clipboard.");
            },
            (err) =>
            {
                loadingScreen?.SetActive(false);
                ServiceLocator.Instance.GetService<PopupManager>().ShowInfoPopup("Error creating tournament, please try again later.");
                SacredTailsLog.LogMessage("ERROR CREATING TOURNAMENT: " + JsonConvert.SerializeObject(err, Formatting.Indented));
            });
        }
        public (string, ExecuteFunctionRequest) FillBotData()
        {
            var tournamentId = "TestBot" + "_" + GenerateTournamentId();

            //Init time
            DateTime now = DateTime.UtcNow;
            DateTime modifiedTime = now.AddMinutes(PlayerDataManager.Singleton.numberOfBots / 1.2f);
            string initTimeDate = modifiedTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            //Request
            var req = new ExecuteFunctionRequest()
            {
                FunctionName = "BracketsTournament_CreateTournament",
                FunctionParameter = new
                {
                    Keys = new
                    {
                        tournamentId,
                        tournamentName = "TestBot",
                        initTime = initTimeDate,
                        maxPlayer = 32
                    }
                },
            };

            return (tournamentId, req);
        }

        public string GenerateTournamentId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}