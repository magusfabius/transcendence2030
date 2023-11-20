using UnityEngine.Experimental.Rendering;
using UnityEngine.Playables;

namespace Code.DynamicGeometryControlTrack
{
    public class DynamicGeometryControlMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object _)
        {
            for (int i = 0, n = playable.GetInputCount(); i < n; i++)
            {
                var inputPlayable = (ScriptPlayable<DynamicGeometryControlBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                var onMode = input.promoteMode ? RayTracingMode.DynamicGeometry : RayTracingMode.DynamicTransform;
                var offMode = input.promoteMode ? RayTracingMode.DynamicTransform : RayTracingMode.DynamicGeometry;
                
                foreach (var target in input.targets)
                {
                    target.rayTracingMode = playable.GetInputWeight(i) > 0f ? onMode : offMode;
                }
            }
        }
    }
}
