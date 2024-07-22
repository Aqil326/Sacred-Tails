using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using Timba.Games.CharacterFactory;

/// <summary>
/// Index of all the current parts of the character
/// </summary>
[CreateAssetMenu(fileName = "PartIndex", menuName = "Timba/Characters/PartIndex")]
public class PartIndex : SerializedScriptableObject
{
    public List<string> accessoriesPartNames = new List<string>();
    public List<string> bodyPartNames = new List<string>();
    public List<string> earsPartNames = new List<string>();
    public List<string> headPartNames = new List<string>();
    public List<string> tailPartNames = new List<string>();
    public Dictionary<string, string> SetNames = new Dictionary<string, string>();
    //public List<long> partsDNA = new List<long>();


    [Button("Generate Random Shinsei")]
    public string GenerateRandomShinsei()
    {

        string randAccessory = accessoriesPartNames[Random.Range(0, accessoriesPartNames.Count)];
        string randBody = bodyPartNames[Random.Range(0, bodyPartNames.Count)];
        string randEars = earsPartNames[Random.Range(0, earsPartNames.Count)];
        string randHead = headPartNames[Random.Range(0, headPartNames.Count)];
        string randTail = tailPartNames[Random.Range(0, tailPartNames.Count)];
        string randColor = Random.Range(1, 13).ToString("000");

        string accessoryDna = GeneratePartDNA(randAccessory);
        string bodyDna = GeneratePartDNA(randBody);
        string earsDna = GeneratePartDNA(randEars);
        string headDna = GeneratePartDNA(randHead);
        string tailDna = GeneratePartDNA(randTail);
        string colorDna = randColor;

        string randShinseiDNA = accessoryDna + bodyDna + earsDna + headDna + tailDna + colorDna;
        colorDna = SetShinseiColor(randShinseiDNA).ToString("000");
        randShinseiDNA = accessoryDna + bodyDna + earsDna + headDna + tailDna + colorDna;
        return randShinseiDNA;
    }


    public int SetShinseiColor(string dna)
    {
        var shinseiType = GetShinseiType(dna);
        return (int)shinseiType;

    }

    [Button("get shinsei Type")]
    public CharacterType GetShinseiType(string shinseiDna)
    {
        CharacterType domType = new CharacterType();
        var parts = SeparateShinseiParts(shinseiDna, 10);
        var parTypes = GetShinseiPartTypes(shinseiDna, new CharacterType());
        var domTypes = parTypes.GroupBy(x => x.Value).Where(x => x.Count() > 1).ToList();
        domType = (CharacterType)Enum.Parse(typeof(CharacterType), parTypes["body"]);
        if (domTypes.Count == 1)
            domType = (CharacterType)Enum.Parse(typeof(CharacterType), domTypes[0].Key.ToString());

        return domType;
    }



    [Button("get shinsei Rarity")]
    public RarityType GetShinseiRarity(string shinseiDna)
    {
        int rarityIndex = 0;
        var domType = new RarityType();
        var parts = SeparateShinseiParts(shinseiDna, 10);
        var parTypes = GetShinseiPartTypes(shinseiDna, new RarityType());
        foreach (var kvp in parTypes)
        {
            var partRarity = (RarityType)Enum.Parse(typeof(RarityType), kvp.Value.ToString());
            int rarityValue = (int)partRarity;
            if (rarityValue > 4)
                rarityValue = 4;
            rarityIndex += rarityValue;
        }
        if (rarityIndex < 3)
            domType = RarityType.Common;
        else if (rarityIndex >= 3 && rarityIndex < 8)
            domType = RarityType.Uncommon;
        else if (rarityIndex >= 8 && rarityIndex < 13)
            domType = RarityType.Rare;
        else if (rarityIndex >= 13 && rarityIndex < 18)
            domType = RarityType.Epic;
        else if (rarityIndex >= 18)
            domType = RarityType.Legendary;

        return domType;
    }
    public Dictionary<string, string> GetShinseiPartTypes(string shinseiDna, Enum genEnum)
    {
        shinseiDna = ShortenString(3, shinseiDna);
        var parts = SeparateShinseiParts(shinseiDna, 10);

        Dictionary<string, string> partTypes = new Dictionary<string, string>();
        Dictionary<string, string> tempDictionary = new Dictionary<string, string>();

        var enumValues = Enum.GetValues(genEnum.GetType());
        int valueIndex = 0;
        if (genEnum.GetType() == typeof(RarityType)) 
            valueIndex = 1;
        foreach (var t in enumValues)
        {
            if (partTypes.Count == 5) 
                break;
            tempDictionary = (parts.Where(i => i.Value.Contains(t.ToString())).Select((i) => new { key = i.Key, value = i.Value.Split('_')[valueIndex] }).ToDictionary(i => i.key, i => i.value));
            tempDictionary.ToList().ForEach(x => partTypes.Add(x.Key, x.Value));
        }
        return partTypes;
    }

    public void ClearAllLists()
    {
        accessoriesPartNames.Clear();
        bodyPartNames.Clear();
        earsPartNames.Clear();
        headPartNames.Clear();
        tailPartNames.Clear();
        //partsDNA.Clear();
    }

