using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using PlayFab.MultiplayerModels;
using Timba.Games.SacredTails;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using System;
using UnityEngine.Events;

namespace Timba.SacredTails.Arena
{
    public class GameSceneManager : NetworkBehaviour
    {
        ILobbyNetworkManager lobbyNetworkController;

        [SerializeField] GameObject combatPlayerPrefab;
        [SerializeField] GameObject lobbyPlayerPrefab;

        [SerializeField] GameObject lobbyUI;

        [SerializeField] GameObject combatInstance;
        [SerializeField] List<Transform> spawnPoints;

        [SerializeField] Button characterStyleControllerBtn;
        [SerializeField] Transform parent;

        public UnityEvent onEndBattle;

        private void Start()
        {
            lobbyNetworkController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>();
        }

        public int GetClosestBattleSpawn()
        {
            int closestSpawnPoint = -1;
            float closestMagnitude = float.MaxValue;
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                float sqrMagnitudCurrent = (spawnPoints[i].position - lobbyNetworkController.CurrentPlayer.transform.position).sqrMagnitude;
                if (sqrMagnitudCurrent < closestMagnitude)
                {
                    closestMagnitude = sqrMagnitudCurrent;
                    closestSpawnPoint = i;
                }
            }
            return closestSpawnPoint;
        }

        public void SendBattle(GetMatchResult getMatchResult = null, string matchId = null, int _closestSpawnPoint = -1, bool isViewing = false, bool isBotFight = false)
        {
            lobbyNetworkController.TickCheckActivates();
            lobbyUI.SetActive(false);

            int closestSpawnPoint = _closestSpawnPoint;
            if (closestSpawnPoint == -1)
                closestSpawnPoint = GetClosestBattleSpawn();

            combatInstance = Instantiate(combatPlayerPrefab, spawnPoints[closestSpawnPoint].position, spawnPoints[closestSpawnPoint].rotation);
            ServiceLocator.Instance.GetService<IWallet>().HideUserWallet();

            BattleGameMode battleInstance = combatInstance.GetComponent<BattleGameMode>();

            if (getMatchResult != null)
            {
                PlayerDataManager.Singleton.localPlayerData.currentMatchId = getMatchResult.MatchId;
                battleInstance.OnStartMatch(getMatchResult);
            }
            else
            {
                PlayerDataManager.Singleton.localPlayerData.currentMatchId = matchId;
                battleInstance.isViewingMatch = isViewing;
                BattleViewerController battleViewer = combatInstance.GetComponent<BattleViewerController>();
                battleViewer.Initialize(matchId, isViewing);
            }
            PlayerDataManager.Singleton.localPlayerData.characterState = CharacterStateEnum.COMBAT;
            lobbyNetworkController.TickCheckActivates();
            lobbyNetworkController.ToggleBattleMode(true);

            battleInstance.gameSceneManager = this;
            if (parent.gameObject.activeSelf)
                characterStyleControllerBtn.onClick.Invoke();

        }

        public void EndBattle()
        {
            lobbyNetworkController.ToggleBattleMode(false);
            PlayerDataManager.Singleton.localPlayerData.characterState = Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.LOBBY;
            PlayerDataManager.Singleton.localPlayerData.currentMatchId = "";
            lobbyNetworkController.TickCheckActivates();
            ServiceLocator.Instance.GetService<IWallet>().UpdateUserWallet();
            if (combatInstance != null)
                Destroy(combatInstance);
            lobbyUI.SetActive(true);
            if (PlayerPrefs.GetString("LastLocation", "Town") == "Town")
                FindObjectOfType<RareThing>().PlaySound("Town");
            else
                FindObjectOfType<RareThing>().PlaySound("Bar");

            SacredTailsLog.LogMessageForBot($"EndMatchCallback");
            onEndBattle?.Invoke();
        }
    }
}