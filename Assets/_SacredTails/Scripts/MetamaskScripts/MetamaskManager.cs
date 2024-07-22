using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MetaMask.Models;
using MetaMask.SocketIOClient;
using MetaMask.Unity;


public class MetamaskManager : MonoBehaviour
{
    private void Awake()
    {
        //this.currentUI = mainMenu;

        MetaMaskUnity.Instance.Initialize();
        MetaMaskUnity.Instance.Events.WalletAuthorized += walletConnected;
        MetaMaskUnity.Instance.Events.WalletReady += walletReady;
        /*MetaMaskUnity.Instance.Events.WalletDisconnected += walletDisconnected;
        
        MetaMaskUnity.Instance.Events.WalletPaused += walletPaused;
        MetaMaskUnity.Instance.Events.EthereumRequestResultReceived += TransactionResult;*/
    }

    private void OnDisable()
    {
        /*MetaMaskUnity.Instance.Events.WalletAuthorized -= walletConnected;
        MetaMaskUnity.Instance.Events.WalletDisconnected -= walletDisconnected;
        MetaMaskUnity.Instance.Events.WalletReady -= walletReady;
        MetaMaskUnity.Instance.Events.WalletPaused -= walletPaused;
        MetaMaskUnity.Instance.Events.EthereumRequestResultReceived -= TransactionResult;*/
    }

    public void Connect()
    {
        MetaMaskUnity.Instance.Connect();
    }

    private void walletConnected(object sender, EventArgs e)
    {
        Debug.Log("La Wallet se ha conectado");
        /*UnityThread.executeInUpdate(() =>
        {
            onWalletConnected?.Invoke(this, EventArgs.Empty);
        });*/
    }

    private void walletReady(object sender, EventArgs e)
    {
        Debug.Log("La Wallet Ready");
        /*UnityThread.executeInUpdate(() =>
        {
            onWalletReady?.Invoke(this, EventArgs.Empty);
        });*/
    }

    // Start is called before the first frame update
    /*void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }*/
}