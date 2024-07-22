using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UI;
using Timba.Games.SacredTails.StoreModule;
using System.Collections;
using System;
using UnityEngine.Events;

public class UiTrigger : IUiTrigger
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] ThirdPersonController player;
    [SerializeField] private StoreController storeController;
    bool playerIsIn = false;

    private protected override void EnableActivateButton(Collider other)
    {
        base.EnableActivateButton(other);
        player = other.GetComponent<ThirdPersonController>();
        playerIsIn = false;
    }
    private protected override bool CheckForInput()
    {
        //if (player == null && playerIsIn)
        //    HideVendor();

        if (!base.CheckForInput())
            return false;
        /*if (Input.GetKeyDown(KeyCode.E))
            if (!uiPanel.activeInHierarchy)
                ShowVendorUi();
            else
                HideVendor();
        */
        return true;
    }
    private protected override void HideTriggerAndButton(Collider other)
    {
        var otherComponent = other.GetComponent<ThirdPersonController>();
        //if (otherComponent == null || !otherComponent.IsLocalPlayer)
            //HideVendor();
        base.HideTriggerAndButton(other);
    }

    public void ShowVendorUi()
    {
        //player.GetComponent<PlayerUI>().OnOffDisplayName(false);
        storeController?.RequestCardsStore();
        uiPanel.SetActive(true);
        if (virtualCamera != null)
            virtualCamera.Priority = 100;
        canvas.SetActive(false);
    }
    public void HideVendor()
    {
        var playerController = player?.GetComponent<PlayerUI>();
        /*if (playerController != null)
            playerController.OnOffDisplayName(true);
        */
        storeController?.HideStore();
        //canvas.SetActive(true);
        uiPanel.SetActive(false);
        if (virtualCamera != null)
            virtualCamera.Priority = 0;
    }
}
