using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Timba.SacredTails.CharacterStyle
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "CharacterStyleDB", menuName = "Timba/SacredTails/CharacterStyleDB", order = 1)]
    public class CharacterStyleDatabase : ScriptableObject
    {
        #region ----Fields----
        [Header("Full DB")]
        public List<ColorIdRelation> colorDB;
        public List<PartIdRelation> partDB;

        [Header("SearchBar")]
        [SerializeField] private PartsOfCharacter searchPartByType;
        [Header("The Changes Made here will not affect the DB")]
        [SerializeField] private List<PartIdRelation> searchResult;
        private PartsOfCharacter previousPartType;
        #endregion ----Fields----

        #region ----Methods----
        #region Color
        public ColorIdRelation GetColorById(int id, PartsOfCharacter slotType)
        {
            return colorDB.Where(colorRelation => colorRelation.id == id && colorRelation.usableOnParts.Contains(slotType)).First();
        }

        public List<ColorIdRelation> GetColorsByPartType(PartsOfCharacter slotType)
        {
            return colorDB.Where(colorRelation => colorRelation.usableOnParts.Contains(slotType)).ToList();
        }
        #endregion Color

        #region Part
        public PartIdRelation GetPartById(int id, PartsOfCharacter slotType)
        {
            return partDB.Where(partRelation => partRelation.id == id && partRelation.slotType == slotType).First();
        }

        public List<PartIdRelation> GetPartsByType(PartsOfCharacter slotType)
        {
            return partDB.Where(partRelation => partRelation.slotType == slotType).ToList();
        }
        #endregion Part

        public void OnValidate()
        {
            if (searchPartByType != previousPartType)
            {
                previousPartType = searchPartByType;
                searchResult = partDB.Where(partRelation => partRelation.slotType == searchPartByType).ToList();
            }

            for (int i = 0; i < colorDB.Count; i++)
                colorDB[i].id = i;
        }
        #endregion ----Methods----
    }
}