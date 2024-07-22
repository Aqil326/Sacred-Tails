using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    /// Scriptable object for configurate the constants of the game for the BattleStatisticsCalculator
    /// </summary>
    [CreateAssetMenu(fileName = "BattleStatisticsSO", menuName = "Timba/SacredTails/BattleStatisticsSO")]
    [System.Serializable]
    public class BattleStatisticsCalculatorSO : ScriptableObject
    {
        #region ---Fields---
        [Header("Attack")]
        public float attackMultiplicationValue = 10;
        public float attackDivisionValue = 50;

        [Header("Deffense")]
        public float deffenseDivisionValue = 100;
        public float deffenseSumValue = 1;

        [Header("Element type bonus")]
        public float elementTypeBonusDivison = 100;

        [Header("Evasion")]
        public float evasionThreshold1 = 30;
        public float evasionThreshold2 = 34;
        public float evasionThreshold3 = 38;
        public float evasionThreshold4 = 43;
        public float evasionThreshold5 = 46;
        #endregion ---Fields---

        [Button()]
        public void SaveChanges()
        {
            BattleStatisticsCalculator.m_attackMultiplicationValue = attackMultiplicationValue;
            BattleStatisticsCalculator.m_attackDivisionValue = attackDivisionValue;

            BattleStatisticsCalculator.m_deffenseSumValue = deffenseSumValue;
            BattleStatisticsCalculator.m_deffenseDivisionValue = deffenseDivisionValue;

            BattleStatisticsCalculator.m_elementTypeBonusDivisionValue = elementTypeBonusDivison;

            BattleStatisticsCalculator.m_evasionThreshold1 = evasionThreshold1;
            BattleStatisticsCalculator.m_evasionThreshold2 = evasionThreshold2;
            BattleStatisticsCalculator.m_evasionThreshold3 = evasionThreshold3;
            BattleStatisticsCalculator.m_evasionThreshold4 = evasionThreshold4;
            BattleStatisticsCalculator.m_evasionThreshold5 = evasionThreshold5;
        }

        private void OnValidate()
        {
            SaveChanges();
        }


        [Button("Generate JSON")]
        public void GetJsonActionCards()
        {
            File.WriteAllText("Assets/_content/ServerData/BattleStatisticsVariables.json", JsonConvert.SerializeObject(this));
        }
    }
}