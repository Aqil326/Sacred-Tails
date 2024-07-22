using System.Collections.Generic;
using Timba.SacredTails.Lobby;
using Timba.SacredTails.Database;
using UnityEngine;
using UnityEngine.Events;
using PlayFab.ClientModels;
using Timba.SacredTails.Arena;
using System;
using System.Linq;
using Sirenix.OdinInspector;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using Timba.Patterns.ServiceLocator;

namespace Timba.Games.SacredTails.Lobby
{
    public abstract class PartyManager : MonoBehaviour
    {
        [SerializeField] private protected Transform slotsParent;
        [SerializeField] private protected GameObject shinseiSlotPrefab;
        [SerializeField] private protected Transform CompanionSelectionPanel;
        public int selectorPos;

        public ShinseiSpawner shinseiSpawner;
        private protected ShinseiSlot shinseiSlotCompanion;
        private protected List<ShinseiSlot> shinseiSlots = new List<ShinseiSlot>();
        private protected IDatabase database;


        bool isInitialize = false;

        public virtual void Start()
        {
            PlayerDataManager.Singleton.OnDataObtained += () => Initialize();
        }

        public void UpdateShinseis()
        {
            List<Shinsei> shinseiParty = new List<Shinsei>();
            shinseiParty.Add(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion);
            shinseiParty.AddRange(PlayerDataManager.Singleton.localPlayerData.ShinseiParty);

            for (int i = 0; i < shinseiParty.Count; i++)
            {
                var NewSlot = shinseiSlots[i];
                var shinseiName = "";
                Sprite shinseiIcon;
                if (i == 0)
                {
                    shinseiSlotCompanion = NewSlot;
                    NewSlot.IsCompanion = true;
                    NewSlot.shinseiKey = Constants.SHINSEI_COMPANION;
                    NewSlot.shinsei = shinseiParty[0];
                    shinseiName = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);
                    shinseiIcon = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.shinseiIcon;
                    shinseiSpawner?.ChangeCurrentShinsei(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);
                }
                else
                {
                    NewSlot.shinseiKey = Constants.SHINSEI_SLOT + i;
                    NewSlot.listIndex = i - 1;
                    NewSlot.shinsei = shinseiParty[i];
                    shinseiName = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(PlayerDataManager.Singleton.localPlayerData.ShinseiParty[NewSlot.listIndex].ShinseiDna);
                    shinseiIcon = PlayerDataManager.Singleton.localPlayerData.ShinseiParty[NewSlot.listIndex].shinseiIcon;
                }
                NewSlot?.UpdateVisual(shinseiName: shinseiName, null, shinseiIcon);
            }

        }
        [Button]
        public virtual void Initialize(Action<int, ShinseiSlot> onNewSlotCreated = null)
        {
            if (isInitialize)
                return;
            isInitialize = true;


            List<Shinsei> shinseiParty = new List<Shinsei>();
            shinseiParty.Add(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion);
            shinseiParty.AddRange(PlayerDataManager.Singleton.localPlayerData.ShinseiParty);
            for (int i = 0; i < shinseiParty.Count; i++)
            {
                ShinseiSlot NewSlot = Instantiate(shinseiSlotPrefab, slotsParent).GetComponent<ShinseiSlot>();
                NewSlot.gameObject.SetActive(true);
                string shinseiName = "";
                Sprite shinseiIcon;
                if (i == 0)
                {
                    shinseiSlotCompanion = NewSlot;
                    NewSlot.IsCompanion = true;
                    NewSlot.shinseiKey = Constants.SHINSEI_COMPANION;
                    NewSlot.shinsei = shinseiParty[0];
                    shinseiName = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);
                    shinseiIcon = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.shinseiIcon;
                }
                else
                {
                    NewSlot.shinseiKey = Constants.SHINSEI_SLOT + i;
                    NewSlot.listIndex = i - 1;
                    NewSlot.shinsei = shinseiParty[i];
                    shinseiName = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(PlayerDataManager.Singleton.localPlayerData.ShinseiParty[NewSlot.listIndex].ShinseiDna);
                    shinseiIcon = PlayerDataManager.Singleton.localPlayerData.ShinseiParty[NewSlot.listIndex].shinseiIcon;
                }

                shinseiSlots.Add(NewSlot);
                NewSlot.OnSlotClicked.AddListener(OnClickSlot);
                NewSlot?.UpdateVisual(shinseiName: shinseiName, null, shinseiIcon);

                onNewSlotCreated?.Invoke(i, NewSlot);
            }
            PlayerDataManager.Singleton.localPlayerData.onPartyChange += UpdateShinseis;
        }


        public virtual void ChangeOnClickSlotAction(UnityAction<int, ShinseiSlot> onClick = null)
        {
            if (onClick == null)
                shinseiSlots.ForEach((shinseiSlot) =>
                {
                    shinseiSlot.OnSlotClicked.RemoveAllListeners();
                    shinseiSlot.OnSlotClicked.AddListener(OnClickSlot);
                });
            else
                shinseiSlots.ForEach((shinseiSlot) =>
                {
                    shinseiSlot.OnSlotClicked.RemoveAllListeners();
                    shinseiSlot.OnSlotClicked.AddListener(onClick);
                });
        }

        public virtual void OnClickSlot(int listIndex, ShinseiSlot eventShinseiSlot)
        {
            if (listIndex == -1)
            {
                shinseiSpawner.ChangeCurrentShinsei(shinseiSlotCompanion.shinsei.ShinseiDna);
                return;
            }

            if (shinseiSlotCompanion.shinseiKey != eventShinseiSlot.shinseiKey)
            {
                Shinsei shinseiTemporal = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion;
                PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion = PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndex];
                PlayerDataManager.Singleton.localPlayerData.ShinseiParty[listIndex] = shinseiTemporal;

                string tempSlotName = shinseiSlotCompanion.shinseiName;
                Shinsei tempSlotShisnei = shinseiSlotCompanion.shinsei;
                shinseiSlotCompanion.ChangeShinseiSlotValues(shinsei: eventShinseiSlot.shinsei, name: eventShinseiSlot.shinseiName);
                eventShinseiSlot.ChangeShinseiSlotValues(name: tempSlotName, shinsei: tempSlotShisnei);
                //shinseiSlotCompanion.UpdateVisual(null, shinseiDNA: PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna, PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.shinseiIcon);
                //eventShinseiSlot.UpdateVisual(null, shinseiDNA: shinseiTemporal.ShinseiDna, shinseiTemporal.shinseiIcon);

                shinseiSpawner?.ChangeCurrentShinsei(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion.ShinseiDna);

                //Change slot between companion and selected position in playfab
                Dictionary<string, string> changedKeys = new Dictionary<string, string>();
                changedKeys.Add(shinseiSlotCompanion.shinseiKey, JsonUtility.ToJson(PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion));
                changedKeys.Add(eventShinseiSlot.shinseiKey, JsonUtility.ToJson(PlayerDataManager.Singleton.localPlayerData.ShinseiParty[eventShinseiSlot.listIndex]));
                PlayfabManager.Singleton.SetUserData(changedKeys, UserDataPermission.Public);
                PlayerDataManager.Singleton.localPlayerData.onPartyChange?.Invoke();
            }
        }

        public virtual void HidePanel()
        {
            CompanionSelectionPanel.gameObject.SetActive(false);
        }

    }
}