using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject helpPanel;

    private void OnEnable()
    {
        helpPanel.SetActive(false);
    }

    public void ToggleHelpPanel()
    {
        helpPanel.SetActive(!helpPanel.activeSelf);
    }
}
