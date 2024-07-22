using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "AlteredStateDataSO", menuName = "Timba/SacredTails/AlteredState/AlteredStateDataSO")]
[System.Serializable]
public class AlteredStatesDataSO : SerializedScriptableObject
{
    public List<BattleAlteredStateDataSO> alteredStates;

    [Button("Generate JSON")]
    public void GetJsonAlteredStates()
    {
        string data = JsonConvert.SerializeObject(this);
        File.WriteAllText($"Assets/_content/ServerData/AlteredStatesSO.json", data);
    }
}
