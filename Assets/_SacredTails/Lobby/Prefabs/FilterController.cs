using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Timba.SacredTails.ChatModule
{
    public class FilterController : MonoBehaviour
    {
        public ChatTextBox chatTextBox;
        public List<Toggle> toggles = new List<Toggle>();
        bool lastIsAll = false;

        private void Start()
        {
            for (int i = 0; i < toggles.Count - 1; i++)
            {
                toggles[i].onValueChanged.AddListener((state) =>
                {
                    if (state)
                    {
                        IfAllEnable();
                    }
                    else
                    {
                        IfAnyDisable();
                    }
                });
            }
            toggles.Last().onValueChanged.AddListener((state) => ChangeAllMinusLast(state));
        }

        public void CallApplyFilters()
        {
            chatTextBox.ApplyFilters();
        }

        public void ChangeAllMinusLast(bool state)
        {
            if (lastIsAll)
                return;
            for (int i = 0; i < toggles.Count - 1; i++)
            {
                toggles[i].isOn = state;
            }
        }

        public void IfAnyDisable()
        {
            lastIsAll = true;
            toggles.Last().isOn = false;
            lastIsAll = false;
        }

        public void IfAllEnable()
        {
            bool response = true;
            for (int i = 0; i < toggles.Count - 1; i++)
            {
                if (toggles[i].isOn == false)
                {
                    response = false;
                    break;
                }
            }
            if (response)
                toggles.Last().isOn = true;
        }
    }
}