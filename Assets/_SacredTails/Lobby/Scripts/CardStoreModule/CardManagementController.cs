using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.SacredTails.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.UiHelpers;
using Timba.SacredTails.Arena;
using Timba.Games.CharacterFactory;
using TMPro;
using System;

namespace Timba.SacredTails.CardStoreModule
{
    public class CardManagementController : MonoBehaviour
    {
        [SerializeField] GameObject prefabCard;
        [SerializeField] Transform prefabParent;
        public BackgroundTypeSwapper backgroundTypeSwapper;
        List<CardPreview> cardViews = new List<CardPreview>();

        [SerializeField] GameObject detailShinseiView, isChanginCardPanel;

        [SerializeField] Shinsei selectedShinsei;

        [SerializeField] Image currentShinseiImage;
        [SerializeField] List<CardPreview> currentAttachedCards;

        [SerializeField] List<Image> ShinseiMiniatures, BGimages;

        [SerializeField] int TargetCard = 0;
        [SerializeField] private bool showCardsByTypeOfShinsei = true;

        [SerializeField] bool IsCardManagement;
        [SerializeField] SeekMouseIntoScreen seekMouseIntoScreen;
        [SerializeField] CardPreview targetCardPreview;
        [SerializeField] CardFilterHandler cardFilterHandler;

        private void OnEnable()
        {
            selectedShinsei = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion;

            SetDetailCardsTooltip(true);
            InitCardManager();
        }

        private void OnDisable()
        {
            SetDetailCardsTooltip(false);
        }

        private void SetDetailCardsTooltip(bool enable)
        {
            for (int i = 0; i < selectedShinsei.ShinseiActionsIndex.Count; i++)
            {
                var currentCard = currentAttachedCards[i];
                if (enable)
                {
                    currentCard.activatedTooltip = true;
                    currentCard.onPointerEnter += () =>
                    {
                        if (currentCard.cardIndex != 0)
                        {
                            seekMouseIntoScreen.Enable(true);
                            targetCardPreview.CopyCardIndex(currentCard);
                        }
                    };
                    currentCard.onPointerExit += () => { seekMouseIntoScreen.Enable(false); };
                }
                else
                {
                    currentCard.activatedTooltip = false;
                    currentCard.onPointerEnter -= () =>
                    {
                        if (currentCard.cardIndex != 0)
                        {
                            seekMouseIntoScreen.Enable(true);
                            targetCardPreview.CopyCardIndex(currentCard);
                        }
                    };
                    currentCard.onPointerExit -= () => { seekMouseIntoScreen.Enable(false); };
                }
            }
        }

        public void InitCardManager()
        {
            var shinseiTypes = ServiceLocator.Instance.GetService<IDatabase>().GetShinseiPartsTypes(selectedShinsei.ShinseiDna, new CharacterType());
            var playerDeck = PlayerDataManager.Singleton.localPlayerData.Deck.cards;
            List<CardPreview> cardListToFilter = new List<CardPreview>();
            for (int i = 0; i < cardViews.Count; i++)
                cardViews[i].gameObject.SetActive(false);
            for (int i = 0; i < playerDeck.Count; i++)
            {
                CardPreview cardViewTemporal;
                if (cardViews.Count - 1 < i)
                {
                    
                    cardViewTemporal = Instantiate(prefabCard, prefabParent).GetComponent<CardPreview>();
                    //cardViewTemporal.SetToggable();
                    if (IsCardManagement)
                    {
                        EventTrigger targetEventTrigger = cardViewTemporal.GetComponent<EventTrigger>();

                        cardViewTemporal.onPointerEnter += () =>
                        {
                            if (cardViewTemporal.cardIndex != 0)
                            {
                                seekMouseIntoScreen.Enable(true);
                                targetCardPreview.CopyCardIndex(cardViewTemporal);
                            }
                        };
                        cardViewTemporal.onPointerExit += () => { seekMouseIntoScreen.Enable(false); };
                    }
                    cardViewTemporal.OnClickHandler += AddSelectedCard;
                    cardViews.Add(cardViewTemporal);
                }

                cardViews[i].gameObject.SetActive(true);
                var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(playerDeck[i].index);
                //Displays cards only suitable for shinsei types
                if (!shinseiTypes.ContainsValue(card.cardType.ToString()) && showCardsByTypeOfShinsei)
                    cardViews[i].gameObject.SetActive(false);
                else
                    cardListToFilter.Add(cardViews[i]);

                cardViews[i].cardIndex = playerDeck[i].index;
                cardViews[i].cardCount = playerDeck[i].count;
                cardViews[i].Init(card, playerDeck[i].index);
            }
            List<Shinsei> currentPlayerShinseiParty = new List<Shinsei> { PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion };
            currentPlayerShinseiParty.AddRange(PlayerDataManager.Singleton.localPlayerData.ShinseiParty);
            for (int i = 0; i < currentPlayerShinseiParty.Count; i++)
            {
                ShinseiMiniatures[i].sprite = currentPlayerShinseiParty[i].shinseiIcon;
                backgroundTypeSwapper.CallByShinseiType(BGimages[i], currentPlayerShinseiParty[i].shinseiType);
            }
            UpdateDetailViewVisual();
            FilterCardsView(cardListToFilter);
        }

        public void FilterCardsView(List<CardPreview> cardsList)
        {
            cardFilterHandler.FilterCardsView(cardsList);
        }

