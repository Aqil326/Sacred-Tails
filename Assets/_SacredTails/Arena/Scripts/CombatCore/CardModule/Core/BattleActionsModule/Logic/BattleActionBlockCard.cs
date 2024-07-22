using System;
using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;

public class BattleActionBlockCard : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {
        //VFX
        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, battleActionData);
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData)
    {
        return;
    }

}
