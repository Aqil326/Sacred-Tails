using System;
using UnityEngine;
using Timba.SacredTails.Arena;
using PlayFab.MultiplayerModels;
using System.Collections;
using CoreRequestManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class BotCombatModule : MonoBehaviour
{
    [SerializeField] CombatBotDataSO testData;
    public void CleanPreviousMatch(string matchId, CombatBotDataSO botData, Action callback)
    {
        string selectShinseiString = "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"{x}\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\",\r\n      \"ShinseiIdList\": [0,1,2],\r\n      \"PlayerMatchData\": \"{\\r\\n  \\\"DisplayName\\\": \\\"{y}\\\",\\r\\n  \\\"shinseisSelected\\\": true,\\r\\n  \\\"hasSurrender\\\": false,\\r\\n  \\\"confirmState\\\": true,\\r\\n  \\\"isAnBotBattle\\\": true,\\r\\n  \\\"overrideShinseiData\\\":{z},\\r\\n  \\\"ShinseiParty\\\": [\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10100010011001000002100200000010080010031008003004008\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        28,\\r\\n        28,\\r\\n        28,\\r\\n        28\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 9,\\r\\n      \\\"shinseiRarity\\\": 2,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 46,\\r\\n        \\\"Attack\\\": 34,\\r\\n        \\\"Defence\\\": 30,\\r\\n        \\\"Luck\\\": 34,\\r\\n        \\\"Speed\\\": 43,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    },\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10100010011001000002100200000010080010031008003004008\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        29,\\r\\n        38,\\r\\n        15,\\r\\n        0\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 2,\\r\\n      \\\"shinseiRarity\\\": 2,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 34,\\r\\n        \\\"Attack\\\": 43,\\r\\n        \\\"Defence\\\": 38,\\r\\n        \\\"Luck\\\": 38,\\r\\n        \\\"Speed\\\": 34,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    },\\r\\n    {\\r\\n      \\\"shinseiName\\\": \\\"\\\",\\r\\n      \\\"ShinseiDna\\\": \\\"10100010011001000002100200000010080010031008003004008\\\",\\r\\n      \\\"generation\\\": \\\"\\\",\\r\\n      \\\"ShinseiActionsIndex\\\": [\\r\\n        7,\\r\\n        37,\\r\\n        19,\\r\\n        14\\r\\n      ],\\r\\n      \\\"shinseiType\\\": 8,\\r\\n      \\\"shinseiRarity\\\": 1,\\r\\n      \\\"ShinseiOriginalStats\\\": {\\r\\n        \\\"Health\\\": 34,\\r\\n        \\\"Attack\\\": 34,\\r\\n        \\\"Defence\\\": 30,\\r\\n        \\\"Luck\\\": 43,\\r\\n        \\\"Speed\\\": 30,\\r\\n        \\\"Energy\\\": 30\\r\\n      }\\r\\n    }\\r\\n  ]\\r\\n}\"\r\n    }\r\n  }\r\n}";
        //string selectShinseiString = "{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"{x}\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\",\r\n      \"ShinseiIdList\": [0,1,2],\r\n      \"PlayerMatchData\": \"{\\r\\n  \\\"DisplayName\\\": \\\"{y}\\\",\\r\\n  \\\"shinseisSelected\\\": true,\\r\\n  \\\"hasSurrender\\\": false,\\r\\n  \\\"confirmState\\\": true,\\r\\n  \\\"isAnBotBattle\\\": true,\\r\\n  \\\"ShinseiParty\\\": \"{z}\" \\r\\n}\"\r\n    }\r\n  }\r\n}"; 
        selectShinseiString = selectShinseiString.Replace("{x}", botData.botPlayfabId);
        selectShinseiString = selectShinseiString.Replace("{y}", botData.botDisplayName);
        PlayerPrefs.SetString("EnemyName", botData.botDisplayName);
        PlayerPrefs.SetString("PlayerName", PlayerDataManager.Singleton.localPlayerData.playerName);
        string daticos = JsonConvert.SerializeObject(botData.shinseis.GetRange(0,3));
        daticos = daticos.Replace("\"", "\\\"");
        selectShinseiString = selectShinseiString.Replace("{z}", daticos);

        StartCoroutine(MakeEndpointCall(
    "https://sacredtailsserver.azurewebsites.net/api/BattleServer/DeleteMatch",
    string.Format(
        @"{{
            ""CallerEntityProfile"": {{
                ""Lineage"": {{
                    ""MasterPlayerAccountId"": ""{0}""
                }}
            }},
            ""FunctionArgument"": {{
                ""Keys"": {{
                    ""MatchId"": ""{1}""
                }}
            }}
        }}",
        botData.botPlayfabId, matchId),
    () =>
    {
        StartCoroutine(MakeEndpointCall(
            "https://sacredtailsserver.azurewebsites.net/api/BattleServer/CreateMatch",
            string.Format(
                @"{{
                    ""CallerEntityProfile"": {{
                        ""Lineage"": {{
                            ""MasterPlayerAccountId"": ""{0}""
                        }}
                    }},
                    ""FunctionArgument"": {{
                        ""Keys"": {{
                            ""MatchId"": ""{1}""
                        }}
                    }}
                }}",
                botData.botPlayfabId, matchId),
            () =>
            {
                StartCoroutine(MakeEndpointCall(
                    "https://sacredtailsserver.azurewebsites.net/api/BattleServer/SelectShinseis",
                    selectShinseiString,
                    () =>
                    {
                        callback?.Invoke();
                    }
                ));
            }
        ));
    }
));
    }

    IEnumerator MakeEndpointCall(string _url, string data, Action callback)
    {
        if(_url == "https://sacredtailsserver.azurewebsites.net/api/BattleServer/SelectShinseis")
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

    public void BotMatchInit(CombatBotDataSO botData)
    {
        string matchId = "TestMatch_" + PlayerDataManager.Singleton.localPlayerData.playfabId;
        CleanPreviousMatch(matchId, botData,() =>
        {
            PlayerPrefs.SetFloat("MatchSelectTime", -1);
            PlayerPrefs.SetString("GetBotName", botData.botDisplayName);
            PlayfabManager.Singleton.BattleServerCreateMatch(
                matchId,
                (result) =>
                {
                    SacredTailsPSDto<object> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());
                    if (!dto.success)
                        return;
                    Debug.Log("Test match dto: " + dto.data.ToString());

                    FindObjectOfType<GameSceneManager>().SendBattle(new GetMatchResult()
                    {
                        MatchId = matchId,
                        Members = new List<MatchmakingPlayerWithTeamAssignment>() {
                        new MatchmakingPlayerWithTeamAssignment() {
                            Attributes = new MatchmakingPlayerAttributes(){ DataObject =  JsonUtility.ToJson(new CustomAtributes (){PlayerPlayfabId= botData.botPlayfabId}) }
                        },
                        new MatchmakingPlayerWithTeamAssignment() {
                            Attributes = new MatchmakingPlayerAttributes(){ DataObject =  JsonUtility.ToJson(new CustomAtributes (){PlayerPlayfabId= PlayerDataManager.Singleton.localPlayerData.playfabId, displayName= PlayerDataManager.Singleton.localPlayerData.playerName} )}
                        },
                        }
                    });
                });
        });
    }

    [ContextMenu("Try match")]
    public void TestMatch()
    {
        BotMatchInit(testData);
    }

    public void StartNPCCombat(int npcIndex = 0)
    {
        PlayerPrefs.SetInt("combatNPC",npcIndex);
        BotMatchInit(PlayerDataManager.Singleton.CombatNPCs[npcIndex]);
    }
}
