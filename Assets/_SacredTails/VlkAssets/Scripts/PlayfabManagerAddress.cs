using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayfabManagerAddress : MonoBehaviour
{
    #region Singleton
    public static PlayfabManagerAddress Singleton;
    #endregion Singleton

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoginOrSignUp(string userAddress, Action<PlayFabError> successCallback = null, Action<PlayFabError> errorCallback = null)
    {
        Debug.Log(userAddress);
        var request = new LoginWithCustomIDRequest {
            CustomId = userAddress,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            success => { },
            error=> { }
            );

        //PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    #region Handlers
    private void OnLoginSuccess(LoginResult result)
    {
        if (result.NewlyCreated)
        {
            Debug.Log("Create account");
        }

        StartCoroutine(WaitUntilClientIsLogged());

        Debug.Log("Is LOGIN step 1!");

        IEnumerator WaitUntilClientIsLogged()
        {
            yield return new WaitUntil(PlayFabClientAPI.IsClientLoggedIn);
            //PlayfabAnalytics._instance.UpdateTotalOpenGame();
            Debug.Log("Is LOGIN step 2!");
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
    #endregion Handlers
}
