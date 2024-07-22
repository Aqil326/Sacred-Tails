using PlayFab.CloudScriptModels;
using System;
using Timba.Patterns.ServiceLocator;

namespace Timba.Games.SacredTails.LobbyDatabase
{
    public interface ITournamentDatabase : IService
    {
        public void TournamentRequest(string tournamentId, TypeOfTournamentRequest typeOfRequest, Action<ExecuteFunctionResult> resultCallback, bool shouldMarkReady = true, int timesTrying = 0);
    }
}
