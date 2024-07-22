using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;

public static class ShinseiTypeMatrixHelper
{
    static float[,] shinseiTypeMatrix = new float[,]
    {
        {0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.0f, 2.0f, 1.0f, 0.8f},
        {1.0f, 0.5f, 1.0f, 0.0f, 2.0f, 0.5f, 2.0f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 0.8f},
        {0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 0.8f},
        {1.0f, 2.0f, 1.0f, 0.5f, 0.0f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.8f},
        {1.0f, 0.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.8f},
        {1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 0.0f, 1.0f, 0.8f},
        {2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.8f},
        {0.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.0f, 0.5f, 0.5f, 2.0f, 1.0f, 1.0f, 0.8f},
        {1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.8f},
        {2.0f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.8f},
        {1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.5f, 2.0f, 0.8f},
        {1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.0f, 0.5f, 0.8f},
        {1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.5f, 1.0f}
    };

    public static float GetShinseiTypeMultiplier(CharacterType attackType, CharacterType defenseType)
    {
        Debug.Log("Matrix called for typeeee " + attackType);
        return shinseiTypeMatrix[(int)attackType, (int)defenseType];
    }

    public static List<float> GetAllMultiplierType(CharacterType defenseType)
    {
        //Debug.Log("defenseType: " + (int)defenseType);
        List<float> auxMultipliers = new List<float>();

        for (int i = 0; i < shinseiTypeMatrix.GetLength(0); i++)
        {
            auxMultipliers.Add(shinseiTypeMatrix[i, (int)defenseType]);

            //Debug.Log("Multi: " + auxMultipliers[i]);
        }

        return auxMultipliers;
    }
}