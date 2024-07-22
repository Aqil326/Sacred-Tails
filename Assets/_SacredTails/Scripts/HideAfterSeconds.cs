using System.Collections;
using UnityEngine;

public class HideAfterSeconds : MonoBehaviour
{
    public float seconds = 2;
    public bool fade = false;

    private CanvasGroup canvasGroup;
    public void StartHideCountdown()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        StartCoroutine(WaitForSecondsThenHide());
    }

    IEnumerator WaitForSecondsThenHide()
    {
        yield return new WaitForSeconds(seconds);
        if (fade)
            StartCoroutine(WaitForVfx(1));
        else
            canvasGroup.alpha = 0;
    }

    IEnumerator WaitForVfx(float timeFade)
    {
        float desiredTime = timeFade;
        float time = 0;

        while (time < desiredTime)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, time / desiredTime);
            time += Time.deltaTime;
            yield return null;
        }
    }
}
