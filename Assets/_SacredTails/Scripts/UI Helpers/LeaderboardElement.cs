using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Timba.SacredTails.UiHelpers
{
    public class LeaderboardElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI victoriesText;
        [SerializeField] private TextMeshProUGUI pointsText;
        public Button button;

        private ElementData elementData;
        public ElementData ElementDataObject { get => elementData; }

        public void DrawElement(ElementData elementData)
        {
            this.elementData = elementData;
            positionText.text = elementData.position;
            nameText.text = elementData.name;
            pointsText.text = elementData.points;
        }

        public void SetRankedElementData(string name, string victories, string points, string position)
        {
            positionText.text = position;
            nameText.text = name;
            victoriesText.text = victories;
            pointsText.text = points;
        }

        public void ChangePosition(int value)
        {
            elementData.position = value.ToString();
            positionText.text = value.ToString();
        }
        public void ChangeVictories(string value)
        {
            elementData.victories = value;
            victoriesText.text = value;
        }

        public class ElementData
        {
            public string position;
            public string name;
            public string victories;
            public string points;
        }
    }
}