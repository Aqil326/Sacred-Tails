using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.SacredTails.Arena
{
    [System.Serializable]
    public class ShinseiStats
    {
        // OrganicStats
        public float Attack, Defence, Speed, Stamina, Vigor, Level;

        //public float attack { get => GetRealStat(Attack); }
        //public float defence { get => GetRealStat(Defence); }
        //public float speed { get => GetRealStat(Speed); }
        //public float stamina { get => GetRealStat(Stamina); }
        //public float vigor { get => GetRealStat(Vigor);  }

        public float attack
        {

            get => GetRealStat(Attack);

            set => Attack = value;

        }

        public float defence
        {

            get => GetRealStat(Defence);

            set => Defence = value;

        }

        public float speed 
        { 
            
            get => GetRealStat(Speed);

            set => Speed = value;
        
        }

        public float stamina
        {

            get => GetRealStat(Stamina);

            set => Stamina = value;

        }
        public float vigor
        {

            get => GetRealStat(Vigor);

            set => Vigor = value;

        }


        public float level
        {

            get => GetRealStat(Level);

            set => Level = value;

        }



        public float GetRealStat(float stat)
        {
            if (stat > 300) return 300;
            else if (stat < 0) return 0;
            else return stat;
        }
        // Compose Stats
        public int Health, Energy;
    }
}