using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Timba.SacredTails.BattleDebugTool
{
    public class ChangeCardsTest : MonoBehaviour
    {
        public PlayerDataManager playerDataManager;

        public void ChangeTestCard0(string newIndex)
        {
            ChangeTestCardWithIndex(newIndex, 0);
        }

        public void ChangeTestCard1(string newIndex)
        {
            ChangeTestCardWithIndex(newIndex, 1);
        }

        public void ChangeTestCard2(string newIndex)
        {
            ChangeTestCardWithIndex(newIndex, 2);
        }

        public void ChangeTestCard3(string newIndex)
        {
            ChangeTestCardWithIndex(newIndex, 3);
        }

        private void ChangeTestCardWithIndex(string newIndex, int listIndex)
        {
            if (playerDataManager.cardToTest.Count > listIndex)
                playerDataManager.cardToTest[listIndex] = int.Parse(newIndex);
        }
    }
}