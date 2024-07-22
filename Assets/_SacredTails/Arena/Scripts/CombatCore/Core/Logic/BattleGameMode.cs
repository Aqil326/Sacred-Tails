using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Timba.Games.DynamicCamera;
using PlayFab.MultiplayerModels;
using Timba.SacredTails.Database;
using System;
using TMPro;
using Timba.Games.SacredTails;
using Timba.Games.SacredTails.PopupModule;
using PlayFab;
using CoreRequestManager;
using Newtonsoft.Json;
using UnityEngine.UI;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Photoboot;
using Timba.Games.CharacterFactory;
using Timba.SacredTails.CharacterStyle;
using Random = UnityEngine.Random;
using Timba.Games.SacredTails.WalletModule;

namespace Timba.SacredTails.Arena
{

    public class BattleGameMode : MonoBehaviour
    {
        #region ----Fields----
        #region General Fields
        [Header("General Fields")]

        public GameSceneManager gameSceneManager;
        public TurnsController turnsController;
        [SerializeField] private BattleShinseiSpawner shinseiSpawner;
        [SerializeField] bool isFirstPlayer = false;
        public float delayTime;
        public int matchIdSeed;
        public bool isViewingMatch = false;
        #endregion General Fields 

        #region Terrain Info
        [Header("Terrain Info")]
        public BattleTerrainBehavioursBase currentTerrain;
        #endregion Terrain Info

        #region LocalPlayer Info
        [Header("Player Info")]

        public UserInfo playerInfo = new UserInfo();
        public Combat localCombat;
        #endregion LocalPlayer Info

        #region Enemy Info
        [Header("Enemy Info")]
        public UserInfo enemyInfo = new UserInfo();
        #endregion Enemy Info

        #region UI
        [Header("UI")]

        [SerializeField] BattleUIController battleUIController;
        #endregion UI

        #region Initial Shinsei Selection
        [Header("Initial Shinsei Selection")]

        [SerializeField] List<List<Shinsei>> playerAllShinseis = new List<List<Shinsei>>();
        [SerializeField] private ArenaShinseiSelectionController selectShinseiForBattleController;
        private bool shinseisSelected = false;
        #endregion Initial Shinsei Selection

        #region Shinsei Change
        [Header("Shinsei Change")]

        [SerializeField] private List<GameObject> shinseiSlotPrefab;
        [SerializeField] private GameObject shinseiSelectorPanel;
        private List<ShinseiSlot> slotsChangeShinsei = new List<ShinseiSlot>();
        [Header("Endgame")]
        public TMP_Text titleEndMatchPanel;
        public List<Sprite> blackSprites;
        public List<Sprite> whiteSprites;
        public List<TextMeshProUGUI> texts;
        public List<Image> changeColorElements, changeColorInvertElements;
        public List<Image> changeSpriteElements;
        public List<Color> colors;
        #endregion Shinsei Change
        [Header("Pictures")]
        public Image pictureImg;
        public Image frameImg;
        public Image pictureImgSelect;
        public Image frameImgSelect;
        public Image pictureTeam;
        public Image frameImgTeam;
        public ProfilePictureStyle pictureStyleDB;
        #region Shinsei Party
        [Header("Shinsei Party")]

        [SerializeField] private List<Shinsei> playfabDataParty = null;
        #endregion Shinsei Party

        #region EndGame
        public Camera cameraEndBattle;
        #endregion EndGame
        #endregion ----Fields----

        #region ----Methods----
        #region  Init Match


        [SerializeField] private WalletController walletController;
        
        private void OnEnable()
        {
            battleUIController.OnGetValueOfBars += GetValueOfHealth;
        }

