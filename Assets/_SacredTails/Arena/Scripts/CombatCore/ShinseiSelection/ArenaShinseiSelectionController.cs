using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.Arena
{
    public class ArenaShinseiSelectionController : MonoBehaviour
    {
        #region ----Fields----
        [SerializeField] private ArenaShinseiSelectionUserPanelController playerPanel;
        [SerializeField] private ArenaShinseiSelectionUserPanelController enemyPanel;
        [SerializeField] private GameObject playerSelectPanel;
        [SerializeField] private GameObject viewerWaitChoosePanel;
        [SerializeField] private GameObject panelWaiting;
        public Action<bool, List<int>> OnShinseisSelected;
        [SerializeField] private BattleUIController battleUI;
        [SerializeField] private TMP_Text timerLabel;
        [SerializeField] private List<Image> backgrounds = new List<Image>();

        private MatchData matchData;
        private int playerIndex;
        #endregion ----Fields----

        #region ----Methods----
        public void Init(MatchData matchData, int playerIndex, bool isViewer = false)
        {
            if (isViewer)
            {
                viewerWaitChoosePanel.SetActive(true);
                return;
            }

            this.gameObject.SetActive(true);
            this.matchData = matchData;
            this.playerIndex = playerIndex;
            playerSelectPanel.SetActive(true);
            playerPanel.Init(matchData.MatchPlayers[playerIndex]);
            enemyPanel.Init(matchData.MatchPlayers[playerIndex == 1 ? 0 : 1], true);
            enemyPanel.MakeSlotsUnclickeable();
            StartCoroutine(ShinseiSelectionCountDown());
        }

        public void OnShinseiSelectionCompleted()
        {
            List<int> shinseisSelected = playerPanel.GetShinseisSelected();
            if (shinseisSelected.Count != 3)
            {
                SacredTailsLog.LogMessage("You have to select 3 shinseis");
                return;
            }
            OnShinseisSelected?.Invoke(false, shinseisSelected);
            panelWaiting.SetActive(true);
        }

        private IEnumerator ShinseiSelectionCountDown()
        {
            float localTime = Time.time;
            float actualTimer = PlayerPrefs.GetFloat("MatchSelectTime") != -1 ? PlayerPrefs.GetFloat("MatchSelectTime") : 120;
            while (actualTimer - (Time.time - localTime) > 0)
            {
                timerLabel.text = ServiceLocator.Instance.GetService<ITimer>().UpdateTimer(actualTimer - (Time.time - localTime));
                //battleUI.UpdateTimer(120 - (Time.time - localTime));
                yield return null;
            }

            List<int> defaultShinseis = new List<int>() { -1, -1, -1 };
            OnShinseisSelected?.Invoke(true, defaultShinseis);
        }
        #endregion ----Methods----
    }
}

