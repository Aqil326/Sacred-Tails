using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPositionateElement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        if (transform != null)
            transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
    }
}
