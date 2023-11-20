using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(LightEffects))]
[TrackClipType(typeof(LightEffectsClip))]
class LightEffectsTrack : TrackAsset
{
    [SerializeField]
    internal LightEffectsData defaultData = LightEffectsData.Default; 

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixerPlayable = ScriptPlayable<LightEffectsMixer>.Create(graph, inputCount);
        mixerPlayable.GetBehaviour().lightEffectsTrack = this;
        return mixerPlayable;
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        var goBinding = GetGameObjectBinding(director);
        if (goBinding)
        {
            var lightEffect = goBinding.GetComponent<LightEffects>();

            if (lightEffect)
                lightEffect.GatherProperties(director, driver);
        }
    }
    
    GameObject GetGameObjectBinding(PlayableDirector director)
    {
        if (director == null)
            return null;

        var binding = director.GetGenericBinding(this);

        var gameObject = binding as GameObject;
        if (gameObject != null)
            return gameObject;

        var comp = binding as Component;
        if (comp != null)
            return comp.gameObject;

        return null;
    }
}
