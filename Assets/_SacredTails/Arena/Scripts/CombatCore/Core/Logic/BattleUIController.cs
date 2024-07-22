using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Timba.SacredTails.UiHelpers;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// Handles all of the events present in the UI of the Combat arena Mode
    /// </summary>
    public class BattleUIController : MonoBehaviour
    {
        [Header("End Match Panel")]
        [SerializeField] public TMP_Text titleEndMatchPanel;

        [Header("Gameplay UI")]
        [SerializeField] private TMP_Text playerNameLabel;
        [SerializeField] private TMP_Text enemyNameLabel;
        [SerializeField] private Image[] typeSprites;
        [SerializeField] private Sprite[] spriteImages;
        public VersusPanelController versusPanelController;

        [Header("Ping")]
        [SerializeField] private TMP_Text pingLabel;

        [Header("Healthbars fields")]
        [SerializeField] private List<GameObject> playerHealthBarObject = new List<GameObject>();
        [SerializeField] private List<Image> shinseiImages = new List<Image>();
        [SerializeField] private List<Slider> playerHealthBar = new List<Slider>();
        [SerializeField] private List<Slider> playerEnergyBar = new List<Slider>();
        [SerializeField] private List<TMP_Text> playerHealthBarText = new List<TMP_Text>();
        [SerializeField] private List<TMP_Text> playerHealthBarMaxText = new List<TMP_Text>();
        [SerializeField] private List<TMP_Text> playerEnergyBarText = new List<TMP_Text>();
        [SerializeField] private List<TMP_Text> playerEnergyBarMaxText = new List<TMP_Text>();
        [SerializeField] private List<TMP_Text> playerShinseNameText = new List<TMP_Text>();

        [SerializeField] private List<TMP_Text> playerShinseLevelText = new List<TMP_Text>();

        private int health1;
        private int health2;
        private int energy1, energy2;

        private float level1, level2;
        [SerializeField] private List<Image> backgrounds;
        [SerializeField] private BackgroundTypeSwapper backgroundTypeSwapper;

        [Header("Combat fields")]
        public GameObject battleMenu;
        public GameObject viewingBackToLobbyButton;
        public GameObject cardContainer;
        public BattleNotificationSystem battleNotificationSystem;
        public List<CardUI> cardUis;
        [SerializeField] GameObject waitingPanel;
        [SerializeField] List<GameObject> panelsToDeactivate = new List<GameObject>();
        [SerializeField] List<GameObject> panelsToDeactivateViewing = new List<GameObject>();
        [SerializeField] UIGroups uIGroups;

        [SerializeField] private List<Coroutine> healthCoroutines = new List<Coroutine>() { null, null };
        [SerializeField] private List<Coroutine> energyCoroutines = new List<Coroutine>() { null, null };
        [SerializeField] private TMP_Text enemyDisconnectMessage;
        [SerializeField] private TextMeshProUGUI timerText;
        public UIDisolver uIDisolver;

        [Header("Damage/Healing Alerts")]
        [SerializeField]
        private InfoContainer enemyInfoContainer;
        [SerializeField]
        private InfoContainer playerInfoContainer;

        public Image pictureImg;
        public Image frameImg;
        public Image pictureImgSelect;
        public Image frameImgSelect;
        public ProfilePictureStyle pictureStyleDB;

        LocalPlayerData localPlayerData = new LocalPlayerData();
        public ShinseiPreviewPanelManager shinseiPreviewPanelManager;
        public void Init(ResourceBarValues player1Hp, ResourceBarValues player2Hp, ResourceBarValues player1Pp, ResourceBarValues player2Pp, string playerName, string enemyName, bool isViewing = false, string player1ShinseDna = "", string player2ShinseDna = "", string player1ShinseName = "", string player2ShinseName = "", int player1ShinseLevel = 0, int player2ShinseLevel = 0)
        {
            
            
            if (!isViewing)
                battleMenu.SetActive(true);
            else
            {
                viewingBackToLobbyButton.SetActive(true);
                uIGroups.ShowOnlyThisGroupWithDeactivating("viewer");
            }

            foreach (var bar in playerHealthBarObject)
                bar.SetActive(true);

            playerNameLabel.text = playerName;
            health1 = player1Hp.currentValue;
            health2 = player2Hp.currentValue;

            enemyNameLabel.text = enemyName;
            energy1 = player1Pp.currentValue;
            energy2 = player2Pp.currentValue;

           // level1 = shinseiPreviewPanelManager.currentShinsei.ShinseiOriginalStats.level;

           // level2 = shinseiPreviewPanelManager.currentShinsei.ShinseiOriginalStats.level;

            playerShinseLevelText[0].text = player1ShinseLevel.ToString();
            playerShinseLevelText[1].text = player2ShinseLevel.ToString();

            //player health bar
            playerHealthBar[0].maxValue = player1Hp.maxValue;
            playerHealthBar[0].value = health1;
            playerHealthBarText[0].text = health1.ToString();
            playerHealthBarMaxText[0].text = player1Hp.maxValue.ToString();

            //enemy health bar
            playerHealthBar[1].maxValue = player2Hp.maxValue;
            playerHealthBar[1].value = health2;
            playerHealthBarText[1].text = health2.ToString();
            playerHealthBarMaxText[1].text = player2Hp.maxValue.ToString();

            //player energy bar
            playerEnergyBar[0].maxValue = player1Pp.maxValue;
            playerEnergyBar[0].value = energy1;
            playerEnergyBarText[0].text = energy1.ToString();
            playerEnergyBarMaxText[0].text = player1Pp.maxValue.ToString();

            //enemy energy bar
            playerEnergyBar[1].maxValue = player2Pp.maxValue;
            playerEnergyBar[1].value = energy2;
            playerEnergyBarText[1].text = energy2.ToString();
            playerEnergyBarMaxText[1].text = player2Pp.maxValue.ToString();

            char[] auxDnaArray = player1ShinseDna.ToCharArray();
            String auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            playerShinseNameText[0].text = "Shinsei#" + auxNameID;

            if (PlayfabManager.Singleton.loginWithAddress)
            {
                playerShinseNameText[0].text = "Shinsei#" + player1ShinseName;
            }

            Debug.Log("Shinsei Name: " + playerShinseNameText[0].text);

            auxDnaArray = player2ShinseDna.ToCharArray();
            auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            playerShinseNameText[1].text = "Shinsei#" + auxNameID;

            if (PlayfabManager.Singleton.loginWithAddress)
            {
                playerShinseNameText[1].text = "Shinsei#" + player2ShinseName;
            }

            Debug.Log("Shinsei Name: " + playerShinseNameText[1].text);

            playerHealthBar[0].onValueChanged.AddListener((newValue) =>
            {
                playerHealthBarText[0].text = ((int)newValue).ToString();
                ChangeHealthColorBar(0);
            });
            playerHealthBar[1].onValueChanged.AddListener((newValue) =>
            {
                playerHealthBarText[1].text = ((int)newValue).ToString();
                ChangeHealthColorBar(1);
            });

            playerEnergyBar[0].onValueChanged.AddListener((newValue) =>
                playerEnergyBarText[0].text = ((int)newValue).ToString());
            playerEnergyBar[1].onValueChanged.AddListener((newValue) =>
                playerEnergyBarText[1].text = ((int)newValue).ToString());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                uIDisolver.ToggleMaximumValius();
            }
        }

        public void AddPing(int amount)
        {
            UpdatePing(Int32.Parse(pingLabel.text) + amount);
        }

        public void UpdatePing(int amount)
        {
            pingLabel.text = amount.ToString();
            SacredTailsLog.LogMessageForBot("Ping: " + amount);
        }

        public void UpdateShinseiPicture(int playerIndex, Shinsei playerShinsei)
        {
            Debug.Log("SE CAMBIO EL SHINSEI 02");
            shinseiImages[playerIndex].sprite = playerShinsei.shinseiIcon;
            backgroundTypeSwapper.CallByShinseiType(backgrounds[playerIndex], playerShinsei.shinseiType);

            typeSprites[playerIndex].sprite = spriteImages[(int)playerShinsei.shinseiType];
        }

        public void UpdateShinseName(int playerIndex, string shinseiDna, string shinseiName)
        {
            char[] auxDnaArray = shinseiDna.ToCharArray();
            String auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            playerShinseNameText[playerIndex].text = "Shinsei#" + auxNameID;

            if (PlayfabManager.Singleton.loginWithAddress)
            {
                playerShinseNameText[playerIndex].text = "Shinsei#" + shinseiName;
            }

            Debug.Log("Shinsei Name: " + playerShinseNameText[playerIndex].text);

            

        }

        public void UpdateShinseiLevel(int playerShinseiLevel, int enemyShinseiLevel)
        {
            playerShinseLevelText[0].text = "Level " + playerShinseiLevel.ToString();
            playerShinseLevelText[1].text = "Level " + enemyShinseiLevel.ToString();
        }

        public void UpdateTimer(float timeInSeconds, string colorText = null, bool didEnemyDisconnect = true)
        {

            if (timeInSeconds == -1)
                timerText.text = "FIGHT!";
            else
            {
                int minutes = (int)timeInSeconds / 60;
                int seconds = (int)timeInSeconds - (minutes * 60);
                timerText.text = $"{(colorText != null ? $"<color={colorText}>" : "")}{minutes.ToString("00")}:{seconds.ToString("00")}{(colorText != null ? $"</color>" : "")}";
            }

            if (enemyDisconnectMessage.gameObject.activeInHierarchy != (colorText != null))
            {
                enemyDisconnectMessage.gameObject.SetActive(colorText != null);
                if (!didEnemyDisconnect)
                    enemyDisconnectMessage.text = "Can not stablish connection to server, disconnecting...";
            }
        }
        public void ChangeHealthColorBar(int indexBar)
        {
            var currentHealthSlider = playerHealthBar[indexBar];
            var barCurrentColor = playerHealthBar[indexBar].fillRect.GetComponent<Image>();

            if (currentHealthSlider.value <= (currentHealthSlider.maxValue / 4))
            {
                if (barCurrentColor.color != Color.red)
                    barCurrentColor.color = Color.red;
            }
            else if (currentHealthSlider.value <= (currentHealthSlider.maxValue / 2))
            {
                if (barCurrentColor.color != Color.yellow)
                    barCurrentColor.color = Color.yellow;
            }
            else if (currentHealthSlider.value > (currentHealthSlider.maxValue / 2))
            {
                if (barCurrentColor.color != Color.green)
                    barCurrentColor.color = Color.green;
            }
        }

        public void InitializeBars(int playerHealth, int healthBarIndex, int maxValueHealth, int playerEnergy, int energyBarIndex, int maxValueEnergy)
        {
            Debug.Log("SE CAMBIO EL SHINSEI 03");
            if (healthCoroutines.Count > 0)
            {
                if (healthCoroutines[0] != null)
                    StopCoroutine(healthCoroutines[0]);
                if (healthCoroutines[1] != null)
                    StopCoroutine(healthCoroutines[1]);
            }
            if (energyCoroutines[energyBarIndex] != null)
                StopCoroutine(energyCoroutines[energyBarIndex]);

            playerHealthBar[healthBarIndex].value = playerHealth;
            playerEnergyBar[energyBarIndex].value = playerEnergy;
            ChangeHealthColorBar(healthBarIndex);

            playerHealthBar[healthBarIndex].maxValue = maxValueHealth;
            playerEnergyBar[energyBarIndex].maxValue = maxValueEnergy;

            playerHealthBarMaxText[healthBarIndex].text = maxValueHealth.ToString();
            playerEnergyBarMaxText[energyBarIndex].text = maxValueEnergy.ToString();

            ChangeHealthbarView();
            Debug.Log("Apply Update Health 04");
        }

        [SerializeField] List<HeadMessages> headMessages;
        public void ShowFaster(int index)
        {
            headMessages[index].ShowMessage("Faster");
        }

        public bool EndMatchCheck(bool isLocalPlayerHealthBars)
        {
            return playerHealthBar[isLocalPlayerHealthBars ? 0 : 1].value <= 0;
        }

        public Func<List<int>> OnGetValueOfBars;
        public void ChangeHealthbarView()
        {
            Debug.Log("SE CAMBIO EL SHINSEI 04");
            List<int> values = OnGetValueOfBars?.Invoke();
            if (values == null)
                SacredTailsLog.LogErrorMessage("Error updating bars");

            if (healthCoroutines.Count > 0)
            {
                if (healthCoroutines[0] != null)
                    StopCoroutine(healthCoroutines[0]);
                if (healthCoroutines[1] != null)
                    StopCoroutine(healthCoroutines[1]);
            }

            healthCoroutines[0] = StartCoroutine(UpdateDataInBars(playerHealthBar[0], values[0], 0));
            healthCoroutines[1] = StartCoroutine(UpdateDataInBars(playerHealthBar[1], values[1], 1));
        }

        public void ChangeHealthbarView(string locCustom, List<NotifyDamageInfo> locNotifyDamageInfo, bool locIsPlayer)
        {
            Debug.Log("SE CAMBIO EL SHINSEI 05");
            Debug.Log("ChangeHealthbarView " + locCustom);

            List<int> values = OnGetValueOfBars?.Invoke();
            if (values == null)
                SacredTailsLog.LogErrorMessage("Error updating bars");

            if (healthCoroutines.Count > 0)
            {
                if (healthCoroutines[0] != null)
                    StopCoroutine(healthCoroutines[0]);
                if (healthCoroutines[1] != null)
                    StopCoroutine(healthCoroutines[1]);
            }

            healthCoroutines[0] = StartCoroutine(UpdateDataInBars(playerHealthBar[0], values[0], 0));
            healthCoroutines[1] = StartCoroutine(UpdateDataInBars(playerHealthBar[1], values[1], 1));

            if (!locIsPlayer)
            {
                enemyInfoContainer.ShowInfoText(locNotifyDamageInfo, battleNotificationSystem, locIsPlayer);
            }
            else
            {
                playerInfoContainer.ShowInfoText(locNotifyDamageInfo, battleNotificationSystem, locIsPlayer);
            }
        }

        public void ApplyEnergyChange(int energyBarIndex, int newEnergyValue)
        {
            Debug.Log("CHANGE SHINSEI 06");
            if (energyCoroutines[energyBarIndex] != null)
                StopCoroutine(energyCoroutines[energyBarIndex]);
            energyCoroutines[energyBarIndex] = StartCoroutine(UpdateDataInBars(playerEnergyBar[energyBarIndex], newEnergyValue));
        }

        public void HideEverythingForWatchMatch(bool isViewer)
        {
            foreach (var item in panelsToDeactivate)
                item.SetActive(false);

            if (isViewer)
                foreach (var item in panelsToDeactivateViewing)
                    item.SetActive(false);


            if (isViewer)
                uIGroups.ShowOnlyThisGroupWithDeactivating("viewerSelect");
            else
                uIGroups.ShowOnlyThisGroup("waiting");
        }

        public void WaitingMessageSetActive (bool auxActive)
        {
            waitingPanel.SetActive(auxActive);
        }

        public void ToggleWaitingPrompt(bool isWaiting, bool isSkipTurn = false)
        {
            waitingPanel.GetComponentInChildren<TMP_Text>().text = isSkipTurn ? "Sending turn..." : "Waiting for enemy...";
            Debug.Log("waiting: " + waitingPanel.GetComponentInChildren<TMP_Text>().text);
            waitingPanel.SetActive(isWaiting);
            if (isWaiting)
            {
                uIGroups.ShowOnlyThisGroup("waiting");
                foreach (var item in panelsToDeactivate)
                    item.SetActive(false);
            }
            else
                uIGroups.ShowOnlyThisGroup("battle");
            battleMenu.SetActive(!isWaiting);
        }
        public void ShowCards(bool isShow)
        {
            cardContainer.SetActive(isShow);
        }
        private IEnumerator UpdateDataInBars(Slider targetHpBar, int newHealth, int barIndex = -1)
        {
            float lerpSpeed = 2.5f;
            float lerpValue = 0f;
            float currHp = targetHpBar.value;

            while (targetHpBar.value != newHealth)
            {
                lerpValue += lerpSpeed * Time.deltaTime;
                targetHpBar.value = Mathf.Lerp(currHp, newHealth, lerpValue);
                yield return null;
            }
            if (barIndex != -1)
                ChangeHealthColorBar(barIndex);
        }
    }
}

