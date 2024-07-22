using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

namespace Timba.SacredTails.UiHelpers
{
    public interface IUIHelpable : IService
    {
        public IconSet AssignIcon(CharacterType? charType);
    }
}

