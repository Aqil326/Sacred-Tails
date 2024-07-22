using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwap_edits : MonoBehaviour {

    private Renderer rend;
    /*
    public Color nc1;
    public Color nc2;
    public Color nc3;
    public Color nc4g;
    public Color nc5g;
    */

    private float valueR;
    private float valueG;
    private float valueB;
    private float valueA;
    private float FXLevel=1.0f;
    private float extraGloss = 0.0f;
    private float extraMetal = 0.0f;
    private int selected = 1;
    private float outlineW;
    

    private GUIStyle currentStyle = null;
    
    // Use this for initialization
    void Start () {
        rend = GetComponent<Renderer>();


    }
	
	// Update is called once per frame
	void Update () {
       
        if (Input.GetKeyDown("1"))
        {
            selected = 1;
            valueR = rend.material.GetColor("_NewColor1").r;
            valueG = rend.material.GetColor("_NewColor1").g;
            valueB = rend.material.GetColor("_NewColor1").b;
            valueA = rend.material.GetFloat("_Soft_FallOff1");
        }

        if (Input.GetKeyDown("2"))
        {
            selected = 2;
            valueR = rend.material.GetColor("_NewColor2").r;
            valueG = rend.material.GetColor("_NewColor2").g;
            valueB = rend.material.GetColor("_NewColor2").b;
            valueA = rend.material.GetFloat("_Soft_FallOff2");
        }

        if (Input.GetKeyDown("3"))
        {
            selected = 3;
            valueR = rend.material.GetColor("_NewColor3").r;
            valueG = rend.material.GetColor("_NewColor3").g;
            valueB = rend.material.GetColor("_NewColor3").b;
            valueA = rend.material.GetFloat("_Soft_FallOff3");
        }

        if (Input.GetKeyDown("4"))
        {
            selected = 4;
            valueR = rend.material.GetColor("_NewColor4").r;
            valueG = rend.material.GetColor("_NewColor4").g;
            valueB = rend.material.GetColor("_NewColor4").b;
            valueA = rend.material.GetFloat("_Soft_FallOff4");
        }

        if (Input.GetKeyDown("5"))
        {
            selected = 5;
            valueR = rend.material.GetColor("_NewColor5").r;
            valueG = rend.material.GetColor("_NewColor5").g;
            valueB = rend.material.GetColor("_NewColor5").b;
            valueA = rend.material.GetFloat("_Soft_FallOff5");
        }

    }

    // Gui Sliders

    void OnGUI()
    {
        if (selected==1)
        {
 
            rend.material.SetColor("_NewColor1",new Color (valueR, valueG, valueB, 1.0f));
            rend.material.SetFloat("_Soft_FallOff1", valueA);
        }

        if (selected == 2)
        {

            rend.material.SetColor("_NewColor2", new Color(valueR, valueG, valueB, 1.0f));
            rend.material.SetFloat("_Soft_FallOff2", valueA);
        }

        if (selected == 3)
        {

            rend.material.SetColor("_NewColor3", new Color(valueR, valueG, valueB,1.0f));
            rend.material.SetFloat("_Soft_FallOff3", valueA);
        }

        if (selected == 4)
        {

            rend.material.SetColor("_NewColor4", new Color(valueR, valueG, valueB, 1.0f));
            rend.material.SetFloat("_Soft_FallOff4", valueA);

            // EXTRA GLOSS
            GUI.Label(new Rect(10, 310, 1920, 20), "EXTRA GLOSS:" + (rend.material.GetFloat("_ExtraGloss_FromColor4")).ToString());
            extraGloss = GUI.HorizontalSlider(new Rect(25, 330, 200, 30), extraGloss, 0.0F, 1.0F);
            rend.material.SetFloat("_ExtraGloss_FromColor4", extraGloss);
        }

        if (selected == 5)
        {

            rend.material.SetColor("_NewColor5", new Color(valueR, valueG, valueB, 1.0f));
            rend.material.SetFloat("_Soft_FallOff5", valueA);

            // EXTRA METAL
            GUI.Label(new Rect(10, 310, 1920, 20), "EXTRA METAL:" + (rend.material.GetFloat("_ExtraMetal_FromColor5")).ToString());
            extraMetal = GUI.HorizontalSlider(new Rect(25, 330, 200, 30), extraMetal, 0.0F, 1.0F);
            rend.material.SetFloat("_ExtraMetal_FromColor5", extraMetal);
        }
        // RED
        GUI.Label(new Rect(10, 130, 1920, 20), "RED:" + (Mathf.Round(valueR * 255).ToString()));
        valueR = GUI.HorizontalSlider(new Rect(25, 150, 200, 30), valueR, 0.0F, 1.0F);

        // GRN
        GUI.Label(new Rect(10, 160, 1920, 20), "GREEN:" + (Mathf.Round(valueG * 255).ToString()));
        valueG = GUI.HorizontalSlider(new Rect(25, 180, 200, 30), valueG, 0.0F, 1.0F);

        // BLU
        GUI.Label(new Rect(10, 190, 1920, 20), "BLUE:" + (Mathf.Round(valueB * 255).ToString()));
        valueB = GUI.HorizontalSlider(new Rect(25, 210, 200, 30), valueB, 0.0F, 1.0F);

        // ALP
        GUI.Label(new Rect(10, 220, 1920, 20), "SOFTNESS(A):" + (Mathf.Round(valueA * 255).ToString()));
        valueA = GUI.HorizontalSlider(new Rect(25, 240, 200, 30), valueA, 0.0F, 1.0F);

        // EFFECT
        GUI.Label(new Rect(10, 260, 1920, 20), "REPLACE EFFECT LEVEL:" + (Mathf.Round(rend.material.GetFloat("_OverallEffect")*100).ToString())+"%" );
        FXLevel = GUI.HorizontalSlider(new Rect(25, 280, 200, 30), FXLevel, 0.0F, 1.0F);
        rend.material.SetFloat("_OverallEffect", FXLevel);

        // OutlineThickness
        outlineW = rend.material.GetFloat("_OutlineWidth");
        GUI.Label(new Rect(10, 380, 1920, 20), "OUTLINE WIDTH:" + rend.material.GetFloat("_OutlineWidth").ToString());
        outlineW = GUI.HorizontalSlider(new Rect(25, 400, 200, 30), outlineW, 0.0F, 1.0F);
        rend.material.SetFloat("_OutlineWidth", outlineW);
        // borrow color 1 color for the outline
        rend.material.SetColor("_OutlineColor", new Color
            (rend.material.GetColor("_NewColor1").r,
            rend.material.GetColor("_NewColor1").g,
            rend.material.GetColor("_NewColor1").b, 1.0f));

       
    // color box
    currentStyle = new GUIStyle(GUI.skin.box);
        currentStyle.normal.background = MakeTex(2, 2, new Color(valueR, valueG, valueB, 1.0f));
        GUI.Box(new Rect(10, 90, 215, 30), "NewColor", currentStyle);

        GUI.Label(new Rect(10, 10, 1920, 20), "Press 1-5 to pick the color to affect: Current Color : "+selected.ToString());
    }

    // Functions

    private void InitStyles()
    {
        if (currentStyle == null)
        {
            currentStyle = new GUIStyle(GUI.skin.box);
            currentStyle.normal.background = MakeTex(2, 2, new Color(valueR, valueG, valueB, 1.0f));
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

}


