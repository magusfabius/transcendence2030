using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class TimelinePlayOnAwake : MonoBehaviour
{
    public int delayFrames = 5;
    public PlayableDirector playableDirector;

    // Started explicitly in standalone (PlayerLoader)
#if UNITY_EDITOR
    IEnumerator Start()
    {
        if(!playableDirector)
            yield break;

        playableDirector.time = playableDirector.initialTime;
        playableDirector.Evaluate();
        
        for (var i = 0; i < delayFrames; ++i)
            yield return null;
        
        //if(Bootstrap.sArgNoWait)
        playableDirector.Play();
    }
#endif
}
