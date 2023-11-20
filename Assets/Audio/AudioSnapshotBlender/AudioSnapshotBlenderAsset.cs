using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class AudioSnapshotBlenderAsset : PlayableAsset
{
    public AudioMixerSnapshot snapshot;

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<AudioSnapshotBlenderBehaviour>.Create(graph);
        playable.GetBehaviour().asset = this;
        return playable;   
    }
}