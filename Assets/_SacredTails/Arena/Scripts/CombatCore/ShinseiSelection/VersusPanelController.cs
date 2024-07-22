using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Timba.SacredTails.UiHelpers;
using Timba.Games.CharacterFactory;

public class VersusPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName, enemyName;
    [SerializeField] private List<Image> images = new List<Image>();
    [SerializeField] private List<Image> backgrounds = new List<Image>();
    [SerializeField] private BackgroundTypeSwapper backgroundTypeSwapper;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Init(List<Sprite> sprites,List<CharacterType> types, string playerName, string enemyName)
    {
        for (int i = 0; i < sprites.Count; i++)
            images[i].sprite = sprites[i];
        for (int i = 0; i < types.Count; i++)
            backgroundTypeSwapper.CallByShinseiType(backgrounds[i],types[i]);
        this.playerName.text = playerName;
        this.enemyName.text = enemyName;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
