void toonLight_half(half _toonmetalic, half _AnisoOffset, half alpha, half4 Albedo, half4 LightColor, half3 Normal, half3 lightDir, half3 viewDir, half atten, half Specular, half4 SpecularColor, half Smoothness, out half4 c) {

#if UNITY_COLORSPACE_GAMMA
	half NdotL = max(0.0, dot(Normal, normalize(lightDir)));

	if (NdotL < 0.4) {
		NdotL = 0.1;
	}
	else if (NdotL < _toonmetalic) {
		NdotL = 0.3;
	}
	else if (NdotL > _toonmetalic) {
		NdotL = 0.6;
	}
#endif

#if !UNITY_COLORSPACE_GAMMA
	half NdotL = max(0.0, dot(Normal, normalize(lightDir)));

	if (NdotL < 0.4) {
		NdotL = 0.1;
	}
	else if (NdotL < _toonmetalic) {
		NdotL = 0.3;
	}
	else if (NdotL > _toonmetalic) {
		NdotL = 0.6;
	}
#endif






	half NdotE2 = max(0.0, dot(Normal, normalize(lightDir)));

	NdotL = 1 + clamp(NdotL, 0, 1);

	half spec = pow(NdotE2, Specular * 32) * Smoothness;



#if UNITY_COLORSPACE_GAMMA
	half3 AlbedoM = Albedo/4  * LightColor  * NdotL + Albedo * NdotL + Albedo * SpecularColor * NdotE2;
#endif
#if !UNITY_COLORSPACE_GAMMA
	half3 AlbedoM = Albedo * 6 * LightColor  * NdotL + Albedo * NdotL + Albedo * SpecularColor  * NdotE2;
#endif
	if (Smoothness > 0) {
		AlbedoM += SpecularColor * spec;
	}

	c.xyz = AlbedoM;
	c.w = 0;

}


void toonAlbedo_half(half4 _MainTex, half4 _Color, out half4 o) {
	half4 tex = _MainTex;
	o.xyz = tex.xyz * _Color;
	o.w = tex.w;
}
