using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using Timba.SacredTails.Database;
using UnityEngine.UI;
using System;
using Timba.SacredTails.UiHelpers;
using Timba.Patterns.ServiceLocator;
using Timba.Games.CharacterFactory;

namespace Timba.SacredTails.Arena
{
    public class ShinseiSlot : MonoBehaviour, IPointerClickHandler
    {
        #region ----Fields----
        public bool IsCompanion { get; set; } = false;
        public string shinseiKey;
        public string shinseiName;
        public int listIndex;
        public Shinsei shinsei;
        public Image shinseiView;

     //   public int shinseiLevel;

        [Header("info panel fields")]
        public Button _infoBtn;
        public TMP_Text _shinseiName;
        public TMP_Text _health;
        public TMP_Text _energy;
        public Slider _helathBar;
        public Slider _energyBar;
        public Color _higlightColor;
        public List<Image> _shinseiTypesImg;
    //    public TMP_Text _shinseiLevel;

        [Header("interaction fields")]
        public bool isPreviewOnly;
        public bool isLocked;
        public bool deactivateSlotOnClick;

        [Header("Extension Fields")]
        public Button previewBtn;

        public TextMeshProUGUI Counter;
        //[SerializeField] TextMeshProUGUI slotText;
        private string slotText;
        public UnityEvent<int, ShinseiSlot> OnSlotClicked;
        #endregion ----Fields----

