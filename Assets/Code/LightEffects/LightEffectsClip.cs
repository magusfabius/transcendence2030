using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
class LightEffectsClip : PlayableAsset
{
    [SerializeField]
    internal LightEffectsData data = LightEffectsData.Default; 
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<LightEffectsBehaviour>.Create(graph);
        playable.GetBehaviour().clip = this;
        return playable;
    }
}
