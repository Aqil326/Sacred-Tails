using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.Arena;
using UnityEngine;

[System.Serializable]
public class Shinsei
{
    public string shinseiName;
    public string ShinseiDna;
    public string generation;
    //token padres  
    //Descripci?n 
    //childrens
    public List<int> ShinseiActionsIndex;
    public CharacterType shinseiType;
    public Dictionary<AlteredStateEnum, AlteredStateData> alteredStates = new Dictionary<AlteredStateEnum, AlteredStateData>();
    public int reflectDamage;
    public RarityType shinseiRarity;
    public ShinseiStats ShinseiOriginalStats;
    //[System.NonSerialized]
    public Sprite shinseiIcon;

    public int shinseiHealth;
    public int shinseiEnergy;
    public int evadeChance = 0;
    public bool didAlteredStateKillShinsei;
    public int healthAfterAlteredState;

    public int healingAmount = 0;
    public int realDirectDamage = 0;
    public float typeMultiplier = 0;
    public bool didEvadeAttack = false;
}

[System.Serializable]
public class AlteredStateData
{
    public bool isTargetLocalPlayer;
    public int amount;
    public bool perTurns;
    public int turnsDuration;
    public int turnsLeft;

    public int realDamageApplied = 0;
    public int realHealApplied = 0;

    public bool HasPassedATurn()
    {
        return turnsDuration != turnsLeft;
    }
}
