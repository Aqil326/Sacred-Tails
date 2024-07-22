using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


namespace Timba.Games.SacredTails.Lobby.Chat
{

    public class ChatModule : MonoBehaviour
    {
        [Header("UI elements")]
        [SerializeField]
        private GameObject chatUI;
        [SerializeField]
        private TMP_InputField chatInput;
        [SerializeField]
        private GameObject chatPanel;

        [Header("ChatBox properties")]
        [SerializeField]
        private int maxMessages = 30;
        [SerializeField]
        private GameObject textObject;

        private List<Message> messageList = new List<Message>();

        private static event Action<string> OnMessage;

        private void Awake()
        {
            Init();
        }

        public void Init() //must get data from the scene controller
        {
            chatUI.SetActive(true);
            OnMessage += HandleNewMessage;
        }

        private void HandleNewMessage(string message) //view
        {
            Message newMessage = new Message();
            newMessage.text = message;
            GameObject newTextObject = Instantiate(textObject, chatPanel.transform);
            newMessage.textObject = newTextObject.GetComponent<TMP_Text>();
            newMessage.textObject.text = newMessage.text;
            messageList.Add(newMessage);
        }

        public void SendMessage()
        {
            if (!Input.GetKeyDown(KeyCode.Return)) { return; }
            if (string.IsNullOrEmpty(chatInput.text)) { return; }
            cmdSendMessage(chatInput.text);
            chatInput.text = string.Empty;
        }

        private void cmdSendMessage(string message) //chat controller
        {
            //TO DO: Validate message contents 
            HandleMessageOnAllClients(message);

        }

        private void HandleMessageOnAllClients(string message)
        {
            OnMessage?.Invoke(message);
        }

    }

    //view
    [System.Serializable]
    public class Message
    {
        public string text;
        public TMP_Text textObject;
    }

}
