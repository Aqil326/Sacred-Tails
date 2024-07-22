using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Timba.SacredTails.Arena;

namespace Timba.SacredTails.BattleDebugTool
{
    /// <summary>
    /// Show all necesary information for debug battle comparing data from local, and server allow to find errors in code
    /// </summary>
    public class DebugShinseiSlot : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI LifeValue, EnergyValue;
        [SerializeField] Image LifeBar, EnergyBar;
        [SerializeField] List<TextMeshProUGUI> StatsValues = new List<TextMeshProUGUI>();
        [SerializeField] GameObject reflect, evade;
        public Image selectedImage;

        public void ShowValues(ShinseiStats originalStats, Shinsei targetShinsei, Shinsei localShinsei)
        {

            LifeValue.text = targetShinsei.shinseiHealth.ToString();
            EnergyValue.text = targetShinsei.shinseiEnergy.ToString();
            if (targetShinsei.ShinseiOriginalStats.Health > 0)
                LifeBar.fillAmount = (float)targetShinsei.shinseiHealth / targetShinsei.ShinseiOriginalStats.Health;
            if (targetShinsei.ShinseiOriginalStats.Energy > 0)
                EnergyBar.fillAmount = (float)targetShinsei.shinseiEnergy / targetShinsei.ShinseiOriginalStats.Energy;
            bool isAttackDifferent = targetShinsei.ShinseiOriginalStats.attack != localShinsei.ShinseiOriginalStats.attack;
            StatsValues[0].text = $"ATK=>{(isAttackDifferent ? "<color=red>" : "")}OG:{originalStats.attack}, SV: {targetShinsei.ShinseiOriginalStats.attack} vs LCL: {localShinsei.ShinseiOriginalStats.attack}{(isAttackDifferent ? "</color>" : "")}";

            bool isDefenceDifferent = targetShinsei.ShinseiOriginalStats.defence != localShinsei.ShinseiOriginalStats.defence;
            StatsValues[1].text = $"DFN=>{(isDefenceDifferent ? "<color=red>" : "")}OG:{originalStats.defence}, SV: {targetShinsei.ShinseiOriginalStats.defence} vs LCL: {localShinsei.ShinseiOriginalStats.defence}{(isDefenceDifferent ? "</color>" : "")}";

            bool isSpeedDifferent = targetShinsei.ShinseiOriginalStats.speed != localShinsei.ShinseiOriginalStats.speed;
            StatsValues[2].text = $"SPD=>{(isSpeedDifferent ? "<color=red>" : "")}OG:{originalStats.speed}, SV: {targetShinsei.ShinseiOriginalStats.speed} vs LCL: {localShinsei.ShinseiOriginalStats.speed}{(isSpeedDifferent ? "</color>" : "")}";

            bool isStaminaDifferent = targetShinsei.ShinseiOriginalStats.stamina != localShinsei.ShinseiOriginalStats.stamina;
            StatsValues[3].text = $"STA=>{(isStaminaDifferent ? "<color=red>" : "")}OG:{originalStats.stamina}, SV: {targetShinsei.ShinseiOriginalStats.stamina} vs LCL: {localShinsei.ShinseiOriginalStats.stamina}{(isStaminaDifferent ? "</color>" : "")}";

            bool isVigorDifferent = targetShinsei.ShinseiOriginalStats.vigor != localShinsei.ShinseiOriginalStats.vigor;
            StatsValues[4].text = $"VGR=> {(isVigorDifferent ? "<color=red>" : "")}OG:{originalStats.vigor}, SV: {targetShinsei.ShinseiOriginalStats.vigor} vs LCL: {localShinsei.ShinseiOriginalStats.vigor}{(isVigorDifferent ? "</color>" : "")}";
            reflect.gameObject.SetActive(targetShinsei.reflectDamage > 0);
            evade.gameObject.SetActive(targetShinsei.evadeChance > 0);
            selectedImage.gameObject.SetActive(false);

        }

    }
}