using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.CharacterStyle;
using UnityEngine;

public class BodyStyle : MonoBehaviour
{
    public List<BodyPartDressable> bodyParts = new List<BodyPartDressable>();

    [System.Serializable]
    public class BodyPartDressable
    {
        [SerializeField] string name;
        [SerializeField] PartsOfCharacter part;
        [SerializeField] List<GameObject> possibleParts = new List<GameObject>();
        [SerializeField] List<GameObject> possiblePartsMale = new List<GameObject>();

        public void SelectObject(int index, bool isLocal = false)
        {
            for (int i = 0; i < possibleParts.Count; i++)
                possibleParts[i].gameObject.SetActive(false);
            possibleParts[index].SetActive(true);
            for (int i = 0; i < possiblePartsMale.Count; i++)
                possiblePartsMale[i].gameObject.SetActive(false);
            possiblePartsMale[index].SetActive(true);
            if (isLocal)
                CharacterStyleController.UpdatePartOfCharacter(part,index);
        }
    }
}
