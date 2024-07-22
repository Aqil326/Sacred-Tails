using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.CharacterStyle
{
    /// <summary>
    ///     This component allow you to define the color of the NPC in the game
    /// </summary>
    public class BakeColorsInCharacter : MonoBehaviour
    {
        public CharacterRecolor characterRecolor;
        public List<MaterialReskin> materialReskins;
        public BodyStyle bodyStyle;
        public Color Skin, Hair, Primary, Secondary, Details;
        [Range(0, 3)]
        public int HairStyle;

        private void Start()
        {
            foreach (var material in materialReskins)
            {
                material.InitReskin();
            }
            UpdateVisual();
        }

        [ContextMenu("UpdateVisual")]
        public void UpdateVisual()
        {
            if (bodyStyle != null)
                bodyStyle.bodyParts[0].SelectObject(HairStyle);
            foreach (var item in materialReskins)
            {
                item.ChangePart(HairStyle, 4);
            }
            characterRecolor.ChangeMaterialColors(PartsOfCharacter.SKIN, Skin);
            characterRecolor.ChangeMaterialColors(PartsOfCharacter.HAIR, Hair);
            characterRecolor.ChangeMaterialColors(PartsOfCharacter.PRIMARY_COLOR, Primary);
            characterRecolor.ChangeMaterialColors(PartsOfCharacter.SECONDARY_COLOR, Secondary);
            characterRecolor.ChangeMaterialColors(PartsOfCharacter.DETAILS, Details);
        }
    }
}