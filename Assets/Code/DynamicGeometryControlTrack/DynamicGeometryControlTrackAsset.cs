using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Code.DynamicGeometryControlTrack
{
    [TrackClipType(typeof(DynamicGeometryControlPlayableAsset))]
    public class DynamicGeometryControlTrackAsset : TrackAsset
    {   
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<DynamicGeometryControlMixerBehaviour>.Create(graph, inputCount);
        }
    }
}
