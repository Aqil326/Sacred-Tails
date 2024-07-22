using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    public class BackgroundTypeSwapper : MonoBehaviour
    {
        [PreviewField]
        [SerializeField] List<Sprite> types = new List<Sprite>();

        public void CallByShinseiType(Image image, CharacterType shinseiType)
        {
            Sprite result = types[0];
            switch (shinseiType)
            {
                case CharacterType.Sky:
                    result = types[1];
                    break;
                case CharacterType.Sun:
                    result = types[0];
                    break;
                case CharacterType.Dark:
                    result = types[7];
                    break;
                case CharacterType.Ocean:
                    result = types[2];
                    break;
                case CharacterType.Nature:
                    result = types[6];
                    break;
                case CharacterType.Light:
                    result = types[8];
                    break;
                case CharacterType.Snow:
                    result = types[3];
                    break;
                case CharacterType.Earth:
                    result = types[4];
                    break;
                case CharacterType.Mecha:
                    result = types[5];
                    break;
                case CharacterType.Volt:
                    result = types[11];
                    break;
                case CharacterType.Cursed:
                    result = types[10];
                    break;
                case CharacterType.Dreamer:
                    result = types[9];
                    break;
                case CharacterType.Celestial:
                    result = types[12];
                    break;
                default:
                    result = types[0];
                    break;
            }
            image.sprite = result;
        }

        public void SetImageSpriteByType(Image image, int indexSprite)
        {
            image.sprite = types[indexSprite];
        }
    }
}