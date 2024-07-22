using System;
using Timba.Packages.Games.ChatModule.Structs;

namespace Timba.Packages.Games.ChatModule.Model
{
    public static class LobbyChatModel
    {
        private static object serverConnection;

        public static Action<MessageDto> OnChatMessage;
        public static void CMDSendMessage(MessagePayload message, Action<bool> callback)
        {
            if (true) //TODO: Validate connection
            {
                //TODO: Send message
                callback?.Invoke(true);
                //TODO: Delete This
                TestMessage(message.messageString, "TestNickname");
            }
        }

        //TODO: Delete This
        public static void TestMessage(string _message, string _nickname)
        {
            OnChatMessage?.Invoke(new MessageDto()
            {
                message = _message,
                nickname = _nickname
            });
        }
    }
}