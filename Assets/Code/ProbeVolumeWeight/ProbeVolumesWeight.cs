using UnityEngine;

[ExecuteAlways]
public class ProbeVolumesWeight : MonoBehaviour
{
    public float weight = 1f;

    void OnEnable() => Set(weight);
    void OnDisable() => Set(1f);
    void LateUpdate() => Set(weight);
    
    static void Set(float value)
    {
#if UNITY_CORERP_14_0_0_OR_NEWER
        if (UnityEngine.Rendering.ProbeReferenceVolume.instance != null)
            UnityEngine.Rendering.ProbeReferenceVolume.instance.probeVolumesWeight = value;
#endif
    }
}
