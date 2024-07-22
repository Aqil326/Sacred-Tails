using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Timba.SacredTails.UiHelpers;
using UnityEngine.EventSystems;
using System;
using Timba.SacredTails.Database;
using Timba.Patterns.ServiceLocator;
using System.Text;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// UI element for card attack
    /// </summary>
    public class CardPreview : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<int> OnClickHandler;
        public int cardIndex;
        public int cardCount;
        [HideInInspector]
        public int ppCost;
        [HideInInspector]
        public bool activatedTooltip = false;

        [Header("imageFields")]
        public Image cardSprite;
        public Image cardType;
        public GameObject cardContainer;

        [Header("Text fields")]
        public TMP_Text cardCountText;
        public TMP_Text cardPP;
        public TMP_Text cardName;
        public TMP_Text cardDesc;

        public bool isTooltip = false;
        public TMP_Text[] info_text;
        public TMP_Text auxInfo;

        List<string> colorKeys = new List<string>
    {
        "[Damage]",
        "[Health]",
        "[Buff]",
        "[Debuff]",
        "[AlteredState]",
        "[BlockActions]",
        "[Random]",
        "[End]"
    };
        List<string> colorValues = new List<string>
    {
        "<color=#FF6700>",
        "<color=#ADFF1D>",
        "<color=#1DBAFF>",
        "<color=#F9F985>",
        "<color=#EB5CFF>",
        "<color=#FFFFFF>",
        "<color=#D0FFBD>",
        "</color>"
    };

        public void SetToggable()
        {
            OnClickHandler += UpdateVisual;
        }

        public void Init(ActionCard card, int cardIndex)
        {
            if (cardContainer != null)
                cardContainer.SetActive(true);
            if (cardSprite != null)
                cardSprite.sprite = card.cardImage;
            cardType.sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(card.cardType).negativeIcon;

            if (cardCountText)
            {
                if (cardCount == 0)
                    cardCountText.transform.parent.gameObject.SetActive(false);
                else
                {
                    cardCountText.transform.parent.gameObject.SetActive(true);
                    cardCountText.text = "x" + cardCount.ToString();
                }
            }
            cardPP.text = card.PpCost.ToString();
            cardName.text = AddSpacesToSentence(card.name);
            ppCost = card.PpCost;
            string description = card.Description;
            for (int i = 0; i < colorKeys.Count; i++)
                description = description.Replace(colorKeys[i], "");
            //description = description.Replace(colorKeys[i], colorValues[i]);
            cardDesc.text = description;
            this.cardIndex = cardIndex;

            //New from tooltip:
            if (isTooltip) //Shade //PocketBigBang //FriendlyFire
            {
                auxInfo.text = "";
                for (int i = 0; i < info_text.Length; i++)
                {
                    if (i < card.BattleActions.Count)
                    {
                        info_text[i].text = GetCardActionText(card.BattleActions[i]);
                    }
                    else
                    {
                        info_text[i].text = "";
                    }
                }
            }
        }

        private string GetCardActionText(BattleActionData battleActionData)
        {
            switch(battleActionData.actionType)
            {
                case ActionTypeEnum.BuffDebuff:
                    string auxTextBuffDebuff = battleActionData.isBuff ? "Buff " : "Debuff ";
                    auxTextBuffDebuff += battleActionData.isSelfInflicted ? "self: " : "enemy: ";
                    auxTextBuffDebuff += battleActionData.isBuff ? "+" : "-";
                    auxTextBuffDebuff += battleActionData.amount;
                    if (battleActionData.isPercertange)
                    {
                        auxTextBuffDebuff += "%";
                    }
                    auxTextBuffDebuff += " " + battleActionData.statToModify + " ";
                    if(battleActionData.turnsDuration < 999)
                    {
                        auxTextBuffDebuff += "for " + battleActionData.turnsDuration + " turns";
                    }
                    else
                    {
                        auxTextBuffDebuff += "untill the end of combat";
                    }
                    if(battleActionData.isBuff)
                    {
                        auxTextBuffDebuff = "<color=#00FAFF>" + auxTextBuffDebuff + "</color>";
                    }
                    else
                    {
                        auxTextBuffDebuff = "<color=#FF7600>" + auxTextBuffDebuff + "</color>";
                    }
                    return auxTextBuffDebuff;
                    //return "BuffDebuff";
                case ActionTypeEnum.Healing:
                    string auxTextHealing = battleActionData.isSelfInflicted ? "Heal self in: " : "Heal enemy in: ";
                    auxTextHealing += battleActionData.amount + " HP";
                    if(battleActionData.bonusPercent > 0)
                    {
                        auxTextHealing += " + " + battleActionData.bonusPercent + "% " + battleActionData.statBonusDamage;
                    }
                    auxTextHealing = "<color=#02FF00>" + auxTextHealing + "</color>";
                    return auxTextHealing;
                //return "Healing";
                case ActionTypeEnum.Damage:
                    string auxTextDamage = battleActionData.isSelfInflicted ? "Damage self: " : "Damage enemy: ";
                    auxTextDamage += battleActionData.amount;
                    if(battleActionData.bonusPercent > 0)
                    {
                        auxTextDamage += " + " + battleActionData.bonusPercent + "% " + battleActionData.statBonusDamage;
                    }
                    if(battleActionData.turnsDuration > 1 && battleActionData.turnsDuration < 999)
                    {
                        auxTextDamage += " for " + battleActionData.turnsDuration + " turns";
                    }
                    if (battleActionData.activateAlteredState)
                    {
                        auxTextDamage += " if is " + battleActionData.alteredStateToActivate + " duplicate Damage";
                    }
                    auxTextDamage = "<color=#FF000B>" + auxTextDamage + "</color>";
                    return auxTextDamage;
                //return "Damage";
                case ActionTypeEnum.PutAlteredState:
                    string alteredStateName = "" + (battleActionData.alteredState != AlteredStateEnum.EvasionChange ? battleActionData.alteredState + "" : "chance of evasion");
                    //string auxTextPutAlteredState = "Apply " + battleActionData.alteredState + " ";
                    string auxTextPutAlteredState = "Apply " + alteredStateName + " ";
                    auxTextPutAlteredState += battleActionData.isSelfInflicted ? "self " : "enemy ";
                    auxTextPutAlteredState += "of " + battleActionData.amount + " for " + battleActionData.turnsDuration + " turns";
                    auxTextPutAlteredState = "<color=#FFF500>" + auxTextPutAlteredState + "</color>";
                    switch (battleActionData.alteredState)
                    {
                        case AlteredStateEnum.EvasionChange:
                            auxInfo.text = "<i><color=#FFFFFF>" + "(Evasion: Increases the chance of dodging an attack)" + "</color></i>";
                            break;
                        case AlteredStateEnum.Ignited:
                            auxInfo.text = "<i><color=#FFFFFF>" + "(Ignited: Burns the target every turn)" + "</color></i>";
                            break;
                        case AlteredStateEnum.Rooted:
                            auxInfo.text = "<i><color=#FFFFFF>" + "(Rooted: Invoke roots causing damage opponent and healing caster)" + "</color></i>";
                            break;
                        case AlteredStateEnum.Bleeding:
                            auxInfo.text = "<i><color=#FFFFFF>" + "(Bleeding: Causes the enemy to bleed every turn)" + "</color></i>";
                            break;
                        default:
                            auxInfo.text = "";
                            break;
                    }
                    return auxTextPutAlteredState;
                //return "PutAlteredState";
                case ActionTypeEnum.SkipTurn:
                    string auxTextSkipTurn = "(Stun: Opponent cannot fight or change shinsei)";
                    auxTextSkipTurn = "<i><color=#FFFFFF>" + auxTextSkipTurn + "</color></i>";
                    return auxTextSkipTurn;
                //return "SkipTurn";
                case ActionTypeEnum.ReflectDamage:
                    string auxTextReflectDamage = "Damage reflected in ";
                    auxTextReflectDamage += battleActionData.amount + " ";
                    auxTextReflectDamage += "for " + battleActionData.turnsDuration + " turns";
                    auxTextReflectDamage = "<i><color=#00FFB4>" + auxTextReflectDamage + "</color></i>";
                    return auxTextReflectDamage;
                //return "ReflectDamage";
                case ActionTypeEnum.StatSwap:
                    string auxTextStatSwap = "(Stat Swap: copies a stat onto another, the stat being copied does not change)";
                    auxTextStatSwap = "<i><color=#FFFFFF>" + auxTextStatSwap + "</color></i>";
                    return auxTextStatSwap;
                    //return "StatSwap";
                /*case ActionTypeEnum.CopyCat:
                    return "CopyCat";*/
                default:
                    return "";
            }
        }

        /*
         [System.Serializable]
            public enum ActionTypeEnum
            {
                BuffDebuff,
                Healing,
                Damage,
                TerrainChange,
                PutAlteredState,
                ChangeShinsei,
                SkipTurn,
                BlockAction,
                Randomize,
                ReflectDamage,
                StatSwap,
                CopyCat,
                TMP
            }
         */

        string AddSpacesToSentence(string text, bool preserveAcronyms = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public void CopyCardIndex(CardPreview cardPreview)
        {
            cardIndex = cardPreview.cardIndex;
            UpdateVisual(cardIndex);
        }

        public void UpdateVisual(int index)
        {
            if (index != 0)
            {
                Init(ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(cardIndex), cardIndex);
                cardIndex = index;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickHandler.Invoke(cardIndex);
        }

        public bool UseInCodeAkSounds = false;

        public Action onPointerDown;
        public Action onPointerEnter;
        public Action onPointerExit;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (UseInCodeAkSounds)
                AkSoundEngine.PostEvent("U_MouseIn", gameObject);
            onPointerDown?.Invoke();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (UseInCodeAkSounds)
                AkSoundEngine.PostEvent("U_Select", gameObject);
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
        }

    }
}