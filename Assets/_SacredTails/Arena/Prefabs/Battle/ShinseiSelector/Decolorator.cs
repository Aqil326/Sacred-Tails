using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Sprites;
using UnityEngine;
using UnityEngine.UI;

public class Decolorator : MonoBehaviour
{
    #region ----Fields----
    public ComputeShader computeShader;
    public List<Image> imageList;
    private List<Sprite> originalSprite = new List<Sprite>();
    public List<Texture> textures = new List<Texture>();
    #endregion ----Fields----

    #region ----Methods----	
    public void Init(List<Image> targetImages)
    {
        imageList = targetImages;
        foreach (var item in imageList)
        {
            Sprite sprite = item.sprite;
            originalSprite.Add(sprite);
        }
    }

    [Button("Black and white")]
    public void BlackAndWhite()
    {
        textures = new List<Texture>();
        foreach (var item in imageList)
        {
            Sprite sprite = item.sprite;
            RenderTexture renderTexture = new RenderTexture((int)sprite.rect.width, (int)sprite.rect.height, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            Texture2D textureToModify = item.sprite.texture;
            Color[] pix = textureToModify.GetPixels(Mathf.CeilToInt(sprite.rect.x), Mathf.CeilToInt(sprite.rect.y), Mathf.CeilToInt(sprite.rect.width), Mathf.CeilToInt(sprite.rect.height));
            Texture2D destTex = new Texture2D(Mathf.CeilToInt(sprite.rect.width), Mathf.CeilToInt(sprite.rect.height));
            destTex.SetPixels(pix);
            destTex.Apply();
            textures.Add(destTex);

            computeShader.SetTexture(0, "Texture", destTex);
            computeShader.SetTexture(0, "Result", renderTexture);
            computeShader.Dispatch(0, renderTexture.width / 7, renderTexture.height / 7, 1);

            Texture2D finalTexture = toTexture2D(renderTexture);
            item.sprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), new Vector2(0.5f, 0.5f),100,1,SpriteMeshType.FullRect,item.sprite.border);
        }
    }

    [Button("Color")]
    public void Color()
    {
        for (int i = 0; i < imageList.Count-1; i++)
        {
            imageList[i].sprite = originalSprite[i];
        }
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    #endregion ----Methods----	
}
