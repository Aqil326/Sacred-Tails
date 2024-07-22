using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="ProfilePicture_style", menuName = "Timba/SacredTails/ProfilePicture_style")]
public class ProfilePictureStyle : ScriptableObject
{
    public Sprite[] picturesOptions;
    public Sprite[] framingOptions;
}