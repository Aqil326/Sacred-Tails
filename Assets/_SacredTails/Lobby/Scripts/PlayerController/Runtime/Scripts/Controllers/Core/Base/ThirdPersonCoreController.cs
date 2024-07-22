using UnityEngine;
using Unity.Netcode;

namespace Timba.Packages.Games.PlayerControllerModule.Core
{
    public abstract class ThirdPersonCoreController : MonoBehaviour
    {
        protected IInputHandleable inputHandler;

        public abstract void Init<T>(T data);
    }
}

