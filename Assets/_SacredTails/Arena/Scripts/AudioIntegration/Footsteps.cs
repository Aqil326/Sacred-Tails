using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Simple behavior that play music acording to the animation
/// </summary>
public class Footsteps : MonoBehaviour
{
    public AK.Wwise.Bank MyBank = null;
    public AK.Wwise.Event MyEvent = null;

    Dictionary<string, string> TextureNames = new Dictionary<string, string>()
    {
        {"Wood","F_Wood"},
        {"Grass_ground_layer","F_Grass"},
        {"Soil_ground_layer","F_Dirt"},
        {"Layer_Sand","F_Dirt"},
        {"Layer_Rock","F_Rock"},
        {"BrickFloor","F_Wood"},
        {"Layer_Cobblestone","F_Rock"}
    };

    public void Start()
    {
        MyBank.Load();
    }

    public void PlayFootSound()
    {
        DetectSurface();
        MyEvent.Post(gameObject);
    }

    public string CurrentTerrain;

    public void DetectSurface()
    {
        Ray ray = new Ray(transform.position, transform.up * -1);
        RaycastHit result;
        Physics.Raycast(ray, out result, 1, LayerMask.GetMask("Enviroment"));
        if (result.collider != null)
        {
            Terrain targetTerrain = result.collider.gameObject.GetComponent<Terrain>();
            if (targetTerrain != null)
            {
                Vector3 terrainPosition = result.point - targetTerrain.transform.position;
                Vector3 splatMapPosition = new Vector3(
                    terrainPosition.x / targetTerrain.terrainData.size.x,
                    0,
                    terrainPosition.z / targetTerrain.terrainData.size.z
                    );

                int x = Mathf.FloorToInt(splatMapPosition.x * targetTerrain.terrainData.alphamapWidth);
                int z = Mathf.FloorToInt(splatMapPosition.z * targetTerrain.terrainData.alphamapHeight);

                float[,,] alphaMap = targetTerrain.terrainData.GetAlphamaps(x, z, 1, 1);
                int primaryIndex = 0;
                for (int i = 0; i < alphaMap.Length; i++)
                {
                    if (alphaMap[0,0,i] > alphaMap[0,0,primaryIndex])
                    {
                        primaryIndex = i;
                    }
                }
                string textureName = targetTerrain.terrainData.terrainLayers[primaryIndex].name;
                CurrentTerrain = textureName;
                SetSwitchUsingNames(textureName);
            }
        }
        else
        {
            Physics.Raycast(ray, out result, 1);
            if (result.collider != null)
            {
                if (result.collider.gameObject.layer == LayerMask.NameToLayer("Rock"))
                {
                    SetSwitchUsingNames("Layer_Rock");
                    CurrentTerrain = "Rock";
                }
                else if (result.collider.gameObject.layer == LayerMask.NameToLayer("Wood"))
                {
                    CurrentTerrain = "Underwood";
                    SetSwitchUsingNames("Wood");
                }
                else
                {
                    CurrentTerrain = "Underwood";
                    SetSwitchUsingNames("Wood");
                }
            }
        }
    }

    public void SetSwitchUsingNames(string name)
    {
        AkSoundEngine.SetSwitch("Footsteps", TextureNames[name], gameObject);
    }
}

