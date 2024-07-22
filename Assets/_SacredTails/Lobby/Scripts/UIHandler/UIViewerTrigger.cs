using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using System.Collections;

public class UIViewerTrigger : IUiTrigger
{
    public CinemachineVirtualCamera lobbyCamera;
    public CinemachineVirtualCamera viewerCamera;
    private bool isViewerCamera;

    private ThirdPersonController localPlayer;

    [SerializeField]
    private UnityEvent useViewpointEvent;
    [SerializeField]
    private UnityEvent startExitViewpointEvent;
    [SerializeField]
    private UnityEvent endExitViewpointEvent;
    private int endExitId = 0;

    private void Start()
    {
        OnTriggerEnterEvent = EnableMovementLocalPlayer; 
        OnTriggerExitEvent = EnableMovementLocalPlayer;
    }

    public void ChangeCamera()
    {
        isViewerCamera = !isViewerCamera;
        viewerCamera.Priority = isViewerCamera ? 3 : 0;
        lobbyCamera.Priority = !isViewerCamera ? 3 : 0;

        if (localPlayer != null)
            localPlayer.EnableMovement = !localPlayer.EnableMovement;
    }

    private protected override void EnableActivateButton(Collider other)
    {
        base.EnableActivateButton(other);
    }

    private protected override bool CheckForInput()
    {
        if (!base.CheckForInput())
            return false;
        ChangeCamera();
        UpdateViewpointState(isViewerCamera);
        //Debug.Log("PRESS E");
        return true;
    }

    private protected override void HideTriggerAndButton(Collider other)
    {
        base.HideTriggerAndButton(other);
    }

    private void EnableMovementLocalPlayer(Collider collider)
    {
        if (localPlayer != null)
            return;

        collider.TryGetComponent(out localPlayer);
    }

    #region Show "exit interface" and hide "ChangeStyleBackground/Background"

    private void UpdateViewpointState(bool locIsViewerCamera)
    {
        endExitId = endExitId > 100 ? 0 : endExitId+1;

        if (locIsViewerCamera)
        {
            useViewpointEvent?.Invoke();
        }
        else
        {
            startExitViewpointEvent?.Invoke();

            StartCoroutine(WaitAndInvokeEndExitEvent(1, endExitId));
        }
    }

    private IEnumerator WaitAndInvokeEndExitEvent(float locTimeWait, int locEndExitId)
    {
        yield return new WaitForSeconds(locTimeWait);

        if (locEndExitId == endExitId)
        {
            endExitViewpointEvent?.Invoke();
        }
    }

    public void PressExitViewpoint()
    {
        if (isViewerCamera)
        {
            onOpenEvent?.Invoke();
            ChangeCamera();
            UpdateViewpointState(isViewerCamera);
        }
    }

    #endregion

}

