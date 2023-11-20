using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;


public class DebugController : MonoBehaviour {
	public PlayableDirector playableDirector;

	public int[] freezeFrames = { 1333, 50 };

	public GameObject statsRoot;
	public GameObject blackRoot;
	public Text fpsText;
	public Text fpsTextSecondary;
	
	enum ShowFPSMode { Off, FPSOnly, FPSAndScale, Count }
	enum ShowHelpMode { Off, Visuals, VisualsAndNavigation, Count }
	
	int m_NextFreezeFrame;

	ShowFPSMode m_showFPS;
	bool m_showPerfStats;
	ShowHelpMode m_showHelp;

	FrameTiming[] m_FrameTimings = new FrameTiming[1];
	
	const int kWindowSize = 8;
	float[] m_FrameWindow = new float[kWindowSize];
	int m_NextWindowPos;

	void OnEnable()
	{
		if (m_showFPS == ShowFPSMode.Off && Bootstrap.sArgShowFPS)
			SetFPSMode(ShowFPSMode.FPSOnly);
	}

	void SetFPSMode(ShowFPSMode mode)
	{
		m_showFPS = mode;
		statsRoot.SetActive(m_showFPS != ShowFPSMode.Off);
		fpsTextSecondary.gameObject.SetActive(m_showFPS == ShowFPSMode.FPSAndScale);
	}
	
	void LateUpdate()
	{
		if(m_showPerfStats || m_showFPS != ShowFPSMode.Off)
			FrameTimingManager.CaptureFrameTimings();

		var anyShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		if (Input.GetKeyDown(KeyCode.F1))
		{
			QuickInputs.ShowFPS = QuickInputs.ShowFPS == 0 ? 1 : 0;
		}

		if (anyShift && Input.GetKeyDown(KeyCode.F1))
		{
			QuickInputs.ShowFPS = QuickInputs.ShowFPS != 2 ? 2 : 0;
		}

		// TODO: Move the rest of the navigation controls to demo controller.

		return;
		
		if (Input.GetKeyDown(KeyCode.F1))
		{
			SetFPSMode(m_showFPS == ShowFPSMode.Off ? ShowFPSMode.FPSOnly : ShowFPSMode.Off);
		}

		if (anyShift && Input.GetKeyDown(KeyCode.F1))
		{
			if(m_showFPS == ShowFPSMode.FPSAndScale)
				SetFPSMode(ShowFPSMode.FPSOnly);
			else
				SetFPSMode(ShowFPSMode.FPSAndScale);
		}

		if (Input.GetKeyDown(KeyCode.F3))
			m_showPerfStats = !m_showPerfStats;

		if (Input.GetKeyDown(KeyCode.F2))
			m_showHelp = (ShowHelpMode)(((int)m_showHelp + 1) % (int)ShowHelpMode.Count);
	
	
		if(freezeFrames.Length > 0 && Input.GetKeyDown(KeyCode.J))
		{
			var f = freezeFrames[m_NextFreezeFrame];
			m_NextFreezeFrame = (m_NextFreezeFrame + freezeFrames.Length + (anyShift ? -1 : 1)) % freezeFrames.Length;

			playableDirector.Pause();
			playableDirector.time = f / 30.0;
			playableDirector.Evaluate();
		}

		// Skip cursor inputs when debug panel is open to avoid conflicts (unless in editor in which case we assume editor GUI is used)
		if (Application.isEditor || DebugManager.instance == null || !DebugManager.instance.isAnyDebugUIActive)
		{
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				playableDirector.time = 0.0;

				if(playableDirector.state == PlayState.Paused)
					playableDirector.Evaluate();
			}
			
			if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				playableDirector.time = playableDirector.duration;

				if(playableDirector.state == PlayState.Paused)
					playableDirector.Evaluate();
			}
			
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				playableDirector.time -= anyShift ? 10.0 : 1.0;

