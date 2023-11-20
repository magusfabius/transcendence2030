using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using SerializableAttribute = System.SerializableAttribute;

public partial class QualitySettingsData
{
    [System.Flags]
    public enum GraphicsAPI
    {
        DX12 = 1 << 0,
        DX11 = 1 << 1,
        Metal = 1 << 2,
        Vulkan = 1 << 3,
        Native = 1 << 4,
        
        All = DX12 | DX11 | Metal | Vulkan | Native,
    }

    public enum VSyncCount
    {
        Never,
        EveryVBlank,
        EveryOtherVBlank,
        EveryThirdVBlank,
        EveryFourthVBlank,
    }
    
    public enum TextureResolution
    {
        [InspectorName("Full Resolution")][Tooltip("Default imported texture size.")] Full,
        [InspectorName("Quarter Resolution")][Tooltip("Half texture width and height.")] Quarter,
        [InspectorName("16th Resolution")][Tooltip("Quarter texture width and height.")] Sixteenth,
        [InspectorName("64th Resolution")][Tooltip("Eight texture width and height.")] SixtyFourth,
    }

    public enum DLSSQuality
    {
        /// <summary>
        ///   <para>Fast performance, lower quality.</para>
        /// </summary>
        MaximumPerformance,
        /// <summary>
        ///   <para>Balances performance with quality.</para>
        /// </summary>
        Balanced,
        /// <summary>
        ///   <para>High quality, less performant.</para>
        /// </summary>
        MaximumQuality,
        /// <summary>
        ///   <para>Fastest performance, lowest quality.</para>
        /// </summary>
        UltraPerformance,
    }

    public enum UpscaleMethod
    {
        Native,
        //TAAU,
        FSR,
        DLSS,
    }

    [Serializable] public class VSyncParameter : QualityParameter<VSyncCount> {}
    [Serializable] public class TextureQualityParameter : QualityParameter<TextureResolution> {}
    [Serializable] public class AnisotropicFilteringParameter : QualityParameter<AnisotropicFiltering> {}
    [Serializable] public class SkinWeightsParameter : QualityParameter<SkinWeights> { }
    [Serializable] public class GraphicsAPIParameter : QualityParameter<GraphicsAPI> {}
    [Serializable] public class DLSSQualityParameter : QualityParameter<DLSSQuality> {}
    [Serializable] public class DynamicResolutionTypeParameter : QualityParameter<DynamicResolutionType> {}
    [Serializable] public class DynamicResUpscaleFilterParameter : QualityParameter<DynamicResUpscaleFilter> {}
    [Serializable] public class FullScreenModeParameter : QualityParameter<FullScreenMode> {}
    
    public abstract class HDRPAssetSettingsComponent : QualitySettingsComponent
    {
        public abstract void ApplyToRenderPipelineAsset(HDRenderPipelineAsset renderPipelineAsset);

        protected static void SetRenderPipelineSettings(HDRenderPipelineAsset renderPipelineAsset, RenderPipelineSettings settings)
        {
            var fiRenderPipelineSettings = typeof(HDRenderPipelineAsset).GetField("m_RenderPipelineSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            fiRenderPipelineSettings.SetValue(renderPipelineAsset, settings);
        }
    }

    public abstract class HDRPVolumeSettingsComponent : QualitySettingsComponent
    {
        public abstract bool AffectsRasterization { get; }
        public abstract bool AffectsRaytracing { get; }
        
        public abstract void ApplyToVolumeProfile(VolumeProfile volumeProfile);
    }

    [Serializable]
    public class GraphicsAPISettings : QualitySettingsComponent
    {
        [LaunchParameter] public GraphicsAPIParameter supportedApis;
        
        public override void ApplyDefaultValues()
        {
            supportedApis = new() {value = GraphicsAPI.DX12, overrideState = true};
        }
    }

    [Serializable]
    public class EngineQualitySettings : QualitySettingsComponent
    {
        [QualitySettingParameter] public VSyncParameter vsync;
        [QualitySettingParameter] public TextureQualityParameter textureQuality;
        [QualitySettingParameter] public AnisotropicFilteringParameter anisotropicFiltering;
        [QualitySettingParameter] public SkinWeightsParameter skinWeights;
        [QualitySettingParameter] public RangeIntParameter asyncUploadTimeSlice;
        [QualitySettingParameter] public RangeIntParameter asyncUploadBufferSize;
        [QualitySettingParameter] public BoolParameter asyncUploadPersistentBuffer;
 
        // Right now these are always hard-coded, but included here for posterity.
        [QualitySettingParameter] public readonly bool streamingMipmapsActive = false;
        [QualitySettingParameter] public readonly bool streamingMipmapsAddAllCameras = false;
        [QualitySettingParameter] public readonly float streamingMipmapsMemoryBudget = 1024f;
        [QualitySettingParameter] public readonly int streamingMipmapsMaxLevelReduction = 4;
        [QualitySettingParameter] public readonly int streamingMipmapsRenderersPerFrame = 512;
        [QualitySettingParameter] public readonly int streamingMipmapsMaxFileIORequests = 8;

