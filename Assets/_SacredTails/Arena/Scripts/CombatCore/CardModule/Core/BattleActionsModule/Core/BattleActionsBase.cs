using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.CharacterFactory;
using Timba.Games.DynamicCamera;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using Timba.SacredTails.Arena.ShinseiType;
using Timba.SacredTails.Database;
using Timba.SacredTails.VFXController;
using UnityEngine;

public abstract class BattleActionsBase : MonoBehaviour
{
    public ActionTypeEnum actionType;

    protected CameraPlaneController camManager;
    protected VFXInstancer vFXInstancer;
    protected BattleGameMode battleGameMode;
    protected BattleUIController battleUIController;
    protected ShinseiTypeScriptable shinseiTypeScriptable;
    protected bool launchVfx = false;
    protected float vfxTime = 0;
    protected float actionTime;
    protected string targetName = "";

    public virtual void Init(CameraPlaneController _camManager, VFXInstancer _vFXInstancer, BattleGameMode _battleGameMode, BattleUIController _battleUIController, ShinseiTypeScriptable _shinseiTypeScriptable)
    {
        camManager = _camManager;
        vFXInstancer = _vFXInstancer;
        battleGameMode = _battleGameMode;
        battleUIController = _battleUIController;
        shinseiTypeScriptable = _shinseiTypeScriptable;
    }

