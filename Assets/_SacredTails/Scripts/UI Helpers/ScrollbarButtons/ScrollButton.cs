using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    [RequireComponent(typeof(Button))]
    public class ScrollButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool isDown = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
        }
    }
}