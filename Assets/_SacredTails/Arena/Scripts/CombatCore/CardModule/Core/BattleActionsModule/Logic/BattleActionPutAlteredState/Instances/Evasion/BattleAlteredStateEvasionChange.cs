using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using Timba.SacredTails.Arena;
using UnityEngine;

public class BattleAlteredStateEvasionChange : BattleAlteredStateBase
{
    public override void InitAlteredState(bool isLocal, UserInfo _targetInfo, BattleGameMode _battleGameMode, BattleUIController _battleUIController, bool justReplace = false, string locDamageMsg = "")
    {
        base.InitAlteredState(isLocal, _targetInfo, _battleGameMode, _battleUIController, justReplace, locDamageMsg);
        ExecuteAlteredState(_targetInfo,
                 isLocal ? battleGameMode.turnsController.battlePlayerCurrentActions : battleGameMode.turnsController.battleEnemyCurrentActions
             , isLocal, locDamageMsg);
    }

    public override void ExecuteAlteredState(UserInfo _targetInfo = null, List<BattleActionData> turnActions = null, bool isTargetLocalPlayer = false, string locDamageMsg = "")
    {
        base.ExecuteAlteredState(_targetInfo, turnActions, isTargetLocalPlayer, locDamageMsg);
    }

    public override void EndAlteredState(UserInfo _targetInfo = null, bool isTargetLocalPlayer = false)
    {
        if (_targetInfo == null)
            _targetInfo = isTargetLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;

        bool countPerTurns = _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[AlteredStateEnum.EvasionChange].perTurns;
        int turnsDuration = _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[alteredStateData.alteredState].turnsDuration;

        if (countPerTurns)
            _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].evadeChance -= (_targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[AlteredStateEnum.EvasionChange].amount) * turnsDuration;
        else
            _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].evadeChance -= (_targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[AlteredStateEnum.EvasionChange].amount);

        base.EndAlteredState(_targetInfo, isTargetLocalPlayer);
    }
}
