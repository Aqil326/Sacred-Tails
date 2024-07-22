using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class AtlasTextures
{
    [PreviewField(ObjectFieldAlignment.Center, Height = 100f)]
    public Texture2D Difuse;
    [PreviewField(ObjectFieldAlignment.Center, Height = 100f)]
    public Texture2D Normal;
    [PreviewField(ObjectFieldAlignment.Center, Height = 100f)]
    public Texture2D Metallic;
    [PreviewField(ObjectFieldAlignment.Center, Height = 100f)]
    public Texture2D AmbientOclusion;
}
