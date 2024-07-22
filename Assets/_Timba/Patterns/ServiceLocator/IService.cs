using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.Patterns.ServiceLocator
{
    public interface IService
    {
        GameObject gameObject { get; }
        bool IsReady();
    }
}