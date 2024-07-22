using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.Lobby;
using Timba.SacredTails.Database;
using UnityEngine;
using UnityEngine.Events;
using PlayFab.ClientModels;
using Timba.SacredTails.Arena;
using System;
using System.Linq;
using UnityEngine.UI;
using Timba.Patterns.ServiceLocator;

namespace Timba.Games.SacredTails.Lobby
{
    public class ShinseiVaultController : MonoBehaviour
    {

        #region ----Fields----
        #region Public Fields
        [Header("Parents")]
        [SerializeField] private GameObject shinseiVaultPanel;
        [SerializeField] private Transform shinseiSlotParent;

        [Header("Prefabs")]
        [SerializeField] private Transform boxPrefab;
        [SerializeField] private Transform rowPrefab;

        [SerializeField] private Transform emptyPrefab;
        [SerializeField] private ShinseiSlot shinseiSlotPrefab;

        [Header("Specs of vault")]
        [SerializeField] private GameObject selectedShinseiPanel;
        [SerializeField, Range(3, 10)] private int numberOfColumnsPerRow;

        [SerializeField] private PartyManager partyManager;
        [SerializeField] private ShinseiPreviewPanelManager shinseiPreviewPanel;
        #endregion Public Fields

        #region Private Fields
        private bool shinseiSlotSelected;
        private List<Shinsei> shinseiVault = new List<Shinsei>();
        private bool isInitialize;
        private List<ShinseiSlot> shinseiSlots = new List<ShinseiSlot>();
        private ShinseiSlot currentSlot = null;
        #endregion Private Fields
        #endregion ----Fields----

        #region ----Methods----
        #region Init
        public void Start()
        {
            PlayerDataManager.Singleton.OnDataObtained += Initialize;
        }

        public void Initialize()
        {
            isInitialize = true;
            shinseiVault = PlayerDataManager.Singleton.localPlayerData.ShinseiVault.ShinseiVaultList;

            int numberOfBoxes = Mathf.CeilToInt((1f * shinseiVault.Count) / (numberOfColumnsPerRow * 5));
            Transform currentBoxPrefab = boxPrefab;
            Transform currentRowPrefab = rowPrefab;

            //float aux = shinseiVault.Count / (numberOfColumnsPerRow * 5.0f);
            //Debug.Log("Decimal: " + aux);
            //Debug.Log("MAX: " + Mathf.Max(numberOfColumnsPerRow * 5, shinseiVault.Count));
            int totalCells = (int)Math.Ceiling((float)(shinseiVault.Count / (numberOfColumnsPerRow * 5.0f)));
            totalCells *= (numberOfColumnsPerRow * 5);

            Debug.Log("MAX: " + totalCells);

            for (int i = 0; i < totalCells; i++)
            //for (int i = 0; i < Mathf.Max(numberOfColumnsPerRow * 5, shinseiVault.Count); i++)
            //for (int i = 0; i < 120; i++)
            {
                if (i % (numberOfColumnsPerRow * 5) == 0)
                {
                    currentBoxPrefab = Instantiate(boxPrefab, shinseiSlotParent);
                    currentBoxPrefab.gameObject.SetActive(true);
                }
                if (i % numberOfColumnsPerRow == 0)
                {
                    currentRowPrefab = Instantiate(rowPrefab, currentBoxPrefab);
                    currentRowPrefab.gameObject.SetActive(true);
                }

                if (i < shinseiVault.Count)
                {
                    ShinseiSlot NewSlot = Instantiate(shinseiSlotPrefab, currentRowPrefab);
                    NewSlot.gameObject.SetActive(true);
                    string shinseiName = "";

                    shinseiName = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(PlayerDataManager.Singleton.localPlayerData.ShinseiVault.ShinseiVaultList[i].ShinseiDna);
                    NewSlot.ChangeShinseiSlotValues(
                        Constants.SHINSEI_VAULT + i,
                        i,
                        shinseiVault[i],
                        shinseiName);

                    NewSlot.OnSlotClicked.AddListener(OnVaultShinseiSelected);
                    shinseiSlots.Add(NewSlot);
                }
                else
                {
                    Transform emptyGO = Instantiate(emptyPrefab, currentRowPrefab);
                    emptyGO.gameObject.SetActive(true);
                }
            }
        }
        #endregion UnityMethods

        #region OnClick
        private void Update()
        {
            HideIfClickedOutside();
        }

