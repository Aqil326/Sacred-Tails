using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.UiHelpers;
using UnityEngine;

public class FreeLookCam : MonoBehaviour
{
    public GameObject freeLookCamera;
    public GameObject mainCamera;
    private bool activate = false;

    public void Update()
    {
        if ((activate && Input.GetKeyDown(KeyCode.Escape)))
        {
            Cursor.lockState = CursorLockMode.None;
            if (UIGroups.instance != null)
                UIGroups.instance.ShowOnlyThisGroup("planner");
            ActivateCamera(false);
        }
    }

    public void ActivateCamera(bool activateFreelook)
    {
        activate = activateFreelook;
        freeLookCamera.SetActive(activateFreelook);
        mainCamera.SetActive(!activateFreelook);
        Cursor.lockState = activateFreelook ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !activateFreelook;
    }
}
