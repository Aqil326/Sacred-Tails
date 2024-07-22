using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;

public class BattleActionChangeTerrain : BattleActionsBase
{
    public List<BattleTerrainBehavioursBase> terrainsBehaviours;
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData changeTerrainData, Action onEndVfxCallback = null)
    {
        battleGameMode.currentTerrain = terrainsBehaviours.Find(item => item.terrainData.terrainType == changeTerrainData.typeOfTerrain);

        battleGameMode.currentTerrain.turnsLeft = changeTerrainData.turnsDuration;
        battleGameMode.currentTerrain.InitTerrainBehaviour(battleGameMode);
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData changeTerrainData)
    {
        return;
    }
}
