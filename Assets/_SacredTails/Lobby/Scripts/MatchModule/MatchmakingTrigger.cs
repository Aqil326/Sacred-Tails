using UnityEngine;
using Unity.Netcode;

public class MatchmakingTrigger : NetworkBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerUI ui))
            if (other.GetComponent<ThirdPersonController>().IsLocalPlayer)
                ui.DisplayMatchmakingOptions();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerUI ui))
            if (other.GetComponent<ThirdPersonController>().IsLocalPlayer)
                ui.SearchMatchInitTimer();
    }
}
