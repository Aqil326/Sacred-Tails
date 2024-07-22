using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void SetActiveToggle(this GameObject target)
    {
        target.SetActive(!target.activeSelf);
    }
}
