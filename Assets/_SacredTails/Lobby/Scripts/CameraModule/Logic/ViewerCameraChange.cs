using Cinemachine;
using UnityEngine;

namespace Timba.SacredTails.CameraModule
{
    public class ViewerCameraChange : MonoBehaviour
    {
        #region ----Fields----
        public CinemachineVirtualCamera lobbyCamera;
        public CinemachineVirtualCamera viewerCamera;
        private bool isViewerCamera;
        #endregion ----Fields----

        #region ----Methods----	
        public void ChangeCamera()
        {
            isViewerCamera = !isViewerCamera;
            viewerCamera.Priority = isViewerCamera ? 10 : 0;
            lobbyCamera.Priority = !isViewerCamera ? 10 : 0;
        }
        #endregion ----Methods----	
    }
}