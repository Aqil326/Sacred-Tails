using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Timba.SacredTails.Database;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;

public class OwnBattleTracker : MonoBehaviour
{
    Analytics analytics;

    private void Start()
    {
        analytics = FindObjectOfType<Analytics>();
    }

    //The use of the Shinseis, the abilities (attacks) used in combat, damage and healing, turns on each combat, # of times they change the Shinseis in each combat..

    public void TotalTurns(int value)
    {
        string rawData = PlayerPrefs.GetString("TotalPromedium", "");
        PromediumObject realData = new PromediumObject();
        if (rawData != "")
        {
            realData = JsonUtility.FromJson<PromediumObject>(rawData);
        }
        realData.AddAtLast(value);
        int turnPromedium = 0;
        for (int i = 0; i < realData.TotalDuration.Count; i++)
            turnPromedium += realData.TotalDuration[i];
        turnPromedium = turnPromedium / realData.TotalDuration.Count;
        analytics.UpdateAnalityc("TurnAverage",turnPromedium);
        PlayerPrefs.SetString("TotalPromedium", JsonUtility.ToJson(realData));
    }

    public void NotifyAttack(int index)
    {
        if (index == 1004 || index == 1005 || index == 1006)
        {
            analytics.UpdateAnalitycCounter("ChangeShinsei");
            return;
        }
        ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(index);
        analytics.UpdateAnalitycCounter("Attack:" + actionCard.name);
    }

    [System.Serializable]
    public class PromediumObject
    {
        public List<int> TotalDuration = new List<int>();

        public void AddAtLast(int index)
        {
            if (TotalDuration.Count > 10)
            {
                TotalDuration.RemoveAt(0);
            }
            TotalDuration.Add(index);
        }
    }
}
