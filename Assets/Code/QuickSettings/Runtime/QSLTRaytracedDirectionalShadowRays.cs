using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class QSLTRaytracedDirectionalShadowRays : QuickSettingLogicTag
{
    public int raysCount;
    public HDAdditionalLightData target;

    int m_PreviousRays;
    //bool m_PreviousRTShadows;
    
    public override void Action(bool apply)
    {
        if (!target)
            return;

        if(apply)
        {
            m_PreviousRays = target.numRayTracingSamples;
            //m_PreviousRTShadows = target.useRayTracedShadows;
            
            if(raysCount == 0)
                target.useRayTracedShadows = false;
            else
                target.numRayTracingSamples = raysCount;
            
            Debug.Log($"[QSLTRaytracedDirectionalShadowRays] Apply: Setting {target.name}.numRayTracingSamples = {raysCount} (stashed previous value {m_PreviousRays}).");
        }
        else
        {
            Debug.Log($"[QSLTRaytracedDirectionalShadowRays] Revert: Setting {target.name}.numRayTracingSamples = {m_PreviousRays}.");
            //target.useRayTracedShadows = m_PreviousRTShadows;
            target.numRayTracingSamples = m_PreviousRays;
            m_PreviousRays = 0;
            //m_PreviousRTShadows = false;
        }
    }
}
