using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AudioSnapshotBlenderMixer : PlayableBehaviour
{
    AudioMixerSnapshot[] m_Snapshots;
    float[] m_Weights;

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        
        var inputCount = playable.GetInputCount();
        m_Snapshots = new AudioMixerSnapshot[inputCount];
        m_Weights = new float[inputCount];

    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var boundMixer = (AudioMixer)playerData;

        if (boundMixer == null)
            return;

        for (int i = 0, n = playable.GetInputCount(); i < n; i++)
        {
            m_Weights[i] = playable.GetInputWeight(i);
            m_Snapshots[i] = ((ScriptPlayable<AudioSnapshotBlenderBehaviour>) playable.GetInput(i)).GetBehaviour().asset.snapshot;
        }

        boundMixer.TransitionToSnapshots(m_Snapshots, m_Weights, 0f);
    }
}
