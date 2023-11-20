#ifndef INCLUDED_LIT_SPECIAL3_HLSL
#define INCLUDED_LIT_SPECIAL3_HLSL

#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.hlsl"

uint LoadObjectID(in float2 positionSS)
{
	uint objectID = 0;
	
#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
	if (_EnableLightLayers)
	{
		float4 inGBuffer4 = LOAD_TEXTURE2D_X(_LightLayersTexture, positionSS);
		objectID = (UnpackByte(inGBuffer4.g) << 8) | UnpackByte(inGBuffer4.r);
	}
#endif

	return objectID;
}

bool GetOverrideLighting(PositionInputs posInput, out float3 color)
{
	GBufferType2 gbuffer2 = LOAD_TEXTURE2D_X(_GBufferTexture2, posInput.positionSS);
	float coatMask; uint materialFeatureId;
	UnpackFloatInt8bit(gbuffer2.a, 8, coatMask, materialFeatureId);
	bool pixelHasSubsurface = materialFeatureId == GBUFFER_LIT_TRANSMISSION_SSS || materialFeatureId == GBUFFER_LIT_SSS;

	float specularOcclusion; bool isLightmap;
	if (pixelHasSubsurface)
	{
		specularOcclusion = gbuffer2.r;
	}
	else
	{
		GBufferType0 gbuffer0 = LOAD_TEXTURE2D_X(_GBufferTexture0, posInput.positionSS);
		specularOcclusion = gbuffer0.a;
	}
	UnpackFloatInt8bit(specularOcclusion, 128, specularOcclusion, isLightmap);

	if(isLightmap)
	{
		GBufferType3 gbuffer3 = LOAD_TEXTURE2D_X(_GBufferTexture3, posInput.positionSS);
		color = gbuffer3.rgb;
		return true;
	}

	return false;
}
#endif //INCLUDED_LIT_SPECIAL3_HLSL
