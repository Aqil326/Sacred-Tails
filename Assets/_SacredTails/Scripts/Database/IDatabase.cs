using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Timba.Games.CharacterFactory;
using Timba.Patterns.ServiceLocator;
using Timba.SacredTails.Arena;
using UnityEngine;

/// <summary>
/// Data base interface for the remote connection with playfab mainly
/// </summary>
namespace Timba.SacredTails.Database
{
    public interface IDatabase : IService
    {
        public string GetRandomShinsei();
        public string GetShinseiStructure(string dna);
        public ShinseiStats GetShinseiStats(string shinseiDna);
        public ActionCard GetActionCardByIndex(int index);
        public int CardDatabaseCount();
        public int GetActionIndex(ActionCard card);
        public Dictionary<string, string> GetShinseiPartsTypes(string shinseiDna, Enum genericEnum);
        public List<int> GetDatabaseCardsIndexListByType(List<string> type);
        public CharacterType ObtainShinseiType(string dna);
        public RarityType ObtainShinseiRarity(string dna);
        void StartMatchmakingSequence(int skillLevel = 0, string gamemodeQueue = Constants.FRIENDLY_MATCH);
    }
}

