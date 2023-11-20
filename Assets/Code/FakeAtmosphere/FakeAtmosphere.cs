using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FakeAtmosphere : MonoBehaviour
{
    public static class Uniforms
    {
        public static int _PlanetCenterWS = Shader.PropertyToID("_PlanetCenterWS");
        public static int _AtmosphereRadius = Shader.PropertyToID("_AtmosphereRadius");
        public static int _PlanetRadius = Shader.PropertyToID("_PlanetRadius");
        
        public static int _AtmosphereColors = Shader.PropertyToID("_AtmosphereColors");
        public static int _AtmosphereParams = Shader.PropertyToID("_AtmosphereParams");
       
        public static int _LightPositionWS = Shader.PropertyToID("_LightPositionWS");
        public static int _LightColor = Shader.PropertyToID("_LightColor");
        
    }
    [Serializable]
    public struct FogParams
    {
        public Color fogColor;
        public float densityFalloff;
        public float diffuseWrap;
        public float fogGlancingPower;
        public float fogSilhouettePower;
        public float intensityMultiplier;
        
    }

    public FogParams fog1;
    public FogParams fog2;
    
    public float atmosphereRadius = 1.0f;
    public float planetRadius = 0.9f;
    public Light sun;

    private MaterialPropertyBlock mpb;

    private void OnEnable()
    {
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {

        Vector3 lightPos =  Vector3.zero;
        Vector3 lightColor = Vector3.zero;
        if (sun)
        {
            lightPos = sun.transform.position;
            lightColor.x = sun.color.r;
            lightColor.y = sun.color.g;
            lightColor.z = sun.color.b;
            //lightColor *= sun.intensity;
        }

        Vector4 fog1Params = new Vector4(fog1.densityFalloff, fog1.diffuseWrap, fog1.fogGlancingPower, fog1.intensityMultiplier );
        Vector4 fog2Params = new Vector4(fog1.fogSilhouettePower, 0.0f, 0.0f, 0.0f );
        
        Vector4 fog3Params = new Vector4(fog2.densityFalloff, fog2.diffuseWrap, fog2.fogGlancingPower, fog2.intensityMultiplier );
        Vector4 fog4Params = new Vector4(fog2.fogSilhouettePower, 0.0f, 0.0f, 0.0f );

        Vector4[] paramsArray = {fog1Params, fog2Params, fog3Params, fog4Params};
        Vector4[] colorsArray = {fog1.fogColor, fog2.fogColor};
        
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.GetPropertyBlock(mpb);
        mpb.SetVector(Uniforms._PlanetCenterWS, transform.position);
        mpb.SetFloat(Uniforms._AtmosphereRadius, atmosphereRadius);
        mpb.SetFloat(Uniforms._PlanetRadius, planetRadius);
        mpb.SetVectorArray(Uniforms._AtmosphereParams, paramsArray);
        mpb.SetVectorArray(Uniforms._AtmosphereColors, colorsArray);
        mpb.SetVector(Uniforms._LightPositionWS, lightPos);
        mpb.SetVector(Uniforms._LightColor, lightColor);
        mr.SetPropertyBlock(mpb);
    }
}
