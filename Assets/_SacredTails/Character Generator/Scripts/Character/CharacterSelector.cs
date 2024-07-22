using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

namespace Timba.Games.CharacterFactory
{
    public class CharacterSelector : MonoBehaviour
    {
        public TMP_Text characterName;
        public Button button;

        public void SetButtonAction(Action _action, bool _isClearActionsSubscribed = true)
        {
            if (_isClearActionsSubscribed)
            {
                button.onClick.RemoveAllListeners();
            }

            button.onClick.AddListener(() => _action());

        }
    }
}