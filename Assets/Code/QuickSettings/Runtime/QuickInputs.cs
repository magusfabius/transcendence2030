using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
public class QuickInputs
{
    const string kKeyActiveQualityPreset = "enemies_key_active_quality_preset_v2";
    const string kKeyActiveQualityData = "enemies_key_active_quality_data_v2";
    const string kKeyCustomQualityData = "enemies_key_custom_quality_data_v2";
    const string kKeyWindowedWidth = "enemies_key_wnd_width_v2";
    const string kKeyWindowedHeight = "enemies_key_wnd_height_v2";
    const string kKeyFullscreenWidth = "enemies_key_fs_width_v2";
    const string kKeyFullscreenHeight = "enemies_key_fs_height_v2";
    const string kKeyFullscreenRefreshNum = "enemies_key_fs_refreshnum_v2";
    const string kKeyFullscreenRefreshDenom = "enemies_key_fs_refreshdenom_v2";
    const string kKeyFullscreenMode = "enemies_key_fs_mode_v2";
    const string kKeyUpscaleMode = "enemies_key_upscale_mode_v2";

    const string kDefaultQualityName = "High";
    public const string kCustomQualityName = "Custom";

    // Not persisted
    public static int ShowFPS;
    public static int VSync;
    
    public static string ActiveQualityPreset
    {
        get => PlayerPrefs.GetString(kKeyActiveQualityPreset, kDefaultQualityName);
        set => PlayerPrefs.SetString(kKeyActiveQualityPreset, value);
    }
    public static string ActiveQualityData
    {
        get => PlayerPrefs.GetString(kKeyActiveQualityData, "");
        set => PlayerPrefs.SetString(kKeyActiveQualityData, value);
    }
    public static string CustomQualityData
    {
        get => PlayerPrefs.GetString(kKeyCustomQualityData, "");
        set => PlayerPrefs.SetString(kKeyCustomQualityData, value);
    }

    public static int FullscreenWidth
    {
        get => PlayerPrefs.GetInt(kKeyFullscreenWidth, 2560);
        set => PlayerPrefs.SetInt(kKeyFullscreenWidth, value);
    }
    public static int FullscreenHeight
    {
        get => PlayerPrefs.GetInt(kKeyFullscreenHeight, 1440);
        set => PlayerPrefs.SetInt(kKeyFullscreenHeight, value);
    }
    public static int FullscreenRefresh
    {
        get => PlayerPrefs.GetInt(kKeyFullscreenRefreshNum, 60);
        set => PlayerPrefs.SetInt(kKeyFullscreenRefreshNum, value);
    }
    public static int FullscreenMode
    {
        get => PlayerPrefs.GetInt(kKeyFullscreenMode, 1);
        set => PlayerPrefs.SetInt(kKeyFullscreenMode, value);
    }

    public static int UpscaleMode
    {
        get => PlayerPrefs.GetInt(kKeyUpscaleMode, (int)QualitySettingsData.UpscaleMethod.DLSS);
        set => PlayerPrefs.SetInt(kKeyUpscaleMode, value);
    }

    public enum DLSSQuality
    {
        MaximumPerformance,
        Balanced,
        MaximumQuality,
        UltraPerformance,
    }

    [System.Serializable] public class QuickSettingsPresetParameter : VolumeParameter<QuickSettings.Preset> {}
    [System.Serializable] public class AnisotropicFilteringParameter : VolumeParameter<AnisotropicFiltering> {}
    [System.Serializable] public class GlobalDynamicResolutionSettingsParameter : VolumeParameter<GlobalDynamicResolutionSettings> {}
    [System.Serializable] public class ScreenResolutionSettingsParameter : VolumeParameter<QualitySettingsData.ScreenResolutionSettings> {}
    [System.Serializable] public class UpscaleMethodParameter : VolumeParameter<QualitySettingsData.UpscaleMethod> {}
    [System.Serializable] public class DynamicResolutionTargetSettingsParameter : VolumeParameter<QualitySettingsData.DynamicResolutionTargetSettings> {}

    // Non-serialized
    public int vsync;

    public bool anyOverride;

