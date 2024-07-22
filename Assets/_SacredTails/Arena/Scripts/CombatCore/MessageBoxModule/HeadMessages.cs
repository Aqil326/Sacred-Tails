using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HeadMessages : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    public void ShowMessage(string msg)
    {
        gameObject.SetActive(true);
        textMeshProUGUI.text = msg;
        StartCoroutine(DisableAfterTime());
    }

    IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(12);
        gameObject.SetActive(false);
    }
}
