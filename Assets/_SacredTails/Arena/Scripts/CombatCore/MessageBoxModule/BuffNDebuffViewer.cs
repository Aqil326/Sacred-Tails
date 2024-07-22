using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffNDebuffViewer : MonoBehaviour
{
    [SerializeField] private BuffDebuffUIItem buffDebuffUIItemPrefab;
    [SerializeField] private Sprite[] buffsIcons;

    private List<BuffDebuffUIItem> itemsOnView = new List<BuffDebuffUIItem>();
    
    public void UpdateTurns()
    {
        Debug.Log("buff Update turns called on "+transform.name);
        foreach (var item in itemsOnView)
        {
            item.turnsLeft--;
            item.buffTurnsLeftField.text = $"x{item.turnsLeft}";
            if(item.turnsLeft > 500)
            {
                item.buffTurnsLeftField.text = "Inf";
            }
            if(item.turnsLeft <= 0)
            {
                Destroy(item.gameObject);
            }
        }
        itemsOnView.RemoveAll(a => a.turnsLeft <= 0);
    }

    public void AddBuff(bool isBuff, int turnsLeft ,ShinseiStatsEnum buffType, int amount, bool isPercentage)
    {
        BuffDebuffUIItem newBuffDebuff = Instantiate(buffDebuffUIItemPrefab, transform);
        newBuffDebuff.gameObject.SetActive(true);
        newBuffDebuff.SetBuffOrDebuff(isBuff, buffType, turnsLeft, amount, isPercentage);
        newBuffDebuff.buffIcon.sprite = buffsIcons[(int)buffType];
        newBuffDebuff.turnsLeft = turnsLeft;
        newBuffDebuff.buffTurnsLeftField.text = $"x{turnsLeft}";
        if (turnsLeft > 500)
        {
            newBuffDebuff.buffTurnsLeftField.text = "Inf";
        }

        itemsOnView.Add(newBuffDebuff);
    }

    public void ClearAllBuffsViews()
    {
        foreach (var item in itemsOnView)
        {
            Destroy(item.gameObject);
        }
        itemsOnView.Clear();
    }
}
