using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Lobby;
using UnityEngine;

public class DisableOtherShinsei : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("OtherPlayer"))
        {
            other.GetComponent<ShinseiSpawner>()?.characterSlot.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OtherPlayer"))
        {
            other.GetComponent<ShinseiSpawner>()?.characterSlot.gameObject.SetActive(true);
        }
    }
}
