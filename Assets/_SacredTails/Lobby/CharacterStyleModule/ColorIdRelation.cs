using System.Collections.Generic;
using Timba.SacredTails.CharacterStyle;
using UnityEngine;

[System.Serializable]
public class ColorIdRelation : CharacterStyleRelation
{
    public List<PartsOfCharacter> usableOnParts;
    public Color color;
}

public class CharacterStyleRelation
{
    public int id;
}
