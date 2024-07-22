using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateAtEndOfFrame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Refresh();
    }
    [ContextMenu("RefreshUI")]
    public void Refresh()
    {
        StartCoroutine(RefreshUI());
    }
    IEnumerator RefreshUI()
    {
        yield return new WaitForSecondsRealtime(0.02f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}
