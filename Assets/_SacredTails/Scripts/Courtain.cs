using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A component that make a black transition 
/// </summary>
public class Courtain : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] int transitionTime;
    [SerializeField] float inblackWaitTime;
    float targetTime, initialTime;
    Coroutine fadeRoutine;

    public void StartFade(int transitionDuration)
    {
        transitionTime = transitionDuration;
        if (fadeRoutine != null)
            return;
        fadeRoutine = StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        initialTime = Time.time;
        targetTime = Time.time + transitionTime;
        canvasGroup.alpha = 1;
        yield return new WaitForSeconds(inblackWaitTime);
        while (Time.time < targetTime)
        {
            canvasGroup.alpha = (targetTime - Time.time) / transitionTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        fadeRoutine = null;
    }
}
