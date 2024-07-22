using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timba.Patterns;

public class TimerService : MonoBehaviour, ITimer
{

    public string UpdateTimer(float timeInSeconds, string colorText = null, bool showHour = false)
    {
        string timerText;
        float totalHours = timeInSeconds / 60 / 60;
        int totalMinutes = (int)timeInSeconds / 60;
        float hours = (int)(timeInSeconds / 60 / 60);
        int minutes = (int)((totalHours - hours) * 60);
        int seconds = (int)timeInSeconds - (totalMinutes * 60);
        if (showHour)
            timerText = $"{(colorText != null ? $"<color={colorText}>" : "")}{hours.ToString("00")}:{minutes.ToString("00")}:{seconds.ToString("00")}{(colorText != null ? $" </ color > " : "")}";
        else
            timerText = $"{(colorText != null ? $"<color={colorText}>" : "")}{minutes.ToString("00")}:{seconds.ToString("00")}{(colorText != null ? $" </ color > " : "")}";
        return timerText;
    }

    public bool IsReady()
    {
        return true;
    }
}
