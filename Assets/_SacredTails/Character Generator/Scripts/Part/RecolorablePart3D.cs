using UnityEngine;
using Timba.Games.CharacterFactory;

namespace Timba.Recolor
{
    public class RecolorablePart3D : MonoBehaviour
    {
        //colors to be assigned
        [SerializeField] public Color32[] colors;

        //target material
        [SerializeField] private Material[] recolorMaterial;

        private void Awake()
        {
            SkinnedMeshRenderer skinnedMesh = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh)
                recolorMaterial = GetComponent<SkinnedMeshRenderer>().materials;
        }

        public void SetColors(string[] materialPropertyName, Color32[] newColors)
        {
            for (int i = 0; i < materialPropertyName.Length; i++)
                foreach (var mat in recolorMaterial)
                    mat.SetColor(materialPropertyName[i], newColors[i]);
        }

        static bool isShuttingDown = false;
        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }

        //this is used only in the cracter generator preview scene
        private void OnEnable()
        {
            if (!ColorSwapper3D.Instance.isCharacterViewScene)
                return;
            ColorSwapper3D.Instance.AddTo3DPartList(this);
            ColorSwapper3D.Instance.UpdatePartPallette();
        }

        private void OnDisable()
        {
            if (!isShuttingDown && !ColorSwapper3D.Instance.isCharacterViewScene)
                ColorSwapper3D.Instance.RemovePart(this);
        }

        private void OnDestroy()
        {
            if (!isShuttingDown && !ColorSwapper3D.Instance.isCharacterViewScene)
                ColorSwapper3D.Instance.RemovePart(this);
        }
    }
}

