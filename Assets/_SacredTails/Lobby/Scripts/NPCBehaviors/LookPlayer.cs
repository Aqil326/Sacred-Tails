using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookPlayer : MonoBehaviour
{
    [SerializeField] Transform playerHead;
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;
    [SerializeField] float velocity;
    Quaternion originalRotation;
    Quaternion currentRotation;

    private void Start()
    {
        originalRotation = playerHead.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.transform;
        }       
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            currentRotation = Quaternion.RotateTowards(currentRotation, originalRotation, velocity * 10 * Time.deltaTime);
            playerHead.transform.rotation = currentRotation;
        }
        else
        {
            playerHead.transform.LookAt(target,Vector3.up);
            playerHead.transform.Rotate(offset);
            Quaternion currentCurrent = playerHead.transform.rotation;
            currentRotation = Quaternion.RotateTowards(currentRotation, currentCurrent, velocity * 10 * Time.deltaTime);
            playerHead.transform.rotation = currentRotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = null;
        }
    }
}
