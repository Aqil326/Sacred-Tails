using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Collections;
using Timba.SacredTails.Arena;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Timba.Games.CharacterFactory;

[CreateAssetMenu(fileName = "BattleTerrainDataSO", menuName = "Timba/SacredTails/TerrainChange/BattleTerrainDataSO ")]
[System.Serializable]
public class BattleTerrainDataSO : SerializedScriptableObject
{
    public TypesOfTerrainEnum terrainType;
    public ActionDataList globalActions;
    public List<TypesActions> typesActions;
    [JsonIgnore] [TextArea] public string displayMessage;

    [Button("Generate JSON")]
    public void GetJsonActionCards()
    {
        string data = JsonConvert.SerializeObject(this);
        File.WriteAllText($"Assets/_content/ServerData/BattleTerrain{terrainType}SO.json", data);
    }
}

[System.Serializable]
public class TypesActions
{
    public CharacterType typeOfShinsei;
    public ActionDataList actionsData;
}
