using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Timba.Games.DynamicCamera;
using PlayFab.MultiplayerModels;
using Timba.SacredTails.Database;
using System;
using Timba.Games.SacredTails.PopupModule;
using FirebaseRequestManager;
using Timba.Games.SacredTails.LobbyDatabase;
using Timba.SacredTails.Arena.ShinseiType;
using CoreRequestManager;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.VFXController;
using Timba.SacredTails.Arena;
using Timba.Games.CharacterFactory;
using PlayFab;
using System.Text.RegularExpressions;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// This class handles the flow of the battle once it has started.
    /// Things like sending turns, recieving the match state and executing the turns with vfx are done/delegated here.
    /// </summary>
    public class TurnsController : MonoBehaviour
    {
        #region ----Fields----
        #region Turn
        [Header("Turn")]
        private Coroutine turnRoutine;
        private Coroutine ownTurnRoutine;
        [SerializeField] private int energyRegenerationAmount = 5;
        [SerializeField] private int maxTurnTimeInSeconds = 60;
        [SerializeField] private float maxSecondsForEnemeyResponse = 30;
        private bool alreadySendTurnOrTimeout = false;
        private int turnStallCount = 0;
        private bool combatInit = false;
        [SerializeField] private GameObject waitingForOtherPlayerToChooseCardPanel;
        [SerializeField] private ShinseiTypeScriptable shinseiTypeScriptable;
        [SerializeField] public List<BattleActionsBase> turnActionsDatabase;
        [SerializeField] private BattleActionsBaseTemplate battleActionsBaseTemplate;
        [SerializeField] public AlteredStateController alteredStateController;
        [HideInInspector] public Dictionary<ActionTypeEnum, BattleActionsBase> turnActionsDatabaseDictionary;
        [SerializeField] private Animator shinseiAnimator;
        [Header("Viewers")]
        [SerializeField] private AlteredView playerAlteredView;
        [SerializeField] private AlteredView opponentAlteredView;
        [SerializeField] private BuffNDebuffViewer playerBuffViewer;
        [SerializeField] private BuffNDebuffViewer opponentBuffViewer;

        [SerializeField] bool canSendTurn = true;
        private event Action OnTurnChangeActions;
        public CombatPlayer localPlayer;
        public List<VFXTypeData> vfxsActionType = new List<VFXTypeData>();
        public bool isViewingMatch = false;
        private bool hasRecieveServerResponse = false;

        public static bool isPlayerChangingShinsei = false;
        public static bool isOpponentChangingShinsei = false;
        public static Shinsei playerShinsei;
        public static Shinsei opponentShinsei;

        private bool CanSendTurn
        {
            get { return canSendTurn; }
            set
            {
                SacredTailsLog.LogMessageForBot($">>>>> Change currentCanSendTurn:{canSendTurn} new:{value} <<<<<<");
                if (value == canSendTurn)
                    return;

                canSendTurn = value;
            }
        }

        private bool isWiner;
        public bool matchEnded = false;

        public List<BattleActionData> battlePlayerCurrentActions = new List<BattleActionData>();
        public List<BattleActionData> battleEnemyCurrentActions = new List<BattleActionData>();
        #endregion Turn

        #region UI
        [Header("UI")]

        [SerializeField] BattleGameMode battleGameMode;
        [SerializeField] BattleUIController battleUIController;
        [SerializeField] public List<CardUI> uiCards = new List<CardUI>();
        [SerializeField] private AliveShinseiPreviewer aliveShinseiPreviewer;
        #endregion UI

        #region Camera
        [Header("Camera")]

        public CameraPlaneController camManager;
        #endregion Camera

        #region VFX
        [Header("VFX")]

        [SerializeField] VFXInstancer vFXInstancer;
        public List<VFXPositionData> vfxPositions;
        public Dictionary<VFXPositionEnum, Transform> vfxPositionsDictionary = new Dictionary<VFXPositionEnum, Transform>();
        [SerializeField] private float actionTime = 6;
        public bool isPlayerSleep = false;
        public bool isEnemySleep = false;
        #endregion VFX

        #region TEST
        [Header("----TEST----")]
        public bool testMatchSendTurnAutomatically = false;
        public int cardToTest = 7;
        #endregion TEST
        #endregion ----Fields----

        #region ----Methods----

        ///<inheritdoc/>
        private void OnApplicationQuit()
        {
            if (PlayerDataManager.Singleton.currentTournamentId.Equals("") || matchEnded)
                return;

            var tournamentReadyController = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.tournamentReadyController;
            PlayerPrefs.SetInt("DisconFromTournament", 1);
            tournamentReadyController.ExitTournament();
        }

        #region Init

        /// <summary>
        /// Initialize turns controller by setting the vfx positions(enemy, center and player),
        /// then  initialize a dictionary with of the battleActions(attack,buffdebuff,etc)
        /// Also it susbscribe to the change of turn to execute the InitNewTurn.
        /// </summary>
        /// <param name="battleGameMode"> Reference to the battlemode in order to acces the users data i.e: shisneis</param>
        /// <param name="isViewing"> if the user is watching a match or participating in the match</param>
        public void Init(BattleGameMode battleGameMode, bool isViewing = false)
        {
            playerShinsei = battleGameMode.playerInfo.battleShinseis[0];
            opponentShinsei = battleGameMode.enemyInfo.battleShinseis[0];

            isViewingMatch = isViewing;

            foreach (var vfxPosition in vfxPositions)
                vfxPositionsDictionary.Add(vfxPosition.vfxPosEnum, vfxPosition.positions);

            CanSendTurn = true;
            this.battleGameMode = battleGameMode;
            turnActionsDatabaseDictionary = new Dictionary<ActionTypeEnum, BattleActionsBase>();
            foreach (BattleActionsBase item in turnActionsDatabase)
            {
                item.Init(camManager, vFXInstancer, battleGameMode, battleUIController, shinseiTypeScriptable);
                turnActionsDatabaseDictionary.Add(item.actionType, item);
            }
            battleActionsBaseTemplate.Init(camManager, vFXInstancer, battleGameMode, battleUIController, shinseiTypeScriptable);

            OnTurnChangeActions += InitNewTurn;
            for (int i = 0; i < uiCards.Count; i++)
            {
                uiCards[i].battleGameMode = battleGameMode;
                uiCards[i].OnTurnChange();
                OnTurnChangeActions += uiCards[i].OnTurnChange;
            }

            turnRoutine = StartCoroutine(TurnCountDown());
            InitNewTurn();
        }
        #endregion Init

        #region Turn flow
        /// <summary>
        /// Init new turn checking if any player has a sleep in his battleActions 
        /// in order to execute an automatic skipturn. This is temporal logic.
        /// The sleep logic should be it's own BattleAction/AlteredState.
        /// </summary>
        public void InitNewTurn()
        {
            SacredTailsLog.LogMessageForBot($">>>>> Init new turn <<<<<<");
            if (isViewingMatch)
            {
                CanSendTurn = false;
                StartCoroutine(WaitOponentTurn());
                return;
            }

            GoToSleep(true);
            GoToSleep(false);

            if (PlayerDataManager.Singleton.isBot)
            {
                StartCoroutine(WaitForSeconds(secondsToSendTurnAfterTurnInit, () => SendBotTurn(battleGameMode.playerInfo,battleGameMode.enemyInfo)));
                return;
            }
        }

        private void GoToSleep(bool isEnemy)
        {
            UserInfo userInfo = !isEnemy ? ref battleGameMode.playerInfo : ref battleGameMode.enemyInfo;
            bool isUserSleep = !isEnemy ? isPlayerSleep : isEnemySleep;

            SacredTailsLog.LogMessageForBot("Go to sleep! it's 3am kendrick: " + isEnemy);
            if (!isEnemy ? CurrentActionsHasSkipTurnForLocalPlayer() : CurrentActionsHasSkipTurnForEnemy())
            {
                SacredTailsLog.LogMessageForBot("Should sleep but lets check if not sleep");
                if (!isUserSleep)
                {
                    userInfo.spawnedShinsei.animator.SetTrigger("Emote");
                    userInfo.spawnedShinsei.animator.SetBool("Sleep", true);

                    if (!isEnemy)
                    {
                        isPlayerSleep = true;
                        battlePlayerCurrentActions = new List<BattleActionData>();
                        battleEnemyCurrentActions.Clear();
                    }
                    else
                    {
                        isEnemySleep = true;
                        battleEnemyCurrentActions = new List<BattleActionData>();
                        battleEnemyCurrentActions.Clear();
                    }

                    SacredTailsLog.LogMessageForBot("Sleep now");
                    string targetText = !isEnemy ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";
                    battleGameMode.AddTextToLog($"{targetText} went to sleep!");
                    Debug.Log($"{(!isEnemy ? "Player" : "Enemy")} went to sleep!" + "17");
                }

                if (!isViewingMatch)
                    battleUIController.ToggleWaitingPrompt(true);

                if (!isEnemy)
                    SendMyTurn(7);
                return;
            }
            else
            {
                SacredTailsLog.LogMessageForBot("Time to wake up for every nation");
                if (isUserSleep)
                {
                    SacredTailsLog.LogMessageForBot("Awaken the centuries");
                    userInfo.spawnedShinsei.animator.SetBool("Sleep", false);
                    userInfo.spawnedShinsei.animator.SetTrigger("Awake");

                    if (!isEnemy) isPlayerSleep = false;
                    else isEnemySleep = false;

                    string targetText = !isEnemy ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";
                    battleGameMode.AddTextToLog($"{targetText} wake up!");
                    Debug.Log($"{(!isEnemy ? "Player" : "Enemy")} wake up!" + "18");
                }
                else
                {
                    SacredTailsLog.LogMessageForBot("NANIII");
                    userInfo.spawnedShinsei.animator.SetTrigger("Recharge");
                }

                if (!isViewingMatch) {
                    battleUIController.ToggleWaitingPrompt(false);
                    if (combatInit)
                    {
                        battleUIController.battleNotificationSystem.AddText("<color=#28BDFA>" + "NEXT TURN!" + "</color>", auxDuration: 1);
                    } else
                    {
                        battleUIController.battleNotificationSystem.AddText("<color=#28BDFA>" + "START COMBAT!" + "</color>", auxDuration: 1);
                    }
                }
                    
            }
        }


        [Header("BOT FIGHT PARAMS")]
        public float secondsToSendTurnAfterTurnInit = 2;

        private void SendBotTurn(UserInfo targetUserInfo, UserInfo enemyUserInfo, Action<string> OnEnd = null)
        {
            string matchId = "TestMatch_" + PlayerDataManager.Singleton.localPlayerData.playfabId;

            List<int> numbersToChooseFrom = new List<int>();

            List<ActionCard> actionCards = new List<ActionCard>();
            //Changes from here
            foreach (var item in targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].ShinseiActionsIndex)
            {
                actionCards.Add(ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(item));
            }
            List<float> benefitByAction = new List<float>();
            //Store current bot type
            BotType thisBotType = PlayerDataManager.Singleton.CombatNPCs[PlayerPrefs.GetInt("combatNPC", 0)].botType;
            foreach (var action in actionCards)
            {
                bool hasDamage = action.BattleActions.Any((battleAction) => battleAction.actionType == ActionTypeEnum.Damage);
                bool hasHealing = action.BattleActions.Any((battleAction) => battleAction.actionType == ActionTypeEnum.Healing);
                bool hasBuffDebuff = action.BattleActions.Any((battleAction) => battleAction.actionType == ActionTypeEnum.PutAlteredState ||
                battleAction.actionType == ActionTypeEnum.BuffDebuff ||
                battleAction.actionType == ActionTypeEnum.ReflectDamage);

                bool enoughPpCost = targetUserInfo.energybars[targetUserInfo.currentShinseiIndex].currentValue >= action.PpCost;

                float numberBenefit = 0;

                foreach (var item in action.BattleActions)
                {
                    if (item.actionType == ActionTypeEnum.Damage)
                        numberBenefit += item.amount * ShinseiTypeMatrixHelper.GetShinseiTypeMultiplier(targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].shinseiType,
                                                                                                        enemyUserInfo.battleShinseis[enemyUserInfo.currentShinseiIndex].shinseiType);
                    if (item.actionType == ActionTypeEnum.Healing)
                    {
                        float totalHeal = item.amount;
                        float maxLife = targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].ShinseiOriginalStats.Health;
                        float currentLife = targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].shinseiHealth;
                        float currentShinseiDamage = maxLife - currentLife;
                        // calculate amount of healing used by the shinsei
                        float heal = currentShinseiDamage - totalHeal;
                        if (heal < 0) //healing is wasted takin that in account for calculate priority
                            heal = currentShinseiDamage;
                        else if (heal >= 0) //shinsei use all healing
                            heal = totalHeal;
                        numberBenefit += heal;
                    }
                }

                //Ad 25 extra points if has buff debuff
                numberBenefit = hasBuffDebuff ? numberBenefit + 25 : numberBenefit;

                switch (thisBotType)
                {
                    case BotType.agressive:
                        //Select the attack ability giving the double value to damage abilities
                            if (hasDamage)
                                numberBenefit = numberBenefit * 2;
                        break;
                    case BotType.thinker:
                        //Select the best ability keeping the original value
                        break;
                    case BotType.random:
                        //select a random ability giving the same value to all abilities
                            numberBenefit = numberBenefit = 10;
                        break;
                    default:
                        break;
                }

                //If not have enought energy remove all points
                if (!enoughPpCost) numberBenefit = 0;
                benefitByAction.Add(CostBenefitRatio(numberBenefit, action.PpCost));
            }
            //Create a list of attacks that you can use by energy
            List<float> enoughtEnergyAttacks = benefitByAction.Where(a => a > 0).ToList();

            if (enoughtEnergyAttacks.Count > 0)
            {
                if (thisBotType == BotType.random)
                {
                    //Add all possible energy enought attacks
                    for (int i = 0; i < enoughtEnergyAttacks.Count; i++)
                        numbersToChooseFrom.Add(actionCards.IndexOf(actionCards[benefitByAction.IndexOf(enoughtEnergyAttacks[i])]));
                }
                else //Only add the attack with priority
                    numbersToChooseFrom.Add(actionCards.IndexOf(actionCards[benefitByAction.IndexOf(benefitByAction.OrderByDescending(a => a).FirstOrDefault())]));
            }
            else
            {
                //Add skip turn
                numbersToChooseFrom.Add(7);
            }

            //bool hasDamageSkill = targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].ShinseiActionsIndex.Any((shinseiAction) =>
            //{
            //    ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(shinseiAction);
            //    return actionCard.BattleActions.Any((battleAction) => battleAction.actionType == ActionTypeEnum.Damage);
            //});

            

            //float randomValue = UnityEngine.Random.value;
            //for (int i = 0; i <= 6; i++)
            //{
            //    if (i <= 3)
            //    {
            //        int trueIndexCard = GetTrueIndexCard(i,targetUserInfo);
            //        ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(trueIndexCard);

            //        //Add avaiable to launch cards based on the ppCost/energy
            //        bool isDamageSkill = actionCard.BattleActions.Any((battleAction) => battleAction.actionType == ActionTypeEnum.Damage);
            //        bool shouldAddNotDamageSkill = (hasDamageSkill && !isDamageSkill && randomValue < .1f);

            //        bool shouldAddSkill = (hasDamageSkill && isDamageSkill) || shouldAddNotDamageSkill || (!hasDamageSkill);
            //        bool enoughPpCost = targetUserInfo.energybars[targetUserInfo.currentShinseiIndex].currentValue >= actionCard.PpCost;

            //        if (shouldAddSkill && enoughPpCost)
            //            numbersToChooseFrom.Add(i);
            //    }
            //    else if (randomValue < .3f)
            //    {
            //        //Add available change shinseis 
            //        if (i != targetUserInfo.currentShinseiIndex + 4)
            //            numbersToChooseFrom.Add(i);
            //    }
            //}
            
            //Send turn
            if (targetUserInfo.isLocalPlayer)
            {
                SendMyTurn(numbersToChooseFrom[UnityEngine.Random.Range(0, numbersToChooseFrom.Count)]);
            }
            else
            {
                //change master player key
                OnEnd?.Invoke("{\r\n  \"CallerEntityProfile\": {\r\n    \"Lineage\": {\r\n      \"MasterPlayerAccountId\": \"5352E306ACAB3F9B\"\r\n    }\r\n  },\r\n  \"FunctionArgument\": {\r\n    \"Keys\": {\r\n      \"MatchId\": \"" + matchId + "\",\r\n      \"indexCard\":" + numbersToChooseFrom[UnityEngine.Random.Range(0, numbersToChooseFrom.Count)] + "\r\n    }\r\n  }\r\n}");
            }
        }

        public float CostBenefitRatio(float damageOrHealing, float energyCost)
        {
            if (energyCost == 0) energyCost = 1;
            // Calculate relation cost/benefy (damageOrHealing / energyCost)
            return (float)damageOrHealing / energyCost;
            
        }

        /// <summary>
        /// Checks the actions of both players to see if any of the both 
        /// has a skip turn directed to the local player
        /// </summary>
        /// <returns> Player has skip turn </returns>
        public bool CurrentActionsHasSkipTurnForLocalPlayer()
        {
            int skipTurnIndexPlayer = battlePlayerCurrentActions.FindIndex(battleAction => battleAction.cardSkipTurn && battleAction.actionType == ActionTypeEnum.SkipTurn && battleAction.turnsDuration > 0);
            int skipTurnIndexEnemy = battleEnemyCurrentActions.FindIndex(battleAction => battleAction.actionType == ActionTypeEnum.SkipTurn && battleAction.turnsDuration > 0);

            bool playerActionsHasSkipTurnForPlayer = skipTurnIndexPlayer != -1 && battlePlayerCurrentActions[skipTurnIndexPlayer].isSelfInflicted;
            bool enemyActionsHasSkipTurnForPlayer = skipTurnIndexEnemy != -1 && !battleEnemyCurrentActions[skipTurnIndexEnemy].isSelfInflicted;

            return playerActionsHasSkipTurnForPlayer || enemyActionsHasSkipTurnForPlayer;
        }

        /// <summary>
        /// Checks the actions of both players to see if any of the both 
        /// has a skip turn directed to the enemy player
        /// </summary>
        /// <returns> Enemy has skip turn </returns>
        public bool CurrentActionsHasSkipTurnForEnemy()
        {
            SacredTailsLog.LogMessageForBot("Un: " + JsonConvert.SerializeObject(battlePlayerCurrentActions, Formatting.Indented));
            SacredTailsLog.LogMessageForBot("Deux: " + JsonConvert.SerializeObject(battleEnemyCurrentActions, Formatting.Indented));
            int skipTurnIndexPlayer = battlePlayerCurrentActions.FindIndex(battleAction => battleAction.actionType == ActionTypeEnum.SkipTurn && battleAction.turnsDuration > 0);
            int skipTurnIndexEnemy = battleEnemyCurrentActions.FindIndex(battleAction => battleAction.cardSkipTurn && battleAction.actionType == ActionTypeEnum.SkipTurn && battleAction.turnsDuration > 0);

            bool enemyActionsHasSkipTurnForEnemy = skipTurnIndexEnemy != -1 && battleEnemyCurrentActions[skipTurnIndexEnemy].isSelfInflicted;
            bool playerActionsHasSkipTurnForEnemy = skipTurnIndexPlayer != -1 && !battlePlayerCurrentActions[skipTurnIndexPlayer].isSelfInflicted;

            return playerActionsHasSkipTurnForEnemy || enemyActionsHasSkipTurnForEnemy;
        }

        /// <summary>
        /// Turn countdown Coroutine. Shown in the match to show max time before 
        /// having a strike or ending the match by disconnection.
        /// </summary>
        /// <returns></returns>
        IEnumerator TurnCountDown()
        {

            SacredTailsLog.LogMessageForBot($">>>>> InitTurnCountDown <<<<<<");
            camManager.InitCameras();
            float localTime = Time.time;
            while (maxTurnTimeInSeconds - (Time.time - localTime) > 0)
            {
                battleUIController.UpdateTimer(maxTurnTimeInSeconds - (Time.time - localTime));
                yield return null;
            }

            if (!alreadySendTurnOrTimeout)
            {
                turnStallCount += 1;
                if (turnStallCount == 3)
                {
                    SendMyTurn(8);
                }
                else
                {
                    if (!matchEnded)
                    {
                        ShowPopupStrike(turnStallCount);
                        SendMyTurn(7);
                    }
                }

                alreadySendTurnOrTimeout = true;

            }

            //Check if enemy hasn't send skip turn on timeout, because of a disconnection
            if (!matchEnded)
                StartCoroutine(WaitForOpponnentToSendSkipTurn());
        }

        private void ShowPopupStrike(int strikeCount)
        {
            Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
            buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
            {
                ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
            });

            ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"You have {strikeCount}/3 strikes. Withdrawing in {3 - strikeCount} more {(strikeCount == 1 ? "strike" : "strikes")} ", buttonsAction);
        }

        private IEnumerator WaitForOpponnentToSendSkipTurn()
        {
            float localTime = Time.time;
            while (alreadySendTurnOrTimeout)
            {
                float currentTimer = maxSecondsForEnemeyResponse - (Time.time - localTime);
                if (currentTimer > 0)
                {
                    battleUIController.UpdateTimer(currentTimer, "red", hasRecieveServerResponse);
                    yield return null;
                }
                else
                {
                    SacredTailsLog.LogErrorMessageForBot("Disconnected from match, isLocalPlayer:" + !hasRecieveServerResponse);
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup(!hasRecieveServerResponse ? "You have lost the match by disconnection. Please check your internet connection" : "Enemy has disconnected, you won!");

                    PlayfabManager.Singleton.BattleServerGetMatchState(battleGameMode.localCombat.MatchData.MatchId, isViewingMatch, enemyDisconnected: true);
                    battleGameMode.ShowEndResultPanel(hasRecieveServerResponse);
                    alreadySendTurnOrTimeout = false;
                }
            }
        }
        #endregion Turn flow

        #region Local Combat 

        /// <summary>
        /// Method that will be called by the cards buttons.
        /// This method will start sending a turn to the server with the SendMyTurn method.
        /// </summary>
        /// <param name="indexCard"></param>
        public void BtnSendTurn(int indexCard)
        {
            SendMyTurn(indexCard);
        }

        /// <summary>
        /// Prepare my turn with the card selected, execute it in local(cameras,ui) and send it to the other player.
        /// Also check if the card has been forbideen by other card effect, if so then shows a popup explaining that.
        /// </summary>
        /// <param name="indexCard">Index of card</param>
        public void SendMyTurn(int indexCard)
        {
            int trueIndexCard = GetTrueIndexCard(indexCard);
            ActionCard actionCard;
            if (trueIndexCard > 1000)
                actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(trueIndexCard - 1000);
            else
                actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(trueIndexCard);
            ownBattleTracker.NotifyAttack(trueIndexCard);
            if (ForbiddenActionCheck(actionCard))
            {
                if (!CanSendTurn || matchEnded)
                    return;
                CanSendTurn = false;
                alreadySendTurnOrTimeout = true;
                battleUIController.ShowCards(false);

                if (!isViewingMatch)
                    battleUIController.ToggleWaitingPrompt(true, isSkipTurn: true);
                SendTurnRequest(trueIndexCard, Callback: () =>
                {
                    if (!isViewingMatch)
                        battleUIController.ToggleWaitingPrompt(true);
                    battleUIController.ShowCards(false);
                    camManager.StopWaitTurnCameras();
                }, BadCallback: () =>
                {
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Couldn't send turn please try again");
                    CanSendTurn = true;
                    alreadySendTurnOrTimeout = false;
                    battleUIController.ShowCards(true);
                });
            }
            else
            {
                Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
                buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
                    ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup()
                );

                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Your card is currently blocked. Try another one.", buttonsAction);
            }
        }

        /// <summary>
        /// For testing only, this method will send an automatic turn to a bot player in the server.
        /// </summary>
        public void TestSendTurn()
        {
            string matchId = "TestMatch_" + PlayerDataManager.Singleton.localPlayerData.playfabId;
            //StartCoroutine(MakeEndpointCall(, null));
            SendBotTurn(battleGameMode.enemyInfo, battleGameMode.playerInfo, (data) =>
            {
                StartCoroutine(MakeEndpointCall("https://sacredtailsserver.azurewebsites.net/api/BattleServer/SendTurn",
                    data,
                    null));
            });
        }

        public IEnumerator MakeEndpointCall(string _url, string data, Action callback)
        {
            UnityWebRequest request;
            request = UnityWebRequest.Post(_url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            callback?.Invoke();
        }

        [SerializeField] OwnBattleTracker ownBattleTracker;
        public bool alreadyCheckWinnerOnSend = false;
        /// <summary>
        /// Main function to send turn. Takes the index card and send a petition to the server to register this turn in the database.
        /// the server process it and then with the server response we continue to the WaitOponentTurn() method in order to wait for the other
        /// player to also send a turn
        /// </summary>
        /// <param name="cardId"> The id of card that you need to send i.e(0,1,2,3...)</param>
        /// <param name="timesTryingToSendTurn">Times trying to send a turn, more than 5 is consider a player disconnect</param>
        public void SendTurnRequest(int cardId, int timesTryingToSendTurn = 0, Action Callback = null, Action BadCallback = null)
        {
            if (isViewingMatch)
                return;
            hasRecieveServerResponse = false;
            Coroutine disconnectCoroutine = StartCoroutine(WaitForSecondsAndDoActionEachSecond(30, 3,
            eachXSecondsCallback: () =>
            {
                battleUIController.AddPing(123);
            },
            callback: () =>
            {
                if (!hasRecieveServerResponse)
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Your connection is too slow, you will be disconnected in 1 minute if it doesn't improve.");
            }));
            var startTime = Time.realtimeSinceStartup;
            PlayfabManager.Singleton.BattleServerSendTurn(battleGameMode.localCombat.MatchData.MatchId, cardId, currentShinsei: battleGameMode.playerInfo.currentShinseiIndex, resultCallback: (result) =>
            {
                battleUIController.UpdatePing(Mathf.FloorToInt((Time.realtimeSinceStartup - startTime) * 100));
                hasRecieveServerResponse = true;
                StopCoroutine(disconnectCoroutine);
                SacredTailsPSDto<object> dtoCheck = JsonConvert.DeserializeObject<SacredTailsPSDto<object>>(result.FunctionResult.ToString());
                if (dtoCheck.code == 9)
                {
                    SacredTailsLog.LogMessageForBot("Error sending turn");
                    //Player data
                    dynamic serverData = dtoCheck.data;
                    ReplaceUserLocalDataWithServerData(true, serverData, ref battleGameMode.playerInfo);
                    //Any was wrong in client data
                    BadCallback?.Invoke();
                    return;
                }
                //Match Already ended
                if (dtoCheck.code == 51)
                {
                    if (alreadyCheckWinnerOnSend)
                    {
                        SacredTailsLog.LogMessageForBot("Match Ended in SendTurnRequest");
                        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Match ended.");
                        localPlayerWinMatch = localPlayer.PlayfabId == dtoCheck.data.ToString();
                        isWiner = localPlayerWinMatch.Value;
                        matchEnded = true;
                        ownBattleTracker.TotalTurns(turnActionsDatabase.Count);
                        battleGameMode.ShowEndResultPanel(localPlayerWinMatch.Value);
                        Callback?.Invoke();
                    }
                    else
                    {
                        alreadyCheckWinnerOnSend = true;
                        if (testMatchSendTurnAutomatically)
                            TestSendTurn();
                        StartCoroutine(WaitOponentTurn());
                    }
                    return;
                }

                if (result.FunctionResult.GetType() == typeof(bool))
                {
                    Dictionary<PopupManager.ButtonType, Action> buttons = new Dictionary<PopupManager.ButtonType, Action>();
                    buttons.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () => battleGameMode.GoBackToLobby());
                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("An error has ocurred, please check your internet connection.", buttons);
                }

                SacredTailsPSDto<ActionCardDto> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<ActionCardDto>>(result.FunctionResult.ToString());
                if (dto.success || timesTryingToSendTurn > 5)
                {
                    if (testMatchSendTurnAutomatically)
                        TestSendTurn();
                    StartCoroutine(WaitOponentTurn());
                }
                else
                {
                    SacredTailsLog.LogErrorMessage("Failed to send turn: code:" + dto.code + $"Message {dto.message}");
                    timesTryingToSendTurn++;
                    SendTurnRequest(cardId, timesTryingToSendTurn);
                }
                Callback?.Invoke();
            });
        }


        #region ---Forbidden Actions---
        public bool ForbiddenActionCheck(ActionCard actionCard)
        {
            foreach (var battleAction in actionCard.BattleActions)
                if (!localPlayer.forbidenActions.ContainsKey((int)battleAction.actionType))
                    return true;

            return false;
        }

        public void SetForbiddenActions(ActionCard actionCard)
        {
            foreach (var battleAction in actionCard.BattleActions)
                if (battleAction.actionType == ActionTypeEnum.BlockAction)
                {
                    if (!localPlayer.forbidenActions.ContainsKey(battleAction.amount))
                        localPlayer.forbidenActions.Add(battleAction.amount, battleAction.turnsDuration);
                    else
                        localPlayer.forbidenActions[battleAction.amount] = battleAction.turnsDuration;
                }

        }

        public void ReduceForbiddenActionsDuration()
        {
            var newforbidenActionsDuration = new Dictionary<int, int>();
            foreach (var forbiddenAction in localPlayer.forbidenActions)
                if (localPlayer.forbidenActions[forbiddenAction.Key] >= 0)
                    newforbidenActionsDuration.Add(forbiddenAction.Key, forbiddenAction.Value - 1);

            localPlayer.forbidenActions = newforbidenActionsDuration;
        }
        #endregion

        /// <summary>
        /// Execute the action of the target player on this turn, uses the battleaction dictionary to execute the pending actions
        /// from the player.  Finally it checks if the shinseis are all dead in order to end the match.
        /// </summary>
        /// <param name="currentActionsData">Actions executed this turn</param>
        /// <param name="playerIndex">Index of user</param>
        /// <param name="ppCost">ppCost of card</param>
        public void CalculateIncomingActions(ref List<BattleActionData> currentActionsData, int playerIndex, int ppCost)
        {
            UserInfo ownerPlayerAction;
            UserInfo otherPlayer;
            bool isLocalPlayer = playerIndex == battleGameMode.playerInfo.userIndex;

            if (isLocalPlayer)
            {
                ownerPlayerAction = battleGameMode.playerInfo;
                otherPlayer = battleGameMode.enemyInfo;

            }
            else
            {
                ownerPlayerAction = battleGameMode.enemyInfo;
                otherPlayer = battleGameMode.playerInfo;
            }

            // Apply PPcosts
            int ppCostValue = ownerPlayerAction.energybars[ownerPlayerAction.currentShinseiIndex].currentValue - ppCost > 0 ? ppCost : 0;
            ownerPlayerAction.energybars[ownerPlayerAction.currentShinseiIndex].currentValue -= ppCostValue;


            battleUIController.ApplyEnergyChange(isLocalPlayer ? 0 : 1, ownerPlayerAction.energybars[ownerPlayerAction.currentShinseiIndex].currentValue);

            //Execute pending battleActions
            bool hasLaunchedVfx = false;

            float currentVfxTime = 0;
            AttacksAnimation currentTargetAnim = AttacksAnimation.NONE;
            List<BattleActionData> currentActionsDataTemp = new List<BattleActionData>();
            var currentActions = currentActionsData.Reverse<BattleActionData>().OrderByDescending((action) => action.actionType == ActionTypeEnum.Damage || (action.actionType == ActionTypeEnum.SkipTurn && action.cardSkipTurn));
            SacredTailsLog.LogMessageForBot($"Calculating incoming actions: {JsonConvert.SerializeObject(currentActions, Formatting.Indented)}");

            if(isLocalPlayer)
                playerBuffViewer.UpdateTurns();
            else
                opponentBuffViewer.UpdateTurns();

            foreach (BattleActionData data in currentActions)
            {

                if (data.turnsDuration <= 0)
                {
                    if (turnActionsDatabaseDictionary.ContainsKey(data.actionType))
                        turnActionsDatabaseDictionary[data.actionType].EndAction(isLocalPlayer, ownerPlayerAction, otherPlayer, data);
                    continue;
                }

                //Check vfx launch
                if (!data.applyEachTurn && data.turnsPassed != 0)
                    data.launchVfx = false;
                else
                    data.launchVfx = !hasLaunchedVfx;

                if (data.launchVfx)
                {
                    data.currentVFXPositions = vfxPositionsDictionary;
                    VfxInfo vfxInfo = vFXInstancer.GetVfx(data.vfxIndex);
                    data.vfxTime = vFXInstancer.GetVfxTime(vfxInfo);
                    currentVfxTime = data.vfxTime;
                    currentTargetAnim = data.targetAnim;

                    data.isVfxReversed = vFXInstancer.GetVfxIsReversed(vfxInfo);
                    hasLaunchedVfx = true;
                }
                else
                {
                    data.vfxTime = currentVfxTime;
                    data.targetAnim = currentTargetAnim;
                }

                if (turnActionsDatabaseDictionary.ContainsKey(data.actionType))
                {

                    if (isLocalPlayer)
                    {
                        
                        if(data.actionType == ActionTypeEnum.ChangeShinsei)
                        {
                            BattleActionBuffDebuff.isChangingPlayerShinsei = true;
                        }
                    }
                    else
                    {
                       
                        if (data.actionType == ActionTypeEnum.ChangeShinsei)
                        {
                            BattleActionBuffDebuff.isChangingOpponentShinsei = true;
                        }
                    }

                    turnActionsDatabaseDictionary[data.actionType].ExecuteAction(isLocalPlayer, ownerPlayerAction, otherPlayer, data);
                    if (data.launchVfx)
                        actionTime = turnActionsDatabaseDictionary[data.actionType].ActionTime();
                        Debug.Log("actionTime 1: " + actionTime + ", Set: " + turnActionsDatabaseDictionary[data.actionType].ActionTime());
                }
                else if (data.launchVfx)
                {
                    battleActionsBaseTemplate.ExecuteAction(isLocalPlayer, ownerPlayerAction, otherPlayer, data);
                    actionTime = battleActionsBaseTemplate.ActionTime();
                    Debug.Log("actionTime 2: " + actionTime + ", Set: " + battleActionsBaseTemplate.ActionTime());
                }

                if (data.actionType == ActionTypeEnum.TerrainChange || data.actionType == ActionTypeEnum.PutAlteredState)
                    data.turnsDuration = 0;
                else
                    data.turnsDuration--;

                data.turnsPassed++;
                currentActionsDataTemp.Add(data);
            }
            isOpponentChangingShinsei = !isLocalPlayer ? false : isOpponentChangingShinsei;
            isPlayerChangingShinsei = isLocalPlayer ? false : isPlayerChangingShinsei;
            if (currentActions.Where(x => x.actionType == ActionTypeEnum.ChangeShinsei).Any())
            {
                if (isLocalPlayer)
                {
                    isPlayerChangingShinsei = true;
                }
                else
                {
                    isOpponentChangingShinsei = true;
                }
            }
            currentActionsData = currentActionsDataTemp;

            playerShinsei = isLocalPlayer ? ownerPlayerAction.battleShinseis[ownerPlayerAction.currentShinseiIndex] : otherPlayer.battleShinseis[otherPlayer.currentShinseiIndex];
            opponentShinsei = isLocalPlayer ? otherPlayer.battleShinseis[otherPlayer.currentShinseiIndex] : ownerPlayerAction.battleShinseis[ownerPlayerAction.currentShinseiIndex];
        }

        /// <summary>
        /// Check if the match has ended due to all the shinseis beign death
        /// </summary>
        /// <param name="isLocalPlayer"> Is the player asking, the current player</param>
        /// <param name="otherPlayer"> Player to check if shinseis are death</param>
        public void CheckEndMatch(bool isLocalPlayer, UserInfo otherPlayer)
        {
            if (battleGameMode.EndMatchCheck(otherPlayer.healthbars) && battleUIController.EndMatchCheck(!isLocalPlayer) && localPlayerWinMatch != null)
            {
                battleGameMode.ShowEndResultPanel(isLocalPlayer, () =>
                {
                    isWiner = isLocalPlayer;
                    matchEnded = true;
                });
            }
        }

        /// <summary>
        /// Check the dead time in order to make the InitTurnsFlow() wait that time.
        /// </summary>
        /// <param name="healthOwner"> health of current shinsei of the owner checking the actions</param>
        /// <param name="healthOther"> helth of other player current shinsei </param>
        /// <returns></returns>
        public int CheckDeadTime(int healthOther)
        {
            int deathTime = 0;
            if (healthOther <= 0)
                deathTime += 10;
            return deathTime;
        }

        /// <summary>
        /// Check if the shinsei was defeated
        /// </summary>
        /// <param name="health"> Health of shinsei </param>
        /// <param name="targetPlayer"> Shinsei player </param>
        private IEnumerator ShinseiDefeatCheck(int health, int targetPlayer, bool isSecond = false, bool waitForActionTime = true, Action<bool> callback = null)
        {
            if (health <= 0)
            {
                bool isLocalPlayer = targetPlayer == battleGameMode.playerInfo.userIndex;
                UserInfo targetInfo = isLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;

                int indexPlayerShinseiAlive = targetInfo.healthbars.FindIndex(x => x.currentValue > 0);
                if (indexPlayerShinseiAlive != -1)
                {
                    if (isLocalPlayer)
                        battleGameMode.playerInfo.currentShinseiIndex = indexPlayerShinseiAlive;
                    else
                        battleGameMode.enemyInfo.currentShinseiIndex = indexPlayerShinseiAlive;

                    targetInfo = isLocalPlayer ? battleGameMode.playerInfo : battleGameMode.enemyInfo;
                    string auxTargetText = isLocalPlayer ? "<color=#2FCC7B>[Player]</color>" : "<color=#F54F4F>[Enemy]</color>";
                    battleGameMode.AddTextToLog($"{auxTargetText} shinsei can't fight anymore");
                    //battleGameMode.AddTextToLog($"{(isLocalPlayer ? "Your" : "Enemy")} shinsei can't fight anymore");
                    Debug.Log($"{isLocalPlayer} shinsei can't fight anymore" + "19");
                    turnActionsDatabaseDictionary[ActionTypeEnum.ChangeShinsei].ExecuteAction(isLocalPlayer, targetInfo, targetInfo, new BattleActionData()
                    {
                        amount = indexPlayerShinseiAlive,
                        isSelfInflicted = true,
                        vfxAffectBoth = true
                    });
                    yield return new WaitForSeconds(9); //9 CAMTIME CONTINUE
                    callback?.Invoke(true);
                }
                else
                {
                    deathTime = 0;
                    callback?.Invoke(true);
                }
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        public int deathTime = 0;
        IEnumerator WaitForNextFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }

        #endregion Turn check

        private bool? localPlayerWinMatch = null;
        #region Enemy Combat
        public bool calculateEndMatchAgain = false;

        private void EndMatch()
        {
            SacredTailsLog.LogMessageForBot($"Ending match....");
            isWiner = localPlayerWinMatch.Value;
            matchEnded = true;
            ownBattleTracker.TotalTurns(turnActionsDatabase.Count);
            battleGameMode.ShowEndResultPanel(localPlayerWinMatch.Value);

        }
        /// <summary>
        /// Bring data and actions from the server and execute them. Checks altered states, terrains and players actions and 
        /// delegate all of those to be executed in order.
        /// It also checks if the match has already ended on the server in orded to force the ending in the
        /// client too. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitOponentTurn()
        {
            hasRecieveServerResponse = false;
            int counterOfPetitions = 0;
            while ((!CanSendTurn && !matchEnded))
            {
                counterOfPetitions++;
                if (!hasRecieveServerResponse && counterOfPetitions > 1)
                {
                    battleUIController.AddPing(123);
                    if (counterOfPetitions == 10)
                        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Your connection is too slow, you will be disconnected in 1 minute if it doesn't improve.");
                }
                var startTime = Time.realtimeSinceStartup;
                PlayfabManager.Singleton.BattleServerGetMatchState(battleGameMode.localCombat.MatchData.MatchId, isViewingMatch, (result) =>
                {
                    if (CanSendTurn || matchEnded)
                    {
                        SacredTailsLog.LogMessageForBot(">>> ALREADY RECIEVE MATCH STATE<<<<");
                        return;
                    }
                    if (result.FunctionResult == null)
                    {
                        GUIUtility.systemCopyBuffer = battleGameMode.localCombat.MatchData.MatchId;
                        SacredTailsLog.LogMessageForBot("Error from getMatchState, null function result");
                        CanSendTurn = true;
                        OnTurnChangeActions?.Invoke();
                        StopCoroutine(turnRoutine);
                        turnRoutine = StartCoroutine(TurnCountDown());
                        return;
                    }
                    battleUIController.UpdatePing(Mathf.FloorToInt((Time.realtimeSinceStartup - startTime) * 100));
                    hasRecieveServerResponse = true;
                    SacredTailsPSDto<MatchState> dto = JsonConvert.DeserializeObject<SacredTailsPSDto<MatchState>>(result.FunctionResult.ToString());
                    if (dto.code == 49 || dto.data == null)
                        return;
                    var playerTurns = dto.data.playersTurn;

                    if (localPlayerWinMatch != null && !canSendTurn)
                    {
                        EndMatch();
                        return;
                    }
                    //If End Match
                    bool hadErrorOnEndMatch = dto.code > 400 && dto.code < 499;
                    if (dto.code == 44 || dto.code == 51 || dto.code == 99 || dto.code == 83 || hadErrorOnEndMatch)
                    {
                        SacredTailsLog.LogMessageForBot($">>>>> MATCH ENDED <<<<<<");
                        if (hadErrorOnEndMatch && !calculateEndMatchAgain)
                        {
                            calculateEndMatchAgain = true;
                            return;
                        }
                        if (dto.code == 44 || dto.code == 51 || hadErrorOnEndMatch)
                        {
                            localPlayerWinMatch = localPlayer.PlayfabId == dto.data.winnerId;
                            if (playerTurns == null)
                            {
                                //TODO: TEST
                                ShowForcedEndMatch(localPlayerWinMatch.Value, localPlayerWinMatch.Value ? "Enemy Disconnected" : "Network error, disconnecting");
                                matchEnded = true;
                                return;
                            }
                            else if (!dto.data.playerWritedLastTurn)
                            {
                                dto.data.playersTurn.Reverse();
                                dto.data.isOwnerLocal = !dto.data.isOwnerLocal;
                            }
                        }
                        else
                        {
                            localPlayerWinMatch = localPlayer.PlayfabId == dto.data.winnerId;
                            EndMatch();
                            if (dto.code == 99)
                                ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Match has reached it's time limit. Showing results now.");
                            return;
                        }
                    }
                    if (dto.data.currentTurn == battleGameMode.localCombat.CurrentTurn)
                        return;

                    battleGameMode.localCombat.CurrentTurn = dto.data.currentTurn;

                    CanSendTurn = true;
                    camManager.StopWaitTurnCameras();

                    if (!String.IsNullOrEmpty(dto.data.winnerId) && isViewingMatch)
                    {
                        isWiner = dto.data.winnerId == battleGameMode.localCombat.MatchData.MatchPlayers[battleGameMode.playerInfo.userIndex].PlayfabId;
                        matchEnded = true;
                        battleGameMode.ShowEndResultPanel(isWiner);
                        return;
                    }

                    if (isViewingMatch)
                        dto.data.isOwnerLocal = dto.data.winnerId == battleGameMode.localCombat.MatchData.MatchPlayers[battleGameMode.enemyInfo.userIndex].PlayfabId;
                    if (CheckIfAnyoneSurrendered(playerTurns, dto.data.isOwnerLocal))
                        return;

                    alreadySendTurnOrTimeout = false;
                    if (turnRoutine != null)
                    {
                        StopCoroutine(turnRoutine);
                        battleUIController.UpdateTimer(-1);
                    }

                    battleGameMode.AddTextToLog(">>>Turn " + battleGameMode.localCombat.CurrentTurn + "<<<<");
                    Debug.Log(">>>Turn " + battleGameMode.localCombat.CurrentTurn + "<<<<" + "20");

                    //Terrain behaviour
                    if (battleGameMode.currentTerrain != null)
                        battleGameMode.currentTerrain.ExecuteTerrainBehaviour();
                    battleGameMode.currentTerrain?.ExecuteTerrainBehaviour();

                    //Download server data ...
                    if (dto.data?.playersServerData != null)
                    {

                        SacredTailsLog.LogMessageForBot($">>>>> replacing server data <<<<<<");
                        //Player data
                        dynamic playerServerData = JsonConvert.DeserializeObject(dto.data.playersServerData["PlayerMatchData_" + localPlayer.PlayfabId]);
                        ReplaceUserLocalDataWithServerData(true, playerServerData, ref battleGameMode.playerInfo);

                        //Enemy data
                        int localIndex = dto.data.playersServerData.Keys.ToList().IndexOf("PlayerMatchData_" + localPlayer.PlayfabId);
                        dynamic enemyServerData = JsonConvert.DeserializeObject(dto.data.playersServerData.Values.ToList()[localIndex == 0 ? 1 : 0]);
                        ReplaceUserLocalDataWithServerData(false, enemyServerData, ref battleGameMode.enemyInfo);
                    }

                    //Player's turns
                    combatInit = true;
                    battleUIController.battleNotificationSystem.AddText("<color=#28BDFA>" + "READY!" + "</color>", auxDuration: 1); //CAMTIME Add
                    //camManager.SwitchToCam(CamerasAvailableEnum.FAR_MIDDLE_CAMERA);
                    camManager.SwitchToCam(CamerasAvailableEnum.GENERAL_CAMERA); //CAMERA ATTACKS
                    ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
                    StartCoroutine(WaitForSeconds(1, () => //CAMTIME Init Turn step_1. tenia 2 seg
                    {
                        SacredTailsLog.LogMessageForBot($">>>>> Init turn flow <<<<<<");
                        InitTurnFlow(dto.data.isOwnerLocal, ExecuteServerAction(playerTurns, false), ExecuteServerAction(playerTurns, true), () =>
                        {
                            CheckEnergyBarsAndInitNextTurn();
                            if (dto.data?.playersServerData != null)
                            {
                                dynamic serverData = JsonConvert.DeserializeObject(dto.data.playersServerData["PlayerMatchData_" + localPlayer.PlayfabId]);
                                ChangeShinseiFromServer(true, serverData, ref battleGameMode.playerInfo);

                                int localIndex = dto.data.playersServerData.Keys.ToList().IndexOf("PlayerMatchData_" + localPlayer.PlayfabId);
                                dynamic enemyServerData = JsonConvert.DeserializeObject(dto.data.playersServerData.Values.ToList()[localIndex == 0 ? 1 : 0]);
                                ChangeShinseiFromServer(false, enemyServerData, ref battleGameMode.enemyInfo);
                            }
                        });
                    }));
                }, calculateEndMatchAgain);
                yield return new WaitForSeconds(battleGameMode.delayTime);
            }
        }


        public void ReplaceUserLocalDataWithServerData(bool isLocalPlayer, dynamic userServerData, ref UserInfo userInfo)
        {
            var serverShinseis = userServerData.ShinseiParty.ToObject<List<Shinsei>>();

            float auxDamageDif = 0;

            for (int i = 0; i < userInfo.battleShinseis.Count; i++)
            {
                //targetInfo.healthbars[targetInfo.currentShinseiIndex].currentValue = targetInfo.battleShinseis[targetInfo.currentShinseiIndex].healthAfterAlteredState;

                auxDamageDif = Mathf.Abs(userInfo.battleShinseis[i].healthAfterAlteredState - serverShinseis[i].healthAfterAlteredState);

                alteredStateController.InitNewAlteredStates(isLocalPlayer, serverShinseis[i].alteredStates, userInfo.battleShinseis[i].alteredStates, true, "DAÑO: " + auxDamageDif);

                userInfo.battleShinseis[i].ShinseiOriginalStats = serverShinseis[i].ShinseiOriginalStats;
                userInfo.battleShinseis[i].shinseiHealth = serverShinseis[i].shinseiHealth;
                userInfo.battleShinseis[i].didAlteredStateKillShinsei = serverShinseis[i].didAlteredStateKillShinsei;
                userInfo.battleShinseis[i].healthAfterAlteredState = serverShinseis[i].healthAfterAlteredState;
                userInfo.battleShinseis[i].shinseiEnergy = serverShinseis[i].shinseiEnergy;
                userInfo.battleShinseis[i].alteredStates = serverShinseis[i].alteredStates;
                userInfo.battleShinseis[i].reflectDamage = serverShinseis[i].reflectDamage;

                userInfo.battleShinseis[i].healingAmount = serverShinseis[i].healingAmount;
                userInfo.battleShinseis[i].realDirectDamage = serverShinseis[i].realDirectDamage;
                userInfo.battleShinseis[i].typeMultiplier = serverShinseis[i].typeMultiplier;
                userInfo.battleShinseis[i].didEvadeAttack = serverShinseis[i].didEvadeAttack;
                Debug.Log("Type multi " + serverShinseis[i].typeMultiplier);
            }
        }

        public void ChangeShinseiFromServer(bool isLocalPlayer, dynamic userServerData, ref UserInfo userInfo)
        {
            var serverShinseis = userServerData.ShinseiParty.ToObject<List<Shinsei>>();

            if (userServerData != null)
            {
                // Update data from server
                for (int i = 0; i < userInfo.battleShinseis.Count; i++)
                {
                    userInfo.battleShinseis[i].ShinseiOriginalStats = serverShinseis[i].ShinseiOriginalStats;
                    userInfo.battleShinseis[i].shinseiHealth = serverShinseis[i].shinseiHealth;
                    userInfo.battleShinseis[i].evadeChance = serverShinseis[i].evadeChance;
                }
                ResourceBarValues targetHealthBar = userInfo.healthbars[userInfo.currentShinseiIndex];
                targetHealthBar.currentValue = userInfo.battleShinseis[userInfo.currentShinseiIndex].shinseiHealth;
                // Update bars from UI
                battleUIController.ChangeHealthbarView();
                Debug.Log("Apply Update Health 05");
            }

            var convertedUserData = userServerData.currentShinsei.ToObject<int>();
            if (userInfo.currentShinseiIndex != convertedUserData)
            {
                userInfo.currentShinseiIndex = convertedUserData;
                battleGameMode.UpdateCurrentShinsei(ref userInfo);

                //Update bars
                int barIndex = isLocalPlayer ? 0 : 1;
                ResourceBarValues healthBar = userInfo.healthbars[userInfo.currentShinseiIndex];
                ResourceBarValues energyBar = userInfo.energybars[userInfo.currentShinseiIndex];

                battleUIController.InitializeBars(healthBar.currentValue, barIndex, healthBar.maxValue, energyBar.currentValue, barIndex, energyBar.maxValue);
                battleUIController.ApplyEnergyChange(barIndex, energyBar.currentValue);

                //Show faster
                float ownSpeed = userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiOriginalStats.speed;
                float otherSpeed = userInfo.battleShinseis[userInfo.currentShinseiIndex].ShinseiOriginalStats.speed;
                if (ownSpeed != otherSpeed)
                    battleUIController.ShowFaster(ownSpeed > otherSpeed ? 0 : 1);
            }
        }

        private void ShowForcedEndMatch(bool localPlayerWinMatch, string message = null)
        {
            Action callback = () =>
            {
                matchEnded = true;
                isWiner = false;
                canSendTurn = false;
            };

            battleGameMode.ShowPopupEndMatch(callback, message, localPlayerWinMatch);
        }

        /// <summary>
        /// Check if the enemy has surrender, if it has then end match.
        /// </summary>
        /// <param name="playersTurnIndexCard">Players turns</param>
        /// <param name="isLocalFirst">Is the local player surrendering first</param>
        /// <returns></returns>
        public bool CheckIfAnyoneSurrendered(List<ActionCardDto> playersTurnIndexCard, bool isLocalFirst)
        {
            int indexOfSurrenderedPlayer = -1;
            int countOfSurrenders = 0;
            for (int i = 0; i < playersTurnIndexCard.Count; i++)
                if (playersTurnIndexCard[i].indexCard == 2)
                {
                    indexOfSurrenderedPlayer = i;
                    countOfSurrenders++;
                }

            if (indexOfSurrenderedPlayer == -1)
                return false;


            Action callback = () =>
            {
                matchEnded = true;
                isWiner = false;
                CanSendTurn = false;
            };

            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();

            if (countOfSurrenders >= playersTurnIndexCard.Count)
            {
                if (!isLocalFirst)
                {
                    battleGameMode.ShowPopupEndMatch(callback, "Enemy has surrendered first. You won by default!", true);
                    StartCoroutine(WaitForSeconds(4, () =>
                        battleGameMode.ShowEndResultPanel(true, callback)));
                }
                else
                {
                    battleGameMode.ShowEndResultPanel(false, callback);
                    StartCoroutine(WaitForSeconds(4, () =>
                        battleGameMode.ShowEndResultPanel(false, callback)));
                }
            }
            else
            {
                if (indexOfSurrenderedPlayer == 1)
                {
                    battleGameMode.ShowPopupEndMatch(callback, "Enemy has surrendered first. You won by default!", true);
                    StartCoroutine(WaitForSeconds(4, () =>
                        battleGameMode.ShowEndResultPanel(true, callback)));
                }
                else
                    battleGameMode.ShowEndResultPanel(false, callback);
            }

            return true;
        }

        /// <summary>
        /// Manage the turn execution flow executing the turns in the order the server executed them.
        /// Check if otherPlayer shinsei dies. If so, then doesn't execute his action.
        /// </summary>
        /// <param name="ownerOfActionIsLocal"></param>
        /// <param name="userAction"></param>
        /// <param name="enemyAction"></param> 
        /// <param name="callback"></param>
        public void InitTurnFlow(bool ownerOfActionIsLocal, Action userAction, Action enemyAction, Action callback)
        {
            //Check if enemy shinsei changed
            UserInfo ownerInfo, otherInfo;
            Action ownerAction, otherAction;
            if (ownerOfActionIsLocal)
            {
                ownerInfo = battleGameMode.playerInfo;
                otherInfo = battleGameMode.enemyInfo;
                ownerAction = userAction;
                otherAction = enemyAction;
            }
            else
            {
                ownerInfo = battleGameMode.enemyInfo;
                otherInfo = battleGameMode.playerInfo;
                ownerAction = enemyAction;
                otherAction = userAction;
            }

            bool didAlteredStateKillShinsei = ownerInfo.battleShinseis[ownerInfo.currentShinseiIndex].didAlteredStateKillShinsei;
            if(didAlteredStateKillShinsei)
                aliveShinseiPreviewer.SetShinseiAliveState(ownerOfActionIsLocal ? true : false, ownerInfo.currentShinseiIndex);
            
            int previousShinseiIndex = otherInfo.currentShinseiIndex;

            ownerAction?.Invoke();
            actionTime += 2;//Add actiontime of alteredstate muzzle //CAMTIME LONG TIME
            SacredTailsLog.LogMessageForBot($">>>>> Owner action with time {actionTime}<<<<<<");
            Debug.Log("actionTime 3: " + actionTime + ", Add: " + 2);
            //Debug.Log("TIMERS: actionTime - " + actionTime);

            StartCoroutine(WaitForSeconds(actionTime, () =>
                {
                    ////Show damage after an attack:
                    //otherInfo.healthbars[otherInfo.currentShinseiIndex].currentValue -= otherInfo.battleShinseis[otherInfo.currentShinseiIndex].realDirectDamage;
                    //battleUIController.ChangeHealthbarView();

                    if (!didAlteredStateKillShinsei) DoDeathVerification(!ownerOfActionIsLocal);
                    SacredTailsLog.LogMessageForBot($">>>>> Owner Death Time {deathTime}<<<<<<");
                    Debug.Log("TIMERS: deathTime - " + deathTime);
                    StartCoroutine(WaitForSeconds(deathTime, () =>
                    {
                        bool hasShisneiChanged = false;
                        bool hasSleepShinsei = ownerOfActionIsLocal ? CurrentActionsHasSkipTurnForEnemy() : CurrentActionsHasSkipTurnForLocalPlayer();
                        if (previousShinseiIndex != -1) //if is dead
                            hasShisneiChanged = previousShinseiIndex != otherInfo.currentShinseiIndex;
                        if (!hasShisneiChanged) //if is sleeping this turn
                            hasShisneiChanged = hasSleepShinsei;

                        if (!hasShisneiChanged)
                        {
                            actionTime = 4;
                            Debug.Log("actionTime 4: " + actionTime + ", Set: " + 4);
                            didAlteredStateKillShinsei = otherInfo.battleShinseis[otherInfo.currentShinseiIndex].didAlteredStateKillShinsei;
                            otherAction?.Invoke();
                            SacredTailsLog.LogMessageForBot($">>>>> Other action with time {actionTime}<<<<<<");

                            StartCoroutine(WaitForSeconds(actionTime, () =>
                            {
                                /////Show damage after an attack:
                                //ownerInfo.healthbars[ownerInfo.currentShinseiIndex].currentValue -= ownerInfo.battleShinseis[ownerInfo.currentShinseiIndex].realDirectDamage;
                                //battleUIController.ChangeHealthbarView();

                                if (!didAlteredStateKillShinsei) DoDeathVerification(ownerOfActionIsLocal);
                                SacredTailsLog.LogMessageForBot($">>>>> Other Death Time {deathTime}<<<<<<");
                                StartCoroutine(WaitForSeconds(deathTime, callback));
                            }));
                        }
                        else
                        {
                            SacredTailsLog.LogMessageForBot($">>>>> Shinsei changed <<<<<<");
                            int userIndex = ownerOfActionIsLocal ? ownerInfo.userIndex : otherInfo.userIndex;

                            if (battleGameMode.playerInfo.userIndex == userIndex)
                                battlePlayerCurrentActions = hasSleepShinsei ? battlePlayerCurrentActions.Where(action => action.actionType == ActionTypeEnum.SkipTurn).ToList() : new List<BattleActionData>();
                            else
                                battleEnemyCurrentActions = hasSleepShinsei ? battleEnemyCurrentActions.Where(action => action.actionType == ActionTypeEnum.SkipTurn).ToList() : new List<BattleActionData>();

                            callback?.Invoke();
                        }
                    }));
                }));
        }

        public void DoDeathVerification(bool targetIsLocal)
        {
            UserInfo otherPlayer;
            if (targetIsLocal)
                otherPlayer = battleGameMode.playerInfo;
            else
                otherPlayer = battleGameMode.enemyInfo;

            //Check shinsei Defeat
            deathTime = CheckDeadTime(otherPlayer.healthbars[otherPlayer.currentShinseiIndex].currentValue);
            StartCoroutine(ShinseiDefeatCheck(otherPlayer.healthbars[otherPlayer.currentShinseiIndex].currentValue, otherPlayer.userIndex, true, callback: (otherShinseiDead) =>
            {
                if (otherShinseiDead)
                {
                    SacredTailsLog.LogMessageForBot($"Shinsei died,  {otherShinseiDead}");
                    if(targetIsLocal)
                    {
                        aliveShinseiPreviewer.SetShinseiAliveState(true, otherPlayer.currentShinseiIndex);
                        playerAlteredView.RemoveAllAlteredStates();
                        playerBuffViewer.ClearAllBuffsViews();
                    }
                    else
                    {
                        aliveShinseiPreviewer.SetShinseiAliveState(false, otherPlayer.currentShinseiIndex);
                        opponentAlteredView.RemoveAllAlteredStates();
                        opponentBuffViewer.ClearAllBuffsViews();
                    }
                    CheckEndMatch(!targetIsLocal, otherPlayer);
                }

                if (targetIsLocal)
                    battleGameMode.playerInfo = otherPlayer;
                else
                    battleGameMode.enemyInfo = otherPlayer;
            }));
        }

        /// <summary>
        /// Wait for X seconds before executing some logic 
        /// </summary>
        /// <param name="time">Time before executing logic</param>
        /// <param name="callback">Logic to execute</param>
        /// <returns></returns>
        public IEnumerator WaitForSecondsAndDoActionEachSecond(int time, int xSeconds, Action eachXSecondsCallback, Action callback)
        {
            int counter = 0;
            for (int i = 0; i < time; i++)
            {
                yield return new WaitForSeconds(1);
                counter++;
                if (counter >= xSeconds)
                {
                    counter = 0;
                    eachXSecondsCallback?.Invoke();
                }
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Wait for X seconds before executing some logic
        /// </summary>
        /// <param name="time">Time before executing logic</param>
        /// <param name="callback">Logic to execute</param>
        /// <returns></returns>
        public IEnumerator WaitForSeconds(float time, Action callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }

        bool duplicatedEnergy = false;
        /// <summary>
        /// After executing actions update energyBars and init next turn.
        /// </summary>
        public void CheckEnergyBarsAndInitNextTurn()
        {
            PlayFab.ServerModels.GetSharedGroupDataRequest req = new PlayFab.ServerModels.GetSharedGroupDataRequest()
            {
                SharedGroupId = battleGameMode.localCombat.MatchData.MatchId,
                Keys = new List<string>() { "ActualTimeStamp", "firstTimeStamp" }
            };

            PlayFabServerAPI.GetSharedGroupData(req, (result) =>
            {
                camManager.SwitchToCam(CamerasAvailableEnum.FAR_MIDDLE_CAMERA);
                camManager.SwitchPointOfInterest(CameraPointOfInteresEnum.ARENA_CENTER);
                int playerShinseiIndex = battleGameMode.playerInfo.currentShinseiIndex;
                Shinsei playerShinsei = battleGameMode.playerInfo.battleShinseis[playerShinseiIndex];

                int enemyShinseiIndex = battleGameMode.enemyInfo.currentShinseiIndex;
                Shinsei enemyShinsei = battleGameMode.enemyInfo.battleShinseis[enemyShinseiIndex];

                // Regenerate energy of both players
                ResourceBarValues playerEnergyBar = battleGameMode.playerInfo.energybars[playerShinseiIndex];
                ResourceBarValues enemyEnergyBar = battleGameMode.enemyInfo.energybars[enemyShinseiIndex];

                int energyMultiplier = 1;

                if (result.Data.Keys.Contains("ActualTimeStamp") && result.Data.Keys.Contains("firstTimeStamp"))
                {
                    ulong actualTimeStamp = ulong.Parse(result.Data["ActualTimeStamp"].Value);
                    ulong firstTimeStamp = ulong.Parse(result.Data["firstTimeStamp"].Value);

                    int timeDifferenceInSeconds = (int)(actualTimeStamp - firstTimeStamp) / 1000;
                    if (timeDifferenceInSeconds >= 420)
                    {
                        energyMultiplier = 2;
                        if (!duplicatedEnergy)
                        {
                            duplicatedEnergy = true;
                            battleGameMode.AddTextToLog("<color=yellow>Energy Regen</color> is now doubled!");
                        }
                    }

                    CalculateEnergyRegen(playerShinsei, playerEnergyBar, battlePlayerCurrentActions, 0, energyMultiplier);
                    CalculateEnergyRegen(enemyShinsei, enemyEnergyBar, battleEnemyCurrentActions, 1, energyMultiplier);
                    Debug.Log("Stamp found and time difference is: " + timeDifferenceInSeconds);

                    ReduceForbiddenActionsDuration();
                    deathTime = 0;


                    // Start turn timer
                    if (localPlayerWinMatch != null)
                        EndMatch();
                    else
                    {
                        OnTurnChangeActions?.Invoke();
                        turnRoutine = StartCoroutine(TurnCountDown());
                    }
                }
            }, (error) =>
            {
                Debug.LogWarning(error.ErrorDetails);
            });
        }

        /// <summary>
        /// Calculates the amount of energy that a shinsei restores after the turn passes
        /// </summary>
        /// <param name="playerShinsei">Shinsei of the player</param>
        /// <param name="energyBar">Energy bar of the shinsei </param>
        /// <param name="playersActions">Player turn</param>
        /// <param name="playerIndex">Index of the player 0=local, 1=enemy</param>

        public void CalculateEnergyRegen(Shinsei playerShinsei, ResourceBarValues energyBar, List<BattleActionData> playersActions, int playerIndex, int energyMultiplier)
        {

                if (playersActions.Count >= 1 && playersActions.Where(x => x.actionType == ActionTypeEnum.SkipTurn).Any())
                    energyBar.currentValue += (int)((playerShinsei.ShinseiOriginalStats.Vigor / 5) + 20) * energyMultiplier;
                //passive regeneration
                else
                    energyBar.currentValue += (int)(playerShinsei.ShinseiOriginalStats.Vigor / 5) * energyMultiplier;

                if (energyBar.currentValue > playerShinsei.ShinseiOriginalStats.Energy)
                    energyBar.currentValue = playerShinsei.ShinseiOriginalStats.Energy;

            battleUIController.ApplyEnergyChange(playerIndex, energyBar.currentValue);

        }

        /// <summary>
        /// Execute enemy actions that we bringed earlier from the cloud.
        /// </summary>
        /// <param name="turns">Turns of the players</param>
        /// <param name="isEnemy">Is the enemy the one who want to execute his action?</param>
        /// <returns></returns>
        public Action ExecuteServerAction(List<ActionCardDto> turns, bool isEnemy = false)
        {

            ActionCardDto turn = isEnemy ? turns[1] : turns[0];
            //Send displaymessage from card
            string textP = isEnemy ? "<color=orange>Enemy</color>" : "<color=green>Your</color>";
            Dictionary<string, string> keyWords = new Dictionary<string, string> { { "[p]", textP } };

            ActionCard actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(turn.indexCard);
            int vfxIndex = actionCard.VfxIndex;

            bool hasAnyCopyAction = turn.isComingFromCopyIndex > 0;
            turn.BattleActions.ForEach(action =>
            {
                action.vfxAffectBoth = actionCard.vfxAffectBoth;
                action.casterAnim = actionCard.casterAnimation;
                action.targetAnim = actionCard.targetAnimation;
                action.isComingFromCopyIndex = turn.isComingFromCopyIndex;
                if (hasAnyCopyAction)
                {
                    actionCard = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(action.isComingFromCopyIndex + 3);
                    action.copiedIndex = turn.indexCard;
                }
                action.vfxIndex = vfxIndex;
            });


            //SetForbiddenActions(actionCard);
            if (turn.indexCard >= 0)
            {
                if (isEnemy)
                    battleEnemyCurrentActions.AddRange(turn.BattleActions);
                else
                    battlePlayerCurrentActions.AddRange(turn.BattleActions);
            }
            battleUIController.WaitingMessageSetActive(false); //CAMTIME is add
            //battleUIController.battleNotificationSystem.AddText("<color=#28BDFA>" + "READY!" + "</color>", auxDuration: 1); //CAMTIME
            Action turnAction = () =>
            {
                string textTurn = isEnemy ? "<color=#F54F4F>[Enemy]</color>" : "<color=#2FCC7B>[Player]</color>"; //"-----<color=orange>Enemy turn</color>-----" : "-----<color=green>Your turn</color>-----";
                //battleGameMode.AddTextToLog(textTurn + "20");
                Debug.Log(textTurn + "20");
                //Altered stateF
                bool didAlteredStateKillShinsei = alteredStateController.CheckAlteredStates(!isEnemy, isEnemy ? turns[0] : turns[1]);

                if (didAlteredStateKillShinsei)
                {
                    battleGameMode.AddTextToLog(textTurn + " Shinsei " + "<color=#F54F4F>killed</color>" + " by altered state");
                    Debug.Log("Shinsei killed by altered state" + "21");
                    actionTime = 2;
                    Debug.Log("actionTime 5: " + actionTime + ", Set: " + 2);
                    return;
                }

                switch(actionCard.name)
                {
                    case "ChangeShinsei":
                        battleUIController.battleNotificationSystem.AddText($"{textTurn} used " + "<color=#28BDFA>" + "Change Shinsei" + "</color>");
                        break;
                    case "SkipTurn":
                        battleUIController.battleNotificationSystem.AddText($"{textTurn} Shinsei has " + "<color=#28BDFA>" + "Skipped Turn" + "</color>");
                        break;
                    default:
                        battleUIController.battleNotificationSystem.AddText($"{textTurn} Shinsei use " + "<color=#28BDFA>" + actionCard.name + "</color>" + " Attack.");
                        break;
                }
                    
                //battleGameMode.AddTextToLog(actionCard.DisplayNotification + "22", keyWords);
                Debug.Log(actionCard.DisplayNotification + "22");
                //Show battle in game
                if (isEnemy)
                    CalculateIncomingActions(ref battleEnemyCurrentActions, battleGameMode.enemyInfo.userIndex, turn.PpCost);
                else
                    CalculateIncomingActions(ref battlePlayerCurrentActions, battleGameMode.playerInfo.userIndex, turn.PpCost);

            };

            return turnAction;
        }
        #endregion Enemy Combat

        #region Helpers

        /// <summary>
        /// Get the index of a card in the cardDatabase 
        /// by passing the index of the card in the shinsei
        /// </summary>
        /// <param name="indexCard">index of the card in the shinsei</param>
        /// <example>
        /// 0,1,2,3 is for the first 4 cards of the shinsei.
        /// 4,5,6 is for the shinsei change 
        /// 7 is for skipping turn
        /// 8 is for surrender 
        /// </example>
        /// <returns></returns>
        public int GetTrueIndexCard(int indexCard, UserInfo targetUserInfo = null)
        {
            if (targetUserInfo == null)
            {
                targetUserInfo = battleGameMode.playerInfo;
            }

            int trueIndexCard = indexCard;
            if (indexCard > 3)
            {
                if (indexCard == 7)
                    trueIndexCard = 0;/* Skip turn */
                else if (indexCard == 8)
                    trueIndexCard = 2;/* Surrender */
                else
                    trueIndexCard = 1000 + indexCard;/*Shinsei Change*/
            }
            else if (indexCard >= 0)
            {
                trueIndexCard = targetUserInfo.battleShinseis[targetUserInfo.currentShinseiIndex].ShinseiActionsIndex[indexCard];
            }

            return trueIndexCard;
        }
        #endregion Helpers
        #endregion ----Methods----
    }
}

[Serializable]
public class MatchState
{
    public List<ActionCardDto> playersTurn;
    public bool isOwnerLocal;
    public bool playerWritedLastTurn;
    public int currentTurn;
    public bool skipTurnIndex;
    public string winnerId;
    public Dictionary<string, string> playersServerData;
}

[Serializable]
public class ActionCardDto : ActionCard
{
    public int indexCard;
}

[System.Serializable]
public class VFXPositionData
{
    public VFXPositionEnum vfxPosEnum;
    public Transform positions;
}
public enum VFXPositionEnum
{
    ARENA_CENTER,
    SHINSEI_PLAYER,
    SHINSEI_ENEMY
}

[System.Serializable]
public class VFXTypeData
{
    public CharacterType type;
    public GameObject vfxPrefab;
}
