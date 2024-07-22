using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Timba.SacredTails.ChatModule
{
    public class EmojiButton : MonoBehaviour
    {
        public Button button;
        public Image icon;
        public string codeValue;
        public string realValue;

        private void Start()
        {
            button.onClick.AddListener(() => EventSystem.current.SetSelectedGameObject(null));
        }
    }
}