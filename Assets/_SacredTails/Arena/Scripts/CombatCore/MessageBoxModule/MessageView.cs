using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MessageView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image icon;
    [PreviewField]
    public List<Sprite> possibleIcons = new List<Sprite>();
    [SerializeField] private Queue<Message> messageQueue = new Queue<Message>();
    [SerializeField] private CanvasGroup canvasGroup;
    
    public void ShowMessage(string message, int icon = -1, float duration = 2) //CAMTIME Old is 2 seg
    {
        Debug.Log("Message:: " + message + " - duration: " + duration);
        messageQueue.Enqueue(new Message() {icon = icon, text = message, time = duration});
    }

    private void Start()
    {
        StartCoroutine(ShowMessagesAfterTime());
    }

    IEnumerator ShowMessagesAfterTime()
    {
        while (true)
        {
            if (messageQueue.Count > 0)
            {
                canvasGroup.alpha = 1;
                Message message = messageQueue.Dequeue();
                messageText.transform.parent.gameObject.SetActive(true);
                messageText.text = message.text;
                if (message.icon >= 0)
                {
                    this.icon.transform.parent.gameObject.SetActive(true);
                    this.icon.sprite = possibleIcons[message.icon];
                }
                else
                    this.icon.transform.parent.gameObject.SetActive(false);
                yield return new WaitForSeconds(message.time);
            }
            yield return null;
            canvasGroup.alpha = 0;
        }
    }

    [System.Serializable]
    public class Message
    {
        public string text;
        public int icon;
        public float time;
    }
}
