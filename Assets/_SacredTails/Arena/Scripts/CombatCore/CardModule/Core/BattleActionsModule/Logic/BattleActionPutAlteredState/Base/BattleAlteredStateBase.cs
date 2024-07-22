using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;

public abstract class BattleAlteredStateBase : MonoBehaviour
{
    public BattleAlteredStateDataSO alteredStateData;
    private protected BattleGameMode battleGameMode;
    private protected BattleUIController battleUIController;
    public AttacksAnimation muzzleAnimation = AttacksAnimation.Damage;

    public virtual void InitAlteredState(bool isLocal, UserInfo _targetInfo, BattleGameMode _battleGameMode, BattleUIController _battleUIController, bool justReplace = false, string locDamageMsg = "")
    {
        battleGameMode = _battleGameMode;
        battleUIController = _battleUIController;
        if (justReplace)
            return;
        string targetName = _targetInfo.isLocalPlayer ? "<color=green>Your</color>" : "<color=orange>Enemy</color>";
        //battleGameMode.AddTextToLog($"{targetName} {alteredStateData.startMessage} {locDamageMsg}" + " - 11");
        Debug.Log($"{targetName} {alteredStateData.startMessage} {locDamageMsg}" + " - 11");
    }

    public virtual void ExecuteAlteredState(UserInfo _targetInfo = null, List<BattleActionData> turnActions = null, bool isTargetLocalPlayer = false, string locDamageMsg = "")
    {
        UserInfo ownerInfo = null;
        try
        {
            if (isTargetLocalPlayer)
                ownerInfo = battleGameMode.enemyInfo;
            else
                ownerInfo = battleGameMode.playerInfo;

            if (_targetInfo == null)
            {
                if (isTargetLocalPlayer)
                    _targetInfo = battleGameMode.playerInfo;
                else
                    _targetInfo = battleGameMode.enemyInfo;
            }

            if (_targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates.ContainsKey(alteredStateData.alteredState) && _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates[alteredStateData.alteredState].HasPassedATurn())
            {
                string targetName = _targetInfo.isLocalPlayer ? "<color=green>Your</color>" : "<color=orange>Enemy</color>";
                //battleGameMode.AddTextToLog($"{targetName} {alteredStateData.displayMessage} {locDamageMsg}" + " -12");
                Debug.Log($"{targetName} {alteredStateData.displayMessage}{locDamageMsg}" + " -12");

                _targetInfo.spawnedShinsei.animator.SetTrigger(Enum.GetName(typeof(AttacksAnimation), muzzleAnimation));
                AlteredStateEnum currentAlteredState = _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates.Keys.GetEnumerator().Current;
                AlteredStateInstantiateVfx(isTargetLocalPlayer, currentAlteredState);

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error on base altered state, info: {JsonConvert.SerializeObject(ownerInfo)} {JsonConvert.SerializeObject(_targetInfo)}");
        }

    }
    private void AlteredStateInstantiateVfx(bool isPlayer, AlteredStateEnum alteredState)
    {
        VFXTypeData vfxTypeData = battleGameMode.turnsController.vfxsActionType[GetVfxIndexByAlteredState(alteredState)];
        if (vfxTypeData != null)
        {
            Transform playerLocation = isPlayer ? battleGameMode.turnsController.vfxPositionsDictionary[VFXPositionEnum.SHINSEI_PLAYER] : battleGameMode.turnsController.vfxPositionsDictionary[VFXPositionEnum.SHINSEI_ENEMY];
            GameObject go = Instantiate(vfxTypeData.vfxPrefab, playerLocation.position, playerLocation.rotation);
            Destroy(go, 2);
        }
    }

    private int GetVfxIndexByAlteredState(AlteredStateEnum alteredState)
    {
        int vfxIndex = 0;
        switch (alteredState)
        {
            case AlteredStateEnum.EvasionChange:
                vfxIndex = 0;
                break;
            case AlteredStateEnum.Bleeding:
                vfxIndex = 1;
                break;
            case AlteredStateEnum.Rooted:
                vfxIndex = 2;
                break;
            case AlteredStateEnum.Ignited:
                vfxIndex = 3;
                break;
            default:
                break;
        }
        return vfxIndex;
    }

    public virtual void EndAlteredState(UserInfo _targetInfo = null, bool isTargetLocalPlayer = false)
    {
        UserInfo ownerInfo;
        if (isTargetLocalPlayer)
            ownerInfo = battleGameMode.enemyInfo;
        else
            ownerInfo = battleGameMode.playerInfo;

        if (_targetInfo == null)
            _targetInfo = isTargetLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;

        //Set variables
        //alteredStateData.alteredStateActions.actions.ForEach(actionData =>
        //    {
        //        UserInfo tempInfo = new UserInfo();
        //        actionData.isSelfInflicted = false;
        //        if (battleGameMode.turnsController.turnActionsDatabaseDictionary.ContainsKey(actionData.actionType))
        //            battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].
        //                EndAction(false, tempInfo, ownerInfo, actionData);
        //    }
        //);

        string targetName = _targetInfo.isLocalPlayer ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";//"Your" : "Enemy";
        _targetInfo.battleShinseis[_targetInfo.currentShinseiIndex].alteredStates.Remove(alteredStateData.alteredState);
        battleGameMode.AddTextToLog($"{targetName} {alteredStateData.endMessage}");
        Debug.Log($"{targetName} {alteredStateData.endMessage}" + "13");
    }
}
