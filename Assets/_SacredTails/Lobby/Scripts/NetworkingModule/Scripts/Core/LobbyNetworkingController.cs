using CoreRequestManager;
using DG.Tweening;
using FirebaseRequestManager;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Games.SacredTails.PopupModule;
using Timba.SacredTails.Lobby;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using static Timba.Games.SacredTails.PopupModule.PopupManager;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.CharacterStyle;
using TMPro;

namespace Timba.Games.SacredTails.LobbyNetworking
{
    public interface ILobbyNetworkManager : IService
    {
        public ThirdPersonController CurrentPlayer { get; set; }
        public float CurrentSecondsBetweenPetition { get; }
        public void ConnectToLobby(int lobbyId);
        public void ManageLobbyPlayers(Dictionary<string, LobbyPlayerBasePayload> usersData);
        public void ToggleBattleMode(bool state);
        public void TickCheckActivates();
        public Action<ThirdPersonController> OnConnected { get; set; }
        public bool CheckIfPlayerHasChallengeOrIsChallenging(string playfabId);
        public bool CheckIfOtherPlayerHasChallengeForPlayfabId(string playfabId);
        public void ShowPlayerPersonalUI(bool show = true);
        public CharacterStateEnum GetPlayerState(string playerId);
    }

    public class LobbyNetworkingController : MonoBehaviour, ILobbyNetworkManager
    {
        #region ----Fields----
        public PlayerDataManager playerDataManager;
        public GameObject loadingScreen;


        public ThirdPersonController localPlayerPrefab;
        public ThirdPersonController otherPlayerPrefab;
        public Dictionary<string, ThirdPersonController> currentPlayersAvatar = new Dictionary<string, ThirdPersonController>();
        public ThirdPersonController currentPlayer;
        public ThirdPersonController CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }

        public Dictionary<string, LobbyPlayerBasePayload> currentPlayersData = new Dictionary<string, LobbyPlayerBasePayload>();
        public Dictionary<string, bool> playersInstantiated = new Dictionary<string, bool>();
        public ILobbydatabase lobbyDatabase;
        public ChatTextBox chatTextBox;

        public bool connected = false;
        public GameObject otherPlayersContainer;
        public TMP_Text playersCountText;
        public bool onBattle = false;

        public float minErrorPosition = 0.5f;
        public float ratePetiton = .5f;
        public float CurrentSecondsBetweenPetition { get => ratePetiton; }
        public float lerpDuration = .1f;
        private string playfabEncrypteId;
        private string encryptIV;

        private Action<ThirdPersonController> onConnected;
        public Action<ThirdPersonController> OnConnected { get => onConnected; set => onConnected = value; }
        public UnityEvent<ThirdPersonController> OnConnectedCallback;
        public ulong connectionId;

        private bool shouldMakePetitions = true;
        #endregion ----Fields----

        #region ----Methods----
        #region Connect to lobby
        public void Start()
        {
            playerDataManager.playerDataReady += () => ConnectToLobby(0);
            shouldMakePetitions = true;
        }

        public void ConnectToLobby(int lobbyId)
        {
            lobbyDatabase = ServiceLocator.Instance.GetService<ILobbydatabase>();

            this.playfabEncrypteId = AESEncryption.Encrypt(playerDataManager.localPlayerData.playfabId);
            this.encryptIV = AESEncryption.IV;
            if (shouldMakePetitions)
                StartCoroutine(TryConnection(lobbyId));
        }

