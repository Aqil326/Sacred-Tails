using System;
using Timba.SacredTails.Arena;
/// <summary>
/// This class allow user send a turn withouth data
/// </summary>
public class BattleActionSkipTurn : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData skipTurnData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = skipTurnData.isSelfInflicted ? ownerPlayer : otherPlayer;

        bool attackEvaded = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].didEvadeAttack;
        if (skipTurnData.cardSkipTurn && skipTurnData.turnsPassed == 0)
        {
            BattleActionData skipTurn = new BattleActionData()
            {
                isSelfInflicted = true,
                actionType = ActionTypeEnum.SkipTurn,
                turnsDuration = 2,
                cardSkipTurn = true,
                turnsPassed = 1
            };

            if (isLocalPlayer)
                battleGameMode.turnsController.battlePlayerCurrentActions.Add(skipTurn);
            else
                battleGameMode.turnsController.battleEnemyCurrentActions.Add(skipTurn);
        }
        //VFX
        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, skipTurnData);
    }
    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData putAlteredStateData)
    {
        return;
    }
}
