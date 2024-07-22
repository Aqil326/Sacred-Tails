using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorSwapper : Utils.Singleton<ColorSwapper>
{
    [MMReadOnly] public string PaletteID;
    private int _palletIndex = 0;
    public ColorsSO _colorsScriptableObject;

    [SerializeField] private KeyCode swapKey = KeyCode.N;

    public List<Timba.Characters.PartVisual> partVisuals = new List<Timba.Characters.PartVisual>();
    public List<Material> _partMaterials = new List<Material>();
    [Header("Editor Only")]
    public bool _dontTakePhoto;
    void Awake()
    {
        PaletteID = $"_PaletteID_{_palletIndex}";
    }
    public void AddItemToPVL(Timba.Characters.PartVisual PV)
    {
        if (!partVisuals.Contains(PV))
        {
            partVisuals.Add(PV);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(swapKey)) SwapPalette();
    }

    private void SwapPalette()
    {
        if (_colorsScriptableObject._palettes.Length == 0) return;

        _palletIndex = (_palletIndex + 1) % _colorsScriptableObject._palettes.Length;
        PaletteID = $"_PaletteID_{_palletIndex}";
        for (int i = 0; i < partVisuals.Count; i++)
        {
            partVisuals[i].ChangeSpritePalette(_colorsScriptableObject._palettes[_palletIndex]._paletteColor);
        }
    }
    public void ChangeMaterial()
    {
        if (_palletIndex >= 0)
        {
            for (int i = 0; i < partVisuals.Count; i++)
            {
                partVisuals[i].ChangeSpritePalette(_colorsScriptableObject._palettes[_palletIndex]._paletteColor);
                //partVisuals[i].ChangeSpritePalette(_colorsScriptableObject._palettes[_palletIndex].ReturnColor(partVisuals[i].name.Split('_').First().ToLower()));
            }
        }
    }
}
