#ifndef CHESSFUNCTIONS_INCLUDE
#define CHESSFUNCTIONS_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

#define MAXIMUM_CONTROLLERS 4

float _BurntSpheresCount;
float3 _BurntSpheresPosition[MAXIMUM_CONTROLLERS];
float4 _BurntSpheresProperties1[MAXIMUM_CONTROLLERS];	// x: Radius, y: Distance Minimum, z: Distance Maximum
float4 _BurntSpheresProperties2[MAXIMUM_CONTROLLERS];	// x: Ember Radius, y: Ember Strength, z: Ember Distance Minimum, w: Ember Distance Maximum

void Sphere_float(in float3 Position, in float3 Center, in float Radius, out float Distance)
{
	Distance = length(Position - Center) - Radius;
}

float Remap(in float Value, in float2 InMinMax, float2 OutMinMax)
{
	return OutMinMax.x + (Value - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float3 NormalStrength(float3 Normal, float Strength)
{
	return float3(Normal.rg * Strength, lerp(1, Normal.b, saturate(Strength)));
}

void SpecularColorFunction_float(	in float2 UV, in UnityTexture2D BaseMap, in float4 BaseColor, in UnityTexture2D MaskMap, in float AOMin, in float AOMax,
									in float SmoothnessMin, in float SmoothnessMax, in UnityTexture2D NormalMap, in float NormalScale,
									in UnityTexture2D SpecularColorMap, in float4 SpecularColor,
									out float3 Color, out float AmbientOcclusion, out float Smoothness, out float3 Normal, out float3 Specular)
{
	Color = SAMPLE_TEXTURE2D(BaseMap.tex, BaseMap.samplerstate, UV).rgb * BaseColor.rgb;

	float4 mask = SAMPLE_TEXTURE2D(MaskMap.tex, MaskMap.samplerstate, UV);
	AmbientOcclusion = Remap(mask.g, float2(0.0, 1.0), float2(AOMin, AOMax));
	Smoothness = Remap(mask.a, float2(0.0, 1.0), float2(SmoothnessMin, SmoothnessMax));

	Normal = NormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(NormalMap.tex, NormalMap.samplerstate, UV)), NormalScale);

	Specular = SAMPLE_TEXTURE2D(SpecularColorMap.tex, SpecularColorMap.samplerstate, UV).rgb * SpecularColor.rgb;
}

void TilingAndOffset_float(in float2 UV, in float4 TilingOffset, out float2 Out)
{
	Out = UV * TilingOffset.xy + TilingOffset.zw;
}

void BlendFactor_float(in float Progress, in float2 UV, in UnityTexture2D ErosionMap, in UnityTexture2D HeightMap, in float Minimum, in float Maximum, out float Out)
{
	float erosion = SAMPLE_TEXTURE2D(ErosionMap.tex, ErosionMap.samplerstate, UV).r;
	float height = SAMPLE_TEXTURE2D(HeightMap.tex, HeightMap.samplerstate, UV).r;

	float factor = saturate(((1.0 - Progress) * 2.0) - erosion);
	Out = smoothstep(Minimum, Maximum, saturate(((1.0 - factor) * 2.0) - height));
}

void BlendFactor_float(in float Progress, in float2 UV, in float Erosion, in UnityTexture2D HeightMap, in float Minimum, in float Maximum, out float Out)
{
	float height = SAMPLE_TEXTURE2D(HeightMap.tex, HeightMap.samplerstate, UV).r;

	float factor = saturate(((1.0 - Progress) * 2.0) - Erosion);
	Out = smoothstep(Minimum, Maximum, saturate(((1.0 - factor) * 2.0) - height));
}

