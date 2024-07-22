using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Games.SacredTails.PopupModule
{
    public class PopupManager : MonoBehaviour, IPopupManager
    {
        #region ----Fields----
        [SerializeField] private RectTransform infoPopup;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private List<Button> buttons;

        public enum ButtonType
        {
            BACK_BUTTON = 0,
            CONFIRM_BUTTON = 1
        }
        #endregion ----Fields----

        #region ----Methods----
        public bool IsReady()
        {
            return true;
        }

        public void ShowInfoPopup(string textInfo, Dictionary<ButtonType, Action> buttonsActionsPair = null)
        {
            infoText.text = textInfo;

            //Set special cases
            if (buttonsActionsPair == null)
            {
                buttonsActionsPair = new Dictionary<ButtonType, Action>();
                buttonsActionsPair.Add(ButtonType.BACK_BUTTON, null);
            }
            if (buttonsActionsPair.ContainsKey(ButtonType.BACK_BUTTON))
                buttonsActionsPair[ButtonType.BACK_BUTTON] += HideInfoPopup;

            foreach (var buttonActionPair in buttonsActionsPair)
            {
                buttons[(int)buttonActionPair.Key].onClick.RemoveAllListeners();
                buttons[(int)buttonActionPair.Key].gameObject.SetActive(true);
                buttons[(int)buttonActionPair.Key].onClick.AddListener(() => buttonActionPair.Value?.Invoke());
            }

            infoPopup.gameObject.SetActive(true);
        }

        public void HideInfoPopup()
        {
            buttons.ForEach((buttons) => buttons.gameObject.SetActive(false));
            infoPopup.gameObject.SetActive(false);
        }
        #endregion ----Methods----

    }
}