using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// Logic that allow change value of stats in combat
/// </summary>
public class BattleActionBuffDebuff : BattleActionsBase
{
    public static bool isTurnPassingPlayer = false;
    public static bool isTurnPassingOponnent = false;

    public static bool isChangingPlayerShinsei = false;
    public static bool isChangingOpponentShinsei = false;
    [SerializeField] private BuffNDebuffViewer playerBuffViewer;
    [SerializeField] private BuffNDebuffViewer opponentBuffViewer;

    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData buffDebuffData, Action onEndVfxCallback = null)
    {       
        bool isEnemy = (isLocalPlayer && !buffDebuffData.isSelfInflicted) || (!isLocalPlayer && buffDebuffData.isSelfInflicted);
        string targetName = !isEnemy ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>"; //"<color=green>Your</color>" : "<color=orange>Enemy</color>";

        Debug.Log("Buff debug :: " + isLocalPlayer + " is buff "+ buffDebuffData.isBuff+ " buff turns " + buffDebuffData.turnsDuration + " buff type " + buffDebuffData.statToModify);


        if(isChangingPlayerShinsei)
        {
            playerBuffViewer.ClearAllBuffsViews();
            isChangingPlayerShinsei = false;
        }
        if(isChangingOpponentShinsei)
        {
            opponentBuffViewer.ClearAllBuffsViews();
            isChangingOpponentShinsei = false;
        }

        Debug.Log("Opponent shinsei " + otherPlayer.currentShinseiIndex);

        if ((buffDebuffData.turnsPassed == 0) && ((isLocalPlayer && buffDebuffData.isSelfInflicted) || (!isLocalPlayer && !buffDebuffData.isSelfInflicted)))
        {
            playerBuffViewer.AddBuff(buffDebuffData.isBuff, buffDebuffData.turnsDuration, buffDebuffData.statToModify, buffDebuffData.amount, buffDebuffData.isPercertange);
        }
        else if((buffDebuffData.turnsPassed == 0) && ((!isLocalPlayer && buffDebuffData.isSelfInflicted) || (isLocalPlayer && !buffDebuffData.isSelfInflicted)))
        {
            opponentBuffViewer.AddBuff(buffDebuffData.isBuff, buffDebuffData.turnsDuration, buffDebuffData.statToModify, buffDebuffData.amount, buffDebuffData.isPercertange);
        }

        if (buffDebuffData.applyEachTurn || (!buffDebuffData.applyEachTurn && buffDebuffData.turnsPassed == 0))
        {
            UserInfo targetPlayer = buffDebuffData.isSelfInflicted ? ownerPlayer : otherPlayer;
            int buffAmount = buffDebuffData.amount;
            if (!buffDebuffData.isBuff)
                buffAmount *= -1;

            bool attackEvaded = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].didEvadeAttack;
            if (attackEvaded)
            {
                bool auxIsPlayer = buffDebuffData.isSelfInflicted ? isLocalPlayer : !isLocalPlayer;
                string auxPlayerName = auxIsPlayer ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";

                battleUIController.battleNotificationSystem.AddText(auxPlayerName + " debuff was evaded");
                Debug.Log("A buff was evaded" + "01");
                buffDebuffData.evadedTurns++;
            }
            else
            {
                (string fieldName, bool didApply) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, buffDebuffData.statToModify, buffAmount, buffDebuffData.isPercertange);
                if (didApply)
                    buffDebuffData.numberOfTimesBuffApplied++;
                battleUIController.battleNotificationSystem.AddText($"{targetName} Shinsei {fieldName} {(buffDebuffData.isBuff ? "<color=#2FCC7B>increases</color>" : "<color=#F54F4F>decreases</color>")}");
                Debug.Log($"{targetName} Shinsei {fieldName} {(buffDebuffData.isBuff ? "increases" : "decreases")}" + "02");
            }

            base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, buffDebuffData);
        }
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData buffDebuffData)
    {

        float buffAmount = buffDebuffData.amount;
        UserInfo targetPlayer = buffDebuffData.isSelfInflicted ? ownerPlayer : otherPlayer;

        if (!buffDebuffData.isBuff)
            buffAmount *= -1;

        string fieldName = "";
        bool didApply;
        int turnsApplied = buffDebuffData.numberOfTimesBuffApplied - buffDebuffData.evadedTurns;
        for (int i = buffDebuffData.applyEachTurn ? 0 : (turnsApplied - 1); i < (turnsApplied); i++)
        {
            if (i == turnsApplied)
                (fieldName, didApply) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, buffDebuffData.statToModify, buffAmount, buffDebuffData.isPercertange, isEndAction: true);
            else
                BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, buffDebuffData.statToModify, buffAmount, buffDebuffData.isPercertange, isEndAction: true);
        }

        //BattleLog 
        string targetName;
        if (buffDebuffData.isSelfInflicted)
            targetName = isLocalPlayer ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";
        else
            targetName = isLocalPlayer ? "<color=#F54F4F>[Enemy]</color>" : "<color=#2FCC7B>[Player]</color>";

        string message = $"{targetName} Shinsei {fieldName} is back to normal.";
        battleUIController.battleNotificationSystem.AddText(message);
        Debug.Log(message + "03");
    }

}
