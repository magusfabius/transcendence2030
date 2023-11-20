using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public partial class QuickSettings
{
    static readonly string[] kDLSSNames = { "UltraPerformance", "MaximumPerformance", "Balanced", "MaximumQuality" };

    public void ToggleFullscreen()
    {
        var caps = UIReadSystemCaps();
        var settings = UIReadCurrentSettings(caps);
        settings.displayModeCurrentSettingIndex = (settings.displayModeCurrentSettingIndex + 1) % 2;
        UIWriteChangedSettings(caps, settings);
    }
    
    public void SetPreset(string presetName, string presetData)
    {
        QuickInputs.ActiveQualityPreset = presetName;
        QuickInputs.ActiveQualityData = presetData;
        
        if(presetName == QuickInputs.kCustomQualityName)
            QuickInputs.CustomQualityData = presetData;
    }
    
    public void SetResolution(int screenWidth, int screenHeight, int screenRefresh, int screenFullscreen)
    {
        QuickInputs.FullscreenWidth = screenWidth;
        QuickInputs.FullscreenHeight = screenHeight;
        QuickInputs.FullscreenRefresh = screenRefresh;
        QuickInputs.FullscreenMode = screenFullscreen;
    }

    public void SetUpscale(int upscaleMode)
    {
        QuickInputs.UpscaleMode = upscaleMode;
    }

    ui_controller.Caps UIReadSystemCaps()
    {
        var raytracingSupportedAndEnabled = SystemInfo.supportsRayTracing && CanDoRaytracing
#if UNITY_EDITOR
            && (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64
            || UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows
            || UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.GameCoreXboxSeries
            || UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.PS5)
#endif
            ;
        var isDLSSSupported = HDDynamicResolutionPlatformCapabilities.DLSSDetected;
        var displayInfo = Screen.mainWindowDisplayInfo;

        var wndRes = new List<Resolution>();
        var fsRes = new List<Resolution>();
        foreach (var res in Screen.resolutions)
        {
            Debug.Log($"Enumerating screen resolution: {res.width}x{res.height}@{res.refreshRateRatio.numerator}/{res.refreshRateRatio.denominator}");
            
            if (!Bootstrap.sArgAllowAnyScreen)
            {
                var aspect = res.width / (float)res.height;
                if (aspect < 1.5f || aspect > 2f)
                    continue;
                
                if(res.height < 900)
                    continue;
            }

            // Unity Hz right now since we don't do exclusive
            var resolution = res;
            resolution.refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 };
            
            // TODO: Should query _actual_ window border dimensions.
            if(resolution.width <= displayInfo.workArea.width - 20 && resolution.height <= displayInfo.workArea.height - 80)
                wndRes.Add(resolution);

            if (resolution.width <= displayInfo.width && resolution.height <= displayInfo.height)
            {
                if (!fsRes.Contains(resolution))
                {
                    Debug.Log($"Keeping screen resolution: {resolution.width}x{resolution.height}@{resolution.refreshRateRatio.numerator}/{resolution.refreshRateRatio.denominator}");
                    fsRes.Add(resolution);
                }
            }
        }

        if (/*wndRes.Count == 0 ||*/ fsRes.Count == 0)
        {
            Debug.LogError("Failed to find any resolutions. Added a couple of standard and hoping for the best. Launch with '-screen-any' to disable filtering.");
            //wndRes.Add(new Resolution { width = 1920, height = 1080, refreshRateRatio = new RefreshRate {numerator = 60, denominator = 1} });
            //wndRes.Add(new Resolution { width = 2560, height = 1440, refreshRateRatio = new RefreshRate {numerator = 60, denominator = 1} });
            fsRes.Add(new Resolution { width = 1920, height = 1080, refreshRateRatio = new RefreshRate {numerator = 60, denominator = 1} });
            fsRes.Add(new Resolution { width = 2560, height = 1440, refreshRateRatio = new RefreshRate {numerator = 60, denominator = 1} });
        }
        
        return new ui_controller.Caps
        {
            IsRaytracingSupported = raytracingSupportedAndEnabled,
            IsDLSSSupported = isDLSSSupported,
            DesktopInfo = $"{displayInfo.width} x {displayInfo.height} @ {displayInfo.refreshRate}",
            SupportedWindowedResolutions = wndRes,
            SupportedFullscreenResolutions = fsRes,
        };
    }

    void UIWriteChangedSettings(ui_controller.Caps caps, ui_controller.SettingsValues settings)
    {
        var newPresetName = ui_controller.presetNames[settings.presetsCurrentSettingIndex];
        var isCustomPreset = string.Compare(QuickInputs.kCustomQualityName, newPresetName, System.StringComparison.InvariantCultureIgnoreCase) == 0;

        var presetData = isCustomPreset ? CreateCustomPresetData(settings) : QuickInputs.LoadPresetData(newPresetName);
        var quickSettings = QuickInputs.ProduceFromPresetData(presetData);

        QuickInputs.ShowFPS = settings.fpsCurrentSettingIndex > 0 ? 2 : 0;
        
        var resolution = caps.SupportedFullscreenResolutions[settings.resolutionsCurrentSettingsIndex];
        var width = resolution.width;
        var height = resolution.height;
        var fullscreen = settings.displayModeCurrentSettingIndex;
        QuickInputs.PatchScreenResolution(quickSettings, ref width, ref height, (int)resolution.refreshRateRatio.numerator, fullscreen);
        
        var upscaleMode = settings.upscaleModeCurrentSettingIndex;
        QuickInputs.PatchUpscaleMode(quickSettings, ref upscaleMode);

        var vsync = settings.vsyncCurrentSettingIndex;
        QuickInputs.PatchVSync(quickSettings, ref vsync);
        QuickInputs.VSync = vsync;
        
        Apply(quickSettings);
        
        SetPreset(newPresetName, presetData);
        SetResolution(resolution.width, resolution.height, (int)resolution.refreshRateRatio.numerator, fullscreen); //write unpatched
        SetUpscale(upscaleMode);
    }

    string CreateCustomPresetData(ui_controller.SettingsValues settingsValues)
    {
        var sb = new System.Text.StringBuilder("Base:Any\n");
        sb.AppendLine($"DLSS:{kDLSSNames[settingsValues.dlssCurrentSettingIndex]}");
        sb.AppendLine($"@TAAU:{settingsValues.resolutionScale}");
        sb.AppendLine($"Texture:{ui_controller.textureSettingsNames[settingsValues.texturesSizeCurrentSettingIndex]}");
        sb.AppendLine($"Material:{ui_controller.qualitySettingsNames[settingsValues.materialQualityCurrentSettingIndex]}");
        sb.AppendLine($"Lighting:{ui_controller.qualitySettingsNames[settingsValues.lightingQualityCurrentSettingIndex]}");
        sb.AppendLine($"Reflections:{ui_controller.qualitySettingsNamesOff[settingsValues.reflectionsCurrentSettingIndex]}");
        sb.AppendLine($"@RTReflections:{ui_controller.toggleNames[settingsValues.reflectionsRtCurrentSettingIndex]}");
        sb.AppendLine($"Occlusion:{ui_controller.qualitySettingsNamesOff[settingsValues.aoCurrentSettingIndex]}");
        sb.AppendLine($"@RTOcclusion:{ui_controller.toggleNames[settingsValues.aoRtCurrentSettingIndex]}");
        sb.AppendLine($"Shadows:{ui_controller.qualitySettingsNames[settingsValues.shadowsCurrentSettingIndex]}");
        sb.AppendLine($"@RTShadows:{ui_controller.toggleNames[settingsValues.shadowsRtCurrentSettingIndex]}");
        sb.AppendLine($"Hair:{ui_controller.qualitySettingsNames[settingsValues.hairCurrentSettingIndex]}");
        sb.AppendLine($"Post:{ui_controller.qualitySettingsNames[settingsValues.postprocessingCurrentSettingIndex]}");
        return sb.ToString();
    }
    
    ui_controller.SettingsValues UIReadCurrentSettings(ui_controller.Caps caps)
    {
        var settings = new ui_controller.SettingsValues {
            presetsCurrentSettingIndex = System.Array.IndexOf(ui_controller.presetNames, QuickInputs.ActiveQualityPreset),
        };

        var lines = QuickInputs.ActiveQualityData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if(line.StartsWith("#") || line.StartsWith("@"))
                continue;

            var parts = line.Split(":");
            switch (parts[0])
            {
                case "DLSS": { settings.dlssCurrentSettingIndex = System.Array.IndexOf(kDLSSNames, parts[1]); break; }
                case "Texture": { settings.texturesSizeCurrentSettingIndex = System.Array.IndexOf(ui_controller.textureSettingsNames, parts[1]); break; }
                case "Material": { settings.materialQualityCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNames, parts[1]); break; }
                case "Lighting": { settings.lightingQualityCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNames, parts[1]); break; }
                case "Reflections": { settings.reflectionsCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNamesOff, parts[1]); break; }
                case "Occlusion": { settings.aoCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNamesOff, parts[1]); break; }
                case "Shadows": { settings.shadowsCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNames, parts[1]); break; }
                case "Hair": { settings.hairCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNames, parts[1]); break; }
                case "Post": { settings.postprocessingCurrentSettingIndex = System.Array.IndexOf(ui_controller.qualitySettingsNames, parts[1]); break; }
            }
        }
        
        foreach (var line in lines)
        {
            if(!line.StartsWith("@"))
                continue;

            var parts = line.Split(":");
            var onOff = parts[1].EndsWith("Off") ? 0 : 1;
            switch (parts[0])
            {
                case "@TAAU":
                {
                    if(float.TryParse(parts[1], out var pct))
                        settings.resolutionScale = pct;
                    break;
                }
                case "@RTOcclusion": { settings.aoRtCurrentSettingIndex = onOff; break; }
                case "@RTReflections": { settings.reflectionsRtCurrentSettingIndex = onOff; break; }
                case "@RTShadows": { settings.shadowsRtCurrentSettingIndex = onOff; break; }
            }
        }

        var w = QuickInputs.FullscreenWidth;
        var h = QuickInputs.FullscreenHeight;
        for (var i = 0; i < caps.SupportedFullscreenResolutions.Count; ++i)
        {
            var r = caps.SupportedFullscreenResolutions[i];
            if (r.width == w && r.height == h)
            {
                settings.resolutionsCurrentSettingsIndex = i;
                break;
            }
        }

        settings.displayModeCurrentSettingIndex = QuickInputs.FullscreenMode;
        settings.upscaleModeCurrentSettingIndex = QuickInputs.UpscaleMode;
        settings.vsyncCurrentSettingIndex = QuickInputs.VSync;
        settings.fpsCurrentSettingIndex = QuickInputs.ShowFPS > 0 ? 1 : 0;

        return settings;
    }
}
