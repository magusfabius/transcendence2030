//using SceneMaster;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class PreWarming : MonoBehaviour
{
    public ShaderVariantCollection[] variantCollections;

    public TimelineAsset mainDirectorAsset;
    public TrackAsset[] warmupTracks;

    public static event Action onComplete;

    public int[] specificFrames;
    
    public IEnumerator PreWarm()
    {
        foreach (var variantCollection in variantCollections)
        {
            if(variantCollection == null)
                continue;
            
            Debug.LogFormat("PreWarm SVC {0}", variantCollection.name);

            variantCollection.WarmUp();
            yield return null;
        }

        var mainDirector = FindObjectsOfType<PlayableDirector>().FirstOrDefault(pd => pd.playableAsset == mainDirectorAsset);
        if (mainDirector != null)
        {
            var preWarmStartTime = Time.realtimeSinceStartup;

            var startState = mainDirector.state;
            var startTime = mainDirector.time;
            var startVolume = AudioListener.volume;

            mainDirector.Stop();
            AudioListener.volume = 0f;

#if false
            var timelineAssets = Resources.FindObjectsOfTypeAll<TimelineAsset>();
            var activationClips = timelineAssets.SelectMany(tla => tla.GetRootTracks().Where(track => track is ActivationTrack || track is ControlTrack || track is SceneControlTrack).SelectMany(track => track.GetClips())).ToArray();
            Debug.LogFormat("activation/control/sceneClips {0}", activationClips.Length);

            foreach (var ac in activationClips)
                Debug.LogFormat("Activation/Control {0} {1}", ac.start, ac.displayName);

            var clipSet = new HashSet<TimelineClip>(activationClips);
            foreach (var warmupTrack in warmupTracks)
            {
                clipSet.UnionWith(warmupTrack.GetClips());
            }
            var clips = clipSet.ToList();
            clips.Sort((a, b) => a.start.CompareTo(b.start));

            foreach (var clip in clips)
            {
                var elapsed = Time.realtimeSinceStartup - preWarmStartTime;
                Debug.LogFormat("PreWarm time {0}/{1} (shot: {2}) (time/frame {3}/{4})",
                    clip.start, Mathf.RoundToInt((float)(clip.start * 30)), clip.displayName, Time.realtimeSinceStartup - preWarmStartTime, Time.renderedFrameCount);
                mainDirector.time = clip.start;
                mainDirector.Evaluate();
                yield return null;
            }
#else
            if (specificFrames != null)
            {
                foreach(int frame in specificFrames)
                {
                    mainDirector.time = frame / mainDirectorAsset.editorSettings.fps;
                    mainDirector.Evaluate();
                    yield return null;
                }
            }

            double kStep = 2.0;
            if (QuickSettings.Instance)
            {
                if (QuickSettings.Instance.AppliedInputs?.preWarmTimeStep.overrideState ?? false)
                    kStep = QuickSettings.Instance.AppliedInputs.preWarmTimeStep.value;
            }

            if (kStep > 0f)
            {
#if false
                for (var t = mainDirector.duration - kStep; t > kStep; t -= kStep)
                {
                    mainDirector.time = t;
                    mainDirector.Evaluate();
                    yield return null;
                }
#else
                for (var t = kStep; t < mainDirector.duration - kStep; t += kStep)
                {
                    mainDirector.time = t;
                    mainDirector.Evaluate();
                    yield return null;
                }
#endif
            }
#endif

            AudioListener.volume = startVolume;

 #if PROFILERDUMP
            mainDirector.time = startTime;
            mainDirector.Evaluate();
            if (startState == PlayState.Playing)
                mainDirector.Play();
#endif
        }

        onComplete?.Invoke();
    }
}
