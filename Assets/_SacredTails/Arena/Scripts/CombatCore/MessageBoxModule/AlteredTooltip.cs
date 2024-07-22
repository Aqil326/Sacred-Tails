using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlteredTooltip : MonoBehaviour, ITooltipItem
{
    [HideInInspector] public string tooltipText;

    public string ReturnTooltipText()
    {
        return tooltipText;
    }

}
