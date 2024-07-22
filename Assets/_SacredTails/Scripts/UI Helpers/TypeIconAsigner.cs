using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;

namespace Timba.SacredTails.UiHelpers
{
    public class TypeIconAsigner : MonoBehaviour, IUIHelpable
    {
        public IconContainerSO IconDatabase;

        public IconSet AssignIcon(CharacterType? charType)
        {
            return IconDatabase.AssignIcon(charType);
        }

        public bool IsReady()
        {
            throw new System.NotImplementedException();
        }
    }
}

