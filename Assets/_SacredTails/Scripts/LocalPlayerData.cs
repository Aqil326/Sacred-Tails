using System;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.SacredTails.CardStoreModule;
using Timba.SacredTails.CharacterStyle;
using UnityEngine;

[Serializable]
public class LocalPlayerData
{
    public ulong localPlayerNetId;
    public string playerName;
    public string playfabId;
    public string entityId;
    public string entityType;

    public Shinsei ShinseiCompanion;
    public List<Shinsei> ShinseiParty = new List<Shinsei>();
    public ShinseiVault ShinseiVault = new ShinseiVault();
    public Deck Deck;
    public List<ChatMessagePayload> currentChatMessages = new List<ChatMessagePayload>();
    public Dictionary<PartsOfCharacter, CharacterStyleInfo> currentCharacterStyle = new Dictionary<PartsOfCharacter, CharacterStyleInfo>();
    public Dictionary<PartsOfCharacter, UnlockedCharacterStyleInfo> unlockedStyles = new Dictionary<PartsOfCharacter, UnlockedCharacterStyleInfo>();
    public string challengedPlayer = "";

    public CharacterStateEnum characterState;
    public string currentMatchId = "";

    public Action onPartyChange;

    public Dictionary<PartsOfCharacter, CharacterStyleInfo> CastCompressedStyleToDictionary(string compressedStyle)
    {
        Debug.Log("compressedStyle: " + compressedStyle);
        Dictionary<PartsOfCharacter, CharacterStyleInfo> dictionary = new Dictionary<PartsOfCharacter, CharacterStyleInfo>();

        string[] splittedString = compressedStyle.Split('-');

        /*foreach(string aux in splittedString)
        {
            Debug.Log("aux: " + aux);
        }*/

        dictionary.Add(PartsOfCharacter.SKIN, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[0]), colorHex = splittedString[1] });
        dictionary.Add(PartsOfCharacter.SECONDARY_COLOR, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[2]), colorHex = splittedString[3] });
        dictionary.Add(PartsOfCharacter.HAIR, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[4]), colorHex = splittedString[5] });
        dictionary.Add(PartsOfCharacter.PRIMARY_COLOR, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[6]), colorHex = splittedString[7] });
        dictionary.Add(PartsOfCharacter.HANDS, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[8]), colorHex = splittedString[9] });
        dictionary.Add(PartsOfCharacter.LEGS, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[10]), colorHex = splittedString[11]});
        dictionary.Add(PartsOfCharacter.DETAILS, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[12]), colorHex = splittedString[13]});

        if (splittedString.Length > 14)
        {
            dictionary.Add(PartsOfCharacter.PICTURE, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[14]), colorHex = splittedString[15] });
            dictionary.Add(PartsOfCharacter.FRAME, new CharacterStyleInfo() { presetId = Int32.Parse(splittedString[16]), colorHex = splittedString[17] });
        } else
        {
            dictionary.Add(PartsOfCharacter.PICTURE, new CharacterStyleInfo() { presetId = 0, colorHex = "FFFFFF" });
            dictionary.Add(PartsOfCharacter.FRAME, new CharacterStyleInfo() { presetId = 0, colorHex = "FFFFFF" });
        }
        /*Debug.Log("splittedString[0]: " + Int32.Parse(splittedString[0]) + ", splittedString[1]" + splittedString[1]);
        Debug.Log("splittedString[2]: " + splittedString[2] + ", splittedString[3]" + splittedString[4]);
        Debug.Log("splittedString[5]: " + splittedString[5] + ", splittedString[6]" + splittedString[6]);
        Debug.Log("splittedString[7]: " + splittedString[7] + ", splittedString[8]" + splittedString[8]);
        Debug.Log("splittedString[9]: " + splittedString[9] + ", splittedString[10]" + splittedString[10]);
        Debug.Log("splittedString[11]: " + splittedString[11] + ", splittedString[12]" + splittedString[12]);
        Debug.Log("splittedString[13]: " + splittedString[13] + ", splittedString[14]" + splittedString[14]);
        Debug.Log("splittedString[15]: " + splittedString[15] + ", splittedString[16]" + splittedString[16]);*/

        return dictionary;
    }

    public string CastDictionaryToCompressedStyle()
    {
        //Debug.Log("currentCharacterStyle.Count: " + currentCharacterStyle.Count);

        if (currentCharacterStyle.Count == 0)
            return "";
        string result = $"{currentCharacterStyle[PartsOfCharacter.SKIN].presetId}-{currentCharacterStyle[PartsOfCharacter.SKIN].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.SECONDARY_COLOR].presetId}-{currentCharacterStyle[PartsOfCharacter.SECONDARY_COLOR].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.HAIR].presetId}-{currentCharacterStyle[PartsOfCharacter.HAIR].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.PRIMARY_COLOR].presetId}-{currentCharacterStyle[PartsOfCharacter.PRIMARY_COLOR].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.HANDS].presetId}-{currentCharacterStyle[PartsOfCharacter.HANDS].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.LEGS].presetId}-{currentCharacterStyle[PartsOfCharacter.LEGS].colorHex}-";
        result += $"{currentCharacterStyle[PartsOfCharacter.DETAILS].presetId}-{currentCharacterStyle[PartsOfCharacter.DETAILS].colorHex}-";

        if(currentCharacterStyle.Count > 7)
        {
            result += $"{currentCharacterStyle[PartsOfCharacter.PICTURE].presetId}-{currentCharacterStyle[PartsOfCharacter.PICTURE].colorHex}-";
            result += $"{currentCharacterStyle[PartsOfCharacter.FRAME].presetId}-{currentCharacterStyle[PartsOfCharacter.FRAME].colorHex}";
        }
        else
        {
            result += "0-FFFFFF-";
            result += "0-FFFFFF";
        }


        //1-E7CBA5- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000
        //1-E7CBA5- 0-FFFFFF- 2-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000
        //1-E7CBA5- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000
        //1-2E1F0D- 0-5BFF87- 0-FFFFFF- 0-5BB8FF- 0-FFFFFF- 0-FFFFFF- 0-5BFFE9
        //1-E7CBA5- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF
        //1-E7CBA5- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000- 2-FFFFFF- 1-FFFFFF
        //1-E7CBA5- 0-FFFFFF- 0-000000- 0-FFFFFF- 0-FFFFFF- 0-FFFFFF- 0-000000- 2-FFFFFF- 1-FFFFFF
        return result;
    }
}

[Serializable]
public class UnlockedCharacterStyleInfo
{
    public List<int> unlockedColors = new List<int>();
    public List<int> unlockedParts = new List<int>();
}