        public void AddSelectedCard(int index)
        {
            if (selectedShinsei.ShinseiActionsIndex.Count < currentAttachedCards.Count && !selectedShinsei.ShinseiActionsIndex.Contains(index))
            {
                RemoveCardInDeck(index);            
                selectedShinsei.ShinseiActionsIndex.Add(index);
                InitCardManager();
                UpdateDetailViewVisual();
                PlayerDataManager.Singleton.UpdatePlayerData();           
            }
        }

        public void RemoveSelectedCard(int index)
        {
            if (index >= 0 && index < selectedShinsei.ShinseiActionsIndex.Count)
            {
                var cardIndex = selectedShinsei.ShinseiActionsIndex[index];
                AddCardInDeck(cardIndex);
                if(index != selectedShinsei.ShinseiActionsIndex.Count - 1)
                {
                    currentAttachedCards[selectedShinsei.ShinseiActionsIndex.Count - 1].cardIndex = 0;
                }
                else
                {
                    currentAttachedCards[index].cardIndex = 0;
                }
                selectedShinsei.ShinseiActionsIndex.RemoveAt(index);
                UpdateDetailViewVisual();
                InitCardManager();
            }
        }

        public void SelectTargetCard(int index)
        {
            TargetCard = index;
            isChanginCardPanel.SetActive(true);
        }

        public void DeselectTargetCard()
        {
            if (TargetCard == 0)
                return;
            TargetCard = 0;
            isChanginCardPanel.SetActive(false);
        }

        public void ChangeTargetShinseiCard(int index)
        {
            if (!selectedShinsei.ShinseiActionsIndex.Contains(index))
            {
                Debug.Log("Change card");
                //Store curren cart
                int oldIndex = selectedShinsei.ShinseiActionsIndex[TargetCard];
                //Change shinsei card
                selectedShinsei.ShinseiActionsIndex[TargetCard] = index;
                //Update visual of clicked card
                AddCardInDeck(oldIndex);
                RemoveCardInDeck(index);
                //Update miniatures of cards
                InitCardManager();
                UpdateDetailViewVisual();
                //Save changes in playfab
                PlayerDataManager.Singleton.UpdatePlayerData();
                TargetCard = 0;
            }
        }

        public void AddCardInDeck(int indexCardToAdd)
        {
            List<Card> cardsInDeck = PlayerDataManager.Singleton.localPlayerData.Deck.cards;
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                if (cardsInDeck[i].index == indexCardToAdd)
                {
                    cardsInDeck[i].count += 1;
                    PlayerDataManager.Singleton.localPlayerData.Deck.cards = cardsInDeck;
                    return;
                }
            }
            var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(indexCardToAdd);
            cardsInDeck.Add(new Card() { index = indexCardToAdd, count = 1, cardName = card.name });
            cardsInDeck = cardsInDeck.OrderBy(card => card.cardName).ToList();
            PlayerDataManager.Singleton.localPlayerData.Deck.cards = cardsInDeck;
        }

        public void RemoveCardInDeck(int indexCardToAdd)
        {
            List<Card> cardsInDeck = PlayerDataManager.Singleton.localPlayerData.Deck.cards;
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                if (cardsInDeck[i].index == indexCardToAdd)
                {
                    cardsInDeck[i].count -= 1;
                    if (cardsInDeck[i].count == 0)
                        cardsInDeck.RemoveAt(i);
                    PlayerDataManager.Singleton.localPlayerData.Deck.cards = cardsInDeck;
                    return;
                }
            }
            SacredTailsLog.LogMessage("you tryed to remove a card that dont exist");
        }

        public void SetCurrentChangeShinsei(int index)
        {
            if (index == 0)
            {
                selectedShinsei = PlayerDataManager.Singleton.localPlayerData.ShinseiCompanion;
            }
            else
            {
                selectedShinsei = PlayerDataManager.Singleton.localPlayerData.ShinseiParty[index - 1];
            }
            UpdateDetailViewVisual();
            InitCardManager();
            detailShinseiView.SetActive(true);
        }

        [SerializeField] ShinseiPreviewPanelManager shinseiPreviewPanelManager;

        public void UpdateDetailViewVisual()
        {
            currentShinseiImage.sprite = selectedShinsei.shinseiIcon;
            shinseiPreviewPanelManager.DisplayPreview(selectedShinsei, isCardManagement: true);
            Debug.Log("DisplayPreview 05");
            backgroundTypeSwapper.CallByShinseiType(shinseiPreviewPanelManager.shinseBackground, selectedShinsei.shinseiType);
            for (int i = 0; i < selectedShinsei.ShinseiActionsIndex.Count; i++)
            {
                var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(selectedShinsei.ShinseiActionsIndex[i]);
                currentAttachedCards[i].SetToggable();
                currentAttachedCards[i].Init(card, selectedShinsei.ShinseiActionsIndex[i]);
                //currentAttachedCards[i].sprite = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(selectedShinsei.ShinseiActionsIndex[i]).cardImage;

            }
            if (selectedShinsei.ShinseiActionsIndex.Count < currentAttachedCards.Count)
            {
                for (int i = selectedShinsei.ShinseiActionsIndex.Count; i < currentAttachedCards.Count; i++)
                    currentAttachedCards[i].cardContainer.SetActive(false);
            }
        }
    }
}