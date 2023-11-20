#ifndef INCLUDED_OVERRIDEBAKING_HLSL
#define INCLUDED_OVERRIDEBAKING_HLSL

#if !PROBE_VOLUMES_OFF

#include "Packages/com.unity.render-pipelines.core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"

StructuredBuffer<float4x4> _MatricesBuffer;
StructuredBuffer<float4x4> _MatricesIBuffer;

StructuredBuffer<float4x4> _MatricesWTWBuffer;
StructuredBuffer<float4x4> _MatricesWTWITBuffer;

StructuredBuffer<uint2> _InstanceHash;
int _InstanceHashAlignedSize;

// 32 bit Murmur3 hash
uint hash(uint k)
{
	k ^= k >> 16;
	k *= 0x85ebca6b;
	k ^= k >> 13;
	k *= 0xc2b2ae35;
	k ^= k >> 16;
	return k & (_InstanceHashAlignedSize-1);
}

uint hashtable_lookup(uint key)
{
	uint slot = hash(key);
	uint retVal = 0;
	while (true)
	{
		if (_InstanceHash[slot].x == key)
		{
			retVal = _InstanceHash[slot].y;
			break;
		}
		if (_InstanceHash[slot].x == 0)
		{
			break;
		}
		slot = (slot + 1) & (_InstanceHashAlignedSize - 1);
	}
	return retVal;
}

void GetOverrideObjectID(inout uint objectID)
{
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
	objectID = hashtable_lookup(objectID);
#else
	objectID = 0;
#endif
}

void GetOverrideBakedLighting(float3 positionRWS, float3 normalWS, uint renderingLayers, out float3 bakeDiffuseLighting, out float3 backBakeDiffuseLighting, out bool isOverride)
{
	bakeDiffuseLighting = float3(0, 0, 0);
	backBakeDiffuseLighting = float3(0, 0, 0);
	isOverride = false;

	uint objectID = asuint(unity_LODFade.z);
	GetOverrideObjectID(objectID);

	float3 positionWS;
	if(objectID-- > 0)
	{
		isOverride = true;
		positionWS = mul( _MatricesBuffer[objectID], float4(TransformWorldToObject(positionRWS), 1.f) ).xyz;
		normalWS = normalize( mul( TransformWorldToObjectNormal(normalWS), (float3x3)_MatricesIBuffer[objectID] ) );
	}
	else
	{
		positionWS = GetAbsolutePositionWS(positionRWS);

		//DBG
		// bakeDiffuseLighting = backBakeDiffuseLighting = float3(10000, 0, 0);
		// return false;
	}
	
	float3 ViewDirWS = float3(0, 0, 0);
	APVSample apvSample = SampleAPV(positionWS, normalWS, ViewDirWS);
	if(apvSample.status != APV_SAMPLE_STATUS_INVALID)
	{
		EvaluateAdaptiveProbeVolume(apvSample, normalWS, -normalWS, bakeDiffuseLighting, backBakeDiffuseLighting);
	}
}

#endif

void GetOverrideBakedLightingSG_float(float3 positionRWS, float3 normalWS, out float3 bakeDiffuseLighting, out float3 backBakeDiffuseLighting)
{
#if PROBE_VOLUMES_OFF
	bakeDiffuseLighting = float3(0, 0, 0);
	backBakeDiffuseLighting = float3(0, 0, 0);
#else
	uint renderingLayers = 0xFFFFFFFF;;
	bool isOverride = false;
	GetOverrideBakedLighting(positionRWS, normalWS, renderingLayers, bakeDiffuseLighting, backBakeDiffuseLighting, isOverride);
#endif
}

#endif //INCLUDED_OVERRIDEBAKING_HLSL

