using System;
using UnityEngine;
using PlayFab;
using PlayFab.CloudScriptModels;
using Timba.Patterns;
using Timba.Games.SacredTails.PopupModule;
using Newtonsoft.Json;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    public class PlayfabTournamentDatabase : MonoBehaviour, ITournamentDatabase
    {
        public bool debugRequest = true;
        public bool debugResult = true;
        public bool debugError = true;
        public bool IsReady() { return true; }

        public void TournamentRequest(string tournamentId, TypeOfTournamentRequest typeOfRequest, Action<ExecuteFunctionResult> resultCallback, bool shouldMarkReady = false, int timesTrying = 0)
        {
            if (!debugRequest && PlayerDataManager.Singleton.isBot)
                debugRequest = true;
            string cloudFunctionName = "";
            switch (typeOfRequest)
            {
                case TypeOfTournamentRequest.JOIN_TOURNAMENT:
                    cloudFunctionName = "BracketsTournament_JoinTournament";
                    break;
                case TypeOfTournamentRequest.CHECK_READY_STATE:
                    cloudFunctionName = "BracketsTournament_GetBracketState";
                    break;
                case TypeOfTournamentRequest.CHECK_TOURNAMENT_STATE:
                    cloudFunctionName = "BracketsTournament_CheckState";
                    break;
                case TypeOfTournamentRequest.GET_CURRENT_BRACKETS_DATA:
                    cloudFunctionName = "BracketsTournament_GetCurrentBracketsData";
                    break;
                case TypeOfTournamentRequest.GET_TOURNAMENT_LIST:
                    cloudFunctionName = "BracketsTournament_GetTournamentList";
                    break;
            }

            object funcParams;
            if (typeOfRequest == TypeOfTournamentRequest.CHECK_READY_STATE)
                funcParams = new { Keys = new { tournamentId = tournamentId, shouldMarkReady = shouldMarkReady } };
            else
                funcParams = new { Keys = new { tournamentId = tournamentId } };

            var req = new ExecuteFunctionRequest()
            {
                FunctionName = cloudFunctionName,
                FunctionParameter = funcParams
            };
            if (debugRequest)
                SacredTailsLog.LogMessage($"<color=cyan>Request of <{typeOfRequest}></color>: {JsonConvert.SerializeObject(req, Formatting.Indented)}", true);
            PlayFabCloudScriptAPI.ExecuteFunction(req, (res) =>
            {
                if (debugResult)
                    SacredTailsLog.LogMessage($"<color=orange>Response of <{typeOfRequest}></color>: {JsonConvert.SerializeObject(res, Formatting.Indented)}", true);
                resultCallback?.Invoke(res);
            }, (err) =>
            {
                var newTry = timesTrying + 1;
                if (timesTrying < 5)
                    TournamentRequest(tournamentId, typeOfRequest, resultCallback, shouldMarkReady, newTry);
                if (debugError)
                    SacredTailsLog.LogErrorMessage($"Any was wrong <{typeOfRequest}>:" + err, true);
            });
        }
    }
}
