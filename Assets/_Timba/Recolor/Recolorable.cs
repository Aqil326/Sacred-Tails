using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Timba.Games.Recolor
{
    public class Recolorable : MonoBehaviour
    {
        [SerializeField]
        private Color32[] colors;
        [SerializeField]
        private Material recolorMaterial;

        public void SetColors(Color32[] newColors)
        {
            recolorMaterial.SetColor("_NewColor1", newColors[0]);
            recolorMaterial.SetColor("_NewColor2", newColors[1]);
            recolorMaterial.SetColor("_NewColor3", newColors[2]);
            recolorMaterial.SetColor("_NewColor4", newColors[3]);
            recolorMaterial.SetColor("_NewColor5", newColors[4]);
            recolorMaterial.SetColor("_NewColor6", newColors[5]);
            /*foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                sr.material = recolorMaterial;
            }*/
        }

        public void SetColors(Color32[] newColors, MaterialPropertyBlock _materialProperty)
        {
            _materialProperty.SetColor("_NewColor1", newColors[0]);
            _materialProperty.SetColor("_NewColor2", newColors[1]);
            _materialProperty.SetColor("_NewColor3", newColors[2]);
            _materialProperty.SetColor("_NewColor4", newColors[3]);
            _materialProperty.SetColor("_NewColor5", newColors[4]);
            _materialProperty.SetColor("_NewColor6", newColors[5]);
        }

        public void SetColors(Color32[] newColors, Material _materialProperty)
        {
            _materialProperty.SetColor("_NewColor1", newColors[0]);
            _materialProperty.SetColor("_NewColor2", newColors[1]);
            _materialProperty.SetColor("_NewColor3", newColors[2]);
            _materialProperty.SetColor("_NewColor4", newColors[3]);
            _materialProperty.SetColor("_NewColor5", newColors[4]);
            _materialProperty.SetColor("_NewColor6", newColors[5]);
        }
    }

}
