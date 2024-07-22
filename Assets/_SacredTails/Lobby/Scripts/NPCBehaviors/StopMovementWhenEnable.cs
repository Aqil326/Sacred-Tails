using System.Collections;
using System.Collections.Generic;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

public class StopMovementWhenEnable : MonoBehaviour
{
    ThirdPersonController thirdPersonController;
    private void OnEnable()
    {
        if (thirdPersonController == null)
            thirdPersonController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
        DisableMovement(true);
    }
    private void OnDisable()
    {
        DisableMovement(false);
    }
    public void DisableMovement(bool newState)
    {
        if (thirdPersonController != null)
            thirdPersonController.IsMovementBloqued = newState;
    }
}
