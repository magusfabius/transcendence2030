using UnityEngine;
using UnityEngine.SceneManagement;
using Path = System.IO.Path;

public class Bootstrap
{
	static bool sHasBootstrapped;

	public static bool sArgShowFPS { get; private set; }
	public static bool sArgAllowAnyScreen { get; private set; }
	public static bool sArgClearSettings { get; private set; }
	public static bool sArgNoPreWarm { get; private set; }
	public static bool sArgNoWait { get; private set; } = true;
	public static bool sArgDSPTime { get; private set; }
	public static bool sArgMute { get; private set; }
	public static bool sArgTrace { get; private set; }
	public static string[] sArgTraceAreas { get; private set; }
	
#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	static void HookEditorStaticReset()
	{
		UnityEditor.EditorApplication.playModeStateChanged += change =>
		{
			if (change == UnityEditor.PlayModeStateChange.ExitingEditMode)
			{
				sHasBootstrapped = false;
			}
		};
	}
#endif
	
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
	    if (sHasBootstrapped)
	    {
		    ApplyRepeating();
		    Debug.Log("[BOOTSTRAP] AfterSceneLoad event post-initialization, running ApplyRepeating only.");
		    return;
	    }

	    var quickSetting = GameObject.FindObjectOfType<QuickSettings>();
	    if(quickSetting == null)
	    {
		    Debug.Log("[BOOTSTRAP] Skipping AfterSceneLoad event as no QuickSettings object was found in scene.");
		    return;
	    }

	    // Mark as initialized (even if things fail below)
	    Debug.Log("[BOOTSTRAP] Found QuickSettings from AfterSceneLoad event.");
	    sHasBootstrapped = true;

