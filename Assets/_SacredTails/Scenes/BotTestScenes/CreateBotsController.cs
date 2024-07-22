using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CreateBotsController : MonoBehaviour
{
    public int lastBotIndex = 32;
    public int numberOfBotsToCreate = 8;
    private int currentBotIndex = 32;
    public BotPlayfabIdsList botPlayfabIdsList;

    public void Start()
    {
        currentBotIndex = lastBotIndex;
    }

    [ContextMenu("Set init number")]
    public void SetInitNumber()
    {
        PlayerPrefs.SetInt("currentBotCreation", lastBotIndex);
    }

    [ContextMenu("Create Bots")]
    public void CreateBots()
    {
        currentBotIndex++;
        SignUp($"bot{currentBotIndex}", $"bot{currentBotIndex}@timba.co", "123456");
    }

    public void SignUp(string userName, string userEmail, string password)
    {
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail,
            DisplayName = userName,
            Username = userName,
            Password = password
        };
        SacredTailsLog.LogMessageForBot($"Request Bot creation:  {(JsonConvert.SerializeObject(registerRequest))} ");
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
            success =>
            {
                botPlayfabIdsList.playfabIdList.Add(success.PlayFabId);
                SacredTailsLog.LogMessageForBot($"Successful bot creation:  {(JsonConvert.SerializeObject(success))} ");
                if (currentBotIndex <= lastBotIndex + numberOfBotsToCreate)
                    CreateBots();
            },
            error =>
            {
                SacredTailsLog.LogErrorMessageForBot($"Error creating bots, {JsonConvert.SerializeObject(error)}");
            }
        );
    }
}
