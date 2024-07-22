using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.AudioIntegration
{
    public class WWiseSacred : MonoBehaviour
    {
        //public List<AK.Wwise.Event> MyEvents = null;

        public void PTEvent(string eventName)
        {
            //AK.Wwise.Event.Post();
            AkSoundEngine.PostEvent(eventName, gameObject);
        }
    }
}