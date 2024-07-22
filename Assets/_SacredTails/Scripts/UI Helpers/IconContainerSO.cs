using System;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;


namespace Timba.SacredTails.UiHelpers
{
    [CreateAssetMenu(fileName = "Icon Helpers", menuName = "Timba/SacredTails/UI/IconHelper")]
    public class IconContainerSO : ScriptableObject
    {
        public IconSet nullIcon;
        public List<IconSet> IconCollection;

        public IconSet AssignIcon(CharacterType? charType)
        {
            if (charType == null)
                return nullIcon;

            IconSet desiredIconSet = new IconSet();
            foreach (var iconSet in IconCollection)
            {
                if (iconSet.iconType == charType.Value)
                    desiredIconSet = iconSet;
            }
            return desiredIconSet;
        }
    }

    [Serializable]
    public class IconSet
    {
        public CharacterType iconType;
        public Sprite battleIcon;
        public Sprite partIcon;
        public Sprite negativeIcon;
        public Sprite backgroundSprite;
        public Color TypeColor;
    }

}