    public bool rtDisallowedAll;
    public bool rtDisallowedAO;
    public bool rtDisallowedGI;
    public bool rtDisallowedReflections;
    public bool rtDisallowedShadows;

    public QuickSettingsPresetParameter presetQualityGeneral;
    public QuickSettingsPresetParameter presetQualityAO;
    public QuickSettingsPresetParameter presetQualityGI;
    public QuickSettingsPresetParameter presetQualityReflections;
    public QuickSettingsPresetParameter presetQualityShadows;
    public QuickSettingsPresetParameter presetQualityHair;
    public QuickSettingsPresetParameter presetQualityDirectionalShadowRays;
    public QuickSettingsPresetParameter presetQualityTargetDisable;
    public QuickSettingsPresetParameter presetQualityDomeLights;
    public QuickSettingsPresetParameter presetQualityFilmGrain;
    public QuickSettingsPresetParameter presetQualityMoBlur;
    public QuickSettingsPresetParameter presetQualityDoF;
    public QuickSettingsPresetParameter presetQualityLensFlare;

    public IntParameter masterTextureLimit;
    public AnisotropicFilteringParameter anisotropicFiltering;
    public ClampedIntParameter anisotropicFilteringForcedMin;
    public ClampedIntParameter anisotropicFilteringGlobalMax;

    public ScreenResolutionSettingsParameter screenResolutionSettings;
    public UpscaleMethodParameter upscaleMethodParameter;
    public ClampedFloatParameter taaUpscaleFactor;
    public GlobalDynamicResolutionSettingsParameter dynamicResolutionSettings;
    public DynamicResolutionTargetSettingsParameter dynamicResolutionTarget;
    public QuickSettingsPresetParameter materialQuality;

    public ClampedFloatParameter audioVolume;
    public ClampedFloatParameter preWarmTimeStep;
    public ClampedFloatParameter heavySettingsDelay;
    public ClampedFloatParameter heavySettingsFade;

    public static QuickInputs NewDefault()
    {
        var qi = new QuickInputs();
        qi.SetDefaults();
        return qi;
    }

    public static string ConfigsDir { get; set; }

    public static string LoadPresetData(string presetName)
    {
        string presetData = null;
        
        if (presetName == kCustomQualityName)
        {
            presetData = CustomQualityData;
        }
        else
        {
            var presetPath = System.IO.Path.Combine(ConfigsDir, $"Preset-{presetName}.txt");
            presetData = System.IO.File.ReadAllText(presetPath);
        }

        return presetData;
    }

    public static QuickInputs ProduceFromPresetData(string presetData)
    {
        var presetLines = presetData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        if(presetLines.Length < 1)
        {
            Debug.LogError($"[QuickInputs] No valid preset data found in:\n{presetData}'");
            return null;
        }
        
        QuickInputs mergedInputs = null;
        foreach (var presetLine in presetLines)
        {
            if(presetLine.StartsWith("#") || presetLine.StartsWith("@"))
                continue;
            
            var path = System.IO.Path.Combine(ConfigsDir, $"{presetLine.Replace(":", "-")}.json");
            Debug.Log($"[QuickInputs] Attempting to merge quality preset '{presetLine}' from {path}.");
            var inputs = Produce(path);

            if (mergedInputs == null)
            {
                mergedInputs = inputs;
            }
            else
            {
                mergedInputs.Override(inputs);
            }
        }

        foreach (var presetLine in presetLines)
        {
            if(!presetLine.StartsWith("@"))
                continue;

            var parts = presetLine.Split(":");
            switch (parts[0])
            {
                case "@TAAU":
                {
                    if(float.TryParse(parts[1], out var pct))
                        mergedInputs.taaUpscaleFactor.Override(pct);
                    break;
                }
                case "@RTOcclusion": { mergedInputs.rtDisallowedAO = string.Compare(parts[1], "Off", System.StringComparison.InvariantCultureIgnoreCase) == 0; break; }
                case "@RTReflections": { mergedInputs.rtDisallowedReflections = string.Compare(parts[1], "Off", System.StringComparison.InvariantCultureIgnoreCase) == 0; break; }
                case "@RTShadows": { mergedInputs.rtDisallowedShadows = string.Compare(parts[1], "Off", System.StringComparison.InvariantCultureIgnoreCase) == 0; break; }
            }
        }

        return mergedInputs;
    }

