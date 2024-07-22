using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.CharacterStyle
{
    /// <summary>
    ///     This component controls the recolor of a character
    /// </summary>
    public class CharacterRecolor : MonoBehaviour
    {
        private Dictionary<PartsOfCharacter, string> materialColor = new Dictionary<PartsOfCharacter, string>() {
        {PartsOfCharacter.SKIN,"_NewColor1"},
        {PartsOfCharacter.HAIR,"_NewColor2"},
        {PartsOfCharacter.PRIMARY_COLOR,"_NewColor3"},
        {PartsOfCharacter.SECONDARY_COLOR,"_NewColor4"},
        {PartsOfCharacter.DETAILS,"_NewColor6"},
        {PartsOfCharacter.HANDS,"_NewColor5"},
        {PartsOfCharacter.LEGS,"_NewColor5"},
        //{PartsOfCharacter.PICTURE,"_NewColor1"}
    };
        private List<Material> newMaterials = new List<Material>();

        public void Init(Material target)
        {
            if (!newMaterials.Contains(target))
                newMaterials.Add(target);
            if (lastColor != null)
                ChangeMaterialColors(lastPart, lastColor);
        }
        private PartsOfCharacter lastPart;
        private Color lastColor;
        public void ChangeMaterialColors(PartsOfCharacter part, Color color)
        {
            lastPart = part;
            lastColor = color;
            foreach (var material in newMaterials)
            {
                if (materialColor.ContainsKey(part))
                {
                    material.SetColor(materialColor[part], color);
                }
            }
        }
    }
}