        // Right now these are always hard-coded, but included here for posterity.
        [QualitySettingParameter] public readonly float resolutionScalingFixeDPIFactor = 1f;
        [QualitySettingParameter] public readonly int particleRaycastBudget = 0;
        [QualitySettingParameter] public readonly bool terrainBillboardsFaceCameraPosition = false;
        
        public override void ApplyDefaultValues()
        {
            vsync = new() {value = VSyncCount.Never, overrideState = true};
            textureQuality = new() {value = TextureResolution.Full, overrideState = true};
            anisotropicFiltering = new() {value = AnisotropicFiltering.ForceEnable, overrideState = true};
            skinWeights = new() {value = SkinWeights.Unlimited, overrideState = true};
            asyncUploadTimeSlice = new() {value = 2, min = 1, max = 33, overrideState = true};
            asyncUploadBufferSize = new() {value = 16, min = 2, max = 512, overrideState = true};
            asyncUploadPersistentBuffer = new() {value = false, overrideState = true};
        }
    }

    [Serializable]
    public class ScreenResolutionSettings : QualitySettingsComponent
    {
        [GlobalStateParameter] public RangeIntParameter screenWidth;
        [GlobalStateParameter] public RangeIntParameter screenHeight;
        [GlobalStateParameter] public FullScreenModeParameter fullScreenMode;
        [GlobalStateParameter] public RangeIntParameter preferredRefreshRate;

        public static ScreenResolutionSettings NewDefault()
        {
            var i = new ScreenResolutionSettings();
            i.ApplyDefaultValues();
            return i;
        }

        public override void ApplyDefaultValues()
        {
            screenWidth = new() {value = 2560, min = 160, max = 8192, overrideState = false};
            screenHeight = new() {value = 1440, min = 160, max = 8192, overrideState = false};
            fullScreenMode = new() {value = FullScreenMode.FullScreenWindow, overrideState = false};
            preferredRefreshRate = new() {value = 60, min = 0, max = 300, overrideState = false};
        }
    }

    [Serializable]
    public class DynamicResolutionTargetSettings : QualitySettingsComponent
    {
        [GlobalStateParameter] public RangeIntParameter dynamicResolutionFPSTarget;
        [GlobalStateParameter] public RangeFloatParameter downResLatency;
        [GlobalStateParameter] public RangeFloatParameter downResMSThreshold;
        [GlobalStateParameter] public RangeFloatParameter downResStepSize;
        [GlobalStateParameter] public RangeFloatParameter upResLatency;
        [GlobalStateParameter] public RangeFloatParameter upResMSThreshold;
        [GlobalStateParameter] public RangeFloatParameter upResStepSize;

        public static DynamicResolutionTargetSettings NewDefault()
        {
            var i = new DynamicResolutionTargetSettings();
            i.ApplyDefaultValues();
            return i;
        }

        public override void ApplyDefaultValues()
        {
            dynamicResolutionFPSTarget = new() {value = 30, min = 24, max = 300, overrideState = true};
            downResLatency = new() {value = 1f, min = 1f/60f, max = 10f, overrideState = true};
            downResMSThreshold = new() {value = 1f, min = -5f, max = 5f, overrideState = true};
            downResStepSize = new() {value = 1f, min = 0.1f, max = 1f, overrideState = true};
            upResLatency = new() {value = 2f, min = 1f/60f, max = 10f, overrideState = true};
            upResMSThreshold = new() {value = 2.5f, min = -5f, max = 5f, overrideState = true};
            upResStepSize = new() {value = 0.5f, min = 0.1f, max = 1f, overrideState = true};
        }
    }

    [Serializable]
    public class EngineGlobalStateSettings : QualitySettingsComponent
    {
        [GlobalStateParameter] public RangeIntParameter anisotropicFilteringMaxLevel;
        [GlobalStateParameter] public RangeIntParameter anisotropicFilteringMinForcedLevel;
        [GlobalStateParameter] public BoolParameter allowThreadedTextureCreation;

        public override void ApplyDefaultValues()
        {
            anisotropicFilteringMaxLevel = new() {value = 16, min = -1, max = 16, overrideState = true};
            anisotropicFilteringMinForcedLevel = new() {value = 16, min = -1, max = 16, overrideState = true};
            allowThreadedTextureCreation = new() {value = true, overrideState = true};
        }
    }

    [Serializable]
    public class HDRPRenderingSettings : HDRPAssetSettingsComponent
    {
        [RenderPipelineAssetParameter] public BoolParameter realtimeRaytracing;
        
