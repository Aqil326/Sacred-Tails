using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ServerModels;
using PlayFab;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Timba.SacredTails.Arena;

namespace Timba.SacredTails.BattleDebugTool
{
    /// <summary>
    /// Download to client all data from the server for debugging 
    /// </summary>
    public class GetShareGroupData : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] CombatPlayerServer ownCombat, enemyCombat;
        [SerializeField] List<ShinseiStats> originalStatsOwn, originalStatsEnemy;
        [Header("UI")]
        [SerializeField] TextMeshProUGUI strikesCounter0, strikesCounter1;
        [SerializeField] List<DebugShinseiSlot> playerDebugShinseiSlots = new List<DebugShinseiSlot>();
        [SerializeField] List<DebugShinseiSlot> enemyDebugShinseiSlots = new List<DebugShinseiSlot>();

        [SerializeField] private float timeBetweenUpdates = 4;
        private float currentTime;

        public string matchId;
        private void Start()
        {
            matchId = PlayerDataManager.Singleton.localPlayerData.currentMatchId;

        }
        private void Update()
        {
            if (matchId == "")
                return;
            if (currentTime - Time.time <= 0)
            {
                GetServerMatchData(matchId, PlayerDataManager.Singleton.localPlayerData.playfabId);
                currentTime = Time.time + timeBetweenUpdates;
            }
        }

        public void GetServerMatchData(string matchId, string ownPlayfabId)
        {
            var battleGameMode = FindObjectOfType<BattleGameMode>();
            var playerInfo = battleGameMode.playerInfo;
            var enemyInfo = battleGameMode.enemyInfo;

            GetSharedGroupDataRequest request = new GetSharedGroupDataRequest() { SharedGroupId = matchId };
            PlayFabServerAPI.GetSharedGroupData(request,
                (succesData) =>
                {
                    //Debug all keys in dictionary of succesdata
                    foreach (var key in succesData.Data.Keys)
                        Debug.Log("KEY IN DICTIONARY : "+key);
                    List<string> dataKeys = succesData.Data.Keys.Where(name => name.Contains("PlayerMatchData_")).ToList();
                    foreach (var key in dataKeys)
                    {                        if (key.Contains($"PlayerMatchData_{ownPlayfabId}"))
                            try
                            {
                                ownCombat = JsonConvert.DeserializeObject<CombatPlayerServer>(succesData.Data[key].Value);
                                ownCombat.ShinseiParty.ForEach(shinsei => originalStatsOwn.Add(shinsei.ShinseiOriginalStats));
                                strikesCounter0.text = $"Strikes: {ownCombat.strikes}";
                            }
                            catch (System.Exception)
                            {
                                ownCombat = new CombatPlayerServer();
                                SacredTailsLog.LogMessage("Any was wrong with YOUR data in battle, the battle is corrupted");
                            }
                        else
                            try
                            {
                                enemyCombat = JsonConvert.DeserializeObject<CombatPlayerServer>(succesData.Data[key].Value);
                                enemyCombat.ShinseiParty.ForEach(shinsei => originalStatsEnemy.Add(shinsei.ShinseiOriginalStats));
                                strikesCounter1.text = $"Strikes: {enemyCombat.strikes}";
                            }
                            catch (System.Exception)
                            {
                                enemyCombat = new CombatPlayerServer();
                                SacredTailsLog.LogMessage("Any was wrong with ENEMY data in battle, your battle is corrupted");
                            }
                    }
                    for (int i = 0; i < ownCombat.ShinseiParty.Count; i++)
                        playerDebugShinseiSlots[i].ShowValues(originalStatsOwn[i], ownCombat.ShinseiParty[i], playerInfo.battleShinseis[i]);
                    for (int i = 0; i < enemyCombat.ShinseiParty.Count; i++)
                        enemyDebugShinseiSlots[i].ShowValues(originalStatsEnemy[i], enemyCombat.ShinseiParty[i], enemyInfo.battleShinseis[i]);
                    playerDebugShinseiSlots[ownCombat.currentShinsei].selectedImage.gameObject.SetActive(true);
                    enemyDebugShinseiSlots[enemyCombat.currentShinsei].selectedImage.gameObject.SetActive(true);
                },
                (errorData) =>
                {
                    SacredTailsLog.LogMessage(JsonConvert.SerializeObject(errorData.ErrorDetails));
                });
        }

        [System.Serializable]
        public class CombatPlayerServer
        {
            public string playfabId;
            public string DisplayName;
            public bool shinseisSelected;
            public bool hasSurrender;
            public bool confirmState;
            public int strikes;
            public Dictionary<int, int> forbidenActions = new Dictionary<int, int>();
            public List<Shinsei> ShinseiParty = new List<Shinsei>();
            public int currentShinsei;
        }
    }
}