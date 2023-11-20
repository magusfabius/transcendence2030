using System.Collections.Generic;
using UnityEngine.Playables;

class GroupBlendMixer : PlayableBehaviour
{
    Dictionary<GroupBlendTarget, float> m_AccumulatedWeights = new();
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        for (int i = 0, n = playable.GetInputCount(); i < n; i++)
        {
            var inputPlayable = (ScriptPlayable<GroupBlendBehaviour>)playable.GetInput(i);
            var inputBehaviour = inputPlayable.GetBehaviour();

            if (inputBehaviour.target)
            {
                m_AccumulatedWeights.TryGetValue(inputBehaviour.target, out var weight);
                m_AccumulatedWeights[inputBehaviour.target] = weight + playable.GetInputWeight(i);
            }
        }

        foreach (var accumulatedWeight in m_AccumulatedWeights)
            accumulatedWeight.Key.ApplyData(accumulatedWeight.Value);
        
        m_AccumulatedWeights.Clear();
    }
}
