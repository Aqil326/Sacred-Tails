using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VendorSeeArea : NetworkBehaviour
{
    [SerializeField] VendorHeadTracking vendorHeadTracking;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>().IsLocalPlayer)
            vendorHeadTracking.SetSeeTarget(other.transform);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>().IsLocalPlayer)
        {
            //vendorHeadTracking.SetSeeTarget(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>().IsLocalPlayer)
            vendorHeadTracking.SetOriginal();
    }
}
