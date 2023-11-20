using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(GroupBlendClip))]
[TrackColor(0.8f, 1.0f, 0.8f)]
class GroupBlendTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) =>
        ScriptPlayable<GroupBlendMixer>.Create(graph, inputCount);
    
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        foreach (var timelineClip in GetClips())
        {
            var groupBlendClip = (GroupBlendClip) timelineClip.asset;
            var groupBlendTarget = groupBlendClip.target.Resolve(director);
            if (groupBlendTarget)
                groupBlendTarget.GatherProperties(director, driver);
        }
    }
}
