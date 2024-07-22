using System.Collections;
using System.Collections.Generic;
using Timba.Games.CharacterFactory;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    /// <summary>
    ///     Turn describes the minimun necesary data to calculate battles in the game
    /// </summary>
    [System.Serializable]
    public class Turn
    {
        public int indexCard = 0;
        public List<BattleActionData> BattleActions = new List<BattleActionData>();
        public int ppCost = 0;
        public CharacterType turnActionType;
        // Current status - Envenenado etc etc 
        // Terrain changes - Aumentan el daño o lo disminuye, o la velocidad...
        // Party and stats de cada perro {100, 50, 100 , (1)}   {100, 0, 100 , (0)}
        // Shinsei, Attack... Deffense... Lucky ... Speed.... Health
    }
}