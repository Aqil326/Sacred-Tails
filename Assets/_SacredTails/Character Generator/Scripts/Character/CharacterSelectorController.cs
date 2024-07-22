using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using Timba.Games.CharacterFactory;
using Random = UnityEngine.Random;
using Timba.SacredTails.Arena;

public class CharacterSelectorController : MonoBehaviour
{
    public Button randomButton;

    public Transform characterContainer;

    public CharacterSelector characterSelectorPrefab;
    public Transform characterSelectorContainer;

    public PartSelector partSelectorPrefab;
    public Transform partSelectorContainer;

    [SerializeField] private CharacterDatabase characterDatabase;

    private CharacterSlot currentCharacterSlot;
    private PartType[] bodyParts;
    private List<CharacterSelector> characterSelectors;
    private List<PartSelector> partSelectors;
    [Header("Inspector Only")]
    [SerializeField] private bool DoRandom;
    [SerializeField] private bool is3D;
    [SerializeField] private List<string> _uniqueID = new List<string>();

    private void Awake()
    {
        characterDatabase.RemoveEmptyCharacterPrefabs();

        bodyParts = (PartType[])Enum.GetValues(typeof(PartType));
        //characterSelectors = new List<CharacterSelector>(characterDatabase.CharacterSlotPrefabs.Length);
        characterSelectors = new List<CharacterSelector>(characterDatabase.CharacterSlotPrefabsList.Count);


        // foreach (var item in characterDatabase.CharacterSlotPrefabs)
        // {
        //     CharacterSelector characterSelector = Instantiate(characterSelectorPrefab, characterSelectorContainer);
        //     characterSelector.characterName.text = item.characterID;
        //     characterSelectors.Add(characterSelector);

        //     characterSelector.SetButtonAction(() => SelectCharacter(characterSelectors.IndexOf(characterSelector)));
        // }
        foreach (var item in characterDatabase.CharacterSlotPrefabsList)
        {
            CharacterSelector characterSelector = Instantiate(characterSelectorPrefab, characterSelectorContainer);
            characterSelector.characterName.text = item.characterID;
            characterSelectors.Add(characterSelector);

            characterSelector.SetButtonAction(() => SelectCharacter(characterSelectors.IndexOf(characterSelector)));
        }

        if (characterSelectors.Count > 0)
        {
            SelectCharacter(0);
        }

        GetCharacterUniqueID();
    }

    private void Start()
    {
        if (DoRandom)
        {
            InvokeRepeating("OnRandomSelect", 1f, 1f);
            InvokeRepeating("AutomaticRandom", 3f, 3f);
        }

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            GetCharacterUniqueID();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeBody();
    }

    private void AutomaticRandom()
    {
        SelectCharacter(Random.Range(0, (int)characterDatabase?.CharacterSlotPrefabsList.Count));
    }
    // Create a new character
    public void SelectCharacter(int _characterIndex)
    {
        // if (characterDatabase?.CharacterSlotPrefabs.Length <= _characterIndex)
        // {
        //     SacredTailsLog.LogMessage($"The length of characters is less of the index searched. \nIndex: {_characterIndex}");
        //     return;
        // }
        if (characterDatabase?.CharacterSlotPrefabsList.Count <= _characterIndex)
        {
            SacredTailsLog.LogMessage($"The length of characters is less of the index searched. \nIndex: {_characterIndex}");
            return;
        }

        if (currentCharacterSlot != null)
        {
            Destroy(currentCharacterSlot.gameObject);
        }

        // Obtiene todos los part slot
        //currentCharacterSlot = Instantiate(characterDatabase?.CharacterSlotPrefabs[_characterIndex], characterContainer);
        currentCharacterSlot = Instantiate(characterDatabase?.CharacterSlotPrefabsList[_characterIndex], characterContainer);

        // Pregunta si consiguio algo
        if (currentCharacterSlot == null)
        {
            Debug.LogWarning("Current character slot is null. Check length of Character slots prefabs in database");
            return;
        }

        currentCharacterSlot.Initialize();

        CreateCharacterPartSelector();
    }

