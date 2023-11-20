using UnityEngine;
using UnityEngine.Rendering;

public class DynamicResolutionScaler
{
    readonly float kDownResTargetThreshold;
    readonly float kDownResLatency;
    readonly float kDownResStepSize;
    
    readonly float kUpResTargetThreshold;
    readonly float kUpResLatency;
    readonly float kUpResStepSize;

    bool m_TransitionFrame;

    int m_FramesAboveDownResThreshold;
    float m_TimeAboveDownResThreshold;
    int m_FramesBelowUpResThreshold;
    float m_TimeBelowUpResThreshold;
    float m_CurrentResolutionScalePct;
    
    public DynamicResolutionScaler(QualitySettingsData.DynamicResolutionTargetSettings targetSettings)
    {
        var targetMS = 1000f / targetSettings.dynamicResolutionFPSTarget.value;

        kDownResTargetThreshold = (targetMS - targetSettings.downResMSThreshold.value) / 1000f;
        kDownResLatency = targetSettings.downResLatency.value;
        kDownResStepSize = targetSettings.downResStepSize.value;
        
        kUpResTargetThreshold = (targetMS - targetSettings.upResMSThreshold.value) / 1000f;
        kUpResLatency = targetSettings.upResLatency.value;
        kUpResStepSize = targetSettings.upResStepSize.value;

        m_CurrentResolutionScalePct = 0.5f;
        
        DynamicResolutionHandler.SetDynamicResScaler(ScaleResolution);
    }

    float ScaleResolution()
    {
        // TODO: Refine and stabilize this.
        // TODO: Look into scaling bugs
        
        var deltaTime = Time.deltaTime;

        if (deltaTime >= kDownResTargetThreshold)
        {
            if (m_TimeBelowUpResThreshold > 0f && !m_TransitionFrame)
            {
                m_TransitionFrame = true;
                return m_CurrentResolutionScalePct;
            }

            ++m_FramesAboveDownResThreshold;
            m_TimeAboveDownResThreshold += deltaTime;

            m_FramesBelowUpResThreshold = 0;
            m_TimeBelowUpResThreshold = 0f;
            
            m_TransitionFrame = false;
        }
        else if(deltaTime <= kUpResTargetThreshold)
        {
            if (m_TimeAboveDownResThreshold > 0f && !m_TransitionFrame)
            {
                m_TransitionFrame = true;
                return m_CurrentResolutionScalePct;
            }

            m_FramesAboveDownResThreshold = 0;
            m_TimeAboveDownResThreshold = 0f;
            
            ++m_FramesBelowUpResThreshold;
            m_TimeBelowUpResThreshold += deltaTime;
            
            m_TransitionFrame = false;
        }
        
        var resolutionScale = m_CurrentResolutionScalePct;
        
        if (m_TimeAboveDownResThreshold >= kDownResLatency)
        {
            var avgFrameTime = m_TimeAboveDownResThreshold / m_FramesAboveDownResThreshold;
            var deltaFactor = kDownResTargetThreshold / avgFrameTime;
            resolutionScale = Mathf.Clamp01(m_CurrentResolutionScalePct - (1f - deltaFactor) * kDownResStepSize);
        }
        else if (m_TimeBelowUpResThreshold >= kUpResLatency)
        {
            var avgFrameTime = m_TimeBelowUpResThreshold / m_FramesBelowUpResThreshold;
            var deltaFactor = kUpResTargetThreshold / avgFrameTime;
            resolutionScale = Mathf.Clamp01(m_CurrentResolutionScalePct + (deltaFactor - 1f) * kUpResStepSize);
        }

#if false
        if(m_CurrentResolutionScalePct != resolutionScale)
            Debug.Log($"{(resolutionScale > m_CurrentResolutionScalePct ? "UP" : "DOWN")} : DRS : {m_CurrentResolutionScalePct} -> {resolutionScale}");
#endif
        
        return m_CurrentResolutionScalePct = resolutionScale;
    }
}
