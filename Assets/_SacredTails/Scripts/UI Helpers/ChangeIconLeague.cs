using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangeIconLeague : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    public void ChangeIconUsingIndex(int index)
    {
        icon.sprite = sprites[index];
    }
}
