using FirebaseRequestManager;
using PlayFab.CloudScriptModels;
using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    public interface ILobbydatabase : IService
    {
        public LobbyPlayerBasePayload CurrentUserData { get; }
        public void ConnectToLobby(int lobbyId, LobbyPlayerBasePayload userData, Action<ExecuteFunctionResult> resultCallback);
        public void SetGetPlayersData(LobbyPlayerBasePayload userData, Action<ExecuteFunctionResult> resultCallback);
        public void RecievePlayersData(Action<Dictionary<string, LobbyPlayerBasePayload>> callback);
        public void SendPlayerData(LobbyPlayerBasePayload userData, Action<bool> onSendSuccess = null);
        public void DisconnectFromLobby(LobbyPlayerBasePayload userData);
    }
}
