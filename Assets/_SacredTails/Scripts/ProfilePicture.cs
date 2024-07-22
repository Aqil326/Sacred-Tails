using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.CharacterStyle;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePicture : MonoBehaviour
{
    public static ProfilePicture self;

    public Image pictureImg;
    public Image frameImg;
    public ProfilePictureStyle pictureStyleDB;
    public Button openStylesPanelBtn;

    private void Awake()
    {
        if(self != null && self != this)
        {
            Destroy(gameObject);
        } else
        {
            self = this;
        }
    }

    private void Start()
    {
        if (PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.ContainsKey(PartsOfCharacter.PICTURE))
        {
            pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE].presetId];
            frameImg.sprite = pictureStyleDB.framingOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME].presetId];

            //frameImg.sprite = pictureStyleDB.framingOptions[PlayerPrefs.GetInt("frameImg", 0)];
        //pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerPrefs.GetInt("pictureImg", 0)];
        //frameImg.sprite = pictureStyleDB.framingOptions[PlayerPrefs.GetInt("frameImg", 0)];
        PlayerDataManager.Singleton.characterStyleController.pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE].presetId];

        PlayerDataManager.Singleton.characterStyleController.frameImg.sprite = pictureStyleDB.framingOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME].presetId];
        }

        openStylesPanelBtn.onClick.AddListener(PlayerDataManager.Singleton.characterStyleController.GetComponent<OpenCharacterStyle>().openCharacterStyleEvent.Invoke);
    }
}
