using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Timba.Games.CharacterFactory;

public class ElementIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ElementToolTip elementToolTip;

    public bool isWeakness = false;
    public bool isStrengths = false;

    private CharacterType type;
    private float multi = 0;
    private float scale = 0;

    public void SetElementData(CharacterType locType, float locMulti, float locScale)
    {
        type = locType;
        multi = locMulti;
        scale = locScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*if (UseInCodeAkSounds)
            AkSoundEngine.PostEvent("U_Select", gameObject);
        onPointerEnter?.Invoke();*/
        if (isWeakness)
        {
            elementToolTip.SetElementWeaknessInfo(type, multi);
        }
        else if (isStrengths)
        {
            elementToolTip.SetElementStrengthInfo(type, multi);
        }
        else
        {
            elementToolTip.SetElementTypeInfo(type, scale);
        }

        elementToolTip.transform.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //onPointerExit?.Invoke();
        elementToolTip.transform.gameObject.SetActive(false);
    }
}
