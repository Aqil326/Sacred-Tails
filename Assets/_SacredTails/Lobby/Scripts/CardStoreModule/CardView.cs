using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Timba.SacredTails.Arena;

namespace Timba.SacredTails.CardStoreModule
{
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        public ActionCard ActionCard
        {
            get => actionCard;
            set
            {
                actionCard = value;
                onActionCardChange?.Invoke(value);
            }
        }
        private ActionCard actionCard;
        Action<ActionCard> onActionCardChange;
        public Action<int, CardView> OnClickHandler;

        [SerializeField] int indexCard;
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI countText;

        private void Awake()
        {
            onActionCardChange += UpdateVisual;
        }

        private void UpdateVisual(ActionCard targetActionCard)
        {
            image.sprite = targetActionCard.cardImage;
        }

        public void UpdateNumber(int number, int indexCard)
        {
            countText.text = number.ToString();
            this.indexCard = indexCard;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickHandler.Invoke(indexCard, this);
        }
    }
}