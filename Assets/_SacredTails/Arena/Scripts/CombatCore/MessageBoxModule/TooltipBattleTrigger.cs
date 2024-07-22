using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipBattleTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipBattleHandler.Show(GetComponent<ITooltipItem>().ReturnTooltipText());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipBattleHandler.Hide();
    }

    private void OnDestroy()
    {
        TooltipBattleHandler.Hide();
    }
}

public interface ITooltipItem
{
    public string ReturnTooltipText();
}
