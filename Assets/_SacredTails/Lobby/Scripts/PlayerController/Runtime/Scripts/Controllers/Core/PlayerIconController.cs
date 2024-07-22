using UnityEngine;
using Timba.Games.SacredTails.LobbyDatabase;

public class PlayerIconController : MonoBehaviour
{
    public GameObject backpackIcon;
    public GameObject combatIcon;

    public void ChangeIcon(CharacterStateEnum characterState)
    {
        switch (characterState)
        {
            case CharacterStateEnum.LOBBY:
                backpackIcon.SetActive(false);
                combatIcon.SetActive(false);
                break;
            case CharacterStateEnum.BACKPACK:
                backpackIcon.SetActive(true);
                combatIcon.SetActive(false);
                break;
            case CharacterStateEnum.COMBAT:
                backpackIcon.SetActive(false);
                combatIcon.SetActive(true);
                break;
            default:
                SacredTailsLog.LogErrorMessage("Character state not contemplated it.");
                break;
        }
    }
}

