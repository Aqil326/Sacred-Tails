//Unity offical codes for Lighting

void MainLight_half(half3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
	Direction = half3(0.5, 0.5, 0);
	Color = 1;
	DistanceAtten = 1;
	ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
	half4 clipPos = TransformWorldToHClip(WorldPos);
	half4 shadowCoord = ComputeScreenPos(clipPos);
#else
	half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
	Light mainLight = GetMainLight(shadowCoord);
	Direction = mainLight.direction;
	Color = mainLight.color;
	DistanceAtten = mainLight.distanceAttenuation;
	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half shadowStrength = GetMainLightShadowStrength();
	ShadowAtten = SampleShadowmap(shadowCoord, TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowSamplingData, shadowStrength, false);
#endif
}






void DirectSpecular_half(half3 Specular, half Smoothness, half3 Direction, half3 Color, half3 WorldNormal, half3 WorldView, out half3 Out)
{
#ifdef SHADERGRAPH_PREVIEW
	Out = 0;
#else
	Smoothness = exp2(10 * Smoothness + 1);
	WorldNormal = normalize(WorldNormal);
	WorldView = SafeNormalize(WorldView);
	Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, half4(Specular, 0), Smoothness);
#endif
}


void AdditionalLights_half(half4 albedo, half3 ramp, Texture2D Ramp2D, sampler rampSample,  half SpecularI, half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
{
	half spec = 0;
	half atten = 1;
	half2 rampUV;
	half NdotL;
	half NdotE;

	half3 diffuseColor = 0;
	half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
	Smoothness = exp2(10 * Smoothness + 1);
	WorldNormal = normalize(WorldNormal);
	WorldView = SafeNormalize(WorldView);
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, WorldPosition);

		NdotL = dot(WorldNormal, normalize(light.direction));
		rampUV.x = NdotL * 0.45 + 0.45 * atten;
		rampUV.y = NdotE;
		ramp = SAMPLE_TEXTURE2D(Ramp2D, rampSample, rampUV);

		
		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
	
		diffuseColor += (albedo * ramp) * LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
		
		atten =  light.distanceAttenuation * light.shadowAttenuation;
		NdotE = max(0.0, dot(WorldNormal, normalize(light.direction)));
		spec +=  (pow(NdotE, SpecularI * 128 ) * Smoothness);

		specularColor += (albedo * ramp) * LightingSpecular( attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);


	}

#endif


	
	Diffuse = albedo +  diffuseColor * ramp * spec;
	Specular = albedo + specularColor * ramp * spec;
}

void AdditionalHighLight_half(half _AnisoOffset, half4 albedo, Texture2D Ramp2D, sampler rampSample, half SpecularI, half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
{
	half NdotL;
	half NdotE;
	half3 ramp = half3(0,0,0);
	half2 rampUV;
	half3 h;
	half HdotA;
	half aniso;
	half spec = 0;
	half spec2 = 0;
	half atten = 1;
	half3 diffuseColor = 0;
	half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
	Smoothness = exp2(10 * Smoothness + 1);
	WorldNormal = normalize(WorldNormal);
	WorldView = SafeNormalize(WorldView);
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, WorldPosition);

		NdotL = dot(WorldNormal, normalize(light.direction));
		rampUV.x = NdotL * 0.45 + 0.45 * atten;
		rampUV.y = NdotE;
		ramp = SAMPLE_TEXTURE2D(Ramp2D, rampSample, rampUV);

	

		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);

		diffuseColor += (albedo * ramp) * LightingLambert(attenuatedLightColor, light.direction, WorldNormal);

		atten = light.shadowAttenuation;
		NdotE = max(0.0, dot(WorldNormal, normalize(light.direction)));
		spec += (pow(NdotE, SpecularI * light.distanceAttenuation) * Smoothness) * SpecColor;

		specularColor += (albedo * ramp) * LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);

		h = normalize(normalize(light.direction) + normalize(WorldView));

		HdotA = dot(normalize(WorldNormal + WorldNormal.xyz), h);
		aniso = max(0, sin(radians((HdotA + _AnisoOffset) * 180)));
		spec2 += pow(aniso, SpecularI *128 * atten) * Smoothness;
	}

#endif

	Diffuse = albedo  + diffuseColor * ramp * spec;
		Diffuse += spec2/4 * SpecColor/8;
		Specular = albedo/2 + specularColor * ramp * spec;
}

void AdditionalLightsSOD_half(half4 albedo, half SpecularI, half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
{
	half spec = 0;
	half atten = 1;


	half3 diffuseColor = 0;
	half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
	Smoothness = exp2(100 * Smoothness);
	WorldNormal = normalize(WorldNormal);
	WorldView = SafeNormalize(WorldView);
	int pixelLightCount = GetAdditionalLightsCount();
	for (int i = 0; i < pixelLightCount; ++i)
	{
		Light light = GetAdditionalLight(i, WorldPosition);



		half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);

		diffuseColor += step(0.5,LightingLambert(attenuatedLightColor, light.direction, WorldNormal)*2);

		atten = light.distanceAttenuation * light.shadowAttenuation;

		specularColor +=  step(0.5,LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness)*2);
	}

#endif



	Diffuse = albedo + albedo *diffuseColor;
	Specular = albedo + albedo * specularColor;
}
