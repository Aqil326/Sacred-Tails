using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// Actions that make damage
/// </summary>
public class BattleActionDamage : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = battleActionData.isSelfInflicted ? ownerPlayer : otherPlayer;

        string ownerMessage = isLocalPlayer ? "<color=green>Your</color>" : "<color=orange>Enemy</color>";

        Debug.Log("isLocalPlayer: " + isLocalPlayer + ", battleActionData.isSelfInflicted: " + battleActionData.isSelfInflicted);

        //targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].shinseiHealth;
        //targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue -= targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].realDirectDamage;

        //Debug.Log("realDirectDamage: " + targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].realDirectDamage + " " + battleActionData.isSelfInflicted
        Debug.Log("Damage type cosa: "+ battleActionData.actionElementType);
        float typeDamageMultiplier = ShinseiTypeMatrixHelper.GetShinseiTypeMultiplier(battleActionData.actionElementType, targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].shinseiType);
        Debug.Log("Type multiplier "+ typeDamageMultiplier);

        List<NotifyDamageInfo> auxNotifyDamageInfo = new List<NotifyDamageInfo>() {
            new NotifyDamageInfo(targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].realDirectDamage, 0, "", typeDamageMultiplier)
        };

        bool auxIsPlayer = battleActionData.isSelfInflicted ? isLocalPlayer : !isLocalPlayer;

        //base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, battleActionData, () => battleUIController.ChangeHealthbarView("1", auxNotifyDamageInfo, battleActionData.isSelfInflicted));
        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, battleActionData, () =>
        {
            targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue -= targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].realDirectDamage;

            Debug.Log("realDirectDamage: " + targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].realDirectDamage + " " + battleActionData.isSelfInflicted);

            //battleUIController.ChangeHealthbarView("1", auxNotifyDamageInfo, battleActionData.isSelfInflicted);
            battleUIController.ChangeHealthbarView("1", auxNotifyDamageInfo, auxIsPlayer);//!isLocalPlayer);
        });

        Debug.Log("Apply Update Health 01");
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData)
    {
        return;
    }

}
