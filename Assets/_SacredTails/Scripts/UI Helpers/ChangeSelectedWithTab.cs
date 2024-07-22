using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Timba.SacredTails.UiHelpers
{
    public class ChangeSelectedWithTab : MonoBehaviour
    {
        EventSystem system;
        [SerializeField] List<GameObject> UiElementsToTab = new List<GameObject>();
        int currentSelected = 0;
        // Start is called before the first frame update
        void Start()
        {
            system = EventSystem.current;
            if (UiElementsToTab.Count < 1)
                Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (UiElementsToTab.Contains(system.currentSelectedGameObject))
                    currentSelected = UiElementsToTab.IndexOf(system.currentSelectedGameObject);
                currentSelected = currentSelected >= UiElementsToTab.Count - 1 ? 0 : currentSelected + 1;
                system.SetSelectedGameObject(UiElementsToTab[currentSelected], new BaseEventData(system));
            }
        }
    }
}