        IEnumerator WaitForSecondsCallback(int seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        IEnumerator TryConnection(int lobbyId)
        {
            while (String.IsNullOrEmpty(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna))
                yield return new WaitForSeconds(0.1f);

            LobbyPlayerBasePayload userData = new LobbyPlayerBasePayload()
            {
                displayName = playerDataManager.localPlayerData.playerName,
                playerPosition = Vector3Extensions.FromVector3(Vector3.zero),
                shinseiCompanionDna = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna,
                characterStyle = PlayerDataManager.Singleton.localPlayerData.CastDictionaryToCompressedStyle(),
                characterState = (int)PlayerDataManager.Singleton.localPlayerData.characterState,
                currentMatchId = PlayerDataManager.Singleton.localPlayerData.currentMatchId,
                challengedPlayer = PlayerDataManager.Singleton.localPlayerData.challengedPlayer,
            };

            lobbyDatabase.ConnectToLobby(lobbyId, userData, (result) =>
             {
                 SacredTailsPSDto<ConnectionResponse> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<ConnectionResponse>>(result.FunctionResult.ToString());
                 if (dto.code == 739)
                 {
                     Dictionary<ButtonType, Action> buttons = new Dictionary<ButtonType, Action>();
                     buttons.Add(ButtonType.CONFIRM_BUTTON, () => Application.Quit());
                     ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"Ups it is time too upgrade the version of your game. \nCurrent version is: <color=red>{Application.version}</color>. \n Please update to the version: <color=green>{dto.data.currentVersion}</color>, \n This application will close soon", buttons);
                     StartCoroutine(WaitForSecondsCallback(20, () => Application.Quit()));
                     return;
                 }

                 if (!dto.success)
                     return;
                 connectionId = dto.data.connectionId;

                 userData.lobby = dto.data.lobbyId;
                 currentPlayersData.Add(playerDataManager.localPlayerData.playfabId, userData);
                 CurrentPlayer = Instantiate(localPlayerPrefab, otherPlayersContainer.transform);
                 CurrentPlayer.transform.position += new Vector3(0, 0, 0);
                 CurrentPlayer.IsLocalPlayer = true;
                 CurrentPlayer.GetComponent<PlayerUI>().OnSpawn(playerDataManager.localPlayerData.playerName);
                 CurrentPlayer.GetComponent<ShinseiSpawner>().OnSpawn(otherPlayersContainer.transform, true, true);
                 currentPlayersAvatar.Add(playerDataManager.localPlayerData.playfabId, CurrentPlayer);

                 loadingScreen.SetActive(false);
                 connected = true;
                 OnConnected?.Invoke(currentPlayer);
                 OnConnectedCallback.Invoke(CurrentPlayer);
                 chatTextBox.isReady = true;


                 StartCoroutine(OnTick());
             });
        }
        #endregion Connect to lobby

        #region OnTick
        IEnumerator OnTick()
        {
            while (connected && shouldMakePetitions)
            {
                if (!onBattle && shouldMakePetitions)
                    TickCheck();

                yield return new WaitForSeconds(ratePetiton);
            }
        }

        public void TickCheckActivates()
        {
            if (!connected)
                return;
            shouldMakePetitions = true;
            StopAllCoroutines();
            StartCoroutine(OnTick());
        }

        public class MessageObject
        {
            public List<DirectMessage> messages = new List<DirectMessage>();
        }
        public class DirectMessage
        {
            public string from;
            public string msg;
        }

        public void TickCheck()
        {
            if (lobbyDatabase == null)
                return;
            LobbyPlayerBasePayload userData = new LobbyPlayerBasePayload()
            {
                connectionId = connectionId,
                playfabIdEncrypted = playfabEncrypteId,
                encryptIV = AESEncryption.IV,
                displayName = playerDataManager.localPlayerData.playerName,
                playerPosition = Vector3Extensions.FromVector3(currentPlayersAvatar[playerDataManager.localPlayerData.playfabId].transform.position),
                chatMessages = PlayerDataManager.Singleton.localPlayerData.currentChatMessages,
                shinseiCompanionDna = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna,
                characterStyle = PlayerDataManager.Singleton.localPlayerData.CastDictionaryToCompressedStyle(),
                characterState = (int)PlayerDataManager.Singleton.localPlayerData.characterState,
                currentMatchId = PlayerDataManager.Singleton.localPlayerData.currentMatchId,
                challengedPlayer = PlayerDataManager.Singleton.localPlayerData.challengedPlayer
            };

            lobbyDatabase.SetGetPlayersData(userData, (result) =>
            {
                var dto = JsonConvert.DeserializeObject<SacredTailsPSDto<dynamic>>(result.FunctionResult.ToString());

                if (!dto.success)
                {
                    //New connection
                    if (dto.code == 0)
                    {
                        shouldMakePetitions = false;
                        Dictionary<ButtonType, Action> buttons = new Dictionary<ButtonType, Action>();
                        buttons.Add(ButtonType.CONFIRM_BUTTON, () => Application.Quit());
                        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You are connected again in other instance. Closing game ...", buttons);
                        StartCoroutine(WaitForSecondsCallback(4, () => Application.Quit()));
                    }
                    return;
                }
                var data = dto.data;
                var playersData = JsonConvert.DeserializeObject<Dictionary<string, PlayersStreamData>>(data.ToString());

                Dictionary<string, LobbyPlayerBasePayload> lobbyPlayers = new Dictionary<string, LobbyPlayerBasePayload>();
                foreach (KeyValuePair<string, PlayersStreamData> kvp in playersData)
                    lobbyPlayers.Add(kvp.Key, JsonConvert.DeserializeObject<LobbyPlayerBasePayload>(kvp.Value.Value.ToString()));
                ManageLobbyPlayers(lobbyPlayers);
                PlayerDataManager.Singleton.localPlayerData.currentChatMessages = new List<ChatMessagePayload>();
            });
            if (userData == null)
                return;

            if (!CheckIfIsMoving(currentPlayersData[playerDataManager.localPlayerData.playfabId], userData))
            {
                timer++;
                if (timer >= 900)
                    Application.Quit();
                else if (timer >= 880)
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You are disconnecting in 1 minute");
                else if (timer >= 800)
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("You are disconnecting in 5 minutes");
            }

            //Check and write directMessages
            PlayfabManager.Singleton.GetUserData(playerDataManager.localPlayerData.playfabId, new List<string> { "DirectMessages" }, (a) =>
            {
                if (!a.Data.ContainsKey("DirectMessages"))
                    return;
                MessageObject MessageData = JsonConvert.DeserializeObject<MessageObject>(a.Data["DirectMessages"].Value);
                if (MessageData != null)
                {
                    if (MessageData.messages.Count > 0)
                    {
                        foreach (DirectMessage chatMessage in MessageData.messages)
                        {
                            chatTextBox.SendMessage(new ChatMessagePayload() { message = chatMessage.msg, id = "0", timeStamp = Time.time.ToString() }, chatMessage.from);
                        }
                        PlayfabManager.Singleton.SetUserData(new Dictionary<string, string>() { { "DirectMessages", "{\"messages\":[]}" } });
                    }
                }
            });
        }
        public float timer = 0;

