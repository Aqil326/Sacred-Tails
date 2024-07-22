using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System;

public class AlteredView : MonoBehaviour
{
    [SerializeField] private GameObject prefabAlteredState;
    [SerializeField] private Transform parentPrincipal; //2 parents for hexagonal grid layout
    [SerializeField] private Transform parentAuxiliar;
    List<AlteredByTurns> alteredViews = new List<AlteredByTurns>();

    Dictionary<int, string> alteredStateColors = new Dictionary<int, string>(){
        {0, "#DE7D00"},
        {1, "#FD413F"},
        {2, "#3B8E27"},
        {3, "#6B0000" }
    };

    Dictionary<AlteredStateEnum, string> alteredStateTooltipMessage = new Dictionary<AlteredStateEnum, string>()
    {
        { AlteredStateEnum.Bleeding, "Receive damage each turn for {0} turns"},
        { AlteredStateEnum.Rooted, "Receive damage each turn for {0} turns"},
        { AlteredStateEnum.Ignited, "Receive damage each turn for {0} turns"},
        { AlteredStateEnum.EvasionChange, "Evasion rate increased for {0} turns"}
    };

    public void ShowAlteredByTime(AlteredStateEnum alteredType,int turnCount, Sprite icon)
    {
        var alteredFound = alteredViews.Find(x => x.alteredState == alteredType);
        if (alteredFound != null)
        {
            alteredFound.turns = turnCount;
            UpdateAlteredVisual(alteredFound.alteredObject, $"<color={alteredStateColors[(int)alteredType]}>x {turnCount}</color>", icon);
            alteredFound.alteredTooltip.tooltipText = String.Format(alteredStateTooltipMessage[alteredFound.alteredState], alteredFound.turns);
        }
        else
        {
            SacredTailsLog.LogMessage("hello my friends");
            Debug.Log("New altered state " + alteredType);
            GameObject altered = Instantiate(prefabAlteredState, parentPrincipal);
            altered.SetActive(true);
            var newAltered = new AlteredByTurns() { alteredState = alteredType, alteredObject = altered, turns = turnCount, alteredTooltip = altered.GetComponentInChildren<AlteredTooltip>() };
            newAltered.alteredTooltip.tooltipText = String.Format(alteredStateTooltipMessage[alteredType], turnCount);
            alteredViews.Add(newAltered);

            UpdateAlteredVisual(altered, $"<color={alteredStateColors[(int)alteredType]}>x {turnCount}</color>", icon);
        }
    }

    public void UpdateAlteredVisual(GameObject target,string text, Sprite icon = null)
    {
        if(icon != null)
            target.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = icon;
        target.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = text;

        int pairIndex = 2;
       foreach (var alteredView in alteredViews)
        {
            if (alteredView.turns <= 0)
            {
                Destroy(alteredView.alteredObject);
                continue;
            }

            if (pairIndex % 2 == 0)
                alteredView.alteredObject.transform.SetParent(parentPrincipal);
            else
                alteredView.alteredObject.transform.SetParent(parentAuxiliar);

            pairIndex++;
        }
        alteredViews.RemoveAll(a => a.turns <= 0);
    }


    public void PassTurn()
    {
        foreach (var alteredView in alteredViews)
        {
            alteredView.turns--;
            UpdateAlteredVisual(alteredView.alteredObject,$"x {alteredView.turns}");
            
            if (alteredView.turns <= 0)
            {
                Destroy(alteredView.alteredObject);
            }
        }
        alteredViews.RemoveAll(a => a.turns <= 0);
    }

    public void RemoveAllAlteredStates()
    {
        foreach (var alteredView in alteredViews)
        {
            Destroy(alteredView.alteredObject);           
        }
        alteredViews.Clear();
    }

    [System.Serializable]
    public class AlteredByTurns
    {
        public AlteredStateEnum alteredState;
        public GameObject alteredObject;
        public AlteredTooltip alteredTooltip;
        public int turns = 0;
    }
}
