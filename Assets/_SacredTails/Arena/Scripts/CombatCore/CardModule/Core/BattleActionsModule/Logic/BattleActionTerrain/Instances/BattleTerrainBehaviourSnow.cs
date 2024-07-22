using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Timba.SacredTails.Arena;
using UnityEngine;

public class BattleTerrainBehaviourSnow : BattleTerrainBehavioursBase
{
    public override float InitTerrainBehaviour(BattleGameMode battleGameMode)
    {
        float actionTime = 0;

        base.InitTerrainBehaviour(battleGameMode);
        ExecuteActionsOfTerrain(ExecuteAction, true);

        return actionTime;
    }

    public override float ExecuteTerrainBehaviour()
    {
        float actionTime = 0;
        base.ExecuteTerrainBehaviour();
        return actionTime;

    }

    public override float EndTerrainBehaviour()
    {
        return base.EndTerrainBehaviour();
    }
}