        public bool CheckIfIsMoving(LobbyPlayerBasePayload currentData, LobbyPlayerBasePayload dataToSend)
        {
            if (currentData.playerPosition != dataToSend.playerPosition)
                if (Vector3.Distance(Vector3Extensions.ToVector3(currentData.playerPosition), Vector3Extensions.ToVector3(dataToSend.playerPosition)) > minErrorPosition)
                    return true;
            return false;
        }

        public void ManageLobbyPlayers(Dictionary<string, LobbyPlayerBasePayload> usersData)
        {
            List<string> playersToDeleteKeys = new List<string>();
            foreach (var item in currentPlayersData)
                if (!usersData.ContainsKey(item.Key))
                    playersToDeleteKeys.Add(item.Key);

            foreach (var playerToDelete in playersToDeleteKeys)
            {
                currentPlayersData.Remove(playerToDelete);

                ThirdPersonController currentPlayerAvatar = currentPlayersAvatar[playerToDelete];
                currentPlayersAvatar.Remove(playerToDelete);

                Destroy(currentPlayerAvatar.gameObject);
                Destroy(currentPlayerAvatar.GetComponent<ShinseiSpawner>().characterSlot.gameObject);
            }

            foreach (var item in usersData)
            {
                if (item.Key != playerDataManager.localPlayerData.playfabId && !currentPlayersData.ContainsKey(item.Key))
                    NewPlayerConnected(item);
                else
                {

                    if (item.Key == playerDataManager.localPlayerData.playfabId)
                        continue;

                    if (currentPlayersData[item.Key].playerPosition != item.Value.playerPosition)
                        currentPlayersData[item.Key].playerPosition = item.Value.playerPosition;

                    if (currentPlayersData[item.Key].shinseiCompanionDna != item.Value.shinseiCompanionDna)
                    {
                        currentPlayersData[item.Key].shinseiCompanionDna = item.Value.shinseiCompanionDna;
                        currentPlayersAvatar[item.Key].GetComponent<ShinseiSpawner>().ChangeCurrentShinsei(item.Value.shinseiCompanionDna);
                    }

                    if (!currentPlayersData[item.Key].characterStyle.Equals((item.Value.characterStyle)))
                    {
                        currentPlayersData[item.Key].characterStyle = item.Value.characterStyle;
                        PaintOtherPlayer(currentPlayersAvatar[item.Key].GetComponent<CharacterRecolor>(),
                                         PlayerDataManager.Singleton.localPlayerData.CastCompressedStyleToDictionary(item.Value.characterStyle));
                    }

                    if (currentPlayersData[item.Key].characterState != item.Value.characterState)
                    {
                        currentPlayersData[item.Key].characterState = item.Value.characterState;
                        currentPlayersAvatar[item.Key].SetStateIcon((CharacterStateEnum)currentPlayersData[item.Key].characterState);
                    }

                    if (currentPlayersData[item.Key].currentMatchId != item.Value.currentMatchId)
                    {
                        currentPlayersAvatar[item.Key].currentMatchId = item.Value.currentMatchId;
                        currentPlayersData[item.Key].currentMatchId = item.Value.currentMatchId;
                    }


                    List<ChatMessagePayload> chatMesssagesSorted = item.Value.chatMessages?.OrderBy(o => o.timeStamp).ToList();
                    if (chatMesssagesSorted != null)
                        foreach (var chatMessage in chatMesssagesSorted)
                            chatTextBox.SendMessage(chatMessage, item.Value.displayName, item.Key.Equals("54BB079356042E83"));

                    currentPlayersAvatar[item.Key].MoveObject(Vector3Extensions.ToVector3(currentPlayersData[item.Key].playerPosition));
                }
            }

            foreach (var item in usersData)
                PlayerChallengeVerification(item);

            //Update players count on lobby
            playersCountText.text = currentPlayersData.Count.ToString();
        }

