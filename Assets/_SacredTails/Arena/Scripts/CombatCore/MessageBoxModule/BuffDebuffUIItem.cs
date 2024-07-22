using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDebuffUIItem : MonoBehaviour, ITooltipItem
{
    public TMP_Text buffTurnsLeftField;
    public Image buffIcon;
    public GameObject[] isBuffOrDebuffIcons;
    public Sprite[] buffFrameIcon;
    public Image buffFrame;
    

    private const string buffColor = "#27B774";
    private const string debuffColor = "#FE4040";

    //data
    [HideInInspector] public int turnsLeft;
    [HideInInspector] public bool isBuff;
    [HideInInspector] public ShinseiStatsEnum buffType;
    [HideInInspector] public int amount;
    [HideInInspector] public bool isPercentage;

    public void SetBuffOrDebuff(bool isBuff, ShinseiStatsEnum buffType, int turnsLeft, int amount, bool isPercentage)
    {
        this.isBuff = isBuff;
        this.turnsLeft = turnsLeft;
        this.buffType = buffType;
        this.isPercentage = isPercentage;
        this.amount = amount;

        isBuffOrDebuffIcons[0].SetActive(isBuff);
        isBuffOrDebuffIcons[1].SetActive(!isBuff);

        buffFrame.sprite = buffFrameIcon[isBuff ? 0 : 1];

        if(isBuff && ColorUtility.TryParseHtmlString(buffColor, out Color color))
        {
            buffTurnsLeftField.color = color;
            buffIcon.color = color;
        }
        else if(!isBuff && ColorUtility.TryParseHtmlString(debuffColor, out color))
        {
            buffTurnsLeftField.color = color;
            buffIcon.color = color;
        }
    }

    public string ReturnTooltipText()
    {
        string infinite = turnsLeft > 500 ? "" : "turns";
        return $"{buffType.ToString()} {(isBuff ? "+" : "-")}{amount}{(isPercentage ? "%" : "")} for {(turnsLeft > 500 ? "the rest of the combat" : turnsLeft.ToString())} {(turnsLeft == 1 ? "turn" : infinite)}";
    }
}
