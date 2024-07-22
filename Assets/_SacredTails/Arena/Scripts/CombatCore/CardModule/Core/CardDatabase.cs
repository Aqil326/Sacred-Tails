using MyBox;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "CombatModule/CardDatabase")]
    public class CardDatabase : ScriptableObject
    {
        public List<ActionCard> actionCards = new List<ActionCard>();

        [Button("Generate JSON")]
        public void GetJsonActionCards()
        {
            foreach (var actionCard in actionCards)
            {
                List<string> data = new List<string>();
                foreach (var battleAction in actionCard.BattleActions)
                {
                    battleAction.actionElementType = actionCard.cardType;
                    data.Add(JsonConvert.SerializeObject(battleAction, Formatting.None, new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() }));
                }

                actionCard.BattleAction = data;
            }
            File.WriteAllText("Assets/_content/ServerData/CardDatabase.json", JsonConvert.SerializeObject(actionCards));
        }

        [Button()]
        public void SetVfxIndex()
        {

            for (int i = 3; i < actionCards.Count; i++)
            {
                actionCards[i].VfxIndex = i - 3;
            }
        }

        #region SearchBar

        [Header("---SearchBar---", order = 0)]
        [Header(">Name", order = 1)]
        public bool searchByName = false;
        [ConditionalField(nameof(searchByName), false, true)][SerializeField] private string nameOfCard;
        private string previousName;

        [Header(">Type")]
        public bool searchByType = false;
        [ConditionalField(nameof(searchByType), false, true)][SerializeField] private ActionTypeEnum typeOfAction;
        private ActionTypeEnum previousCardByType;

        [Header(">IndexOfCard")]
        public bool searchByIndex = false;
        [ConditionalField(nameof(searchByIndex), false, true)][SerializeField] private bool includeSpecialCards = false;
        [ConditionalField(nameof(searchByIndex), false, true)][SerializeField] private int indexOfCard;
        private int previousIndex;

        [Header(">>Result<<")]
        [SerializeField] private List<ActionCard> searchResult;
        [Header("---Helpers---")]
        public bool _;

        public void OnValidate()
        {
            List<ActionCard> currentSearchResult = actionCards;
            if (searchByName && nameOfCard != previousName)
            {
                searchResult = actionCards.Where(card => card.name.IndexOf(nameOfCard, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                previousName = nameOfCard;
                currentSearchResult = searchResult;
            }
            if (searchByType && typeOfAction != previousCardByType)
            {
                searchResult = currentSearchResult.Where(card => card.BattleActions.Exists(battleAction => battleAction.actionType == typeOfAction)).ToList();
                previousCardByType = typeOfAction;
                currentSearchResult = searchResult;
            }

            if (searchByIndex && indexOfCard != previousIndex)
            {
                int indexToSearch = includeSpecialCards ? indexOfCard : indexOfCard + 3;
                if (indexToSearch >= actionCards.Count)
                    return;
                searchResult = new List<ActionCard>();
                searchResult.Add(actionCards[indexToSearch]);
                previousIndex = indexOfCard;
            }
            if (!searchByIndex && !searchByType && !searchByName)
                searchResult = new List<ActionCard>();
        }
        #endregion SearchBar
    }
}