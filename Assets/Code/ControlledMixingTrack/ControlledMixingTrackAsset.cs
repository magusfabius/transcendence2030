using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
 
[TrackClipType(typeof(AnimationPlayableAsset))]
[TrackBindingType(typeof(Animator))]
class ControlledMixingTrackAsset : TrackAsset, ILayerable
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return AnimationMixerPlayable.Create(graph, inputCount);
    }
 
    public Playable CreateLayerMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var layerMixer = AnimationLayerMixerPlayable.Create(graph, inputCount + 1);
 
        var mixerPlayable = ScriptPlayable<MixerPlayableBehaviour>.Create(graph, 0);
        mixerPlayable.GetBehaviour().layerMixerPlayable = layerMixer;

        graph.Connect(mixerPlayable, 0, layerMixer, inputCount);
        
        var director = go.GetComponent<PlayableDirector>();
        var controller = director.GetGenericBinding(this) as Animator;
        var playableOutput = AnimationPlayableOutput.Create(graph, "ControlledLayerMixer", controller);
        playableOutput.SetSourcePlayable(layerMixer);
 
        return layerMixer;
    }
}
