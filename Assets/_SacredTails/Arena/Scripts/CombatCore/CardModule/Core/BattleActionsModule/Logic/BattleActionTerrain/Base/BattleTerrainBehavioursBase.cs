using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using UnityEngine;

public abstract class BattleTerrainBehavioursBase : MonoBehaviour
{
    [HideInInspector] protected List<Shinsei> allShinseis;
    [HideInInspector] public int turnsLeft;
    [HideInInspector] protected int turnsDuration;
    [HideInInspector] protected BattleGameMode battleGameMode;

    public BattleTerrainDataSO terrainData;

    #region MainMethods
    /// <summary>
    /// Init terrain behaviour, return time of behaviour
    /// </summary>
    /// <returns></returns>
    public virtual float InitTerrainBehaviour(BattleGameMode battleGameMode)
    {
        this.battleGameMode = battleGameMode;

        turnsDuration = turnsLeft;
        allShinseis = new List<Shinsei>();
        allShinseis.AddRange(battleGameMode.playerInfo.battleShinseis);
        allShinseis.AddRange(battleGameMode.enemyInfo.battleShinseis);

        return 0;
    }

    /// <summary>
    /// Execute terrain behaviour, return time of behaviour
    /// </summary>
    /// <returns></returns>
    public virtual float ExecuteTerrainBehaviour()
    {
        //Say displayMessage
        battleGameMode.AddTextToLog(terrainData.displayMessage);
        Debug.Log(terrainData.displayMessage + "16");

        turnsLeft--;
        if (turnsLeft == 0)
            EndTerrainBehaviour();
        return 0;
    }

    /// <summary>
    /// End terrain behaviour, return time of behaviour
    /// </summary>
    /// <returns></returns>
    public virtual float EndTerrainBehaviour()
    {
        battleGameMode.currentTerrain = null;
        return ExecuteActionsOfTerrain(EndAction, true);
    }
    #endregion MainMethods

    #region Helpers
    /// <summary>
    /// Execute the actions set to this terrain, 
    /// </summary>
    /// <param name="executeOrEnd"></param>
    /// <param name="toAll">toAll shinseis or just to the current ones</param>
    /// <returns></returns>
    public virtual float ExecuteActionsOfTerrain(Func<CharacterType, bool, float> executeOrEnd, bool toAll = false)
    {
        float? actionTime = 0;
        if (toAll)
        {
            var originalPlayerShinseiIndex = battleGameMode.playerInfo.currentShinseiIndex;
            var originalEnemyShinseiIndex = battleGameMode.enemyInfo.currentShinseiIndex;

            int counter = 0;
            allShinseis.ForEach(shinsei =>
            {
                if (counter < 3)
                {
                    battleGameMode.playerInfo.currentShinseiIndex = counter % 3;
                    actionTime += executeOrEnd?.Invoke(shinsei.shinseiType, true);
                }
                else
                {
                    battleGameMode.enemyInfo.currentShinseiIndex = counter % 3;
                    actionTime += executeOrEnd?.Invoke(shinsei.shinseiType, false);
                }

                counter++;
            });

            battleGameMode.playerInfo.currentShinseiIndex = originalPlayerShinseiIndex;
            battleGameMode.enemyInfo.currentShinseiIndex = originalEnemyShinseiIndex;
        }
        else
        {
            var shinseiTypePlayer = battleGameMode.playerInfo.battleShinseis[battleGameMode.playerInfo.currentShinseiIndex].shinseiType;
            var shinseiTypeEnemy = battleGameMode.enemyInfo.battleShinseis[battleGameMode.enemyInfo.currentShinseiIndex].shinseiType;

            actionTime += executeOrEnd?.Invoke(shinseiTypePlayer, true);
            actionTime += executeOrEnd?.Invoke(shinseiTypeEnemy, false);
        }
        return (float)actionTime;
    }

    public float ExecuteAction(CharacterType shinseiType, bool isPlayer = false)
    {
        float actionTime = 0;
        int indexOfTypeAction = terrainData.typesActions.FindIndex((TypesActions actionType) => actionType.typeOfShinsei == shinseiType);
        if (indexOfTypeAction != -1)
        {
            terrainData.typesActions[indexOfTypeAction].actionsData.actions.ForEach(actionData =>
            {
                actionData.isSelfInflicted = isPlayer;
                actionData.launchVfx = false;
                actionData.turnsPassed = turnsDuration;
                battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ExecuteAction(
                    false, battleGameMode.playerInfo, battleGameMode.enemyInfo, actionData);

                actionTime = battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ActionTime();
            });
        }
        terrainData.globalActions?.actions.ForEach(actionData =>
        {
            actionData.isSelfInflicted = isPlayer;
            actionData.launchVfx = false;
            actionData.turnsPassed = turnsDuration;
            battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ExecuteAction(
                false, battleGameMode.playerInfo, battleGameMode.enemyInfo, actionData);

            actionTime = battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ActionTime();
        });
        return actionTime;
    }

    public float EndAction(CharacterType shinseiType, bool isPlayer = false)
    {
        float actionTime = 0;
        int indexOfTypeAction = terrainData.typesActions.FindIndex((TypesActions actionType) => actionType.typeOfShinsei == shinseiType);
        if (indexOfTypeAction != -1)
        {
            terrainData.typesActions[indexOfTypeAction].actionsData.actions.ForEach(actionData =>
            {
                actionData.isSelfInflicted = isPlayer;
                actionData.launchVfx = false;
                actionData.turnsPassed = turnsDuration;
                battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].EndAction(
                    false, battleGameMode.playerInfo, battleGameMode.enemyInfo, actionData);

                actionTime = battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ActionTime();
            });
        }
        terrainData.globalActions?.actions.ForEach(actionData =>
        {
            actionData.isSelfInflicted = isPlayer;
            actionData.launchVfx = false;
            actionData.turnsPassed = turnsDuration;
            battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].EndAction(
                false, battleGameMode.playerInfo, battleGameMode.enemyInfo, actionData);

            actionTime = battleGameMode.turnsController.turnActionsDatabaseDictionary[actionData.actionType].ActionTime();
        });
        return actionTime;
    }
    #endregion Helpers
}
