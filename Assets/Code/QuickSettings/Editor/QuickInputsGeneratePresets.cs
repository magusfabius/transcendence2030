using UnityEngine;

public class QuickInputsGeneratePresets
{
    //[UnityEditor.MenuItem("Tools/Generate Quick Settings")]
    static void Generate()
    {
        GenerateTargets();
        GenerateRecorder();
        GeneratePlayer();
    }

    static void GenerateTargets()
    {
        var playerScriptsPath = $"{Application.dataPath}/../Assets/Meta/PlayerScripts";

        var settings = QuickInputs.NewDefault();
        settings.anyOverride = true;
        
        var drsSettings = settings.dynamicResolutionSettings.value;
        drsSettings.enabled = true;
        drsSettings.enableDLSS = true;
        drsSettings.DLSSUseOptimalSettings = false;
        drsSettings.useMipBias = true;
        drsSettings.forcedPercentage = 60.1f;
        drsSettings.forceResolution = true;
        drsSettings.DLSSPerfQualitySetting = (int)QuickInputs.DLSSQuality.Balanced;
        settings.dynamicResolutionSettings.overrideState = true;
        settings.dynamicResolutionSettings.value = drsSettings;

        settings.presetQualityGeneral = new (){value = QuickSettings.Preset.Medium, overrideState = true};
        settings.presetQualityHair = new (){value = QuickSettings.Preset.Medium, overrideState = true};
        settings.presetQualityShadows = new (){value = QuickSettings.Preset.High, overrideState = true};
        settings.presetQualityDirectionalShadowRays = new (){value = QuickSettings.Preset.Medium, overrideState = true};
        settings.presetQualityTargetDisable = new (){value = QuickSettings.Preset.Medium, overrideState = true};
        settings.presetQualityDomeLights = new (){value = QuickSettings.Preset.Medium, overrideState = true};
        settings.presetQualityFilmGrain = new (){value = QuickSettings.Preset.Off, overrideState = true};

        MakeEditableAndWriteJson($"{playerScriptsPath}/Target.json", JsonUtility.ToJson(settings, true));

        settings.dynamicResolutionSettings.overrideState = false;
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetNative.json", JsonUtility.ToJson(settings, true));
        
        drsSettings.useMipBias = false;
        drsSettings.forcedPercentage = 50f;
        settings.dynamicResolutionSettings.overrideState = true;
        settings.dynamicResolutionSettings.value = drsSettings;
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetXboxSeriesX.json", JsonUtility.ToJson(settings, true));
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetPlaystation5.json", JsonUtility.ToJson(settings, true));

        settings.dynamicResolutionSettings.overrideState = false;
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetXboxSeriesXNative.json", JsonUtility.ToJson(settings, true));
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetPlaystation5Native.json", JsonUtility.ToJson(settings, true));

        settings.dynamicResolutionSettings.overrideState = true;
        settings.presetQualityGeneral = new (){value = QuickSettings.Preset.Low, overrideState = true};
        settings.presetQualityHair = new (){value = QuickSettings.Preset.Low, overrideState = true};
        settings.presetQualityShadows = new (){value = QuickSettings.Preset.Low, overrideState = true};
        settings.presetQualityDirectionalShadowRays = new (){value = QuickSettings.Preset.Off, overrideState = true};
        settings.presetQualityTargetDisable = new (){value = QuickSettings.Preset.Low, overrideState = true};
        settings.presetQualityDomeLights = new (){value = QuickSettings.Preset.Low, overrideState = true};
        settings.presetQualityFilmGrain = new (){value = QuickSettings.Preset.Off, overrideState = true};
        MakeEditableAndWriteJson($"{playerScriptsPath}/TargetXboxSeriesS.json", JsonUtility.ToJson(settings, true));
    }

