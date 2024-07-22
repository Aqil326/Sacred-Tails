using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    [System.Serializable]
    public class Combat
    {
        public MatchData MatchData;
        public List<ActionCard> Turns = new List<ActionCard>();
        public int CurrentTurn;
        public int CurrentShinsei;
    }
}