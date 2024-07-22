using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.SacredTails.AudioIntegration
{
    public class AudioSettings : MonoBehaviour
    {
        public AK.Wwise.RTPC AmbienceVolume = null;
        public AK.Wwise.RTPC MusicVolume = null;
        public AK.Wwise.RTPC SFXVolume = null;

        public Slider MasterSlider;
        public Slider MusicSlider;
        public Slider SFXSlider;

        public Toggle muteToggle;
        bool isMute;

        public void CustomStart()
        {
            //MasterSlider.value = PlayerPrefs.GetFloat("Master",0.5f);
            MusicSlider.value = PlayerPrefs.GetFloat("Music", 0.5f);
            SFXSlider.value = PlayerPrefs.GetFloat("SFX", 0.5f);
            SetMusicValue(PlayerPrefs.GetFloat("Music", 0.5f));
            SetSFXValue(PlayerPrefs.GetFloat("SFX", 0.5f));

            muteToggle.isOn = PlayerPrefs.GetInt("UsersMuteOption", 0) == 1;
            SetMute(muteToggle.isOn);
        }

        public void SetMute(bool value)
        {
            isMute = value;
            PlayerPrefs.SetInt("UsersMuteOption", isMute ? 1 : 0);
            SetMusicValue(PlayerPrefs.GetFloat("Music", 0.5f));
            SetSFXValue(PlayerPrefs.GetFloat("SFX", 0.5f));
        }

        public void SetMasterValue(float value)
        {
            //MasterVolume.SetGlobalValue(value * 100);
            PlayerPrefs.SetFloat("Master", value );
        }

        public void SetMusicValue(float value)
        {
            MusicVolume.SetGlobalValue(value * value * 10 * (isMute ? 0 : 1));
            PlayerPrefs.SetFloat("Music", value);
        }

        public void SetSFXValue(float value)
        {
            AmbienceVolume.SetGlobalValue(value * value * 10 * (isMute ? 0 : 1));
            SFXVolume.SetGlobalValue(value * value * 10 * (isMute ? 0 : 1));
            PlayerPrefs.SetFloat("SFX", value);
        }
    }
}