using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Timba.Games.Recolor;
//using Timba.Database;
//using Timba.Patterns;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Timba.Characters
{
    public class PartVisual : MonoBehaviour
    {
        //public PartDefinition partDefinition;
        public event Action<bool> OnPowered;

        private static MaterialPropertyBlock m_propertyBlock;
        private static Material m_recolorMaterial;
        private Material m_imageMaterial;

        // public PartDefinition PartDefinition
        // {
        //     get => partDefinition;
        //     set
        //     {
        //         partDefinition = value;
        //         UpdateVisuals();
        //         //Debug.LogFormat("PartVisual {0} updated with PartDefinition {1}", name, partDefinition.definitionId);
        //     }
        // }
        Material newMaterial;
        private void OnEnable()
        {
            ColorSwapper.Instance.AddItemToPVL(this);
            ColorSwapper.Instance.ChangeMaterial();
        }
        private void OnDisable()
        {
            if (!isShuttingDown)
            {
                ColorSwapper.Instance.partVisuals.Remove(this);
            }
        }
        private void Awake()
        {
            if (m_propertyBlock == null)
            {
                m_propertyBlock = new MaterialPropertyBlock();
            }
            newMaterial = Resources.Load<Material>($"Materials/{this.name.Split('_').First()}");
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.material = newMaterial;

            RecolorPartSprites((m_propertyBlock) => this.GetComponent<Recolorable>().SetColors(ColorSwapper.Instance._colorsScriptableObject._palettes[0]._paletteColor, m_propertyBlock));
            //RecolorPartSprites((m_propertyBlock) =>
            //this.GetComponent<Timba.Recolor.Recolorable>().SetColors(ColorSwapper.Instance._colorsScriptableObject._palettes[0].PalleteColors[0]._paletteColor, m_propertyBlock));


            // if (m_recolorMaterial == null){
            //     m_recolorMaterial = ServiceLocator.Instance.GetService<DefinitionManager>().definitionDatabase.recolorMaterial;
            // }
        }

        private void OnDestroy()
        {
            if (m_imageMaterial)
            {
                Destroy(m_imageMaterial);
            }

            if (!isShuttingDown)
            {
                ColorSwapper.Instance.partVisuals.Remove(this);
            }
        }
        static bool isShuttingDown = false;
        void OnApplicationQuit()
        {
            isShuttingDown = true;
        }
        // [SerializeField]
        // private Color32[] colors;
        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         RecolorPartSprites((m_propertyBlock) => this.GetComponent<Timba.Recolor.Recolorable>().SetColors(colors, m_propertyBlock));
        //     }
        // }
        public void ChangeSpritePalette(Color32[] _colors)
        {
            RecolorPartSprites((m_propertyBlock) => this.GetComponent<Recolorable>().SetColors(_colors, m_propertyBlock));
        }
        // private void UpdateVisuals()
        // {
        //     foreach (var sr in GetComponentsInChildren<Component>())
        //     {
        //         bool isComponentRequired = sr is SpriteRenderer || sr is Image;
        //         if (!isComponentRequired) { continue; }

        //         //Debug.LogFormat("Updating {0}", sr.name);
        //         Transform newSpriteTransform = partDefinition.gameplayPrefab.transform.FindDeepChild(sr.name);
        //         //Debug.LogFormat("Definition transform found {0}", newSpriteTransform);

        //         if (sr is SpriteRenderer){
        //             (sr as SpriteRenderer).sprite = newSpriteTransform.GetComponent<SpriteRenderer>().sprite;
        //         }
        //         else{
        //             (sr as Image).sprite = newSpriteTransform.GetComponent<SpriteRenderer>().sprite;
        //         }
        //     }       
        // }

        public void RecolorPartSprites(Action<MaterialPropertyBlock> _recolorAction)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                sr.GetPropertyBlock(m_propertyBlock);
                _recolorAction?.Invoke(m_propertyBlock);
                sr.SetPropertyBlock(m_propertyBlock);
            }
        }

        public void RecolorPartImages(Action<Material> _recolorAction)
        {
            foreach (var image in GetComponentsInChildren<Image>())
            {
                if (m_imageMaterial == null)
                {
                    m_imageMaterial = Instantiate(m_recolorMaterial);
                }

                if (image.material != m_imageMaterial)
                {
                    image.material = m_imageMaterial;
                }
                _recolorAction?.Invoke(m_imageMaterial);
            }
        }

        /// <summary>
        /// Turns on/off a part. Parts are turned off for building and other no-gameplay situations.
        /// Current implementation tries to be generic. Might need to change in the future
        /// </summary>
        [ExecuteInEditMode]
        public void SetPowered(bool isPowered)
        {
            OnPowered?.Invoke(isPowered);
        }
    }
}