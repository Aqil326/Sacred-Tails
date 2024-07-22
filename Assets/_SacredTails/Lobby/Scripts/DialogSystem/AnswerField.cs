using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Timba.SacredTails.DialogSystem
{
    public class AnswerField : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI AnswerText;
        [SerializeField] Button button;
        public void SetButtonResponse(int index, string text, Conversation conversation, Action backButtonCallback = null, Action skipDialog = null)
        {
            button.onClick.RemoveAllListeners();
            if (backButtonCallback == null)
                button.onClick.AddListener(() =>
                {
                    conversation.SendResponse(index);
                    skipDialog?.Invoke();
                });
            else
                button.onClick.AddListener(() =>
                {
                    conversation.SendResponse(index); backButtonCallback?.Invoke();
                    skipDialog?.Invoke();
                });
            AnswerText.text = text;
        }
    }
}