//using SceneMaster;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using TimelineBuddy;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.Timeline;
using UnityEngine.UI;
//#define PROFILERDUMP

public class PlayerLoader : MonoBehaviour
{
    public static bool AutoStart;

    public MultiSceneController sceneController;
    public PreWarming preWarming;
    public TimelineAsset mainDirectorAsset;

    [Header("UI")]
    public Text progressText;
    public Slider progressSlider;

    [Space]
    public GameObject[] activatePreLoading;
    public GameObject[] activateForLoading;
    public GameObject[] activatePostLoading;

    public bool debugLoadInEditor;

    private void Reset()
    {
        sceneController = GetComponent<MultiSceneController>();
        preWarming = GetComponent<PreWarming>();
    }

    IEnumerator Start()
    {
        if (Application.isEditor && !debugLoadInEditor)
            yield break;

        foreach (var go in activatePreLoading)
            go.SetActive(true);

        foreach (var go in activateForLoading)
            go.SetActive(true);

        yield return null;

        var originalBackgroundLoadingPriority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        AudioListener.volume = 0f;

        var pathList = sceneController.mainScenePath;
        for (int i = 1; i < pathList.scenePaths.Length; ++i)
            yield return StartCoroutine(LoadScene(i, pathList));

        Application.backgroundLoadingPriority = originalBackgroundLoadingPriority;

        yield return null;

        progressText.text = "STARTING";
        progressSlider.value = 0.95f;

        Time.timeScale = 1f;

        yield return null;

        if (preWarming != null)
        {
            if(Bootstrap.sArgNoPreWarm)
                Debug.Log("Requested pre-warm skip.");
            else
                yield return StartCoroutine(preWarming.PreWarm());
        }


        //progressText.text = "READY: PRESS ANY KEY";
        progressText.text = "READY";
        progressSlider.value = 1;
        //progressSlider.enabled = false;
        progressSlider.gameObject.SetActive(false);

        var allowPause = true;
#if PROFILERDUMP
        var isProfilerDump = Environment.GetCommandLineArgs().Any(a => a.ToLowerInvariant().Contains("profilerdump"));

        var profilerDump = FindObjectOfType<ProfilerDump>();
        if (profilerDump != null && !isProfilerDump)
        {
            Destroy(profilerDump);
            profilerDump = null;
        }
        allowPause &= profilerDump == null || profilerDump.isActiveAndEnabled;
#endif
        if (Bootstrap.sArgNoWait)
            allowPause = false;
        
        var mainDirector = FindObjectsOfType<PlayableDirector>().FirstOrDefault(pd => pd.playableAsset == mainDirectorAsset);
        if (allowPause)
        {
            if (mainDirector)
                mainDirector.Pause();

            while (!Input.anyKey && !AutoStart)
                yield return null;
        }

        Input.ResetInputAxes();
        AudioListener.volume = Bootstrap.sArgMute ? 0f : 1f;

        foreach (var go in activateForLoading)
            go.SetActive(false);

        foreach (var go in activatePostLoading)
            go.SetActive(true);

        for (var i = 0; i < 10; ++i)
            yield return null;

        if (mainDirector)
        {
            if (Bootstrap.sArgDSPTime)
                mainDirector.timeUpdateMode = DirectorUpdateMode.DSPClock;

            mainDirector.extrapolationMode = DirectorWrapMode.Loop;
            mainDirector.time = mainDirector.initialTime;
            mainDirector.Evaluate();
            mainDirector.Play();
        }

#if PROFILERDUMP
        if (profilerDump != null && profilerDump.isActiveAndEnabled)
        {
            yield return StartCoroutine(profilerDump.DowIt(SceneObject.FindSceneObject(mainDirector_).GetComponent<PlayableDirector>(), preWarming != null ? preWarming.cameraControlTrack : null));
        }
#endif

        // Setup a simple looper for the entire cinematic.
        var goLooper = new GameObject();
        var looper = goLooper.AddComponent<TimelineRangeLooper>();
        looper.playableDirector = mainDirector;
        looper.anyState = true;
        looper.endTime = mainDirector.duration;

        // Disable garbage collection, and instead collect on each loop.
        GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
        TimelineRangeLooper.sTimelineLooping += () =>
        {
            Debug.Log("Garbage collecting on loop.");
            System.GC.Collect();
        };
        
        // Clear this after loading for simplicity (can always check log)
        Debug.developerConsoleVisible = false;
        
#if DEVELOPMENT_BUILD
        // Kick off trace if requested
        if (Bootstrap.sArgTrace)
        {
            mainDirector.extrapolationMode = DirectorWrapMode.Hold;
            MiniTrace.Start();
        }
#endif
    }

    IEnumerator LoadScene(int index, MultiSceneController.ScenePathList pathList)
    {
        var asyncOp = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        if (asyncOp != null)
        {
            progressText.text = "LOADING";
            while (!asyncOp.isDone)
            {
                progressSlider.value = (((index - 1) + asyncOp.progress) / (pathList.scenePaths.Length - 1)) * 0.9f;
                yield return null;
            }

            var scene = SceneManager.GetSceneByBuildIndex(index);
            //SceneObject.RegisterAllSceneObjectsInScene(scene);

            if (index == pathList.activeSceneIndex)
                SceneManager.SetActiveScene(scene);
            
            // Make sure to collect garbage after loading has completed
            System.GC.Collect();
        }
    }
}