        private void Start()
        {
            if (PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle.ContainsKey(PartsOfCharacter.PICTURE))
            {
                pictureImg.sprite = pictureStyleDB.picturesOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE].presetId];
                frameImg.sprite = pictureStyleDB.framingOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME].presetId];
                pictureImgSelect.sprite = pictureStyleDB.picturesOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE].presetId];
                frameImgSelect.sprite = pictureStyleDB.framingOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME].presetId];
                pictureTeam.sprite = pictureStyleDB.picturesOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.PICTURE].presetId];
                frameImgTeam.sprite = pictureStyleDB.framingOptions[PlayerDataManager.Singleton.localPlayerData.currentCharacterStyle[PartsOfCharacter.FRAME].presetId];
            }

            #region Get Vault

           // walletController = GetComponent<WalletController>();

              walletController = FindObjectOfType<WalletController>();
            
            #endregion
        }

        private List<int> GetValueOfHealth()
        {
            return new List<int> { playerInfo.healthbars[playerInfo.currentShinseiIndex].currentValue, enemyInfo.healthbars[enemyInfo.currentShinseiIndex].currentValue };
        }


        public void OnStartMatch(GetMatchResult getMatchResult)
        {
            StartCoroutine(OnStartMatchRoutine(getMatchResult));
        }

        public IEnumerator OnStartMatchRoutine(GetMatchResult getMatchResult)
        {
            //Get custom atributes from match result
            List<CustomAtributes> MatchCustomAtributes = new List<CustomAtributes>();

            for (int i = 0; i < getMatchResult.Members.Count; i++)
            {
                if (getMatchResult.Members[i].Attributes.DataObject.GetType() == typeof(CustomAtributes))
                    MatchCustomAtributes.Add(getMatchResult.Members[i].Attributes.DataObject as CustomAtributes);
                else
                    MatchCustomAtributes.Add(JsonUtility.FromJson<CustomAtributes>(getMatchResult.Members[i].Attributes.DataObject.ToString()));
                playfabDataParty = null;
                MakeShinseiPartyUsingPlayfabId(MatchCustomAtributes[i].PlayerPlayfabId);
                //Wait until get data from playfab
                while (playfabDataParty == null)
                    yield return null;
                playerAllShinseis.Add(new List<Shinsei>(playfabDataParty));
            }

            Debug.Log($"Data player 1 {getMatchResult.Members[0].Attributes.DataObject.ToString()} with name {MatchCustomAtributes[0].displayName} and player 2 {getMatchResult.Members[1].Attributes.DataObject.ToString()} with name {MatchCustomAtributes[1].displayName}");

            //Create local match data from download data
            //TO DO: Add confirm bool & create check confirmation for both players
            MatchData currentMatchData = new MatchData()
            {
                MatchId = getMatchResult.MatchId,
                MatchPlayers = new List<CombatPlayer>()
                {
                    new CombatPlayer()
                    {
                        DisplayName = MatchCustomAtributes[0].displayName,
                        PlayfabId = MatchCustomAtributes[0].PlayerPlayfabId,
                        ShinseiParty = playerAllShinseis[0],
                        strikes = 0
                    },
                    new CombatPlayer()
                    {
                        DisplayName = MatchCustomAtributes[1].displayName,
                        PlayfabId = MatchCustomAtributes[1].PlayerPlayfabId,
                        ShinseiParty = playerAllShinseis[1],
                        strikes = 0
                    }
                }
            };

            List<Shinsei> battleShinseis = new List<Shinsei>();
            battleShinseis.AddRange(currentMatchData.MatchPlayers[0].ShinseiParty);
            battleShinseis.AddRange(currentMatchData.MatchPlayers[1].ShinseiParty);
            ServiceLocator.Instance.GetService<IIconGeneration>().GenerateShinseiIcons(battleShinseis);

            yield return new WaitForSeconds(2);
            Initialize(currentMatchData);

            if (!PlayerDataManager.Singleton.isBot)
            {
                selectShinseiForBattleController.Init(currentMatchData, playerInfo.userIndex, isViewingMatch);
                selectShinseiForBattleController.OnShinseisSelected += ShinseiSelected;
            }
            else
            {
                List<int> shinseisSelected = new List<int>() { Random.Range(0, 2), Random.Range(2, 4), Random.Range(4, 6) };
                ShinseiSelected(false, shinseisSelected);
            }
        }

        /// <summary>
        /// Bring shinsei party from playfab
        /// </summary>
        /// <param name="playfabId">Playfab ID</param>
        public void MakeShinseiPartyUsingPlayfabId(string playfabId)
        {
            if (playfabId == "5352E306ACAB3F9B")
            {
                int selectedBotIndex = PlayerPrefs.GetInt("combatNPC",0);
                playfabDataParty = PlayerDataManager.Singleton.CombatNPCs[selectedBotIndex].shinseis;
                return;
            }
            List<string> SelectedShinsei = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                if (i == 0)
                    SelectedShinsei.Add(Constants.SHINSEI_COMPANION);
                else
                    SelectedShinsei.Add(Constants.SHINSEI_SLOT + i);
            }

            PlayfabManager.Singleton.GetUserData(playfabId, SelectedShinsei, (userData) =>
            {
                playfabDataParty = new List<Shinsei>();
                for (int i = 0; i < 6; i++)
                    playfabDataParty.Add(JsonUtility.FromJson<Shinsei>(userData.Data[SelectedShinsei[i]].Value));
            });
        }

        public void Initialize(MatchData matchData)
        {

            SacredTailsLog.LogMessage("Battle has initialize");
            localCombat = new Combat()
            {
                MatchData = matchData,
                CurrentTurn = 0
            };
            if (localCombat.MatchData.MatchPlayers[0].PlayfabId == PlayerDataManager.Singleton.localPlayerData.playfabId)
            {
                playerInfo.userIndex = 0;
                enemyInfo.userIndex = 1;
            }
            else
            {
                playerInfo.userIndex = 1;
                enemyInfo.userIndex = 0;
            }

            playerInfo.isLocalPlayer = true;
            enemyInfo.isLocalPlayer = false;

            turnsController.localPlayer = localCombat.MatchData.MatchPlayers[playerInfo.userIndex];
            turnsController.alteredStateController.InitAlteredStateController(this, battleUIController);
            FirstTurnDecider();
        }

        public void ShinseiSelected(bool endTime, List<int> shinseisPlayer)
        {
            if (endTime)
            {
                GoBackToLobby();
                //return;
            }
            List<Shinsei> selectedShinseis = new List<Shinsei>();
            for (int i = 0; i < shinseisPlayer.Count; i++)
                selectedShinseis.Add(localCombat.MatchData.MatchPlayers[playerInfo.userIndex].ShinseiParty[shinseisPlayer[i]]);

            localCombat.MatchData.MatchPlayers[playerInfo.userIndex].ShinseiParty = selectedShinseis;
            localCombat.MatchData.MatchPlayers[playerInfo.userIndex].shinseisSelected = true;

            //TODO update a secure form of send shinsei party;
            PlayfabManager.Singleton.BattleServerSelectShinseis(
                localCombat.MatchData.MatchId,
                localCombat.MatchData.MatchPlayers[playerInfo.userIndex],
                shinseisPlayer,
                (result) =>
                {
                    SacredTailsPSDto<object> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());

                    if (dto.success)
                        StartCoroutine(WaitForOpponentToSelect());
                }, null);
        }

        public void OnDisable()
        {
            CancelSelectShinsei();
        }
        public void OnApplicationQuit()
        {
            CancelSelectShinsei();
        }

        public void CancelSelectShinsei()
        {
            if (localCombat.MatchData.MatchPlayers[playerInfo.userIndex].shinseisSelected)
                return;

            PlayfabManager.Singleton.BattleServerSelectShinseis(
                localCombat.MatchData.MatchId,
                null,
                new List<int> { -1, -1, -1 }, null, null);
        }

        public void WaitForOpponentToSelectViewMatch()
        {
            selectShinseiForBattleController.Init(new MatchData(), playerInfo.userIndex, isViewingMatch);
            battleUIController.HideEverythingForWatchMatch(isViewer: true);
            StartCoroutine(WaitForOpponentToSelect());
        }

        public IEnumerator WaitForOpponentToSelect()
        {
            while (!shinseisSelected)
            {
                PlayfabManager.Singleton.BattleServerCheckShinseisChoosed(
                    localCombat.MatchData.MatchId,
                    (result) =>
                    {
                        SacredTailsPSDto<List<CombatPlayer>> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<List<CombatPlayer>>>(result.FunctionResult.ToString());
                        if (!dto.success)
                            return;

                        //Enemy disconnected
                        if (dto.code == 30)
                        {
                            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
                            buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
                            {
                                ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
                                GoBackToLobby();
                            });
                            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("The other player disconnected. Match canceled", buttonsAction);
                            return;
                        }

                        shinseisSelected = true;
                        localCombat.MatchData.MatchPlayers[playerInfo.userIndex] = isViewingMatch ? dto.data[1] : dto.data[0];
                        localCombat.MatchData.MatchPlayers[enemyInfo.userIndex] = isViewingMatch ? dto.data[0] : dto.data[1];

                        //regenerate icons after being overwriten
                        var battleShinseis = new List<Shinsei>();
                        battleShinseis.AddRange(localCombat.MatchData.MatchPlayers[enemyInfo.userIndex].ShinseiParty);
                        battleShinseis.AddRange(localCombat.MatchData.MatchPlayers[playerInfo.userIndex].ShinseiParty);

                        ServiceLocator.Instance.GetService<IIconGeneration>().GenerateShinseiIcons(battleShinseis, () =>
                        {
                            SacredTailsLog.LogMessage("<color=blue> Wait for opponet to select: </color> opponent has selected");

                            if (!battleInited)
                            {
                                InitBattle();
                                battleInited = true;
                            }

                        });
                    });
                yield return new WaitForSeconds(delayTime);
            }
        }

        private bool battleInited = false;

        public void InitBattle()
        {
            SacredTailsLog.LogMessageForBot("Init battle");
            battleUIController.versusPanelController.Show();
            selectShinseiForBattleController.gameObject.SetActive(false);
            battleUIController.gameObject.SetActive(true);

            //Set the current index for the shinsei as 0 
            playerInfo.currentShinseiIndex = 0;
            enemyInfo.currentShinseiIndex = 0;

            //Get shinsei health stats 
            foreach (var player in localCombat.MatchData.MatchPlayers)
            {
                foreach (var shinsei in player.ShinseiParty)
                {
                    ref UserInfo targetInfo = ref enemyInfo;
                    if (player == localCombat.MatchData.MatchPlayers[playerInfo.userIndex])
                        targetInfo = ref playerInfo;

                    targetInfo.healthbars.Add(new ResourceBarValues()
                    {
                        currentValue = shinsei.shinseiHealth,
                        maxValue = shinsei.ShinseiOriginalStats.Health
                    });

                    targetInfo.energybars.Add(new ResourceBarValues()
                    {
                        currentValue = shinsei.shinseiEnergy,
                        maxValue = shinsei.ShinseiOriginalStats.Energy
                    });
                }
            }



            //Set player shinseis and enemy shinseis
            if (isFirstPlayer)
            {
                playerInfo.battleShinseis = localCombat.MatchData.MatchPlayers[0].ShinseiParty;
                enemyInfo.battleShinseis = localCombat.MatchData.MatchPlayers[1].ShinseiParty;
            }
            else
            {
                playerInfo.battleShinseis = localCombat.MatchData.MatchPlayers[1].ShinseiParty;
                enemyInfo.battleShinseis = localCombat.MatchData.MatchPlayers[0].ShinseiParty;
            }

            playerInfo.spawnedShinsei = shinseiSpawner.SpawnPlayerShinseis(false, playerInfo.battleShinseis[0].ShinseiDna);
            enemyInfo.spawnedShinsei = shinseiSpawner.SpawnPlayerShinseis(true, enemyInfo.battleShinseis[0].ShinseiDna);
            FillChangeShinseiSlots();

            StartCoroutine(ShowShinseisInitCams());
        }

        IEnumerator ShowShinseisInitCams()
        {
            List<Sprite> battleSprites = new List<Sprite>();
            List<CharacterType> battleTypes = new List<CharacterType>();
            for (int i = 0; i < 3; i++)
            {
                battleSprites.Add(playerInfo.battleShinseis[i].shinseiIcon);
                battleTypes.Add(playerInfo.battleShinseis[i].shinseiType);
            }
            for (int i = 0; i < 3; i++)
            {
                battleSprites.Add(enemyInfo.battleShinseis[i].shinseiIcon);
                battleTypes.Add(enemyInfo.battleShinseis[i].shinseiType);
            }
            battleUIController.versusPanelController.Init(battleSprites, battleTypes, localCombat.MatchData.MatchPlayers[playerInfo.userIndex].DisplayName, localCombat.MatchData.MatchPlayers[enemyInfo.userIndex].DisplayName);
            PlayerPrefs.SetString("EnemyName", localCombat.MatchData.MatchPlayers[playerInfo.userIndex].DisplayName);
            PlayerPrefs.SetString("EnemyName", localCombat.MatchData.MatchPlayers[enemyInfo.userIndex].DisplayName);
            yield return new WaitForSeconds(4); //6
            battleUIController.versusPanelController.Hide();
            turnsController.camManager.SwitchToCam(CamerasAvailableEnum.MIDDLE_CAMERA);
            turnsController.camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.PLAYER_SHINSEI);
            yield return new WaitForSeconds(2); //3
            turnsController.camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.ENEMY_SHINSEI);
            yield return new WaitForSeconds(2); //3

            turnsController.Init(this, isViewingMatch);
            battleUIController.Init(playerInfo.healthbars[0], enemyInfo.healthbars[0], playerInfo.energybars[0], enemyInfo.energybars[0], localCombat.MatchData.MatchPlayers[playerInfo.userIndex].DisplayName, localCombat.MatchData.MatchPlayers[enemyInfo.userIndex].DisplayName, isViewingMatch, playerInfo.battleShinseis[0].ShinseiDna, enemyInfo.battleShinseis[0].ShinseiDna, playerInfo.battleShinseis[0].shinseiName, enemyInfo.battleShinseis[0].shinseiName,(int) playerInfo.battleShinseis[playerInfo.currentShinseiIndex].ShinseiOriginalStats.level, (int)enemyInfo.battleShinseis[enemyInfo.currentShinseiIndex].ShinseiOriginalStats.level);
            battleUIController.UpdateShinseiPicture(0, playerInfo.battleShinseis[0]);
            battleUIController.UpdateShinseiPicture(1, enemyInfo.battleShinseis[0]);

            float ownSpeed = playerInfo.battleShinseis[playerInfo.currentShinseiIndex].ShinseiOriginalStats.speed;
            float otherSpeed = enemyInfo.battleShinseis[enemyInfo.currentShinseiIndex].ShinseiOriginalStats.speed;
            if (ownSpeed != otherSpeed)
                battleUIController.ShowFaster(ownSpeed > otherSpeed ? 0 : 1);

            UpdateCurrentShinsei(ref playerInfo);


        }
        #endregion Init Match

        #region Shinsei Change 
        public void OpenChangeShinseiPanel()
        {
            for (int i = 0; i < slotsChangeShinsei.Count; i++)
            {
                slotsChangeShinsei[i].ChangeInteractuable(playerInfo.healthbars[i].currentValue <= 0);
                slotsChangeShinsei[i].GetComponent<SelectableUiButton>().SetUnselected();
                if ((playerInfo.healthbars[i].currentValue <= 0))
                    slotsChangeShinsei[i].GetComponent<SelectableUiButton>().SetDisable();
                //slotsChangeShinsei[i].GetComponent<Decolorator>().BlackAndWhite();
                if (slotsChangeShinsei[i].shinsei == playerInfo.battleShinseis[playerInfo.currentShinseiIndex])
                    slotsChangeShinsei[i].GetComponent<SelectableUiButton>().SetSelected();
            }
        }

        private void FillChangeShinseiSlots()
        {
            Debug.Log("SE CAMBIO EL SHINSEI 01");
            var viewController = shinseiSelectorPanel.GetComponent<ShinseiPreviewPanelManager>();
            for (int i = 0; i < shinseiSlotPrefab.Count; i++)
            {
                ShinseiSlot NewSlot = shinseiSlotPrefab[i].GetComponent<ShinseiSlot>();
                SelectableUiButton selectedBtn = shinseiSlotPrefab[i].GetComponent<SelectableUiButton>();
                NewSlot.gameObject.SetActive(true);
                NewSlot.shinseiKey = playerInfo.battleShinseis[i].ShinseiDna;
                NewSlot.shinsei = playerInfo.battleShinseis[i];
                NewSlot.listIndex = i;

                NewSlot.UpdateVisual(null, shinseiDNA: NewSlot.shinseiKey, playerInfo.battleShinseis[i].shinseiIcon);
                NewSlot.ChangeInteractuable(playerInfo.healthbars[i].currentValue <= 0);
                NewSlot.PopulateShinseiTypesSprites(NewSlot.shinsei.ShinseiDna, NewSlot.shinsei.shinseiType);
                if (NewSlot.shinsei == playerInfo.battleShinseis[playerInfo.currentShinseiIndex])
                {
                    viewController.DisplayPreview(NewSlot.shinsei, false, true, index: NewSlot.listIndex);
                    Debug.Log("DisplayPreview 03");
                    selectedBtn.SetSelected();
                }
                NewSlot.OnSlotClicked.AddListener((index, shinsheiSlot) =>
                {
                    viewController.DisplayPreview(NewSlot.shinsei, false, true, index: NewSlot.listIndex);
                    Debug.Log("DisplayPreview 04");
                    viewController.selectBtn.onClick.RemoveAllListeners();
                    viewController.selectBtn.onClick.AddListener(() =>
                    {
                        if (playerInfo.healthbars[index].currentValue > 0 && index != playerInfo.currentShinseiIndex)
                        {
                            turnsController.SendMyTurn(4 + index); /*SendMyTurn(3+index) because the other 4 cards of battle go first*/
                            shinseiSelectorPanel.SetActive(false);
                        }
                    });

                });
                slotsChangeShinsei.Add(NewSlot);
            }
        }


        //updates current shinsei card list 
        public void UpdateCurrentShinsei(ref UserInfo userInfo)
        {
            userInfo.spawnedShinsei.SetCharacterCode(ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStructure(userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiDna));
            userInfo.spawnedShinsei.UpdateVisual();

            if (userInfo.userIndex == playerInfo.userIndex)
            {
                battleUIController.UpdateShinseiPicture(0, userInfo.battleShinseis[userInfo.currentShinseiIndex]);
                for (int i = 0; i < battleUIController.cardUis.Count; i++)
                {
                    if (i < userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiActionsIndex.Count)
                    {
                        ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiActionsIndex[i]);
                        battleUIController.cardUis[i].SetDataCard(actionCard.name, actionCard.Description, actionCard.PpCost.ToString(), actionCard.cardType, actionCard);
                        battleUIController.cardUis[i].OnTurnChange();
                    }
                    else
                        battleUIController.cardUis[i].SetCardEmpty();
                }
                battleUIController.UpdateShinseName(0, userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiDna, userInfo.battleShinseis[userInfo.currentShinseiIndex].shinseiName);
                battleUIController.UpdateShinseiLevel((int)playerInfo.battleShinseis[playerInfo.currentShinseiIndex].ShinseiOriginalStats.level, (int)enemyInfo.battleShinseis[enemyInfo.currentShinseiIndex].ShinseiOriginalStats.level);
            }
            else
            {
                battleUIController.UpdateShinseiPicture(1, userInfo.battleShinseis[userInfo.currentShinseiIndex]);
                battleUIController.UpdateShinseName(1, userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiDna, userInfo.battleShinseis[userInfo.currentShinseiIndex].shinseiName);
            }
        }
        #endregion Shinsei Change

        #region End Battle
        IEnumerator WaitForSeconds(int seconds, Action callback)
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Please Wait...", buttonsAction);
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        public void ShowPopupEndMatch(Action callback, string message, bool localPlayerWon)
        {
            if (isViewingMatch || PlayerDataManager.Singleton.isBot)
            {
                ShowEndResultPanel(true);
                return;
            }

            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
            {
                ShowEndResultPanel(localPlayerWon, callback);
                ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            });

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup(message, buttonsAction);
        }

        public bool hasShowEndResult = false;
        public void ShowEndResultPanel(bool localPlayerWon, Action onEndAnimations = null)
        {

            PlayerDataManager.Singleton.localPlayerData.characterState = Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.LOBBY;
            PlayerDataManager.Singleton.localPlayerData.currentMatchId = "";
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().TickCheckActivates();

            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            turnsController.camManager.SwitchToCam(CamerasAvailableEnum.MIDDLE_CAMERA);
            playerInfo.spawnedShinsei.animator.SetTrigger(localPlayerWon ? "Win" : "Lose");
            turnsController.camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.PLAYER_SHINSEI);

            StartCoroutine(WaitForSecondsCallback(3, () =>
            {
                turnsController.camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.ENEMY_SHINSEI);
                enemyInfo.spawnedShinsei.animator.SetTrigger(localPlayerWon ? "Lose" : "Win");

                StartCoroutine(WaitForSecondsCallback(3, () =>
                {
                    SpawnWinnerShisneis(localPlayerWon, onEndAnimations);
                    if (PlayerDataManager.Singleton.isBot)
                        GoBackToLobby();
                }));
            }));
        }

        bool? isLocalPlayerWon = null;

      
        
        public void SpawnWinnerShisneis(bool localPlayerWon, Action onEndAnimations = null)
        {
            //Spawn Shinsei
            if (hasShowEndResult)
                return;
            hasShowEndResult = true;

            shinseiSpawner.SpawnShinseiEndGame(playerInfo.battleShinseis.Select(x => x.ShinseiDna).ToList<string>(), PlayerDataManager.Singleton.endGamePoint);
            battleUIController.UpdateTimer(0);
            battleUIController.gameObject.SetActive(false);

            cameraEndBattle.transform.position = PlayerDataManager.Singleton.endGamePoint.position;
            cameraEndBattle.gameObject.SetActive(true);

            battleInited = false;
            string resultText = localPlayerWon ? "VICTORY" : "DEFEAT";
            isLocalPlayerWon = localPlayerWon;

            SetShinseNamesEndResult();

            titleEndMatchPanel.text = resultText;
            if (resultText == "VICTORY")
                FindObjectOfType<RareThing>().PlaySound("Win");
            else
                FindObjectOfType<RareThing>().PlaySound("Lose");


            int currentCurrency = walletController.currentCurrency;

            if (localPlayerWon)
            {
                for (int i = 0; i < changeColorElements.Count; i++)
                    changeColorElements[i].color = colors[1];
                for (int i = 0; i < changeColorInvertElements.Count; i++)
                    changeColorInvertElements[i].color = colors[0];
                for (int i = 0; i < changeSpriteElements.Count; i++)
                    changeSpriteElements[i].sprite = whiteSprites[i];
                for (int i = 0; i < texts.Count; i++)
                    texts[i].color = colors[0];
                
                // Add Reward 200 coins
                currentCurrency += 200;
                walletController.AddCurrency(currentCurrency);
            }
            else
            {
                for (int i = 0; i < changeColorElements.Count; i++)
                    changeColorElements[i].color = colors[0];
                for (int i = 0; i < changeColorInvertElements.Count; i++)
                    changeColorInvertElements[i].color = colors[1];
                for (int i = 0; i < changeSpriteElements.Count; i++)
                    changeSpriteElements[i].sprite = blackSprites[i];
                for (int i = 0; i < texts.Count; i++)
                    texts[i].color = colors[1];
                
                
            }
            onEndAnimations?.Invoke();
        }

        private void SetShinseNamesEndResult()
        {
            char[] auxDnaArray = playerInfo.battleShinseis[0].ShinseiDna.ToCharArray();
            String auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            texts[1].text = "Shinsei#" + auxNameID;

            if(PlayfabManager.Singleton.loginWithAddress)
            {
                texts[1].text = "Shinsei#" + playerInfo.battleShinseis[0].shinseiName;
            }

            Debug.Log("Shinsei Name: " + texts[1].text);

            auxDnaArray = playerInfo.battleShinseis[1].ShinseiDna.ToCharArray();
            auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            texts[2].text = "Shinsei#" + auxNameID;

            if (PlayfabManager.Singleton.loginWithAddress)
            {
                texts[2].text = "Shinsei#" + playerInfo.battleShinseis[1].shinseiName;
            }

   

            Debug.Log("Shinsei Name: " + texts[2].text);

            auxDnaArray = playerInfo.battleShinseis[2].ShinseiDna.ToCharArray();
            auxNameID = auxDnaArray[auxDnaArray.Length - 14] + "";
            auxNameID += auxDnaArray[auxDnaArray.Length - 10];
            auxNameID += auxDnaArray[auxDnaArray.Length - 7];
            auxNameID += auxDnaArray[auxDnaArray.Length - 4];
            auxNameID += auxDnaArray[auxDnaArray.Length - 1];
            texts[0].text = "Shinsei#" + auxNameID;

            if (PlayfabManager.Singleton.loginWithAddress)
            {
                texts[0].text = "Shinsei#" + playerInfo.battleShinseis[2].shinseiName;
            }

       

            Debug.Log("Shinsei Name: " + texts[0].text);
        }

        IEnumerator WaitForSecondsCallback(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        public void GoBackToLobbyPopup()
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
            {
                GoBackToLobby();
                ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            });
            buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, () => ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup());

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Stop viewing match and go back to lobby?", buttonsAction);
        }

        public void GoBackToLobby()
        {
            SacredTailsLog.LogMessageForBot("Going back to lobby...");
            if (!PlayerDataManager.Singleton.isBot)
                ServiceLocator.Instance.GetService<IWallet>().ShowUserWallet();

            if (gameSceneManager == null)
                gameSceneManager = FindObjectOfType<GameSceneManager>();

            ServiceLocator.Instance.GetService<IBracketsTournament>().CheckTournamentStateController.CheckTournamentState(isLocalPlayerWon);
            gameSceneManager.EndBattle();
            isLocalPlayerWon = null;
        }

        public bool EndMatchCheck(List<ResourceBarValues> healthBars)
        {
            if (healthBars.Count > 3)
                healthBars.RemoveRange(3, 3);
            bool allDead = healthBars.All(x => x.currentValue <= 0);
            return allDead;
        }

        //Checks both players stats to decide who goes first
        private void FirstTurnDecider()
        {
            if (localCombat.MatchData.MatchPlayers[0].PlayfabId == PlayerDataManager.Singleton.localPlayerData.playfabId)
                isFirstPlayer = true;

            int numberOfLetters = 0;
            foreach (char character in localCombat.MatchData.MatchId)
            {
                if (Char.IsDigit(character))
                    numberOfLetters++;
            }
            matchIdSeed = numberOfLetters;
        }

        #endregion End Battle

        #region Helpers
        public void AddTextToLog(string text, Dictionary<string, string> customCodes = null)
        {
            if (String.IsNullOrEmpty(text))
                return;
            battleUIController.battleNotificationSystem.AddText(text, customCodes);
            Debug.Log(text + "15");
        }

        public void GetOwnCombatData(Action<Combat> onRecieveData)
        {
            string ownJsonCombat = "";
            try
            {
                PlayfabManager.Singleton.GetUserData(localCombat.MatchData.MatchPlayers[playerInfo.userIndex].PlayfabId, new List<string>() { localCombat.MatchData.MatchId },
                result =>
                {
                    if (result.Data.ContainsKey(localCombat.MatchData.MatchId))
                    {
                        ownJsonCombat = result.Data[localCombat.MatchData.MatchId].Value;
                        if (ownJsonCombat != "")
                        {
                            localCombat = UnityNewtonsoftJsonSerializer.Deserialize<Combat>(ownJsonCombat);
                            onRecieveData?.Invoke(localCombat);
                        }
                    }
                });
            }
            catch
            {
                SacredTailsLog.LogMessage("An error ocurred in this moment");
            }
        }

        public int GetCurrentShinseiEnergy()
        {
            try
            {
                return playerInfo.energybars[playerInfo.currentShinseiIndex].currentValue;
            }
            catch
            {
                SacredTailsLog.LogMessage("");
                return 0;
            }
        }
        #endregion Helpers
        #endregion ----Methods----
    }
}

