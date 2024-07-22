using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using Timba.SacredTails.VFXController;
using TMPro;
using UnityEngine;

public class AlteredStateController : MonoBehaviour
{
    #region ----Fields-----
    public List<BattleAlteredStateBase> alteredStates;
    private BattleGameMode battleGameMode;
    private BattleUIController battleUIController;
    [SerializeField] private VFXInstancer vFXInstancer;
    [SerializeField] private AlteredView playerAlteredView;
    [SerializeField] private AlteredView opponentAlteredView;
    [SerializeField] private Sprite[] alteredStateIcons;

    [SerializeField] private BuffNDebuffViewer playerBuffViewer;
    [SerializeField] private BuffNDebuffViewer opponentBuffViewer;

    private int playerShinseiIndex = 99;
    private int enemyShinseiIndex = 99;
    #endregion ----Fields-----

    #region ----Method-----
    #region Init
    public void InitAlteredStateController(BattleGameMode _battleGameMode, BattleUIController _battleUIController)
    {
        battleGameMode = _battleGameMode;
        battleUIController = _battleUIController;
    }
    #endregion Init

    #region Check shinsei alteredStates
    /// <summary>
    /// Check the player altered states, execute them with the aid of the
    /// alteredStateControllers classes found in the alteredStates list.
    /// </summary>
    /// <param name="isPlayer">Is the Local Player altered states</param>
    /// <param name="playerTurn">turn of the player checking the actions</param>
    /// <returns></returns>
    public bool CheckAlteredStates(bool isPlayer, ActionCardDto playerTurn)
    {
        SacredTailsLog.LogMessageForBot($">>>>> Check altered state {isPlayer} <<<<<<");
        UserInfo targetInfo = isPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;

        //float auxDamageInfo = Mathf.Abs(targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue - targetInfo.battleShinseis[targetInfo.currentShinseiIndex].healthAfterAlteredState);

        int accumulatedDamageOfAlteredStates = 0;

        var listOfAlteredStatesToRemove = new List<AlteredStateEnum>();

        List<NotifyDamageInfo> auxNotifyDamageInfoAlteredState = new List<NotifyDamageInfo>();

        foreach (var alteredState in targetInfo.battleShinseis[targetInfo.currentShinseiIndex].alteredStates)
        {
            Debug.Log("Altered state data :: " + alteredState.Key + " is player " + isPlayer + " turns left" + alteredState.Value.turnsLeft);
            if (isPlayer)
                playerAlteredView.ShowAlteredByTime((AlteredStateEnum)((int)alteredState.Key), alteredState.Value.turnsLeft, alteredStateIcons[(int)alteredState.Key]);
            else
                opponentAlteredView.ShowAlteredByTime((AlteredStateEnum)((int)alteredState.Key), alteredState.Value.turnsLeft, alteredStateIcons[(int)alteredState.Key]);

            if (alteredState.Value.turnsLeft == 0)
            {
                listOfAlteredStatesToRemove.Add(alteredState.Key);
            }
            else
            {
                alteredStates[(int)alteredState.Key].ExecuteAlteredState(
                    turnActions: playerTurn.BattleActions,
                    isTargetLocalPlayer: isPlayer,
                    //locDamageMsg: "Applies damage of " + auxDamageInfo + " ");
                    locDamageMsg: "Applies damage of " + alteredState.Value.realDamageApplied + " ");

                accumulatedDamageOfAlteredStates += alteredState.Value.realDamageApplied; //Accumulates damage to be used later.
                Debug.Log("Altered debug xx " + alteredState.Key);
                CharacterType typeSelected = AlteredStateToType(alteredState.Key);
                if (typeSelected != CharacterType.NotSelected)
                {
                    float multiplierType = ShinseiTypeMatrixHelper.GetShinseiTypeMultiplier(typeSelected, targetInfo.battleShinseis[targetInfo.currentShinseiIndex].shinseiType);
                    auxNotifyDamageInfoAlteredState.Add(new NotifyDamageInfo(alteredState.Value.realDamageApplied, 0, alteredState.Key.ToString(), multiplierType));
                }
                else {
                    auxNotifyDamageInfoAlteredState.Add(new NotifyDamageInfo(alteredState.Value.realDamageApplied, 0, alteredState.Key.ToString()));
                }
            }
            
        }

        if ((isPlayer && targetInfo.currentShinseiIndex != playerShinseiIndex) || (!isPlayer && playerTurn.indexCard == 1))
        {
            playerAlteredView.RemoveAllAlteredStates();
            playerBuffViewer.ClearAllBuffsViews();
            playerShinseiIndex = targetInfo.currentShinseiIndex;
        }
        else if ((!isPlayer && targetInfo.currentShinseiIndex != enemyShinseiIndex) || (isPlayer && playerTurn.indexCard == 1))
        {
            opponentAlteredView.RemoveAllAlteredStates();
            opponentBuffViewer.ClearAllBuffsViews();
            enemyShinseiIndex = targetInfo.currentShinseiIndex;
        }

        if (targetInfo.battleShinseis[targetInfo.currentShinseiIndex].didAlteredStateKillShinsei)
        {
            //targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue = targetInfo.battleShinseis[targetInfo.currentShinseiIndex].shinseiHealth;
            targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue -= accumulatedDamageOfAlteredStates;
            battleGameMode.turnsController.DoDeathVerification(isPlayer);
            if (isPlayer)
            {
                playerAlteredView.RemoveAllAlteredStates();
                playerBuffViewer.ClearAllBuffsViews();
            }
            else
            {
                opponentAlteredView.RemoveAllAlteredStates();
                opponentBuffViewer.ClearAllBuffsViews();
            }
            return true;
        }

        if (accumulatedDamageOfAlteredStates != 0)
        {
            //targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue = targetInfo.battleShinseis[targetInfo.currentShinseiIndex].healthAfterAlteredState;
            targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue -= accumulatedDamageOfAlteredStates;
            battleUIController.ChangeHealthbarView("5", auxNotifyDamageInfoAlteredState, isPlayer);
            Debug.Log("Apply Update Health 02");
        }

        foreach (var alteredStateToRemove in listOfAlteredStatesToRemove)
            alteredStates[(int)alteredStateToRemove].EndAlteredState(isTargetLocalPlayer: isPlayer);

        return false;
    }

