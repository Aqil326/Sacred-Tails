using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Timba.SacredTails.Arena;
using System.Linq;
using Sirenix.OdinInspector;

namespace Timba.SacredTails.VFXController
{
    public class VFXInstancer : MonoBehaviour
    {
        [SerializeField] List<GameObject> projectVfx;

        public GameObject SpawnVFX(int vfxIndex, Vector3 position, Quaternion rotation)
        {
            GameObject vfxObject = Instantiate(projectVfx[vfxIndex], position, rotation);
            vfxObject.AddComponent<VFXDestroyer>();
            return vfxObject;
        }

        [ContextMenu("Test")]
        public void Test()
        {
            foreach (var puto in projectVfx)
                puto.GetComponentInChildren<VfxInfo>().isVfxReversed = false;
        }


        [Button()]
        public void Test2()
        {
            projectVfx = projectVfx.OrderBy(vfx => vfx.name).ToList();
        }

        public VfxInfo GetVfx(int vfxIndex)
        {
            if (vfxIndex < 0)
                return null;

            GameObject vfx = projectVfx[vfxIndex];
            return vfx.GetComponent<VfxInfo>();
        }

        public float GetVfxTime(VfxInfo vfxInfo)
        {
            if (vfxInfo == null)
                return 0;

            return vfxInfo.vfxDuration;
        }

        public float GetVfxHitDelay(int vfxIndex)
        {
            if (vfxIndex < 0)
                return 1;

            GameObject vfx = projectVfx[vfxIndex];
            return vfx.GetComponent<VfxInfo>().vfxBeforeHit;
        }

        public float GetVfxTime(int vfxIndex)
        {
            if (vfxIndex < 0)
                return 0;

            GameObject vfx = projectVfx[vfxIndex];
            return vfx.GetComponent<VfxInfo>().vfxDuration;
        }

        public bool GetVfxIsReversed(VfxInfo vfxInfo)
        {
            if (vfxInfo == null)
                return false;

            return vfxInfo.isVfxReversed;
        }
        public bool GetVfxIsReversed(int vfxIndex)
        {
            if (vfxIndex < 0)
                return false;

            GameObject vfx = projectVfx[vfxIndex];
            return vfx.GetComponent<VfxInfo>().isVfxReversed;
        }
    }
}