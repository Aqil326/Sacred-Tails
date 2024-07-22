using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutBackgroundMusic : MonoBehaviour
{
    [SerializeField] string MusicName = "Town";
    private void Start()
    {
        AkSoundEngine.SetState("MX", MusicName);
        RareThing rareThing = FindObjectOfType<RareThing>();
        rareThing.PlaySound(MusicName);
        //AkSoundEngine.SetSwitch("Music_Play", MusicName, GameObject.Find("AmbientAudios"));

        if (MusicName == "Town")
            PlayerPrefs.SetString("LastLocation", MusicName);
        SacredTailsLog.LogMessage("i tryed to put event sound");
    }
}
