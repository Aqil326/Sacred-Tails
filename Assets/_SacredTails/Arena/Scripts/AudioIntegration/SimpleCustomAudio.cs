using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    /// <summary>
    ///     Play simple custom audio
    /// </summary>
    public class SimpleCustomAudio : MonoBehaviour
    {
        public string eventName;

        public void Play()
        {
            AkSoundEngine.PostEvent(eventName, gameObject);
        }
    }
}