        public void NewPlayerConnected(KeyValuePair<string, LobbyPlayerBasePayload> item)
        {
            currentPlayersData.Add(item.Key, item.Value);

            ThirdPersonController otherPlayer = Instantiate(otherPlayerPrefab, Vector3Extensions.ToVector3(item.Value.playerPosition), Quaternion.identity);
            otherPlayer.playfabId = item.Key;
            otherPlayer.displayName = item.Value.displayName;
            otherPlayer.currentMatchId = item.Value.currentMatchId;
            otherPlayer.transform.SetParent(otherPlayersContainer.transform);
            currentPlayersAvatar.Add(item.Key, otherPlayer);
            PaintOtherPlayer(currentPlayersAvatar[item.Key].GetComponent<CharacterRecolor>(),
                             PlayerDataManager.Singleton.localPlayerData.CastCompressedStyleToDictionary(item.Value.characterStyle));

            otherPlayer.GetComponent<PlayerUI>().OnSpawn(item.Value.displayName);
            otherPlayer.GetComponent<ShinseiSpawner>().SpawnOtherShinsei(item.Value.shinseiCompanionDna, otherPlayersContainer.transform, otherPlayer.transform.position);
        }

        public CharacterStateEnum GetPlayerState(string playerId)
        {
            return (CharacterStateEnum)currentPlayersData[playerId].characterState;
        }

        public void PlayerChallengeVerification(KeyValuePair<string, LobbyPlayerBasePayload> item)
        {
            bool isMatchSameAsLastOne = currentPlayersData[item.Key].challengedPlayer.Equals(item.Value.challengedPlayer);
            currentPlayersData[item.Key].challengedPlayer = item.Value.challengedPlayer;

            if (!String.IsNullOrEmpty(item.Value.challengedPlayer))
            {
                string[] challengeData = item.Value.challengedPlayer.Split('_');
                string playfabIdChallengedPlayer = challengeData[0];

                // Verifications
                bool isCurrentPlayer = item.Key == PlayerDataManager.Singleton.localPlayerData.playfabId;

                bool challengedPlayerCanceledMatch = currentPlayersData.ContainsKey(playfabIdChallengedPlayer) && currentPlayersData[playfabIdChallengedPlayer].challengedPlayer.Equals("CANCEL");
                bool canceledAlreadyProcessed = item.Value.challengedPlayer.Contains("CANCEL") && (!currentPlayersData.Any(playerData => playerData.Value.challengedPlayer.Contains(item.Key)) || !isMatchSameAsLastOne);
                bool currentPlayerHasChallenge = challengeData.Length > 1 && playfabIdChallengedPlayer.Equals(PlayerDataManager.Singleton.localPlayerData.playfabId);

                // Verification logic
                if (isMatchSameAsLastOne && challengedPlayerCanceledMatch)
                {
                    if (isCurrentPlayer)
                    {
                        // "ChallengeId" -> ""
                        PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
                        TickCheckActivates();
                    }
                    currentPlayersData[item.Key].challengedPlayer = "";
                    currentPlayersAvatar[item.Key].challengePlayerController.StopAllCoroutines();
                    currentPlayersAvatar[item.Key].challengePlayerController.MatchCanceledByChallenged(item.Key == playerDataManager.localPlayerData.playfabId);

                    currentPlayersData[playfabIdChallengedPlayer].challengedPlayer = "";
                    currentPlayersAvatar[playfabIdChallengedPlayer].challengePlayerController.StopAllCoroutines();
                    currentPlayersAvatar[playfabIdChallengedPlayer].challengePlayerController.MatchCanceledByChallenged(item.Key == playerDataManager.localPlayerData.playfabId);
                }
                else if (isCurrentPlayer && canceledAlreadyProcessed)
                {
                    // "CANCELED" -> ""
                    PlayerDataManager.Singleton.localPlayerData.challengedPlayer = "";
                    TickCheckActivates();
                }
                else if (currentPlayerHasChallenge)
                {
                    string randomMatchNumber = challengeData[1];
                    currentPlayer.challengePlayerController.RecieveChallenge(currentPlayersAvatar[item.Key], randomMatchNumber);
                }
            }
        }

