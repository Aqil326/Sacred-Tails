using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]

public class ActiveOnNight : MonoBehaviour
{
    public TimeController timeManager;
    public GameObject[] NightObjects;
    bool turnon;


    // Update is called once per frame
    void Update()
    {
        if (timeManager != null && NightObjects.Length > 0)
        {
            if (timeManager.hour >= 18 || timeManager.hour <= 4.5f)
            {
                if (turnon == false)
                {
                    StartCoroutine(activeObj());
                    turnon = true;
                }
            }

            if (timeManager.hour < 18 && timeManager.hour > 4.5f)
            {
                if (turnon == true)
                {
                    StartCoroutine(unactiveObj());
                    turnon = false;
                }
            }
        }

    }

    IEnumerator activeObj()
    {
        for (int i = 0; i < NightObjects?.Length; i++)
            NightObjects[i]?.SetActive(true);
        yield break;
    }

    IEnumerator unactiveObj()
    {
        for (int i = 0; i < NightObjects?.Length; i++)
            NightObjects[i]?.SetActive(false);
        yield break;
    }

}
