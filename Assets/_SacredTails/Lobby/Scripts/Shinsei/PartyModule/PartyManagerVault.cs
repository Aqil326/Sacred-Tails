using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using Timba.SacredTails.Arena;
using UnityEngine;
using UnityEngine.UI;

namespace Timba.Games.SacredTails.Lobby
{
    public class PartyManagerVault : PartyManager
    {
        public ShinseiPreviewPanelManager shinseiPreviewPanel;
        public SelectableUi selectableUi;

        public override void Initialize(Action<int, ShinseiSlot> onNewSlotCreated = null)
        {
            base.Initialize((index, NewSlot) =>
            {
                SelectableUiButton slotButton = NewSlot.GetComponent<SelectableUiButton>();
                if (slotButton)
                    selectableUi.selectableButtons.Add(slotButton);

                NewSlot.isPreviewOnly = false;
            });
            selectableUi.InitButtons();
        }

        public override void OnClickSlot(int listIndex, ShinseiSlot eventShinseiSlot)
        {
            selectableUi.OnClick(eventShinseiSlot.GetComponent<SelectableUiButton>(), true);
            shinseiPreviewPanel.gameObject.SetActive(true);
            shinseiPreviewPanel.DisplayPreview(eventShinseiSlot.shinsei, isVault: true);
            Debug.Log("DisplayPreview 06");
            ChangeOnClickSlotAction();

            ChangeOnClickSlotAction((shinseiPartyListIndex, partyShinseiSlot) =>
                    ChangeShinseis(listIndex, eventShinseiSlot, shinseiPartyListIndex, partyShinseiSlot)
                );
        }

        public void DisplayPreviewForShisnei(int shinseIndex)
        {
            shinseiPreviewPanel.DisplayPreview(shinseiSlots[shinseIndex].shinsei, isVault: true);
            Debug.Log("DisplayPreview 07");
        }


        public void ChangeShinseis(int listIndexFrom, ShinseiSlot shinseiSlotFrom, int listIndexTarget, ShinseiSlot shinseiSlotTarget)
        {
            //if (listIndexFrom == -1)
            //{
            //    shinseiSpawner.ChangeCurrentShinsei(shinseiSlotCompanion.shinsei.ShinseiDna);
            //    return;
            //}

            if (shinseiSlotTarget.shinseiKey != shinseiSlotFrom.shinseiKey)
            {
                Shinsei fromShinsei = (shinseiSlotFrom.IsCompanion) ? PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion : PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexFrom];
                Shinsei targetShinsei = (shinseiSlotTarget.IsCompanion) ? PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion : PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexTarget];
                if (shinseiSlotFrom.IsCompanion)
                    PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion = targetShinsei;
                else
                    PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexFrom] = targetShinsei;

                if (shinseiSlotTarget.IsCompanion)
                    PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion = fromShinsei;
                else
                    PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexTarget] = fromShinsei;

                string tempSlotName = shinseiSlotTarget.shinseiName;
                Shinsei tempSlotShisnei = shinseiSlotTarget.shinsei;
                shinseiSlotTarget.ChangeShinseiSlotValues(shinsei: shinseiSlotFrom.shinsei, name: shinseiSlotFrom.shinseiName);
                shinseiSlotFrom.ChangeShinseiSlotValues(name: tempSlotName, shinsei: tempSlotShisnei);

                shinseiSpawner?.ChangeCurrentShinsei(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);

                fromShinsei = (shinseiSlotFrom.IsCompanion) ? PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion : PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexFrom];
                targetShinsei = (shinseiSlotTarget.IsCompanion) ? PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion : PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndexTarget];

                //Change slot between companion and selected position in playfab
                Dictionary<string, string> changedKeys = new Dictionary<string, string>();
                changedKeys.Add(shinseiSlotTarget.shinseiKey, JsonUtility.ToJson(targetShinsei));
                changedKeys.Add(shinseiSlotFrom.shinseiKey, JsonUtility.ToJson(fromShinsei));
                PlayfabManager.Singleton.SetUserData(changedKeys, UserDataPermission.Public);
                PlayerDataManager.Singleton.localPlayerData.onPartyChange?.Invoke();
            }

            selectableUi.OnClick(shinseiSlotFrom.GetComponent<SelectableUiButton>(), false);
            selectableUi.OnClick(shinseiSlotTarget.GetComponent<SelectableUiButton>(), false);
            ChangeOnClickSlotAction();
        }
    }
}