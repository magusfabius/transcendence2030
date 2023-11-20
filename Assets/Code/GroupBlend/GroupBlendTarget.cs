using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Timeline;

public class GroupBlendTarget : MonoBehaviour
{
    // These are really only serialized for the purpose of lazy, consistent UI
    [SerializeField] float weight;
    [SerializeField] HDAdditionalReflectionData[] targetProbes;

    // In playmode we fetch and cache the children once (OnEnable). In editor, we always grab a fresh list to capture hierarchy changes.
    HDAdditionalReflectionData[] TargetProbes
    {
        get
        {
            if(!Application.isPlaying || targetProbes == null || targetProbes.Length == 0)
                targetProbes = GetComponentsInChildren<HDAdditionalReflectionData>(true);

            return targetProbes;
        }
    }
    void OnEnable() => targetProbes = GetComponentsInChildren<HDAdditionalReflectionData>(true);
    
    internal void ApplyData(float weight)
    {
        // // ReSharper disable once CompareOfFloatsByEqualityOperator
        // // Early out in playmode if value hasn't changed.
        // if (Application.isPlaying && this.weight == weight)
        //     return;

        foreach (var target in TargetProbes)
            target.weight = weight;
        
        this.weight = weight;
    }

    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        driver.AddFromName(this, "weight");
       
        foreach (var target in TargetProbes)
            driver.AddFromName(target, "m_ProbeSettings.lighting.weight");
    }
}
