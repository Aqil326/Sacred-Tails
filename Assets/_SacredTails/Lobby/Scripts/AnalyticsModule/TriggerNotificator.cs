using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerNotificator : MonoBehaviour
{
    [SerializeField] Analytics analytics;
    [SerializeField] int zone;
    [SerializeField] float duration = 60;
    private float timeBetweenNotifications;
    private void OnTriggerStay(Collider other)
    {
        Notify();
    }

    private void OnTriggerEnter(Collider other)
    {
        timeBetweenNotifications = Time.time + duration;
    }

    public void Notify()
    {
        if (timeBetweenNotifications - Time.time < 0)
        {
            analytics.UpdateAnalitycCounter(gameObject.name);
            timeBetweenNotifications = Time.time + duration;
            SacredTailsLog.LogMessage("Update zone statistic");
        }
    }
}
