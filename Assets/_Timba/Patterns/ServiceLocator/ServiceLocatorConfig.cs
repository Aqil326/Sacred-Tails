using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.Patterns.ServiceLocator
{
    [CreateAssetMenu(fileName = "ServiceLocatorConfig", menuName = "Timba/Patterns/Service Locator Config")]
    public class ServiceLocatorConfig : ScriptableObject
    {
        public GameObject[] defaultServicesPrefabs;
    }
}