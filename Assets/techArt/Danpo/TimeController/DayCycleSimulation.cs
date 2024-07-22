using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public struct ApiUTC
{
    public DateTime dateTime;
}

public class DayCycleSimulation : MonoBehaviour
{
    public TimeController timeManager;
    public float currentTime;
    public Action<float> onHourPassed;
    private const string urlApiUtc = "http://worldtimeapi.org/api/timezone/America/Bogota";
    private bool isReady = false;

    public void Start()
    {
        isReady = false;
        Init();
    }

    public void Init()
    {
        StartCoroutine(MakeEndpointCall(urlApiUtc, (timeJson) =>
        {
            if (String.IsNullOrEmpty(timeJson))
            {
                Init();
                return;
            }
            string[] splittedResponse = timeJson.Split('T');
            ApiUTC apiUTC = JsonConvert.DeserializeObject<ApiUTC>(timeJson);
            apiUTC.dateTime.AddHours(5);
            var hour = Int16.Parse(splittedResponse[1].Substring(0, 2));
            var minute = Int16.Parse(splittedResponse[1].Substring(3, 2));

            var regularTime = hour + (minute / 60f);
            var currentMinutes = (((regularTime) % 3) / 3) * 24;
            currentTime = currentMinutes;

            timeManager.hour = currentTime;
            isReady = true;
        }));
    }

    IEnumerator MakeEndpointCall(string _url, Action<string> callback)
    {
        UnityWebRequest request;
        request = UnityWebRequest.Get(_url);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        callback?.Invoke(request.downloadHandler.text);
    }

    private float delayCount = 0;
    public void Update()
    {
        if (!isReady)
            return;

        delayCount += 1 * Time.deltaTime;

        if (delayCount >= 60)
        {
            var previousHour = currentTime;

            delayCount = 0;
            if (currentTime >= 24)
                currentTime = 0;
            else
                currentTime += .125f;

            if (Mathf.FloorToInt(previousHour) < Mathf.FloorToInt(currentTime))
                onHourPassed?.Invoke(currentTime);
            timeManager.hour = currentTime;
        }

        onHourPassed?.Invoke(currentTime);
    }
}
