using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public partial class QuickSettings : MonoBehaviour
{
    public enum Preset { Off = 0, Any = 1, Low = 2, Medium = 3, High = 4 }
    public enum ConfigPresetNames { Low, Medium, High, Ultra }

    const string kAny = "Any";
    const string kRaster = "Raster";
    const string kRaytrace = "Raytrace";
    const int kCustomQuality = 3;

    public ConfigPresetNames editorPlaymodeDefaultQuality = ConfigPresetNames.Ultra;

    public HDRenderPipelineAsset hdrpRaster;
    public HDRenderPipelineAsset hdrpRaytrace;
    public HDRenderPipelineAsset hdrpXboxSeries;
    public HDRenderPipelineAsset hdrpPlaystation;

    HDRenderPipelineAsset GetHDRPAsset(bool isRaytracing)
    {
#if UNITY_GAMECORE_XBOXSERIES
        return hdrpXboxSeries;
#elif UNITY_PLAYSTATION
        return hdrpPlaystation;
#else
        return isRaytracing ? hdrpRaytrace : hdrpRaster;
#endif
    }
        
    public GameObject general;
    public GameObject ambientOcclusion;
    public GameObject globalIllumination;
    public GameObject reflections;
    public GameObject shadows;
    public GameObject hair;
    public GameObject directionalShadowRaySamples;
    public GameObject targetDisable;
    public GameObject domeLights;
    public GameObject filmGrain;
    public GameObject moBlur;
    public GameObject dof;
    public GameObject lensFlare;

    public Preset defaultGeneral;
    public Preset defaultAO;
    public Preset defaultGI;
    public Preset defaultReflections;
    public Preset defaultShadows;
    public Preset defaultHair;
    public Preset defaultDirectionalShadowRaySamples;
    public Preset defaultTargetDisable;
    public Preset defaultDomeLights;
    public Preset defaultFilmGrain;
    public Preset defaultMoBlur = Preset.High;
    public Preset defaultDoF = Preset.High;
    public Preset defaultLensFlare = Preset.High;

    [SerializeReference]
    public QuickInputs defaultQuickInputs;

    int m_OriginalQualityLevel;
    QualitySettingsData.EngineQualitySettings m_OriginalQualitySettings;
    HDRenderPipelineAsset m_OriginalRenderPipelineAsset;
    DynamicResolutionScaler m_DynamicResolutionScaler;

    public Preset m_CurrentGeneral;
    public Preset m_CurrentAO;
    public Preset m_CurrentGI;
    public Preset m_CurrentReflections;
    public Preset m_CurrentShadows;
    public Preset m_CurrentHair;
    public Preset m_CurrentDirectionalShadowRaySamples;
    public Preset m_CurrentTargetDisable;
    public Preset m_CurrentDomeLights;
    public Preset m_CurrentFilmGrain;
    public Preset m_CurrentMoBlur;
    public Preset m_CurrentDoF;
    public Preset m_CurrentLensFlare;

    QuickInputs m_AppliedQuickInputs;

    HDRenderPipelineAsset m_hdrpInstantiated;
    bool m_hasAppliedHeavySettingsOnce;
    
    public QuickInputs AppliedInputs => m_AppliedQuickInputs;
    
    public static QuickSettings Instance { get; private set; }
    
    void OnEnable()
    {
        Instance = this;

        m_OriginalQualityLevel = QualitySettings.GetQualityLevel();
        m_OriginalQualitySettings = new QualitySettingsData.EngineQualitySettings();
        m_OriginalQualitySettings.ApplyDefaultValues();
        QualitySettingsData.CaptureCurrentQualityToEngineQualitySettings(ref m_OriginalQualitySettings);
        m_OriginalRenderPipelineAsset = (HDRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[QuickSettings] Default RP '{m_OriginalRenderPipelineAsset.name}' default quality '{QualitySettings.names[m_OriginalQualityLevel]}'.");
#endif
        
        Apply();
        
        SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
        ui_controller.ReadSystemCaps += UIReadSystemCaps;
        ui_controller.ReadCurrentSettings += UIReadCurrentSettings;
        ui_controller.WriteChangedSettings += UIWriteChangedSettings;
    }
    
    void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Every time we load a scene, we need to check for tags to apply
        if(Instance)
            Instance.ApplyTags();
    }

    void OnDisable()
    {
        Debug.Assert(Instance == this);
        Instance = null;

        ui_controller.ReadSystemCaps -= UIReadSystemCaps;
        ui_controller.WriteChangedSettings -= UIWriteChangedSettings;
        ui_controller.ReadCurrentSettings -= UIReadCurrentSettings;
        SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;

#if UNITY_EDITOR
        if (QualitySettings.GetQualityLevel() == kCustomQuality)
        {
            QualitySettings.renderPipeline = null;
            QualitySettings.SetQualityLevel(m_OriginalQualityLevel, true);
        }
#endif
    }

    public bool CanDoRaytracing
    {
        get
        {
            var hdrp = (HDRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            return hdrp.currentPlatformRenderPipelineSettings.supportRayTracing;
        }
    }
    
    public void Apply() => Apply(defaultQuickInputs);

    public void Apply(QuickInputs inputs, bool runtime = true, bool applyall = true)
    {
        Debug.Log($"[QuickSettings] Apply: CanDoRayTracing {CanDoRaytracing}  AO: {defaultAO} GI: {defaultGI}  Reflections: {defaultReflections}  Shadows: {defaultShadows}");

        m_CurrentGeneral = defaultGeneral;
        m_CurrentAO = defaultAO;
        m_CurrentGI = defaultGI;
        m_CurrentReflections = defaultReflections;
        m_CurrentShadows = defaultShadows;
        m_CurrentHair = defaultHair;
        m_CurrentDirectionalShadowRaySamples = defaultDirectionalShadowRaySamples;
        m_CurrentTargetDisable = defaultTargetDisable;
        m_CurrentDomeLights = defaultDomeLights;
        m_CurrentFilmGrain = defaultFilmGrain;
        m_CurrentMoBlur = defaultMoBlur;
        m_CurrentDoF = defaultDoF;
        m_CurrentLensFlare = defaultLensFlare;

        var canDoRayTracing = CanDoRaytracing;
        bool canDoRayTracingAO = canDoRayTracing, canDoRayTracingGI = canDoRayTracing, canDoRayTracingReflections = canDoRayTracing, canDoRayTracingShadows = canDoRayTracing;

        if (inputs != null && inputs.anyOverride)
        {
            if (inputs.presetQualityGeneral.overrideState)
                m_CurrentGeneral = inputs.presetQualityGeneral.value;
            if (inputs.presetQualityAO.overrideState)
                m_CurrentAO = inputs.presetQualityAO.value;
            if (inputs.presetQualityGI.overrideState)
                m_CurrentGI = inputs.presetQualityGI.value;
            if (inputs.presetQualityReflections.overrideState)
                m_CurrentReflections = inputs.presetQualityReflections.value;
            if (inputs.presetQualityShadows.overrideState)
                m_CurrentShadows = inputs.presetQualityShadows.value;
            if (inputs.presetQualityHair.overrideState)
                m_CurrentHair = inputs.presetQualityHair.value;
            if (inputs.presetQualityDirectionalShadowRays.overrideState)
                m_CurrentDirectionalShadowRaySamples = inputs.presetQualityDirectionalShadowRays.value;
            if (inputs.presetQualityTargetDisable.overrideState)
                m_CurrentTargetDisable = inputs.presetQualityTargetDisable.value;
            if (inputs.presetQualityDomeLights.overrideState)
                m_CurrentDomeLights = inputs.presetQualityDomeLights.value;
            if (inputs.presetQualityFilmGrain.overrideState)
                m_CurrentFilmGrain = inputs.presetQualityFilmGrain.value;
            if (inputs.presetQualityMoBlur.overrideState)
                m_CurrentMoBlur = inputs.presetQualityMoBlur.value;
            if (inputs.presetQualityDoF.overrideState)
                m_CurrentDoF = inputs.presetQualityDoF.value;
            if (inputs.presetQualityLensFlare.overrideState)
                m_CurrentLensFlare = inputs.presetQualityLensFlare.value;

            canDoRayTracing &= !inputs.rtDisallowedAll;
            canDoRayTracingAO &= canDoRayTracing && !inputs.rtDisallowedAO;
            canDoRayTracingGI &= canDoRayTracing && !inputs.rtDisallowedGI;
            canDoRayTracingReflections &= canDoRayTracing && !inputs.rtDisallowedReflections;
            canDoRayTracingShadows &= canDoRayTracing && !inputs.rtDisallowedShadows;
        }

        if(applyall) Apply(canDoRayTracing, general, m_CurrentGeneral);
        Apply(canDoRayTracingAO, ambientOcclusion, m_CurrentAO);
        Apply(canDoRayTracingGI, globalIllumination, m_CurrentGI);
        Apply(canDoRayTracingReflections, reflections, m_CurrentReflections);
        Apply(canDoRayTracingShadows, shadows, m_CurrentShadows);
        if(applyall) Apply(canDoRayTracing, hair, m_CurrentHair);
        Apply(canDoRayTracingShadows, directionalShadowRaySamples, m_CurrentDirectionalShadowRaySamples);
        if(applyall) Apply(canDoRayTracing, targetDisable, m_CurrentTargetDisable);
        Apply(canDoRayTracing, domeLights, m_CurrentDomeLights);
        Apply(canDoRayTracing, filmGrain, m_CurrentFilmGrain);
        Apply(canDoRayTracing, moBlur, m_CurrentMoBlur);
        Apply(canDoRayTracing, dof, m_CurrentDoF);
        Apply(canDoRayTracing, lensFlare, m_CurrentLensFlare);

        if (Application.isPlaying && runtime)
            ApplyRuntime(inputs);
        
        ApplyTags(applyall);

        // Collect after change as resources can be re-allocated
        System.GC.Collect();
    }

    void ApplyRuntime(QuickInputs inputs)
    {
        Debug.Log($"[QuickSettings] ApplyRuntime: IsRayTracing {CanDoRaytracing}  inputs: {inputs} ({inputs?.anyOverride ?? false})");

        m_AppliedQuickInputs = inputs;
        
        var isRaytracing = CanDoRaytracing;
        
        if(!m_hasAppliedHeavySettingsOnce)
            m_hdrpInstantiated = Instantiate(GetHDRPAsset(isRaytracing));

        var qualitySettings = (QualitySettingsData.EngineQualitySettings)m_OriginalQualitySettings.Clone();
        var drsTargetSettings = (QualitySettingsData.DynamicResolutionTargetSettings)null;
        
        if (inputs != null && inputs.anyOverride)
        {
            if (inputs.masterTextureLimit.overrideState)
            {
                qualitySettings.textureQuality.value = (QualitySettingsData.TextureResolution)inputs.masterTextureLimit.value;
                Debug.Log($"..masterTextureLimit {inputs.masterTextureLimit.value}");
            }

            if (inputs.anisotropicFiltering.overrideState)
            {
                qualitySettings.anisotropicFiltering.value = inputs.anisotropicFiltering.value;
                Debug.Log($"..anisotropicFiltering {inputs.anisotropicFiltering.value}");
            }

            if (inputs.anisotropicFilteringForcedMin.overrideState || inputs.anisotropicFilteringGlobalMax.overrideState)
            {
                var forcedMin = inputs.anisotropicFilteringForcedMin.overrideState ? inputs.anisotropicFilteringForcedMin.value : -1;
                var globalMax = inputs.anisotropicFilteringGlobalMax.overrideState ? inputs.anisotropicFilteringGlobalMax.value : -1;
                Debug.Log($"..globalAnisotropicFilteringLimits {forcedMin},{globalMax}");
                Texture.SetGlobalAnisotropicFilteringLimits(forcedMin, globalMax);
            }

            qualitySettings.vsync.Override((QualitySettingsData.VSyncCount)inputs.vsync);

            if (inputs.screenResolutionSettings.overrideState)
            {
                var screenWidth = Screen.width;
                var screenHeight = Screen.height;
                var screenRefreshRate = Screen.currentResolution.refreshRate;
                var screenFullScreenMode = Screen.fullScreenMode;
                
                var screenResolutionSettings = inputs.screenResolutionSettings.value;
                if (screenResolutionSettings.screenWidth.overrideState)
                    screenWidth = screenResolutionSettings.screenWidth.value; 
                if (screenResolutionSettings.screenHeight.overrideState)
                    screenHeight = screenResolutionSettings.screenHeight.value; 
                if (screenResolutionSettings.preferredRefreshRate.overrideState)
                    screenRefreshRate = screenResolutionSettings.preferredRefreshRate.value;
                if (screenResolutionSettings.fullScreenMode.overrideState)
                    screenFullScreenMode = screenResolutionSettings.fullScreenMode.value;
                
                if(screenWidth != Screen.width || screenHeight != Screen.height || screenRefreshRate != Screen.currentResolution.refreshRate || screenFullScreenMode != Screen.fullScreenMode)
                    Screen.SetResolution(screenWidth, screenHeight, screenFullScreenMode, screenRefreshRate);
            }
            
            var settings = m_hdrpInstantiated.currentPlatformRenderPipelineSettings;

            if (inputs.dynamicResolutionSettings.overrideState)
            {
                settings.dynamicResolutionSettings = inputs.dynamicResolutionSettings.value;
                Debug.Log($"..dynamicResolutionSettings: {settings.dynamicResolutionSettings.enabled}  DLSS: {settings.dynamicResolutionSettings.enableDLSS}  ForcedResolution: {settings.dynamicResolutionSettings.forceResolution} ({settings.dynamicResolutionSettings.forcedPercentage})");
            }

            if (inputs.dynamicResolutionTarget.overrideState)
            {
                drsTargetSettings = inputs.dynamicResolutionTarget.value;
                Debug.Log($"..drsTargetSettings {drsTargetSettings.dynamicResolutionFPSTarget}");
            }

            if (inputs.materialQuality.overrideState)
            {
                //m_hdrpInstantiated.currentPlatformRenderPipelineSettings.set
            }
            
            if (inputs.audioVolume.overrideState)
            {
                AudioListener.volume = inputs.audioVolume.value;
            }

            foreach (var cam in Camera.allCameras)
            {
                var hdAdditionalCamera = cam.GetComponent<HDAdditionalCameraData>();
                if (!hdAdditionalCamera)
                {
                    Debug.Log($"Skipping camera {cam.name}");
                    continue;
                }

                if (inputs.materialQuality.overrideState || inputs.rtDisallowedShadows)
                {
                    hdAdditionalCamera.customRenderingSettings = true;
                    hdAdditionalCamera.renderingPathCustomFrameSettingsOverrideMask = new FrameSettingsOverrideMask
                    {
                        mask = new BitArray128(
                            inputs.rtDisallowedShadows ? 1ul << (int)FrameSettingsField.ScreenSpaceShadows : 0ul,
                            inputs.materialQuality.overrideState ? 1ul << (int)(FrameSettingsField.MaterialQualityLevel - 64) : 0ul
                        )
                    };
                    
                    hdAdditionalCamera.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.ScreenSpaceShadows, !inputs.rtDisallowedShadows);
                    
                    if(inputs.materialQuality.value == Preset.Low)
                        hdAdditionalCamera.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.Low;
                    else if(inputs.materialQuality.value == Preset.Medium)
                        hdAdditionalCamera.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.Medium;
                    else
                        hdAdditionalCamera.renderingPathCustomFrameSettings.materialQuality = MaterialQuality.High;
                }
                else
                {
                    hdAdditionalCamera.customRenderingSettings = false;
                    hdAdditionalCamera.renderingPathCustomFrameSettingsOverrideMask = new FrameSettingsOverrideMask();
                    hdAdditionalCamera.renderingPathCustomFrameSettings.materialQuality = 0;
                }
                
                if (inputs.upscaleMethodParameter.overrideState)
                {
                    var method = inputs.upscaleMethodParameter.value;
                    if (method == QualitySettingsData.UpscaleMethod.Native)
                    {
                        Debug.Log("Disabling camera DRS as native was requested.");
                        hdAdditionalCamera.allowDynamicResolution = false;
                    }
                    else
                    {
                        Debug.Log($"Enabling camera DRS (method = {method}).");
                        hdAdditionalCamera.allowDynamicResolution = true;
                        
                        if (method == QualitySettingsData.UpscaleMethod.DLSS)
                        {
                            hdAdditionalCamera.allowDeepLearningSuperSampling = true;
                        }
                        else
                        {
                            hdAdditionalCamera.allowDeepLearningSuperSampling = false;
                            settings.dynamicResolutionSettings.forceResolution = true;
                            settings.dynamicResolutionSettings.forcedPercentage = inputs.taaUpscaleFactor.value;

                            // if (method == QualitySettingsData.UpscaleMethod.TAAU)
                            //     settings.dynamicResolutionSettings.upsampleFilter = DynamicResUpscaleFilter.TAAU;
                            // else
                                settings.dynamicResolutionSettings.upsampleFilter = DynamicResUpscaleFilter.EdgeAdaptiveScalingUpres;
                        }
                    }
                }
            }

            // Write back settings to RP asset.
            typeof(HDRenderPipelineAsset).GetField("m_RenderPipelineSettings", BindingFlags.Instance|BindingFlags.NonPublic).SetValue(m_hdrpInstantiated, settings);
        }

#if UNITY_EDITOR
        if(!m_hasAppliedHeavySettingsOnce)
            QualitySettings.SetQualityLevel(kCustomQuality, true);
#endif
        
        var needsHDRPReinit = QualitySettingsData.ApplyEngineQualitySettingsToCurrentQuality(qualitySettings);

        // var dynamicResolutionSettings = m_hdrpInstantiated.currentPlatformRenderPipelineSettings.dynamicResolutionSettings;
        // if (dynamicResolutionSettings.enabled && !dynamicResolutionSettings.forceResolution && drsTargetSettings != null)
        // {
        //     m_DynamicResolutionScaler = new DynamicResolutionScaler(drsTargetSettings);
        //     Debug.Log($"..dynamic resolution scaler");
        // }
        // else
        // {
        //     DynamicResolutionHandler.SetDynamicResScaler(null);
        //     m_DynamicResolutionScaler = null;
        // }

        if(!m_hasAppliedHeavySettingsOnce || needsHDRPReinit)
            QualitySettings.renderPipeline = m_hdrpInstantiated;

        m_hasAppliedHeavySettingsOnce = true;
    }

    static string PresetNameFromGameObject(GameObject go) => go.name.Substring(go.name.LastIndexOf("-", System.StringComparison.Ordinal) + 1);

    static void Apply(bool isRaytracing, GameObject groupRoot, Preset preset)
    {
        var groupRootT = groupRoot.transform;
        for (int i = 0, n = groupRootT.childCount; i < n; ++i)
        {
            var kindT = groupRootT.GetChild(i);
            var isKindAny = kindT.name == kAny;
            var isKindRT = kindT.name == kRaytrace;

            if (isKindAny || isKindRT == isRaytracing)
            {
                for (int j = 0, m = kindT.childCount; j < m; ++j)
                {
                    var presetT = kindT.GetChild(j);
                    var presetName = PresetNameFromGameObject(presetT.gameObject);
                    
                    presetT.gameObject.SetActive(presetName == preset.ToString());
                }

                kindT.gameObject.SetActive(true);
            }
            else 
            {
                kindT.gameObject.SetActive(false);
            }
        }
    }

    void ApplyTags(bool applyall = true)
    {
        if(m_AppliedQuickInputs == null)
            return;
        
        if(applyall) ApplyTag(general);
        ApplyTag(ambientOcclusion);
        ApplyTag(globalIllumination);
        ApplyTag(reflections);
        ApplyTag(shadows);
        if(applyall) ApplyTag(hair);
        ApplyTag(directionalShadowRaySamples);
        if(applyall) ApplyTag(targetDisable);
        if(applyall) ApplyTag(domeLights);
        //if(applyall) ApplyTag(filmGrain);
        ApplyTag(moBlur);
        ApplyTag(dof);
        ApplyTag(lensFlare);
    }

    void ApplyTag(GameObject groupRoot)
    {
        Debug.Log($"[QuickSettings] ApplyTag: '{groupRoot.name}'.");

        // We could keep lists of these, but for now it won't matter much..
        var quickSettingTags = GameObject.FindObjectsOfType<QuickSettingTag>(true);
        var groupQuickSettingTagQualities = groupRoot.GetComponentsInChildren<QuickSettingTagQuality>(true);
            
        // First iterate and disable anything that's active
        foreach(var groupQuickSettingTagQuality in groupQuickSettingTagQualities)
        {
            foreach (var quickSettingTag in quickSettingTags)
            {
                // Skip tag mismatches
                if (groupQuickSettingTagQuality.tag != quickSettingTag.tag)
                    continue;
                
                Debug.Log($"[QuickSettings] ApplyTag: Un-Setting quickSettingTag '{quickSettingTag.name}'.");
                quickSettingTag.Action(false);
            }
        }
        
        // Then iterate and enable only what's wanted
        foreach(var groupQuickSettingTagQuality in groupQuickSettingTagQualities)
        {
            foreach (var quickSettingTag in quickSettingTags)
            {
                // Skip tag mismatches
                if (groupQuickSettingTagQuality.tag != quickSettingTag.tag)
                    continue;
                
                var qualityMatch = System.Array.IndexOf(quickSettingTag.validPresets, groupQuickSettingTagQuality.preset) != -1;
                if (qualityMatch && groupQuickSettingTagQuality.isActiveAndEnabled)
                {
                    Debug.Log($"[QuickSettings] ApplyTag: Setting quickSettingTag '{quickSettingTag.name}' based on groupQuickSettingTagQuality '{groupQuickSettingTagQuality.name}'.");
                    quickSettingTag.Action(true);
                }
            }
        }
    }

    public static Preset NextInSequence(GameObject groupRoot, Preset preset, int direction)
    {
        var currentPresetName = preset.ToString();
        
        // Any kind will do as the are supposed to be symmetric
        var groupRootT = groupRoot.transform;
        for (int i = 0, n = groupRootT.childCount; i < n; ++i)
        {
            var kindT = groupRootT.GetChild(i);
            for (int j = 0, m = kindT.childCount; j < m; ++j)
            {
                var presetT = kindT.GetChild(j);
                var presetName = PresetNameFromGameObject(presetT.gameObject);

                if (presetName == currentPresetName)
                {
                    var k = (j + direction + m) % m;
                    
                    var newPresetT = kindT.GetChild(k);
                    var newPresetName = PresetNameFromGameObject(newPresetT.gameObject);

                    return System.Enum.Parse<Preset>(newPresetName);
                }
            }
        }
        
        Debug.LogError("Unable to find next preset.");
        return preset;
    }

    public static Preset Next(GameObject groupRoot, Preset preset) => NextInSequence(groupRoot, preset, 1);
    public static Preset Prev(GameObject groupRoot, Preset preset) => NextInSequence(groupRoot, preset, -1);
}
