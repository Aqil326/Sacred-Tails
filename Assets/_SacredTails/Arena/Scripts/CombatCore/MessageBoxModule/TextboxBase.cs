using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Timba.SacredTails.Arena
{
    public abstract class TextboxBase : MonoBehaviour
    {
        [SerializeField] GameObject textPrefab;
        [SerializeField] Transform parent;
        [SerializeField] BattleUIController battleUIController;
        [SerializeField] Scrollbar scrollbar;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] List<TextMeshProUGUI> ExistingText = new List<TextMeshProUGUI>();
        [SerializeField] List<string> Filters = new List<string>();
        [SerializeField] MessageView messageView;
        [SerializeField] AlteredView alteredView;
        bool needToBeFiltered = false;
        public virtual void AddText(string message, Dictionary<string, string> customCodes = null, float auxDuration = 3)
        {
            Debug.Log("Message :D "+ message);

            if (string.IsNullOrEmpty(message))
                return;
            if (message.Contains("Dodge") && ExistingText.Last().text == message)
                return;
            
            //try to dont use data from local player because this element works in visualization 
            message = message.Replace("[Player]", $"[{PlayerPrefs.GetString("PlayerName")}]");
            message = message.Replace("[Enemy]", $"[{PlayerPrefs.GetString("EnemyName")}]");

            TextMeshProUGUI textObject = Instantiate(textPrefab, parent).GetComponent<TextMeshProUGUI>();
            //textObject.gameObject.SetActive(true);
            ExistingText.Add(textObject);

            if (customCodes != null)
                foreach (var Key in customCodes.Keys)
                    message = message.Replace(Key, customCodes[Key]);

            if (messageView != null)
            {
                if (alteredView != null & message.Contains(">>>"))
                    Debug.Log("Passed");
                else
                    messageView.ShowMessage(message, duration: auxDuration);
            }

            if (alteredView != null)
                CheckAlteredState(message);

            textObject.text = message;
            if (needToBeFiltered)
                CheckIndividual(textObject);
            else
                textObject.gameObject.SetActive(true);
            /*if (battleUIController != null)
                battleUIController.uIDisolver.SetTargetValue(1);
            */
            StartCoroutine(PutScrollAtEnd());
        }

        List<string> possibleAlteredState = new List<string>
    {
        "rooted",
        "ignited",
        "bleed",
        "freezing",
        "sleeped",
        "paralyzed"
    };

        public void CheckAlteredState(string message)
        {
            if (message.Contains("Your"))
            {
                for (int i = 0; i < possibleAlteredState.Count; i++)
                {
                    if (message.Contains(possibleAlteredState[i]))
                    {
                        //alteredView.ShowAlteredByTime(1, messageView.possibleIcons[i]);
                    }
                }
            }
        }

        public void State(bool state)
        {
            value = state;
        }

        bool value;

        public void AddRemoveFilter(string data)
        {
            if (value)
            {
                if (!Filters.Contains(data))
                    Filters.Add(data);
            }
            else
            {
                if (Filters.Contains(data))
                    Filters.Remove(data);
            }
        }

        public void ApplyFilters()
        {
            StartCoroutine(ApplyAtEndOfFrame());
        }

        IEnumerator ApplyAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < ExistingText.Count; i++)
            {
                CheckIndividual(ExistingText[i]);
            }
        }
        public void CheckIndividual(TextMeshProUGUI text)
        {
            bool filtered = false;
            foreach (var item in Filters)
            {
                if (text.text.Contains(item))
                {
                    filtered = true;
                }
            }
            text.gameObject.SetActive(filtered);
        }

        IEnumerator PutScrollAtEnd()
        {
            yield return new WaitForEndOfFrame();
            scrollbar.value = 0;
            scrollRect.verticalNormalizedPosition = 0;
            //scrollRect.CalculateLayoutInputVertical();
        }
    }
}