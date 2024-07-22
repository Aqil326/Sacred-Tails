using System;
using System.Collections.Generic;
using System.Reflection;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// This class allow swap between two stats
/// </summary>
public class BattleActionStatSwap : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData statSwapData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = statSwapData.isSelfInflicted ? ownerPlayer : otherPlayer;
        var targetActions = (statSwapData.isSelfInflicted && isLocalPlayer) || (!statSwapData.isSelfInflicted && !isLocalPlayer) ? battleGameMode.turnsController.battlePlayerCurrentActions : battleGameMode.turnsController.battleEnemyCurrentActions;
        if (statSwapData.turnsPassed != 0)
            return;

        bool attackEvaded = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].didEvadeAttack;
        if (attackEvaded)
        {
            statSwapData.evadedTurns++;
            battleUIController.battleNotificationSystem.AddText("An stat swap was evaded");
            Debug.Log("An stat swap was evaded" + "06");
            return;
        }
        if (statSwapData.changeMinAndMaxStats)
        {
            var stats = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats;
            ShinseiStatsEnum maxStat = ShinseiStatsEnum.Attack;
            float maxValue = 0;
            ShinseiStatsEnum minStat = ShinseiStatsEnum.Attack;
            float minValue = 999999999;
            foreach (FieldInfo fieldInfo in typeof(ShinseiStats).GetFields())
            {
                if (fieldInfo.Name == "Health" || fieldInfo.Name == "Energy")
                    continue;
                float statValue = (float)fieldInfo.GetValue(stats);
                if (statValue > maxValue)
                {
                    maxValue = statValue;
                    Enum.TryParse(fieldInfo.Name, out maxStat);
                }
                if (statValue < minValue)
                {
                    minValue = statValue;
                    Enum.TryParse(fieldInfo.Name, out minStat);
                }
            }
            (string fieldName1, bool didApply) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, maxStat, minValue, false, true);
            (string fieldName2, bool didApply2) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, minStat, maxValue, false, true);
            statSwapData.stat1 = maxStat;
            statSwapData.stat2 = minStat;

            battleUIController.battleNotificationSystem.AddText($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values");
            Debug.Log($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values" + "07");
            CheckBuffDebuffTargets(fieldName1, fieldName2, targetActions);
        }
        else
        {
            var stat1Value = BattleStatisticsCalculator.GetStatByName(targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat1);
            var stat2Value = BattleStatisticsCalculator.GetStatByName(targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat2);

            (string fieldName1, bool didApply) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat1, stat2Value, false, replaceStat: true);
            (string fieldName2, bool didApply2) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat2, stat1Value, false, replaceStat: true);

            CheckBuffDebuffTargets(fieldName1, fieldName2, targetActions);
            battleUIController.battleNotificationSystem.AddText($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values");
            Debug.Log($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values" + "08");
        }
        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, statSwapData);
    }

    public void CheckBuffDebuffTargets(string stat1Name, string stat2Name, List<BattleActionData> targetBattleActions)
    {
        targetBattleActions.ForEach(action =>
        {
            ShinseiStatsEnum stat1 = (ShinseiStatsEnum)Enum.Parse(typeof(ShinseiStatsEnum), stat1Name);
            ShinseiStatsEnum stat2 = (ShinseiStatsEnum)Enum.Parse(typeof(ShinseiStatsEnum), stat2Name);
            bool stat1Target = action.statToModify == stat1;
            bool stat2Target = action.statToModify == stat2;
            if (action.actionType == ActionTypeEnum.BuffDebuff && (stat1Target || stat2Target))
            {
                if (stat1Target)
                    action.statToModify = stat2;
                else
                    action.statToModify = stat1;
            }
        });
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData statSwapData)
    {
        UserInfo targetPlayer = statSwapData.isSelfInflicted ? ownerPlayer : otherPlayer;
        var stat1Value = BattleStatisticsCalculator.GetStatByName(targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat1);
        var stat2Value = BattleStatisticsCalculator.GetStatByName(targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat2);

        (string fieldName1, bool didApply) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat1, stat2Value, false, replaceStat: true);
        (string fieldName2, bool didApply2) = BattleStatisticsCalculator.SetStatByName(ref targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].ShinseiOriginalStats, statSwapData.stat2, stat1Value, false, replaceStat: true);

        var targetActions = (statSwapData.isSelfInflicted && isLocalPlayer) || (!statSwapData.isSelfInflicted && !isLocalPlayer) ? battleGameMode.turnsController.battlePlayerCurrentActions : battleGameMode.turnsController.battleEnemyCurrentActions;
        CheckBuffDebuffTargets(fieldName1, fieldName2, targetActions);

        battleUIController.battleNotificationSystem.AddText($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values back to normal.");
        Debug.Log($"{targetName} Shinsei has swapped {fieldName1} and {fieldName2} stat values back to normal." + "09");

    }

}
