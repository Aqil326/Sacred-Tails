using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideDebug : MonoBehaviour
{
    [SerializeField] GameObject target;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (target != null)
            {
                target.SetActive(!target.activeSelf);
            }
        }
    }
}
