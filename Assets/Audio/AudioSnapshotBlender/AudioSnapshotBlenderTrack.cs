using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(AudioSnapshotBlenderAsset))]
[TrackBindingType(typeof(AudioMixer))]
public class AudioSnapshotBlenderTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
        return ScriptPlayable<AudioSnapshotBlenderMixer>.Create(graph, inputCount);
    }
}