        #region ----Methods----
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isPreviewOnly || (isLocked && deactivateSlotOnClick))
                return;
            OnSlotClicked?.Invoke(listIndex, this);
        }

        //public void UpdateVisual(string shinseiName = null, string shinseiDNA = null, Sprite shinseiIcon = null, int shinseiLevel = 0)
        //{
        //    if (shinseiDNA != null)
        //        slotText = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(shinseiDNA);
        //    else if (shinseiName != null)
        //        slotText = shinseiName;

        //    if (shinseiIcon != null)
        //        shinseiView.sprite = shinseiIcon;

        //    this.shinseiName = slotText;

        //    if(_shinseiName != null)
        //    {
        //        char[] auxDnaArray = shinsei.ShinseiDna.ToCharArray();
        //        String auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
        //        auxNameID += auxDnaArray[auxDnaArray.Length - 10];
        //        auxNameID += auxDnaArray[auxDnaArray.Length - 7];
        //        auxNameID += auxDnaArray[auxDnaArray.Length - 4];
        //        auxNameID += auxDnaArray[auxDnaArray.Length-1];

        //        _shinseiName.text = "Shinsei#" + auxNameID;

        //        if (PlayfabManager.Singleton.loginWithAddress)
        //        {
        //            _shinseiName.text = "Shinsei#" + shinsei.shinseiName;
        //        }

        //        Debug.Log("Shinsei Name: " + _shinseiName.text);
        //        Debug.LogError(auxNameID + "Yah Hai Shinsei ki id....................");

        //    }
        //}

        public void UpdateVisual(string shinseiName = null, string shinseiDNA = null, Sprite shinseiIcon = null /*int shinseiLevel = 0*/)
        {
            if (shinseiDNA != null)
                slotText = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(shinseiDNA);
            else if (shinseiName != null)
                slotText = shinseiName;

            if (shinseiIcon != null)
                shinseiView.sprite = shinseiIcon;

            this.shinseiName = slotText;
          /*  this.shinseiLevel = shinseiLevel; */ // Add this line

            if (_shinseiName != null)
            {
                char[] auxDnaArray = shinsei.ShinseiDna.ToCharArray();
                String auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
                auxNameID += auxDnaArray[auxDnaArray.Length - 10];
                auxNameID += auxDnaArray[auxDnaArray.Length - 7];
                auxNameID += auxDnaArray[auxDnaArray.Length - 4];
                auxNameID += auxDnaArray[auxDnaArray.Length - 1];

                _shinseiName.text = "Shinsei#" + auxNameID;

                if (PlayfabManager.Singleton.loginWithAddress)
                {
                    _shinseiName.text = "Shinsei#" + shinsei.shinseiName;
                }

                Debug.Log("Shinsei Name: " + _shinseiName.text);
                //Debug.LogError(auxNameID + "Yah Hai Shinsei ki id....................");
            }

            // Update shinseiLevel display
            //if (_shinseiLevel != null)
            //{
            //    _shinseiLevel.text = $"Level: {shinseiLevel}";


            //    //Debug.LogError(_shinseiLevel.text + "Shinsei : " + shinseiName);
            //}
        }



        //10030000011001001000100400300210050010031009003004001
        //39341
        //10080000011005001000100800200210120070031006001004008
        //36148
        //10010010011005001000100600200210110040031004001004005
        //34145
        //10040020011000000000100000100210050000031006003004000
        //36340
        //10060020011011000000100300200210100030031008000004011
        //38041
        //10100010011005003000100400000210090030031009003004009
        //39349

        //public void ChangeShinseiSlotValues(string shinseiKey = null, int listIndex = -1, Shinsei shinsei = null, string name = null, ShinseiSlot newSlot = null)
        //{
        //    if (newSlot == null)
        //    {
        //        if (shinseiKey != null)
        //            this.shinseiKey = shinseiKey;
        //        if (listIndex != -1)
        //            this.listIndex = listIndex;
        //        if (shinsei != null)
        //            this.shinsei = shinsei;

        //        UpdateVisual(name != null ? name : this.shinseiName, null, this.shinsei.shinseiIcon);
        //    }
        //    else
        //    {
        //        this.shinseiKey = shinseiKey == null ? newSlot.shinseiKey : shinseiKey;
        //        this.listIndex = listIndex == -1 ? newSlot.listIndex : listIndex;
        //        this.shinsei = shinsei == null ? newSlot.shinsei : shinsei;
        //        UpdateVisual(name == null ? newSlot.shinseiName : name, null, this.shinsei.shinseiIcon);
        //    }
        //}

        public void ChangeShinseiSlotValues(string shinseiKey = null, int listIndex = -1, Shinsei shinsei = null, string name = null, ShinseiSlot newSlot = null)
        {
            if (newSlot == null)
            {
                if (shinseiKey != null)
                    this.shinseiKey = shinseiKey;
                if (listIndex != -1)
                    this.listIndex = listIndex;
                if (shinsei != null)
                    this.shinsei = shinsei;

                UpdateVisual(name != null ? name : this.shinseiName, null, this.shinsei.shinseiIcon/* this.shinseiLevel*/);  // Pass shinseiLevel here
            }
            else
            {
                this.shinseiKey = shinseiKey == null ? newSlot.shinseiKey : shinseiKey;
                this.listIndex = listIndex == -1 ? newSlot.listIndex : listIndex;
                this.shinsei = shinsei == null ? newSlot.shinsei : shinsei;
                UpdateVisual(name == null ? newSlot.shinseiName : name, null, this.shinsei.shinseiIcon/*, newSlot.shinseiLevel*/);  // Pass shinseiLevel here
            }
        }


        public void ChangeInteractuable(bool slotIsLocked = false, bool deactivateSlotOnClick = true)
        {
            if (isLocked == slotIsLocked)
                return;
            isLocked = slotIsLocked;
            this.deactivateSlotOnClick = deactivateSlotOnClick;

            SacredTailsLog.LogMessage($"slotsIsActive: {slotIsLocked}");
            SetDataColor();
        }

        public void SetDataColor()
        {
            try
            {

                if (!isLocked)
                {
                    _health.color = Color.white;
                    _energy.color = Color.white;
                    _shinseiName.color = Color.white;
                }
                else
                {
                    _health.color = Color.black;
                    _energy.color = Color.black;
                    _shinseiName.color = Color.black;
                }
            }
            catch (Exception ex)
            {
                SacredTailsLog.LogErrorMessage($"<color=red>ShinseiSlot.cs: </color> Image component not found. \n StackTrace: \n {ex.StackTrace}", gameObject);
            }
        }

        public void PopulateShinseiTypesSprites(string dna, CharacterType mainType)
        {
            var pTypes = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiPartsTypes(dna, new CharacterType());
            int i = 0;
            float auxScale = 0;
            foreach (var kvp in pTypes)
            {
                var pType = new CharacterType();
                Enum.TryParse(kvp.Value, out pType);
                if (pType == mainType)
                {
                    auxScale = 1.16f;
                    _shinseiTypesImg[i].transform.localScale = Vector3.one * auxScale;
                }
                else
                {
                    auxScale = .8f;
                    _shinseiTypesImg[i].transform.localScale = Vector3.one * auxScale;
                }

                _shinseiTypesImg[i].sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(pType).partIcon;
                _shinseiTypesImg[i].GetComponent<ElementIcon>().SetElementData(pType, 0, auxScale);
                i++;
            }
        }
        #endregion ----Methods----
    }
}