using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Timba.SacredTails.VFXController
{
    public class VFXDestroyer : MonoBehaviour
    {
        void Start()
        {
            AutoDestroy();
        }

        void AutoDestroy()
        {
            //await Task.Delay(TimeSpan.FromSeconds(6));
            Destroy(gameObject, 6);
        }
    }
}