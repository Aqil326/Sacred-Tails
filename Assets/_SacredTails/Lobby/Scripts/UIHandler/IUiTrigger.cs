using UnityEngine;
using Timba.Games.SacredTails.StoreModule;
using UnityEngine.Events;
using System;

public abstract class IUiTrigger : MonoBehaviour
{
    [SerializeField] private protected GameObject indicatorWoldPosition, canvas, imageIndicator;
    [SerializeField] private protected UnityEvent onOpenEvent;
    
    protected Action<Collider> OnTriggerEnterEvent;
    protected Action<Collider> OnTriggerExitEvent;

    bool playerIsIn = false;

    #region Methods

    #region Class Methods

    private void LateUpdate()
    {
        if (canvas.activeSelf)
            imageIndicator.transform.position = Camera.main.WorldToScreenPoint(indicatorWoldPosition.transform.position);
    }

    public void Update()
    {
        CheckForInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnableActivateButton(other);
        OnTriggerEnterEvent?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        HideTriggerAndButton(other);
        OnTriggerExitEvent?.Invoke(other);
    }

    #endregion Class Methods

    #region Abstract methods

    private protected virtual void EnableActivateButton(Collider other)
    {
        var otherComponent = other.GetComponent<ThirdPersonController>();
        if (otherComponent == null || !otherComponent.IsLocalPlayer)
            return;
        canvas.SetActive(true);
        otherComponent.OnDisablePLayer = () => canvas.SetActive(false);
        playerIsIn = true;
    }
    private protected virtual bool CheckForInput()
    {
        if (!playerIsIn)
            return false;
        if (Input.GetKeyDown(KeyCode.E))
        {
            onOpenEvent?.Invoke();
            return true;
        }
        else
            return false;
    }

    private protected virtual void HideTriggerAndButton(Collider other)
    {
        var otherComponent = other.GetComponent<ThirdPersonController>();
        if (otherComponent == null || !otherComponent.IsLocalPlayer)
            return;

        playerIsIn = false;
        canvas.SetActive(false);
        otherComponent.OnDisablePLayer = null;
    }

    #endregion Abstract methods

    #endregion Methods
}

