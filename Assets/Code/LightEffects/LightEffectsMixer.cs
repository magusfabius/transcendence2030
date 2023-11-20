using UnityEngine.Playables;

class LightEffectsMixer : PlayableBehaviour
{
    internal LightEffectsTrack lightEffectsTrack { get; set; }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var data = LightEffectsData.Zero;
        var weight = 0f;
        
        for (int i = 0, n = playable.GetInputCount(); i < n; i++)
        {
            var inputWeight = playable.GetInputWeight(i);
            var inputPlayable = (ScriptPlayable<LightEffectsBehaviour>)playable.GetInput(i);
            var inputBehaviour = inputPlayable.GetBehaviour();

            data = LightEffectsData.AddScaled(data, inputBehaviour.clip.data, inputWeight);
            weight += inputWeight;
        }
        
        data = LightEffectsData.Lerp(lightEffectsTrack.defaultData, data, weight);

        var lightEffects = (LightEffects) playerData;
        lightEffects.ApplyData(data);
    }
}
