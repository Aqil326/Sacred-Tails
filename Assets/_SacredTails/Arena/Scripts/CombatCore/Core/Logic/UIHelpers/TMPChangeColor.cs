using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TMPChangeColor : MonoBehaviour
{
    public Color currentHighlight, highlight, currentNormal, normal;
    public void ChangeColor(bool black)
    {
        GetComponent<TMP_Text>().color = !black ? currentNormal : currentHighlight;
    }
}