    public static void ResetSettings()
    {
        PlayerPrefs.DeleteKey(kKeyActiveQualityPreset);
        PlayerPrefs.DeleteKey(kKeyActiveQualityData);
        PlayerPrefs.DeleteKey(kKeyCustomQualityData);
        PlayerPrefs.DeleteKey(kKeyWindowedWidth);
        PlayerPrefs.DeleteKey(kKeyWindowedHeight);
        PlayerPrefs.DeleteKey(kKeyFullscreenHeight);
        PlayerPrefs.DeleteKey(kKeyFullscreenWidth);
        PlayerPrefs.DeleteKey(kKeyFullscreenRefreshNum);
        PlayerPrefs.DeleteKey(kKeyFullscreenRefreshDenom);
        PlayerPrefs.DeleteKey(kKeyFullscreenMode);
        PlayerPrefs.DeleteKey(kKeyUpscaleMode);
        QuickInputs.VSync = 0;
    }

    public static QuickInputs ProduceFromOverrides(string presetOverride, string widthOverride, string heightOverride, string fullscreenOverride,
        out string presetName, out string presetData, out int screenWidth, out int screenHeight, out int screenRefresh, out int screenFullscreen, out int upscaleMode)
    {
        // First get previously used preset, or the default one if first run.
        presetName = ActiveQualityPreset;
        
        // Overwrite preset if one is provided
        if (!string.IsNullOrEmpty(presetOverride))
        {
            presetName = presetOverride;
        }

        // Get previously used fullscreen mode (default fs wnd) and account for overrides
        screenFullscreen = FullscreenMode;
        if (!string.IsNullOrEmpty(fullscreenOverride))
        {
            if (int.TryParse(fullscreenOverride, out var fsOverride))
            {
                screenFullscreen = Mathf.Clamp(fsOverride, 0, 1);
            }
        }

        // Get previously used screen resolution (but capped to display caps)
        var displayInfo = Screen.mainWindowDisplayInfo;
        // screenWidth = PlayerPrefs.GetInt(screenFullscreen == 0 ? kKeyWindowedWidth : kKeyFullscreenWidth, screenFullscreen == 0 ? displayInfo.workArea.width : displayInfo.width);
        // screenHeight = PlayerPrefs.GetInt(screenFullscreen == 0 ? kKeyWindowedHeight : kKeyFullscreenHeight, screenFullscreen == 0 ? displayInfo.workArea.width : displayInfo.width);
        // screenRefresh = screenFullscreen == 0 ? (int)displayInfo.refreshRate.numerator : PlayerPrefs.GetInt(kKeyFullscreenRefreshNum, (int)displayInfo.refreshRate.numerator);
        // var refreshDenom = screenFullscreen == 0 ? (int)displayInfo.refreshRate.denominator : PlayerPrefs.GetInt(kKeyFullscreenRefreshDenom, (int)displayInfo.refreshRate.denominator);
        screenWidth = PlayerPrefs.GetInt(kKeyFullscreenWidth, displayInfo.width);
        screenHeight = PlayerPrefs.GetInt(kKeyFullscreenHeight, displayInfo.height);
        screenRefresh = PlayerPrefs.GetInt(kKeyFullscreenRefreshNum, (int)displayInfo.refreshRate.numerator);

        if (!string.IsNullOrEmpty(widthOverride))
        {
            if (int.TryParse(widthOverride, out var widthO))
                screenWidth = Mathf.Clamp(widthO, 800, displayInfo.width);
        }
        if (!string.IsNullOrEmpty(heightOverride))
        {
            if (int.TryParse(heightOverride, out var heightO))
                screenHeight = Mathf.Clamp(heightO, 600, displayInfo.height);
        }
        
        // Get previously used upscale mode or default
        upscaleMode = UpscaleMode;
        
        presetData = LoadPresetData(presetName);
        var produced = ProduceFromPresetData(presetData);
        PatchScreenResolution(produced, ref screenWidth, ref screenHeight, screenRefresh, screenFullscreen);
        PatchUpscaleMode(produced, ref upscaleMode);
        return produced;
    }

