using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Timba.SacredTails.DialogSystem
{
    /// <summary>
    /// This class keep the logic of write dialogs in screen, received a dialog graph and play them
    /// </summary>
    /// <param name="dialogGraph">The scriptable object that contains all conversation, answers and responses </param>
    [Serializable]
    public class Conversation
    {
        public DialogGraph dialogGraph;
        public DialogNode currentNode;

        public bool isWaitingResponse = false, isPlayerReadText = false;
        public int responseIndex;

        public void Init(DialogUI dialogUI, Action EndConversationCallback)
        {
            isWaitingResponse = false;
            isPlayerReadText = false;
            foreach (DialogNode dialogNode in dialogGraph.nodes)
            {
                if (!dialogNode.GetInputPort("input").IsConnected)
                {
                    UpdateDialog(dialogNode, dialogUI, EndConversationCallback);
                    break;
                }
            }
        }

        public void UpdateDialog(DialogNode dialogNode, DialogUI dialogUI, Action EndConversationCallback)
        {
            currentNode = dialogNode;
            dialogUI.ShowResponses(currentNode.Answers, this, () =>
            {
                dialogUI.gameObject.SetActive(false);
                dialogUI.IsPlayerDialogate = false;
                EndConversationCallback?.Invoke();
            });
        }

        public void NotifyPlayerReadText()
        {
            isPlayerReadText = true;
        }

        public void SendResponse(int index)
        {
            responseIndex = index;
            isWaitingResponse = false;
        }

        public IEnumerator ConversationRoutine(DialogUI dialogUI, Action EndConversationCallback = null, Dialogable targetDialogable = null)
        {
            dialogUI.IsPlayerDialogate = true;
            Init(dialogUI, EndConversationCallback);
            while (true)
            {
                //NodePort auxPort;
                //auxPort = currentNode.GetPort("input");
                //auxPort = currentNode.GetPort("output");
                //DialogNode dialogNode = auxPort.Connection.node as DialogNode;

                string dialogText = currentNode.dialogText;

                /*if (auxPort.Connection.node as DialogNode)
                {
                    DialogNode dialogNode = auxPort.Connection.node as DialogNode;

                    if(currentNode.GetPort("output").IsConnected)
                    {
                        dialogText = dialogNode.dialogText;
                    }
                }*/
                
                //string dialogText = currentNode.GetInputPort("input");
                //Get actions inside texts
                if (currentNode.dialogText.Contains("<!"))
                {
                    string[] dialogParts = dialogText.Split(new string[] { "<!" }, StringSplitOptions.RemoveEmptyEntries);
                    string splitedDialog = dialogParts[1];
                    splitedDialog = splitedDialog.Split(new string[] { "!>" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (targetDialogable != null)
                        targetDialogable.CallbackEvents[int.Parse(splitedDialog)].Invoke();
                    dialogText = dialogText.Replace($"<!{splitedDialog}!>", "");
                    //TODO fix this later for more cases in this case if you use callback by dialog the dialog close inmediately
                    EndConversationCallback?.Invoke();
                    EndConversationCallback = null;
                    dialogUI.gameObject.SetActive(false);
                    dialogUI.IsPlayerDialogate = false;
                    break;
                }
                dialogUI.WriteText(dialogText, NotifyPlayerReadText, currentNode.Answers.Count > 0);
                while (true)
                {
                    if (isPlayerReadText)
                    {
                        isPlayerReadText = false;
                        break;
                    }
                    yield return null;
                }
                NodePort port;
                if (currentNode.Answers.Count < 1)                          //Take default node
                    port = currentNode.GetPort("output");
                else                                                        //Take answer node
                {
                    isWaitingResponse = true;
                    while (isWaitingResponse)
                        yield return null;
                    port = currentNode.GetPort("Answers " + responseIndex);
                }
                if (port != null && port.IsConnected)
                    UpdateDialog(port.Connection.node as DialogNode, dialogUI, EndConversationCallback);
                else
                    break;

                yield return null;
            }
            EndConversationCallback?.Invoke();
            dialogUI.gameObject.SetActive(false);
            dialogUI.IsPlayerDialogate = false;
        }
    }
}