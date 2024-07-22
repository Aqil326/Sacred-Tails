using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainChangeDataSO", menuName = "Timba/SacredTails/TerrainChange/TerrainChangeDataSO")]
[System.Serializable]
public class TerrainChangesDataSO : SerializedScriptableObject
{
    public List<BattleTerrainDataSO> terrainChanges;

    [Button("Generate JSON")]
    public void GetJsonAlteredStates()
    {
        string data = JsonConvert.SerializeObject(this);
        File.WriteAllText($"Assets/_content/ServerData/TerrainChangeDataSO.json", data);
    }
}
