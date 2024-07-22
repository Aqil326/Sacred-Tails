using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Timba.SacredTails.UiHelpers;
using UnityEngine.EventSystems;
using System;
using Timba.Patterns.ServiceLocator;
using Timba.Games.CharacterFactory;
using System.Text;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// UI element of cards from the card store also show a preview of card with details
    /// </summary>
    public class CardUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI textTitle, textEnergy;
        [SerializeField] TMPChangeColor textEnergyChangeColor;
        public string textDescription;
        [SerializeField] TMP_Text tooltipText;
        [SerializeField] TMP_Text tooltipName;
        [SerializeField] TMP_Text tooltipPP;
        [SerializeField] GameObject tooltipObject;
        [SerializeField] Image imagen;
        [SerializeField] Button button;
        [SerializeField] EventTrigger eventTrigger;
        [SerializeField] GameObject effectivenessContainer;
        [SerializeField] TMP_Text effectivenessText;
        public BattleGameMode battleGameMode;
        private CharacterType cardType;

        public float cardEnergy = 0;

        /*List<string> colorKeys = new List<string>
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
    };*/
        List<string> colorKeys = new List<string>
    {
        "[Damage]",
        "[Health]",//"ocum[Health]",
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

        public void OnTurnChange()
        {
            Debug.Log("On turn change");
            ProcessTypeMatrixResult();
            if (string.IsNullOrEmpty(textEnergy.text) || battleGameMode.GetCurrentShinseiEnergy() < int.Parse(textEnergy.text))
            {
                button.interactable = false;
                textEnergy.color = new Color(255, 0, 0);
                textEnergyChangeColor.currentHighlight = textEnergy.color;
                textEnergyChangeColor.currentNormal = textEnergy.color;
            }
            else
            {
                button.interactable = true;
                textEnergy.color = Color.white;
                textEnergyChangeColor.currentHighlight = textEnergyChangeColor.highlight;
                textEnergyChangeColor.currentNormal = textEnergyChangeColor.normal;

            }
        }
        public void SetCardEmpty()
        {
            textTitle.text = "-";
            textDescription = "";
            textEnergy.text = "";
            button.enabled = false;
            imagen.sprite = null;
            eventTrigger.enabled = false;
            imagen.sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(null).battleIcon;
        }

        //TO DO: add this to tooltip object as a new class
        public void DisplayTooltip()
        {
            string description = textDescription;
            for (int i = 0; i < colorKeys.Count; i++)
                description = description.Replace(colorKeys[i], colorValues[i]);
            tooltipText.text = description;
            tooltipName.text = textTitle.text;
            tooltipPP.text = textEnergy.text;
            tooltipObject.SetActive(true);
        }

        public void SetDataCard(string textTitle, string textDescription, string textEnergy, CharacterType cardType, ActionCard card)
        {
            eventTrigger.enabled = true;
            this.textTitle.text = AddSpacesToSentence(textTitle,false);
            this.textDescription = textDescription;
            this.textEnergy.text = textEnergy;
            cardEnergy = float.Parse(textEnergy);
            imagen.sprite = ServiceLocator.Instance.GetService<IUIHelpable>().AssignIcon(cardType).battleIcon;
            this.cardType = cardType;

            shouldShowMatrixType = false;
            foreach (var action in card.BattleActions)
            {
                if (action.actionType == ActionTypeEnum.Damage || (action.actionType == ActionTypeEnum.PutAlteredState && action.alteredState != AlteredStateEnum.EvasionChange && !action.isSelfInflicted))
                {
                    shouldShowMatrixType = true;
                    break;
                }
            }  
        }

        string AddSpacesToSentence(string text, bool preserveAcronyms)
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

        bool shouldShowMatrixType;
        private void ProcessTypeMatrixResult()
        {
            if (shouldShowMatrixType)
            {
                float typeMatrixResult = ShinseiTypeMatrixHelper.GetShinseiTypeMultiplier(cardType, TurnsController.opponentShinsei.shinseiType);
                effectivenessContainer.SetActive(true);
                //Debug.Log("Matrix result : " + typeMatrixResult + " Type for both : "+ TurnsController.playerShinsei.shinseiType+"   opponent: "+ TurnsController.opponentShinsei.shinseiType);
                switch(typeMatrixResult)
                {
                    case 0:
                        effectivenessText.text = "<color=#F54F4F>Minimal Impact</color>";
                        break;
                    case 0.5f:
                        effectivenessText.text = "<color=#F54F4F>Low Impact</color>";
                        break;
                    case 1.0f:
                        effectivenessContainer.SetActive(false);
                        break;
                    case 1.5f:
                        effectivenessText.text = "<color=#2FCC7B>Good Impact</color>";
                        break;
                    case 2.0f:
                        effectivenessText.text = "<color=#2FCC7B>High Impact</color>";
                        break;
                }
            }
            else
            {
                effectivenessContainer.SetActive(false);
            }
        }
    }
}