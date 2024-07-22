using UnityEngine;
using System;
using TMPro;

namespace Timba.SacredTails.UiHelpers
{
    public class CalendarDateItem : MonoBehaviour
    {
        public Action<string> onClickDate;
        public TMP_Text textField;

        public void OnDateItemClick()
        {
            onClickDate?.Invoke(textField.text);
        }
    }
}