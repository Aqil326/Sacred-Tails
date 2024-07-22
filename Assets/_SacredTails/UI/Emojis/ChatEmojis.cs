using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace Timba.SacredTails.ChatModule
{
    public class ChatEmojis : MonoBehaviour
    {
        public ChatTextBox chatTextBox;
        public DiccionaryOfEmojis diccionaryOfEmojis;
        public TMP_InputField inputField;
        public int pendingCarets = 0;
        bool isFocused;

        private void Start()
        {
            chatTextBox.OnStartEditing += () => { isFocused = true; };
            chatTextBox.OnEndEditing += () => { isFocused = false; };
        }

        public void OnChangeValue(string msg)
        {
            StartCoroutine(OnChangeValueRoutine(msg));
        }

        IEnumerator OnChangeValueRoutine(string msg)
        {
            bool whisperFilled = false;
            //Complete whisper
            if (msg.Contains("/r") && msg.Length == 2)
            {
                string name = PlayerPrefs.GetString("LastWhisper", "");
                msg = msg + " " + name + " ";
                if (isFocused)
                    inputField.caretPosition += 2 + name.Length;
                else
                    pendingCarets += 2 + name.Length;
                whisperFilled = true;
            }
            //Emoji behavior
            string withEmojis = msg;
            foreach (var key in diccionaryOfEmojis.keys)
                if (withEmojis.Contains(key))
                {
                    withEmojis = withEmojis.Replace(key, diccionaryOfEmojis.GetByKey(key));
                    if (isFocused)
                        inputField.caretPosition += 1;
                    else
                        pendingCarets++;
                }
            inputField.text = withEmojis;
            if (whisperFilled)
            {
                inputField.Select();
                inputField.ActivateInputField();
                yield return new WaitForEndOfFrame();
                inputField.MoveToEndOfLine(shift: false, ctrl: false);
            }
            yield return new WaitForEndOfFrame();
        }

        public void Update()
        {
            if (pendingCarets > 0 && isFocused)
            {
                for (int i = 0; i < pendingCarets; i++)
                {
                    inputField.caretPosition++;
                }
                pendingCarets = 0;
            }
        }

        [System.Serializable]
        public class DiccionaryOfEmojis
        {
            public List<string> keys = new List<string>();
            public List<string> RealValues = new List<string>();
            [PreviewField]
            public List<Sprite> visual = new List<Sprite>();

            public string GetByKey(string key)
            {
                if (keys.Contains(key))
                    return RealValues[keys.IndexOf(key)];
                else
                    return key;
            }
        }
    }
}