using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    [System.Serializable]
    public class CombatPlayer
    {
        [System.NonSerialized] public string PlayfabId;
        public string DisplayName;
        public bool shinseisSelected;
        public bool hasSurrender;
        public bool confirmState;
        public int strikes;
        public Dictionary<int, int> forbidenActions = new Dictionary<int, int>();
        public List<Shinsei> ShinseiParty = new List<Shinsei>();
    }
}