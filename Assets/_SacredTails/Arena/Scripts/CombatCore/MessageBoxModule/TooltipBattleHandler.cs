using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipBattleHandler : MonoBehaviour
{
    private static TooltipBattleHandler Instance;

    public TooltipUI tooltip;

    public void Awake()
    {
        Instance = this;
    }

    public static void Show(string text)
    {
        Instance.tooltip.SetText(text);
        Instance.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Instance.tooltip.gameObject.SetActive(false);
    }

}
