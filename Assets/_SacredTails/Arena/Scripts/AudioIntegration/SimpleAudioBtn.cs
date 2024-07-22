using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    /// <summary>
    ///     This component add a callback to component attached button to play a WWise event
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SimpleAudioBtn : MonoBehaviour
    {
        public string eventName;
        Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Start()
        {
            button.onClick.AddListener(() => { AkSoundEngine.PostEvent(eventName, gameObject); });
        }
    }
}