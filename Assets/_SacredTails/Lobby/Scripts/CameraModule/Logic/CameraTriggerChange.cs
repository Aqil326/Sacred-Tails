using Cinemachine;
using UnityEngine;

namespace Timba.SacredTails.CameraModule
{
    public class CameraTriggerChange : MonoBehaviour
    {
        #region ----Fields----
        public CinemachineVirtualCamera lobbyCamera;
        public CinemachineVirtualCamera treeCamera;
        #endregion ----Fields----

        #region ----Methods----	
        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                treeCamera.Priority = 3;
                lobbyCamera.Priority = 0;
            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                lobbyCamera.Priority = 3;
                treeCamera.Priority = 0;
            }
        }
        #endregion ----Methods----	
    }
}