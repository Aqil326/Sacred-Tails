using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    /// <summary>
    ///     This component call an WWise event 
    /// </summary>
    public class SimpleAudioCallback : MonoBehaviour
    {
        public string eventName;

        public void PlayAudio()
        {
            AkSoundEngine.PostEvent(eventName, gameObject);
        }
    }
}