void BlendSpecularColorFunction_float(	in float Factor, in float3 Color_A, in float Occlusion_A, in float Smoothness_A, in float3 Normal_A, in float3 Specular_A,
										in float3 Color_B, in float Occlusion_B, in float Smoothness_B, in float3 Normal_B, in float3 Specular_B,
										out float3 Color, out float Occlusion, out float Smoothness, out float3 Normal, out float3 Specular)
{
	Color = lerp(Color_A, Color_B, Factor);
	Occlusion = lerp(Occlusion_A, Occlusion_B, Factor);
	Smoothness = lerp(Smoothness_A, Smoothness_B, Factor);

	Normal_A = lerp(Normal_A, float3(0.0, 0.0, 1.0), Factor);
	Normal = lerp(Normal_A, BlendNormalRNM(Normal_A, Normal_B), Factor);

	Specular = lerp(Specular_A, Specular_B, Factor);
}

void FlowNormal_float(in float2 UV, in UnityTexture2D NormalMap, in float NormalScale, in UnityTexture2D FlowTex, in float2 FlowUV, in float2 FlowStrength, out float3 Normal)
{
	float2 flowDir = SAMPLE_TEXTURE2D(FlowTex.tex, FlowTex.samplerstate, FlowUV).xy * 2.0f - 1.0f;
	flowDir *= FlowStrength;

	float phase0 = frac(_Time.y * 0.5f + 0.5f);
	float phase1 = frac(_Time.y * 0.5f + 1.0f);

	float4 tex0 = SAMPLE_TEXTURE2D(NormalMap.tex, NormalMap.samplerstate, UV + flowDir.xy * phase0);
	float4 tex1 = SAMPLE_TEXTURE2D(NormalMap.tex, NormalMap.samplerstate, UV + flowDir.xy * phase1);

	float factor = abs((0.5f - phase0) / 0.5f);
	float4 n = lerp(tex0, tex1, factor);
	Normal = NormalStrength(UnpackNormal(n), NormalScale);
}

void Ember_float(in float3 Position, in float Factor, in float2 UV, in UnityTexture2D EmberMap, in float4 Color, in float Minimum, in float Maximum, in float Speed, out float3 Emission)
{
	float sphereMask = 0.0;
	for (int i = 0; i < _BurntSpheresCount; ++i)
	{
		float radius = _BurntSpheresProperties2[i].x;
		float strength = _BurntSpheresProperties2[i].y;
		float minimum = min(_BurntSpheresProperties2[i].z, _BurntSpheresProperties2[i].w - 0.001);
		float maximum = _BurntSpheresProperties2[i].w;

		float distance = 0.0;
		Sphere_float(Position, _BurntSpheresPosition[i].xyz, radius, distance);
		sphereMask += (1.0 - saturate(smoothstep(minimum * radius, maximum * radius, distance))) * strength;
	}

	float staticMask = smoothstep(Minimum, Maximum, SAMPLE_TEXTURE2D(EmberMap.tex, EmberMap.samplerstate, UV).r);

	float animatedMask = SAMPLE_TEXTURE2D(EmberMap.tex, EmberMap.samplerstate, UV + float2(0.0, Speed * _Time.y)).g;
	animatedMask *= SAMPLE_TEXTURE2D(EmberMap.tex, EmberMap.samplerstate, UV + float2(0.0, -Speed * _Time.y)).g;

	Emission = Factor * staticMask * Color.rgb * animatedMask * sphereMask;
	//Emission = sphereMask;
}

void BurntErosion_float(in float3 Position, out float Out)
{
	Out = 1.0;
	for (int i = 0; i < _BurntSpheresCount; ++i)
	{
		float radius = _BurntSpheresProperties1[i].x;
		float minimum = min(_BurntSpheresProperties1[i].y, _BurntSpheresProperties1[i].z - 0.001);
		float maximum = _BurntSpheresProperties1[i].z;

		float distance = 0.0;
		Sphere_float(Position, _BurntSpheresPosition[i].xyz, radius, distance);
		Out *= saturate(smoothstep(minimum * radius, maximum * radius, distance));
	}
	Out = 1.0 - Out;
}

#endif // CHESSFUNCTIONS_INCLUDE