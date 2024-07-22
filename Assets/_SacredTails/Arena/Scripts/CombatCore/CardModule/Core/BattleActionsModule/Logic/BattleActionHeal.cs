using System;
using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// This class allow grown up the life values
/// </summary>
public class BattleActionHeal : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData healData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = healData.isSelfInflicted ? ownerPlayer : otherPlayer;

        /*ResourceBarValues targetHealthBar = targetPlayer.healthbars[targetPlayer.currentShinseiIndex];
        targetHealthBar.currentValue = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].shinseiHealth;*/
        targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue += targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount;

        List<NotifyDamageInfo> auxNotifyDamageInfo = new List<NotifyDamageInfo>() {
            //new NotifyDamageInfo(0, targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount, "")
            new NotifyDamageInfo(0, targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount, "")
        };

        bool auxIsPlayer = healData.isSelfInflicted ? isLocalPlayer : !isLocalPlayer;

        Debug.Log("State healingAmount: " + targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount);

        int barIndex = 0;
        if (healData.isSelfInflicted)
            barIndex = isLocalPlayer ? 0 : 1;
        else
            barIndex = isLocalPlayer ? 1 : 0;

        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, healData, () =>
        {
            //battleUIController.battleNotificationSystem.AddText($"Shinsei has healed " + targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount + " 04");
            Debug.Log($"Shinsei has healed " + targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healingAmount + " 04");
            battleUIController.ChangeHealthbarView("2", auxNotifyDamageInfo, auxIsPlayer);//isLocalPlayer);
            Debug.Log("Apply Update Health 03");
        });
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData)
    {
        return;
    }
}
