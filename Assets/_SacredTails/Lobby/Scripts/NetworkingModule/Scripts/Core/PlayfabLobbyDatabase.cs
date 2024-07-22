using System;
using UnityEngine;
using CoreRequestManager;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;
using PlayFab;
using PlayFab.CloudScriptModels;
using Newtonsoft.Json;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    public class PlayfabLobbyDatabase : MonoBehaviour, ILobbydatabase
    {
        #region ----Fields----
        public LobbyPlayerBasePayload currentUserData;
        public int currentLobby;
        public string testPlayfabId;
        public LobbyPlayerBasePayload testUserData;

        public LobbyPlayerBasePayload CurrentUserData { get => currentUserData; }

        public void Awake()
        {
            testUserData = new LobbyPlayerBasePayload()
            {
                playfabIdEncrypted = AESEncryption.Encrypt(testPlayfabId),
                encryptIV = AESEncryption.IV,
                displayName = "NotRadox",
                playerPosition = Vector3Extensions.FromVector3(Vector3.one * 5),
            };
        }
        public bool IsReady()
        {
            return true;
        }
        #endregion ----Fields----

        #region ----Methods----
        #region <<<Connect to Lobby>>>

        public void ConnectToLobby(int lobbyId, LobbyPlayerBasePayload userData, Action<ExecuteFunctionResult> resultCallback)
        {
            var playerData = JsonConvert.SerializeObject(userData);
            var appVersion = Application.version;
            var req = new ExecuteFunctionRequest()
            {
                FunctionName = "Connect_To_Lobby",
                FunctionParameter = new
                {
                    Keys = new
                    {
                        lobby = 0,
                        displayName = userData.displayName,
                        encryptIV = userData.encryptIV,
                        playerPosition = userData.playerPosition,
                        playfabIdEncrypted = userData.playfabIdEncrypted,
                        shinseiCompanionDna = userData.shinseiCompanionDna,
                        characterStyle = userData.characterStyle,
                        characterState = userData.characterState,
                        currentMatchId = userData.currentMatchId,
                        challengedPlayer = userData.challengedPlayer,
                        version = appVersion
                    }
                },
            };

            PlayFabCloudScriptAPI.ExecuteFunction(req, resultCallback, null);
        }


        #endregion <<<Connect to Lobby>>>

        #region <<<Send Get Player Data>>>
        public void SetGetPlayersData(LobbyPlayerBasePayload userData, Action<ExecuteFunctionResult> callback)
        {
            var playerData = JsonConvert.SerializeObject(userData);
            var req = new ExecuteFunctionRequest()
            {
                FunctionName = "Send_Get_LobbyData",
                FunctionParameter = new
                {
                    Keys = new
                    {
                        connectionId = userData.connectionId,
                        lobby = userData.lobby,
                        displayName = userData.displayName,
                        encryptIV = userData.encryptIV,
                        chatMessages = userData.chatMessages,
                        playerPosition = userData.playerPosition,
                        playfabIdEncrypted = userData.playfabIdEncrypted,
                        shinseiCompanionDna = userData.shinseiCompanionDna,
                        characterStyle = userData.characterStyle,
                        currentMatchId = userData.currentMatchId,
                        characterState = userData.characterState,
                        challengedPlayer = userData.challengedPlayer
                    }
                },
            };

            PlayFabCloudScriptAPI.ExecuteFunction(req, callback, (error) =>
            {
                SacredTailsLog.LogMessage(JsonConvert.SerializeObject(error));
            });

        }

        #endregion <<<Send Get Player Data>>>

        #region <<<Recieve Players Data>>>
        public void RecievePlayersData(Action<Dictionary<string, LobbyPlayerBasePayload>> callback)
        {
            Request.instance.RequestPetiton<Dictionary<string, LobbyPlayerBasePayload>>(
                _url: $"{RequestConstants.baseUrl}GetPlayerData/{currentLobby}",
                _type: RequestType.GET,
                _callback: (response) =>
                {
                    if (!(response == null || !response.success))
                        callback?.Invoke(response.data);
                }
                );
        }

        //public void PaintOtherPlayer(string playfabId, CharacterRecolor playerRecolor)
        //{
        //    var request = new PlayFab.ServerModels.GetUserDataRequest()
        //    {
        //        PlayFabId = playfabId,
        //        Keys = new List<string>() { "CharacterStyle" }
        //    };
        //    PlayFabServerAPI.GetUserData(request, (result) =>
        //    {
        //        var style = JsonConvert.DeserializeObject<Dictionary<PartsOfCharacter, CharacterStyleInfo>>(result.Data["CharacterStyle"].Value);
        //        foreach (var partStyle in style)
        //        {
        //            Color color;
        //            ColorUtility.TryParseHtmlString("#" + partStyle.Value.colorHex, out color);
        //            playerRecolor.ChangeMaterialColors(partStyle.Key, color);
        //        }
        //    },
        //    (error) => SacredTailsLog.LogErrorMessage(error.ErrorMessage));
        //}
        #endregion <<<Recieve Players Data>>>

        #region <<<Send Player Data>>>
        public void SendPlayerData(LobbyPlayerBasePayload userData, Action<bool> onSendSuccess = null)
        {
            if (currentLobby == -1)
            {
                SacredTailsLog.LogMessage($"{RequestConstants.firebaseError} Please connect to a lobby first before you send your data.");
                return;
            }

            Request.instance.RequestPetiton<LobbyPlayerBasePayload>(
                _url: $"{RequestConstants.baseUrl}SendPlayerData/{currentLobby}",
                 _type: RequestType.POST,
                 _payload: userData,
                 _callback: (response) =>
                 {
                     onSendSuccess?.Invoke(response.success);
                 }
             );
        }
        #endregion <<<Send Player Data>>>

        #region <<<Disconnect>>>
        public void DisconnectFromLobby(LobbyPlayerBasePayload userData)
        {
            var playerData = JsonConvert.SerializeObject(userData);
            var req = new ExecuteFunctionRequest()
            {
                FunctionName = "Disconnect_From_Lobby",
                FunctionParameter = new
                {
                    Keys = new
                    {
                        lobby = userData.lobby,
                        encryptIV = userData.encryptIV,
                        playfabIdEncrypted = userData.playfabIdEncrypted


                    }
                },
            };

            PlayFabCloudScriptAPI.ExecuteFunction(req, (success) => { SacredTailsLog.LogMessage("Disconected"); }, (error) => { SacredTailsLog.LogMessage("Disconection failed " + error.ErrorMessage); });
            /* Request.instance.RequestPetiton<LobbyPlayerBasePayload>(
                 _url: $"{RequestConstants.baseUrl}DeletePlayer/{currentLobby}",
                 _payload: new DeletePayload()
                 {
                     playfabIdEncrypted = currentUserData.playfabIdEncrypted,
                     encryptIV = currentUserData.encryptIV
                 },
                 _type: RequestType.DELETE,
                     _callback: (response) =>
                     {
                         if (response.success)
                             currentLobby = -1;
                     }
             );*/
        }

        #endregion <<<Disconnect>>>
        #endregion ----Methods----
    }

    public struct DeletePayload
    {
        public string playfabIdEncrypted;
        public string encryptIV;
    }
}


namespace Timba.Games.SacredTails.LobbyDatabase
{
}
