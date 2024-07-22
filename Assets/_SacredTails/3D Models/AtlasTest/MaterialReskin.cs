using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Timba.SacredTails.CharacterStyle;

public class MaterialReskin : MonoBehaviour
{
    [Header("Material parameter names")]
    [SerializeField] string mainTexture = "_MainTex";
    [SerializeField] string normalMap = "_BumpMap", metallicGloss = "_MetallicGlossMap", ambientOclussion = "_OcclusionMap";
    [SerializeField] List<SkinnedMeshRenderer> meshRenderers = new List<SkinnedMeshRenderer>();
    [SerializeField] Material targetMaterial;
    [SerializeField] int horizontalSplitParts, verticalSplitParts;
    private int horizontalTextureSize, verticalTextureSize;
    [PreviewField]
    public List<AtlasTextures> skinList = new List<AtlasTextures>();
    List<Vector2Int> texturePosition = new List<Vector2Int>();
    List<Texture2D> mergeTextures = new List<Texture2D>();
    [SerializeField] CharacterRecolor characterRecolor;
    //TODO remove this later
    [SerializeField] int targetSkin, targetPosition;
    bool isInit = false;
    public void InitReskin()
    {
        if (!isInit)
        {
            Init();
            ChangePart(0, 0);
        }
    }
    public void Init()
    {
        mergeTextures.Add(new Texture2D(skinList[0].Difuse.width, skinList[0].Difuse.height, skinList[0].Difuse.format, true));
        mergeTextures[0].SetPixels(skinList[0].Difuse.GetPixels());
        mergeTextures.Add(new Texture2D(skinList[0].Normal.width, skinList[0].Normal.height, skinList[0].Normal.format, true));
        mergeTextures[1].SetPixels(skinList[0].Normal.GetPixels());
        mergeTextures.Add(new Texture2D(skinList[0].Metallic.width, skinList[0].Metallic.height, skinList[0].Metallic.format, true));
        mergeTextures[2].SetPixels(skinList[0].Metallic.GetPixels());
        mergeTextures.Add(new Texture2D(skinList[0].AmbientOclusion.width, skinList[0].AmbientOclusion.height, skinList[0].AmbientOclusion.format, true));
        mergeTextures[3].SetPixels(skinList[0].AmbientOclusion.GetPixels());

        horizontalTextureSize = mergeTextures[0].width / horizontalSplitParts;
        verticalTextureSize = mergeTextures[0].height / verticalSplitParts;

        //Create new material
        Material material = meshRenderers[0].material;
        material.SetTexture(mainTexture, mergeTextures[0]);
        material.SetTexture(normalMap, mergeTextures[1]);
        material.SetTexture(metallicGloss, mergeTextures[2]);
        material.SetTexture(ambientOclussion, mergeTextures[3]);
        material.EnableKeyword("_NORMALMAP");
        targetMaterial = material;
        characterRecolor.Init(targetMaterial);

        //Apply new material to all parts :D
        for (int i = 0; i < meshRenderers.Count; i++)
            meshRenderers[i].material = targetMaterial;
        //Verify if split numbers are even
        if (horizontalSplitParts % 2 != 0)
            horizontalSplitParts += 1;
        if (verticalSplitParts % 2 != 0)
            verticalSplitParts += 1;
        //Add the position of textures in a list
        for (int i = 0; i < horizontalSplitParts; i++)
            for (int a = 0; a < verticalSplitParts; a++)
                texturePosition.Add(new Vector2Int(i * horizontalTextureSize, a * verticalTextureSize));
        isInit = true;
    }

    public void ChangePart(int targetSkin, int targetPosition)
    {
        if (!isInit)
            Init();
        //Change Difuse
        Color[] partOfTexture = skinList[targetSkin].Difuse.GetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize);
        mergeTextures[0].SetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize, partOfTexture);
        mergeTextures[0].Apply();
        //Change Normal
        partOfTexture = skinList[targetSkin].Normal.GetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize);
        mergeTextures[1].SetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize, partOfTexture);
        mergeTextures[1].Apply();
        //mergeTextures[1] = NormalMapToUnityFormat(mergeTextures[1]);
        //Change Metal
        partOfTexture = skinList[targetSkin].Metallic.GetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize);
        mergeTextures[2].SetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize, partOfTexture);
        mergeTextures[2].Apply();
        //Change Ambient
        partOfTexture = skinList[targetSkin].AmbientOclusion.GetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize);
        mergeTextures[3].SetPixels(texturePosition[targetPosition].x, texturePosition[targetPosition].y, horizontalTextureSize, verticalTextureSize, partOfTexture);
        mergeTextures[3].Apply();
    }

    [Button("ChangeMergeTexture")]
    public void ChangePart()
    {
        ChangePart(targetSkin, targetPosition);
    }
    private Texture2D DTXnm2RGBA(Texture2D tex)
    {
        Color[] colors = tex.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            Color c = colors[i];
            c.r = c.a * 2 - 1;  //red<-alpha (x<-w)
            c.g = c.g * 2 - 1; //green is always the same (y)
            Vector2 xy = new Vector2(c.r, c.g); //this is the xy vector
            c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy))); //recalculate the blue channel (z)
            colors[i] = new Color(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f); //back to 0-1 range
        }
        tex.SetPixels(colors); //apply pixels to the texture
        tex.Apply();
        return tex;
    }

    public static Texture2D NormalMapToUnityFormat(Texture2D normalMap)
    {

        Texture2D t = new Texture2D(normalMap.width, normalMap.height);
        Color32[] cols = normalMap.GetPixels32(0);
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i] = new Color32(1, cols[i].g, cols[i].g, cols[i].r);
        }
        t.SetPixels32(cols);
        t.Apply();
        return t;
    }
}
