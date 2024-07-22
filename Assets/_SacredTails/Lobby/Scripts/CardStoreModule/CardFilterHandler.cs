using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using Timba.SacredTails.Database;
using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// Handles the filter for searching skill cards in the player inventory
/// </summary>
public class CardFilterHandler : MonoBehaviour
{
    private CharacterType selectedType = CharacterType.NotSelected;

    [Header("Filter data setup")]
    [SerializeField] private TMP_InputField filterByNameField;
    [SerializeField] private TMP_InputField filterByPPCostField;
    [SerializeField] private TMP_Dropdown typeDropdownField;
    [SerializeField] private TMP_Dropdown orderFilterField;

    private bool isOnSearchField = false;

    List<CardPreview> actualCardDeck;

    private void Update()
    {
        if(isOnSearchField && Input.GetKeyDown(KeyCode.Return) && actualCardDeck != null)
        {
            FilterCardsView(actualCardDeck);
        }
            
    }

    /// <summary>
    /// Filters the cards based on the actual parameters (name, type, pp cost)
    /// </summary>
    /// <param name="cardViews">The cards preview list for the filter (usually passed by the CardManagementController)</param>
    public void FilterCardsView(List<CardPreview> cardViews)
    {
        actualCardDeck = cardViews;

        selectedType = typeDropdownField.value == 0 ? CharacterType.NotSelected : (CharacterType)(typeDropdownField.value - 1);

        string filterText = filterByNameField.text.ToLowerInvariant();
        bool hasFilterByName = !string.IsNullOrEmpty(filterText);
        bool hasFilterByType = selectedType != CharacterType.NotSelected;
        bool hasPPFilter = !string.IsNullOrEmpty(filterByPPCostField.text);

        HashSet<CardPreview> filteredCards = new HashSet<CardPreview>();

        foreach (var cardView in cardViews)
        {
            bool passFilter = true;

            if (hasFilterByName)
            {
                if (!cardView.cardName.text.ToLowerInvariant().Contains(filterText))
                    passFilter = false;
            }

            if (hasFilterByType && passFilter)
            {
                var card = ServiceLocator.Instance.GetService<IDatabase>().GetActionCardByIndex(cardView.cardIndex);
                if (card.cardType != selectedType)
                    passFilter = false;
            }

            if (hasPPFilter && passFilter)
            {
                if (cardView.ppCost > int.Parse(filterByPPCostField.text))
                    passFilter = false;
            }

            cardView.gameObject.SetActive(passFilter);

            if (passFilter)
                filteredCards.Add(cardView);
        }

        //Order the cards

        int childrenIndex = 0;
        switch (orderFilterField.value)
        {
            case 0:
                foreach (var cardView in filteredCards.OrderBy(card => card.cardName.text))
                {
                    cardView.transform.SetSiblingIndex(childrenIndex);
                    childrenIndex++;
                }
                break;

            case 1:
                foreach (var cardView in filteredCards.OrderBy(card => card.ppCost))
                {
                    cardView.transform.SetSiblingIndex(childrenIndex);
                    childrenIndex++;
                }
                break;

            case 2:
                foreach (var cardView in filteredCards.OrderBy(card => -card.ppCost))
                {
                    cardView.transform.SetSiblingIndex(childrenIndex);
                    childrenIndex++;
                }
                break;
        }
    }

    public void ClearFilters()
    {
        filterByNameField.text = string.Empty;
        filterByPPCostField.text = string.Empty;
        typeDropdownField.value = 0;
        orderFilterField.value = 0;

        if(actualCardDeck != null)
        {
            FilterCardsView(actualCardDeck);
        }
    }

    public void ChangeFilterSubmitStatus(bool newState)
    {
        isOnSearchField = newState;
    }
}
