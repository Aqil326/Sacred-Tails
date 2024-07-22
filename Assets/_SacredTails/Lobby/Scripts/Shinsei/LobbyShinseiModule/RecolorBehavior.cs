using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Timba.Games.SacredTails.LobbyNetworking;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.CharacterStyle;

public class RecolorBehavior : MonoBehaviour
{
    [SerializeField] private List<PartsOfCharacter> targetParts = new List<PartsOfCharacter>();
    public List<Color> possibleColors = new List<Color>();
    [SerializeField] private List<Transform> parents = new List<Transform>();
    [SerializeField] private List<TextMeshProUGUI> titles = new List<TextMeshProUGUI>();
    [SerializeField] private List<string> titleName = new List<string>();
    [SerializeField] private GameObject prefabColor;
    private ThirdPersonController currentPlayer;
    private void Start()
    {
        if (currentPlayer == null)
            currentPlayer = ServiceLocator.Instance.GetService<ILobbyNetworkManager>().CurrentPlayer;
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < parents.Count; i++)
        {
            titles[i].text = titleName[i];
            for (int a = 0; a < possibleColors.Count; a++)
            {
                Button button = Instantiate(prefabColor, parents[i]).GetComponent<Button>();
                button.gameObject.SetActive(true);
                button.GetComponent<Image>().color = possibleColors[a];

                int targetIndex = i;
                int targetColor = a;
                button.onClick.AddListener(() =>
                {
                    currentPlayer.GetComponent<CharacterRecolor>().ChangeMaterialColors(targetParts[targetIndex], possibleColors[targetColor]);
                    CharacterStyleController.UpdateColorPartOfCharacter(targetParts[targetIndex], possibleColors[targetColor], false);
                });
            }

        }
    }
}
