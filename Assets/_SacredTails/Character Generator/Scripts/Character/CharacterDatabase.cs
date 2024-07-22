using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using Timba.Games.CharacterFactory;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Assets/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    #region Part Addition
    [Header("PartSelector mechanic")]
    [Tooltip("Is used when is in PartSelector scene")]
    // [SerializeField] private CharacterSlot[] characterSlotPrefabs;
    // public CharacterSlot[] CharacterSlotPrefabs => characterSlotPrefabs;


    [SerializeField] private List<CharacterSlot> characterSlotPrefabsList = new List<CharacterSlot>();
    public List<CharacterSlot> CharacterSlotPrefabsList => characterSlotPrefabsList;

    public void RemoveEmptyCharacterPrefabs()
    {
        characterSlotPrefabsList = characterSlotPrefabsList.Where(x => x != null).ToList();
    }
    public void AddCharacterSlotToArray(CharacterSlot _characterSlot)
    {
        // if (Array.Exists(characterSlotPrefabs, x => x.characterID == _characterSlot.characterID))
        // {
        //     Debug.LogWarning($"The characterSlot {_characterSlot.name} couldn't be added to database character slot array because already exist. \nId to Compare: {_characterSlot.characterID}");
        //     return;
        // }
        // else
        // {
        //     int characterSlotsLength = characterSlotPrefabs.Length;
        //     Array.Resize(ref characterSlotPrefabs, characterSlotsLength + 1);
        //     characterSlotPrefabs[characterSlotPrefabs.Length - 1] = _characterSlot;
        // }

        if (characterSlotPrefabsList.Contains(_characterSlot))
        {
            Debug.LogWarning($"The characterSlot {_characterSlot.name} couldn't be added to database character slot array because already exist. \nId to Compare: {_characterSlot.characterID}");
            return;
        }
        else
        {
            int characterSlotsLength = characterSlotPrefabsList.Count;
            characterSlotPrefabsList.Add(_characterSlot);


            //AddElement();
        }
    }

    // ================================================================================================================================
    public PartType FindPartTypeInName(GameObject _referenceGameObject)
    {
        var partTypes = Enum.GetValues(typeof(PartType)) as PartType[];
        string[] value = _referenceGameObject.name.Split('_');

        foreach (var item in partTypes)
        {
            if (Array.Exists(value, x => x.ToUpper() == item.ToString().ToUpper()))
            {
                return item;
            }
        }

        Debug.LogWarning($"No found any partType in {_referenceGameObject.name} name");
        return 0;
    }
    public void FindPartSlots(GameObject _referenceGameObject, Action<GameObject> _onGetSlot)
    {
        var childs = _referenceGameObject.GetComponentsInChildren<Transform>(true);

        foreach (var item in childs)
        {
            if (item.name.IndexOf("PartSlot", StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                _onGetSlot?.Invoke(item.gameObject);
            }
        }

        // foreach (Transform child in _referenceGameObject.transform.GetChild(0))
        // {
        //     _onGetSlot?.Invoke(child.gameObject);
        // }
    }
    public void FindPartEntities(GameObject _referenceGameObject, Action<GameObject> _onGetEntity)
    {
        // var childs = _referenceGameObject.GetComponentsInChildren<Transform>(true);

        // foreach (var item in childs)
        // {
        //     if (item.name.IndexOf("PartEntity", StringComparison.CurrentCultureIgnoreCase) > 0)
        //     {
        //         _onGetEntity?.Invoke(item.gameObject);
        //     }
        // }
        foreach (Transform child in _referenceGameObject.transform)
        {
            _onGetEntity?.Invoke(child.gameObject);
        }
    }
    #endregion

    #region NFTs Creation
    [Space(15)]
    [Header("Character creation mechanic")]
    [SerializeField] private string captureImagePath;
    [SerializeField] private List<PartEntityModel> partEntityModel = new List<PartEntityModel>();

    //[SerializeField] private PartEntityModel[] partEntityModel;
    [SerializeField] private NFTsModel[] nftsModels;

    public string CaptureImagePath => captureImagePath;
    //public PartEntityModel[] PartEntityModel => partEntityModel;
    public List<PartEntityModel> PartEntityModel => partEntityModel;
    public NFTsModel[] NFTsModels => nftsModels;

    public void AddElement()
    {
        PartEntityModel _element = new PartEntityModel();
        PartEntityModel.Add(_element);
    }
    public void AddPartEntityToArray(PartType _partType, PartEntity _partEntity)
    {
        //PartEntityModel partModel = Array.Find(partEntityModel, x => x.PartType == _partType);
        PartEntityModel partModel = partEntityModel.Find(x => x.PartType == _partType);

        if (partModel != null)
        {
            partModel.AddEntity(DetermineEntityRarity(ref _partEntity), _partEntity);
        }
        else
        {
            Debug.LogWarning($"The PartEntityModel {_partEntity.name} of type {_partType.ToString()} couldn't be found");
        }
    }

    // ================================================================================================================================
    public RarityType DetermineEntityRarity(ref PartEntity _partEntity)
    {
        var rarityTypes = Enum.GetValues(typeof(RarityType)) as RarityType[];
        return rarityTypes[Random.Range(0, rarityTypes.Length)];
    }

    public NFTsModel[] GetNFT(string[] _nftsIDs)
    {
        if (nftsModels == null || nftsModels.Length == 0)
        {
            Debug.LogWarning("No exist any NFT");
            return null;
        }

        var nftsFound = new List<NFTsModel>();
        foreach (var item in _nftsIDs)
        {
            if (!string.IsNullOrEmpty(item) && nftsModels.Any(x => x.NFTsID == item))
            {
                var searchedNFT = nftsModels.First(x => x.NFTsID == item);
                nftsFound.Add(searchedNFT);
            }
        }

        return nftsFound.ToArray();
    }

    #endregion
}

