using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit;
    [SerializeField] private RectTransform rectTransform;

    public void SetText(string content)
    {
        tooltipText.text = content;

        int headerLength = tooltipText.text.Length;
        //layoutElement.enabled = headerLength > characterWrapLimit;
    }

    private void Update()
    {
        Vector2 position = Input.mousePosition;

        float pivotX = position.x / Screen.width;
        float pivotY = position.y / Screen.height;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = position;
    }
}
