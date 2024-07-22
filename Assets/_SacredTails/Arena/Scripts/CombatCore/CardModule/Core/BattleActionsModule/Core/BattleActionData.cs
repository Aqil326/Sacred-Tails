using MyBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using UnityEngine;
using UnityEngine.Serialization;



[System.Serializable]
public class BattleActionData : ICloneable
{
    #region ---- Fields ----
    #region <<< Base stats >>>
    public ActionTypeEnum actionType;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.PutAlteredState)] public bool perTurns = true;
    public int turnsDuration;
    [ConditionalField(nameof(actionType), true, ActionTypeEnum.TerrainChange, ActionTypeEnum.SkipTurn)] public int amount;
    [ConditionalField(nameof(actionType), true, ActionTypeEnum.TerrainChange)] public bool isSelfInflicted;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.SkipTurn)] public bool cardSkipTurn = true;

    [FormerlySerializedAs("bonusDamagePercent")]
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.Damage, ActionTypeEnum.Healing)] public float bonusPercent;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.Damage, ActionTypeEnum.Healing)] public ShinseiStatsEnum statBonusDamage;
    #endregion <<< Base stats >>>

    #region <<< Buff stats >>>
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.BuffDebuff)] public bool applyEachTurn;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.BuffDebuff)] public bool isBuff;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.BuffDebuff)] public bool isPercertange = true;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.BuffDebuff)] public ShinseiStatsEnum statToModify;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.BuffDebuff)] public int numberOfTimesBuffApplied;
    #endregion <<< Buff stats >>>

    #region <<< StatSwap stats >>>
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.StatSwap)] public bool changeMinAndMaxStats;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.StatSwap)] public ShinseiStatsEnum stat1;
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.StatSwap)] public ShinseiStatsEnum stat2;
    #endregion <<< StatSwap  stats >>>


    #region <<< Terrain stats >>>
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.TerrainChange)] public TypesOfTerrainEnum typeOfTerrain;
    #endregion <<< Terrain stats >>>

    #region <<< PutAlteredState stats >>>
    [ConditionalField(nameof(actionType), false, ActionTypeEnum.PutAlteredState)] public AlteredStateEnum alteredState;
    #endregion <<< PutAlteredState stats >>>

    #region <<< Damage stats >>>

    [ConditionalField(nameof(actionType), false, ActionTypeEnum.Damage)] public bool activateAlteredState;
    [ConditionalField(nameof(activateAlteredState), false, true)] public AlteredStateEnum alteredStateToActivate;

    [ConditionalField(nameof(actionType), false, ActionTypeEnum.Damage)] public int criticsPercentChange;
    #endregion <<< Damage stats >>>

    #region <<< NON SERIALIZABLE >>>
    [HideInInspector] public CharacterType actionElementType;
    [HideInInspector] public float criticsRoll;
    [HideInInspector] public float evadeRoll;
    [HideInInspector] public int turnsPassed;
    [HideInInspector] public int evadedTurns;

    [System.NonSerialized] public bool isVfxReversed;
    [System.NonSerialized] public bool launchVfx;
    [System.NonSerialized] public bool vfxAffectBoth;
    [System.NonSerialized] public AttacksAnimation casterAnim;
    [System.NonSerialized] public AttacksAnimation targetAnim;
    [System.NonSerialized] public ActionCardDto turnActions;

    [System.NonSerialized] public int vfxIndex;
    [System.NonSerialized] public float vfxTime;
    [System.NonSerialized] public int copiedIndex = -1;
    [System.NonSerialized] public int isComingFromCopyIndex = -1;
    [System.NonSerialized] public Dictionary<VFXPositionEnum, Transform> currentVFXPositions;
    #endregion <<< NON SERIALIZABLE >>>
    #endregion ---- Fields ----

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public class ShouldSerializeContractResolver : DefaultContractResolver
{
    public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyName != "BattleActions")
        {
            if (property.PropertyName == "amount")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType != ActionTypeEnum.TerrainChange &&
                               actionData.actionType != ActionTypeEnum.SkipTurn;
                    };
            }
            else if (property.PropertyName == "isSelfInflicted")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType != ActionTypeEnum.TerrainChange;
                    };
            }
            else if ((property.PropertyName == "isBuff" || property.PropertyName == "statToModify" || property.PropertyName == "isPercentage"))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType == ActionTypeEnum.BuffDebuff;
                    };
            }
            else if (property.PropertyName == "alteredState" || property.PropertyName == "perTurns")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType == ActionTypeEnum.PutAlteredState;
                    };
            }
            else if (property.PropertyName == "typeOfTerrain")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType == ActionTypeEnum.TerrainChange;
                    };
            }
            else if (property.PropertyName == "criticsDamageIncrease" || property.PropertyName == "bonusDamagePercent" || property.PropertyName == "statBonusDamage")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        BattleActionData actionData = (BattleActionData)instance;
                        return actionData.actionType == ActionTypeEnum.Damage || actionData.actionType == ActionTypeEnum.Healing;
                    };
            }
        }
        return property;
    }
}
