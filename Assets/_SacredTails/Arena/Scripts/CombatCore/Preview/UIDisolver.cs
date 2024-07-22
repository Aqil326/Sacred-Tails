using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDisolver : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    float targetValue, currentValue, timeToDisolve = 0;
    [SerializeField] bool autoDissolveAfterTime;
    [SerializeField] float requiredElapsedTime;

    private void Start()
    {
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (currentValue != targetValue)
        {
            currentValue = Mathf.MoveTowards(currentValue, targetValue, 10 * Time.deltaTime);
            canvasGroup.alpha = currentValue;
        }
        if (autoDissolveAfterTime && timeToDisolve < Time.time)
        {
            //targetValue = 0;
            autoDissolveAfterTime = false;
        }
    }

    public void ToggleMaximumValius()
    {
        targetValue = targetValue + 1 > 1 ? 0 : 1;
        timeToDisolve = Time.time + requiredElapsedTime;
        //autoDissolveAfterTime = true;
    }

    public void SetTargetValue(float value)
    {
        //targetValue = value;
        timeToDisolve = Time.time + requiredElapsedTime;
        //autoDissolveAfterTime = true;
    }
}
