using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using Timba.Games.SacredTails.LobbyDatabase;
using UnityEngine.EventSystems;
using System.Globalization;
using Timba.Games.SacredTails.LobbyNetworking;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine.UI;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
/// <summary>
/// Chat behavior, send messages and show in a UI Elements
/// </summary>
public class ChatTextBox : TextboxBase
{
    #region ----Fields----
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private CanvasGroup chatGroup;
    [HideInInspector] public bool isReady = false;
    float targetValue = 0, currentValue = 0;
    bool toggle = true;
    public EventSystem system;
    ThirdPersonController thirdPersonController;
    [SerializeField] GameObject chatButton;
    public GameObject filter;

    public Action OnStartEditing, OnEndEditing;
    public ChatBadWordsDB badWordsDB;
    #endregion ----Fields----

    #region ----Methods----
    #region ----Init----
    private void Awake()
    {
        TextAsset badWordsJson = (TextAsset)Resources.Load("BadWordJsonConverted");
        badWordsDB = JsonConvert.DeserializeObject<ChatBadWordsDB>(badWordsJson.text);
    }
    #endregion ----Init----

    #region ----ChatMessages----

    public void CallStartEditing()
    {
        OnStartEditing?.Invoke();
    }

    public void CallEndEditing()
    {
        OnEndEditing?.Invoke();
    }

    public void ToggleShowFilter()
    {
        filter.gameObject.SetActiveToggle();
    }

    public void Update()
    {
        if (!isReady)
            return;
        else
            thirdPersonController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;

        if (Input.GetKeyDown(KeyCode.Return) && isReady)
        {
            if (string.IsNullOrEmpty(chatInput.text))
            {
                ShowHide(toggle);
                return;
            }
            else
            {
                SendLocalMessage();
                return;
            }
        }
        if (system.currentSelectedGameObject != chatInput.gameObject)
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ShowHide(toggle);
                return;
            }
        if (currentValue != targetValue)
        {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, 5 * Time.deltaTime);
            chatGroup.alpha = currentValue;
        }
    }

    public void DisableMovement(bool newState)
    {
        if (thirdPersonController != null)
            thirdPersonController.IsMovementBloqued = newState;
    }

    public void ShowHide(bool state)
    {
        if (state)
        {
            system.SetSelectedGameObject(chatInput.gameObject, new BaseEventData(system));
            targetValue = 1;
            chatButton.gameObject.SetActive(false);
        }
        else
        {
            system.SetSelectedGameObject(null, new BaseEventData(system));
            targetValue = 0;
            chatButton.gameObject.SetActive(true);
        }
        toggle = !state;
    }

    public void SendLocalMessage()
    {
        if (String.IsNullOrWhiteSpace(chatInput.text))
            return;

        AkSoundEngine.PostEvent("U_Text_Send", gameObject);

        if (PlayerPrefs.GetInt("BadWordFilterOption", 1) == 1)
            chatInput.text = CheckForBadWords(chatInput.text);
        if (!chatInput.text.Contains("/r "))
            AddText($"<color=#DFDBC0>[Server] ({PlayerDataManager.Singleton.localPlayerData.playerName}): {chatInput.text}</color>");
        else
        {
            string[] trimText = chatInput.text.Split(' ');
            AddText($"<color=#E146CD>[To] ({trimText[1]}): {(chatInput.text).Replace("/r " + trimText[1],"")}</color>");
            PlayerPrefs.SetString("LastWhisper", trimText[1]);
        }
        PlayerDataManager.Singleton.localPlayerData.currentChatMessages.Add(new ChatMessagePayload() { message = chatInput.text, timeStamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) });
        chatInput.text = "";

        chatInput.Select();
        chatInput.ActivateInputField();
    }

    public List<string> alreadyWriteMessages = new List<string>();
    public void SendMessage(ChatMessagePayload chatMessage, string displayName, bool isAdmin = false)
    {
        if (alreadyWriteMessages.Contains(chatMessage.id))
            return;

        if (PlayerPrefs.GetInt("BadWordFilterOption", 1) == 1)
            chatMessage.message = CheckForBadWords(chatMessage.message);

        alreadyWriteMessages.Add(chatMessage.id);
        if (isAdmin)
            AddText($"<color=red>[ADMIN] </color>: {chatMessage.message}");
        else
            AddText($"<color=#EFEBCE>[Server] ({displayName}): {chatMessage.message}</color>");

    }

    public void SendMessage(ChatMessagePayload chatMessage,string from)
    {
        if (PlayerPrefs.GetInt("BadWordFilterOption", 1) == 1)
            chatMessage.message = CheckForBadWords(chatMessage.message);
        AddText($"<color=#E146CD>[Whisper]({from}): {chatMessage.message}</color>");
        PlayerPrefs.SetString("LastWhisper", from);
    }

    public string CheckForBadWords(string message)
    {
        string[] messageWords = message.Split(' ');

        for (int i = 0; i < messageWords.Length; i++)
        {
            string word = messageWords[i];
            foreach (List<string> languageBadWords in badWordsDB.listOfBadWords)
            {
                if (languageBadWords.Contains(word.ToLower()) )
                {
                    messageWords[i] = "****";
                    break;
                }
            }
        }

        string resultAfterFilter = "";
        foreach (var word in messageWords)
            resultAfterFilter += $"{word} ";

        return resultAfterFilter;
    }
    #endregion ----ChatMessages----

    #region ----Helpers----
    // Start is called before the first frame update
    [ContextMenu("Convert BWJson")]
    private void ConvertBadWordJson()
    {
        string jsonBadWordDB = File.ReadAllText(Application.dataPath + "/Resources/BadWordJson.json").ToString();
        string pattern1 = @"""Banned Words"": """",";
        string pattern2 = @""""": """",";
        string pattern3 = @"""__\d*"":";

        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern1, "");
        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern2, "");
        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern3, "");

        string pattern4 = @"{";
        string pattern5 = @"}";
        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern4, "[");
        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern5, "]");

        string pattern6 = @"^\s+$[\r\n]*";
        jsonBadWordDB = Regex.Replace(jsonBadWordDB, pattern6, string.Empty, RegexOptions.Multiline);
        jsonBadWordDB = jsonBadWordDB.Replace("\"\"", "");
        jsonBadWordDB = @"{""listOfBadWords"":" + jsonBadWordDB + "}";
        File.WriteAllText(Application.dataPath + "/Resources/BadWordJsonConverted.json", jsonBadWordDB);
    }
    #endregion ----Helpers----
    #endregion ----Methods----
}

public struct ChatBadWordsDB
{
    public List<List<string>> listOfBadWords;
}
