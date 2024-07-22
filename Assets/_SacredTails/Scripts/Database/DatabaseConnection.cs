using UnityEngine;
using System.Threading.Tasks;
using PlayFab.MultiplayerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Timba.SacredTails.Arena;
using Timba.Games.CharacterFactory;
using Unity.Collections.LowLevel.Unsafe;

namespace Timba.SacredTails.Database
{
    public class DatabaseConnection : MonoBehaviour, IDatabase
    {
        [SerializeField] private PartIndex partIndex;
        [SerializeField] private PartsStatDatabaseSO patStatsDB;
        [SerializeField] private CardDatabase cardDatabase;

        private void Awake()
        {
            PlayfabManager.Singleton.OnMatchResultSuccess.AddListener(match => SacredTailsLog.LogMessage("$$ " + JsonConvert.SerializeObject(match)));
            PlayfabManager.Singleton.OnCreateTicketSuccess.AddListener(x => CheckTicketStatus(x, Constants.FRIENDLY_MATCH));
        }

        public ActionCard GetActionCardByIndex(int index)
        {
            return cardDatabase.actionCards[index];
        }

        public int CardDatabaseCount()
        {
            return cardDatabase.actionCards.Count;
        }

        public List<int> GetDatabaseCardsIndexListByType(List<string> partTypes)
        {
            List<int> typeSpecificPossibleCardList = cardDatabase.actionCards
                                                                    .Select((card, index) => (card, index))
                                                                    .Where(cardObject => partTypes.Contains(cardObject.card.cardType.ToString()) && cardObject.index > 3)
                                                                    .Select(cardObject => cardObject.index)
                                                                    .ToList<int>();
            return typeSpecificPossibleCardList;
        }

        public string GetRandomShinsei()
        {
            string selecteShinsei = partIndex.GenerateRandomShinsei();
            return selecteShinsei;
        }

        public string GetShinseiStructure(string dna)
        {
            string structure = partIndex.ParseShinseiDNA(dna, 3, 10);
            return structure;
        }
        public ShinseiStats GetShinseiStats(string shinseiDna)
        {
            var shinseiTypes = partIndex.GetShinseiPartTypes(shinseiDna, new CharacterType());
            var shinseiTiers = partIndex.GetShinseiPartTypes(shinseiDna, new RarityType());
            var mainType = partIndex.GetShinseiType(shinseiDna);
            var mainTier = ObtainShinseiRarity(shinseiDna);

            ShinseiStats stats = new ShinseiStats();
            ShinseiStats baseStats = new ShinseiStats();
            ShinseiStats partStats = new ShinseiStats();
            ShinseiStats balanceStats = new ShinseiStats();
            baseStats.Attack = patStatsDB.GetStat(PartType.Ears, shinseiTiers["ears"], shinseiTypes["ears"], mainType, StatValueType.Base);
            partStats.Attack = patStatsDB.GetStat(PartType.Ears, shinseiTiers["ears"], shinseiTypes["ears"], mainType, StatValueType.Part);
            baseStats.Defence = patStatsDB.GetStat(PartType.Accessory, shinseiTiers["accessory"], shinseiTypes["accessory"], mainType, StatValueType.Base);
            partStats.Defence = patStatsDB.GetStat(PartType.Accessory, shinseiTiers["accessory"], shinseiTypes["accessory"], mainType, StatValueType.Part);
            baseStats.Stamina = patStatsDB.GetStat(PartType.Body, shinseiTiers["body"], shinseiTypes["body"], mainType, StatValueType.Base);
            partStats.Stamina = patStatsDB.GetStat(PartType.Body, shinseiTiers["body"], shinseiTypes["body"], mainType, StatValueType.Part);
            baseStats.Speed = patStatsDB.GetStat(PartType.Head, shinseiTiers["head"], shinseiTypes["head"], mainType, StatValueType.Base);
            partStats.Speed = patStatsDB.GetStat(PartType.Head, shinseiTiers["head"], shinseiTypes["head"], mainType, StatValueType.Part);
            baseStats.Vigor = patStatsDB.GetStat(PartType.Tail, shinseiTiers["tail"], shinseiTypes["tail"], mainType, StatValueType.Base);
            partStats.Vigor = patStatsDB.GetStat(PartType.Tail, shinseiTiers["tail"], shinseiTypes["tail"], mainType, StatValueType.Part);

            int balancePartSummatory = Mathf.FloorToInt(partStats.attack + partStats.defence + partStats.stamina + partStats.speed + partStats.vigor);
            balanceStats.Attack = patStatsDB.GetStatBalanceValue(ShinseiStatsEnum.Attack, mainType, mainTier, balancePartSummatory);
            balanceStats.Defence = patStatsDB.GetStatBalanceValue(ShinseiStatsEnum.Defence, mainType, mainTier, balancePartSummatory);
            balanceStats.Stamina = patStatsDB.GetStatBalanceValue(ShinseiStatsEnum.Stamina, mainType, mainTier, balancePartSummatory);
            balanceStats.Speed = patStatsDB.GetStatBalanceValue(ShinseiStatsEnum.Speed, mainType, mainTier, balancePartSummatory);
            balanceStats.Vigor = patStatsDB.GetStatBalanceValue(ShinseiStatsEnum.Vigor, mainType, mainTier, balancePartSummatory);

            stats.Attack = Mathf.Max(0, baseStats.attack + partStats.attack + balanceStats.attack);
            stats.Defence = Mathf.Max(0, baseStats.defence + partStats.defence + balanceStats.defence);
            stats.Stamina = Mathf.Max(0, baseStats.stamina + partStats.stamina + balanceStats.stamina);
            stats.Speed = Mathf.Max(0, baseStats.speed + partStats.speed + balanceStats.speed);
            stats.Vigor = Mathf.Max(0, baseStats.vigor + partStats.vigor + balanceStats.vigor);

            float raritySum = 0;
            foreach (var kvp in shinseiTiers)
                raritySum += ((int)Enum.Parse(typeof(RarityType), kvp.Value)) < (int)RarityType.Legendary1? ((int)Enum.Parse(typeof(RarityType), kvp.Value)) + 1 : ((int)Enum.Parse(typeof(RarityType), kvp.Value)) - 4;

            stats.Health = Mathf.CeilToInt(228 + stats.vigor * 4);
            stats.Energy = Mathf.CeilToInt(20 + stats.stamina * 1.5f);
            //int finalStatSummatory = stats.Attack + stats.Deffense + stats.Stamina + stats.Speed + stats.Vigor;
            //----OLD LOGIC----
            /*

            var partTypes = partIndex.GetPartsDna(shinseiDna, 10);

            stats.Deffense = SetStat(partTypes["ears"]);
            stats.Attack = SetStat(partTypes["accessory"]);
            stats.Speed = SetStat(partTypes["body"]);
            stats.Health = SetStat(partTypes["head"]);
            stats.Luck = SetStat(partTypes["tail"]);

            //TODO
            stats.Energy = 30;*/
            return stats;
        }

