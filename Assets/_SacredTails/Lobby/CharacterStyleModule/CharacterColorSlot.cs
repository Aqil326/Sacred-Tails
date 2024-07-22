using System;
using Timba.Games.SacredTails.PopupModule;
using Timba.Patterns.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     UI Element that represents a color for character
/// </summary>
public class CharacterColorSlot : CharacterStyleSlot
{
    private ColorIdRelation colorValue;
    public Image colorSlotImage;

    public override void InitSlot<T>(T _colorValue)
    {
        colorValue = _colorValue as ColorIdRelation;
        colorSlotImage.color = colorValue.color;
    }

    public void SelectSlot()
    {
        OnColorSelected?.Invoke(colorValue);
    }

    public void SelectLockedSlot()
    {
        ServiceLocator.Instance.GetService<IPopupManager>().ShowInfoPopup("Item locked!");
    }
}

public abstract class CharacterStyleSlot : MonoBehaviour
{
    public Action<CharacterStyleRelation> OnColorSelected;
    public abstract void InitSlot<T>(T _colorValue) where T : CharacterStyleRelation;
}
