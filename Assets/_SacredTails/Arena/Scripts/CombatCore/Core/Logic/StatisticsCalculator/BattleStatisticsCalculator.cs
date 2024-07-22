using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// This class process all damage in the game apply the effect of statistics to final value, Attack, Deffence etc
    /// </summary>
    public static class BattleStatisticsCalculator
    {
        [Header("Health")]
        public static float healthMultiplier = 3.2f;

        [Header("Attack")]
        public static float m_attackMultiplicationValue = 10;
        public static float m_attackDivisionValue = 50;

        [Header("Deffense")]
        public static float m_deffenseDivisionValue = 100;
        public static float m_deffenseSumValue = 1;

        [Header("Element type bonus")]
        public static float m_elementTypeBonusDivisionValue = 100;

        [Header("Evasion")]
        public static float m_evasionThreshold1 = 30;
        public static float m_evasionThreshold2 = 34;
        public static float m_evasionThreshold3 = 38;
        public static float m_evasionThreshold4 = 43;
        public static float m_evasionThreshold5 = 46;

        public static float CalculateHealth(int healthStat)
        {
            return healthStat;
        }

        public static float GetRawDamage(float skillHitDamage, float shinseiAttackStat, float stab, float elementBonusMultiplier, float criticsMultiplier)
        {
            float division = shinseiAttackStat / m_attackDivisionValue;
            float multiplication = m_attackMultiplicationValue * division * stab * elementBonusMultiplier * criticsMultiplier;
            float sum = skillHitDamage + multiplication;
            SacredTailsLog.LogMessage("<color=green>Division</color>" + division);
            SacredTailsLog.LogMessage("<color=green>Multiplication</color>" + multiplication);
            SacredTailsLog.LogMessage("<color=green>Sum</color>" + sum);
            return sum;
        }

        public static float GetDamageReceiveByTarget(float deffenseStat, float rawDamage)
        {
            return rawDamage / (m_deffenseSumValue + deffenseStat / m_deffenseDivisionValue);
        }

        public static float ApplyEvationCritics(int multiplierStat)
        {
            if (multiplierStat > m_evasionThreshold5)
                return 1f;
            else if (multiplierStat > m_evasionThreshold4)
                return 0.8f;
            else if (multiplierStat > m_evasionThreshold3)
                return 0.6f;
            else if (multiplierStat > m_evasionThreshold2)
                return 0.5f;
            else if (multiplierStat > m_evasionThreshold1)
                return 0.3f;
            else
                return 0;
        }

        public static bool CheckIfEvade(int multiplierStat, float evadeRoll)
        {
            return evadeRoll <= ApplyEvationCritics(multiplierStat);
        }

        public static float GetBonusStat(Shinsei shinsei, ShinseiStatsEnum statBonus, float bonusPercentage)
        {
            float statAmount = GetStatByName(shinsei.ShinseiOriginalStats, statBonus);
            return statAmount * (bonusPercentage / m_elementTypeBonusDivisionValue);
        }

        public static (string, bool) SetStatByName(ref ShinseiStats shinseiStats, ShinseiStatsEnum statToModify, float amountToAdd, bool isPercentage, bool replaceStat = false, bool isEndAction = false)
        {
            string name = Enum.GetName(typeof(ShinseiStatsEnum), statToModify);

            var field = typeof(ShinseiStats).GetField(name);
            float fieldValue = GetStatByName(shinseiStats, statToModify);
            bool didApply = false;

            //TODO: When is on limits, save amount to add to later revert it.
            if (replaceStat)
            {
                field.SetValue(shinseiStats, amountToAdd);
                didApply = true;
            }
            else if (isPercentage)
            {
                float percentageBase = amountToAdd / 100;
                var valueOnBuffing = fieldValue + fieldValue * percentageBase;
                var valueOnReverting = fieldValue / (1 + percentageBase);
                if (amountToAdd > 0)
                {
                    float finalValue = isEndAction ? valueOnReverting : valueOnBuffing;
                    field.SetValue(shinseiStats, finalValue);
                    didApply = true;
                }
                else if (amountToAdd < 0)
                {
                    float finalValue = isEndAction ? valueOnReverting : valueOnBuffing;
                    field.SetValue(shinseiStats, finalValue);
                    didApply = true;
                }
                else
                    SacredTailsLog.LogErrorMessage("Fuk the buff went to narnia");
            }
            else
            {
                if (fieldValue + amountToAdd < 300 && fieldValue + amountToAdd > 0)
                    field.SetValue(shinseiStats, fieldValue + amountToAdd);
                didApply = true;
            }

            return (name, didApply);
        }
        public static float GetStatByName(ShinseiStats shinseiStats, ShinseiStatsEnum statToModify)
        {
            string name = Enum.GetName(typeof(ShinseiStatsEnum), statToModify);

            var field = typeof(ShinseiStats).GetField(name);
            float fieldValue = (float)field.GetValue(shinseiStats);

            return fieldValue;
        }
    }
}