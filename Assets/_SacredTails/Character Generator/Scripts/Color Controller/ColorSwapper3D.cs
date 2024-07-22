using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timba.Recolor;

namespace Timba.Games.CharacterFactory
{
    public class ColorSwapper3D : Utils.Singleton<ColorSwapper3D>{
        [Header("Color Palette Data")]
        [MMReadOnly] public string PaletteID;
        private int _palletIndex = 0;
        public ColorsSO _colorsScriptableObject;
        [SerializeField] public bool isCharacterViewScene = false;

        [Header("Model parts")]
        private List<RecolorablePart3D> modelParts = new List<RecolorablePart3D>();
        public SwapColorPropertyNameSO _materialColorPropertyName;

        private void Awake(){
            PaletteID = $"_PaletteID_{_palletIndex}";
        }

        public void AddTo3DPartList(RecolorablePart3D part){
            if (!modelParts.Contains(part)){
                modelParts.Add(part);
            }
        }

        public void RemovePart(RecolorablePart3D part){
            modelParts.Remove(part);
        }

        public void SwapPallette(){
            if (_colorsScriptableObject._palettes.Length == 0) return;
            _palletIndex = (_palletIndex + 1) % _colorsScriptableObject._palettes.Length;
            PaletteID = $"_PaletteID_{_palletIndex}";

            for (int i = 0; i < modelParts.Count; i++){
                modelParts[i].SetColors(_materialColorPropertyName._materialPropertyNames, _colorsScriptableObject._palettes[_palletIndex]._paletteColor);

            }
        }

        public void UpdatePartPallette(){
            if (_palletIndex >= 0){
                for (int i = 0; i < modelParts.Count; i++){
                    modelParts[i].SetColors(_materialColorPropertyName._materialPropertyNames, _colorsScriptableObject._palettes[_palletIndex]._paletteColor);
                }
            }
        }

        public void AssignPallet(int index){
            for (int i = 0; i < modelParts.Count; i++){
                modelParts[i].SetColors(_materialColorPropertyName._materialPropertyNames, _colorsScriptableObject._palettes[index]._paletteColor);
            }
            modelParts.Clear();
        }

        void Update(){
            if (Input.GetKeyDown(KeyCode.N) && isCharacterViewScene) SwapPallette();
        }
    }
}
  