    public CharacterType AlteredStateToType(AlteredStateEnum alteredState)
    {
        switch (alteredState)
        {
            case AlteredStateEnum.Ignited:
                return CharacterType.Sun;
            case AlteredStateEnum.Rooted:
                return CharacterType.Nature;
            default:
                return CharacterType.NotSelected;
        }
    }

    #endregion Check shinsei alteredStates

    #region Server Data
    public void InitNewAlteredStates(bool isPlayer, Dictionary<AlteredStateEnum, AlteredStateData> serverStates, Dictionary<AlteredStateEnum, AlteredStateData> localStates,bool justReplace = false, string locDamageMsg = "")
    {
        
        foreach (var alteredState in serverStates)
        {
            if (!localStates.ContainsKey(alteredState.Key))
            {  
                alteredStates[(int)alteredState.Key].InitAlteredState(isPlayer,
                                                                      isPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo,
                                                                      battleGameMode,
                                                                      battleUIController,
                                                                      justReplace,
                                                                      locDamageMsg);
            }
        }
        UserInfo targetInfo = isPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;
        if (isPlayer && targetInfo.currentShinseiIndex != playerShinseiIndex)
        {
            playerBuffViewer.ClearAllBuffsViews();
            playerAlteredView.RemoveAllAlteredStates();
            playerShinseiIndex = targetInfo.currentShinseiIndex;
        }
        else if (!isPlayer && targetInfo.currentShinseiIndex != enemyShinseiIndex)
        {
            opponentBuffViewer.ClearAllBuffsViews();
            opponentAlteredView.RemoveAllAlteredStates();
            enemyShinseiIndex = targetInfo.currentShinseiIndex;
        }

        foreach (var alteredState in targetInfo.battleShinseis[targetInfo.currentShinseiIndex].alteredStates)
        {
            Debug.Log("Altered index : "+((int)alteredState.Key));
            if (alteredState.Value.turnsLeft == alteredState.Value.turnsDuration - 1)
            {
                if(isPlayer)
                    playerAlteredView.ShowAlteredByTime(ConvertAlteredStateIndexToEnum((int)alteredState.Key), alteredState.Value.turnsLeft, alteredStateIcons[(int)ConvertAlteredStateIndexToEnum((int)alteredState.Key)]);
                else
                    opponentAlteredView.ShowAlteredByTime(ConvertAlteredStateIndexToEnum((int)alteredState.Key), alteredState.Value.turnsLeft, alteredStateIcons[(int)ConvertAlteredStateIndexToEnum((int)alteredState.Key)]);
            }
        }

    }

    private AlteredStateEnum ConvertAlteredStateIndexToEnum(int index)
    {
        AlteredStateEnum alteredStateEnum = AlteredStateEnum.Ignited;
        switch(index)
        {
            case 0:
                alteredStateEnum = AlteredStateEnum.EvasionChange;
                break;
            case 1:
                alteredStateEnum = AlteredStateEnum.Ignited;
                break;
            case 2:
                alteredStateEnum = AlteredStateEnum.Rooted;
                break;
            case 3:
                alteredStateEnum = AlteredStateEnum.Bleeding;
                break;
        }
        return alteredStateEnum;
    }
    #endregion Server Data
    #endregion ----Method-----
}
