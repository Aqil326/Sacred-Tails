using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Timba.SacredTails.UiHelpers;
using Timba.SacredTails.Database;
using System;
using Timba.Patterns.ServiceLocator;
using Timba.Games.CharacterFactory;
using Timba.Games.SacredTails.WalletModule;
using Timba.Games.SacredTails;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// UI element that show stats and details of Shinsei
    /// </summary>
    public class ShinseiPreviewPanelManager : MonoBehaviour
    {
        #region -----Fields-----
        [Header("Shinsei Panel")]
        public Image shinseiSprite, shinseBackground;
        public BackgroundTypeSwapper backgroundTypeSwapper;
        public Slider health;
        public Slider energy;
        public TMP_Text shinseiName;
        public TMP_Text healthLabel;
        public TMP_Text energyLabel;
        public PositionCounter positionCounter;


        [Header("Shinsei New Stats")]
        public TMP_Text shinseiAttackText;
        public TMP_Text shinseiDefenceText;
        public TMP_Text shinseiSpeedText;
        public TMP_Text shinseiStaminaText;
        public TMP_Text shinseiVigorText;
        public TMP_Text shinseiLevelText;
        public Button levelUpButton;

        // Shinsei New Level
        public static int newLevel;
        public TMP_Text levelUpRuquiredCoinsText;
        public TMP_Text insufficientCoin;

        [Header("Stats Panel")]
        public List<Image> partElements;
        public List<TMP_Text> statLabels;

        [Header("CardsPanel")]
        public List<CardPreview> shinseiCards;

        public Transform weaknessesElements;
        public List<Sprite> weaknessesLevelSprites;
        public Transform strengthsElements;

        [Header("SelectButton")]
        public Button selectBtn;

        public Shinsei currentShinsei;

        //Intarfaces
        public IUIHelpable uiHelper;
        public IDatabase database;
        public BattleGameMode battleGameMode;
        WalletController walletController;
       
        #endregion

        #region -----Methods-----

        private void Awake()
        {
            uiHelper = ServiceLocator.Instance.GetService<IUIHelpable>();
            database = ServiceLocator.Instance.GetService<IDatabase>();
            walletController = FindObjectOfType<WalletController>();
      
        }

       
        public void DisplayPreview(Shinsei shinsei, bool isEnemyPreview = false, bool isSelectionScreen = false, bool isVault = false, bool isCardManagement = false, int index = 0)
        {
            Debug.Log("ENTRO A DisplayPreview 01");

            if (!isVault)
                positionCounter?.EnablePosition(index);
            currentShinsei = shinsei;
            shinseiSprite.sprite = shinsei.shinseiIcon;
            if (backgroundTypeSwapper != null)
                backgroundTypeSwapper.CallByShinseiType(shinseBackground, shinsei.shinseiType);

            if (!isEnemyPreview)
            {
                Debug.Log("ENTRO A DisplayPreview 02");
                shinseiName.text = shinsei.shinseiName;
               
                if (isVault || isCardManagement)
                {
                    Debug.Log("ENTRO A DisplayPreview 03:");
                    SetSlider(health, healthLabel, shinsei.ShinseiOriginalStats.Health, shinsei.ShinseiOriginalStats.Health);
                    SetSlider(energy, energyLabel, shinsei.ShinseiOriginalStats.Energy, shinsei.ShinseiOriginalStats.Energy);
                    shinseiLevelText.text = "Level " + currentShinsei.ShinseiOriginalStats.level.ToString(); //Added for Level Text
                    UpdateLevelUpButtonState();
                }
                else
                {
                    Debug.Log("ENTRO A DisplayPreview 04: Index " + index);
                    if (battleGameMode != null && battleGameMode.playerInfo.healthbars.Count > 0)
                    {
                        SetSlider(health, healthLabel, battleGameMode.playerInfo.healthbars[index].currentValue, battleGameMode.playerInfo.healthbars[index].maxValue);
                        SetSlider(energy, energyLabel, battleGameMode.playerInfo.energybars[index].currentValue, battleGameMode.playerInfo.energybars[index].maxValue);
                    }
                    else
                    {
                        SetSlider(health, healthLabel, shinsei.ShinseiOriginalStats.Health, shinsei.ShinseiOriginalStats.Health);
                        SetSlider(energy, energyLabel, shinsei.ShinseiOriginalStats.Energy, shinsei.ShinseiOriginalStats.Energy);
                    }

                    SetWeaknessesAndStrengths(shinsei.shinseiType);
                }

                if (!isSelectionScreen)
                {
                    if (database == null)
                    {
                        database = ServiceLocator.Instance.GetService<IDatabase>();
                        uiHelper = ServiceLocator.Instance.GetService<IUIHelpable>();
                    }
                    List<float> stats = new List<float>() { shinsei.ShinseiOriginalStats.vigor, shinsei.ShinseiOriginalStats.stamina, shinsei.ShinseiOriginalStats.attack, shinsei.ShinseiOriginalStats.defence, shinsei.ShinseiOriginalStats.speed, shinsei.ShinseiOriginalStats.level};
                    Dictionary<string, string> partTypes = database.GetShinseiPartsTypes(shinsei.ShinseiDna, new CharacterType());
                    SetStatPanel(stats, partTypes, shinsei.shinseiType);
                    shinseiLevelText.text = "Level " + currentShinsei.ShinseiOriginalStats.level.ToString();
                }

                if (!isVault)
                    SetCardsPanel(shinsei);

            }
            
        }

      
        public void LevelUp()
        {
            int currentLevel = (int)currentShinsei.ShinseiOriginalStats.level;

            if (currentLevel >= 0)
            { 
                int requiredAmount;
                
                if(currentLevel == 0)
                {
                    requiredAmount = 200;
                }
                
                else
                {
                    requiredAmount = 200 * currentLevel * 2;
                }

                int currentCurrency = walletController.currentCurrency;
              

                if (requiredAmount <= currentCurrency)
                {

                    currentCurrency -= requiredAmount; 
                    walletController.SubtractCurrency(currentCurrency);
                    currentShinsei.ShinseiOriginalStats.level = currentLevel + 1;
                    newLevel = (int)currentShinsei.ShinseiOriginalStats.level;

                    // Calculate increments based on the new level
                    int attackIncrement = 10 + (newLevel - 1) ; 
                    int defenseIncrement = 15 + (newLevel - 1) ; 
                    int speedIncrement = 15 + (newLevel - 1) ; 
                    int staminaIncrement = 15 + (newLevel - 1) ; 
                    int vigorIncrement = 10 + (newLevel - 1) ;

                    // Update stats with the calculated increments
                    currentShinsei.ShinseiOriginalStats.attack += attackIncrement;
                    currentShinsei.ShinseiOriginalStats.defence += defenseIncrement;
                    currentShinsei.ShinseiOriginalStats.speed += speedIncrement;
                    currentShinsei.ShinseiOriginalStats.stamina += staminaIncrement;
                    currentShinsei.ShinseiOriginalStats.vigor += vigorIncrement;

                    // Update UI elements
                    shinseiLevelText.text = "Level " + newLevel.ToString();
                    shinseiAttackText.text = (currentShinsei.ShinseiOriginalStats.attack).ToString();
                    shinseiDefenceText.text = (currentShinsei.ShinseiOriginalStats.defence).ToString();
                    shinseiSpeedText.text = (currentShinsei.ShinseiOriginalStats.speed).ToString();
                    shinseiStaminaText.text = (currentShinsei.ShinseiOriginalStats.stamina).ToString();
                    shinseiVigorText.text = (currentShinsei.ShinseiOriginalStats.vigor).ToString();

                
                    UpdateLevelUpButtonState();
                    Debug.Log(requiredAmount + " Required Amount");
                }
                else
                {
                    UpdateLevelUpButtonState();
                    Debug.LogError("Insufficient Coin");
                }
 
            }
        }

        public void UpdateLevelUpButtonState()
        {
            int currentLevel = (int)currentShinsei.ShinseiOriginalStats.level;
            int requiredAmount;
            if (currentLevel == 0)
            {
                requiredAmount = 200;
            }
            else
            {
                 requiredAmount = 200 * currentLevel * 2;
            }
            
            int currentCurrency = walletController.currentCurrency;

            levelUpButton.interactable = currentCurrency >= requiredAmount;

            if(levelUpButton.interactable == false)
            {
                insufficientCoin.text = "Insufficient Coin";
            }
            else
            {
                insufficientCoin.text = " ";
            }

            levelUpRuquiredCoinsText.text = "Required: " + requiredAmount.ToString();

            Debug.Log($"Current Level: {currentLevel}, Required Amount: {requiredAmount}, Current Currency: {currentCurrency}");
        }




        public void SetCardsPanel(Shinsei shinsei)
        {
            int i = 0;
            foreach (var cardView in shinseiCards)
            {
                if (i >= shinsei.ShinseiActionsIndex.Count)
                {
                    Debug.Log("Cardview set for preview 01");
                    cardView.cardContainer.SetActive(false);
                    i = shinsei.ShinseiActionsIndex.Count;
                }
                else
                {
                    var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(shinsei.ShinseiActionsIndex[i]);
                    Debug.Log("Cardview set for preview 02 "+ card);
                    cardView.Init(card, shinsei.ShinseiActionsIndex[i]);
                }
                i++;
            }
        }

        public void SetStatPanel(List<float> shinseiStats, Dictionary<string, string> pTypes, CharacterType mainType)
        {
            int i = 0;

            foreach (var statLabel in statLabels)
            {
                statLabel.text = shinseiStats[i].ToString();
                i++;
            }
            
            i = 0;
            float auxScale = 0;
            foreach (var kvp in pTypes)
            {
                var pType = new CharacterType();
                Enum.TryParse(kvp.Value, out pType);
                if (pType == mainType)
                {
                    auxScale = 1.16f;
                    partElements[i].transform.localScale = Vector3.one * auxScale;
                }
                else
                {
                    auxScale = .8f;
                    partElements[i].transform.localScale = Vector3.one * auxScale;
                }

                partElements[i].sprite = uiHelper.AssignIcon(pType).partIcon;
                partElements[i].GetComponent<ElementIcon>().SetElementData(pType, 0, auxScale);
                i++;
            }

            SetWeaknessesAndStrengths(mainType);
        }

        private void SetWeaknessesAndStrengths(CharacterType auxMainType)
        {
            Debug.Log("mainType: " + (int)auxMainType);
            //List<float> multipliersType = ShinseiTypeMatrixHelper.GetAllMultiplierType(0);
            List<float> multipliersType = ShinseiTypeMatrixHelper.GetAllMultiplierType(auxMainType);

            int auxCounter = 0;
            float auxScale = 0;

            uiHelper = ServiceLocator.Instance.GetService<IUIHelpable>();

            foreach (float auxMulti in multipliersType)
            {
                CharacterType auxEnum = (CharacterType)auxCounter;
                Debug.Log(auxEnum.ToString() + " - Weak Multi: " + auxMulti);
                Transform auxGameObject = weaknessesElements.GetChild(auxCounter);
                if (auxMulti > 1)
                {
                    //uiHelper = ServiceLocator.Instance.GetService<IUIHelpable>();
                    auxGameObject.gameObject.GetComponent<Image>().sprite = uiHelper.AssignIcon(auxEnum).partIcon;

                    if (auxMulti <= 1.5f)
                    {
                        auxScale = .8f;
                        auxGameObject.localScale = Vector3.one * auxScale;
                    }
                    else
                    {
                        auxScale = 1.16f;
                        auxGameObject.localScale = Vector3.one * auxScale;
                    }
                    //auxGameObject.GetChild(0).gameObject.GetComponent<Image>().sprite = GetDownArrowType(auxMulti);
                    auxGameObject.GetComponent<ElementIcon>().SetElementData(auxEnum, auxMulti, auxScale);
                    auxGameObject.GetChild(0).gameObject.SetActive(false);
                    auxGameObject.gameObject.SetActive(true);
                }
                else
                {
                    auxGameObject.gameObject.SetActive(false);
                }

                auxCounter++;
            }

            auxCounter = 0;
            auxScale = 0;

            foreach (float auxMulti in multipliersType)
            {
                CharacterType auxEnum = (CharacterType)auxCounter;
                Debug.Log(auxEnum.ToString() + " - Strengt Multi: " + auxMulti);
                Transform auxGameObject = strengthsElements.GetChild(auxCounter);
                if (auxMulti < 1)
                {
                    //uiHelper = ServiceLocator.Instance.GetService<IUIHelpable>();
                    auxGameObject.gameObject.GetComponent<Image>().sprite = uiHelper.AssignIcon(auxEnum).partIcon;

                    if (auxMulti >= .5f)
                    {
                        auxScale = .8f;
                        auxGameObject.localScale = Vector3.one * auxScale;
                    }
                    else
                    {
                        auxScale = 1.16f;
                        auxGameObject.localScale = Vector3.one * auxScale;
                    }
                    //auxGameObject.GetChild(0).gameObject.GetComponent<Image>().sprite = GetDownArrowType(auxMulti);
                    auxGameObject.GetComponent<ElementIcon>().SetElementData(auxEnum, auxMulti, auxScale);
                    auxGameObject.GetChild(0).gameObject.SetActive(false);
                    auxGameObject.gameObject.SetActive(true);
                }
                else
                {
                    auxGameObject.gameObject.SetActive(false);
                }

                auxCounter++;
            }
        }

        private Sprite GetDownArrowType(float auxMulti)
        {
            if(auxMulti > 1f)
            {
                if(auxMulti <= 1.5f)
                {
                    return weaknessesLevelSprites[0];
                }
                else if (auxMulti <= 1.8f)
                {
                    return weaknessesLevelSprites[1];
                }
                else
                {
                    return weaknessesLevelSprites[2];
                }
            }

            return null;
        }

        //weaknessesElements;
        //weaknessesLevelSprites;

        public void SetSlider(Slider slider, TMP_Text label, int value, int maxValue, bool isShinseiPreview = true)
        {
            slider.maxValue = maxValue;
            label.text = value.ToString() + "/" + maxValue.ToString();
            if (isShinseiPreview)
                slider.value = value;
        }

        #endregion
    }
}