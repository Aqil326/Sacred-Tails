using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New color Palette", menuName = "Timba/Color Palett")]
public class ColorSO : ScriptableObject
{
    public Color32[] _paletteColor = new Color32[] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
}

[System.Serializable]
public class PalleteColors
{
    public string _partID;
    public Color32[] _paletteColor = new Color32[] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
}