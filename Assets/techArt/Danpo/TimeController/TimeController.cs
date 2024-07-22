//Daniel Porras 2022 danielporrasdeveloper@outlook.com
//Timba 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[ExecuteAlways]

public class TimeController : MonoBehaviour

{
     public GameObject Sun;
         [HideInInspector]
     public GameObject Moon;
      [Range(0, 24)]
    public float hour = 24;
      float rotationX;

    [Range(0, 360)]
    public float SunYAngle = 60;

    float colorvalue;
    public Gradient LightColor;
    public AnimationCurve LightIntensity;
    public float IntensityMultiplier = 1;
    public Gradient SkyColor;
    public Gradient FogColor;
    float Daynight;
    public bool isOnBar;
    

    // Update is called once per frame


    void Update()
    {
        if(Sun != null)
        {
            if (isOnBar)
            {
                colorvalue = 0;
                rotationX = ((0 - 6) * 360 / 24);
            }
            else
            {
                colorvalue = hour/24;
                rotationX = ((hour - 6)* 360/24);
            }
            Sun.GetComponent<Transform>().eulerAngles = new Vector3(rotationX, SunYAngle, 0);
            Sun.GetComponent<Light>().intensity = LightIntensity.Evaluate(colorvalue)  * IntensityMultiplier;
            Sun.GetComponent<Light>().color = LightColor.Evaluate(colorvalue);

            Shader.SetGlobalColor("_SkyColor", SkyColor.Evaluate(colorvalue));
            Shader.SetGlobalColor("_SunColor", LightColor.Evaluate(colorvalue));
            Shader.SetGlobalVector("_WorldSpaceLightRot", new Vector4(Sun.transform.rotation.x, Sun.transform.rotation.y,Sun.transform.rotation.z,Sun.transform.rotation.w));
            Shader.SetGlobalVector("_WorldSpaceLightPos", Sun.transform.forward);
            RenderSettings.ambientLight = SkyColor.Evaluate(colorvalue);
            RenderSettings.ambientEquatorColor = SkyColor.Evaluate(colorvalue);
            RenderSettings.fogColor = FogColor.Evaluate(colorvalue);
            if(hour >= 12 && !isOnBar){
                Daynight = ((hour-12)/(24-12));
                Shader.SetGlobalFloat("_SkyDayNight", Daynight);
            }
            if(hour < 12 && !isOnBar){
                Daynight = 1-(hour/12);
                Shader.SetGlobalFloat("_SkyDayNight", Daynight); 
            }
            if (isOnBar)
                Shader.SetGlobalFloat("_SkyDayNight", 1-(0/12));
        }
    }

    public void SetOnBar(bool value)
    {
        isOnBar = value;
    }
   
}
