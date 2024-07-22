using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoneAnimationTracker : MonoBehaviour
{
    public GameObject boneTracker;
    public List<MatAnimation> matAnimations = new List<MatAnimation>();
    public List<Material> materials = new List<Material>();
    float x, y ,z;

    public void Update()
    {
        if (x != boneTracker.transform.localPosition.x)
        {
            x = boneTracker.transform.localPosition.x;
            VerifyAllMatAnimations(MatAnimation.AttachedEdge.x, boneTracker.transform.localPosition.x);
        }
        if (y != boneTracker.transform.localPosition.y)
        {
            y = boneTracker.transform.localPosition.y;
            VerifyAllMatAnimations(MatAnimation.AttachedEdge.y, boneTracker.transform.localPosition.y);
        }
        if (z != boneTracker.transform.localPosition.z)
        {
            z = boneTracker.transform.localPosition.z;
            VerifyAllMatAnimations(MatAnimation.AttachedEdge.z, boneTracker.transform.localPosition.z);
        }
    }

    public void VerifyAllMatAnimations(MatAnimation.AttachedEdge targetAttachedEdge, float bonePositionEdge)
    {
        matAnimations.Where((x) => x.attachedEdge == targetAttachedEdge).ToList().ForEach((x) => {
            if (Mathf.Approximately(x.targetValue,bonePositionEdge))
                materials[x.matIndex].mainTexture = x.image;
        });
    }
}
[System.Serializable]
public struct MatAnimation
{
    public enum AttachedEdge
    {
        x,
        y,
        z
    }

    public int matIndex;
    public float targetValue;
    public AttachedEdge attachedEdge;
    public Texture2D image;
}


