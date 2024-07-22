using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Timba.SacredTails.UiHelpers
{
    public class SelectableButtonGroup : MonoBehaviour
    {
        [SerializeField] Color normalColor, SelectedColor;
        [SerializeField] List<ButtonList> buttonList;
        [SerializeField] RankedLeaderboard leaderboard;
        public void SelectButton(int index)
        {
            if (leaderboard != null && leaderboard.isProcessingLeaderboard)
                return;
            foreach (var button in buttonList)
            {
                button.buttonText.color = normalColor;
                button.marker?.SetActive(false);
            }
            buttonList[index].buttonText.color = SelectedColor;
            buttonList[index].marker?.SetActive(true);
        }

        public void SelectButton(int index, bool overrideLeaderboard)
        {
            if (leaderboard != null && leaderboard.isProcessingLeaderboard && !overrideLeaderboard)
                return;
            foreach (var button in buttonList)
            {
                button.buttonText.color = normalColor;
                button.marker?.SetActive(false);
            }
            buttonList[index].buttonText.color = SelectedColor;
            buttonList[index].marker?.SetActive(true);
        }
        [System.Serializable]
        public class ButtonList
        {
            public TextMeshProUGUI buttonText;
            public GameObject marker;
        }
    }
}