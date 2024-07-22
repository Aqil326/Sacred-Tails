using System;
using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.TournamentBehavior;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Allow hide all objects innecesary in tournament mode, only visual results
/// </summary>
public class CheckTournamentInscription : MonoBehaviour
{
    [SerializeField] List<GameObject> DisableObjects = new List<GameObject>();
    [SerializeField] UnityEvent<int> OnCheckTournamentRegist;
    [SerializeField] GameObject Friendly;
    public Action onCheckIfTournamentExist;

    private void OnEnable()
    {
        EnableNPCs();
    }

    public bool hasAlreadyRecheckConnection = false;
    public void EnableNPCs()
    {
        PlayfabManager.Singleton.GetUserData(PlayerDataManager.Singleton.localPlayerData.playfabId, new List<string>() { "CurrentTournament" }, (data) =>
        {

            if (!hasAlreadyRecheckConnection)
            {
                hasAlreadyRecheckConnection = true;

                if (data.Data.ContainsKey("CurrentTournament"))
                    PlayerDataManager.Singleton.currentTournamentId = data.Data["CurrentTournament"].Value;
                if (PlayerDataManager.Singleton.currentTournamentId != "")
                    FindObjectOfType<TournamentReadyController>(true).gameObject.SetActive(true);
            }

            OnCheckTournamentRegist.Invoke(PlayerDataManager.Singleton.currentTournamentId != "" ? 1 : 0);
            onCheckIfTournamentExist?.Invoke();
        });
    }

    public void DisableObjectsInTournament(bool state)
    {
        if (state)
            FindObjectOfType<TournamentReadyController>(true).gameObject.SetActive(false);
        if (Friendly == null)
        {
            Friendly = GameObject.Find("FriendlyMatch_Btn");
            Friendly.SetActive(state);
        }
        else
            Friendly.SetActive(state);
        PlayerDataManager.Singleton.isOnTheTournament = !state;
        foreach (var item in DisableObjects)
        {
            item.SetActive(state);
        }
    }
}
