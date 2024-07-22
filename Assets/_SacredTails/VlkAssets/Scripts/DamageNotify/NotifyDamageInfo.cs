/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;*/

public class NotifyDamageInfo
{
    private int damageAmount;
    public int _damageAmount { get { return damageAmount; } set { damageAmount = value; } }

    private int healingAmount;
    public int _healingAmount { get { return healingAmount; } set { healingAmount = value; } }

    private string alteredStateType;
    public string _alteredStateType { get { return alteredStateType; } set { alteredStateType = value; } }
    /*private AlteredStateEnum alteredStateType;
    public AlteredStateEnum _alteredStateType { get { return alteredStateType; } set { alteredStateType = value; } }*/
    public float multiplier;

    public NotifyDamageInfo(int locDamageAmount, int locHealingAmount, string locAlteredStateType, float multiplier = 1)
    {
        damageAmount = locDamageAmount;
        healingAmount = locHealingAmount;
        alteredStateType = locAlteredStateType;
        this.multiplier = multiplier;
    }
}
