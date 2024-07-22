using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timba.Games.CharacterFactory;
using TMPro;

public class ElementToolTip : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    public void SetElementWeaknessInfo(CharacterType type, float multi = 0)
    {
        infoText.text = (multi <= 1.5f ? "Weak to:\n" : "Very weak to:\n") + type.ToString();
    }

    public void SetElementStrengthInfo(CharacterType type, float multi = 0)
    {
        infoText.text = (multi >= .5f ? "Strong to:\n" : "Very strong to:\n") + type.ToString();
    }

    public void SetElementTypeInfo(CharacterType type, float scale)
    {
        infoText.text = (scale > .8f ? "Main element:\n" : "Secondary element:\n") + type.ToString();
    }
}
