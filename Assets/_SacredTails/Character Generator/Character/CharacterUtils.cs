using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.Games.CharacterFactory
{
    public class CharacterUtils : MonoBehaviour
    {
        private const int COMMONVALUE = 30;
        private const int RAREVALUE = 34;
        private const int EPICVALUE = 38;
        private const int LEGENDARYVALUE = 43;
        private const int MYTHICALVALUE = 46;

        public static string GeneratePartDNA(string partEntityName)
        {
            string Dna = "";

            string partID = GetTypeValues(new PartType(), partEntityName);
            string typeID = GetTypeValues(new CharacterType(), partEntityName);
            string rarityID = GetTypeValues(new RarityType(), partEntityName);

            if (partID != "" && typeID != "" && rarityID != "")
            {
                //"1" is assigned to the top of the string so the leading zeros of the enum values are preserved 
                Dna = "1" + typeID + rarityID + partID;
            }

            return Dna;
        }
        public static string GetTypeValues(Enum genericEnum, string partName)
        {
            var enumValues = Enum.GetValues(genericEnum.GetType());
            int partID;
            foreach (var t in enumValues)
            {
                string tName = t.ToString();
                if (partName.Contains(tName))
                {
                    partID = (int)t;
                    return partID.ToString("D3");
                }
            }

            return "";
        }
        public static string ParsePartDNA(long dna)
        {

            long reducer = 1000;
            long part = dna % reducer;

            dna /= reducer;
            long rarity = dna % reducer;
            dna /= reducer;
            long type = dna % reducer;
            dna /= reducer;

            string charType = Enum.GetName(typeof(CharacterType), type);
            string rarityType = Enum.GetName(typeof(RarityType), rarity);
            string partType = Enum.GetName(typeof(PartType), part);
            string partID = charType + "_" + rarityType + "_" + partType + "_" + "PartEntity";
            return partID;
        }

        public static string ParseRarityDNA(long dna)
        {
            long reducer = 1000;

            dna /= reducer;
            long rarity = dna % reducer;

            return Enum.GetName(typeof(RarityType), rarity);
        }


        public static int GetRarityStat(string raritydna)
        {
            int value = 0;
            if (!string.IsNullOrEmpty(raritydna))
            {
                switch (raritydna)
                {
                    case nameof(RarityType.Common):
                        value = COMMONVALUE;
                        break;
                    case nameof(RarityType.Uncommon):
                        value = RAREVALUE;
                        break;
                    case nameof(RarityType.Rare):
                        value = EPICVALUE;
                        break;
                    case nameof(RarityType.Epic):
                        value = LEGENDARYVALUE;
                        break;
                    case nameof(RarityType.Legendary):
                    case nameof(RarityType.Legendary1):
                    case nameof(RarityType.Legendary2):
                    case nameof(RarityType.Legendary3):
                    case nameof(RarityType.Legendary4):
                    case nameof(RarityType.Legendary5):
                        value = MYTHICALVALUE;
                        break;
                }
                return value;
            }
            else
                return value;

        }
    }
}