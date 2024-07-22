using UnityEngine;

namespace Timba.Packages.Games.PlayerControllerModule {
    [CreateAssetMenu(fileName = "Keybindings", menuName = "Character Controller Module/KeyBindings")]
    public class InputSource : ScriptableObject
    {
        public KeyCode[] right;
        public KeyCode[] left;
        public KeyCode[] up;
        public KeyCode[] down;
        public KeyCode[] interact;


        public float SetMoveValue(KeyCode[] aSet, KeyCode[] bSet)
        {
            foreach (var aKey in aSet)
            {
                foreach (var bKey in bSet)
                {
                    if (Input.GetKey(aKey))
                    {
                        return 1;
                    }
                    else if (Input.GetKey(bKey))
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }

        public bool SetInteractValue()
        {
            //TO DO: interaction logic
            return false;
        }
    }
}

