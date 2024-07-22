using UnityEngine;
using Timba.Recolor;

namespace Timba.Games.CharacterFactory
{
    /// <summary>
    /// Recives a string with the character's structure and Instantiates the respective mesh 
    /// </summary>
    public class CharacterBuilder : Utils.Singleton<CharacterBuilder>
    {

        [SerializeField] CharacterDatabase characterDatabase;
        private CharacterSlot currentCharacter;

        public GameObject InstantiateCharacter(int index, Transform parent, Vector3 position)
        {
            //TODO Check this xD
            currentCharacter = Instantiate(characterDatabase?.CharacterSlotPrefabsList[0], position, parent.rotation);
            currentCharacter.transform.SetParent(parent);
            if (currentCharacter == null)
            {
                SacredTailsLog.LogMessage("there is no character slot");
                return null;
            }
            return currentCharacter.gameObject;
        }

        public void UpdateVisual(string characterStructure, CharacterSlot targetCharacter)
        {
            var _partSlots = targetCharacter.PartSlots;
            var _characterString = GetCharacterStructure(characterStructure);
            EnablePartsByName(_characterString, _partSlots);
            SetPartsColor(_characterString[_characterString.Length - 1], _partSlots);
        }

        private void EnablePartsByName(string[] characterStructure, PartSlot[] partSlots)
        {

            foreach (var item in partSlots)
            {
                for (int i = 0; i < characterStructure.Length - 1; i++)
                {
                    item.ActiveBodyPartByName(characterStructure[i]);
                }
            }
        }

        private void SetPartsColor(string palettName, PartSlot[] partSlots)
        {
            int palettIndex = int.Parse(palettName.Split('_')[palettName.Split('_').Length - 1]);
            foreach (var item in partSlots)
            {
                ColorSwapper3D.Instance.AddTo3DPartList(item.GetComponentInChildren<RecolorablePart3D>());
            }
            ColorSwapper3D.Instance.AssignPallet(palettIndex);
        }

        private string[] GetCharacterStructure(string characterStructure)
        {
            string[] characterParts = characterStructure.Split(',');
            return characterParts;
        }

    }
}
