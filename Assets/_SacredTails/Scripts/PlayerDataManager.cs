using MyBox;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.CharacterFactory;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using Timba.SacredTails.CardStoreModule;
using Timba.SacredTails.CharacterStyle;
using Timba.SacredTails.Database;
using Timba.SacredTails.Photoboot;
using Timba.SacredTails.UiHelpers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerDataManager : MonoBehaviour
{
    #region ----Fields----
    public LocalPlayerData localPlayerData = new LocalPlayerData();
    public GameObject localPlayerGameObject;
    public bool isFrenchKeyboardLayout;
    public static PlayerDataManager Singleton;
    [SerializeField] bool isResetData;
    public CharacterStyleController characterStyleController;
    public List<CombatBotDataSO> CombatNPCs = new List<CombatBotDataSO>();
    public BotCombatModule botCombatModule;

    [SerializeField] bool usePlayerPrefs;

    [ConditionalField(nameof(usePlayerPrefs), true, true)]
    [SerializeField] bool useTestCards;

    [ConditionalField(nameof(useTestCards), true, true)]
    [SerializeField] public List<int> cardToTest = new List<int>();

    [SerializeField] int numberOfShinseisInVault = 3;
    [SerializeField] int numberOfTestingCard = 6;
    [SerializeField] Button changeShinseiButton;
    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject genderSelect;
    public Action playerDataReady;
    public event Action OnDataObtained;

    public Transform endGamePoint;
    public string currentTournamentId;
    public int currentTournamentStage;
    public bool isOnTheTournament = false;


    public bool isBot = false;
    public int numberOfBots;
    public bool isBotCreatorOfTournaments;
    public string botChallengeId;

    [SerializeField] UserNftsManager userNftsManager;

   // ShinseiStats shinseiStats = new ShinseiStats();

    #endregion ----Fields----

    #region ----Methods----
    #region Init Class
    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (Singleton != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void Start()
    {
        PlayfabManager.Singleton.OnLoginSucces.AddListener(InitPlayerData);
        PlayfabManager.Singleton.OnUpdateNameSuccess.AddListener(UpdatePlayerName);

        userNftsManager = this.GetComponent<UserNftsManager>();
    }
    #endregion Init Class

    #region Init PlayerData
    /// <summary>
    /// Create NewlyPlayerData
    /// </summary>
    /// <param name="loginResult"></param>
    public void InitPlayerData(LoginResult loginResult)
    {
        if (usePlayerPrefs)
        {
            useTestCards = PlayerPrefs.GetInt("useTest", 0) == 1;
            List<int> newListCards = new List<int>();
            for (int i = 0; i < 4; i++)
                newListCards.Add(PlayerPrefs.GetInt($"card{i}", cardToTest[i]));

            cardToTest = newListCards;
        }

        localPlayerData.playfabId = loginResult.PlayFabId;
        localPlayerData.entityId = loginResult.EntityToken.Entity.Id;
        localPlayerData.entityType = loginResult.EntityToken.Entity.Type;
        localPlayerData.playerName = loginResult.InfoResultPayload != null ? loginResult.InfoResultPayload.PlayerProfile.DisplayName : loginResult.PlayFabId;//"ouo";
        localPlayerData.characterState = Timba.Games.SacredTails.LobbyDatabase.CharacterStateEnum.LOBBY;


        if (loginResult.NewlyCreated)
        {
            PlayfabManager.Singleton.SetUserData(FillPlayerData(), UserDataPermission.Public);
        }
        else
        {
            List<string> keys = new List<string>();
            keys.Add(Constants.SHINSEI_COMPANION);
            keys.Add(Constants.CHARACTER_STYLE);
            keys.Add(Constants.SHINSEI_VAULT);
            keys.Add(Constants.CARD_STORE);
            for (int i = 1; i < 6; i++)
            {
                keys.Add(Constants.SHINSEI_SLOT + i);
            }
            PlayfabManager.Singleton.GetUserData(localPlayerData.playfabId, keys, GetShinseis);
        }
        playerDataReady?.Invoke();
    }

    public void GetShinseis(GetUserDataResult getUserDataResult)
    {

        if (getUserDataResult.Data.ContainsKey(Constants.SHINSEI_COMPANION) &&
            getUserDataResult.Data.ContainsKey(Constants.SHINSEI_SLOT + 1) &&
            getUserDataResult.Data.ContainsKey(Constants.SHINSEI_VAULT) &&
            getUserDataResult.Data.ContainsKey(Constants.CARD_STORE) &&
            getUserDataResult.Data.ContainsKey(Constants.CHARACTER_STYLE) &&
            !getUserDataResult.Data[Constants.CHARACTER_STYLE].Value.Contains("SKIN") &&
            !isResetData)
        {
            localPlayerData.ShinseiCompanion = JsonUtility.FromJson<Shinsei>(getUserDataResult.Data[Constants.SHINSEI_COMPANION].Value);
            localPlayerData.ShinseiVault = JsonUtility.FromJson<ShinseiVault>(getUserDataResult.Data[Constants.SHINSEI_VAULT].Value);
            localPlayerData.Deck = JsonUtility.FromJson<Deck>(getUserDataResult.Data[Constants.CARD_STORE].Value);
            //Load character style from playfab
            localPlayerData.currentCharacterStyle = localPlayerData.CastCompressedStyleToDictionary(getUserDataResult.Data[Constants.CHARACTER_STYLE].Value);
            for (int i = 1; i < 6; i++)
                localPlayerData.ShinseiParty.Add(JsonUtility.FromJson<Shinsei>(getUserDataResult.Data[Constants.SHINSEI_SLOT + i].Value));
            GenerateShinseiIcons();

        }
        else
            PlayfabManager.Singleton.SetUserData(FillPlayerData(), UserDataPermission.Public);
    }

 
    #endregion Init PlayerData

    #region Fill Player Data
    public Dictionary<string, string> FillPlayerData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        CardManagerFill(data);

        if (!PlayfabManager.Singleton.loginWithAddress)
        {
            PlayerShinseisFill(data);
        }
        else
        {
            NFT_PlayerShinseisFill(data);
        }

        CharacterStyleFill(data);

        GenerateShinseiIcons();
        return data;
    }

    public void ButtonChangeShinseis()
    {
        Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
        buttonsAction.Add(PopupManager.ButtonType.BACK_BUTTON, () =>
        {
            ServiceLocator.Instance.GetService<IPopupManager>().HideInfoPopup();
        });
        buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
        {
            var ShinseiDataToSend = FillShinseiData();
            ShinseiDataToSend.Add(Constants.CHANGE_SHINSEI, checkForShinseiUpdateValue);
            Debug.Log("Value to send : "+ checkForShinseiUpdateValue);
            PlayfabManager.Singleton.SetUserData(ShinseiDataToSend, () => { Application.Quit(); }, UserDataPermission.Public);
        });

        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"Are you sure you want to quit the game?, with this option your shinseis will be random changed.", buttonsAction);
    }


    string checkForShinseiUpdateValue;
    public void CheckForButtonChangeShinsei()
    {
        PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest(), (result) => {
            string time = result.Time.ToString("dd'/'MM'/'yyyy").Trim();
            PlayfabManager.Singleton.GetUserData(localPlayerData.playfabId, new List<string>() { Constants.CHANGE_SHINSEI }, (result) => {
                if(result.Data.ContainsKey(Constants.CHANGE_SHINSEI))
                {
                    string savedDate = (result.Data[Constants.CHANGE_SHINSEI].Value.Split('-')[0]).Trim();
                    int savedCount = int.Parse(result.Data[Constants.CHANGE_SHINSEI].Value.Split('-')[1]);

                    if (savedDate == time && savedCount >= 3)
                    {
                        changeShinseiButton.interactable = false;
                    }
                    else
                    {
                        if (savedDate != time)
                            savedCount = 0;
                        checkForShinseiUpdateValue = time + "-" + (savedCount + 1);
                        changeShinseiButton.interactable = true;
                    }
                    CheckForDailyChangeShinsei(savedDate);
                    Debug.Log("savedDate: " + savedDate + " time: " + time + " savedCount: " + savedCount);
                }
                else
                {
                    checkForShinseiUpdateValue = time + "-1";
                    changeShinseiButton.interactable = true;
                    CheckForDailyChangeShinsei(string.Empty);
                }
            });
        },
        (error) =>
        {
            Debug.LogWarning("Error getting time : "+ error.GenerateErrorReport());
        });
    }

    Coroutine checkDailyShinsei;
    public void CheckForDailyChangeShinsei(string savedDateOnLogIn)
    {
        if (checkDailyShinsei != null)
            StopCoroutine(checkDailyShinsei);

        //checkDailyShinsei = StartCoroutine(CheckForServerDate(savedDateOnLogIn)); //ENABLE FOR REGENERE SHINSEIS
    }

    IEnumerator CheckForServerDate(string savedDateOnLogIn)
    {
         //Each 10 minutes
        while(genderSelect.activeSelf)
        {
            yield return new WaitForSeconds(1);
        }

        if (lobbyUI.activeSelf && !isOnTheTournament)
        {
            PlayFabServerAPI.GetTime(new PlayFab.ServerModels.GetTimeRequest(), (result) =>
            {
                string time = result.Time.ToString("dd'/'MM'/'yyyy").Trim();
                Debug.Log("Debug time : " + time);
                if(time != savedDateOnLogIn)
                {
                    Dictionary<PopupManager.ButtonType, Action> buttonsAction = new Dictionary<PopupManager.ButtonType, Action>();
                    buttonsAction.Add(PopupManager.ButtonType.CONFIRM_BUTTON, () =>
                    {
                        var ShinseiDataToSend = FillShinseiData();
                        string updateActualDate = time + "-0";
                        ShinseiDataToSend.Add(Constants.CHANGE_SHINSEI, updateActualDate);
                        Debug.Log("Value to send : " + checkForShinseiUpdateValue);
                        PlayfabManager.Singleton.SetUserData(ShinseiDataToSend, () => { Application.Quit(); } ,UserDataPermission.Public);
                        
                    });

                    ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup($"A new day beckons! Hit \"Confirm\" to access fresh shinseis in Sacred Tails. Forge ahead, outwit rivals worldwide, and ascend with past Sacred Champions", buttonsAction);
                }
            },
            (error) =>
            {
                Debug.LogWarning("Error getting time for daily change : " + error.GenerateErrorReport());
            });
        }
        yield return new WaitForSecondsRealtime(300);
        checkDailyShinsei = StartCoroutine(CheckForServerDate(savedDateOnLogIn));
    }

    public Dictionary<string, string> FillShinseiData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        CardManagerFill(data);
        PlayerShinseisFill(data);
        GenerateShinseiIcons();
        return data;
    }

    public void UpdateCharacterStyleForAnyReason()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        if (data != null)
            data.Add(Constants.CHARACTER_STYLE, localPlayerData.CastDictionaryToCompressedStyle());
        PlayfabManager.Singleton.SetUserData(data, UserDataPermission.Public);
    }

    public void CharacterStyleFill(Dictionary<string, string> data = null)
    {
        characterStyleController.GenerateRandomOutfit();
        if (data != null)
            data.Add(Constants.CHARACTER_STYLE, localPlayerData.CastDictionaryToCompressedStyle());
    }

    public void CardManagerFill(Dictionary<string, string> data)
    {
        List<Card> shinseiCardList = new List<Card>();
        for (int i = 3; i < ServiceLocator.Instance.GetService<IDatabase>().CardDatabaseCount(); i++)
        {
            var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(i);
            shinseiCardList.Add(new Card() { index = i, count = UnityEngine.Random.Range(1, 5), cardName = card.name });
        }

        Deck newDeck = new Deck() { cards = shinseiCardList };
        data.Add(Constants.CARD_STORE, JsonUtility.ToJson(newDeck));
        localPlayerData.Deck = newDeck;
    }

    public void PlayerShinseisFill(Dictionary<string, string> data)
    {
        int shinseisToBeCreated = 6 + (numberOfShinseisInVault > 0 ? numberOfShinseisInVault : 0);
        List<Shinsei> shinseiVaultList = new List<Shinsei>();

        for (int i = 0; i < shinseisToBeCreated; i++)
        {
            string newShinseiDna = ServiceLocator.Instance.GetService<IDatabase>().GetRandomShinsei();
            Dictionary<string, string> partsType = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiPartsTypes(newShinseiDna, new CharacterType());

            //TODO change this from aleatory to specific attacks per shinsei from this to
            List<int> orderedList = new List<int>();
            List<int> shinseiCardIndexList = new List<int>();

            if (useTestCards)
            {
                orderedList.AddRange(Enumerable.Range(3, (ServiceLocator.Instance.GetService<IDatabase>().CardDatabaseCount() - 3))); ;
                for (int a = 0; a < cardToTest.Count; a++)
                    shinseiCardIndexList.Add(orderedList[cardToTest[a]]);
            }
            else
            {
                orderedList = ServiceLocator.Instance.GetService<IDatabase>().GetDatabaseCardsIndexListByType(partsType.Select(kvp => kvp.Value).ToList());

                //Add moves to a shinsei
                for (int a = 0; a < 4; a++)
                {
                    int randomIndex = Random.Range(0, orderedList.Count);
                    shinseiCardIndexList.Add(orderedList[randomIndex]);
                    orderedList.RemoveAt(randomIndex);
                }
            }
            //TODO to This
            Shinsei newShinsei = new Shinsei()
            {
                ShinseiDna = newShinseiDna,
                ShinseiActionsIndex = new List<int>(),
                ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(newShinseiDna),
                shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(newShinseiDna),
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(newShinseiDna)

            };
            Debug.Log("ADN " +newShinseiDna);
            newShinsei.ShinseiOriginalStats.Energy = 200; //Override energy to 200
            foreach (var shinseiCardIndex in shinseiCardIndexList)
                newShinsei.ShinseiActionsIndex.Add(shinseiCardIndex);

            if (i == 0)
            {
                data.Add(Constants.SHINSEI_COMPANION, JsonUtility.ToJson(newShinsei));
                localPlayerData.ShinseiCompanion = newShinsei;
            }
            else if (i < 6)
            {
                data.Add(Constants.SHINSEI_SLOT + i, JsonUtility.ToJson(newShinsei));
                localPlayerData.ShinseiParty.Add(newShinsei);
            }
            else
            {
                shinseiVaultList.Add(newShinsei);
            }
        }

        ShinseiVault shinseiVault = new ShinseiVault() { ShinseiVaultList = shinseiVaultList };
        data.Add(Constants.SHINSEI_VAULT, JsonUtility.ToJson(shinseiVault));
        localPlayerData.ShinseiVault = shinseiVault;
    }

    public void NFT_PlayerShinseisFill(Dictionary<string, string> data)
    {
        Debug.Log("Se fue poraka!!");
        int shinseisToBeCreated = 6 + (numberOfShinseisInVault > 0 ? numberOfShinseisInVault : 0);
        List<Shinsei> shinseiVaultList = new List<Shinsei>();

        //for (int i = 0; i < shinseisToBeCreated; i++)
        //for (int i = 0; i < userNftsManager.nftOwnership.result.Length)
        //for (int i = 0; i < 36; i++)
        for (int i = 0; i < userNftsManager.nftOwnership.result.Length; i++)
        {
            //string newShinseiDna = ServiceLocator.Instance.GetService<IDatabase>().GetRandomShinsei();
            string newShinseiDna = userNftsManager.nftOwnership.result[i].metadataInfo.dna;
            Dictionary<string, string> partsType = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiPartsTypes(newShinseiDna, new CharacterType());

            //TODO change this from aleatory to specific attacks per shinsei from this to
            List<int> orderedList = new List<int>();
            List<int> shinseiCardIndexList = new List<int>();

            if (useTestCards)
            {
                orderedList.AddRange(Enumerable.Range(3, (ServiceLocator.Instance.GetService<IDatabase>().CardDatabaseCount() - 3))); ;
                for (int a = 0; a < cardToTest.Count; a++)
                    shinseiCardIndexList.Add(orderedList[cardToTest[a]]);
            }
            else
            {
                orderedList = ServiceLocator.Instance.GetService<IDatabase>().GetDatabaseCardsIndexListByType(partsType.Select(kvp => kvp.Value).ToList());

                //Add moves to a shinsei
                for (int a = 0; a < 4; a++)
                {
                    int randomIndex = Random.Range(0, orderedList.Count);
                    shinseiCardIndexList.Add(orderedList[randomIndex]);
                    orderedList.RemoveAt(randomIndex);
                }
            }
            //TODO to This
            Shinsei newShinsei = new Shinsei()
            {
                shinseiName = userNftsManager.nftOwnership.result[i].metadataInfo.name,
                ShinseiDna = newShinseiDna,
                ShinseiActionsIndex = new List<int>(),
                ShinseiOriginalStats = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiStats(newShinseiDna),
                shinseiType = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiType(newShinseiDna),
                shinseiRarity = ServiceLocator.Instance.GetService<IDatabase>().ObtainShinseiRarity(newShinseiDna)

            };
            Debug.Log("ADN " + newShinseiDna);
            newShinsei.ShinseiOriginalStats.Energy = 200; //Override energy to 200
            foreach (var shinseiCardIndex in shinseiCardIndexList)
                newShinsei.ShinseiActionsIndex.Add(shinseiCardIndex);

            if (i == 0)
            {
                data.Add(Constants.SHINSEI_COMPANION, JsonUtility.ToJson(newShinsei));
                localPlayerData.ShinseiCompanion = newShinsei;
            }
            else if (i < 6)
            {
                data.Add(Constants.SHINSEI_SLOT + i, JsonUtility.ToJson(newShinsei));
                localPlayerData.ShinseiParty.Add(newShinsei);
            }
            else
            {
                shinseiVaultList.Add(newShinsei);
            }
        }

        ShinseiVault shinseiVault = new ShinseiVault() { ShinseiVaultList = shinseiVaultList };
        data.Add(Constants.SHINSEI_VAULT, JsonUtility.ToJson(shinseiVault));
        localPlayerData.ShinseiVault = shinseiVault;
    }
    #endregion Fill Player Data

    #region Helpers
    public void SetLocalPlayerId(ulong data)
    {
        localPlayerData.localPlayerNetId = data;
    }

    private void GenerateShinseiIcons()
    {
        List<Shinsei> playerShinseis = new List<Shinsei>();
        playerShinseis.Add(localPlayerData.ShinseiCompanion);
        playerShinseis.AddRange(localPlayerData.ShinseiParty);
        playerShinseis.AddRange(localPlayerData.ShinseiVault.ShinseiVaultList);
        ServiceLocator.Instance.GetService<IIconGeneration>().GenerateShinseiIcons(playerShinseis);
        StartCoroutine(IconRefreshTime(playerShinseis));
    }

    private IEnumerator IconRefreshTime(List<Shinsei> ps)
    {
        yield return new WaitUntil(() => ps[ps.Count - 1].shinseiIcon != null);
        OnDataObtained?.Invoke();
    }

    public void UpdatePlayerData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        data.Add(Constants.SHINSEI_COMPANION, JsonUtility.ToJson(localPlayerData.ShinseiCompanion));
        int shinseiIndex = 0;
        localPlayerData.ShinseiParty.ForEach((shinsei) =>
        {
            shinseiIndex++;
            data.Add(Constants.SHINSEI_SLOT + shinseiIndex, JsonUtility.ToJson(shinsei));
        });
        data.Add(Constants.SHINSEI_VAULT, JsonUtility.ToJson(localPlayerData.ShinseiVault));
        data.Add(Constants.CARD_STORE, JsonUtility.ToJson(localPlayerData.Deck));

        PlayfabManager.Singleton.SetUserData(data, UserDataPermission.Public);
    }

    public void UpdatePlayerName(UpdateUserTitleDisplayNameResult displayNameResult)
    {
        localPlayerData.playerName = displayNameResult.DisplayName;
    }
    #endregion Helpers
    #endregion ----Methods----
}
