using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;

public class PutNightModeForVfx : MonoBehaviour
{
    public float targetHour = 20;
    void OnEnable()
    {
        StartCoroutine(WaitForVfx(FindObjectsOfType<TimeController>()[0]));
    }

    // Update is called once per frame
    IEnumerator WaitForVfx(TimeController timeController)
    {
        float desiredTime = GetComponent<VfxInfo>().vfxDuration /2;
        float time = 0;

        float initHour = timeController.hour;
        while (time < desiredTime)
        {
            timeController.hour = Mathf.Lerp(timeController.hour, targetHour, time / desiredTime);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        while (time < desiredTime)
        {
            timeController.hour = Mathf.Lerp(timeController.hour, initHour, time / desiredTime);
            time += Time.deltaTime;
            yield return null;
        }
    }
}
