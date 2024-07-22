using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using DG.Tweening;
using System;
//using Unity.Netcode.Transports.UNET;
using System.Linq;
using Timba.SacredTails.UiHelpers;
using Unity.Netcode.Transports.UTP;

namespace Timba.SacredTails.Database
{
    public class AuthController : MonoBehaviour
    {
        #region ---Fields---
        #region Login
        [SerializeField] private float timeSequence = .5f;

        [SerializeField] private RectTransform inputsPanel;

        [SerializeField] private TMP_InputField emailInput;
        [SerializeField] private TMP_InputField userNameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_Text errorLabel;

        [SerializeField] private GameObject LoginButton;
        [SerializeField] private GameObject goToSignUpButton;

        [SerializeField] private GameObject SignUpButton;
        [SerializeField] private GameObject goToLoginButton;

        [SerializeField] private GameObject SendEmailButton;
        [SerializeField] private GameObject forgotPasswordButton;
        [SerializeField] private GameObject goBackButton;

        private List<AuthPanel> changePanelObjects = new List<AuthPanel>();
        public bool tournamentCreationLogin = false;
        #endregion Login

        #region Networking
        public TMP_InputField ipField;
        public UnityTransport transport;
        #endregion Networking
        #endregion ---Fields---

        #region ---Methods---
        #region Networking
        private void Start()
        {
            if (ipField != null)
            {
                ipField.text = PlayerPrefs.GetString("LastIP", "");
                if (string.IsNullOrEmpty(ipField.text))
                    ipField.text = "127.0.0.1";
            }

            LoadCachedData();
            EventsListenging();
            CreatePanelsLists();

            if (UIGroups.instance != null)
                UIGroups.instance.ShowOnlyThisGroup("loggin");
        }

        public void LoadCachedData()
        {
            emailInput.text = PlayerPrefs.GetString("userEmail", "");
            passwordInput.text = PlayerPrefs.GetString("userPassword", "");
        }

