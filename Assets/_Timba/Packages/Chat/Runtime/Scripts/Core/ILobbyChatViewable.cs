using System;
using Timba.Packages.Games.ChatModule.Structs;

namespace Timba.Packages.Games.ChatModule.Core
{
    public interface ILobbyChatViewable
    {
        public Action<string> OnSendMessage { get; set; }
        public void Init();
        public void ShowMessage(MessageDto MessageDto);
    }
}
