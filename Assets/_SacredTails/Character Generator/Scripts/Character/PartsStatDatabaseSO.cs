using System;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.Games.CharacterFactory
{
    [CreateAssetMenu(fileName = "PartsStatDatabaseSO", menuName = "Timba/SacredTails/PartsStatDatabaseSO")]
    [Serializable]
    public class PartsStatDatabaseSO : ScriptableObject
    {
        public List<PartStat> partsStats;

        public int GetStat(PartType part, string sTier, string sType, CharacterType cType, StatValueType statValueType)
        {
            var pType = (CharacterType)Enum.Parse(typeof(CharacterType), sType);
            var tier = (RarityType)Enum.Parse(typeof(RarityType), sTier);

            int baseStatValue = 0;
            int partStatValue = 0;
            var charStat = partsStats.Find((x) => x.partType == cType);
            var partStat = partsStats.Find((x) => x.partType == pType);
            switch (part)
            {
                case PartType.Ears:
                    baseStatValue = CalculateStat(charStat, ShinseiStatsEnum.Attack, tier, true);
                    partStatValue = CalculateStat(partStat, ShinseiStatsEnum.Attack, tier);
                    break;
                case PartType.Accessory:
                    baseStatValue = CalculateStat(charStat, ShinseiStatsEnum.Defence, tier, true);
                    partStatValue = CalculateStat(partStat, ShinseiStatsEnum.Defence, tier);
                    break;
                case PartType.Body:
                    baseStatValue = CalculateStat(charStat, ShinseiStatsEnum.Stamina, tier, true);
                    partStatValue = CalculateStat(partStat, ShinseiStatsEnum.Stamina, tier);
                    break;
                case PartType.Head:
                    baseStatValue = CalculateStat(charStat, ShinseiStatsEnum.Speed, tier, true);
                    partStatValue = CalculateStat(partStat, ShinseiStatsEnum.Speed, tier);
                    break;
                case PartType.Tail:
                    baseStatValue = CalculateStat(charStat, ShinseiStatsEnum.Vigor, tier, true);
                    partStatValue = CalculateStat(partStat, ShinseiStatsEnum.Vigor, tier);
                    break;

            }
            if (statValueType.Equals(StatValueType.Base)) return baseStatValue;
            else if (statValueType.Equals(StatValueType.Part)) return partStatValue;
            else return baseStatValue;
        }

        public int GetStatBalanceValue(ShinseiStatsEnum protectedStat, CharacterType shinseiType, RarityType shinseiTier, int partStatValuesSummatory)
        {
            var charStat = partsStats.Find((x) => x.partType == shinseiType);
            int divisorBalanceStatValue = shinseiType.Equals(CharacterType.Celestial) ? 5 : 4;
            int deltaStatSummatory = 150 + (int)shinseiTier * 50 - partStatValuesSummatory;
            if (charStat.baseMultipliers.statPenalty.Equals(protectedStat) || shinseiType.Equals(CharacterType.Celestial))
            {
                return 0;
            }
            else
            {
                return Mathf.CeilToInt(deltaStatSummatory / divisorBalanceStatValue);
            }
        }
        public int CalculateStat(PartStat partStat, ShinseiStatsEnum bonus, RarityType tier, bool isMainElementStat = false)
        {
            float statBase = partStat.typeStatsAndMultipliers.globalPartStat;
            float finalStat = isMainElementStat ? statBase : 0;
            if (partStat.baseMultipliers.statBonus1 == bonus || partStat.partType.Equals(CharacterType.Celestial))
                finalStat += statBase * partStat.typeStatsAndMultipliers.elementBonusMultiplier1;
            else if (partStat.baseMultipliers.statBonus2 == bonus)
                finalStat += statBase * partStat.typeStatsAndMultipliers.elementBonusMultiplier1 / 2;
            if (isMainElementStat)
            {
                if (partStat.baseMultipliers.statPenalty == bonus && !partStat.partType.Equals(CharacterType.Celestial))
                    finalStat += statBase * -partStat.typeStatsAndMultipliers.elementPenaltyMultiplier;
            }
            else
            {
                switch (tier)
                {
                    case RarityType.Common:
                        finalStat += statBase * partStat.partMultipliers.commonPartMultiplier;
                        break;
                    case RarityType.Uncommon:
                        finalStat += statBase * partStat.partMultipliers.uncommonPartMultiplier;
                        break;
                    case RarityType.Rare:
                        finalStat += statBase * partStat.partMultipliers.rarePartMultiplier;
                        break;
                    case RarityType.Epic:
                        finalStat += statBase * partStat.partMultipliers.epicPartMultiplier;
                        break;
                    case RarityType.Legendary:
                        finalStat += statBase * partStat.partMultipliers.legendaryPartMultiplier;
                        break;
                    case RarityType.Legendary1:
                        finalStat += statBase * partStat.partMultipliers.commonPartMultiplier;
                        break;
                    case RarityType.Legendary2:
                        finalStat += statBase * partStat.partMultipliers.uncommonPartMultiplier;
                        break;
                    case RarityType.Legendary3:
                        finalStat += statBase * partStat.partMultipliers.rarePartMultiplier;
                        break;
                    case RarityType.Legendary4:
                        finalStat += statBase * partStat.partMultipliers.epicPartMultiplier;
                        break;
                    case RarityType.Legendary5:
                        finalStat += statBase * partStat.partMultipliers.legendaryPartMultiplier;
                        break;
                }
            }
            return Mathf.CeilToInt(finalStat);
        }
    }
}