    public virtual void ExecuteAction(bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {

        bool isEnemy = (isLocalPlayer && !battleActionData.isSelfInflicted) || (!isLocalPlayer && battleActionData.isSelfInflicted);
        string targetName = !isEnemy ? "<color=green>Your</color>" : "<color=orange>Enemy</color>";
        //VFX
        Debug.Log("Name: " + battleActionData.ToString() + ", launchVfx: " + battleActionData.launchVfx);
        if (battleActionData.launchVfx)
        {
            if (battleActionData.vfxIndex == -1 && battleActionData.isComingFromCopyIndex > 0)
            {
                //actionTime += vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration + 4;
                Debug.Log("vfx Add: " + vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration);
                actionTime += vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration;
                battleActionData.isSelfInflicted = false;
                StartCoroutine(CheckAndPlayVfx(battleActionData.currentVFXPositions, battleActionData.vfxIndex, battleActionData.vfxTime, isLocalPlayer, ownerPlayer, otherPlayer, battleActionData, onEndVfxCallback));
                return;
            }
            vfxTime = battleActionData.vfxTime;
            launchVfx = battleActionData.launchVfx;
            Debug.Log("vfx Set: " + vfxTime);

            if (battleActionData.targetAnim != AttacksAnimation.NONE)
                actionTime = (vfxTime - 3 >= 0 ? vfxTime - 3 : 0) + vfxTime;
                //actionTime = 8 + (vfxTime - 3 >= 0 ? vfxTime - 3 : 0) + vfxTime;
            else
                actionTime = vfxTime;
            //actionTime = 4 + vfxTime;

            if (battleActionData.isComingFromCopyIndex > 0)
            {
                actionTime += vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration;
                //actionTime += vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration + 4;
                Debug.Log("vfx Add: " + vFXInstancer.GetVfx(battleActionData.isComingFromCopyIndex).vfxDuration);
            }

            StartCoroutine(CheckAndPlayVfx(battleActionData.currentVFXPositions, battleActionData.vfxIndex, battleActionData.vfxTime, isLocalPlayer, ownerPlayer, otherPlayer, battleActionData, onEndVfxCallback));
        }
        else
        {
            StartCoroutine(CheckAction(actionTime, onEndVfxCallback));
            Debug.Log("actionTime: " + actionTime + "VFX");
            actionTime = 0;
        }
    }
    public IEnumerator CheckAction(float time, Action onEndVfxCallback)
    {
        yield return new WaitForSeconds(time);
        onEndVfxCallback?.Invoke();
    }

    IEnumerator CheckAndPlayVfx(Dictionary<VFXPositionEnum, Transform> currentVFXPositions, int vfxIndex, float vfxTime, bool isLocalPlayer, UserInfo ownerPlayer, UserInfo otherPlayer, BattleActionData battleActionData, Action onEndVfxCallback = null)
    {
        //BattleLog 
        VFXPositionEnum userIndex;
        VFXPositionEnum targetIndex;
        CamerasAvailableEnum lookAtShinsei;
        CamerasAvailableEnum lookAtOtherShinsei;
        DecideIndexes(out userIndex, out targetIndex, out lookAtShinsei, out lookAtOtherShinsei, isLocalPlayer, battleActionData);

        if (battleActionData.isComingFromCopyIndex > 0)
        {
            //Play attack animation
            PlayAnimation(ownerPlayer, AttacksAnimation.ATTACK0, isLocalPlayer, battleActionData);
            camManager.SwitchToCam(lookAtShinsei);

            yield return new WaitForSeconds(2);

            // Show Vfx of original ability
            PlayAnimation(ownerPlayer, AttacksAnimation.Recharge, isLocalPlayer, battleActionData);
            VFXPositionEnum copyAbilityVfxPosIndex = isLocalPlayer ? VFXPositionEnum.SHINSEI_PLAYER : VFXPositionEnum.SHINSEI_ENEMY;
            VfxInfo vfx = vFXInstancer?.SpawnVFX(battleActionData.isComingFromCopyIndex, currentVFXPositions[copyAbilityVfxPosIndex].position, currentVFXPositions[copyAbilityVfxPosIndex].rotation).GetComponent<VfxInfo>();

            yield return new WaitForSeconds(vfx.vfxDuration);

            camManager.SwitchToCam(CamerasAvailableEnum.FAR_MIDDLE_CAMERA);

            // Add copied ability to the log
            string textP = !isLocalPlayer ? "<color=orange>Enemy</color>" : "<color=green>Your</color>";
            Dictionary<string, string> keyWords = new Dictionary<string, string> { { "[p]", textP } };

            ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(battleActionData.copiedIndex);
            string chosenCardText = "Chosen card: " + actionCard.name;
            battleGameMode.AddTextToLog(chosenCardText, keyWords);
            Debug.Log(chosenCardText + "10");
            Debug.Log(actionCard.DisplayNotification + "11");
            StartCoroutine(WaitForSeconds(1, () => battleGameMode.AddTextToLog(actionCard.DisplayNotification, keyWords)));
        }
        if (vfxIndex != -1)
        {
            UserInfo targetPlayer = battleActionData.isSelfInflicted ? ownerPlayer : otherPlayer;

            if (battleActionData.vfxAffectBoth)
            {
                PlayVfxActionType(battleActionData.actionElementType, currentVFXPositions[userIndex].position, currentVFXPositions[userIndex].rotation);
                //ShowBothShinseisFar(); //CAMTIME was disabled
                PlayAnimation(ownerPlayer, battleActionData.casterAnim, isLocalPlayer, battleActionData);

                yield return new WaitForSeconds(2);

                vFXInstancer?.SpawnVFX(vfxIndex, currentVFXPositions[VFXPositionEnum.ARENA_CENTER].position, currentVFXPositions[VFXPositionEnum.ARENA_CENTER].rotation);

                bool attackEvadedOwner = ownerPlayer.battleShinseis[ownerPlayer.currentShinseiIndex].didEvadeAttack;
                bool attackEvadedOther = otherPlayer.battleShinseis[otherPlayer.currentShinseiIndex].didEvadeAttack;

                PlayAnimation(ownerPlayer, attackEvadedOwner ? AttacksAnimation.Dodge : battleActionData.targetAnim, isLocalPlayer, battleActionData);
                PlayAnimation(otherPlayer, attackEvadedOther ? AttacksAnimation.Dodge : battleActionData.targetAnim, isLocalPlayer, battleActionData);
                Debug.Log("vfxTime: " + vfxTime);
                yield return new WaitForSeconds(vfxTime);
            }
            else
            {
                //ShowBothShinseisFar(); //CAMTIME was disabled

                //yield return new WaitForSeconds(2); //2
                camManager.SwitchToCam(lookAtShinsei); //CAMTIME Init Turn step_3
                camManager.ClearPointOfInterest();
                yield return new WaitForSeconds(1); //2


                if (vfxIndex != -1)
                {

                    PlayAnimation(ownerPlayer, battleActionData.casterAnim, isLocalPlayer, battleActionData);
                    PlayVfxActionType(battleActionData.actionElementType, currentVFXPositions[userIndex].position, currentVFXPositions[userIndex].rotation);
                    //Play vfx for 1sec and delete it 
                    if (currentVFXPositions != null)
                    {
                        //OWNER VFX
                        /*yield return new WaitForSeconds(2);
                        GameObject vfx = vFXInstancer?.SpawnVFX(vfxIndex, currentVFXPositions[targetIndex].position, currentVFXPositions[targetIndex].rotation);
                        if (battleActionData.targetAnim != AttacksAnimation.NONE)
                        {
                            yield return new WaitForSeconds(1);
                            Destroy(vfx);
                        }
                        else
                            yield return new WaitForSeconds(vfxTime);*/ //CAMTIME Is Disable

                        //TARGET
                        if (battleActionData.targetAnim != AttacksAnimation.NONE && !battleActionData.isSelfInflicted)
                        {
                            //camManager.SwitchToCam(lookAtOtherShinsei); //CAMTIME Init Turn step_4
                            //camManager.ClearPointOfInterest();
                            float hitDelay = vFXInstancer != null ? vFXInstancer.GetVfxHitDelay(vfxIndex) : 1;
                            //Debug.Log("hitDelay: " + hitDelay);
                            //Debug.Log("vfxTime - 3: " + (vfxTime - 3));
                            yield return new WaitForSeconds(1);

                            vFXInstancer?.SpawnVFX(vfxIndex, currentVFXPositions[targetIndex].position, currentVFXPositions[targetIndex].rotation);
                            Debug.Log("hitDelay: " + hitDelay);
                            yield return new WaitForSeconds(hitDelay);

                            bool attackEvaded = targetPlayer.battleShinseis[targetPlayer.currentShinseiIndex].didEvadeAttack;
                            PlayAnimation(otherPlayer, attackEvaded ? AttacksAnimation.Dodge : battleActionData.targetAnim, isLocalPlayer, battleActionData);

                            yield return new WaitForSeconds(vfxTime - 3 >= 0 ? vfxTime - 3 : 0);
                        }
                    }
                }
            }
        }
        onEndVfxCallback?.Invoke();
    }

    private void PlayVfxActionType(CharacterType actionType, Vector3 position, Quaternion rotation)
    {
        VFXTypeData vfxTypeData = battleGameMode.turnsController.vfxsActionType.Find(vfx => vfx.type == actionType);
        if (vfxTypeData != null)
        {
            GameObject go = Instantiate(vfxTypeData.vfxPrefab, position, rotation);
            Destroy(go, 2);
        }
    }

    public void DecideIndexes(out VFXPositionEnum ownerPositionIndexForMuzzle, out VFXPositionEnum targetPositionIndex, out CamerasAvailableEnum lookAtShinseiCam, out CamerasAvailableEnum lookAtOtherShinseiCam, bool isLocal, BattleActionData battleActionData)
    {
        //lookAtShinseiCam = isLocal ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY; //CAMERA ATTACK
        //lookAtShinseiCam = isLocal ? CamerasAvailableEnum.GENERAL_CAMERA : CamerasAvailableEnum.GENERAL_CAMERA; //CAMTIME Init Turn step_2_1 Set Camera Lauch  CAMERA ATTACK

        List<int> auxValues = new List<int> { 1, 0, 1, 0};

        // Select a random index from the list
        int randomIndex = UnityEngine.Random.Range(0, auxValues.Count);

        // Retrieve the element at the random index
        int elementResult = auxValues[randomIndex];

        if (elementResult == 1)
        //if(Random.ra)
        {
            lookAtShinseiCam = isLocal ? CamerasAvailableEnum.GENERAL_CAMERA : CamerasAvailableEnum.GENERAL_CAMERA; //CAMERA ATTACK
        }
        else
        {
            lookAtShinseiCam = isLocal ? CamerasAvailableEnum.FAR_MIDDLE_CAMERA : CamerasAvailableEnum.FAR_MIDDLE_CAMERA; //CAMERA ATTACK
        }

        ownerPositionIndexForMuzzle = isLocal ? VFXPositionEnum.SHINSEI_PLAYER : VFXPositionEnum.SHINSEI_ENEMY;
        //ownerPositionIndexForMuzzle = isLocal ? VFXPositionEnum.SHINSEI_PLAYER : VFXPositionEnum.SHINSEI_PLAYER;
        if (!battleActionData.isSelfInflicted)
        {
            lookAtOtherShinseiCam = isLocal ? CamerasAvailableEnum.SIDE_CAMERA_ENEMY : CamerasAvailableEnum.SIDE_CAMERA_PLAYER;
            targetPositionIndex = isLocal ? VFXPositionEnum.SHINSEI_ENEMY : VFXPositionEnum.SHINSEI_PLAYER;
        }
        else
        {
            lookAtOtherShinseiCam = isLocal ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY;
            targetPositionIndex = isLocal ? VFXPositionEnum.SHINSEI_PLAYER : VFXPositionEnum.SHINSEI_ENEMY;
        }
    }

    public void ShowBothShinseisFar()
    {
        camManager.SwitchToCam(CamerasAvailableEnum.FAR_MIDDLE_CAMERA); //CAMTIME Init Turn step_2
        camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.ARENA_CENTER);
    }

