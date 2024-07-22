using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Timba.SacredTails.Arena;
using Timba.SacredTails.VFXController;

public class VFXtester : MonoBehaviour
{
    public int vfxIndex;
    public VFXPositionEnum targetIndex;
    public bool isLocalPlayer = false;
    public TurnsController turnsController;
    public VFXInstancer vFXInstancer;

    [Button]
    public void CallVFX()
    {
        Dictionary<VFXPositionEnum, Transform> currentVfxPositions = turnsController.vfxPositionsDictionary;
        vFXInstancer.SpawnVFX(vfxIndex, currentVfxPositions[targetIndex].position, currentVfxPositions[targetIndex].rotation);
    }
}
