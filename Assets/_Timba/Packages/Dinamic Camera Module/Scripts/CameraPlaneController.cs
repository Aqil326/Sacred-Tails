using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace Timba.Games.DynamicCamera
{
    /// <summary>
    /// Switch between diferent cameras by index
    /// </summary>
    public class CameraPlaneController : MonoBehaviour
    {
        public bool testStaticCam = false; //Desable automatic transition.

        [Header("Introductory Camera")]
        [SerializeField]
        private float timeIntroduce;
        [SerializeField]
        private CinemachineVirtualCamera mainIntroductoryCamera;

        [Header("")]
        [SerializeField] private List<CinemachineVirtualCamera> _cameras = new List<CinemachineVirtualCamera>();
        [SerializeField] private List<Transform> _pointsOfInterest = new List<Transform>();
        private CinemachineVirtualCamera currentCamera;
        private CinemachineDollyCart _path;
        private bool executingWaitingInputMode = true;

        private void Awake()
        {
            if (testStaticCam)
                currentCamera = _cameras[(int)CamerasAvailableEnum.GENERAL_CAMERA];
            currentCamera = _cameras.Find(cam => cam.Priority == 10);
        }

        private void Start()
        {
            
        }

        [Button("Switch Cam")]
        public void SwitchToCam(CamerasAvailableEnum camIndex, float time = 5, float pathPosition = 0, Action callback = null)
        {
            //time = 1;
            Debug.Log("CAM Index: " + camIndex);
            Debug.Log("CAM time: " + time);
            Debug.Log("CAM pathPosition: " + pathPosition);

            if (testStaticCam)
                return;

            foreach (var cam in _cameras)
            {
                if (cam == null)
                    continue;
                else if (cam != _cameras[(int)camIndex])
                {
                    cam.Priority = 0;
                    continue;
                }

                _path = null;
                _path = cam.GetComponentInParent<CinemachineDollyCart>();
                if (_path != null)
                {
                    _path.m_Speed = _path.m_Path.PathLength / time;
                    _path.m_Position = pathPosition;
                }

                if (time == -1)
                    time = 5;
                StartCoroutine(WaitForSecondsToCallback(time, callback));
                currentCamera = cam;
                cam.Priority = 10;
            }
        }

        public IEnumerator WaitForSecondsToCallback(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        [Button("Switch Point")]
        public void SwitchPointOfInterest(CameraPointOfInteresEnum pointIndex, bool doFocusOffset = false, bool focusOffsetRight = false)
        {
            if (testStaticCam)
                return;
            if (doFocusOffset)
            {
                if (focusOffsetRight)
                    currentCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = .2f;
                else
                    currentCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = .7f;
            }

            currentCamera.LookAt = _pointsOfInterest[(int)pointIndex];
        }
        public void ClearPointOfInterest()
        {
            currentCamera.LookAt = null;
        }

        #region Wait Turn Cameras

        bool isWaitingTurnCamerasOn = false;

        public void InitCameras()
        {
            StartCoroutine(IntroductoryCamera(InitWaitTurnCameras));
        }

        public void InitWaitTurnCameras()
        {
            if (testStaticCam)
                return;
            isWaitingTurnCamerasOn = true;
            WaitingForInputModeCamera();
        }

        public void StopWaitTurnCameras()
        {
            isWaitingTurnCamerasOn = false;
        }

        private void WaitingForInputModeCamera()
        {
            if (testStaticCam)
                return;
            if (!isWaitingTurnCamerasOn)
                return;
            float randomValue = UnityEngine.Random.value;
            Debug.Log("randomValue: " + randomValue);
            if (randomValue > .8f)
                OrbitalCamera(WaitingForInputModeCamera);
            else if (randomValue > .5f) //(randomValue > .55f) CAMTIMER
                StaticCamera(randomValue > .7f, WaitingForInputModeCamera);
            //else if (randomValue > .4f)
            //    PanThenStaticFrame(randomValue > .5f, WaitingForInputModeCamera); // To do quit
            else if (randomValue > .4f) //(randomValue > .3f) CAMTIMER
                PanCamera(WaitingForInputModeCamera);
                //GeneralFrame(WaitingForInputModeCamera); //CAMTIMER was enable
            else if (randomValue > .25f) //(randomValue > .3f) CAMTIMER
                FarMiddleCamera(WaitingForInputModeCamera);
            else
                CenitalFrame(randomValue > .1f, WaitingForInputModeCamera);
        }

        #region Camera Frames
        public void FarMiddleCamera(Action _callback)
        {
            SwitchToCam(CamerasAvailableEnum.FAR_MIDDLE_CAMERA, callback: _callback);
        }

        public void PanCamera(Action _callback)
        {
            SwitchToCam(CamerasAvailableEnum.PAN_CAMERA, callback: _callback);
        }

        public void OrbitalCamera(Action _callback)
        {
            if (testStaticCam)
                return;
            Debug.Log("CAMFRAME: " + "Orbital");
            SwitchToCam(CamerasAvailableEnum.ORBIT_CAMERA, callback: _callback);
        }

        public void StaticCamera(bool _playerFocus, Action _callback)
        {
            if (testStaticCam)
                return;
            Debug.Log("CAMFRAME: " + "staticCam");
            CamerasAvailableEnum cameraFocus = _playerFocus ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY;
            SwitchToCam(cameraFocus, callback: _callback);

            CameraPointOfInteresEnum pointOfInterest = _playerFocus ? CameraPointOfInteresEnum.PLAYER_SHINSEI : CameraPointOfInteresEnum.ENEMY_SHINSEI;
            //SwitchPointOfInterest(pointOfInterest);
        }

        //public void PanThenStaticFrame(bool _playerFocus, Action _callback)
        //{
        //    if (testStaticCam)
        //        return;
        //    Debug.Log("CAMFRAME: " + "PanThenStatic");

        //    CamerasAvailableEnum cameraFocus = _playerFocus ? CamerasAvailableEnum.PAN_CAMERA : CamerasAvailableEnum.PAN_CAMERA_BACK;
        //    SwitchToCam(cameraFocus, callback: () =>
        //    {
        //        if (isWaitingTurnCamerasOn)
        //        {
        //            CameraPointOfInteresEnum pointOfInterest = _playerFocus ? CameraPointOfInteresEnum.PLAYER_SHINSEI : CameraPointOfInteresEnum.ENEMY_SHINSEI;
        //            SwitchPointOfInterest(pointOfInterest);

        //            CamerasAvailableEnum cameraSide = _playerFocus ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY;
        //            SwitchToCam(cameraSide, callback: _callback);
        //        }
        //    });
        //}

        public void GeneralFrame(Action _callback)
        {
            if (testStaticCam)
                return;
            Debug.Log("CAMFRAME: " + "General");
            SwitchToCam(CamerasAvailableEnum.GENERAL_CAMERA, callback: _callback);
        }

        public void CenitalFrame(bool focusPlayer, Action _callback)
        {
            if (testStaticCam)
                return;
            Debug.Log("CAMFRAME: " + "CenitalFrame");
            CamerasAvailableEnum cameraFocus = focusPlayer ? CamerasAvailableEnum.SIDE_CAMERA_PLAYER : CamerasAvailableEnum.SIDE_CAMERA_ENEMY;

            CameraPointOfInteresEnum pointOfInterest = focusPlayer ? CameraPointOfInteresEnum.PLAYER_SHINSEI : CameraPointOfInteresEnum.ENEMY_SHINSEI;
            //SwitchPointOfInterest(pointOfInterest);
            SwitchToCam(cameraFocus, callback: () =>
            {
                if (isWaitingTurnCamerasOn)
                    SwitchToCam(
                        focusPlayer ? CamerasAvailableEnum.CENITAL_PLAYER : CamerasAvailableEnum.CENITAL_ENEMY,
                        pathPosition: 12,
                        callback: _callback);

                Debug.Log("CAMFRAME: " + "CenitalFrame2");
            });
        }

        private IEnumerator IntroductoryCamera(Action onFinishIntroductory)
        {
            mainIntroductoryCamera.gameObject.SetActive(true);
            yield return new WaitForSeconds(timeIntroduce);
            mainIntroductoryCamera.gameObject.SetActive(false);
            onFinishIntroductory?.Invoke();
        }

        #endregion Camera Frames

        #endregion Wait Turn Cameras
    }
}
public enum CamerasAvailableEnum
{
    DEFAULT_CAMERA,
    SIDE_CAMERA_PLAYER,
    SIDE_CAMERA_ENEMY,
    PAN_CAMERA,
    PAN_CAMERA_BACK,
    ORBIT_CAMERA,
    ORBIT_CAMERA_BACK,
    LOOK_AT_CAMERA_PLAYER,
    LOOK_AT_CAMERA_ENEMY,
    MIDDLE_CAMERA,
    FAR_MIDDLE_CAMERA,
    GENERAL_CAMERA,
    CENITAL_PLAYER,
    CENITAL_ENEMY,
}

public enum CameraPointOfInteresEnum
{
    ARENA_CENTER,
    PLAYER_SHINSEI,
    ENEMY_SHINSEI,
}

public enum CameraPhasesEnum
{
    PHASE_A,
    PHASE_B,
    PHASE_C,
    PHASE_D,
}

