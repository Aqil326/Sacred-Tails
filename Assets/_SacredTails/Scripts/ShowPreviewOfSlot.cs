using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowPreviewOfSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShinseiPreviewPanelManager shinseiPreviewPanel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*shinseiPreviewPanel.gameObject.SetActive(true);
        shinseiPreviewPanel.DisplayPreview(GetComponent<ShinseiSlot>().shinsei, isVault: true);
        Debug.Log("DisplayPreview 11");*/
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //shinseiPreviewPanel.gameObject.SetActive(false);
    }
}
