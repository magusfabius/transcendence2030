using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

class GroupBlendClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField]
    internal ExposedReference<GroupBlendTarget> target;

    public ClipCaps clipCaps => ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<GroupBlendBehaviour>.Create(graph);
        playable.GetBehaviour().target = target.Resolve(graph.GetResolver());
        return playable;
    }
}