    //generates a long int that represents the dna of each part and adds it to the partsDNA List
    private string GeneratePartDNA(string partEntityName)
    {
        string Dna = "";

        string partID = GetTypeValues(new PartType(), partEntityName);
        string typeID = GetTypeValues(new CharacterType(), partEntityName);
        string rarityID = GetTypeValues(new RarityType(), partEntityName);

        if (partID != "" && typeID != "" && rarityID != "")
        {
            //"1" is assigned to the top of the string so the leading zeros of the enum values are preserved 
            Dna = "1" + typeID + rarityID + partID;
            // partsDNA.Add(long.Parse(Dna));
        }

        return Dna;
    }

    //returns the values of the enum type that matches the part entity name 
    private string GetTypeValues(Enum genericEnum, string partName)
    {
        var enumValues = Enum.GetValues(genericEnum.GetType());
        string legendayType = "Legendary";
        int partID;
        foreach (var t in enumValues)
        {
            string tName = t.ToString();
            if (partName.Contains(tName))
            {
                if (tName.Contains(legendayType) && !partName.Split('_')[1].Equals(tName))
                    continue;

                partID = (int)t;
                return partID.ToString("D3");
            }
        }

        return "";
    }

    [Button("Parse Shinsei DNA")]
    public string ParseShinseiDNA(string shinseiDna, int colorDigits, int partDigits)
    {
        string paletteID = "_PaletteID_" + shinseiDna.Substring(shinseiDna.Length - colorDigits);
        string shinseiStructure = "";
        shinseiDna = ShortenString(colorDigits, shinseiDna);
        var parts = SeparateShinseiParts(shinseiDna, partDigits);
        if (parts.ContainsKey("accessory"))
            shinseiStructure = parts["accessory"] + "," + parts["body"] + "," + parts["ears"] + "," + parts["head"] + "," + parts["tail"] + "," + paletteID;

        return shinseiStructure;
    }

    //Gets the dna of each part
    public Dictionary<string, long> GetPartsDna(string shinseiDna, int partDigits)
    {
        shinseiDna = ShortenString(3, shinseiDna);
        Dictionary<string, long> shinseiStructure = new Dictionary<string, long>();
        shinseiStructure.Add("tail", long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits)));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("head", long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits)));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("ears", long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits)));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("body", long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits)));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("accessory", long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits)));
        return shinseiStructure;
    }

    //Separates the dna string into each shinsei part. 
    public Dictionary<string, string> SeparateShinseiParts(string shinseiDna, int partDigits)
    {
        Dictionary<string, string> shinseiStructure = new Dictionary<string, string>();
        shinseiStructure.Add("tail", ParsePartDNA(long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits))));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("head", ParsePartDNA(long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits))));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("ears", ParsePartDNA(long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits))));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("body", ParsePartDNA(long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits))));
        shinseiDna = ShortenString(partDigits, shinseiDna);
        shinseiStructure.Add("accessory", ParsePartDNA(long.Parse(shinseiDna.Substring(shinseiDna.Length - partDigits))));
        return shinseiStructure;
    }

    private string ShortenString(int amount, string input)
    {
        string output = input.Substring(0, input.Length - amount);
        return output;
    }


    [Button("ParseDNA")]
    public string ParsePartDNA(long dna)
    {

        long reducer = 1000;
        long part = dna % reducer;

        dna /= reducer;
        long rarity = dna % reducer;
        dna /= reducer;
        long type = dna % reducer;
        dna /= reducer;

        string charType = Enum.GetName(typeof(CharacterType), type);
        string rarityType = Enum.GetName(typeof(RarityType), rarity);
        string partType = Enum.GetName(typeof(PartType), part);
        string partID = charType + "_" + rarityType + "_" + partType + "_" + "PartEntity";
        return partID;
    }


    public void PopulateLists(string entity)
    {

        if (entity.Contains("Head"))
        {
            headPartNames.Add(entity);
        }
        if (entity.Contains("Accesory") || entity.Contains("Accessories") || entity.Contains("Accessory") || entity.Contains("Accesories"))
        {
            accessoriesPartNames.Add(entity);
        }
        if (entity.Contains("Body"))
        {
            bodyPartNames.Add(entity);
        }
        if (entity.Contains("Ears"))
        {
            earsPartNames.Add(entity);
        }
        if (entity.Contains("Tail"))
        {
            tailPartNames.Add(entity);
        }

        GeneratePartDNA(entity);
    }
}

[Serializable]
public class PartStat
{
    public CharacterType partType;
    public TypeStatsAndMultipliers typeStatsAndMultipliers;
    public BaseMultipliers baseMultipliers;
    public PartMultipliers partMultipliers;
}

[Serializable]
public class TypeStatsAndMultipliers
{
    public int globalPartStat;
    public float elementBonusMultiplier1;
    public float elementPenaltyMultiplier;
}

[Serializable]
public class BaseMultipliers
{
    public ShinseiStatsEnum statBonus1;
    public ShinseiStatsEnum statBonus2;
    public ShinseiStatsEnum statPenalty;

}

[Serializable]
public class PartMultipliers
{

    public float commonPartMultiplier;
    public float uncommonPartMultiplier;
    public float rarePartMultiplier;
    public float epicPartMultiplier;
    public float legendaryPartMultiplier;
}
