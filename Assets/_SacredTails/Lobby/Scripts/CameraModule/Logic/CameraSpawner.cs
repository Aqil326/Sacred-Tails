using UnityEngine;
using Unity.Netcode;
using Timba.SacredTails.Lobby;

public class CameraSpawner : MonoBehaviour
{
    private void Start()
    {
        if (GetComponent<ThirdPersonController>().IsLocalPlayer)
            PlayerCameraFollow.Instance.FollowPlayer(this.transform);
    }
}
