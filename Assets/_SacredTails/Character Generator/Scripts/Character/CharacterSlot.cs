using UnityEngine;

namespace Timba.Games.CharacterFactory
{
    public class CharacterSlot : MonoBehaviour
    {
        #region ----Fields----
        public static new string FOLDER_NAME = "Gestures";

        public string characterCode;
        public string characterID;

        public Shinsei shinsei;
        public Animator animator;

        [SerializeField] private PartSlot[] partSlots;

        public PartSlot[] PartSlots => partSlots;
        #endregion ----Fields----

        #region ----Methods----
        public void Initialize()
        {
            animator = GetComponentInChildren<Animator>();
            var child = transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in child)
                if (item.name.Contains("Layer") || item.name.Contains("Capa"))
                    item.gameObject.SetActive(false);

            partSlots = GetComponentsInChildren<PartSlot>(true);

            // Pregunta si consiguio algo
            if (partSlots == null || partSlots.Length == 0)
            {
                Debug.LogWarning("No any body parts");
                return;
            }

            // Inicializa los part slot
            foreach (var item in partSlots)
            {
                item.Initialize();
                item.ActiveBodyPart(0);
            }
        }

        public void SetShinseiEvolution(bool isOn)
        {
            foreach (var part in partSlots)
                part.m_selectedChild.PutEvolution(isOn);
        }

        public void UpdateVisual()
        {
            if (partSlots.Length <= 0)
                Initialize();
            CharacterBuilder.Instance.UpdateVisual(characterCode, this);
        }

        public void SetCharacterCode(string code, bool isGenerator = false)
        {
            characterCode = code;

            if (animator == null || isGenerator)
                return;

            shinsei = PlayerDataManager.Singleton.localPlayerData?.ShinseiCompanion;
            //animator.SetFloat("Type", (int)shinsei.shinseiType);
            //animator.SetFloat("Rarity", (int)shinsei.shinseiRarity);
            animator.SetFloat("Type", 0);
            animator.SetFloat("Rarity", 0);
        }
        #endregion ----Methods----
    }
}