[Serializable]
public class PartEntityModel
{
    private PartType partType;
    public PartType PartType => partType;

    // [SerializeField] private PartRarityModel[] partRarityModels = new PartRarityModel[] {
    //     new PartRarityModel(RarityType.common, null),
    //     new PartRarityModel(RarityType.uncommon, null),
    //     new PartRarityModel(RarityType.rare, null),
    //     new PartRarityModel(RarityType.epic, null),
    //     new PartRarityModel(RarityType.legendary, null),
    // };

    //public PartRarityModel[] PartRarityModels => partRarityModels;

    [SerializeField]
    private List<PartRarityModel> partRarityModels = new List<PartRarityModel>
    {
            new PartRarityModel(RarityType.Common, null),
            new PartRarityModel(RarityType.Uncommon, null),
            new PartRarityModel(RarityType.Rare, null),
            new PartRarityModel(RarityType.Epic, null),
            new PartRarityModel(RarityType.Legendary, null),
    };

    public List<PartRarityModel> PartRarityModels => partRarityModels;

    public void AddEntity(RarityType _rarityType, PartEntity _partEntity)
    {
        //PartRarityModel partRarityModel = Array.Find(PartRarityModels, x => x.PartRarityType == _rarityType);
        PartRarityModel partRarityModel = PartRarityModels.Find(x => x.PartRarityType == _rarityType);

        if (partRarityModel != null)
        {
            partRarityModel.AddEntity(_partEntity);
        }
    }
}

[Serializable]
public class PartRarityModel
{
    [SerializeField] private RarityType partRarityType;
    [SerializeField] private int partRarityMaxCount;
    //[SerializeField] private PartEntity[] partEntities;

    [SerializeField] private List<PartEntity> partEntities = new List<PartEntity>();

    public RarityType PartRarityType => partRarityType;
    //public PartEntity[] PartEntities => partEntities;
    public List<PartEntity> PartEntities => partEntities;
    public PartRarityModel(RarityType _partRarityType, List<PartEntity> _partEntity/*PartEntity[] _partEntity*/)
    {
        partRarityType = _partRarityType;
        partEntities = _partEntity;
    }

    public void AddEntity(PartEntity _partEntity)
    {
        if (partEntities != null)
        {
            // if (partEntities.Length > 0 && Array.Exists(partEntities, x => x.BodyPartID == _partEntity.BodyPartID))
            // {
            //     Debug.LogWarning($"The PartEntity {_partEntity.name} couldn't be added to database part entities array of {partRarityType} because already exist. \nId to Compare: {_partEntity.BodyPartID}");
            //     return;
            // }

            // int partEntityLength = partEntities.Length;
            // if (partEntityLength < partRarityMaxCount)
            // {
            //     Array.Resize(ref partEntities, partEntityLength + 1);
            //     partEntities[partEntities.Length - 1] = _partEntity;
            // }
            if (partEntities.Count > 0 && partEntities.Find(x => x.BodyPartID == _partEntity.BodyPartID))
            {
                Debug.LogWarning($"The PartEntity {_partEntity.name} couldn't be added to database part entities array of {partRarityType} because already exist. \nId to Compare: {_partEntity.BodyPartID}");
                return;
            }

            partEntities.Add(_partEntity);

            // int partEntityLength = partEntities.Count;
            // if (partEntityLength < partRarityMaxCount)
            // {
            //     partEntities.Add(_partEntity);
            // }
        }
    }


    public void RemoveEntity(PartEntity _partEntity)
    {
        if (partEntities != null)
        {
            if (_partEntity == null)
            {
                partEntities.Remove(_partEntity);
            }
        }
    }
}

