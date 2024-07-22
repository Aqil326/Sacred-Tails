using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    public class PositionCounter : MonoBehaviour
    {
        [SerializeField] List<Image> sprites = new List<Image>();

        public void EnablePosition(int index)
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].gameObject.SetActive(false);
            sprites[index].gameObject.SetActive(true);
        }
    }
}