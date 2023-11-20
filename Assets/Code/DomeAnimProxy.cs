using UnityEngine;

[ExecuteAlways]
public class DomeAnimProxy : MonoBehaviour
{
    [SerializeField] Renderer source;
    [SerializeField] Renderer target;
    [SerializeField] Material targetMaterial;

    [SerializeField] bool fullLiveUpdate;

    Material[] m_SrcMaterials;
    Material[] m_DstMaterials;
    MaterialPropertyBlock m_PropertyBlock;
    
    static class Uniforms
    {
        internal static int  _SurfaceType = Shader.PropertyToID("_SurfaceType");
        internal static int  _Opaque_RT_only = Shader.PropertyToID("_Opaque_RT_only");
        
        internal static int  _Off = Shader.PropertyToID("_Off");
        internal static int  _Off_Emission_Multiplier = Shader.PropertyToID("_Off_Emission_Multiplier");
        internal static int  _Overall_Transparency = Shader.PropertyToID("_Overall_Transparency");
        internal static int  _Pre_D_Influence = Shader.PropertyToID("_Pre_D_Influence");
        internal static int  _Shockwave_Intensity = Shader.PropertyToID("_Shockwave_Intensity");
        internal static int  Mask_Hardness = Shader.PropertyToID("Mask_Hardness");
        internal static int  Mask_Size = Shader.PropertyToID("Mask_Size");
        internal static int  Resolution_Multiplier = Shader.PropertyToID("Resolution_Multiplier");
        internal static int  Transition_Texture_Influence = Shader.PropertyToID("Transition_Texture_Influence");
        internal static int  _Emission_Multiplier_RT = Shader.PropertyToID("_Emission_Multiplier_RT");
        internal static int  _Sky_Texture_Off_Base_Tint_Color_RT = Shader.PropertyToID("_Sky_Texture_Off_Base_Tint_Color_RT");
    }

    // void OnValidate()
    // {
    //     OnDisable();
    //     OnEnable();
    // }
    
    void OnEnable()
    {

        if (source && target)
        {
            m_DstMaterials = new [] { targetMaterial };
            target.sharedMaterials = m_DstMaterials;
        }
        

        m_PropertyBlock = new MaterialPropertyBlock();
    }

    
    
    void LateUpdate()
    {
        if (source == null || target == null)
            return;

        
        m_PropertyBlock.Clear();
        source.GetPropertyBlock(m_PropertyBlock);
        
        foreach (var material in m_DstMaterials)
        {
            if(m_PropertyBlock.HasFloat(Uniforms._Off)) material.SetFloat(Uniforms._Off, m_PropertyBlock.GetFloat(Uniforms._Off));
            if(m_PropertyBlock.HasFloat(Uniforms._Off_Emission_Multiplier)) material.SetFloat(Uniforms._Off_Emission_Multiplier, m_PropertyBlock.GetFloat(Uniforms._Off_Emission_Multiplier));
            if(m_PropertyBlock.HasFloat(Uniforms._Overall_Transparency)) material.SetFloat(Uniforms._Overall_Transparency, m_PropertyBlock.GetFloat(Uniforms._Overall_Transparency));
            if(m_PropertyBlock.HasFloat(Uniforms._Pre_D_Influence)) material.SetFloat(Uniforms._Pre_D_Influence, m_PropertyBlock.GetFloat(Uniforms._Pre_D_Influence));
            if(m_PropertyBlock.HasFloat(Uniforms._Shockwave_Intensity)) material.SetFloat(Uniforms._Shockwave_Intensity, m_PropertyBlock.GetFloat(Uniforms._Shockwave_Intensity));
            if(m_PropertyBlock.HasFloat(Uniforms.Mask_Hardness)) material.SetFloat(Uniforms.Mask_Hardness, m_PropertyBlock.GetFloat(Uniforms.Mask_Hardness));
            if(m_PropertyBlock.HasFloat(Uniforms.Mask_Size)) material.SetFloat(Uniforms.Mask_Size, m_PropertyBlock.GetFloat(Uniforms.Mask_Size));
            if(m_PropertyBlock.HasFloat(Uniforms.Resolution_Multiplier)) material.SetFloat(Uniforms.Resolution_Multiplier, m_PropertyBlock.GetFloat(Uniforms.Resolution_Multiplier));
            if(m_PropertyBlock.HasFloat(Uniforms.Transition_Texture_Influence)) material.SetFloat(Uniforms.Transition_Texture_Influence, m_PropertyBlock.GetFloat(Uniforms.Transition_Texture_Influence));
            if(m_PropertyBlock.HasFloat(Uniforms._Emission_Multiplier_RT)) material.SetFloat(Uniforms._Emission_Multiplier_RT, m_PropertyBlock.GetFloat(Uniforms._Emission_Multiplier_RT));            
            if(m_PropertyBlock.HasColor(Uniforms._Sky_Texture_Off_Base_Tint_Color_RT)) material.SetColor(Uniforms._Sky_Texture_Off_Base_Tint_Color_RT, m_PropertyBlock.GetColor(Uniforms._Sky_Texture_Off_Base_Tint_Color_RT));
        }
    }
}