        private int SetStat(long p_characterAPI)
        {
            if (p_characterAPI > 0)
                return CharacterUtils.GetRarityStat(CharacterUtils.ParseRarityDNA(p_characterAPI));
            else
                return 0;
        }

        private int SetFinalStat(string partType, string partTier, string part)
        {
            var rarityStat = 0;
            var typeStat = 0;
            var finalStat = 0;
            return finalStat;
        }

        public string GetSetName(string partRarity, string partType)
        {
            string key = partType + " " + partRarity;
            return partIndex.SetNames[key];
        }

        public Dictionary<string, string> GetShinseiPartsTypes(string shinseiDna, Enum genericEnum)
        {
            return partIndex.GetShinseiPartTypes(shinseiDna, genericEnum);
        }

        public CharacterType ObtainShinseiType(string dna)
        {
            return partIndex.GetShinseiType(dna);
        }

        public RarityType ObtainShinseiRarity(string dna)
        {
            return partIndex.GetShinseiRarity(dna);
        }

        /// <summary>
        /// starts the matchmaking sequence under the specified queue 
        /// </summary>
        public void StartMatchmakingSequence(int skillLevel, string gamemodeQueue)
        {

            PlayfabManager.Singleton.CreateMatchTicket(
                    PlayerDataManager.Singleton.localPlayerData.entityId,
                    PlayerDataManager.Singleton.localPlayerData.entityType,
                    skillLevel,
                    gamemodeQueue);


        }

        public async void CheckTicketStatus(CreateMatchmakingTicketResult ticket, string gamemodeQueue)
        {

            if (!string.IsNullOrEmpty(ticket.TicketId))
            {

                var matchmakingResult = new GetMatchmakingTicketResult();
                while (matchmakingResult.Status != "Matched" && matchmakingResult.Status != "Canceled" && Application.isPlaying)
                {
                    PlayfabManager.Singleton.GetTicketState(ticket.TicketId, gamemodeQueue, result => matchmakingResult = result);

                    await Task.Delay(6000);
                }



                if (matchmakingResult.Status == "Matched")
                {
                    SacredTailsLog.LogMessage("***" + JsonConvert.SerializeObject(matchmakingResult));
                    PlayfabManager.Singleton.GetMatch(matchmakingResult.MatchId, gamemodeQueue);
                }

            }
        }

        public bool IsReady()
        {
            return true;
        }

        public int GetActionIndex(ActionCard card)
        {
            int keyIndex = cardDatabase.actionCards.IndexOf(card);
            return keyIndex;
        }
    }
}

