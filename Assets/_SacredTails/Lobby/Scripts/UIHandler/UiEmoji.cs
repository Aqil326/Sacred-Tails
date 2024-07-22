using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Timba.SacredTails.ChatModule;

//List.Distinc()

public class UiEmoji : MonoBehaviour
{
    [SerializeField] private GameObject prefabBtnEmoji;
    [SerializeField] private Transform emojiParent;
    [SerializeField] private ChatEmojis chatEmojis;
    [SerializeField] private List<string> posibleStrings;

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        posibleStrings = chatEmojis.diccionaryOfEmojis.RealValues.Distinct().ToList();
        foreach (var item in posibleStrings)
        {
            if (item == "<sprite=39>" || item == "<sprite=40>")
                continue;
            GameObject instancedButton = Instantiate(prefabBtnEmoji, emojiParent);
            EmojiButton emojiButton = instancedButton.GetComponent<EmojiButton>();
            int targetIndex = chatEmojis.diccionaryOfEmojis.RealValues.IndexOf(item);
            emojiButton.realValue = item;
            emojiButton.codeValue = chatEmojis.diccionaryOfEmojis.keys[targetIndex];
            emojiButton.icon.sprite = chatEmojis.diccionaryOfEmojis.visual[targetIndex];
            emojiButton.button.onClick.AddListener(() => {
                StartCoroutine(WriteRoutine(emojiButton));
                //chatEmojis.pendingCarets += emojiButton.realValue.Length;
            });
            instancedButton.SetActive(true);
        }

    }

    public void ResetLastEmoji()
    {
        //LastEmoji = 0;
    }

    //public int LastEmoji = 0;

    IEnumerator WriteRoutine(EmojiButton emojiButton)
    {
        yield return new WaitForEndOfFrame();
        chatEmojis.inputField.text = chatEmojis.inputField.text.Insert(chatEmojis.inputField.stringPosition, emojiButton.codeValue);
        //chatEmojis.pendingCarets++;
        chatEmojis.inputField.Select();
        //LastEmoji += emojiButton.realValue.Length;
    }
    public void ToggleShow()
    {
        Show(!gameObject.activeSelf);
    }

    public void Show(bool state)
    {
        gameObject.SetActive(state);
    }
}