	    string sArgOverrideWidth = null, sArgOverrideHeight = null, sArgOverrideFullscreen = null;
	    string argQualityNames = null;
	    string argPresetName = null;
		var cmdLineArgs = System.Environment.GetCommandLineArgs();
		for (int i = 0, n = cmdLineArgs.Length; i < n; ++i)
		{
			if (cmdLineArgs[i].ToLowerInvariant() == "-quality" && (i + 1) < n)
			{
				argQualityNames = cmdLineArgs[++i];
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-preset" && (i + 1) < n)
			{
				argPresetName = cmdLineArgs[++i];
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-screen-width" && (i + 1) < n)
			{
				sArgOverrideWidth = cmdLineArgs[++i];
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-screen-height" && (i + 1) < n)
			{
				sArgOverrideHeight = cmdLineArgs[++i];
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-screen-fullscreen" && (i + 1) < n)
			{
				sArgOverrideFullscreen = cmdLineArgs[++i];
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-screen-any")
			{
				sArgAllowAnyScreen = true;
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-reset-settings")
			{
				sArgClearSettings = true;
			}
			
			if (cmdLineArgs[i].ToLowerInvariant() == "-mute")
			{
				sArgMute = true;
			}
			
			if (cmdLineArgs[i].ToLowerInvariant() == "-showfps")
			{
				sArgShowFPS = true;
			}
			
			if (cmdLineArgs[i].ToLowerInvariant() == "-noprewarm")
			{
				sArgNoPreWarm = true;
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-nowait")
			{
				sArgNoWait = true;
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-wait")
			{
				sArgNoWait = false;
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-dsptime")
			{
				sArgDSPTime = true;
			}

			if (cmdLineArgs[i].ToLowerInvariant() == "-trace" && (i + 1) < n)
			{
				sArgTrace = true;
				sArgNoWait = true; // force nowait with trace
				sArgTraceAreas = cmdLineArgs[++i].Split(",");
			}
		}
		
#if UNITY_GAMECORE_XBOXSERIES
	    if (string.IsNullOrEmpty(argQualityNames))
	    {
#if UNITY_EDITOR
		    argQualityNames = "TargetXboxSeriesX";
#else
		    var isSeriesS = UnityEngine.GameCore.Hardware.version == UnityEngine.GameCore.HardwareVersion.XboxSeriesS;
		    argQualityNames = isSeriesS ? "TargetXboxSeriesS" : "TargetXboxSeriesX";
#endif
		    Debug.Log($"[BOOTSTRAP] No '-quality' command-line argument provided, defaulting to '{argQualityNames}'.");
	    }
#endif
	    
#if UNITY_PLAYSTATION
	    if (string.IsNullOrEmpty(argQualityNames))
	    {
#if UNITY_EDITOR
		    argQualityNames = "TargetPlaystation5";
#else
		    
		    argQualityNames = "TargetPlaystation5";
#endif
		    Debug.Log($"[BOOTSTRAP] No '-quality' command-line argument provided, defaulting to '{argQualityNames}'.");
	    }
#endif

#if PLATFORM_STANDALONE
	    if (!string.IsNullOrEmpty(argPresetName))
	    {
		    Debug.Log($"[BOOTSTRAP] Quality '-preset' override '{argPresetName}' provided.");
	    }
#endif
	    
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
		// Turn off stack traces for log type to reduce hiccups from logging.
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
	    
		Debug.unityLogger.logEnabled = false;
#endif
	    
		ApplyRepeating();
		
		// Every time we load a scene, we might have repeat actions to perform. 
		SceneManager.sceneLoaded += (_, _) => ApplyRepeating();


#if !PLATFORM_STANDALONE
		if (string.IsNullOrEmpty(argQualityNames))
		{
			Debug.Log("[BOOTSTRAP] No-op AfterSceneLoad event as no '-quality <name>' argument was provided..");
			return;
		}
#endif

	    QuickInputs quickInputs = null;
		try
		{
			if (sArgClearSettings || Application.isEditor)
			{
				Debug.Log($"[BOOTSTRAP] Clearing settings.");
				QuickInputs.ResetSettings();
			}

			QuickInputs.ShowFPS = sArgShowFPS ? 1 : 0;
			QuickInputs.VSync = 0;
			
#if PLATFORM_STANDALONE
	#if UNITY_EDITOR
			if (string.IsNullOrEmpty(argPresetName))
			{
				argPresetName = quickSetting.editorPlaymodeDefaultQuality.ToString();
				Debug.Log($"[BOOTSTRAP] Forcing editor preview quality to {argPresetName}..");
			}
			
			var configsDir = Path.Combine(Application.dataPath, "..", "Assets", "Meta", "PlayerScripts", "Config");
	#else
			#if UNITY_STANDALONE_OSX
			var configsDir = Path.Combine(Application.dataPath, "..", "..", "Config");
			#else
			var configsDir = Path.Combine(Application.dataPath, "..", "Config");
			#endif
	#endif

			QuickInputs.ConfigsDir = configsDir;
			quickInputs = QuickInputs.ProduceFromOverrides(argPresetName, sArgOverrideWidth, sArgOverrideHeight, sArgOverrideFullscreen,
				out var presetName, out var presetData, out var screenWidth, out var screenHeight, out var screenRefresh, out var screenFullscreen, out var upscaleMode);

			Debug.Log($"[BOOTSTRAP] Attempting to apply quality preset.");
			quickSetting.Apply(quickInputs);
			
			quickSetting.SetPreset(presetName, presetData);
			quickSetting.SetResolution(screenWidth, screenHeight, screenRefresh, screenFullscreen);
			quickSetting.SetUpscale(upscaleMode);
#else
	#if UNITY_EDITOR
			var qualitiesDir = Path.Combine(Application.dataPath, "..", "Assets", "Meta", "PlayerScripts");
	#else
			#if UNITY_STANDALONE_OSX
			var qualitiesDir = Path.Combine(Application.dataPath, "..", "..");
			#else
			var qualitiesDir = Path.Combine(Application.dataPath, "..");
			#endif
	#endif

			var argQualityNamesSplit = argQualityNames.Split(",");
			var qualityPath = Path.Combine(qualitiesDir, $"{argQualityNamesSplit[0]}.json");
		
			if(argQualityNamesSplit.Length == 1)
				Debug.Log($"[BOOTSTRAP] Attempting to load quality preset '{argQualityNamesSplit[0]}' from '{qualityPath}'.");
			else
				Debug.Log($"[BOOTSTRAP] Attempting to load quality preset '{argQualityNamesSplit[0]}' of group '{argQualityNames}' from '{qualityPath}'.");
			
			quickInputs = QuickInputs.Produce(qualityPath);

			for (var i = 1; i < argQualityNamesSplit.Length; ++i)
			{
				qualityPath = Path.Combine(qualitiesDir, $"{argQualityNamesSplit[i]}.json");
		
				Debug.Log($"[BOOTSTRAP] Attempting to load quality preset '{argQualityNamesSplit[i]}' of group '{argQualityNames}' from '{qualityPath}'.");
				var overrideInputs = QuickInputs.Produce(qualityPath);
				quickInputs.Override(overrideInputs);
			}

			Debug.Log($"[BOOTSTRAP] Attempting to apply quality preset '{argQualityNames}'.");
			quickSetting.Apply(quickInputs);
#endif
		}
		catch(System.Exception)
		{
			Debug.LogError($"[BOOTSTRAP] Failed to load quality preset(s) '{argQualityNames}'. Expected format is <name> or <name>,<name>,<name> matching file names not including extension.");
		}
		
		Debug.Log($"[BOOTSTRAP] All done.");
	}

    static void ApplyRepeating()
    {
	    Debug.developerConsoleVisible = false;
    }
}
