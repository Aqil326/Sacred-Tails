using System.Collections.Generic;
using UnityEngine;
using System;
using Timba.Patterns;
using Timba.SacredTails.Database;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// Panel before the combat when players select the Shinsei party to the battle
    /// </summary>
    public class ArenaShinseiSelectionUserPanelController : MonoBehaviour
    {
        #region ----Fields----
        [Header("shinsei slots fields")]
        public List<Image> selectionIndicators, selectionIndicatorsDetail;
        public Sprite selectedIndicator;
        public Sprite unselectedIndicator;
        public ShinseiSlot shinseSlotPrefab;
        private List<ShinseiSlot> shinseisSlots;
        public Transform partyRow1;
        public Transform partyRow2;
        public TMP_Text nameLabel;
        public TextMeshProUGUI textCounter;

        [Header("")]
        public bool isEnemyPanel;
        public ShinseiPreviewPanelManager previewPanel;

        private List<int> shinseisSelected;
        #endregion ----Fields----

        #region ----Methods----
        public void Init(CombatPlayer combatPlayer, bool isEnemy = false)
        {
            nameLabel.text = combatPlayer.DisplayName;
            if (isEnemy)
                nameLabel.text = String.IsNullOrEmpty(combatPlayer.DisplayName) ? PlayerPrefs.GetString("GetBotName") : combatPlayer.DisplayName;
            this.gameObject.SetActive(true);
            this.gameObject.SetActive(true);
            shinseisSelected = new List<int>();
            shinseisSlots = new List<ShinseiSlot>();

            for (int i = 0; i < combatPlayer.ShinseiParty.Count; i++)
            {
                ShinseiSlot NewSlot = Instantiate(shinseSlotPrefab, i < (combatPlayer.ShinseiParty.Count / 2) ? partyRow1 : partyRow1).GetComponent<ShinseiSlot>();
                NewSlot.gameObject.SetActive(true);
                NewSlot.shinsei = combatPlayer.ShinseiParty[i];
                NewSlot.shinseiKey = NewSlot.shinsei.ShinseiDna;
                NewSlot.listIndex = i;
                NewSlot.UpdateVisual(null, shinseiDNA: NewSlot.shinseiKey, NewSlot.shinsei.shinseiIcon);

                //NewSlot._shinseiName.text = "SHINSEI #" + NewSlot.shinsei.shinseiName;

                NewSlot._health.text = "HP: " + combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Health.ToString() + "/" + combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Health.ToString();
                NewSlot._helathBar.maxValue = combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Health;
                NewSlot._helathBar.value = combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Health;

                NewSlot._energy.text = "PP: " + combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Energy.ToString() + "/" + combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Energy.ToString();
                NewSlot._energyBar.maxValue = combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Energy;
                NewSlot._energyBar.value = combatPlayer.ShinseiParty[i].ShinseiOriginalStats.Energy;

                shinseisSlots.Add(NewSlot);
                NewSlot?.OnSlotClicked.AddListener(SelectShinseiForBattle);
                NewSlot._infoBtn.targetGraphic.color = Color.black;
                NewSlot?._infoBtn.onClick.AddListener(() => { SelectShinseiForBattle(NewSlot.listIndex, NewSlot); });
                NewSlot?.previewBtn.onClick.AddListener(() =>
                {
                    previewPanel.gameObject.SetActive(true);
                    previewPanel.DisplayPreview(NewSlot.shinsei, isEnemyPanel, index:NewSlot.listIndex);
                    Debug.Log("DisplayPreview 01");
                    textCounter.text = NewSlot.Counter.text;
                    textCounter.transform.parent.gameObject.SetActive(NewSlot.Counter.gameObject.activeInHierarchy);
                    if (previewPanel.selectBtn != null)
                    {
                        previewPanel.selectBtn.onClick.RemoveAllListeners();
                        previewPanel.selectBtn.onClick.AddListener(() =>
                        {
                            previewPanel.currentShinsei = NewSlot.shinsei;
                            SelectShinseiForBattle(NewSlot.listIndex, NewSlot);
                            Show(0);
                        });
                    }
                });
            }
        }

        public void ShowNext(bool direction)
        {
            Show(direction ? 1 : -1);
        }

        public void Show(int direction)
        {
            previewPanel.gameObject.SetActive(true);
            int index = shinseisSlots.IndexOf(shinseisSlots.Where(data => data.shinsei == previewPanel.currentShinsei).ToList()[0]) + direction;
            if (index < 0)
                index = shinseisSlots.Count - 1;
            if (index > shinseisSlots.Count - 1)
                index = 0;
            ShinseiSlot slot = shinseisSlots[index];
            previewPanel.DisplayPreview(slot.shinsei, isEnemyPanel, index: slot.listIndex);
            Debug.Log("DisplayPreview 02");
            textCounter.text = slot.Counter.text;
            textCounter.transform.parent.gameObject.SetActive(slot.Counter.gameObject.activeInHierarchy);
            previewPanel.selectBtn.onClick.RemoveAllListeners();
            previewPanel.selectBtn.onClick.AddListener(() =>
            {
                previewPanel.currentShinsei = slot.shinsei;
                SelectShinseiForBattle(slot.listIndex, slot);
                Show(0);
            });
        }


        public void SelectShinseiForBattle(int listIndex, ShinseiSlot shinseiSlot)
        {
            foreach (var slots in shinseisSlots)
                slots.Counter.transform.parent.gameObject.SetActive(false);
            if (shinseisSelected.Contains(listIndex))
            {
                foreach (var indicator in selectionIndicators)
                    indicator.sprite = unselectedIndicator;
                foreach (var indicator in selectionIndicatorsDetail)
                    indicator.sprite = unselectedIndicator;
                    
                shinseiSlot.ChangeInteractuable(false, false);
                shinseiSlot._infoBtn.targetGraphic.color = Color.black;
                shinseisSelected.Remove(listIndex);
                for (int i = 0; i < shinseisSelected.Count; i++)
                {
                    shinseisSlots[shinseisSelected[i]].Counter.transform.parent.gameObject.SetActive(true);
                    shinseisSlots[shinseisSelected[i]].Counter.text = (i + 1).ToString();
                }
                if (shinseisSelected.Count == 0)
                    return;

                for (int i = 0; i <= shinseisSelected.Count - 1; i++)
                {
                    selectionIndicators[i].sprite = selectedIndicator;
                    selectionIndicatorsDetail[i].sprite = selectedIndicator;
                }
            }
            else
            {
                if (shinseisSelected.Count >= 3)
                {
                    foreach (var indicator in selectionIndicators)
                        indicator.sprite = selectedIndicator;
                    foreach (var indicator in selectionIndicatorsDetail)
                        indicator.sprite = unselectedIndicator;
                    SacredTailsLog.LogMessage("Max shisneis equal 3");
                    for (int i = 0; i < shinseisSelected.Count; i++)
                    {
                        shinseisSlots[shinseisSelected[i]].Counter.transform.parent.gameObject.SetActive(true);
                        shinseisSlots[shinseisSelected[i]].Counter.text = (i + 1).ToString();
                    }
                    return;
                }
                for (int i = 0; i <= shinseisSelected.Count; i++)
                {
                    selectionIndicators[i].sprite = selectedIndicator;
                    selectionIndicatorsDetail[i].sprite = selectedIndicator;
                }
                shinseiSlot._infoBtn.targetGraphic.color = shinseiSlot._higlightColor;
                shinseiSlot.ChangeInteractuable(true, false);
                shinseisSelected.Add(listIndex);
                for (int i = 0; i < shinseisSelected.Count; i++)
                {
                    shinseisSlots[shinseisSelected[i]].Counter.transform.parent.gameObject.SetActive(true);
                    shinseisSlots[shinseisSelected[i]].Counter.text = (i + 1).ToString();
                }
            }
        }

        public void MakeSlotsUnclickeable()
        {
            shinseisSlots.ForEach((x) =>
            {
                x.isPreviewOnly = true;
                x.transform.GetChild(0).GetComponent<Button>().interactable = false;
            });
        }

        public List<int> GetShinseisSelected()
        {
            return shinseisSelected;
        }

        #endregion ----Methods----
    }
}

