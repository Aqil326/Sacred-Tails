using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script solves a problem with WWise when you put the game in second plane at return all songs in queque play at time, and control the bird sounds
/// </summary>
public class RareThing : MonoBehaviour
{
    [SerializeField] TimeController timeController;
    public AK.Wwise.Event MyEvent = null;
    public AK.Wwise.Event Birds = null;
    private void Start()
    {
        PlaySound("Town");
        StartCoroutine(PlayBirdEachTime());
        //AkSoundEngine.SetSwitch("Music_Play", "Town", gameObject);
    }

    public void PlaySound(string state)
    {
        MyEvent.Stop(gameObject);
        AkSoundEngine.SetState("MX", state);
        MyEvent.Post(gameObject);
    }

    void Update()
    {
        AkSoundEngine.WakeupFromSuspend();
        AkSoundEngine.RenderAudio();
    }

    IEnumerator PlayBirdEachTime()
    {
        while (true)
        {
            if (timeController.hour < 12)
            {
                Birds.Post(gameObject);
            }
            yield return new WaitForSeconds(Random.Range(5, 20));
        }
    }

}
