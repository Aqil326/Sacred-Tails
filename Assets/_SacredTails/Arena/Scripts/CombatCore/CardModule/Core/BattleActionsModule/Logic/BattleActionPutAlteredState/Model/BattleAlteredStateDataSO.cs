using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleAlteredStateDataSO", menuName = "Timba/SacredTails/AlteredState/BattleAlteredStateDataSO")]
[System.Serializable]
public class BattleAlteredStateDataSO : SerializedScriptableObject
{
    public AlteredStateEnum alteredState;
    public ActionDataList alteredStateActions;
    [JsonIgnore] [TextArea] public string startMessage;
    [JsonIgnore] [TextArea] public string displayMessage;
    [JsonIgnore] [TextArea] public string endMessage;
}
