using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Code.ComponentActivationTrack
{
    public class ComponentActivationAsset : PlayableAsset, IPropertyPreview
    {
        public ExposedReference<MonoBehaviour> behaviour;

        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ComponentActivationBehaviour>.Create(graph);

            var playableBehaviour = playable.GetBehaviour();
            playableBehaviour.target = behaviour.Resolve(graph.GetResolver());

            return playable;
        }

        public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var resolvedBehaviour = behaviour.Resolve(director);
            driver.AddFromName(resolvedBehaviour, "m_Enabled");
        }
    }
}