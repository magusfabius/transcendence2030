#ifndef INCLUDED_LIT_SPECIAL_HLSL
#define INCLUDED_LIT_SPECIAL_HLSL

#include "Packages/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"

StructuredBuffer<float4x4> _MatricesBuffer;
StructuredBuffer<float4x4> _MatricesIBuffer;

void UnpackTangentNormalWithScale_float(float4 PackedNormal, float NormalScale, out float3 UnpackedNormal)
{
	UnpackedNormal = UnpackNormalScale(PackedNormal, NormalScale);
}

void UnpackTangentNormalAGWithScale_float(float4 PackedNormal, float NormalScale, out float3 UnpackedNormal)
{
	UnpackedNormal = UnpackNormalAG(PackedNormal, NormalScale);
}

void ApplyDetailAlbedo_float(float DetailMask, float DetailAlbedo, float DetailAlbedoScale, float3 BaseColor, out float3 BaseColorDetail)
{
	// From https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDataIndividualLayer.hlsl#L211

	// Goal: we want the detail albedo map to be able to darken down to black and brighten up to white the surface albedo.
	// The scale control the speed of the gradient. We simply remap detailAlbedo from [0..1] to [-1..1] then perform a lerp to black or white
	// with a factor based on speed.

	// For base color we interpolate in sRGB space (approximate here as square) as it get a nicer perceptual gradient

	float albedoDetailSpeed = saturate(abs(DetailAlbedo) * DetailAlbedoScale);
	albedoDetailSpeed *= albedoDetailSpeed;
	
	float3 baseColorOverlay = lerp(sqrt(BaseColor), (DetailAlbedo < 0.0) ? float3(0.0, 0.0, 0.0) : float3(1.0, 1.0, 1.0), albedoDetailSpeed);
	baseColorOverlay *= baseColorOverlay;

    // Lerp with details mask
    BaseColorDetail = lerp(BaseColor, saturate(baseColorOverlay), DetailMask);
}

void ApplyDetailNormal_float(float DetailMask, float3 DetailNormalTS, float3 BaseNormalTS, out float3 BaseDetailNormalTS)
{
	BaseDetailNormalTS = lerp(BaseNormalTS, BlendNormalRNM(BaseNormalTS, DetailNormalTS), DetailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
}

void ApplyDetailSmoothness_float(float DetailMask, float DetailSmoothness, float DetailSmoothnessScale, float BasePerceptualSmoothness, out float BaseDetailPerceptualSmoothness)
{
	// From https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDataIndividualLayer.hlsl#L238

	// See comment for baseColorOverlay (ApplyDetailAlbedo_float)
	float smoothnessDetailSpeed = saturate(abs(DetailSmoothness) * DetailSmoothnessScale);
	float smoothnessOverlay = lerp(BasePerceptualSmoothness, (DetailSmoothness < 0.0) ? 0.0 : 1.0, smoothnessDetailSpeed);
	// Lerp with details mask
	BaseDetailPerceptualSmoothness = lerp(BasePerceptualSmoothness, saturate(smoothnessOverlay), DetailMask);
}

void CalculateAlphaCutoff_float(out float AlphaCutoff)
{
	// From: https://github.com/Unity-Technologies/Graphics/blob/3efe468ddc17867f0ae0a2b3247cce6c3d83018a/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitData.hlsl#L220
	
#if SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_PREPASS
	AlphaCutoff = _AlphaCutoffPrepass;
#elif SHADERPASS == SHADERPASS_TRANSPARENT_DEPTH_POSTPASS
	AlphaCutoff = _AlphaCutoffPostpass;
#elif (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
	AlphaCutoff = _UseShadowThreshold ? _AlphaCutoffShadow : _AlphaCutoff;
#else
	AlphaCutoff = _AlphaCutoff;
#endif
}

void SampleBakedGI_float(
	float3 PositionRWS,
	float3 NormalWS,
	out float3 BakeDiffuseLighting,
	out float3 BackBakeDiffuseLighting)
{
	BakeDiffuseLighting = BackBakeDiffuseLighting = float3(0, 0, 0);

#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	BakeDiffuseLighting = BackBakeDiffuseLighting = float3(0, 0, 1000000);
#elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
	if(!_EnableProbeVolumes)
	{
		BakeDiffuseLighting = BackBakeDiffuseLighting = float3(0, 1000000, 0);
	}
	else
	{
		float3 positionWS = GetAbsolutePositionWS(PositionRWS);
		float3 ViewDirWS = float3(0, 0, 0);
		
		APVSample apvSample = SampleAPV(positionWS, NormalWS, ViewDirWS);
		if(apvSample.status == APV_SAMPLE_STATUS_INVALID)
			BakeDiffuseLighting = BackBakeDiffuseLighting = float3(1000000, 0, 0);
		else
			EvaluateAdaptiveProbeVolume(apvSample, NormalWS, -NormalWS, BakeDiffuseLighting, BackBakeDiffuseLighting);
	}
#endif
}

void GetOverridePositionRWSNormalWS_float(
	float3 PositionOS,
	float3 PositionRWS,
	float3 NormalOS,
	float3 NormalWS,
	out float3 OverridePositionRWS,
	out float3 OverrideNormalWS)
{
	if(_ObjectID < 0)
	{
		OverridePositionRWS = PositionRWS;
		OverrideNormalWS = NormalWS;
	}
	else
	{
		OverridePositionRWS = GetCameraRelativePositionWS(mul(_MatricesBuffer[(int)_ObjectID], float4(PositionOS, 1.f)).xyz);
		OverrideNormalWS = mul(NormalOS, (float3x3)_MatricesIBuffer[(int)_ObjectID]);
	}
}

void DebugObjectID_float(out float3 ObjectID)
{
	if(_ObjectID < 0)
		ObjectID = float3(0,1,0);
	else
		ObjectID = float3(_ObjectID / 100.f, 0,0);
}

#endif //INCLUDED_LIT_SPECIAL_HLSL
