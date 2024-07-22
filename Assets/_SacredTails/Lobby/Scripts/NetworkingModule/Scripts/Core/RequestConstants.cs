using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using UnityEngine;
using FirebaseRequestManager;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    public static class RequestConstants
    {
        public const string baseUrl = "https://sacredtails.azurewebsites.net/api/";

        public const string firebaseMessage = "<color=blue>Firebase Lobby Database: </color>";
        public const string firebaseError = "<color=red>Firebase Lobby Database: </color>";
    }
}
