#ifndef INCLUDED_LIT_SPECIAL2_HLSL
#define INCLUDED_LIT_SPECIAL2_HLSL

#include "Packages/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"

// #if !defined(HAS_LIGHTLOOP)
// 	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"
// #endif

StructuredBuffer<float4x4> _MatricesBuffer;
StructuredBuffer<float4x4> _MatricesIBuffer;

StructuredBuffer<float4x4> _MatricesWTWBuffer;
StructuredBuffer<float4x4> _MatricesWTWITBuffer;

uint _ObjectID;
float4 _ProbeKillPositionRadius;

void GetOverrideObjectID(inout uint objectID)
{
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
	objectID = _ObjectID;
#endif
}

// int LoadObjectID(in float2 positionSS)
// {
// 	int objectID = 0;
// 	
// #if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
// 	if (_EnableLightLayers)
// 	{
// 		float4 inGBuffer4 = LOAD_TEXTURE2D_X(_LightLayersTexture, positionSS);
// 		objectID = inGBuffer4.r;
// 	}
// #endif
//
// 	return objectID;
// }

void RetransformLookup(in uint objectID, in float3 positionRWS, inout float3 normalWS, out float3 positionWS)
{
	if(objectID > 0)
	{
		objectID -= 1;

		// Usable for non-deferred lookup experiments
		//positionWS = mul( _MatricesBuffer[objectID], float4(TransformWorldToObject(positionRWS), 1.f) ).xyz;
		//normalWS = normalize( mul( TransformWorldToObjectNormal(normalWS), (float3x3)_MatricesIBuffer[objectID] ) );
		
		positionWS = mul(_MatricesWTWBuffer[objectID], float4(GetAbsolutePositionWS(positionRWS), 1.f)).xyz;
		normalWS = normalize(mul((float3x3)_MatricesWTWITBuffer[objectID], normalWS));
	}
	else
	{
		positionWS = GetAbsolutePositionWS(positionRWS);
	}
}

bool ShouldKillProbe(float3 positionWS)
{
	float3 probeKillDelta = _ProbeKillPositionRadius.xyz - positionWS;
	return dot(probeKillDelta, probeKillDelta) < _ProbeKillPositionRadius.w;
}

void OverrideEvaluateAdaptiveProbeVolume(in uint objectID, in float3 positionRWS, in float3 normalWS, in float3 backNormalWS, in float3 reflDir, in float3 viewDir, in float2 positionSS,
	out float3 bakeDiffuseLighting, out float3 backBakeDiffuseLighting, out float3 lightingInReflDir)
{
	float3 positionWS;
	RetransformLookup(objectID, positionRWS, normalWS, positionWS);
	
	if(ShouldKillProbe(positionWS))
	{
		bakeDiffuseLighting = backBakeDiffuseLighting = float3(0, 0, 0);
		lightingInReflDir = -1;
		return;
	}

	EvaluateAdaptiveProbeVolume(positionWS, normalWS, -normalWS, reflDir, viewDir, positionSS,
		bakeDiffuseLighting, backBakeDiffuseLighting, lightingInReflDir);
}

void OverrideEvaluateAdaptiveProbeVolume(in uint objectID, in float3 positionRWS, in float3 normalWS, in float3 backNormalWS, in float3 viewDir, in float2 positionSS, 
	out float3 bakeDiffuseLighting, out float3 backBakeDiffuseLighting)
{
	float3 positionWS;
	RetransformLookup(objectID, positionRWS, normalWS, positionWS);

	if(ShouldKillProbe(positionWS))
	{
		bakeDiffuseLighting = backBakeDiffuseLighting = float3(0, 0, 0);
        return;
	}

	EvaluateAdaptiveProbeVolume(positionWS, normalWS, normalWS, viewDir, positionSS, bakeDiffuseLighting, backBakeDiffuseLighting);
}

void OverrideEvaluateAdaptiveProbeVolume(in float3 posWS, in float2 positionSS, out float3 bakeDiffuseLighting)
{
	if(ShouldKillProbe(posWS))
	{
		bakeDiffuseLighting = float3(0, 0, 0);
		return;
	}

	EvaluateAdaptiveProbeVolume(posWS, positionSS, bakeDiffuseLighting);
}

bool GetOverrideBakedLighting(float3 positionRWS, float3 normalWS, uint renderingLayers, out float3 bakeDiffuseLighting, out float3 backBakeDiffuseLighting, out bool isLightmap)
{
	uint objectID = _ObjectID;
	
	if(objectID == 0)
	{
		bakeDiffuseLighting = backBakeDiffuseLighting = float3(0, 0, 0);
		isLightmap = false;
		return false;
	}
	
	--objectID;
	
	// TODO: Consider uploading a single new-world-to-old-world transform
	
	float3 positionWS = mul( _MatricesBuffer[objectID], float4(TransformWorldToObject(positionRWS), 1.f) ).xyz;
	normalWS = normalize( mul( TransformWorldToObjectNormal(normalWS), (float3x3)_MatricesIBuffer[objectID] ) );
	
	float3 ViewDirWS = float3(0, 0, 0);
	APVSample apvSample = SampleAPV(positionWS, normalWS, ViewDirWS);
	if(apvSample.status == APV_SAMPLE_STATUS_INVALID)
	{
		//bakeDiffuseLighting = backBakeDiffuseLighting = float3(1000000, 0, 0);
		bakeDiffuseLighting = backBakeDiffuseLighting = float3(0, 0, 0);
		isLightmap = false;
		return false;
	}

	EvaluateAdaptiveProbeVolume(apvSample, normalWS, -normalWS, bakeDiffuseLighting, backBakeDiffuseLighting);
	isLightmap = true;
	return true;
}

#endif //INCLUDED_LIT_SPECIAL2_HLSL
