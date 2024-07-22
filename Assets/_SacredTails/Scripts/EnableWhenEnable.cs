using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableWhenEnable : MonoBehaviour
{
    [SerializeField] List<GameObject> attachedObjects = new List<GameObject>();
    private void OnEnable()
    {
        foreach (var item in attachedObjects)
            item.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        foreach (var item in attachedObjects)
            item.gameObject.SetActive(false);
    }
}