    public void PlayAnimation(UserInfo targetInfo, AttacksAnimation animation, bool isLocalPlayer, BattleActionData battleActionData)
    {
        if (battleActionData.isSelfInflicted)
        {
            if ((battleGameMode.turnsController.isPlayerSleep && isLocalPlayer) || (battleGameMode.turnsController.isEnemySleep && !isLocalPlayer))
                return;
        }
        else
        {
            if ((battleGameMode.turnsController.isPlayerSleep && !isLocalPlayer) || (battleGameMode.turnsController.isEnemySleep && isLocalPlayer))
                return;
        }
        switch (animation)
        {
            case AttacksAnimation.NONE:
                break;
            case AttacksAnimation.ATTACK0:
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 0);
                targetInfo.spawnedShinsei.animator.SetTrigger("Attack");
                break;
            case AttacksAnimation.ATTACK1:
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 1);
                targetInfo.spawnedShinsei.animator.SetTrigger("Attack");
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 0);
                break;
            case AttacksAnimation.ATTACK2:
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 2);
                targetInfo.spawnedShinsei.animator.SetTrigger("Attack");
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 0);
                break;
            case AttacksAnimation.ATTACK3:
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 3);
                targetInfo.spawnedShinsei.animator.SetTrigger("Attack");
                targetInfo.spawnedShinsei.animator.SetFloat("Random", 0);
                break;
            default:
                targetInfo.spawnedShinsei.animator.SetTrigger(Enum.GetName(typeof(AttacksAnimation), animation));
                break;
        }
    }


    public virtual float ActionTime()
    {
        return actionTime;
    }

    public virtual void EndAction(bool isLocalPlayer, UserInfo ownerPlayerAction, UserInfo otherPlayer, BattleActionData battleActionData)
    {

    }

    IEnumerator WaitForSeconds(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
}
