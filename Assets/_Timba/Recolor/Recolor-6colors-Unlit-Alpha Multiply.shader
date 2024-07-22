
Shader "Timba/Recolor/6 colors Unlit Multiply" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_BlackThreshold ("Black Threshold", Range(0,0.2)) = 0.1
	_NewColor1 ("Replace Red", Color) = (1,1,1,1)
	_NewColor2 ("Replace Green", Color) = (1,1,1,1)
	_NewColor3 ("Replace Blue", Color) = (1,1,1,1)
	_NewColor4 ("Replace Yellow", Color) = (1,1,1,1)
	_NewColor5 ("Replace Cyan", Color) = (1,1,1,1)
	_NewColor6 ("Replace Magenta", Color) = (1,1,1,1)

	//MASK SUPPORT ADD
    _StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255
    _ColorMask ("Color Mask", Float) = 15
    //MASK SUPPORT END
	}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Cull Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	//MASK SUPPORT ADD
    Stencil
    {
      Ref [_Stencil]
      Comp [_StencilComp]
      Pass [_StencilOp]
      ReadMask [_StencilReadMask]
      WriteMask [_StencilWriteMask]
    }
    ColorMask [_ColorMask]
    //MASK SUPPORT END

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			fixed4 _Color;
			float _BlackThreshold;
			fixed4 _NewColor1;
			fixed4 _NewColor2;
			fixed4 _NewColor3;
			fixed4 _NewColor4;
			fixed4 _NewColor5;
			fixed4 _NewColor6;
			static const fixed4 _PatCol1 = fixed4(1, 0, 0, 1);
			static const fixed4 _PatCol2 = fixed4(0, 1, 0, 1);
			static const fixed4 _PatCol3 = fixed4(0, 0, 1, 1);
			static const fixed4 _PatCol4 = fixed4(1, 1, 0, 1);
			static const fixed4 _PatCol5 = fixed4(0, 1, 1, 1);
			static const fixed4 _PatCol6 = fixed4(1, 0, 1, 1);
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.texcoord) * _Color;
				fixed3 pureColor = step(_BlackThreshold, c.rgb);
				// Buscar el color de patron mas cercano al pixel
				fixed3 hue = _PatCol1.rgb;
				fixed3 newColor = _NewColor1.rgb;
				float d = distance(pureColor, _PatCol1.rgb);
				float d2 = distance(pureColor, _PatCol2.rgb);
				float d3 = distance(pureColor, _PatCol3.rgb);
				float d4 = distance(pureColor, _PatCol4.rgb);
				float d5 = distance(pureColor, _PatCol5.rgb);
				float d6 = distance(pureColor, _PatCol6.rgb);
				if(d2 < d)
				{
					d = d2;
					hue = _PatCol2.rgb;
					newColor = _NewColor2.rgb;
				}
				if(d3 < d)
				{
					d = d3;
					hue = _PatCol3.rgb;
					newColor = _NewColor3.rgb;
				}
				if(d4 < d)
				{
					d = d4;
					hue = _PatCol4.rgb;
					newColor = _NewColor4.rgb;
				}
				if(d5 < d)
				{
					d = d5;
					hue = _PatCol5.rgb;
					newColor = _NewColor5.rgb;
				}
				if(d6 < d)
				{
					d = d6;
					hue = _PatCol6.rgb;
					newColor = _NewColor6.rgb;
				}

				// Aplicar multiplicacion de color
				float grayInt = max (max (c.r, c.g), c.b);
				fixed3 gray = fixed3(grayInt,grayInt,grayInt);
				c.rgb = gray * newColor;
				return c;
			}


		ENDCG
	}
}

}
