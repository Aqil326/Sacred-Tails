using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using MultiplayerModels = PlayFab.MultiplayerModels;
using UnityEngine.Events;
using System;
using System.Text;
using Timba.Games.SacredTails;
using Timba.Games.SacredTails.LobbyDatabase;
using PlayFab.CloudScriptModels;
using Newtonsoft.Json;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using Sirenix.Utilities;
using System.Net.Mail;

/// <summary>
/// This works as bridge with playfab, all client petitions pass first here
/// </summary>
public class PlayfabManager : MonoBehaviour
{
    #region ----Fields----
    #region Singleton
    public static PlayfabManager Singleton;
    #endregion Singleton

    #region BattleServer
    public bool debugDataOnServerRequest;
    #endregion BattleServer

    public bool loginWithAddress = false;

    #region Auth
    public UnityEvent<LoginResult> OnLoginSucces;
    public event Action<string> OnSuccess;

    public UnityEvent<RegisterPlayFabUserResult> OnSignupSuccess;
    public UnityEvent OnLoginFailed;
    public UnityEvent OnUserNotVerified;

    public UnityEvent<UpdateUserTitleDisplayNameResult> OnUpdateNameSuccess;
    #endregion Auth

    #region Match
    public UnityEvent<MultiplayerModels.CreateMatchmakingTicketResult> OnCreateTicketSuccess;
    public UnityEvent OnCreateTicketFailed;

    public UnityEvent<MultiplayerModels.GetMatchmakingTicketResult> OnTicketResultSuccess;
    public UnityEvent OnTicketResultFailed;

    public UnityEvent<MultiplayerModels.GetMatchResult> OnMatchResultSuccess;
    public UnityEvent OnMatchResultFailed;
    #endregion Match

    #region Economy
    public UnityEvent<PlayFab.ClientModels.GetUserInventoryResult> OnUserCurrencyGetSuccess;
    public UnityEvent OnUserCurrencyGetFailed;

    public UnityEvent<PlayFab.ClientModels.GetCatalogItemsResult> OnGetCardsStoreSuccess;
    public UnityEvent OnGetCardsStoreFailed;

    public UnityEvent<PurchaseItemResult> OnPurchaseCardsSuccess;
    public UnityEvent<PlayFabError> OnPurchaseCardsFailed;
    #endregion Economy

    #region Leaderboards
    public event Action<string> OnGetLeaderboardsArroundPlayerSuccess;
    public event Action<string> OnGetLeaderboardsSuccess;
    #endregion
    #endregion ----Fields----

    #region ----Methods----
    #region Init
    private void Awake()
    {
        loginWithAddress = false;

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
    #endregion Init

    #region Auth
    /// <summary>
    /// Register with user email and password
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userEmail"></param>
    /// <param name="password"></param>
    /// <param name="errorCallback"></param>
    public void SignUp(string userName, string userEmail, string password, Action<PlayFabError> errorCallback = null)
    {
        Debug.Log("Correo: " + userEmail);
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail,
            Username = userName,
            Password = password
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
            success =>
            {
                var request = new AddOrUpdateContactEmailRequest
                {
                    EmailAddress = userEmail
                };
                PlayFabClientAPI.AddOrUpdateContactEmail(request, result =>
                {
                    Debug.Log("The player's account has been updated with a contact email");
                }, (PlayFabError error) => Debug.LogError("Error updating contact email :: " + error.ErrorMessage));
                SacredTailsLog.LogMessageForBot("Succesfull signUp account " + success.PlayFabId);

                OnSignupSuccess?.Invoke(success);
            },
            error =>
            {
                OnLoginFailed?.Invoke();
                errorCallback?.Invoke(error);
            }
        );
    }

