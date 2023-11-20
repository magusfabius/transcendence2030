using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Code.DynamicGeometryControlTrack
{
    public class DynamicGeometryControlPlayableAsset : PlayableAsset, IPropertyPreview
    {
        public bool promoteMode;
        public ExposedReference<SkinnedMeshRenderer>[] targets;

        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DynamicGeometryControlBehaviour>.Create(graph);

            var playableBehaviour = playable.GetBehaviour();
            playableBehaviour.promoteMode = promoteMode;
            playableBehaviour.targets = targets.Select(t => t.Resolve(graph.GetResolver())).ToArray();

            return playable;
        }

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            foreach (var target in targets)
            {
                var resolve = target.Resolve(director);
                driver.AddFromName(resolve, "m_RayTracingMode");
            }
        }
    }
}