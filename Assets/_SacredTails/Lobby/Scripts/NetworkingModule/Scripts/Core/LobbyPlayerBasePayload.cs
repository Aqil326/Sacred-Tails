using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using UnityEngine;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    [System.Serializable]
    public class LobbyPlayerBasePayload
    {
        public ulong connectionId = 0;
        public int lobby;
        public string displayName;
        public string encryptIV;
        public List<ChatMessagePayload> chatMessages;
        public SerializableVector3 playerPosition;
        public string playfabIdEncrypted;
        public string shinseiCompanionDna;
        public string characterStyle;
        public string currentMatchId;
        public int characterState;
        public string challengedPlayer;
    }
    [System.Serializable]
    public class ChatMessagePayload
    {
        public string id;
        public string message;
        public string timeStamp;
    }

    [System.Serializable]
    public enum CharacterStateEnum
    {
        LOBBY = 0,
        BACKPACK = 1,
        COMBAT = 2,
    }
}
