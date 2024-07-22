using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyChanger : MonoBehaviour
{
    [SerializeField] Material daymaterial, midleMaterial, nightMaterial;
    [SerializeField] DayCycleSimulation simulation;
    [SerializeField] float testTime;

    void Start()
    {
        simulation.onHourPassed += ChangeSky;
    }

    [ContextMenu("TestHour")]
    public void TestChangeSky()
    {
        ChangeSky(testTime);
    }
    
    public void ChangeSky(float currentTime)
    {
        //currentTime = 4.0f;
        //Debug.Log("currentTime: " + currentTime);
        //RenderSettings.skybox = midleMaterial;

        if (currentTime >=5 && currentTime <6)
        {
            RenderSettings.skybox = midleMaterial;
        }
        else if (currentTime >= 6 && currentTime < 17)
        {
            RenderSettings.skybox = daymaterial;
        }
        else if (currentTime >= 17 && currentTime < 18)
        {
            RenderSettings.skybox = midleMaterial;
        }
        else if (currentTime >= 18 && currentTime < 24 || currentTime >= 0 && currentTime < 5)
        {
            RenderSettings.skybox = nightMaterial;
        }
    }
}
