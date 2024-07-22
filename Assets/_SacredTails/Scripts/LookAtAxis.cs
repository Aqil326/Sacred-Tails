using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtAxis : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    void LateUpdate()
    {
        transform.LookAt(transform.position + Vector3.up);
        transform.Rotate(offset);
    }
}
