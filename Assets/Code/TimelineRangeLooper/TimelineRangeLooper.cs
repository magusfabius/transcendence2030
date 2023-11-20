using UnityEngine;
using UnityEngine.Playables;

public class TimelineRangeLooper : MonoBehaviour
{
    public PlayableDirector playableDirector;

    public bool anyState;
    public double endTime; 

    double m_InitialTime;

    public static System.Action sTimelineLooping;
    
    void Start()
    {
        if (playableDirector != null)
        {
            m_InitialTime = playableDirector.initialTime;
            endTime = System.Math.Clamp(endTime, m_InitialTime + 1.0, playableDirector.duration);
        }
    }
    
    void Update()
    {
        if (playableDirector != null)
        {
            if(playableDirector.state == PlayState.Playing || anyState)
            {
                var failSafeButExpensiveLoopCheck = (playableDirector.time == m_InitialTime && playableDirector.state == PlayState.Paused); 
                if (playableDirector.time < m_InitialTime || playableDirector.time >= endTime - 0.25 || failSafeButExpensiveLoopCheck)
                {
                    Debug.Log($"Looping (after {sTimelineLooping?.GetInvocationList().Length ?? 0} callbacks). Expensive version: {failSafeButExpensiveLoopCheck}");
                    sTimelineLooping?.Invoke();
                    playableDirector.time = m_InitialTime;
                    playableDirector.Play();
                }
            }
        }
    }
}
