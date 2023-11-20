using UnityEngine.Playables;

namespace Code.ComponentActivationTrack
{
    public class ComponentActivationTrackMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object _)
        {
            for (int i = 0, n = playable.GetInputCount(); i < n; i++)
            {
                var inputPlayable = (ScriptPlayable<ComponentActivationBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                if (input.target)
                    input.target.enabled = playable.GetInputWeight(i) > 0f;
            }
        }
    }
}
