using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// Specific case of Altered State Ignite
/// </summary>
public class BattleAlteredStateIgnited : BattleAlteredStateBase
{
    public override void ExecuteAlteredState(UserInfo _targetInfo = null, List<BattleActionData> turnActions = null, bool isTargetLocalPlayer = false, string locDamageMsg = "")
    {
        base.ExecuteAlteredState(_targetInfo, turnActions, isTargetLocalPlayer, locDamageMsg);
        if (_targetInfo == null)
            _targetInfo = isTargetLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;

        int extraDamageAmount = _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[AlteredStateEnum.Ignited].amount;
        turnActions.ForEach((System.Action<BattleActionData>)(action =>
        {
            if (action.activateAlteredState && action.alteredStateToActivate == AlteredStateEnum.Ignited)
            {
                battleUIController.battleNotificationSystem.AddText($"Attack improved by ignite!");
                Debug.Log($"Attack improved by ignite!" + "14");
                //action.amount = Mathf.FloorToInt(action.amount * (1 + (((float)extraDamageAmount) / 100)));
            }
        }));
    }

    
}

