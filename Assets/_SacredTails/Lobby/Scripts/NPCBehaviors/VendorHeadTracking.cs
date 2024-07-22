using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorHeadTracking : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] Transform targetRotation;
    [SerializeField] Transform forwardOriginalDirection;
    [SerializeField] float velocity = 10;
    [SerializeField] Quaternion currentRotation;

    private void Start()
    {
        currentRotation = transform.rotation;
    }

    public void SetSeeTarget(Transform colisionTarget)
    {
        targetTransform = colisionTarget;
    }

    public void SetOriginal()
    {
        targetTransform = forwardOriginalDirection;
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            return;
        }
        targetRotation.LookAt(targetTransform);
        currentRotation = Quaternion.RotateTowards(currentRotation, targetRotation.rotation, velocity * Time.deltaTime);
        transform.rotation = currentRotation;
    }
}
