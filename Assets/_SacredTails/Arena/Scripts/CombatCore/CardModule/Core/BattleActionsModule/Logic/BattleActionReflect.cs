using System;
using Timba.SacredTails.Arena;
/// <summary>
/// This class controls a special attack case when shinsei reflect damage
/// </summary>
public class BattleActionReflect : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = battleActionData.isSelfInflicted ? ownerPlayer : otherPlayer;

        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, battleActionData, onEndVfxCallback);
    }
}
