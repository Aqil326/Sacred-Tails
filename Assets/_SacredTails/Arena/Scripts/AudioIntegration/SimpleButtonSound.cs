using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    /// <summary>
    /// Add callback to event OnClick of attached button using Unity Audio System
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(AudioSource))]
    public class SimpleButtonSound : MonoBehaviour
    {
        AudioSource audioSource;
        Button button;
        [SerializeField] AudioClip audioClip;
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            button = GetComponent<Button>();
            button.onClick.AddListener(() => audioSource.PlayOneShot(audioClip));
        }
    }
}