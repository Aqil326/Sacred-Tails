using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class NFTGenerator : MonoBehaviour {
    [Range(1, 100)]
    public int nftsToCreate = 1;

    public CharacterDatabase characterDatabase;

    [Button, LabelText("Crear NFTs")]
    public void CreateNFTs() {
        if (characterDatabase == null) {
            Debug.LogWarning($"No se tiene referencia a la base de datos");
        }

        // TODO: crear nfts de acuerdo a la base de datos
    }
}

[Serializable, ReadOnly]
public class NFTsModel {
    [SerializeField] private string nftsID;
    [SerializeField] private Sprite nftsSprite;

    [SerializeField] private PartEntity[] partEntities;

    public string NFTsID => nftsID;

    //public void GenerateTheID() {
    //    string newName = 

    //    nftsID = 
    //}
}