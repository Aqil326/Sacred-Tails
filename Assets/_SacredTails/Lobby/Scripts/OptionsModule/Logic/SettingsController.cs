using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    private bool popupState = false;

    private void Awake()
    {
        this.transform.localScale= new Vector2(0,0);
        this.gameObject.SetActive(popupState);
    }


    public void ToggleSettingsPopUp()
    {
        popupState = !popupState;
        if (popupState)
        {
            this.gameObject.SetActive(popupState);
            this.transform.DOScale(1f, 0.25f);
        }
        else
        {
            this.transform.DOScale(0, 0.25f);
            //this.gameObject.SetActive(popupState);
        }
    }
}