        public override void ApplyDefaultValues()
        {
            realtimeRaytracing = new() {value = true, overrideState = true};
        }

        public override void ApplyToRenderPipelineAsset(HDRenderPipelineAsset renderPipelineAsset)
        {
            var setting = renderPipelineAsset.currentPlatformRenderPipelineSettings;
            setting.supportRayTracing = realtimeRaytracing.value;
            SetRenderPipelineSettings(renderPipelineAsset, setting);
        }
    }

    [Serializable]
    public class HDRPRendererLODSettings : HDRPAssetSettingsComponent
    {
        [RenderPipelineAssetParameter] public RangeFloatParameter lodBias;
        [RenderPipelineAssetParameter] public RangeIntParameter maxLODLevel;

        public override void ApplyDefaultValues()
        {
            lodBias = new() {value = 1, min = 0, max = 5, overrideState = true};
            maxLODLevel = new() {value = 0, min = 0, max = 5, overrideState = true};
        }
        
        public override void ApplyToRenderPipelineAsset(HDRenderPipelineAsset renderPipelineAsset)
        {
            var setting = renderPipelineAsset.currentPlatformRenderPipelineSettings;
            setting.lodBias = new FloatScalableSetting(new[] { lodBias.value, lodBias.value, lodBias.value }, ScalableSettingSchemaId.With3Levels);
            setting.maximumLODLevel = new IntScalableSetting(new[] {maxLODLevel.value, maxLODLevel.value, maxLODLevel.value}, ScalableSettingSchemaId.With3Levels);
            SetRenderPipelineSettings(renderPipelineAsset, setting);
        }
    }

    [Serializable]
    public class HDRPRendererDRSSettings : HDRPAssetSettingsComponent
    {
        [RenderPipelineAssetParameter] public BoolParameter enable;
        
        [RenderPipelineAssetParameter] public BoolParameter dlssEnable;
        [RenderPipelineAssetParameter] public BoolParameter dlssUseOptimalSettings;
        [RenderPipelineAssetParameter] public DLSSQualityParameter dlssQuality;
        [RenderPipelineAssetParameter] public UnitRangeFloatParameter dlssSharpness;

        [RenderPipelineAssetParameter] public BoolParameter useMipBias;
        [RenderPipelineAssetParameter] public BoolParameter useForcePercentage;
        [RenderPipelineAssetParameter] public PercentageRangeFloatParameter forcedPercentage;
        [RenderPipelineAssetParameter] public PercentageRangeFloatParameter maxPercentage;
        [RenderPipelineAssetParameter] public PercentageRangeFloatParameter minPercentage;
        [RenderPipelineAssetParameter] public DynamicResolutionTypeParameter dynamicResolutionType;
        [RenderPipelineAssetParameter] public DynamicResUpscaleFilterParameter upscaleFilterParameter;
        [RenderPipelineAssetParameter] public PercentageRangeFloatParameter lowResTransparencyMinimumThreshold;
        [RenderPipelineAssetParameter] public PercentageRangeFloatParameter rayTracingHalfResThreshold;
        
        public override void ApplyDefaultValues()
        {
            enable = new() {value = true, overrideState = true};

            dlssEnable = new() {value = true, overrideState = true};
            dlssUseOptimalSettings = new() {value = false, overrideState = true};
            dlssQuality = new() {value = DLSSQuality.MaximumPerformance, overrideState = true};
            dlssSharpness = new() {value = 0.5f, overrideState = true};
            dlssEnable = new() {value = true, overrideState = true};
            
            useMipBias = new() {value = false, overrideState = true};
            useForcePercentage = new() {value = false, overrideState = true};
            forcedPercentage = new() {value = 2f/3f, overrideState = true};
            maxPercentage = new() {value = 100f, overrideState = true};
            minPercentage = new() {value = 50f, overrideState = true};
            dynamicResolutionType = new() {value = DynamicResolutionType.Hardware, overrideState = true};
            upscaleFilterParameter = new() {value = DynamicResUpscaleFilter.EdgeAdaptiveScalingUpres, overrideState = true};
            lowResTransparencyMinimumThreshold = new() {value = 0f, overrideState = true};
            rayTracingHalfResThreshold = new() {value = 50f, overrideState = true};
        }
        
