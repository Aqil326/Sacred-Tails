using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// The specific case of altered state Root
/// </summary>
public class BattleAlteredStateRoot : BattleAlteredStateBase
{
    public override void ExecuteAlteredState(UserInfo ownerInfo = null, List<BattleActionData> turnActions = null, bool isTargetLocalPlayer = false, string locDamageMsg = "")
    {
        base.ExecuteAlteredState(ownerInfo, turnActions, isTargetLocalPlayer, locDamageMsg);

        if (ownerInfo == null)
            ownerInfo = isTargetLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;
    }
}

