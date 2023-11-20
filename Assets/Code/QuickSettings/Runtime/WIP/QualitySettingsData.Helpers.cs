using UnityEngine;

public partial class QualitySettingsData
{
    public static bool ApplyEngineQualitySettingsToCurrentQuality(EngineQualitySettings engineQualitySettings)
    {
        var needsReinit = false; // QualitySettings.globalTextureMipmapLimit != (int)engineQualitySettings.textureQuality.value;
        
        QualitySettings.vSyncCount = (int)engineQualitySettings.vsync.value;
        QualitySettings.globalTextureMipmapLimit = (int)engineQualitySettings.textureQuality.value;
        QualitySettings.anisotropicFiltering = engineQualitySettings.anisotropicFiltering.value;
        QualitySettings.skinWeights = engineQualitySettings.skinWeights.value;
        QualitySettings.asyncUploadTimeSlice = engineQualitySettings.asyncUploadTimeSlice.value;
        QualitySettings.asyncUploadBufferSize = engineQualitySettings.asyncUploadBufferSize.value;
        QualitySettings.asyncUploadPersistentBuffer = engineQualitySettings.asyncUploadPersistentBuffer.value;

        QualitySettings.streamingMipmapsActive = engineQualitySettings.streamingMipmapsActive;
        QualitySettings.streamingMipmapsAddAllCameras = engineQualitySettings.streamingMipmapsAddAllCameras;
        QualitySettings.streamingMipmapsMemoryBudget = engineQualitySettings.streamingMipmapsMemoryBudget;
        QualitySettings.streamingMipmapsMaxLevelReduction = engineQualitySettings.streamingMipmapsMaxLevelReduction;
        QualitySettings.streamingMipmapsRenderersPerFrame = engineQualitySettings.streamingMipmapsRenderersPerFrame;
        QualitySettings.streamingMipmapsMaxFileIORequests = engineQualitySettings.streamingMipmapsMaxFileIORequests;

        QualitySettings.resolutionScalingFixedDPIFactor = engineQualitySettings.resolutionScalingFixeDPIFactor;
        QualitySettings.particleRaycastBudget = engineQualitySettings.particleRaycastBudget;
        QualitySettings.billboardsFaceCameraPosition = engineQualitySettings.terrainBillboardsFaceCameraPosition;

        return needsReinit;
    }
    
    public static void CaptureCurrentQualityToEngineQualitySettings(ref EngineQualitySettings engineQualitySettings)
    {
        engineQualitySettings.vsync.value = (VSyncCount)QualitySettings.vSyncCount;
        engineQualitySettings.textureQuality.value = (TextureResolution)QualitySettings.globalTextureMipmapLimit;
        engineQualitySettings.anisotropicFiltering.value = QualitySettings.anisotropicFiltering;
        engineQualitySettings.skinWeights.value = QualitySettings.skinWeights;
        engineQualitySettings.asyncUploadTimeSlice.value = QualitySettings.asyncUploadTimeSlice;
        engineQualitySettings.asyncUploadBufferSize.value = QualitySettings.asyncUploadBufferSize;
        engineQualitySettings.asyncUploadPersistentBuffer.value = QualitySettings.asyncUploadPersistentBuffer;
        
        // engineQualitySettings.streamingMipmapsActive = QualitySettings.streamingMipmapsActive;
        // engineQualitySettings.streamingMipmapsAddAllCameras = QualitySettings.streamingMipmapsAddAllCameras;
        // engineQualitySettings.streamingMipmapsMemoryBudget = QualitySettings.streamingMipmapsMemoryBudget;
        // engineQualitySettings.streamingMipmapsMaxLevelReduction = QualitySettings.streamingMipmapsMaxLevelReduction;
        // engineQualitySettings.streamingMipmapsRenderersPerFrame = QualitySettings.streamingMipmapsRenderersPerFrame;
        // engineQualitySettings.streamingMipmapsMaxFileIORequests = QualitySettings.streamingMipmapsMaxFileIORequests;
        
        // engineQualitySettings.resolutionScalingFixeDPIFactor = QualitySettings.resolutionScalingFixedDPIFactor;
        // engineQualitySettings.particleRaycastBudget = QualitySettings.particleRaycastBudget;
        // engineQualitySettings.terrainBillboardsFaceCameraPosition = QualitySettings.billboardsFaceCameraPosition;
    }
}
