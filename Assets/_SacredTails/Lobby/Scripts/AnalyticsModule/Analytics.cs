using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Analytics : MonoBehaviour
{
    public void UpdateAnalitycCounter(string statistic)
    {
        int currentValue = PlayerPrefs.GetInt(statistic,0);
        if (currentValue == 0)
            PlayfabManager.Singleton.GetStatistics((statistics) => {
                SacredTailsLog.LogMessage("Need to check if server has a own versión of data");
                List<string> names = statistics.Select(a => a.StatisticName).ToList();
                if (names.Contains(statistic))
                {
                    PlayfabManager.Singleton.UpdateStatistic(statistic, statistics[names.IndexOf(statistic)].Value + 1);
                    PlayerPrefs.SetInt(statistic, statistics[names.IndexOf(statistic)].Value + 1);
                }
                else
                {
                    PlayfabManager.Singleton.UpdateStatistic(statistic, 1);
                    PlayerPrefs.SetInt(statistic,1);
                }
            });
        else
        {
            PlayfabManager.Singleton.UpdateStatistic(statistic,currentValue + 1);
            PlayerPrefs.SetInt(statistic, currentValue + 1);
        }
    }

    public void UpdateAnalityc(string statistic,int value)
    {
        PlayfabManager.Singleton.UpdateStatistic(statistic, value);
    }
}
