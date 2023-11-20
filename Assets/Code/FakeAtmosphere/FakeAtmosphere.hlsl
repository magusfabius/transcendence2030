#ifndef __FAKEATMOSPHERE_HLSL__
#define __FAKEATMOSPHERE_HLSL__

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"

#define ATMOSPHERE_COLORS_ARRAY_COUNT 2
#define ATMOSPHERE_PARAMS_ARRAY_COUNT 4

float3 _PlanetCenterWS;
float _PlanetRadius;
float _AtmosphereRadius;
float4 _AtmosphereColors[ATMOSPHERE_COLORS_ARRAY_COUNT];
float4 _AtmosphereParams[ATMOSPHERE_PARAMS_ARRAY_COUNT];

float3 _LightPositionWS;
float3 _LightColor;

//#pragma enable_d3d11_debug_symbols

struct FogParams
{
    float falloff;
    float wrap;
    float glancingPower;
    float intensity;
    float fogSilhouettePower;
};

FogParams GetFogParams(int fogIndex)
{
    FogParams p;
    float4 param1 = _AtmosphereParams[fogIndex * 2];
    float4 param2 = _AtmosphereParams[fogIndex * 2 + 1];
    p.falloff = param1.x;
    p.wrap = param1.y;
    p.glancingPower = param1.z;
    p.intensity = param1.w;
    p.fogSilhouettePower = param2.x;
    return p;
}

float4 GetFogColor(int fogIndex)
{
    return _AtmosphereColors[fogIndex];
}

void CalculateAtmosphericScattering(float3 inPositionWS, float3 inCamPosWS, out float4 outColor)
{
    float3 rayOrigin = inCamPosWS;
    float3 rayDirection = normalize(inPositionWS - inCamPosWS);
    float2 intersections;

    float3 planetOriginToCamera = rayOrigin - _PlanetCenterWS;
    

    if(!IntersectRaySphere(planetOriginToCamera, rayDirection, _AtmosphereRadius, intersections))
    {
        outColor = 0.f;
        return;
    }

    float2 intersectionsPlanet;
    bool planetHit = IntersectRaySphere(planetOriginToCamera, rayDirection, _PlanetRadius, intersectionsPlanet);

    float atmosphereWidth = _AtmosphereRadius - _PlanetRadius;

    float3 pointOnSphere = rayOrigin + rayDirection * intersections.x;
    float3 sphereNormal = normalize(pointOnSphere - _PlanetCenterWS);

    float tDisk = max(0.0f, dot(rayDirection, _PlanetCenterWS - inCamPosWS));
    float3 pointOnDisk = rayOrigin + rayDirection * tDisk;

    float distAtmosphere = (length(pointOnDisk - _PlanetCenterWS) - _PlanetRadius) / atmosphereWidth;

    float fogVisibleDistance = planetHit ? intersectionsPlanet.x -  max(0, intersections.x): intersections.y - max(0, intersections.x);
    
    float wrappedFogMantissa = 1.0 - saturate(dot(-rayDirection, sphereNormal));    

    float3 toLight = normalize(_LightPositionWS - inPositionWS);
    float dotNL = saturate(dot(sphereNormal, toLight));
    
    float3 planetOriginToCameraDir = normalize(planetOriginToCamera);
    float3 lightDirSilhouette = normalize(toLight - dot(planetOriginToCameraDir, toLight) * planetOriginToCameraDir);
    float silhouetteFactor = (1.f + dot(lightDirSilhouette,sphereNormal)) * 0.5f;

    float atmosphereMask = distAtmosphere > 1.f ? 0.f : 1.f;
    
    float4 atmosphereColor = 0;
    
    for(int i = 0; i < 2; ++i)
    {
        FogParams fogParams = GetFogParams(i);
        float4 fogColor = GetFogColor(i);
        float falloff = fogParams.falloff;
        float wrap = fogParams.wrap;
        float glancingPower = fogParams.glancingPower;
        float intensity = fogParams.intensity;
        float silhouettePower = fogParams.fogSilhouettePower;
        
        float att = exp2(-fogVisibleDistance * fogColor.a);
        
        float fogThickness = planetHit  ? fogColor.a  : exp2(-max(0 , distAtmosphere) * falloff);
        
        float fogAmount = (1.f - att) * fogThickness;
        float wrappedFog = pow(wrappedFogMantissa, glancingPower);
        float dotNLWrap = max(0.0, (dotNL + wrap) / (1.0 + wrap));
        //atmosphereIntensity *= silhouetteMask;
        float silhouette = pow(silhouetteFactor, silhouettePower);

        

        atmosphereColor += fogColor * fogAmount * dotNLWrap * wrappedFog * intensity * silhouette;
        
    }

    atmosphereColor.xyz *= _LightColor * atmosphereMask;

    outColor = atmosphereColor;


    
}

void CalculateFakeAtmosphere_float(
in float3 inPositionWS,
in float3 inCamPosWS,
out float4 outResult)
{
    float4 outColor;
    CalculateAtmosphericScattering(inPositionWS, inCamPosWS, outColor);
    outResult = outColor;
}

#endif