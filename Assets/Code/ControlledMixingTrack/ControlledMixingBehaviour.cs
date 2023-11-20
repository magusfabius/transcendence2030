using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

class MixerPlayableBehaviour : PlayableBehaviour
{
    public AnimationLayerMixerPlayable layerMixerPlayable;

    float m_Value1;
    float m_Value2; 
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        m_Value1 = 0f;
        m_Value2 = 0f;
        
        base.OnBehaviourPause(playable, info);
    }
    
    public void OnUpdateValue1(float value) => m_Value1 = value;
    public void OnUpdateValue2(float value) => m_Value2 = value;

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        var totalWeight = m_Value1 + m_Value2;
        if (totalWeight > 1f)
        {
            m_Value1 /= totalWeight;
            m_Value2 /= totalWeight;
        }

        layerMixerPlayable.SetInputWeight(0, Mathf.Max(0f, 1f - totalWeight));
        layerMixerPlayable.SetInputWeight(1, m_Value1);
        layerMixerPlayable.SetInputWeight(2, m_Value2);

        base.PrepareFrame(playable, info);
    }
}