    public static void ClampScreenResolution(ref int width, ref int height, int fullscreen)
    {
        var displayInfo = Screen.mainWindowDisplayInfo;

        if (fullscreen == 0)
        {
            width = Mathf.Min(width, displayInfo.workArea.width - 20);
            height = Mathf.Min(height, displayInfo.workArea.height - 80);
        }
        else
        {
            width = Mathf.Min(width, displayInfo.width);
            height = Mathf.Min(height, displayInfo.height);
        }
    }
    public static void PatchScreenResolution(QuickInputs input, ref int width, ref int height, int refresh, int fullscreen)
    {
        ClampScreenResolution(ref width, ref height, fullscreen);

        var res = QualitySettingsData.ScreenResolutionSettings.NewDefault();
        res.screenWidth.Override(width);
        res.screenHeight.Override(height);
        res.fullScreenMode.Override(fullscreen == 0 ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
        res.preferredRefreshRate.Override(refresh);
        input.screenResolutionSettings = new ScreenResolutionSettingsParameter
        {
            overrideState = true,
            value = res,
        };
    }

    public static void PatchUpscaleMode(QuickInputs input, ref int upscaleMode)
    {
        upscaleMode = Mathf.Clamp(upscaleMode, 0, (int)(HDDynamicResolutionPlatformCapabilities.DLSSDetected ? QualitySettingsData.UpscaleMethod.DLSS : QualitySettingsData.UpscaleMethod.FSR));
        
        input.upscaleMethodParameter.Override((QualitySettingsData.UpscaleMethod)upscaleMode);
    }

    public static void PatchVSync(QuickInputs input, ref int vsync)
    {
        vsync = Mathf.Clamp(vsync, 0, 4);
        input.vsync = vsync;
    }

    public static QuickInputs Produce(string jsonPath)
    {
        return JsonUtility.FromJson<QuickInputs>(System.IO.File.ReadAllText(jsonPath));
    }

    public void SetDefaults()
    {
        anyOverride = false;
        
        presetQualityGeneral = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityAO = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityGI = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityReflections = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityShadows = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityHair = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityDirectionalShadowRays = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityTargetDisable = new() { value = QuickSettings.Preset.Medium, overrideState = false};
        presetQualityDomeLights = new() { value = QuickSettings.Preset.High, overrideState = false};
        presetQualityFilmGrain = new() { value = QuickSettings.Preset.Any, overrideState = false};

        masterTextureLimit = new(0, false);
        anisotropicFiltering = new() {value = AnisotropicFiltering.ForceEnable, overrideState = false};
        anisotropicFilteringForcedMin = new(16, -1, 16, false);
        anisotropicFilteringGlobalMax = new(16, -1, 16, false);

        screenResolutionSettings = new() {value = QualitySettingsData.ScreenResolutionSettings.NewDefault(), overrideState = false};
        upscaleMethodParameter = new UpscaleMethodParameter() { value = QualitySettingsData.UpscaleMethod.DLSS, overrideState = false};
        taaUpscaleFactor = new ClampedFloatParameter(66.667f, 25f, 100f, false);
        
        var gdrs = GlobalDynamicResolutionSettings.NewDefault();
        gdrs.dynResType = DynamicResolutionType.Hardware;
        gdrs.minPercentage = 50f;
        gdrs.forcedPercentage = 200f / 3f;
        gdrs.forceResolution = true;
        gdrs.upsampleFilter = DynamicResUpscaleFilter.EdgeAdaptiveScalingUpres;
        gdrs.enableDLSS = true;
        gdrs.useMipBias = false;
        gdrs.DLSSPerfQualitySetting = (int)DLSSQuality.Balanced;
        gdrs.DLSSUseOptimalSettings = true;
        gdrs.DLSSInjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.AfterDepthOfField;
        dynamicResolutionSettings = new() {value = gdrs, overrideState = false};

        dynamicResolutionTarget = new() {value = QualitySettingsData.DynamicResolutionTargetSettings.NewDefault(), overrideState = false};
        materialQuality = new QuickSettingsPresetParameter { value = QuickSettings.Preset.High, overrideState = false };
        
        audioVolume = new(1f, 0f, 1f, false);
        preWarmTimeStep = new(2f, 0f, 30f, false);
        heavySettingsDelay = new(1.75f, 0f, 5f, false);
        heavySettingsFade = new(0.5f, 0f, 1f, false);
    }

    public void Override(QuickInputs other)
    {
        if(!other.anyOverride)
            return;
        
        anyOverride |= other.anyOverride;
        if(other.presetQualityGeneral.overrideState) presetQualityGeneral.Override((QuickSettings.Preset)other.presetQualityGeneral);
        if(other.presetQualityAO.overrideState) presetQualityAO.Override((QuickSettings.Preset)other.presetQualityAO);
        if(other.presetQualityGI.overrideState) presetQualityGI.Override((QuickSettings.Preset)other.presetQualityGI);
        if(other.presetQualityReflections.overrideState) presetQualityReflections.Override((QuickSettings.Preset)other.presetQualityReflections);
        if(other.presetQualityShadows.overrideState) presetQualityShadows.Override((QuickSettings.Preset)other.presetQualityShadows);
        if(other.presetQualityHair.overrideState) presetQualityHair.Override((QuickSettings.Preset)other.presetQualityHair);
        if(other.presetQualityDirectionalShadowRays.overrideState) presetQualityDirectionalShadowRays.Override((QuickSettings.Preset)other.presetQualityDirectionalShadowRays);
        if(other.presetQualityTargetDisable.overrideState) presetQualityTargetDisable.Override((QuickSettings.Preset)other.presetQualityTargetDisable);
        if(other.presetQualityDomeLights.overrideState) presetQualityDomeLights.Override((QuickSettings.Preset)other.presetQualityDomeLights);
        if(other.presetQualityFilmGrain.overrideState) presetQualityFilmGrain.Override((QuickSettings.Preset)other.presetQualityFilmGrain);
        if(other.presetQualityMoBlur.overrideState) presetQualityMoBlur.Override((QuickSettings.Preset)other.presetQualityMoBlur);
        if(other.presetQualityDoF.overrideState) presetQualityDoF.Override((QuickSettings.Preset)other.presetQualityDoF);
        if(other.presetQualityLensFlare.overrideState) presetQualityLensFlare.Override((QuickSettings.Preset)other.presetQualityLensFlare);
        if(other.masterTextureLimit.overrideState) masterTextureLimit.Override((int)other.masterTextureLimit);
        if(other.anisotropicFiltering.overrideState) anisotropicFiltering.Override((AnisotropicFiltering)other.anisotropicFiltering);
        if(other.anisotropicFilteringForcedMin.overrideState) anisotropicFilteringForcedMin.Override((int)other.anisotropicFilteringForcedMin);
        if(other.anisotropicFilteringGlobalMax.overrideState) anisotropicFilteringGlobalMax.Override((int)other.anisotropicFilteringGlobalMax);
        if(other.screenResolutionSettings.overrideState) screenResolutionSettings.Override((QualitySettingsData.ScreenResolutionSettings)other.screenResolutionSettings);
        if(other.upscaleMethodParameter.overrideState) upscaleMethodParameter.Override((QualitySettingsData.UpscaleMethod)other.upscaleMethodParameter);
        if(other.taaUpscaleFactor.overrideState) taaUpscaleFactor.Override((float)other.taaUpscaleFactor);
        if(other.dynamicResolutionSettings.overrideState) dynamicResolutionSettings.Override((GlobalDynamicResolutionSettings)other.dynamicResolutionSettings);
        if(other.dynamicResolutionTarget.overrideState) dynamicResolutionTarget.Override((QualitySettingsData.DynamicResolutionTargetSettings)other.dynamicResolutionTarget);
        if(other.materialQuality.overrideState) materialQuality.Override((QuickSettings.Preset)other.materialQuality);
        if(other.audioVolume.overrideState) audioVolume.Override((float)other.audioVolume);
        if(other.preWarmTimeStep.overrideState) preWarmTimeStep.Override((float)other.preWarmTimeStep);
        if(other.heavySettingsDelay.overrideState) heavySettingsDelay.Override((float)other.heavySettingsDelay);
        if(other.heavySettingsFade.overrideState) heavySettingsFade.Override((float)other.heavySettingsFade);
    }
}
