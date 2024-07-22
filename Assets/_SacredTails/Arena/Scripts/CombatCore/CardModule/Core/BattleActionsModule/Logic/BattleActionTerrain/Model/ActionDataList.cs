using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Collections;
using Timba.SacredTails.Arena;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

[CreateAssetMenu(fileName = "ActionDataList", menuName = "Timba/SacredTails/ActionDataList")]
[System.Serializable]
public class ActionDataList : SerializedScriptableObject
{
    public List<BattleActionData> actions;
}
