//UNITY_SHADER_NO_UPGRADE
#ifndef SMOKEFUNCTIONS_INCLUDED
#define SMOKEFUNCTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

/***  SMOKE RELATED FUNCTIONS ***/

// Inspired by "The Invisible Hours" from Tequila Works.

void Motion_float(in float3 VertexWS, in float3 TargetPositionWS, in UnityTexture2D PositionsTex, in float Frame, in float Delay, out float3 PositionWS)
{
	float2 frameCount = PositionsTex.texelSize.zw;
	float frameTotal = frameCount.x * frameCount.y;

	Frame = Frame - Delay;
	if (Frame < 0.0)
	{
		Frame = 0.0;
		//Frame = frameTotal - abs(Frame);
	}

	float column = Frame % frameCount.x;
	float row = Frame / frameCount.y;
	float2 coord = float2(column, row);
	coord = coord.xy / frameCount.xy;
	float3 targetPositionWS = SAMPLE_TEXTURE2D_LOD(PositionsTex.tex, PositionsTex.samplerstate, coord, 0).xyz;

	// Next frame interpolation, useful when using low delay value
	float nextFrame = (Frame + 1.0) % frameTotal;
	column = nextFrame % frameCount.x;
	row = nextFrame / frameCount.y;
	coord = float2(column, row);
	coord = coord.xy / frameCount.xy;
	targetPositionWS = lerp(targetPositionWS, SAMPLE_TEXTURE2D_LOD(PositionsTex.tex, PositionsTex.samplerstate, coord, 0).xyz, frac(column));

	PositionWS = targetPositionWS - TargetPositionWS + VertexWS;
}

void Turbulence_float(in float Time, in float3 VertexOS, in UnityTexture2D TurbulenceTex, in float2 UV, in float2 Tiling, in float2 Speed, in float Strength, out float3 PositionOS)
{
	float2 coord = UV * Tiling + (Speed * Time);
	float3 turbulence = SAMPLE_TEXTURE2D_LOD(TurbulenceTex.tex, TurbulenceTex.samplerstate, coord, 0).rgb;
	turbulence = (turbulence - 0.5) * 2.0;
	turbulence *= Strength;
	PositionOS = VertexOS + turbulence;
}

void AxisLock_float(in float3 VertexOS, in float3 ObjectPositionWS, in float3 ScaleWS, out float3 PositionOS)
{
	PositionOS = TransformWorldToObject(GetCameraRelativePositionWS((VertexOS * ScaleWS + ObjectPositionWS)));
}

/***  CHESS PIECE RELATED FUNCTIONS ***/

void Erosion_float(in float Value, in float Offset, in float Smoothness, out float Factor)
{
	Smoothness = max(0.0001, Smoothness);
	float min = 0.0 - Smoothness;
	float max = 1.0 + Smoothness;
	float offset = min + (Offset) * (max - min);
	Factor = smoothstep(offset + Smoothness * 0.5, offset - Smoothness * 0.5, Value);
}

/***  HEAT RELATED FUNCTIONS ***/

void DistortionFlow_float(in UnityTexture2D DistortionTex, in float2 DistortionUV, in UnityTexture2D FlowTex, in float2 FlowUV, in float Speed, out float2 Displacement)
{
	float2 flowDir = SAMPLE_TEXTURE2D(FlowTex.tex, FlowTex.samplerstate, FlowUV).xy * 2.0f - 1.0f;
	flowDir *= Speed;

	float phase0 = frac(_Time.y * 0.5f + 0.5f);
	float phase1 = frac(_Time.y * 0.5f + 1.0f);

	float3 tex0 = SAMPLE_TEXTURE2D(DistortionTex.tex, DistortionTex.samplerstate, DistortionUV + flowDir.xy * phase0).xyz;
	float3 tex1 = SAMPLE_TEXTURE2D(DistortionTex.tex, DistortionTex.samplerstate, DistortionUV + flowDir.xy * phase1).xyz;

	float factor = abs((0.5f - phase0) / 0.5f);
	Displacement = lerp(tex0, tex1, factor).xy;
}

// Those functions are not used for now and are related to heat distortion
void Turbulence02_float(in float3 VertexOS, float3 NormalOS, in UnityTexture2D TurbulenceTex, in float2 UV, in float2 Tiling, in float2 Speed, in float Strength, out float3 PositionOS)
{
	float2 coord = UV * Tiling + (Speed * _Time.y);
	float3 turbulence = SAMPLE_TEXTURE2D_LOD(TurbulenceTex.tex, TurbulenceTex.samplerstate, coord, 0).rgb;
	turbulence = (turbulence - 0.5) * 2.0;
	turbulence *= Strength;
	PositionOS = VertexOS + NormalOS * turbulence;
}

#endif // SMOKEFUNCTIONS_INCLUDED