using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Timba.SacredTails.DialogSystem
{
    /// <summary>
    /// Draw necesary components to dialog system in player screen
    /// </summary>
    public class DialogUI : MonoBehaviour
    {
        [SerializeField] float writeTime = 0.4f;
        float currentWriteTime = 0.4f;
        [SerializeField] TextMeshProUGUI dialogTextField;
        [SerializeField] Transform responseParent;
        [SerializeField] GameObject responsePrefab;
        [SerializeField] GameObject skipButton;
        [SerializeField] public List<AnswerField> answerFields;
        [SerializeField] CanvasGroup canvas;
        bool hasPlayerSkippedDialogue = false, isPlayerDialogate = false;
        public bool IsPlayerDialogate { get => isPlayerDialogate; set { isPlayerDialogate = value; } }
        private void Update()
        {
            SkipDialog();
        }

        public void ShowResponses(List<string> responses, Conversation conversation, Action EndConversationCallback)
        {
            hasPlayerSkippedDialogue = false;
            skipButton.SetActive(false);
            if (conversation.currentNode.sequentialAnswers || conversation.currentNode.randomAnswer)
            {
                int answerIndex = -1;
                if (conversation.currentNode.sequentialAnswers)
                {
                    answerIndex = PlayerPrefs.GetInt(conversation.currentNode.sequentialCurrentAnswerKey);
                    if (answerIndex + 1 < responses.Count)
                        PlayerPrefs.SetInt(conversation.currentNode.sequentialCurrentAnswerKey, answerIndex + 1);
                }
                else if (conversation.currentNode.randomAnswer)
                    answerIndex = UnityEngine.Random.Range(0, responses.Count);

                var port = conversation.currentNode.GetPort("Answers " + answerIndex);
                conversation.UpdateDialog(port.Connection.node as DialogNode, this, EndConversationCallback);
                return;
            }
            foreach (var answerField in answerFields)
                answerField.gameObject.SetActive(false);
            if (responses.Count <= 0)
                canvas.alpha = 0;
            else
                canvas.alpha = 1;

            bool customBackResponseText = false;
            int indexCustomBackResponseText = 0;
            for (int i = 0; i < responses.Count; i++)
            {
                if(!responses[i].Contains("{x}"))
                {
                    if (answerFields.Count - 1 < i)
                    {
                        AnswerField answerField = Instantiate(responsePrefab, responseParent).GetComponent<AnswerField>();
                        answerFields.Add(answerField);
                    }
                    answerFields[i].SetButtonResponse(i, responses[i], conversation, skipDialog: () => SkipDialog());
                    answerFields[i].gameObject.SetActive(true);
                }
                else
                {
                    indexCustomBackResponseText = i;
                    customBackResponseText = true;
                }
            }

            AnswerField backResponse = Instantiate(responsePrefab, responseParent).GetComponent<AnswerField>();
            answerFields.Add(backResponse);
            //backResponse.SetButtonResponse(responses.Count, "Goodbye!", conversation, EndConversationCallback, skipDialog: () => SkipDialog());
            String backResponseText = customBackResponseText ? responses[indexCustomBackResponseText].Replace("{x}-", "") : "I think I am done for now";
            backResponse.SetButtonResponse(responses.Count, backResponseText, conversation, EndConversationCallback, skipDialog: () => SkipDialog());
            //backResponse.SetButtonResponse(responses.Count, "I think I am done for now", conversation, EndConversationCallback, skipDialog: () => SkipDialog());
            backResponse.gameObject.SetActive(true);
        }

        public void WriteText(string dialogText, Action OnEndWrite = null, bool isTextWithAnswer = false)
        {
            if (!isTextWithAnswer)
                skipButton.SetActive(true);
            if (!gameObject.activeSelf)
                return;
            StartCoroutine(WriteDialog(new List<char>(dialogText.ToCharArray()), OnEndWrite, isTextWithAnswer));
        }

        public void SkipDialog(bool forceSkip = false)
        {
            if (Input.GetKeyDown(KeyCode.Return) || forceSkip)
                hasPlayerSkippedDialogue = true;
            if (Input.GetKeyDown(KeyCode.Mouse0) || forceSkip)
                nextText = true;
        }

        private bool nextText = false;
        IEnumerator WriteDialog(List<char> characters, Action OnEndWrite = null, bool isTextWithAnswer = false)
        {
            nextText = false;
            dialogTextField.text = "";
            currentWriteTime = writeTime;
            Queue<char> characterQueue = new Queue<char>(characters);
            while (characterQueue.Count > 0 && (!hasPlayerSkippedDialogue || isTextWithAnswer))
            {
                dialogTextField.text += characterQueue.Dequeue();
                AkSoundEngine.PostEvent("U_Text", gameObject);
                yield return new WaitForSecondsRealtime(currentWriteTime);
            }
            hasPlayerSkippedDialogue = false;
            while (!nextText)
                yield return null;
            OnEndWrite?.Invoke();
        }
    }
}