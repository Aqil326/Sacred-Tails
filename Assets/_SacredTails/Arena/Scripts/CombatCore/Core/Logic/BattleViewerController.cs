using UnityEngine;

namespace Timba.SacredTails.Arena
{
    public class BattleViewerController : MonoBehaviour
    {
        public BattleGameMode battleGameMode;

        public void Initialize(string matchId, bool isViewing)
        {
            battleGameMode.localCombat = new Combat()
            {
                CurrentTurn = 0
            };
            battleGameMode.playerInfo.userIndex = 0;
            battleGameMode.enemyInfo.userIndex = 1;

            battleGameMode.playerInfo.isLocalPlayer = true;
            battleGameMode.enemyInfo.isLocalPlayer = false;

            battleGameMode.localCombat.MatchData.MatchId = matchId;
            battleGameMode.localCombat.MatchData.MatchPlayers = new System.Collections.Generic.List<CombatPlayer>() { null, null };

            if (isViewing)
                battleGameMode.WaitForOpponentToSelectViewMatch();
            else
                battleGameMode.StartCoroutine(battleGameMode.WaitForOpponentToSelect());
        }
    }
}

