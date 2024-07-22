using System;
using Timba.Packages.Games.ChatModule.Core;
using UnityEngine;
using TMPro;
using Timba.Packages.Games.ChatModule.Structs;

public class LobbyChatView : MonoBehaviour, ILobbyChatViewable
{
    public Action<string> OnSendMessage { get; set; }

    [SerializeField] private TMP_InputField chatInputField;

    public void Init()
    {
        chatInputField.onSubmit.AddListener(SendMessageTo);
    }

    private void SendMessageTo(string message)
    {
        Debug.Log($"Message to Send: {message}");
        if (!string.IsNullOrEmpty(message))
        {
            OnSendMessage?.Invoke(message);
        }
    }

    public void ShowMessage(MessageDto messageDto)
    {
        MessageDto dto = (MessageDto)messageDto;
        Debug.Log($"Create message to:{messageDto.nickname} and the message: {messageDto.message}");
    }
}