    private void CreateCharacterPartSelector()
    {
        // Remove prev saved values
        if (partSelectors != null && partSelectors.Count > 0)
        {
            List<PartSelector> referencePartSelectors = new List<PartSelector>(partSelectors);
            foreach (var item in referencePartSelectors)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            partSelectors.Clear();
        }

        // Create PartSelectors
        partSelectors = new List<PartSelector>(bodyParts.Length);
        for (int i = 0; i < bodyParts.Length; i++)
        {
            PartSelector instantiatedPartSelector = Instantiate(partSelectorPrefab, partSelectorContainer);
            instantiatedPartSelector.selectorName.text = bodyParts[i].ToString();
            instantiatedPartSelector.SetBodyPart(bodyParts[i]);
            partSelectors.Add(instantiatedPartSelector);
        }

        // Add options to PartSelector
        foreach (var item in partSelectors)
        {
            var dummyBodyPart = Array.Find(currentCharacterSlot.PartSlots, x => x.bodyPart == item.BodyPart);
            if (dummyBodyPart != null)
            {
                // Set PartSlots ui values
                if (dummyBodyPart.Childrens.Length == 0)
                {
                    item.gameObject.SetActive(false);
                    Debug.LogWarning($"PartSlot of type {dummyBodyPart.bodyPart} no contain entities. Proceed to disable selector");
                    continue;
                }

                item.currentSelectorPart.text = dummyBodyPart.SelectedChildName;

                item.leftButton.onClick.AddListener(() => OnButtonPress(true, dummyBodyPart, item));
                item.rigthButton.onClick.AddListener(() => OnButtonPress(false, dummyBodyPart, item));
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }

        randomButton?.onClick.AddListener(OnRandomSelect);
    }

    public void OnButtonPress(bool _isLeftButton, PartSlot _dummyBodyPart, PartSelector _dummyPartSelector)
    {
        if (_dummyBodyPart != null)
        {
            var index = _dummyBodyPart.GetIndex();
            if (_isLeftButton)
            {
                index = index == 0 ? _dummyBodyPart.ChildrenLength - 1 : --index;
            }
            else
            {
                index = index == _dummyBodyPart.ChildrenLength - 1 ? 0 : ++index;
            }
            _dummyBodyPart.ActiveBodyPart(index);

            _dummyPartSelector.currentSelectorPart.text = _dummyBodyPart.SelectedChildName;
        }
    }
    public void GetCharacterUniqueID()
    {
        string uniqueID = string.Empty;
        foreach (var item in currentCharacterSlot.PartSlots)
        {
            uniqueID += item.SelectedChildName + ",";// + ColorSwapper.Instance.PaletteID;
        }
        string finalID = uniqueID;
        if (!is3D)
        {
            finalID += ColorSwapper.Instance.PaletteID;
        }
        else
        {
            finalID += ColorSwapper3D.Instance.PaletteID;
        }

        if (!_uniqueID.Contains(finalID))
        {
            _uniqueID.Add(finalID);

            TransparentBackgroundScreenshotRecorder.OnTakePhoto?.Invoke();
        }
    }
    public void OnRandomSelect()
    {
        if (currentCharacterSlot == null || currentCharacterSlot.PartSlots.Length == 0) { return; }

        foreach (var item in currentCharacterSlot.PartSlots)
        {
            item.ActiveBodyPart(Random.Range(0, item.ChildrenLength - 1));

            PartSelector searchedPartSelector = partSelectors.Find(x => x.BodyPart == item.bodyPart);
            if (searchedPartSelector != null)
            {
                searchedPartSelector.currentSelectorPart.text = item.SelectedChildName;
            }
        }
        GetCharacterUniqueID();
    }
    [SerializeField] private ShinseiStats _shinseiStats;
    private CharacterAPI characterAPI;
    private void ChangeBody()
    {
        //info from server as a json
        string dnaSample = @"{
            'Ears': '1000001000',
            'Accessory': '1000002001',
            'Body': '1000004002',
            'Head': '1000000003',
            'Tail': '1000003004'
        }";
        //parse json
        characterAPI = Newtonsoft.Json.JsonConvert.DeserializeObject<CharacterAPI>(dnaSample);

        GeneratePartFromDNA(currentCharacterSlot, PartType.Ears, characterAPI.Ears);
        _shinseiStats.Defence = SetStat(characterAPI.Accessory);
        GeneratePartFromDNA(currentCharacterSlot, PartType.Accessory, characterAPI.Accessory);
        _shinseiStats.Vigor = SetStat(characterAPI.Tail);
        GeneratePartFromDNA(currentCharacterSlot, PartType.Body, characterAPI.Body);
        _shinseiStats.Attack = SetStat(characterAPI.Ears);
        GeneratePartFromDNA(currentCharacterSlot, PartType.Head, characterAPI.Head);
        _shinseiStats.Speed = SetStat(characterAPI.Head);
        GeneratePartFromDNA(currentCharacterSlot, PartType.Tail, characterAPI.Tail);
        _shinseiStats.Stamina = SetStat(characterAPI.Body);
    }
    private void GeneratePartFromDNA(CharacterSlot p_characterSlot, PartType p_partype, long p_characterAPI)
    {
        if (!Array.Find(p_characterSlot.PartSlots, x => x.bodyPart == p_partype))
            return;
        //get each PartSlot by they PartType value
        PartSlot partSlot = Array.Find(currentCharacterSlot.PartSlots, x => x.bodyPart == p_partype);
        if (p_characterAPI > 0)
        {
            //change the bodypart
            partSlot.ActiveBodyPartByName(CharacterUtils.ParsePartDNA(p_characterAPI));
        }
        else
            SacredTailsLog.LogErrorMessage($"DNA {p_characterAPI} is not correct.");
    }
    private int SetStat(long p_characterAPI)
    {
        if (p_characterAPI > 0)
        {
            //get the shinsei stat by the partslot rarity
            int p_value = CharacterUtils.GetRarityStat(CharacterUtils.ParseRarityDNA(p_characterAPI));
            return p_value;
        }
        else
            return 0;
    }
}
#region Classes For testing purpose
[System.Serializable]
public class CharacterAPI
{
    public long Ears;
    public long Accessory;
    public long Body;
    public long Head;
    public long Tail;
}
#endregion