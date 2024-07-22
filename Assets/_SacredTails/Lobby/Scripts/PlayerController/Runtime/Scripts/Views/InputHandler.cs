using UnityEngine;

namespace Timba.Packages.Games.PlayerControllerModule
{
    public class InputHandler : MonoBehaviour, IInputHandleable
    {
        public InputSource keyBindings;

        public void Init()
        {
            //TO DO: Init logic
        }

        public float GetHorizontalInput()
        {
            return keyBindings.SetMoveValue(keyBindings.right,keyBindings.left);
        }

        public float GetVerticalInput()
        {
            return keyBindings.SetMoveValue(keyBindings.up, keyBindings.down);
        }

        //TO DO: Interactable logic
        public bool GetInteractableInput()
        {
            return false;
        }

    }
}

