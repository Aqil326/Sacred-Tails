using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectableUiButton : MonoBehaviour
{
    public Button button;
    [SerializeField] List<UiElement> elementImage = new List<UiElement>();
    [SerializeField] Decolorator decolorator;

    private void Start()
    {
        if (decolorator == null)
            return;
        List<Image> allImages = new List<Image>();
        foreach (var elements in elementImage)
        {
            if (elements.elementImage == null)
                continue;
            allImages.Add(elements.elementImage);
        }
        decolorator?.Init(allImages);
    }

    public void SetSelected()
    {
        if (!button.enabled)
            return;
        foreach (var element in elementImage)
            element.PutSelectedColors();
    }

    public void SetUnselected()
    {
        if (!button.enabled)
            return;
        foreach (var element in elementImage)
            element.PutUnselectedColors();
    }

    public void SetEnable()
    {
        button.interactable = true;
        button.enabled = true;
        decolorator?.Color();
    }

    public void SetDisable()
    {
        button.interactable = false;
        button.enabled = false;
        decolorator?.BlackAndWhite();
    }

    [System.Serializable]
    class UiElement
    {
        public Image elementImage ;
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Color selectedColor, unselectedColor;

        public void PutSelectedColors()
        {
            if (elementImage != null)
                elementImage.color = selectedColor;
            if (text != null)
                text.color = selectedColor;
        }

        public void PutUnselectedColors()
        {
            if (elementImage != null)
                elementImage.color = unselectedColor;
            if (text != null)
                text.color = unselectedColor;
        }
    }
}
