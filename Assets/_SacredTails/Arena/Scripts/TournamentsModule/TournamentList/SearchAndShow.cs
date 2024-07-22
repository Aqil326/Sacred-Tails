using Newtonsoft.Json;
using PlayFab;
using PlayFab.CloudScriptModels;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Timba.Games.SacredTails.LobbyDatabase;
using CoreRequestManager;
using TMPro;
using System;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.TournamentBehavior
{
    /// <summary>
    ///     Show all active tournaments registered in the game
    /// </summary>
    public class SearchAndShow : MonoBehaviour
    {
        public List<TournamentSlot> tournamentSlots = new List<TournamentSlot>();
        [SerializeField] private GameObject tournamentSlotPrefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TextMeshProUGUI tournamentName;

        List<TournamentEntry> tournamentEntries = new List<TournamentEntry>();

        // Update is called once per frame

        void OnEnable()
        {
            if (PlayerDataManager.Singleton.isBot)
                return;

            var thirdPerson = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
            thirdPerson.IsMovementBloqued = true;
            thirdPerson.CanBeBlocked = false;
            SearchAndShowTournaments();
        }

        private void OnDisable()
        {
            if (PlayerDataManager.Singleton.isBot)
                return;

            var thirdPerson = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
            thirdPerson.CanBeBlocked = true;
            thirdPerson.IsMovementBloqued = false;
        }

        public void SearchAndShowTournaments()
        {
            foreach (var tournamentSlot in tournamentSlots)
                tournamentSlot.gameObject.SetActive(false);
            GetUserDataRequest request = new GetUserDataRequest()
            {
                PlayFabId = "7F1965D480D991B5",
            };
            ServiceLocator.Instance.GetService<ITournamentDatabase>().TournamentRequest("", TypeOfTournamentRequest.GET_TOURNAMENT_LIST, (succesData) =>
            {
                tournamentEntries.Clear();
                List<TournamentEntry> succesDataJson = JsonConvert.DeserializeObject<SacredTailsPSDto<List<TournamentEntry>>>(succesData.FunctionResult.ToString()).data;
                foreach (var tournament in succesDataJson)
                    tournamentEntries.Add(tournament);

                foreach (var entries in tournamentSlots)
                    entries.gameObject.SetActive(false);

                int counter = 0;
                for (int i = 0; i < tournamentEntries.Count; i++)
                {
                    try
                    {

                        DateTime tournamentDate = DateTime.Parse(tournamentEntries[i].initTimeStage_1).ToUniversalTime();

                        var nowTime = DateTime.UtcNow;
                        var substraction = tournamentDate.Subtract(nowTime).TotalSeconds;
                        if (substraction > 0)
                        {
                            if (tournamentSlots.Count <= counter + 1)
                            {
                                TournamentSlot temporal = Instantiate(tournamentSlotPrefab, parent).GetComponent<TournamentSlot>();
                                temporal.OnClick += (Slot, Name) =>
                                {
                                    foreach (var item in tournamentSlots)
                                        item.ShowUnselected();
                                    Slot.ShowSelected();
                                    tournamentName.text = Name;
                                };
                                tournamentSlots.Add(temporal);
                            }
                            tournamentSlots[counter].DrawEntry(tournamentEntries[i]);
                            tournamentSlots[counter].gameObject.SetActive(true);
                            counter++;
                        }
                    }
                    catch(Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            });
        }


        /*{
      "success": true,
      "code": 0,
      "message": "here is all tournaments",
      "data": {
        "NewTournamentBoi_2022": {
          "Value": "{\"initTimeStage_0\":\"20/20/20\",\"tournamentName\":\"New tournament boi\",\"maxPlayer\":60}",
          "LastUpdated": "2022-11-28T15:53:42.824Z",
          "Permission": "Private"
        }
      }
    }*/
    }
    public class TournamentEntry
    {
        public string tournamentId;
        public string tournamentName;
        public string initTimeStage_1;
        public string maxPlayer;
        public string playersJoined;
    }
}