    public void Login(string email, string password, Action<PlayFabError> errorCallback = null, bool isTournamentCreationLogin = false)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new PlayFab.ClientModels.GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetUserAccountInfo = true,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowContactEmailAddresses = true
                }
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request,
            success =>
            {
                //success.InfoResultPayloadultPayLoad.PlayerProfile.ContactEmailAddresses.Length > 0 && success.InfoResultPayLoad.PlayerProfile.ContactEmailAddresses[0].VerificationStatus != "Confirmed"
                //Debug.Log($"Contact : {success.InfoResultPayload.PlayerProfile.ContactEmailAddresses} Verification status: {success.InfoResultPayload.PlayerProfile.ContactEmailAddresses[0].VerificationStatus.ToString()}");
                //ENABLE FOR VALIDE MAIL STATUS:
                /*if ((success.InfoResultPayload.PlayerProfile.ContactEmailAddresses != null && success.InfoResultPayload.PlayerProfile.ContactEmailAddresses.Count > 0) && success.InfoResultPayload.PlayerProfile.ContactEmailAddresses[0].VerificationStatus.ToString() != "Confirmed")
                {
                    OnLoginFailed?.Invoke();
                    errorCallback?.Invoke(new PlayFabError()
                    {
                        Error = PlayFabErrorCode.AccountNotFound,
                        ErrorMessage = "Verify your email account"
                    });
                    Debug.Log("User email not verified");
                    return;
                }*/
                OnLoginSucces?.Invoke(success);
                OnSuccess?.Invoke(success.PlayFabId);
                if (isTournamentCreationLogin)
                    return;
                if (string.IsNullOrEmpty(success.InfoResultPayload.PlayerProfile.DisplayName))
                {
                    UpdateDisplayName(success.InfoResultPayload.AccountInfo.Username);
                }
                GetPlayerCurrency();
                ServiceLocator.Instance.GetService<IWallet>().ShowUserWallet();
                ServiceLocator.Instance.GetService<IWallet>().UpdateUserWallet();
                SacredTailsLog.LogMessageForBot("Succesfull login account " + success.PlayFabId);
            },
            error =>
            {
                OnLoginFailed?.Invoke();
                errorCallback?.Invoke(error);
            }
        );
    }

    public void RequestPasswordRecovery(string email, Action<string> successCallback = null, Action<PlayFabError> errorCallback = null)
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = Constants.TITLE_ID
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request,
            success =>
            {
                string successMessage = "Reset pasword email sent";
                SacredTailsLog.LogMessageForBot(successMessage);
                successCallback?.Invoke(successMessage);
            },
            error =>
            {
                errorCallback?.Invoke(error);
                SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
            });
    }
    #endregion Auth

    #region Address Auth

    public void LoginOrSignUp(string userAddress, bool userHasNfts, Action<PlayFabError> errorCallback = null)
    {
        Debug.Log(userAddress);
        var request = new LoginWithCustomIDRequest {
            CustomId = userAddress,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            success => {
                //OnSignupSuccess?.Invoke(success);
                //OnSignupSuccess?.Invoke(null);
                loginWithAddress = userHasNfts;//true;
                OnLoginSucces?.Invoke(success);
                OnSuccess?.Invoke(success.PlayFabId);
                if (success.InfoResultPayload != null && string.IsNullOrEmpty(success.InfoResultPayload.PlayerProfile.DisplayName))
                {
                    UpdateDisplayName(success.InfoResultPayload.AccountInfo.Username);
                }
                GetPlayerCurrency();
                ServiceLocator.Instance.GetService<IWallet>().ShowUserWallet();
                ServiceLocator.Instance.GetService<IWallet>().UpdateUserWallet();
                SacredTailsLog.LogMessageForBot("Succesfull login account " + success.PlayFabId);
            },
            error => {
                OnLoginFailed?.Invoke();
                errorCallback?.Invoke(error);
            }
            );

        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    #endregion Address Auth

    #region User Data
    public void UpdateDisplayName(string newName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, success =>
        {
            OnUpdateNameSuccess?.Invoke(success);
            SacredTailsLog.LogMessageForBot("Display Name updated");

        },
               error => { SacredTailsLog.LogMessageForBot(error.GenerateErrorReport()); });
    }

    public void UpdateStatistic(string statisticName, int value)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = statisticName, Value = value },
                }
        },
        result => { SacredTailsLog.LogMessageForBot("User statistics updated"); },
        error => { SacredTailsLog.LogErrorMessageForBot(error.GenerateErrorReport()); });
    }

    public void GetStatistics(Action<List<StatisticValue>> actionCallback)
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            (result) =>
            {
                actionCallback?.Invoke(result.Statistics);
            },
            error => SacredTailsLog.LogErrorMessageForBot(error.GenerateErrorReport())
        );
    }

    /// <summary>
    /// Remove  keys of user data
    /// </summary>
    /// <param name="newData"></param>
    public void RemoveUserData(List<string> keysToRemove, Action<UpdateUserDataResult> onResult = null, int timesTrying = 0, PlayFab.ClientModels.UserDataPermission permission = PlayFab.ClientModels.UserDataPermission.Private)
    {
        PlayFab.ClientModels.UpdateUserDataRequest userRequest = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Permission = permission,
            KeysToRemove = keysToRemove
        };
        PlayFabClientAPI.UpdateUserData(userRequest,
        result =>
        {
            onResult?.Invoke(result);
            SacredTailsLog.LogMessageForBot("Successfully updated user data");
        },
        error =>
        {
            SacredTailsLog.LogMessageForBot("Got error setting user data");
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
            int newTimes = timesTrying + 1;
            RemoveUserData(keysToRemove, onResult, newTimes, permission);
        });
    }
    /// <summary>
    /// Send <string,string> pair dictionary for change user data
    /// </summary>
    /// <param name="newData"></param>
    public void SetUserData(Dictionary<string, string> newData, PlayFab.ClientModels.UserDataPermission permission = PlayFab.ClientModels.UserDataPermission.Private)
    {
        PlayFab.ClientModels.UpdateUserDataRequest userRequest = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = newData,
            Permission = permission
        };

        //Debug new data dictionary with Debug.Log
        foreach (var item in newData)
        {
            Debug.Log($"Key change: {item.Key} Value: {item.Value}");
        }
        PlayFabClientAPI.UpdateUserData(userRequest,
        result => SacredTailsLog.LogMessageForBot("Successfully updated user data"),
        error =>
        {
            SacredTailsLog.LogMessageForBot("Got error setting user data");
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }


    public void SetUserData(Dictionary<string, string> newData, Action callback, PlayFab.ClientModels.UserDataPermission permission = PlayFab.ClientModels.UserDataPermission.Private)
    {
        PlayFab.ClientModels.UpdateUserDataRequest userRequest = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = newData,
            Permission = permission
        };

        //Debug new data dictionary with Debug.Log
        foreach (var item in newData)
        {
            Debug.Log($"Key change: {item.Key} Value: {item.Value}");
        }
        PlayFabClientAPI.UpdateUserData(userRequest,
        result => {
            SacredTailsLog.LogMessageForBot("Successfully updated user data");
            callback?.Invoke();
            },
        error =>
        {
            SacredTailsLog.LogMessageForBot("Got error setting user data");
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }

    public void SetUserDataForBot(Dictionary<string, string> newData, string playfabId,Action callback, PlayFab.ClientModels.UserDataPermission permission = PlayFab.ClientModels.UserDataPermission.Private)
    {
        if(playfabId != PlayerDataManager.Singleton.localPlayerData.playfabId)
        {
            Debug.Log("Bot ID is different from local player ID");
            return;
        }
        PlayFab.ClientModels.UpdateUserDataRequest userRequest = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = newData,
            Permission = permission
        };

        PlayFabClientAPI.UpdateUserData(userRequest,
        result => {
            SacredTailsLog.LogMessageForBot("Successfully updated user data");
            callback?.Invoke();
        },
        error =>
        {
            SacredTailsLog.LogMessageForBot("Got error setting user data");
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }

    /*public void SetUserData(Dictionary<string, string> newData, string playfabId, PlayFab.ServerModels.UserDataPermission permission = PlayFab.ServerModels.UserDataPermission.Private, Action callback = null, Action badCallback = null)
    {
        PlayFab.ServerModels.UpdateUserDataRequest userRequest = new PlayFab.ServerModels.UpdateUserDataRequest()
        {
            PlayFabId = playfabId,
            Data = newData,
            Permission = permission
        };
        PlayFabServerAPI.UpdateUserData(userRequest,
        result => callback?.Invoke(),
        error =>
        {
            badCallback?.Invoke();
            SacredTailsLog.LogMessageForBot("Got error setting user data");
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }*/

    public void GetUserData(string playfabId, List<string> targetKeys = null, Action<PlayFab.ClientModels.GetUserDataResult> OnResult = null)
    {
        PlayFab.ClientModels.GetUserDataRequest userRequest = new PlayFab.ClientModels.GetUserDataRequest()
        {
            PlayFabId = playfabId,
            Keys = targetKeys
        };
        PlayFabClientAPI.GetUserData(userRequest,
            OnResult,
            error =>
            {
                SacredTailsLog.LogMessageForBot("Got error retrieving user data:");
                SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
            }
        );
    }

    /*public void GetServerUserData(string playfabId, List<string> targetKeys = null, Action<PlayFab.ServerModels.GetUserDataResult> OnResult = null, Action errorCallback = null)
    {
        PlayFab.ServerModels.GetUserDataRequest userRequest = new PlayFab.ServerModels.GetUserDataRequest()
        {
            PlayFabId = playfabId,
            Keys = targetKeys
        };
        PlayFabServerAPI.GetUserData(userRequest,
            OnResult,
            error =>
            {
                SacredTailsLog.LogMessageForBot("Got error retrieving user data:");
                SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
                errorCallback?.Invoke();
            }
        );
    }*/

    /*public void SetCharStyle(string playfabId, string characterStyle, Action<PlayFab.ServerModels.UpdateUserDataResult> OnResult = null, Action errorCallback = null)
    {
        PlayFab.ServerModels.UpdateUserDataRequest userRequest = new PlayFab.ServerModels.UpdateUserDataRequest()
        {
            PlayFabId = playfabId,
            Data = new Dictionary<string, string>() { { "CharacterStyle", characterStyle } }
        };


        PlayFabServerAPI.UpdateUserData(userRequest,
            OnResult,
            error =>
            {
                SacredTailsLog.LogMessageForBot("Got error retrieving user data:");
                SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
                errorCallback?.Invoke();
            }
        );
    }

    public void GetCharStyle(string playfabId, Action<PlayFab.ServerModels.GetUserDataResult> OnResult = null, Action errorCallback = null)
    {
        PlayFab.ServerModels.GetUserDataRequest userRequest = new PlayFab.ServerModels.GetUserDataRequest()
        {
            PlayFabId = playfabId,
            Keys = new List<string>() { "CharacterStyle" }
        };
        PlayFabServerAPI.GetUserData(userRequest,
            OnResult,
            error =>
            {
                SacredTailsLog.LogMessageForBot("Got error retrieving user data:");
                SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
                errorCallback?.Invoke();
            }
        );
    }*/

    #endregion User Data

    #region Match
    /// <summary>
    /// Create a matchmaking ticket to the specified Queue
    /// </summary>
    public void CreateMatchTicket(string uniqueId, string playerType, int skillLevel, string queueName)
    {
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
          new MultiplayerModels.CreateMatchmakingTicketRequest
          {
              Creator = new MultiplayerModels.MatchmakingPlayer
              {
                  Entity = new MultiplayerModels.EntityKey
                  {
                      Id = uniqueId,
                      Type = playerType,
                  },
                  Attributes = new MultiplayerModels.MatchmakingPlayerAttributes
                  {
                      DataObject = new CustomAtributes
                      {
                          PlayerPlayfabId = PlayerDataManager.Singleton.localPlayerData.playfabId,
                          Skill = skillLevel,
                          displayName = PlayerDataManager.Singleton.localPlayerData.playerName
                      }
                  },
              },
              GiveUpAfterSeconds = 120,
              QueueName = queueName,
          },
          Success =>
          {
              OnCreateTicketSuccess.Invoke(Success);
              SacredTailsLog.LogMessageForBot("Ticket creation succeded :" + Success.TicketId);
          },
          error =>
          {
              OnCreateTicketFailed.Invoke();
              SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
          });

    }

    void OnError(PlayFabError error)
    {
        SacredTailsLog.LogMessageForBot("Error while loggin in/creating account!");
        SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
    }

    /// <summary>
    /// call this method every 6 to 10 seconds to query the specified ticket state
    /// </summary>
    public void GetTicketState(string ticketId, string queueName, Action<MultiplayerModels.GetMatchmakingTicketResult> result)
    {

        PlayFabMultiplayerAPI.GetMatchmakingTicket(
       new MultiplayerModels.GetMatchmakingTicketRequest
       {
           TicketId = ticketId,
           QueueName = queueName,
       },
       success =>
       {
           OnTicketResultSuccess.Invoke(success);
           SacredTailsLog.LogMessageForBot("Ticket Status: " + success.Status);
           result.Invoke(success);
       },
       error =>
       {
           OnTicketResultFailed.Invoke();
           SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());

       });
    }
    /// <summary>
    /// Gets the created match under the specified ticket id
    /// </summary>
    public void GetMatch(string matchId, string queueName)
    {
        PlayFabMultiplayerAPI.GetMatch(
        new MultiplayerModels.GetMatchRequest
        {
            MatchId = matchId,
            QueueName = queueName,
            ReturnMemberAttributes = true
        },
        success =>
        {
            OnMatchResultSuccess.Invoke(success);
            SacredTailsLog.LogMessageForBot("Match Found");
        },
        error =>
        {
            OnMatchResultFailed.Invoke();
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }

    public void CancelMatchmaking(string queueName, string ticketId)
    {
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(
         new MultiplayerModels.CancelMatchmakingTicketRequest
         {
             QueueName = queueName,
             TicketId = ticketId,
         },
        success =>
        {
            SacredTailsLog.LogMessageForBot("Match Canceled for " + ticketId);
        },
        error =>
        {
            SacredTailsLog.LogMessageForBot(error.GenerateErrorReport());
        });
    }
    #endregion Match

    #region Economy
    public void GetPlayerCurrency()
    {
        PlayFabClientAPI.GetUserInventory(new PlayFab.ClientModels.GetUserInventoryRequest(),
        (result) => OnUserCurrencyGetSuccess?.Invoke(result),
        (error) => SacredTailsLog.LogErrorMessageForBot($"<color=red>Playfab Error: </color>{error.ErrorMessage}"));
    }
    public void GetStoreCards()
    {
        PlayFabClientAPI.GetCatalogItems(new PlayFab.ClientModels.GetCatalogItemsRequest() { CatalogVersion = "0.0.1" },
        (result) => OnGetCardsStoreSuccess?.Invoke(result),
        (error) => SacredTailsLog.LogErrorMessageForBot($"<color=red>Playfab Error: </color>{error.ErrorMessage}"));
    }

    public void PurchaseCard(int itemId, uint itemPrice)
    {
        PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest() { ItemId = itemId.ToString(), VirtualCurrency = "SC", Price = (int)itemPrice },
        (result) => OnPurchaseCardsSuccess?.Invoke(result),
        (error) => OnPurchaseCardsFailed?.Invoke(error));
    }

    public void AddPlayerCurrency(int amountToModify)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new PlayFab.ClientModels.AddUserVirtualCurrencyRequest() { Amount = amountToModify, VirtualCurrency = "SC" },
        (result) => { GetPlayerCurrency(); },
        (error) => SacredTailsLog.LogErrorMessageForBot($"<color=red>Playfab Error: </color>{error.ErrorMessage}"));
    }

    public void SubtractPlayerCurrency(int amountToModify)
    {
        PlayFabClientAPI.SubtractUserVirtualCurrency(new PlayFab.ClientModels.SubtractUserVirtualCurrencyRequest() { Amount = amountToModify, VirtualCurrency = "SC" },
        (result) => { GetPlayerCurrency(); },
        (error) => SacredTailsLog.LogErrorMessageForBot($"<color=red>Playfab Error: </color>{error.ErrorMessage}"));
    }
    #endregion Economy

    #region Leaderboards

    public void UpdatePlayerStatistics(string leaderboardName, int score)
    {
        var request = new PlayFab.ClientModels.UpdatePlayerStatisticsRequest
        {
            Statistics = new List<PlayFab.ClientModels.StatisticUpdate>
            {
                new PlayFab.ClientModels.StatisticUpdate
                {
                    StatisticName = leaderboardName,
                    Value = score
                }
            },
        };

        PlayFabClientAPI.UpdatePlayerStatistics
            (request,
            success =>
            {
                SacredTailsLog.LogMessageForBot("Score Updated");
            },
            error =>
            {
                SacredTailsLog.LogErrorMessageForBot($"Here's some debug information: {error.GenerateErrorReport()}");
            });
    }

    //Get leaderboards around player
    public void GetLeaderboardAroundPlayer(string playerId, int maxResultCount, string leaderboardName)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            PlayFabId = playerId,
            MaxResultsCount = maxResultCount,
            StatisticName = leaderboardName
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetLeaderboardAroundPlayerSuccess, error =>
        {
            SacredTailsLog.LogErrorMessageForBot($"Here's some debug information: {error.GenerateErrorReport()}");
        });
    }

    public void GetLeaderboardAroundPlayer(string playerId, int maxResultCount, string leaderboardName, Action<GetLeaderboardAroundPlayerResult> callback)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            PlayFabId = playerId,
            MaxResultsCount = maxResultCount,
            StatisticName = leaderboardName
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, callback, error =>
        {
            SacredTailsLog.LogErrorMessageForBot($"Here's some debug information: {error.GenerateErrorReport()}");
        });
    }

    private void OnGetLeaderboardAroundPlayerSuccess(GetLeaderboardAroundPlayerResult response)
    {
        var result = JsonConvert.SerializeObject(response.Leaderboard);
        OnGetLeaderboardsArroundPlayerSuccess?.Invoke(result);
    }

    public void GetLeaderboardEntries(int startPosition, int maxResultCount, string leaderboardName)
    {
        var request = new PlayFab.ClientModels.GetLeaderboardRequest
        {
            StartPosition = startPosition,
            MaxResultsCount = maxResultCount,
            StatisticName = leaderboardName
        };
        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, error =>
        {
            SacredTailsLog.LogErrorMessageForBot($"Here's some debug information: {error.GenerateErrorReport()}");
        });
    }

    public void GetLeaderboardEntries(int startPosition, int maxResultCount, string leaderboardName, Action<GetLeaderboardResult> Callback)
    {
        var request = new PlayFab.ClientModels.GetLeaderboardRequest
        {
            StartPosition = startPosition,
            MaxResultsCount = maxResultCount,
            StatisticName = leaderboardName
        };
        PlayFabClientAPI.GetLeaderboard(request, Callback, error =>
        {
            SacredTailsLog.LogErrorMessageForBot($"Here's some debug information: {error.GenerateErrorReport()}");
        });
    }

    private void OnGetLeaderboardSuccess(PlayFab.ClientModels.GetLeaderboardResult response)
    {
        var Leaderboard = new StringBuilder();
        foreach (var playerLeaderboardEntry in response.Leaderboard)
        {
            Leaderboard.AppendLine($"{playerLeaderboardEntry.Position + 1}.- {playerLeaderboardEntry.DisplayName} {playerLeaderboardEntry.StatValue}");
        }
        var result = JsonConvert.SerializeObject(response.Leaderboard);
        OnGetLeaderboardsSuccess?.Invoke(result);
        //OnGetLeaderboardsSuccess?.Invoke(leaderboardEntries.ToString());
    }

    #endregion

    #region BattleServer
    public const string debugMessagePrefix = "<color=orange>PlayfabManager battleServer message: </color>";

    #region <<<Init>>>
    public void BattleServerCreateMatch(string matchId, Action<ExecuteFunctionResult> resultCallback)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_CreateMatch",
            FunctionParameter = new { Keys = new { MatchId = matchId } }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Create match with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");
        PlayFabCloudScriptAPI.ExecuteFunction(req, (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Create match response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, (error) =>
        {
            BattleServerCreateMatch(matchId, resultCallback);
        });
    }

    public void BattleServerCheckMatchConfirm(string matchId, int spawnPoint, Action<ExecuteFunctionResult> resultCallback, Action<PlayFabError> errorCallback = null)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_MatchConfirm",
            FunctionParameter = new { Keys = new { MatchId = matchId, SpawnPoint = spawnPoint } }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Check match confirm with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");
        PlayFabCloudScriptAPI.ExecuteFunction(req, (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Check match response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, errorCallback);
    }

    public void BattleServerSelectShinseis(string matchId, CombatPlayer playerData, List<int> shinseisPlayer, Action<ExecuteFunctionResult> resultCallback, Action<PlayFabError> errorCallback)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_SelectShinseis",
            FunctionParameter = new
            {
                Keys = new
                {
                    MatchId = matchId,
                    ShinseiIdList = shinseisPlayer,
                    PlayerMatchData = UnityNewtonsoftJsonSerializer.Serialize(playerData)
                }
            }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Select shinsei with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");
        PlayFabCloudScriptAPI.ExecuteFunction(req,
        (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix} Select shinsei response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, (error) =>
        {
            errorCallback?.Invoke(error);
            BattleServerSelectShinseis(matchId, playerData, shinseisPlayer, resultCallback, errorCallback);
        });
    }

    public void BattleServerCheckShinseisChoosed(string matchId, Action<ExecuteFunctionResult> resultCallback)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_CheckShinseisChoosed",
            FunctionParameter = new
            {
                Keys = new
                {
                    MatchId = matchId
                }
            }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Check shinsei choosed with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");

        PlayFabCloudScriptAPI.ExecuteFunction(req,
        (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix} Check Shinsei Choosed response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, null);
    }
    #endregion <<<Init>>>

    #region <<<Battle>>>
    public void BattleServerSendTurn(string matchId, int indexOfCard, Action<ExecuteFunctionResult> resultCallback, int counter = 0, int currentShinsei = 0)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_SendTurnDev",
            FunctionParameter = new
            {
                Keys = new
                {
                    MatchId = matchId,
                    indexCard = indexOfCard,
                    clientShinsei = currentShinsei
                }
            }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Send Turn with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");

        PlayFabCloudScriptAPI.ExecuteFunction(req,
        (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix} Send turn response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, (error) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix} Send turn Error: {JsonConvert.SerializeObject(error, Formatting.Indented)}");
            if (counter < 5)
                BattleServerSendTurn(matchId, indexOfCard, resultCallback, counter++);
            else
                resultCallback?.Invoke(new ExecuteFunctionResult()
                {
                    FunctionResult = false
                });
        });
    }

    public void BattleServerGetMatchState(string matchId, bool isViewer = false, Action<ExecuteFunctionResult> resultCallback = null, bool retryEndMatch = false, bool enemyDisconnected = false)
    {
        var req = new ExecuteFunctionRequest()
        {
            FunctionName = "BattleServer_GetMatchState",
            FunctionParameter = new
            {
                Keys = new
                {
                    MatchId = matchId,
                    IsViewer = isViewer,
                    calculateEndMatchAgain = retryEndMatch,
                    enemyDisconnected = enemyDisconnected 
                }
            }
        };
        if (debugDataOnServerRequest)
            SacredTailsLog.LogMessageForBot($"{debugMessagePrefix}Get match turn state with this data:{JsonConvert.SerializeObject(req, Formatting.Indented)}");

        PlayFabCloudScriptAPI.ExecuteFunction(req,
        (result) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogMessageForBot($"{debugMessagePrefix} Get match turn state response: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
            resultCallback?.Invoke(result);
        }, (error) =>
        {
            if (debugDataOnServerRequest)
                SacredTailsLog.LogErrorMessageForBot($"{debugMessagePrefix} Get match turn state ERROR: {JsonConvert.SerializeObject(error, Formatting.Indented)}");
        });
    }
    #endregion <<<Battle>>>
    #endregion BattleServer
    #endregion ----Methods----
}
