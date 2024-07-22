using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _panelExit;
    [SerializeField] private UnityEngine.UI.Button _confirmExitButton;
    [SerializeField] private UnityEngine.UI.Button _cancelExitButton;
    void Start()
    {
        _panelExit.SetActive(false);
        _confirmExitButton.onClick.AddListener(() => ExitApp());
        _cancelExitButton.onClick.AddListener(() => CancelExitApp());
    }

    [SerializeField] private bool panelOn = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            panelOn = !panelOn;
            _panelExit.SetActive(panelOn);
        }
    }
    private void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    private void CancelExitApp()
    {
        panelOn = false;
        _panelExit.SetActive(panelOn);
    }
}
