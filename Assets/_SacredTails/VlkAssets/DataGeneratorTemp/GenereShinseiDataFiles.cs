using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Database;
using System.IO;
using Newtonsoft.Json;

public class GenereShinseiDataFiles : MonoBehaviour
{
    [SerializeField]
    int initIndex = 0;
    [SerializeField]
    int maxIndex = 0;

    [SerializeField]
    bool useList = false;

    [SerializeField]
    List<int> indexNameList = new List<int>();

    [SerializeField]
    JsonNftMetadata jsonNftMetadata;

    string fileName = "";
    // Start is called before the first frame update
    void Start()
    {
        //fileName = Application.dataPath + "/ShinseStats.csv";
        fileName = "GeneratedShinseis/ShinseStats.csv";

        TextAsset json = (TextAsset)Resources.Load("HoardableJson/" + 0);

        jsonNftMetadata = JsonUtility.FromJson<JsonNftMetadata>(json.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunGenereData()
    {
        if (!useList)
        {
            StartCoroutine(GenereData());
        }
        else
        {
            StartCoroutine(GenereDataList());
        }
    }

    private IEnumerator GenereDataList()
    {
        TextWriter tw = new StreamWriter(fileName, false);
        tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, External_url");
        //tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, Description, External_url");
        //tw.WriteLine("Name, IpfsUrl, Dna, Description, External_url, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail");
        //tw.Close();

        var metadataOpenSea = new ShinseiMetadata();
        var metadataHoardable = new ShinseiMetadataHoardable();

        yield return new WaitForSeconds(0.5f);

        foreach (int i in indexNameList)
        {
            TextAsset json = (TextAsset)Resources.Load("HoardableJson/" + i);

            jsonNftMetadata = JsonUtility.FromJson<JsonNftMetadata>(json.ToString());

            yield return new WaitForSeconds(0.2f);

            string newShinseiDna = jsonNftMetadata.dna; //"dfg12dfg5dfg45d";//userNftsManager.nftOwnership.result[i].metadataInfo.dna;

            Shinsei newShinsei = new Shinsei()
            {
                shinseiName = jsonNftMetadata.name,//userNftsManager.nftOwnership.result[i].metadataInfo.name,
                ShinseiDna = newShinseiDna,
                ShinseiActionsIndex = new List<int>(),
                ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(newShinseiDna),
                shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(newShinseiDna),
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(newShinseiDna)

            };

            //WriteCSV(newShinsei, jsonNftMetadata);

            tw.WriteLine(jsonNftMetadata.name + "," + jsonNftMetadata.dna + "," + jsonNftMetadata.properties[0].value + "," + jsonNftMetadata.properties[1].value + "," + jsonNftMetadata.properties[2].value + "," + newShinsei.ShinseiOriginalStats.Health + "," + 200/*newShinsei.ShinseiOriginalStats.Energy*/ + "," + newShinsei.ShinseiOriginalStats.Defence + "," + newShinsei.ShinseiOriginalStats.Attack + "," + newShinsei.ShinseiOriginalStats.Stamina + "," + newShinsei.ShinseiOriginalStats.Speed + "," + newShinsei.ShinseiOriginalStats.Vigor /*"Strength"*/ + "," + jsonNftMetadata.properties[10].value + "," + jsonNftMetadata.properties[11].value + "," + jsonNftMetadata.properties[12].value + "," + jsonNftMetadata.properties[13].value + "," + jsonNftMetadata.properties[14].value + "," + jsonNftMetadata.ipfsUrl + "," + jsonNftMetadata.external_url);

            yield return new WaitForSeconds(0.2f);

            /*jsonNftMetadata.properties[3].value = newShinsei.ShinseiOriginalStats.Health.ToString();
            jsonNftMetadata.properties[4].value = 200.ToString();
            jsonNftMetadata.properties[5].value = newShinsei.ShinseiOriginalStats.Defence.ToString();
            jsonNftMetadata.properties[6].value = newShinsei.ShinseiOriginalStats.Attack.ToString();
            jsonNftMetadata.properties[7].value = newShinsei.ShinseiOriginalStats.Stamina.ToString();
            jsonNftMetadata.properties[8].value = newShinsei.ShinseiOriginalStats.Speed.ToString();
            jsonNftMetadata.properties[9].value = newShinsei.ShinseiOriginalStats.Vigor.ToString();*/

            metadataOpenSea = GetShinseiMetadata(newShinsei, jsonNftMetadata);
            metadataHoardable = GetHoardableMetadata(newShinsei, jsonNftMetadata);

            //string jsonString = JsonUtility.ToJson(jsonNftMetadata);
            //System.IO.File.WriteAllText(Application.dataPath + "/JsonGenerados/" + i + ".json", jsonString);

            //
            //string auxPathSea = Application.dataPath + "/JsonGenerados/" + i + ".json";
            string path1 = "GeneratedShinseis/JSON/OpenSea/" + i + ".json"; //string.Format(i + ".json", "GeneratedShinseis/JSON/OpenSeaSecondBatch/");
            string path2 = "GeneratedShinseis/JSON/Hoardable/" + i + ".json"; //string.Format(i + ".json", "GeneratedShinseis/JSON/HoardableSecondBatch/");

            //fileName = "GeneratedShinseis/ShinseStats.csv";
            File.WriteAllText(path1, JsonConvert.SerializeObject(metadataOpenSea, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            //File.WriteAllText(auxPath, JsonConvert.SerializeObject(jsonNftMetadata, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            File.WriteAllText(path2, JsonConvert.SerializeObject(metadataHoardable, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            yield return new WaitForSeconds(0.2f);

            Debug.Log("Json Generate: " + i);

            //yield return null;
        }

        tw.Close();
    }

    private IEnumerator GenereData()
    {
        TextWriter tw = new StreamWriter(fileName, false);
        tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, External_url");
        //tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, Description, External_url");
        //tw.WriteLine("Name, IpfsUrl, Dna, Description, External_url, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail");
        //tw.Close();

        var metadataOpenSea = new ShinseiMetadata();
        var metadataHoardable = new ShinseiMetadataHoardable();

        yield return new WaitForSeconds(0.5f);

        for (int i = initIndex; i <= maxIndex; i++)
        {
            TextAsset json = (TextAsset)Resources.Load("HoardableJson/" + i);

            jsonNftMetadata = JsonUtility.FromJson<JsonNftMetadata>(json.ToString());

            yield return new WaitForSeconds(0.2f);

            string newShinseiDna = jsonNftMetadata.dna; //"dfg12dfg5dfg45d";//userNftsManager.nftOwnership.result[i].metadataInfo.dna;

            Shinsei newShinsei = new Shinsei()
            {
                shinseiName = jsonNftMetadata.name,//userNftsManager.nftOwnership.result[i].metadataInfo.name,
                ShinseiDna = newShinseiDna,
                ShinseiActionsIndex = new List<int>(),
                ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(newShinseiDna),
                shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(newShinseiDna),
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(newShinseiDna)

            };

            //WriteCSV(newShinsei, jsonNftMetadata);

            tw.WriteLine(jsonNftMetadata.name + "," + jsonNftMetadata.dna + "," + jsonNftMetadata.properties[0].value + "," + jsonNftMetadata.properties[1].value + "," + jsonNftMetadata.properties[2].value + "," + newShinsei.ShinseiOriginalStats.Health + "," + 200/*newShinsei.ShinseiOriginalStats.Energy*/ + "," + newShinsei.ShinseiOriginalStats.Defence + "," + newShinsei.ShinseiOriginalStats.Attack + "," + newShinsei.ShinseiOriginalStats.Stamina + "," + newShinsei.ShinseiOriginalStats.Speed + "," + newShinsei.ShinseiOriginalStats.Vigor /*"Strength"*/ + "," + jsonNftMetadata.properties[10].value + "," + jsonNftMetadata.properties[11].value + "," + jsonNftMetadata.properties[12].value + "," + jsonNftMetadata.properties[13].value + "," + jsonNftMetadata.properties[14].value + "," + jsonNftMetadata.ipfsUrl + "," + jsonNftMetadata.external_url);

            yield return new WaitForSeconds(0.2f);

            /*jsonNftMetadata.properties[3].value = newShinsei.ShinseiOriginalStats.Health.ToString();
            jsonNftMetadata.properties[4].value = 200.ToString();
            jsonNftMetadata.properties[5].value = newShinsei.ShinseiOriginalStats.Defence.ToString();
            jsonNftMetadata.properties[6].value = newShinsei.ShinseiOriginalStats.Attack.ToString();
            jsonNftMetadata.properties[7].value = newShinsei.ShinseiOriginalStats.Stamina.ToString();
            jsonNftMetadata.properties[8].value = newShinsei.ShinseiOriginalStats.Speed.ToString();
            jsonNftMetadata.properties[9].value = newShinsei.ShinseiOriginalStats.Vigor.ToString();*/

            metadataOpenSea = GetShinseiMetadata(newShinsei, jsonNftMetadata);
            metadataHoardable = GetHoardableMetadata(newShinsei, jsonNftMetadata);

            //string jsonString = JsonUtility.ToJson(jsonNftMetadata);
            //System.IO.File.WriteAllText(Application.dataPath + "/JsonGenerados/" + i + ".json", jsonString);

            //
            //string auxPathSea = Application.dataPath + "/JsonGenerados/" + i + ".json";
            string path1 = "GeneratedShinseis/JSON/OpenSea/" + i + ".json"; //string.Format(i + ".json", "GeneratedShinseis/JSON/OpenSeaSecondBatch/");
            string path2 = "GeneratedShinseis/JSON/Hoardable/" + i + ".json"; //string.Format(i + ".json", "GeneratedShinseis/JSON/HoardableSecondBatch/");

            //fileName = "GeneratedShinseis/ShinseStats.csv";
            File.WriteAllText(path1, JsonConvert.SerializeObject(metadataOpenSea, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            //File.WriteAllText(auxPath, JsonConvert.SerializeObject(jsonNftMetadata, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            File.WriteAllText(path2, JsonConvert.SerializeObject(metadataHoardable, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            yield return new WaitForSeconds(0.2f);

            Debug.Log("Json Generate: " + i);

            //yield return null;
        }

        tw.Close();
    }

    public ShinseiMetadata GetShinseiMetadata(Shinsei shinsei, JsonNftMetadata jsonNftMetadata)
    {

        var shinseiAtributes = GetShinseiAtributes(shinsei, jsonNftMetadata);

        ShinseiMetadata generatedMetadata = new ShinseiMetadata()
        {
            name = shinsei.shinseiName,
            dna = shinsei.ShinseiDna,
            description = jsonNftMetadata.description,
            external_url = jsonNftMetadata.external_url,
            //image = "ipfs url here/" + shinsei.shinseiName + ".png",
            animation_url = "ipfs://" + "QmSsbdTYLJH3Cp5Sk5CnFcXTzmCyyMjYi5NUekSZY1M6Bo" + "/" + shinsei.shinseiName + ".mp4",
            //animation_url = "ipfs://" + ipfsCID + "/" + shinsei.shinseiName + ".mp4",
            attributes = shinseiAtributes
        };

        return generatedMetadata;

    }

    public List<ShinseiAtributes> GetShinseiAtributes(Shinsei shinsei, JsonNftMetadata jsonNftMetadata)
    {
        //shinsei.ShinseiOriginalStats = indexDatabase.GetShinseiStats(shinsei.ShinseiDna);
        //var partsRarity = indexDatabase.GetShinseiPartTypes(shinsei.ShinseiDna, new RarityType());
        //var partsType = indexDatabase.GetShinseiPartTypes(shinsei.ShinseiDna, new CharacterType());


        List<ShinseiAtributes> shinseiAtributes = new List<ShinseiAtributes> {
            //overall data
            new ShinseiAtributes {trait_type = "Type", value = jsonNftMetadata.properties[0].value },
            new ShinseiAtributes {trait_type = "Tier", value = jsonNftMetadata.properties[1].value },
            new ShinseiAtributes {trait_type = "Generation", value = jsonNftMetadata.properties[2].value },
            //stats
            new ShinseiAtributes {trait_type = "Health", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Health },
            new ShinseiAtributes {trait_type = "Energy", display_type = "number", value = 200/*(int)shinsei.ShinseiOriginalStats.Energy*/ },
            new ShinseiAtributes {trait_type = "Defence", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Defence },
            new ShinseiAtributes {trait_type = "Attack", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Attack },
            new ShinseiAtributes {trait_type = "Stamina", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Stamina },
            new ShinseiAtributes {trait_type = "Strength", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Vigor },
            new ShinseiAtributes {trait_type = "Speed", display_type = "number", value = (int)shinsei.ShinseiOriginalStats.Speed },
            //parts
            new ShinseiAtributes {trait_type = "Accessory", value = jsonNftMetadata.properties[10].value},
            new ShinseiAtributes {trait_type = "Head", value = jsonNftMetadata.properties[11].value},
            new ShinseiAtributes {trait_type = "Body", value = jsonNftMetadata.properties[12].value},
            new ShinseiAtributes {trait_type = "Ears", value = jsonNftMetadata.properties[13].value},
            new ShinseiAtributes {trait_type = "Tail", value = jsonNftMetadata.properties[14].value},
            //movements
            //new ShinseiAtributes {trait_type = "MoveList", value = shinsei.ShinseiActionsIndex},
        };

        return shinseiAtributes;
    }

    public ShinseiMetadataHoardable GetHoardableMetadata(Shinsei shinsei, JsonNftMetadata jsonNftMetadata)
    {
        var shinseiPorperties = GetShinseiAtributesHoardables(shinsei, jsonNftMetadata);
        ShinseiMetadataHoardable generatedMetadata = new ShinseiMetadataHoardable()
        {
            name = shinsei.shinseiName,
            ipfsUrl = "https://ipfs.io/ipfs/" + "QmSsbdTYLJH3Cp5Sk5CnFcXTzmCyyMjYi5NUekSZY1M6Bo" + "/" + shinsei.shinseiName + ".mp4",
            //ipfsUrl = "https://ipfs.io/ipfs/" + ipfsCID + "/" + shinsei.shinseiName + ".mp4",
            description = jsonNftMetadata.description,
            external_url = jsonNftMetadata.external_url,
            properties = shinseiPorperties,
            dna = shinsei.ShinseiDna
        };

        return generatedMetadata;
    }

    public List<ShinseiAtributesHoardable> GetShinseiAtributesHoardables(Shinsei shinsei, JsonNftMetadata jsonNftMetadata)
    {
        //shinsei.ShinseiOriginalStats = indexDatabase.GetShinseiStats(shinsei.ShinseiDna);
        //var partsRarity = indexDatabase.GetShinseiPartTypes(shinsei.ShinseiDna, new RarityType());
        //var partsType = indexDatabase.GetShinseiPartTypes(shinsei.ShinseiDna, new CharacterType());


        List<ShinseiAtributesHoardable> shinseiAtributes = new List<ShinseiAtributesHoardable> {
            //overall data
            new ShinseiAtributesHoardable {name = "Type", value = jsonNftMetadata.properties[0].value },
            new ShinseiAtributesHoardable {name = "Tier", value = jsonNftMetadata.properties[1].value },
            new ShinseiAtributesHoardable {name = "Generation", value = jsonNftMetadata.properties[2].value },
            //stats
            new ShinseiAtributesHoardable {name = "Health", value = (int)shinsei.ShinseiOriginalStats.Health },
            new ShinseiAtributesHoardable {name = "Energy", value = 200/*(int)shinsei.ShinseiOriginalStats.Energy*/ },
            new ShinseiAtributesHoardable {name = "Defence", value = (int)shinsei.ShinseiOriginalStats.Defence },
            new ShinseiAtributesHoardable {name = "Attack", value = (int)shinsei.ShinseiOriginalStats.Attack },
            new ShinseiAtributesHoardable {name = "Stamina", value = (int)shinsei.ShinseiOriginalStats.Stamina },
            new ShinseiAtributesHoardable {name = "Speed", value = (int)shinsei.ShinseiOriginalStats.Speed },
            new ShinseiAtributesHoardable {name = "Strength", value = (int)shinsei.ShinseiOriginalStats.Vigor },

            //parts
            new ShinseiAtributesHoardable {name = "Accessory", value = jsonNftMetadata.properties[10].value},
            new ShinseiAtributesHoardable {name = "Head", value = jsonNftMetadata.properties[11].value},
            new ShinseiAtributesHoardable {name = "Body", value = jsonNftMetadata.properties[12].value},
            new ShinseiAtributesHoardable {name = "Ears", value = jsonNftMetadata.properties[13].value},
            new ShinseiAtributesHoardable {name = "Tail", value = jsonNftMetadata.properties[14].value},
            //movements
            //new ShinseiAtributesHoardable {name = "MoveList", value = shinsei.ShinseiActionsIndex},
        };

        return shinseiAtributes;
    }

    /*private void WriteCSV(Shinsei newShinsei, JsonNftMetadata jsonNftMetadata)
    {
        TextWriter tw = new StreamWriter(fileName, false);
        tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, External_url");
        //tw.WriteLine("Name, Dna, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail, IpfsUrl, Description, External_url");
        //tw.WriteLine("Name, IpfsUrl, Dna, Description, External_url, Type, Tier, Generation, Health, Energy, Defence, Attack, Stamina, Speed, Strength, Accessory, Head, Body, Ears, Tail");
        tw.Close();

        TextWriter tw = new StreamWriter(fileName, true);

        tw.WriteLine(jsonNftMetadata.name + "," + jsonNftMetadata.dna + "," + jsonNftMetadata.properties[0].value + "," + jsonNftMetadata.properties[1].value + "," + jsonNftMetadata.properties[2].value + "," + newShinsei.ShinseiOriginalStats.Health + "," + 200 + "," + newShinsei.ShinseiOriginalStats.Defence + "," + newShinsei.ShinseiOriginalStats.Attack + "," + newShinsei.ShinseiOriginalStats.Stamina + "," + newShinsei.ShinseiOriginalStats.Speed + "," + newShinsei.ShinseiOriginalStats.Vigor + "," + jsonNftMetadata.properties[10].value + "," + jsonNftMetadata.properties[11].value + "," + jsonNftMetadata.properties[12].value + "," + jsonNftMetadata.properties[13].value + "," + jsonNftMetadata.properties[14].value + "," + jsonNftMetadata.ipfsUrl + "," + jsonNftMetadata.external_url);
        //tw.WriteLine(jsonNftMetadata.name + "," + jsonNftMetadata.dna + "," + jsonNftMetadata.properties[0] + "," + jsonNftMetadata.properties[1] + "," + jsonNftMetadata.properties[0] + "," + newShinsei.ShinseiOriginalStats.Health + "," + newShinsei.ShinseiOriginalStats.Energy + "," + newShinsei.ShinseiOriginalStats.defence + "," + newShinsei.ShinseiOriginalStats.attack + "," + newShinsei.ShinseiOriginalStats.stamina + "," + newShinsei.ShinseiOriginalStats.speed + "," + newShinsei.ShinseiOriginalStats.vigor + "," + jsonNftMetadata.properties[10] + "," + jsonNftMetadata.properties[11] + "," + jsonNftMetadata.properties[12] + "," + jsonNftMetadata.properties[13] + "," + jsonNftMetadata.properties[14] + "," + jsonNftMetadata.ipfsUrl + "," + jsonNftMetadata.description + "," + jsonNftMetadata.external_url);
        //tw.WriteLine(jsonNftMetadata.name + "," + jsonNftMetadata.dna + "," + jsonNftMetadata.properties[0] + "," + jsonNftMetadata.properties[1] + "," + jsonNftMetadata.properties[0] + "," + "Health" + "," + "Energy" + "," + "Defence" + "," + "Attack" + "," + "Stamina" + "," + "Speed" + "," + "Strength" + "," + jsonNftMetadata.properties[10] + "," + jsonNftMetadata.properties[11] + "," + jsonNftMetadata.properties[12] + "," + jsonNftMetadata.properties[13] + "," + jsonNftMetadata.properties[14] + "," + jsonNftMetadata.ipfsUrl + "," + jsonNftMetadata.description + "," + jsonNftMetadata.external_url);
        //tw.WriteLine("Name" + "," + "Dna" + "," + "Type" + "," + "Tier" + "," + "Generation" + "," + "Health" + "," + "Energy" + "," + "Defence" + "," + "Attack" + "," + "Stamina" + "," + "Speed" + "," + "Strength" + "," + "Accessory" + "," + "Head" + "," + "Body" + "," + "Ears" + "," + "Tail" + "," + "IpfsUrl" + "," + "Description" + "," + "External_url");

        tw.Close();
    }*/
}

[System.Serializable]
public struct JsonNftMetadata
{
    public string name;
    public string ipfsUrl;
    public string dna;
    public string description;
    public string external_url;
    public JsonNftAttributes[] properties;
}

[System.Serializable]
public struct JsonNftAttributes
{
    public string name;
    public string value;
}


//__________________________________________

[System.Serializable]
public class ShinseiMetadata
{
    public string name;
    public string dna;
    public string description;
    public string external_url;
    public string image;
    public string animation_url;
    public List<ShinseiAtributes> attributes;
}

[System.Serializable]
public class ShinseiMetadataHoardable
{
    public string name;
    public string ipfsUrl;
    public string dna;
    public string description;
    public string external_url;
    public List<ShinseiAtributesHoardable> properties;
}

[System.Serializable]
public class ShinseiAtributes
{
    public string trait_type;
    public string display_type;
    public object value;
}


[System.Serializable]
public class ShinseiAtributesHoardable
{
    public string name;
    public object value;
}