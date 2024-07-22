using System.Collections;
using System.Collections.Generic;
using Timba.Patterns.ServiceLocator;
using UnityEngine;

public interface ITimer : IService
{
    public string UpdateTimer(float timeInSeconds, string colorText = null, bool showHour = false);
}
