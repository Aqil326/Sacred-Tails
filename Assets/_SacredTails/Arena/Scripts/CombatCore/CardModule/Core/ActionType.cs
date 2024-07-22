using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ActionTypeEnum
{
    BuffDebuff,
    Healing,
    Damage,
    TerrainChange,
    PutAlteredState,
    ChangeShinsei,
    SkipTurn,
    BlockAction,
    Randomize,
    ReflectDamage,
    StatSwap,
    CopyCat,
    TMP
}

public enum ShinseiStatsEnum
{
    Attack = 0,
    Defence = 1,
    Vigor = 2,
    Speed = 3,
    Stamina = 4
}

public enum TypesOfTerrainEnum
{
    Flames,
    Snow,
    Eclipse,
    Light
}
public enum AlteredStateEnum
{
    EvasionChange,
    Ignited,
    Rooted,
    Bleeding,
    None
}
public enum CriticsCheck
{
    Is_Sleep
}
