using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script used for managing the visuals of the shinsei's that are alive
/// </summary>
public class AliveShinseiPreviewer : MonoBehaviour
{
    // in the order of the shinseis being index 0 the first shinsei
    [SerializeField] private GameObject[] playersAliveShinseisPreview;
    [SerializeField] private GameObject[] opponentAliveShinseisPreview;

    public void SetShinseiAliveState(bool isPlayer, int shinseiIndex, bool isAlive = false)
    {
        Debug.Log("Shinsei index : " + shinseiIndex);
        if (isPlayer)
        {
            playersAliveShinseisPreview[shinseiIndex].SetActive(isAlive);
        }
        else
        {
            opponentAliveShinseisPreview[shinseiIndex].SetActive(isAlive);
        }
    }
}
