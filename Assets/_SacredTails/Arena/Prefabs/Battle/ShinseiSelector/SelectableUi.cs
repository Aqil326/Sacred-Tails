using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableUi : MonoBehaviour
{
    [SerializeField] public List<SelectableUiButton> selectableButtons = new List<SelectableUiButton>();
    public int selectable;
    public bool dontAddOnClickToButtons;

    public void Awake()
    {
        InitButtons();
    }

    public void InitButtons()
    {
        if (dontAddOnClickToButtons)
            return;
        foreach (var UiButton in selectableButtons)
        {
            UiButton.button.onClick.RemoveAllListeners();
            UiButton.button.onClick.AddListener(() => { OnClick(UiButton); });
            UiButton.button.onClick.AddListener(UiButton.SetSelected);
        }

    }

    public void OnClick(SelectableUiButton selectableUiButton, bool selected = false)
    {
        foreach (var UiButton in selectableButtons)
        {
            if (selectableUiButton == UiButton)
            {
                if (selected)
                    UiButton.SetSelected();
                else
                    UiButton.SetUnselected();
            }
            else if (selectableUiButton.button.enabled)
                UiButton.SetUnselected();
        }
    }


    [Button("ChangeStateToSelectedButton")]
    public void TryChangeStateToSelectable()
    {
        if (!selectableButtons[selectable].button.enabled)
        {
            selectableButtons[selectable].SetEnable();
            selectableButtons[selectable].SetUnselected();
        }
        else
        {
            selectableButtons[selectable].SetDisable();
            selectableButtons[selectable].SetUnselected();
        }
    }
}
