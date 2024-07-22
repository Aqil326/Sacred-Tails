using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
//using Newtonsoft.Json;
//using System.IO;

public class UserNftsManager : MonoBehaviour
{
    [SerializeField]
    string contractAddress = "0x878eF413F193E0Bf8C356076C01267F48cC0dd7c";

    [SerializeField]
    string userAddress = "0x3aa9131a56f78a6ec000cf27d356cb43cfeccfe2";

    [SerializeField]
    string moralis_x_api_key = "";

    [SerializeField]
    string ipfsBaseUrl = "https://gateway.pinata.cloud/ipfs/";

    [SerializeField]
    string jsonData = "";

    [SerializeField]
    private GameObject NftInfoCellPrefab;

    [SerializeField]
    private Transform contentScroll;

    [SerializeField]
    private GameObject alertTextObj;

    public NftOwnership nftOwnership;

    private NftOwnership auxnftOwnership;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(GetNftData());
    }

    /*public void GetDataAddress(string locAddress)
    {
        userAddress = locAddress;
        StartCoroutine(GetNftData());
    }*/

    public IEnumerator GetNftData(string locAddress)
    {
        userAddress = locAddress;
        /*WWWForm dataForm = new WWWForm();
        dataForm.AddField("chain", "polygon");
        dataForm.AddField("format", "decimal");*/

        UnityWebRequest webRequest = UnityWebRequest.Get("https://deep-index.moralis.io/api/v2.2/" + userAddress + "/nft?" +
            "chain=polygon" +
            "&format=decimal" +
            "&limit=100" +
            //"&page=2" +
            "&exclude_spam=true" +
            "&token_addresses%5B0%5D=" + contractAddress +
            "&normalizeMetadata=true" +
            "&media_items=true");

        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("x-api-key", moralis_x_api_key);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Nfts Data: " + webRequest.downloadHandler.text);
            jsonData = webRequest.downloadHandler.text;
            //nftOwnership = JsonUtility.FromJson<NftOwnership>(webRequest.downloadHandler.text);
            auxnftOwnership = JsonUtility.FromJson<NftOwnership>(webRequest.downloadHandler.text);

            /*string path1 = "GeneratedShinseis/JJSON/File.json";
            File.WriteAllText(path1, JsonConvert.SerializeObject(nftOwnership, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));*/

            //int auxLimit = nftOwnership.result.Length <= 36 ? nftOwnership.result.Length : 36;
            int auxLimit = auxnftOwnership.result.Length;

            int counterNoMetadataInfo = 0;
            //foreach(nftInfo aux in nftOwnership.result)
            //for (int i = 0; i < nftOwnership.result.Length; i++)
            for (int i = 0; i < auxLimit; i++)
            {
                if (auxnftOwnership.result[i].metadata != "")
                {
                    auxnftOwnership.result[i].metadataInfo = JsonUtility.FromJson<nftMetadata>(auxnftOwnership.result[i].metadata);
                }
                else
                {
                    counterNoMetadataInfo++;
                }
                
                auxnftOwnership.result[i].metadataInfo.animation_url = BuildIpfsUrl("ipfs://", auxnftOwnership.result[i].metadataInfo.animation_url);
                auxnftOwnership.result[i].token_uri = BuildIpfsUrl("https://ipfs-metadata.moralis.io:2053/ipfs/", auxnftOwnership.result[i].token_uri);

                /*UnityWebRequest ipfsWebRequest = UnityWebRequest.Get(nftOwnership.result[i].token_uri);

                yield return ipfsWebRequest.SendWebRequest();

                if (ipfsWebRequest.result == UnityWebRequest.Result.Success)
                {
                    nftOwnership.result[i].metadataInfo = JsonUtility.FromJson<nftMetadata>(ipfsWebRequest.downloadHandler.text);
                    nftOwnership.result[i].metadataInfo.animation_url = BuildIpfsUrl("ipfs://", nftOwnership.result[i].metadataInfo.animation_url);
                }
                else
                {
                    Debug.Log("Error IPFS Data: " + ipfsWebRequest.result);
                }*/
            }

            nftOwnership.result = new nftInfo[auxLimit - counterNoMetadataInfo];
            int auxCounter = 0;
            for (int i = 0; i < auxLimit; i++)
            {
                if(auxnftOwnership.result[i].metadata != "")
                {
                    nftOwnership.result[auxCounter] = auxnftOwnership.result[i];

                    auxCounter++;
                }
            }

            //SetScrollViewInfo(); //ENABLE TO LOAD SCROLL MENU

                //Debug.Log("Real Url: " + BuildIpfsUrl(nftOwnership.result[0].token_uri));
        }
        else
        {
            Debug.Log("Error Nfts Data: " + webRequest.result);
        }

        yield return new WaitForSeconds(1);
    }

    private void SetScrollViewInfo()
    {
        int auxLimit = nftOwnership.result.Length <= 30 ? nftOwnership.result.Length : 30;
        int i = 0;
        //for (int i = 0; i < auxLimit; i++)
        while (i < auxLimit)
        {
            //GameObject instance = Instantiate(NftInfoCellPrefab, transform.position, Quaternion.identity);
            //instance.transform.parent = contentScroll;

            nftMetadata _nftMetadata = nftOwnership.result[i].metadataInfo;

            GameObject auxChild = contentScroll.GetChild(i).gameObject;

            auxChild.SetActive(true);

            string attrib = nftOwnership.result[i].metadataInfo.attributes[0].trait_type + "-" + nftOwnership.result[i].metadataInfo.attributes[0].value + " / ";

            attrib += nftOwnership.result[i].metadataInfo.attributes[1].trait_type + "-" + nftOwnership.result[i].metadataInfo.attributes[1].value + " / ";

            attrib += nftOwnership.result[i].metadataInfo.attributes[2].trait_type + "-" + nftOwnership.result[i].metadataInfo.attributes[2].value;

            attrib += " / ...";

            auxChild.GetComponent<NftsInfoCell>().SetInfo(_nftMetadata.name, _nftMetadata.dna, _nftMetadata.description, _nftMetadata.animation_url, attrib);

            //instance.GetComponent<NftsInfoCell>().SetInfo(_nftMetadata.name, _nftMetadata.dna, _nftMetadata.description, _nftMetadata.animation_url, "");
            i++;
        }

        if (i < 30)
        {
            while (i < 30)
            {
                GameObject auxChild = contentScroll.GetChild(i).gameObject;

                auxChild.SetActive(false);

                i++;
            }
        }

        if(nftOwnership.result.Length == 0)
        {
            alertTextObj.SetActive(true);
        }
        else
        {
            alertTextObj.SetActive(false);
        }
    }

    private string BuildIpfsUrl(string removeUrl, string Url)
    {
        return Url.Replace(removeUrl, ipfsBaseUrl);
    }
}

[System.Serializable]
public struct NftOwnership
{
    public nftInfo[] result;
}

[System.Serializable]
public struct nftInfo
{
    public long token_id;
    public string metadata;
    public string token_uri;
    public nftMetadata metadataInfo;
}

[System.Serializable]
public struct nftMetadata
{
    public string name;
    public string dna;
    public string description;
    public string external_url;
    public string animation_url;
    public nftattributes[] attributes;
}

[System.Serializable]
public struct nftattributes
{
    public string trait_type;
    public string display_type;
    public string value;
}