using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.CharacterStyle
{
    /// <summary>
    ///     UI element that represents a part of character for edition
    /// </summary>
    public class CharacterStyleButton : MonoBehaviour
    {
        [SerializeField] Image image;
        [SerializeField] List<Sprite> colorImage, grayscaleImage;
        [SerializeField] GameObject comingSoonObj;
        [SerializeField] bool isEnable;

        private void OnEnable()
        {
            UpdateBtn(PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.SKIN].presetId, isEnable);
        }

        public void UpdateBtn(int index, bool enable)
        {
            if (enable)
            {
                image.sprite = colorImage[index];
                if (comingSoonObj != null)
                    comingSoonObj?.SetActive(false);
            }
            else
            {
                image.sprite = grayscaleImage[index];
                if (comingSoonObj != null)
                    comingSoonObj?.SetActive(true);
            }
        }
    }
}