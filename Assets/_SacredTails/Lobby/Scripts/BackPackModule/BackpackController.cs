using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.WalletModule;
using UnityEngine;
using UnityEngine.Events;

public class BackpackController : MonoBehaviour
{
    public GameObject backpackPanel;
    private ThirdPersonController localPlayer;
    public CardFilterHandler cardFilterHandler;

    [SerializeField] private UnityEvent OnOpenBackpack;
    [SerializeField] private UnityEvent OnCloseBackpack;

  
    public void OpenBackPack()
    {
        //First time opening the backpack
        if(localPlayer == null)
            localPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<ThirdPersonController>();
            
        localPlayer.EnableMovement = false;
        cardFilterHandler.ClearFilters();
        backpackPanel.SetActive(true);
        PlayerDataManager.Singleton.localPlayerData.characterState = Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.BACKPACK;

        OnOpenBackpack?.Invoke();
    }

    public void CloseBackPack()
    {
        localPlayer.EnableMovement = true;
        backpackPanel.SetActive(false);
        PlayerDataManager.Singleton.localPlayerData.characterState = Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.LOBBY;

        OnCloseBackpack?.Invoke();

      
    }
}
