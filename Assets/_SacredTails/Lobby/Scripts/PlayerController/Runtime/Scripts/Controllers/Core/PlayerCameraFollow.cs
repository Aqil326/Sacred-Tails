using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Lobby
{
    public class PlayerCameraFollow : Utils.Singleton<PlayerCameraFollow>
    {
        public List<CinemachineVirtualCamera> virtualCam;

        public void FollowPlayer(Transform transform)
        {
            virtualCam.ForEach(cam => cam.Follow = transform);
        }
    }
}

