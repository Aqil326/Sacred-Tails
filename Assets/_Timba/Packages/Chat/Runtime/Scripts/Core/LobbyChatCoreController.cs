using UnityEngine;

namespace Timba.Packages.Games.ChatModule.Core
{
    public abstract class LobbyChatCoreController : MonoBehaviour
    {
        protected ILobbyChatViewable chatView;

        public abstract void Init<T>(T data);
    }
}