        public void OnVaultShinseiSelected(int listIndex, ShinseiSlot vaultShinseiSlot)
        {
            if (currentSlot)
            {
                currentSlot.transform.GetChild(0).GetComponent<Image>().enabled = false;
            }
            if (!currentSlot || vaultShinseiSlot.listIndex != currentSlot.listIndex)
            {
                shinseiPreviewPanel.gameObject.SetActive(true);
                currentSlot = vaultShinseiSlot;
                currentSlot.transform.GetChild(0).GetComponent<Image>().enabled = true;
                shinseiPreviewPanel.DisplayPreview(vaultShinseiSlot.shinsei, isVault: true);
                Debug.Log("DisplayPreview 08");
            }
            else
            {
                shinseiPreviewPanel.gameObject.SetActive(false);
                currentSlot.transform.GetChild(0).GetComponent<Image>().enabled = false;
                currentSlot = null;
                //shinseiSlotSelected = false;
                //selectedShinseiPanel.SetActive(false);
                //HideSelectedShinsei();
                /*shinseiPreviewPanel.DisplayPreview(vaultShinseiSlot.shinsei, isVault: false);
                currentSlot = null;*/
            }

            //Debug.Log("ENTRO AQUI CAMBIO 0: " + currentSlot);

            shinseiPreviewPanel.DisplayPreview(vaultShinseiSlot.shinsei, isVault: true);
            Debug.Log("DisplayPreview 09");

            shinseiSlotSelected = true;
            partyManager.ChangeOnClickSlotAction((shinseiPartyListIndex, partyShinseiSlot) =>
            {
                //Debug.Log("ENTRO AQUI CAMBIO 01");
                string shinseiKeyTemp = vaultShinseiSlot.shinseiKey;
                int listIndexTemp = vaultShinseiSlot.listIndex;
                Shinsei shineiTemp = vaultShinseiSlot.shinsei;
                string shineiName = vaultShinseiSlot.shinseiName;
                //Debug.Log("ENTRO AQUI CAMBIO 02");
                vaultShinseiSlot.ChangeShinseiSlotValues(newSlot: partyShinseiSlot, listIndex: vaultShinseiSlot.listIndex);
                //Debug.Log("ENTRO AQUI CAMBIO 03");
                int listIndexTempParty = partyShinseiSlot.listIndex;
                partyShinseiSlot.ChangeShinseiSlotValues(
                                    shinseiKeyTemp,
                                    listIndexTempParty,
                                    shineiTemp,
                                    shineiName);
                //Debug.Log("ENTRO AQUI CAMBIO 04");
                if (partyShinseiSlot.IsCompanion) {
                    //Debug.Log("ENTRO AQUI CAMBIO 05");
                    PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion = partyShinseiSlot.shinsei;
                }
                else {
                    //Debug.Log("ENTRO AQUI CAMBIO 06");
                    PlayerDataManager.Singleton.localPlayerData.ShinseiParty[shinseiPartyListIndex] = partyShinseiSlot.shinsei;
                }
                //Debug.Log("ENTRO AQUI CAMBIO 07");
                PlayerDataManager.Singleton.localPlayerData.ShinseiVault.ShinseiVaultList[listIndexTemp] = vaultShinseiSlot.shinsei;
                PlayerDataManager.Singleton.UpdatePlayerData();
                currentSlot.transform.GetChild(0).GetComponent<Image>().enabled = false;
                currentSlot = null;
                PlayerDataManager.Singleton.localPlayerData.onPartyChange?.Invoke();
                HideSelectedShinsei();
                //Debug.Log("ENTRO AQUI CAMBIO 08");
            });

            if(!currentSlot)
            {
                HideSelectedShinsei();
            }
            Debug.Log("DisplayPreview 10");
        }

        private void noSelect()
        {

        }

        private void HideIfClickedOutside()
        {
            if (currentSlot == null)
                return;

            bool isInsideSlot = !RectTransformUtility.RectangleContainsScreenPoint(currentSlot.GetComponent<RectTransform>(),
                                                                                   Input.mousePosition,
                                                                                   Camera.main);

            if (Input.GetMouseButton(0) && !isInsideSlot)
            {
                //TODO: Change by highlighting and remove line bellow
                currentSlot.GetComponent<Image>().color = Color.white;
                shinseiPreviewPanel.gameObject.SetActive(false);
                currentSlot = null;
            }
        }
        public void DisplayPreviewForShisnei(int shinseIndex)
        {
            shinseiPreviewPanel.DisplayPreview(shinseiSlots[shinseIndex].shinsei, isVault: true);
            Debug.Log("DisplayPreview 10");
        }
        #endregion OnClick

        #region Hide 
        private void HideSelectedShinsei(int listIndex = -1, ShinseiSlot eventShisneiSlot = null)
        {
            partyManager.ChangeOnClickSlotAction();
            shinseiSlotSelected = false;
            selectedShinseiPanel.SetActive(false);
        }
        #endregion Hide
        #endregion ----Methods----
    }
}