using System;
using UnityEngine;
using UnityEngine.Profiling;

#if DEVELOPMENT_BUILD
public class MiniTrace
{
    static string sProfilerBasePath;
    static int sProfilerStartFrame;
    static string sNextProfilerPath => $"{sProfilerBasePath}{sProfilerStartFrame}";
    
    static public void Start()
    {
        Debug.Log("Starting MiniTrace with requested areas " + string.Join(",", Bootstrap.sArgTraceAreas));
        
        // Activate only requested areas
        foreach (var area in Enum.GetValues(typeof(ProfilerArea)))
        {
            var active = System.Array.IndexOf(Bootstrap.sArgTraceAreas, area.ToString()) != -1;
            Profiler.SetAreaEnabled((ProfilerArea)area, active);
            Debug.Log($"..area {area} {(active ? "active" : "inactive")}.");
        }

        // Make sure the app runs in background
        Application.runInBackground = true;
        
        // Request application quit at end of timeline.
        TimelineRangeLooper.sTimelineLooping += () => Application.Quit();

        // Stop profiling when quitting
        Application.quitting += () => Profiler.enabled = false;

        // 2 GB buffer to avoid flushing to disk during run
        Profiler.maxUsedMemory = int.MaxValue;
        
        // Setup base path (have to slice data into multiple segments to view them)
        var time = DateTime.Now.ToString("yyyyMMdd'_'HHmmss");
        sProfilerBasePath = $"{Application.dataPath}/../{time}-trace-";

        // Since we can only view a maximum of 2000 frames in the profiler view, we'll restart every 2000 frames
        Application.onBeforeRender += () =>
        {
            if(Time.frameCount - sProfilerStartFrame >= 1999)
                ReStartProfilerGroup();
        };

        // Start the first group
        ReStartProfilerGroup();
    }

    static void ReStartProfilerGroup()
    {
        Profiler.enabled = false;

        sProfilerStartFrame = Time.frameCount;
        Profiler.logFile = sNextProfilerPath;
        
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;
    }
}
#endif
