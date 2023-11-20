using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Code.ComponentActivationTrack
{
    [TrackClipType(typeof(ComponentActivationAsset))]
    public class ComponentActivationTrackAsset : TrackAsset
    {   
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<ComponentActivationTrackMixerBehaviour>.Create(graph, inputCount);
        }
    }
}