        private void CreatePanelsLists()
        {
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.LOGIN, AuthPanelType.SIGN_UP }, panelItem = emailInput.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.LOGIN, AuthPanelType.SIGN_UP }, panelItem = passwordInput.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.LOGIN }, panelItem = LoginButton.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.LOGIN }, panelItem = goToSignUpButton.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.SIGN_UP }, panelItem = userNameInput.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.SIGN_UP }, panelItem = SignUpButton.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.SIGN_UP }, panelItem = goToLoginButton.GetComponent<RectTransform>() });

            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.FORGOT_PASSWORD }, panelItem = goBackButton.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.FORGOT_PASSWORD }, panelItem = SendEmailButton.GetComponent<RectTransform>() });
            changePanelObjects.Add(new AuthPanel() { authPanelType = new List<AuthPanelType>() { AuthPanelType.FORGOT_PASSWORD }, panelItem = forgotPasswordButton.GetComponent<RectTransform>() });
        }

        public void SetIP()
        {
            // IP
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipField.text, 7777);
            //PlayerPrefs.SetString("LastIP", ipField.text);
        }
        #endregion Networking

        #region Playfab


        #region Login
        public void SetLoginInfo(string userEmail, string password, int lobby = 0)
        {
            emailInput.text = userEmail;
            passwordInput.text = password;
        }

        public void OpenPanel(bool isLogin)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetEase(Ease.InBack);
            sequence.Join(inputsPanel.DOScale(.4f, timeSequence));

            sequence.OnComplete(() =>
            {
                changePanelObjects.ForEach((panelObject) => panelObject.panelItem.gameObject.SetActive(false));
                List<AuthPanel> panelObjects = changePanelObjects.Where((panelObject) => panelObject.authPanelType.Contains((isLogin ? AuthPanelType.LOGIN : AuthPanelType.SIGN_UP))).ToList();
                Sequence sequence2 = DOTween.Sequence();
                sequence2.SetEase(Ease.OutBack);
                sequence2.Join(inputsPanel.DOScale(1f, timeSequence));
                panelObjects.ForEach((panelObject) => panelObject.panelItem.gameObject.SetActive(true));
            });
        }

        public void OpenForgotPasswordPanel(bool isLogin)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetEase(Ease.InBack);
            changePanelObjects.ForEach((panelObject) =>
            {
                sequence.Join(panelObject.panelItem.DOScale(.4f, timeSequence));
            });
            sequence.OnComplete(() =>
            {
                LoginButton.SetActive(isLogin);
                goToSignUpButton.SetActive(isLogin);
                forgotPasswordButton.SetActive(isLogin);
                passwordInput.gameObject.SetActive(isLogin);
                ipField.gameObject.SetActive(isLogin);
                SendEmailButton.SetActive(!isLogin);
                goBackButton.SetActive(!isLogin);

                Sequence sequence2 = DOTween.Sequence();
                sequence2.SetEase(Ease.OutBack);
                changePanelObjects.ForEach((panelObject) =>
                {
                    sequence2.Join(panelObject.panelItem.DOScale(1f, timeSequence));
                });

            });
        }

        public void Login()
        {
            SetIP();
            PlayfabManager.Singleton.Login(emailInput.text, passwordInput.text, (error) =>
            {
                errorLabel.text = error.ErrorMessage;
                errorLabel.transform.parent.gameObject.SetActive(true);
            }, tournamentCreationLogin);
        }

        public void loginAddress(string userAddress, bool userHasNfts)
        {
            PlayfabManager.Singleton.LoginOrSignUp(userAddress, userHasNfts, (error) =>
            {
                errorLabel.text = error.ErrorMessage;
                errorLabel.transform.parent.gameObject.SetActive(true);
            });
        }
        #endregion Login

        #region Signup
        public void EventsListenging()
        {
            PlayfabManager.Singleton.OnSignupSuccess.AddListener((result) =>
            {
                Login();
            });

            PlayfabManager.Singleton.OnLoginSucces.AddListener((result) =>
            {
                PlayerPrefs.SetString("userEmail", emailInput.text);
                PlayerPrefs.SetString("userPassword", passwordInput.text);
            });
        }
        public void SignUp()
        {
            SetIP();
            PlayfabManager.Singleton.SignUp(userNameInput.text, emailInput.text, passwordInput.text, (error) =>
               {
                   errorLabel.text = error.ErrorMessage;
                   errorLabel.transform.parent.gameObject.SetActive(true);
                   StartCoroutine(WaitForSeconds(1, () =>
                   {
                       errorLabel.text = "-";
                       errorLabel.transform.parent.gameObject.SetActive(false);
                   }));
               });
        }

        public void SendPasswordEmail()
        {
            PlayfabManager.Singleton.RequestPasswordRecovery(emailInput.text, (success) =>
            {

                errorLabel.color = Color.green;
                errorLabel.text = success;
                errorLabel.transform.parent.gameObject.SetActive(true);
                StartCoroutine(WaitForSeconds(2, () =>
                {
                    errorLabel.text = "-";
                    errorLabel.transform.parent.gameObject.SetActive(false);
                    errorLabel.color = Color.red;
                }));

            },
            (error) =>
            {
                errorLabel.text = error.ErrorMessage;
                errorLabel.transform.parent.gameObject.SetActive(true);
                StartCoroutine(WaitForSeconds(1, () =>
                {
                    errorLabel.text = "-";
                    errorLabel.transform.parent.gameObject.SetActive(false);
                }));
            });
        }

        IEnumerator WaitForSeconds(float seconds, Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }
        #endregion Signup
        #endregion Playfab
        #endregion ---Methods---
    }

}
public class AuthPanel
{
    public List<AuthPanelType> authPanelType;
    public Transform panelItem;
}

public enum AuthPanelType
{
    SIGN_UP,
    LOGIN,
    FORGOT_PASSWORD
}