    static void GenerateRecorder()
    {
        var editorPath = $"{Application.dataPath}/Code/QuickSettings";

        var settings = QuickInputs.NewDefault();
        settings.anyOverride = true;
        
        var drsSettings = settings.dynamicResolutionSettings.value;
        drsSettings.enabled = true;
        drsSettings.enableDLSS = true;
        drsSettings.useMipBias = true;
        drsSettings.forcedPercentage = 200f / 3f;
        drsSettings.forceResolution = true;
        drsSettings.DLSSPerfQualitySetting = (int)QuickInputs.DLSSQuality.MaximumQuality;
        settings.dynamicResolutionSettings.value = drsSettings;
        settings.dynamicResolutionSettings.overrideState = true;

        MakeEditableAndWriteJson($"{editorPath}/Recorder_DLSS.json", JsonUtility.ToJson(settings, true));
    }

    static void GeneratePlayer()
    {
        var playerScriptsPath = $"{Application.dataPath}/../PlayerScripts";

        var settings = QuickInputs.NewDefault();
        var drsSettings = settings.dynamicResolutionSettings.value;
        drsSettings.DLSSUseOptimalSettings = false;
        settings.dynamicResolutionSettings.value = drsSettings;
        
        MakeEditableAndWriteJson($"{playerScriptsPath}/Neutral.json", JsonUtility.ToJson(settings, true));

        drsSettings.enabled = false;
        settings.dynamicResolutionSettings.value = drsSettings;

        MakeEditableAndWriteJson($"{playerScriptsPath}/Native.json", JsonUtility.ToJson(settings, true));

        drsSettings.enabled = true;
        drsSettings.enableDLSS = true;
        drsSettings.useMipBias = false;
        drsSettings.forcedPercentage = 200f / 3f;
        drsSettings.forceResolution = true;
        drsSettings.DLSSPerfQualitySetting = (int)QuickInputs.DLSSQuality.MaximumPerformance;
        settings.anyOverride = true;
        settings.dynamicResolutionSettings.overrideState = true;
        settings.dynamicResolutionSettings.value = drsSettings;
        
        MakeEditableAndWriteJson($"{playerScriptsPath}/DLSS_Fixed_66.json", JsonUtility.ToJson(settings, true));

        drsSettings.useMipBias = true;
        drsSettings.forcedPercentage = 62f;
        drsSettings.DLSSPerfQualitySetting = (int)QuickInputs.DLSSQuality.Balanced;
        settings.dynamicResolutionSettings.value = drsSettings;
        
        MakeEditableAndWriteJson($"{playerScriptsPath}/Target.json", JsonUtility.ToJson(settings, true));

        drsSettings.forcedPercentage = 50f;
        settings.dynamicResolutionSettings.value = drsSettings;

        MakeEditableAndWriteJson($"{playerScriptsPath}/DLSS_Fixed_50.json", JsonUtility.ToJson(settings, true));

        drsSettings.forceResolution = false;
        drsSettings.minPercentage = 40f;
        drsSettings.maxPercentage = 75f;
        settings.dynamicResolutionSettings.value = drsSettings;

        settings.dynamicResolutionTarget.overrideState = true;

        MakeEditableAndWriteJson($"{playerScriptsPath}/DLSS_Dynamic_40-75.json", JsonUtility.ToJson(settings, true));
    }
    
    static void MakeEditable(string path)
    {
        if (UnityEditor.AssetDatabase.AssetPathToGUID(path) != string.Empty)
        {
            UnityEditor.AssetDatabase.MakeEditable(path);
        }
        else
        {
            if (System.IO.File.Exists(path))
            {
                var attrs = System.IO.File.GetAttributes(path);
                attrs &= ~System.IO.FileAttributes.ReadOnly;
                System.IO.File.SetAttributes(path, attrs);
            }
        }
    }
        
    static void MakeEditableAndWriteJson(string path, string json)
    {
        MakeEditable(path);
        System.IO.File.WriteAllText(path, json);
    }
}
