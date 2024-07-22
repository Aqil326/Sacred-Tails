using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using UnityEngine;
/// <summary>
/// Logic of action that allow you change shinsei in combat
/// </summary>
public class BattleActionChangeShinsei : BattleActionsBase
{

    public override void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {
        actionTime = 6;
        ownerPlayerAction.currentShinseiIndex = battleActionData.amount >= ownerPlayerAction.battleShinseis.Count ? ownerPlayerAction.currentShinseiIndex : battleActionData.amount;
        StartCoroutine(CheckAndPlayVfx(isLocalPlayer, ownerPlayerAction, otherPlayer, battleActionData, () =>
        {
            //The most important part
            battleGameMode.UpdateCurrentShinsei(ref ownerPlayerAction);
            StartCoroutine(WaitForNextFrame(() =>
            {
                //Update bars
                ResourceBarValues healthBar = ownerPlayerAction.healthbars[ownerPlayerAction.currentShinseiIndex];
                ResourceBarValues energyBar = ownerPlayerAction.energybars[ownerPlayerAction.currentShinseiIndex];
                int barIndex = isLocalPlayer ? 0 : 1;

                battleUIController.InitializeBars(healthBar.currentValue, barIndex, healthBar.maxValue, energyBar.currentValue, barIndex, energyBar.maxValue);
                battleUIController.ApplyEnergyChange(barIndex, energyBar.currentValue);
                //Show faster
                float ownSpeed = ownerPlayerAction.battleShinseis[ownerPlayerAction.currentShinseiIndex].ShinseiOriginalStats.speed;
                float otherSpeed = otherPlayer.battleShinseis[otherPlayer.currentShinseiIndex].ShinseiOriginalStats.speed;
                if (ownSpeed != otherSpeed)
                    battleUIController.ShowFaster(ownSpeed > otherSpeed ? 0 : 1);

                if (isLocalPlayer)
                    battleGameMode.turnsController.battlePlayerCurrentActions = new List<BattleActionData>();
                else
                    battleGameMode.turnsController.battleEnemyCurrentActions = new List<BattleActionData>();
            }));
            battleActionData.vfxIndex = -1;
        }));
    }

    IEnumerator CheckAndPlayVfx(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action callback)
    {
        CamerasAvailableEnum lookAtShinsei;
        if ((battleActionData.isSelfInflicted))
            lookAtShinsei = isLocalPlayer ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY;
        else
            lookAtShinsei = isLocalPlayer ? CamerasAvailableEnum.SIDE_CAMERA_ENEMY : CamerasAvailableEnum.SIDE_CAMERA_PLAYER;

        UserInfo targetPlayer = battleActionData.isSelfInflicted ? ownerPlayer : otherPlayer;

        //ShowBothShinseisFar(); //CAMTIME ChangeShinsei
        yield return new WaitForSeconds(2);


        camManager.SwitchToCam(lookAtShinsei);

        if (battleActionData.vfxAffectBoth)
            targetPlayer.spawnedShinsei.animator.SetTrigger("Death");
        yield return new WaitForSeconds(2);
        callback?.Invoke();
        targetPlayer.spawnedShinsei.GetComponent<CharacterSlot>().SetShinseiEvolution(true);

        if (battleActionData.vfxAffectBoth)
            targetPlayer.spawnedShinsei.animator.SetTrigger("Change");
        yield return new WaitForSeconds(4);
        targetPlayer.spawnedShinsei.GetComponent<CharacterSlot>().SetShinseiEvolution(false);
        //ShowBothShinseisFar(); //CAMTIME ChangeShinsei
    }


    public override float ActionTime()
    {
        return 6;
    }

    IEnumerator WaitForNextFrame(Action callback)
    {
        yield return new WaitForEndOfFrame();
        callback?.Invoke();
    }

    public override void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData)
    {
        return;
    }
}