				if(playableDirector.state == PlayState.Paused)
					playableDirector.Evaluate();
			}

			if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				playableDirector.time += anyShift ? 10.0 : 1.0;

				if(playableDirector.state == PlayState.Paused)
					playableDirector.Evaluate();
			}
		}

		if(Input.GetKeyDown(KeyCode.Comma))
		{
			playableDirector.time -= 1.0/30.0;

			if(playableDirector.state == PlayState.Paused)
				playableDirector.Evaluate();
		}

		if(Input.GetKeyDown(KeyCode.Period))
		{
			playableDirector.time += 1.0/30.0;

			if(playableDirector.state == PlayState.Paused)
				playableDirector.Evaluate();
		}
	
	}
	
	void OnGUI()
	{
		if (QuickInputs.ShowFPS > 0 && m_showFPS != ShowFPSMode.FPSAndScale)
		{
			SetFPSMode(ShowFPSMode.FPSAndScale);
		}

		if (QuickInputs.ShowFPS == 0 && m_showFPS != ShowFPSMode.Off)
		{
			SetFPSMode(ShowFPSMode.Off);
		}


		if (m_showFPS == ShowFPSMode.Off && !m_showPerfStats && m_showHelp == ShowHelpMode.Off)
			return;
		
		var windowMargined = new Rect(5f, 5f, Screen.width - 10f, Screen.height - 10f);
		using (new GUILayout.AreaScope(windowMargined))
		{
			GUI.color = Color.green;
			
			if (m_showFPS != ShowFPSMode.Off || m_showPerfStats)
				ShowFPS();

			if (m_showPerfStats)
				ShowPerfStats();

			if (m_showHelp != ShowHelpMode.Off)
				ShowHelp();
		}
	}

	static readonly string[] kModes = { "NATIVE", /*"TAAU",*/ "FSR", "DLSS" };

	void ShowFPS()
	{
		if (FrameTimingManager.IsFeatureEnabled())
		{
			var received = FrameTimingManager.GetLatestTimings(1, m_FrameTimings);
			if (received >= 1)
			{
				ref var ft = ref m_FrameTimings[received - 1];
				var fps = (float)(1000.0 / ft.cpuFrameTime);
				m_FrameWindow[m_NextWindowPos++ % kWindowSize] = fps;

				var avgFps = 0f;
				var validSamples = Mathf.Min(kWindowSize, m_NextWindowPos);
				for (var i = 0; i < validSamples; ++i)
					avgFps += m_FrameWindow[i];
				avgFps /= validSamples;
				
				if (m_showFPS == ShowFPSMode.FPSOnly)
				{
					fpsText.text = $"{Mathf.RoundToInt(avgFps)}";
				}
				else if (m_showFPS == ShowFPSMode.FPSAndScale)
				{
					var w = Screen.width;
					var h = Screen.height;
					var r = Screen.currentResolution.refreshRate;
					
					var lines = $"{w}x{h}@{r}\n";

					var mode = QuickInputs.UpscaleMode;
					if (mode == 0)
					{
						lines += kModes[0] + "\n";
					}
					else
					{
						var sw = Mathf.RoundToInt(w * ft.widthScale);
						var sh = Mathf.RoundToInt(h * ft.heightScale);
						lines += $"{kModes[mode]} {sw}x{sh}/{ft.widthScale:F2}\n";
					}
					lines += "VSYNC " + (QuickInputs.VSync == 0 ? "OFF" : $"ON ({QuickInputs.VSync})");

					fpsText.text = $"{avgFps:F1}";
					fpsTextSecondary.text = lines;
				}
			}
		}
	}

	void ShowPerfStats()
	{
		if (!FrameTimingManager.IsFeatureEnabled())
			return;
		
		var received = FrameTimingManager.GetLatestTimings(1, m_FrameTimings);
		if (received <= 0)
			return;

		ref var ft = ref m_FrameTimings[received - 1];
		GUILayout.Label($"GPU Time:        {(int)ft.gpuFrameTime} ms");
		GUILayout.Label($"CPU Time:        {(int)ft.cpuFrameTime} ms");
		GUILayout.Label($"  Main Thread:   {(int)ft.cpuMainThreadFrameTime}");
		GUILayout.Label($"    Present:     {(int)ft.cpuMainThreadPresentWaitTime}");
		GUILayout.Label($"  Render Thread: {(int)ft.cpuRenderThreadFrameTime}");
		GUILayout.Label($"Sync:            {(int)ft.syncInterval}");
		GUILayout.Label($"DRS Width:       {ft.widthScale}");
		GUILayout.Label($"DRS Height:      {ft.heightScale}");
		GUILayout.Space(20f);
	}

	void ShowHelp()
	{
		if (m_showHelp != ShowHelpMode.VisualsAndNavigation)
			return;
		
		GUILayout.Space(10);

		if(freezeFrames.Length > 0)
			GUILayout.Label($"[J] Freeze Frame:   {freezeFrames[m_NextFreezeFrame]}");

		GUILayout.Space(10);

		GUILayout.Label($"[SPACE] Play State: {playableDirector.state}");

		GUILayout.Space(10);

		GUILayout.Label($"[Up] Jump Beginning");
		GUILayout.Label($"[Down] Jump End");
		GUILayout.Label($"[,] Rewind 1 frame");
		GUILayout.Label($"[Left] Rewind 1 second");
		GUILayout.Label($"[Shift+Left] Rewind 10 seconds");
		GUILayout.Label($"[.] Forward 1 frame");
		GUILayout.Label($"[Right] Forward 1 second");
		GUILayout.Label($"[Shift+Right] Forward 10 seconds");
	}
}
