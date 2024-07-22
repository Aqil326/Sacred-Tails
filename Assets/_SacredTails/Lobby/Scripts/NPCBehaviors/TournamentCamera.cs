using System.Collections;
using System.Collections.Generic;
using Timba.SacredTails.UiHelpers;
using UnityEngine;

public class TournamentCamera : MonoBehaviour
{
    [SerializeField] GameObject Camera;
    [SerializeField] string UIGroup;

    private void OnEnable()
    {
        StartCoroutine(DoAtLast());
    }

    IEnumerator DoAtLast()
    {
        yield return null;
        Show();
    }

    private void OnDisable()
    {
        Hide();
    }

    public void Show()
    {
        UIGroups.instance.ShowOnlyThisGroup(UIGroup);
        Camera.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Camera.gameObject.SetActive(false);
        UIGroups.instance.ShowOnlyThisGroup("planner");
    }
}
