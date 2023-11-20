//UNITY_SHADER_NO_UPGRADE
#ifndef FLAMEFUNCTIONS_INCLUDED
#define FLAMEFUNCTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

void Sine01Remap_float(in float3 In, in float3 Minimum, in float3 Maximum, out float3 Out)
{
	Out = Minimum + 1.0 / 2.0 * (Maximum - Minimum) * (1.0 + sin((In - 0.5) * PI));
}

// Used to apply simple distortion on flame flipbook
void UVDistortionOffset_float(in float Time, in UnityTexture2D NoiseTex, in float2 NoiseUV, in float Strength, in float2 Scale, in float2 Speed, out float2 Out)
{
	float2 tiling = Scale;
	float2 offset = Time * Speed;
	float2 coord = NoiseUV * tiling + offset;
	float2 noise = SAMPLE_TEXTURE2D(NoiseTex.tex, NoiseTex.samplerstate, coord).rg;
	noise = noise * 2.0 - 1.0;
	noise *= Strength;
	Out = noise;
}

// Pivot position when used in a flipbook
void SubUVPivot_float(in float2 Pivot, in float Frame, in float Row, in float Column, out float2 Out)
{
	Frame = floor(Frame);
	// TODO: Handle inverted flipbook behaviors
	float x = ((1.0 / Column) * fmod(Frame, Column)) + (Pivot.x / Column);
	float y = (1.0 - ((1.0 / Row) * floor(Frame / Row))) - (Pivot.y / Row);
	Out = float2(x, y);
}

// Used to apply uv scale transformation on flame flipbook
void UVScale_float(in float Time, in float2 UV, in UnityTexture2D NoiseTex, in float2 NoiseUV, in float2 ScaleOffsetMinimum, in float2 ScaleOffsetMaximum, in float2 ScalePivot, in float Strength, in float2 Speed, out float2 Out)
{
	float2 offset = Time * Speed;
	float2 coord = NoiseUV + offset;
	float2 noise = SAMPLE_TEXTURE2D(NoiseTex.tex, NoiseTex.samplerstate, coord).rg;
	float3 scaleOffset = float3(0.0, 0.0, 0.0);
	Sine01Remap_float(float3(noise.xy, 0.0), float3(ScaleOffsetMinimum.xy, 0.0), float3(ScaleOffsetMaximum.xy, 0.0), scaleOffset);
	scaleOffset.xy *= Strength;
	scaleOffset.xy = float2(1.0, 1.0) + scaleOffset.xy;
	float2x2 scaleMatrix = float2x2(scaleOffset.x, 0.0,
									0.0, scaleOffset.y);

	UV = UV - ScalePivot;
	UV = mul(scaleMatrix, UV);
	UV = UV + ScalePivot;
	Out = UV;
}

// Used to apply some object space vertex position offset to have the flame jittering effect
void VertexPositionOffset_float(in float Time, in UnityTexture2D NoiseTex, in float2 NoiseUV, in float3 OffsetMinimum, in float3 OffsetMaximum, in float2 Speed, out float3 Out)
{
	float2 offset = Time * Speed;
	float2 coord = NoiseUV + offset;
	float3 noise = SAMPLE_TEXTURE2D_LOD(NoiseTex.tex, NoiseTex.samplerstate, coord, 0).rgb;
	float3 vertexPositionOffset = float3(0.0, 0.0, 0.0);
	Sine01Remap_float(noise, OffsetMinimum, OffsetMaximum, vertexPositionOffset);
	Out = vertexPositionOffset;
}

// Runtime optical flow
void RuntimeOpticalFlow_float(in float2 UVA, in float2 UVB, in float Lerp, in float o_FlowScale, in float Index, in float g_sampleDistance, in float g_sampleOffset, in float LOD, in UnityTexture2D Tex,
	out float2 FirstUV, out float2 SecondUV, out float Blend)
{
	// TODO: This is temporary from Naughty Dog. Maybe use pre-computed flowmaps
	Index = floor(Index) % 3;

	float2 pFrameUV = UVA;
	float2 cFrameUV = UVB;
	float3 frameDiff = Lerp;
	float4 frameDiffRGB = SAMPLE_TEXTURE2D(Tex.tex, Tex.samplerstate, cFrameUV) - SAMPLE_TEXTURE2D(Tex.tex, Tex.samplerstate, pFrameUV);

	float offset = g_sampleDistance;
	float2 offsetX = float2(offset, 0.0);
	float2 offsetY = float2(0.0, offset);

	float4 gradientX =
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, cFrameUV + offsetX, LOD) -
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, cFrameUV - offsetX, LOD) +
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, pFrameUV + offsetX, LOD) -
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, pFrameUV - offsetX, LOD);

	float4 gradientY =
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, cFrameUV + offsetY, LOD) -
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, cFrameUV - offsetY, LOD) +
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, pFrameUV + offsetY, LOD) -
		SAMPLE_TEXTURE2D_LOD(Tex.tex, Tex.samplerstate, pFrameUV - offsetY, LOD);

	float4 gradientMag = sqrt((gradientX * gradientX) + (gradientY * gradientY)) + g_sampleOffset;

	float4 velocityX_p = (frameDiffRGB) * (frameDiff.xxxx) * (gradientX / gradientMag) * o_FlowScale;
	float4 velocityY_p = (frameDiffRGB) * (frameDiff.xxxx) * (gradientY / gradientMag) * o_FlowScale;
	float4 velocityX_c = (frameDiffRGB) * (1 - frameDiff.xxxx) * (gradientX / gradientMag) * o_FlowScale;
	float4 velocityY_c = (frameDiffRGB) * (1 - frameDiff.xxxx) * (gradientY / gradientMag) * o_FlowScale;

	float2 pFrameUV_flow = pFrameUV + float2(velocityX_p[Index], velocityY_p[Index]);
	float2 cFrameUV_flow = cFrameUV - float2(velocityX_c[Index], velocityY_c[Index]);
	//float4 color1 = SAMPLE_TEXTURE2D(Tex.tex, Tex.sampleState, pFrameUV_flow);
	//float4 color2 = SAMPLE_TEXTURE2D(Tex.tex, Tex.sampleState, cFrameUV_flow);
	//float4 color = lerp(color1, color2, frameDiff.x);

	FirstUV = pFrameUV_flow;
	SecondUV = cFrameUV_flow;
	Blend = frameDiff.x;
}

#endif // FLAMEFUNCTIONS_INCLUDED