using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Timba.Games.SacredTails.LobbyNetworking;
using UnityEngine.Events;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.UiHelpers;

namespace Timba.SacredTails.DialogSystem
{
    /// <summary>
    ///     A component that allow start conversations
    /// </summary>
    public class Dialogable : MonoBehaviour
    {
        /// <summary>
        ///     Events that can be triggered in conversations using a code character
        /// </summary>
        /// <remarks>
        ///     You can find a example of use of this in NPC5_Conversation_Out scriptable object at Assets - _SacredTails - Lobby - Scripts - DialogSystem - Conversations\NPC5_Conversation_Out.asset.
        /// </remarks>
        public List<UnityEvent> CallbackEvents;
        /// <summary>
        ///     This store the answers and responses
        /// </summary>
        [SerializeField] private Conversation conversation;
        /// <summary>
        ///     This is the same that conversation but only call the first time that player interact with this Dialogable
        /// </summary>
        /// <remarks>
        ///     This variable is opcional but if you use them you need to configure the field playerPrefVar.
        /// </remarks>
        [SerializeField] private Conversation firstConversation;
        [SerializeField] GameObject canvas, indicatorWoldPosition, imageIndicator;
        [SerializeField] GameObject dialoguerCamera;
        ThirdPersonController thirdPersonController;
        [SerializeField] private bool autoTriggerConversation = false;
        private bool isOnDialog = false;
        bool firstDialog = false;
        bool playerIsIn = false;
        /// <summary>
        /// The name of variable when game saves the first time state
        /// </summary>
        [SerializeField] string playerPrefVar;

        public DialogUI dialogUI;

        private void Start()
        {
            if (PlayerPrefs.GetInt(playerPrefVar, 1) == 1)
            {
                firstDialog = true;
            }
            else
            {
                firstDialog = false;
            }
        }

        private void Update()
        {
            if (!playerIsIn)
                return;
            if ((Input.GetKeyDown(KeyCode.E) || autoTriggerConversation) && !isOnDialog)
                StartConversation();
        }

        private void LateUpdate()
        {
            if (playerIsIn)
                imageIndicator.transform.position = Camera.main.WorldToScreenPoint(indicatorWoldPosition.transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            ThirdPersonController otherController = other.GetComponent<ThirdPersonController>();
            if (otherController != null && otherController.IsLocalPlayer)
            {
                thirdPersonController = otherController;
                if (!autoTriggerConversation)
                {
                    otherController.OnDisablePLayer = () => canvas.SetActive(false);
                    canvas.SetActive(true);
                }
                else
                    canvas.SetActive(false);
                playerIsIn = true;
            }
        }


        private void OnTriggerExit(Collider other)
        {
            if (autoTriggerConversation)
                StartCoroutine(WaitForSecondsCallback(1));
            dialoguerCamera.SetActive(false);

            var otherController = other.GetComponent<ThirdPersonController>();
            if (otherController != null && otherController.IsLocalPlayer)
            {
                thirdPersonController.OnDisablePLayer = null;
                canvas.SetActive(false);
                playerIsIn = false;
            }
        }

        IEnumerator WaitForSecondsCallback(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            isOnDialog = false;
        }

        /// <summary>
        /// This start a new conversation
        /// </summary>
        public void StartConversation()
        {
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().ShowPlayerPersonalUI(false);
            if (dialogUI.IsPlayerDialogate)
                return;
            if (UIGroups.instance != null && !UIGroups.instance.lastActivate.Equals("planner"))
                return;

            isOnDialog = true;
            /*if (thirdPersonController == null)
                thirdPersonController = PlayerDataManager.Singleton.localPlayerGameObject.GetComponent<ThirdPersonController>();*/
            thirdPersonController.CanBeBlocked = true;
            thirdPersonController.IsMovementBloqued = true;
            thirdPersonController.CanBeBlocked = false;
            dialoguerCamera.SetActive(true);
            canvas.SetActive(false);
            dialogUI.gameObject.SetActive(true);
            if (UIGroups.instance != null)
                UIGroups.instance.ShowOnlyThisGroup("dialogue");
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.GetComponent<PlayerUI>().HideNameTag(false);
            if (firstDialog && firstConversation.dialogGraph != null)
            {
                PlayerPrefs.SetInt(playerPrefVar, 0);
                firstDialog = false;
                StartCoroutine(firstConversation.ConversationRoutine(dialogUI, EndConversation, this));
            }
            else
            {
                StartCoroutine(conversation.ConversationRoutine(dialogUI, EndConversation, this));
            }

        }

        public bool isExecutingAnswerWithCallbackEvent = false;
        public void EnteringAnotherPanel()
        {
            isExecutingAnswerWithCallbackEvent = true;
        }
        public void EndConversation()
        {
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().ShowPlayerPersonalUI(true);
            if (!autoTriggerConversation)
            {
                isOnDialog = false;
                canvas.SetActive(true);
            }

            dialogUI.IsPlayerDialogate = false;
            ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer.GetComponent<PlayerUI>().HideNameTag(true);
            UIGroups.instance.ShowOnlyThisGroup("planner");
            if (!isExecutingAnswerWithCallbackEvent)
            {
                thirdPersonController.CanBeBlocked = true;
                thirdPersonController.IsMovementBloqued = false;
            }

            isExecutingAnswerWithCallbackEvent = false;
            dialoguerCamera.SetActive(false);
        }
    }
}