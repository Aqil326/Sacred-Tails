using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// This class control the behavior of Altered States
/// </summary>
public class BattleActionPutAlteredState : BattleActionsBase
{
    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData putAlteredStateData, Action onEndVfxCallback = null)
    {
        UserInfo targetPlayer = putAlteredStateData.isSelfInflicted ? ownerPlayer : otherPlayer;

        Action auxCallBack = null;

        bool attackEvaded = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].didEvadeAttack;
        if (attackEvaded && putAlteredStateData.alteredStateToActivate != AlteredStateEnum.EvasionChange)
        {
            battleUIController.battleNotificationSystem.AddText("Put altered state evaded!");
            Debug.Log("Put altered state evaded!" + "05");
        }
        else
        {
            //float auxDamageDif = Mathf.Abs(targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue - targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].alteredStates[AlteredStateEnum.Ignited].amount);
            //float auxDamageDif = Mathf.Abs(targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue - targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].healthAfterAlteredState);
            //Damage applyed only for altered state:
            int auxRealDamageApplied = 0;
            if (targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].alteredStates.ContainsKey(putAlteredStateData.alteredState))
                auxRealDamageApplied = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].alteredStates[putAlteredStateData.alteredState].realDamageApplied;
            //float auxDamageDif = auxRealDamageApplied;
            targetPlayer.healthbars[targetPlayer.currentShinseiIndex].currentValue -= auxRealDamageApplied;

            Debug.Log("AlteredState - realDamageApplied: " + auxRealDamageApplied);

            if (targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].alteredStates.ContainsKey(putAlteredStateData.alteredState))
                targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].alteredStates.Remove(putAlteredStateData.alteredState);

            bool isTargetLocal = (isLocalPlayer && putAlteredStateData.isSelfInflicted) || (!isLocalPlayer && !putAlteredStateData.isSelfInflicted);
            battleGameMode.turnsController.alteredStateController.alteredStates[(int)putAlteredStateData.alteredState].InitAlteredState(isTargetLocal, targetPlayer, battleGameMode, battleUIController, false, "Danoo: " + auxRealDamageApplied);

            if (auxRealDamageApplied != 0)
            {
                Debug.Log("Altered State efficiency data: " + putAlteredStateData.actionElementType);
                float typeDamageMultiplier = ShinseiTypeMatrixHelper.GetShinseiTypeMultiplier(putAlteredStateData.actionElementType, targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].shinseiType);

                List<NotifyDamageInfo> auxNotifyDamageInfoAlteredState = new List<NotifyDamageInfo>() {
                    new NotifyDamageInfo(auxRealDamageApplied, 0, putAlteredStateData.alteredState.ToString(),typeDamageMultiplier)
                };

                bool auxIsPlayer = putAlteredStateData.isSelfInflicted ? isLocalPlayer : !isLocalPlayer;

                //isLocalPlayer && putAlteredStateData.isSelfInflicted ? true : false;

                auxCallBack = () => {

                    battleUIController.ChangeHealthbarView("6", auxNotifyDamageInfoAlteredState, auxIsPlayer);//!isLocalPlayer);//putAlteredStateData.isSelfInflicted);
                    Debug.Log("Apply Update Health -- Put alteredState 100");
                };
                //targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue = targetInfo.battleShinseis[targetInfo.currentShinseiIndex].healthAfterAlteredState;
                //targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue -= accumulatedDamageOfAlteredStates;
                //battleUIController.ChangeHealthbarView("6");
                //Debug.Log("Apply Update Health -- Put alteredState 100");
            }
        }

        //VFX
        base.ExecuteAction(isLocalPlayer, ownerPlayer, otherPlayer, putAlteredStateData, auxCallBack);
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData putAlteredStateData)
    {
        return;
    }
}
