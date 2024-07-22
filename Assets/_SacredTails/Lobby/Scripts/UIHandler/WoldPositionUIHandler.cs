using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.UiHelpers
{
    /// <summary>
    /// This allow you to put UI elements attached to player view
    /// </summary>
    public class WoldPositionUIHandler : MonoBehaviour
    {
        public List<WoldPositionateUiElement> WorldPositionateUiElements = new List<WoldPositionateUiElement>();
        public static WoldPositionUIHandler instance;
        [SerializeField] private Transform elementsParent;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            foreach (var element in WorldPositionateUiElements)
            {
                if (element.uiElement == null)
                {
                    element.uiElement.SetActive(false);
                    continue;
                }
                //element.uiElement.gameObject.SetActive(element.gameObject.activeInHierarchy);
                if (element.transform.position != null)
                    element.uiElement.transform.position = Camera.main.WorldToScreenPoint(element.transform.position);
            }
        }

        public void RegisterUiElement(WoldPositionateUiElement target)
        {
            WorldPositionateUiElements.Add(target);
            Vector3 localScale = target.uiElement.transform.localScale;
            target.uiElement.transform.SetParent(elementsParent);
            target.uiElement.transform.rotation = new Quaternion(0, 0, 0, 0);
            target.uiElement.transform.localScale = localScale;
            target.uiElement.transform.position = Camera.main.WorldToScreenPoint(target.transform.position);
        }

        public void UnregisterUiElement(WoldPositionateUiElement target)
        {
            WorldPositionateUiElements.Remove(target);
            Destroy(target.uiElement.gameObject);
        }
    }
}