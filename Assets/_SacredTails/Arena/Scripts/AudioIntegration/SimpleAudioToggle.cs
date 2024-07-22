using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    /// <summary>
    ///     UI Element that allow enable or disable audio in the game
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class SimpleAudioToggle : MonoBehaviour
    {
        public string eventName;
        Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
        }

        private void Start()
        {
            toggle.onValueChanged.AddListener((isOn) => { AkSoundEngine.PostEvent(eventName, gameObject); });
        }
    }
}