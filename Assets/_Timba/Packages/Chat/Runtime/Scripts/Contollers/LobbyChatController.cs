using System;
using Timba.Packages.Games.ChatModule.Core;
using Timba.Packages.Games.ChatModule.Model;
using Timba.Packages.Games.ChatModule.Structs;
using UnityEngine;

namespace Timba.Packages.Games.ChatModule.Controller
{
    public class LobbyChatController : LobbyChatCoreController
    {
        private string userId;
        //TODO: Delete This.
        private void Awake()
        {
            Init<object>(null);
        }

        public override void Init<T>(T data)
        {
            if (chatView == null)
            {
                if (!TryGetComponent<ILobbyChatViewable>(out chatView))
                {
                    Debug.LogWarning($"Something wrong ILobbyChatViewable not found.");
                }
                else
                {
                    userId = "ThisisarealUserId";
                    chatView.Init();
                    chatView.OnSendMessage += OnSendMessage;
                    LobbyChatModel.OnChatMessage += OnChatMessage;
                }
            }
        }

        private void OnChatMessage(MessageDto messageDto)
        {
            chatView.ShowMessage(messageDto);
        }

        private void OnSendMessage(string message)
        {
            //TODO: Validate message and send to Model

            LobbyChatModel.CMDSendMessage(new Structs.MessagePayload()
            {
                messageString = message,
                userId = userId
            }, (success) =>
            {
                if (success)
                {
                    Debug.Log($"Controller said: All works");
                }
                else
                {
                    Debug.LogError("Controller said: FAIL");
                }
            });
        }
    }
}