        public override void ApplyToRenderPipelineAsset(HDRenderPipelineAsset renderPipelineAsset)
        {
            var setting = renderPipelineAsset.currentPlatformRenderPipelineSettings;
            setting.dynamicResolutionSettings.enabled = enable.value;
            setting.dynamicResolutionSettings.enableDLSS = dlssEnable.value;
            setting.dynamicResolutionSettings.DLSSUseOptimalSettings = dlssUseOptimalSettings.value;
            setting.dynamicResolutionSettings.DLSSPerfQualitySetting = (uint) dlssQuality.value;
            setting.dynamicResolutionSettings.DLSSSharpness = dlssSharpness.value;
            setting.dynamicResolutionSettings.useMipBias = useMipBias.value;
            setting.dynamicResolutionSettings.forceResolution = useForcePercentage.value;
            setting.dynamicResolutionSettings.forcedPercentage = forcedPercentage.value;
            setting.dynamicResolutionSettings.maxPercentage = maxPercentage.value;
            setting.dynamicResolutionSettings.minPercentage = minPercentage.value;
            setting.dynamicResolutionSettings.dynResType = dynamicResolutionType.value;
            setting.dynamicResolutionSettings.upsampleFilter = upscaleFilterParameter.value;
            setting.dynamicResolutionSettings.lowResTransparencyMinimumThreshold = lowResTransparencyMinimumThreshold.value;
            setting.dynamicResolutionSettings.rayTracingHalfResThreshold = rayTracingHalfResThreshold.value;
            SetRenderPipelineSettings(renderPipelineAsset, setting);
        }
    }

    [Serializable]
    public class HDRPSSRRaytracingSettings : HDRPVolumeSettingsComponent
    {
        public override bool AffectsRasterization => false;
        public override bool AffectsRaytracing => true;

        [VolumeParameterAttribute] public RangeFloatParameter rayLength;
        [VolumeParameterAttribute] public RangeIntParameter textureLodBias;
        [VolumeParameterAttribute] public BoolParameter denoise;
        [VolumeParameterAttribute] public RangeIntParameter denoiserRadius;
        [VolumeParameterAttribute] public BoolParameter fullResolution;
        
        public override void ApplyDefaultValues()
        {
            rayLength = new() {value = 25, min = 0.1f, max = 200f, overrideState = true};
            textureLodBias = new() {value = 1, min = 0, max = 7, overrideState = true};
            denoise = new() {value = true, overrideState = true};
            denoiserRadius = new() {value = 8, min = 1, max = 32, overrideState = true};
            fullResolution = new() {value = false, overrideState = true};
        }
        
        public override void ApplyToVolumeProfile(VolumeProfile volumeProfile)
        {
            if (!volumeProfile.TryGet<ScreenSpaceReflection>(out var screenSpaceReflection))
                screenSpaceReflection = volumeProfile.Add<ScreenSpaceReflection>();

            screenSpaceReflection.quality.value = ScalableSettingLevelParameter.LevelCount;
            screenSpaceReflection.mode.value = RayTracingMode.Performance;
            screenSpaceReflection.rayLength = rayLength.value;
            screenSpaceReflection.textureLodBias.SetValue(textureLodBias);
            screenSpaceReflection.denoise = denoise.value;
            screenSpaceReflection.denoiserRadius = denoiserRadius.value;
            screenSpaceReflection.fullResolution = fullResolution.value;
        }
    }

    [Serializable]
    public class HDRPSSRRaymarchingSettings : HDRPVolumeSettingsComponent
    {
        public override bool AffectsRasterization => true;
        public override bool AffectsRaytracing => false;

        [VolumeParameterAttribute] public RangeIntParameter rayMaxIterations;

        public override void ApplyDefaultValues()
        {
            rayMaxIterations = new() {value = 32, min = 4, max = 128, overrideState = true};
        }
        
        public override void ApplyToVolumeProfile(VolumeProfile volumeProfile)
        {
            if (!volumeProfile.TryGet<ScreenSpaceReflection>(out var screenSpaceReflection))
                screenSpaceReflection = volumeProfile.Add<ScreenSpaceReflection>();

            screenSpaceReflection.quality.value = ScalableSettingLevelParameter.LevelCount;
            screenSpaceReflection.rayMaxIterations = rayMaxIterations.value;
        }
    }
    
    [Serializable]
    public class QualitySetting : QualityComponentsBag
    {
        public GraphicsAPISettings graphicsAPISettings;
        public EngineQualitySettings engineQualitySettings;
        public EngineGlobalStateSettings engineGlobalStateSettings;
        public HDRPRenderingSettings hdrpRenderingSettings;
        public HDRPRendererLODSettings hdrpRendererLODSettings;
        public HDRPRendererDRSSettings hdrpRendererDRSSettings;

        static FieldInfo[] sComponentCache;
        protected override FieldInfo[] GetComponents()
        {
            if (sComponentCache == null) sComponentCache = GetComponents(GetType());
            return sComponentCache;
        }
    }

    [Serializable]
    public class QualityPreset : QualityComponentsBag
    {
        public HDRPSSRRaymarchingSettings ssrRaymarchingSettings;
        public HDRPSSRRaytracingSettings ssrRaytracingSettings;

        static FieldInfo[] sComponentCache;
        protected override FieldInfo[] GetComponents()
        {
            if (sComponentCache == null) sComponentCache = GetComponents(GetType());
            return sComponentCache;
        }
    }
}
