using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Timba.Games.SacredTails.Lobby
{
    /// <summary>
    /// Teleport the character from trigger position to targetPosition transform
    /// </summary>
    public class SacredTeleport : MonoBehaviour
    {
        [SerializeField] UnityEvent OnPlayerTriggerEnter;
        [SerializeField] Transform targetPosition;
        [SerializeField] string MusicName = "Bar";
        private void UpdateBackgroundMusicWhenTeleport()
        {
            AkSoundEngine.SetState("MX", MusicName);
            PlayerPrefs.SetString("LastLocation", MusicName);
            RareThing rareThing = FindObjectOfType<RareThing>();
            rareThing.PlaySound(MusicName);
            //AkSoundEngine.SetSwitch("Music_Play", MusicName, GameObject.Find("AmbientAudios"));

            SacredTailsLog.LogMessage("i tryed to put event sound");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (MusicName != "")
                    UpdateBackgroundMusicWhenTeleport();
                other.GetComponent<NavMeshAgent>().Warp(targetPosition.position);
                other.transform.rotation = targetPosition.transform.rotation;
                OnPlayerTriggerEnter.Invoke();
            }
        }
    }
}