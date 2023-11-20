using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class ShadowsControlTarget : MonoBehaviour
{
    public enum Mode { Full, Sub }
    
    public int LastRenderedFrameUpdated { get; private set; }
    public Mode LastUpdateMode { get; private set; }
    public int LastUpdateSubIndex { get; private set; }
    
    [SerializeField] Light light;
    [SerializeField] HDAdditionalLightData hdAdditionalLightData;
    [SerializeField] int cascadesCount;
    [SerializeField] int updateFrequency;
    [SerializeField] int updateOffset;
    [SerializeField] bool hasBeenReset;
    
    [SerializeField] int shadowResolutionBias;
    [SerializeField] float shadowResolutionDivisor;

    [SerializeField] bool shadowDisable;
    [SerializeField] bool shadowForceUpdate;

    ShadowUpdateMode m_OriginalShadowUpdateMode;
    bool m_OriginalUpdateUponLightMovement;
    int m_OriginalShadowLevel;
    int m_OriginalShadowResolution;
    bool m_OriginalShadowEnable;

    private Dictionary<Camera, int> m_NextUpdateIndices = new();
    
    void Reset()
    {
        light = GetComponent<Light>();
        hdAdditionalLightData = GetComponent<HDAdditionalLightData>();
        cascadesCount = 2;
        updateFrequency = 1;
        updateOffset = 0;
        hasBeenReset = true;
    }

    void OnEnable()
    {
        if (hdAdditionalLightData == null)
        {
            if (!hasBeenReset)
                Reset();

            if (hdAdditionalLightData == null)
            {
                enabled = false;
                return;
            }
        }
        
        if (shadowDisable && light.shadows != LightShadows.None)
        {
            m_OriginalShadowEnable = true;
            hdAdditionalLightData.EnableShadows(false);
            Debug.Log($"[ShadowControlTarget] Enable: In '{name}' setting {hdAdditionalLightData.name} shadows disabled.");
        }

        if (updateFrequency != 0 && updateOffset != 0)
        {
            m_OriginalShadowUpdateMode = hdAdditionalLightData.shadowUpdateMode; 
            m_OriginalUpdateUponLightMovement = hdAdditionalLightData.updateUponLightMovement = false;
            hdAdditionalLightData.shadowUpdateMode = ShadowUpdateMode.OnDemand;
            hdAdditionalLightData.updateUponLightMovement = false;
            Debug.Log($"[ShadowControlTarget] Enable: In '{name}' setting {hdAdditionalLightData.name} OnDemand, count {cascadesCount}, freq {updateFrequency}, offset {updateOffset}.");
        }

        if (shadowResolutionBias != 0)
        {
            if(hdAdditionalLightData.shadowResolution.level < 0)
                throw new UnityException($"[ShadowControlTarget] : Cannot bias shadow resolution in '{hdAdditionalLightData.name}' because it uses a Custom setting.");
            
            m_OriginalShadowLevel = hdAdditionalLightData.shadowResolution.level;
            hdAdditionalLightData.shadowResolution.level = Mathf.Clamp(hdAdditionalLightData.shadowResolution.level + shadowResolutionBias, 0, 3) ;
            Debug.Log($"[ShadowControlTarget] Enable: In '{name}' setting {hdAdditionalLightData.name}.shadowResolution.level = {hdAdditionalLightData.shadowResolution.level} (stashed previous value {m_OriginalShadowLevel}).");
        }
        
        if (shadowResolutionDivisor != 0f && shadowResolutionDivisor != 1f)
        {
            if(!hdAdditionalLightData.shadowResolution.useOverride)
                throw new UnityException($"[ShadowControlTarget] : Cannot scale shadow resolution in '{hdAdditionalLightData.name}' because it uses a quality preset.");
            
            m_OriginalShadowResolution = hdAdditionalLightData.shadowResolution.@override;
            hdAdditionalLightData.shadowResolution.@override = Mathf.RoundToInt(hdAdditionalLightData.shadowResolution.@override / shadowResolutionDivisor);
            Debug.Log($"[ShadowControlTarget] Enable: In '{name}' setting {hdAdditionalLightData.name}.shadowResolution.override = {hdAdditionalLightData.shadowResolution.@override} (stashed previous value {m_OriginalShadowResolution}).");
        }
        
        if(hdAdditionalLightData != null && hdAdditionalLightData.shadowUpdateMode == ShadowUpdateMode.OnDemand)
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

        m_NextUpdateIndices.Clear();

        if (hdAdditionalLightData)
        {
            if (m_OriginalShadowEnable)
            {
                hdAdditionalLightData.EnableShadows(true);
                Debug.Log($"[ShadowControlTarget] Disable: In '{name}' setting {hdAdditionalLightData.name} shadows enabled.");
            }

            if (updateFrequency != 0 && updateOffset != 0)
            {
                hdAdditionalLightData.updateUponLightMovement = m_OriginalUpdateUponLightMovement;
                hdAdditionalLightData.shadowUpdateMode = m_OriginalShadowUpdateMode;
                Debug.Log($"[ShadowControlTarget] Disable: In '{name}' reverting {hdAdditionalLightData.name} OnDemand to {m_OriginalShadowUpdateMode}.");
            }

            if (shadowResolutionBias != 0)
            {
                hdAdditionalLightData.shadowResolution.level = m_OriginalShadowLevel;
                Debug.Log($"[ShadowControlTarget] Disable: In '{name}' reverting {hdAdditionalLightData.name}.shadowResolution.level = {m_OriginalShadowLevel}.");
            }
            
            if (shadowResolutionDivisor != 0)
            {
                hdAdditionalLightData.shadowResolution.@override = m_OriginalShadowResolution;
                Debug.Log($"[ShadowControlTarget] Disable: In '{name}' reverting {hdAdditionalLightData.name}.shadowResolution.override = {m_OriginalShadowResolution}.");
            }
        }
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Nothing to update
        if (hdAdditionalLightData == null || hdAdditionalLightData.shadowUpdateMode != ShadowUpdateMode.OnDemand)
            return;

#if false
        // Always update shadows when frame debugger is active 
        if (FrameDebugger.enabled)
        {
            hdAdditionalLightData.RequestShadowMapRendering();
            return;
        }
#endif
        
        // Always fully update shadow cascades outside of game view
        if (light.type == LightType.Directional && camera.cameraType != CameraType.Game)
        {
            hdAdditionalLightData.RequestShadowMapRendering();
            return;
        }

        if (!m_NextUpdateIndices.TryGetValue(camera, out var nextUpdateIndex) || shadowForceUpdate)
        {
            // Always run full render first update after enable
            hdAdditionalLightData.RequestShadowMapRendering();

            // Force updates to run with absolute offset and frequency as opposed to relative to activation time.
            nextUpdateIndex = Time.renderedFrameCount;
            
            LastRenderedFrameUpdated = Time.renderedFrameCount;
            LastUpdateMode = Mode.Full;
            LastUpdateSubIndex = int.MaxValue;
        }
        else
        {
            var offsetIndex = nextUpdateIndex + updateOffset;
            var shouldUpdate = offsetIndex % updateFrequency == 0;
            if (shouldUpdate)
            {
                var updateIndex = offsetIndex % cascadesCount;
                hdAdditionalLightData.RequestSubShadowMapRendering(updateIndex);
                
                LastRenderedFrameUpdated = Time.renderedFrameCount;
                LastUpdateMode = Mode.Sub;
                LastUpdateSubIndex = updateIndex;
            }
        }

#if true
        // Freeze shadows update when frame debugger is active 
        if (!FrameDebugger.enabled)
#endif
            m_NextUpdateIndices[camera] = ++nextUpdateIndex;
    }
}