        public bool CheckIfOtherPlayerHasChallengeForPlayfabId(string playfabId)
        {
            foreach (var playerData in currentPlayersData)
                if (playerData.Value.challengedPlayer.Contains(playfabId))
                    return true;
            return false;
        }

        public bool CheckIfPlayerHasChallengeOrIsChallenging(string playfabId)
        {
            TickCheckActivates();
            if (!String.IsNullOrEmpty(currentPlayersData[playfabId].challengedPlayer))
                return true;
            if (CheckIfOtherPlayerHasChallengeForPlayfabId(playfabId))
                return true;
            return false;
        }

        public void PaintOtherPlayer(CharacterRecolor playerRecolor, Dictionary<PartsOfCharacter, CharacterStyleInfo> characterStyle)
        {
            foreach (var partStyle in characterStyle)
            {
                Debug.Log("PERFIL: " + partStyle.Key);
                Color color;
                ColorUtility.TryParseHtmlString("#" + partStyle.Value.colorHex, out color);
                playerRecolor.ChangeMaterialColors(partStyle.Key, color); //ENABLE
                //Add specific hair depending of style
                if (partStyle.Key == PartsOfCharacter.HAIR)
                {
                    BodyStyle bodyStyle = playerRecolor.transform.GetComponentInChildren<BodyStyle>();
                    if (bodyStyle != null)
                    {
                        bodyStyle.bodyParts[0].SelectObject(partStyle.Value.presetId);
                        foreach (var materialReskin in playerRecolor.transform.GetComponentsInChildren<MaterialReskin>())
                            materialReskin.ChangePart(partStyle.Value.presetId, 4);
                    }
                }
                if (partStyle.Key == PartsOfCharacter.SKIN)
                {
                    if (partStyle.Value.presetId == 1)
                    {
                        playerRecolor.transform.GetChild(1).gameObject.SetActive(true);
                        playerRecolor.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        playerRecolor.transform.GetChild(2).gameObject.SetActive(true);
                        playerRecolor.transform.GetChild(1).gameObject.SetActive(false);

                    }
                }
            }
        }
        #endregion OnTick

        #region DisconnectFromLobby
        public void OnApplicationQuit()
        {
            if (currentPlayersData.ContainsKey(playerDataManager.localPlayerData.playfabId.ToString()))
            {
                var thisPlayer = currentPlayersData[playerDataManager.localPlayerData.playfabId.ToString()];
                lobbyDatabase.DisconnectFromLobby(thisPlayer);
            }
            connected = false;
            SacredTailsLog.LogMessage("Discconect succesfully");
        }

        bool onChatMode = false;
        #endregion DisconnectFromLobby

        #region Helpers
        public void ShowPlayerPersonalUI(bool show = true)
        {
            foreach (KeyValuePair<string, ThirdPersonController> playerAvatar in currentPlayersAvatar)
                playerAvatar.Value.playerPersonalUI.SetActive(show);
        }

        public void ToogleChatMode()
        {
            if (CurrentPlayer)
                CurrentPlayer.IsChatMode = true;
        }

        public void UntoogleChatMode()
        {
            if (CurrentPlayer)
                CurrentPlayer.IsChatMode = false;
        }

        public void ToggleBattleMode(bool state)
        {
            if (CurrentPlayer != null)
                CurrentPlayer.IsMovementBloqued = state;

            onBattle = state;
            otherPlayersContainer?.SetActive(!state);
        }

        public string GeneratePlayfabIdHash(string text)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(text);
            StringBuilder sb = new StringBuilder();

            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] hash = sha256.ComputeHash(stringBytes);
                foreach (Byte b in hash)
                    sb.Append(b.ToString("X2"));
            }

            return sb.ToString();

        }

        public bool IsReady()
        {
            throw new NotImplementedException();
        }
        #endregion Helpers
        #endregion ----Methods----
    }

    [Serializable]
    public class ConnectionResponse
    {
        public ulong connectionId;
        public int lobbyId;
        public string currentVersion;
    }

    [Serializable]
    public class PlayersStreamData
    {
        public string Value;
        public string LastUpdated;
        public string Permission;
    }
}


