using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.CharacterStyle;
using UnityEngine;

/// <summary>
/// Change between male and female character and update in playfab data
/// </summary>
public class ChangeGender : MonoBehaviour
{
    [SerializeField] List<PartsOfCharacter> partsOfCharacter = new List<PartsOfCharacter>();
    [SerializeField] CharacterStyleController characterStyleController;
    public void SelectGender(int index)
    {
        CharacterStyleController.UpdatePartOfCharacter(partsOfCharacter[index], index);
        PlayerDataManager.Singleton.UpdateCharacterStyleForAnyReason();
        characterStyleController.UpdateGender();
        PlayerPrefs.SetInt("changeGender", 1);
        gameObject.SetActive(false);

        ProfilePicture.self.openStylesPanelBtn.interactable = true;
    }

    private void Start()
    {
        /*if (PlayerPrefs.GetInt("changeGender",0) == 1)
            gameObject.SetActive(false);*/
    }
}
