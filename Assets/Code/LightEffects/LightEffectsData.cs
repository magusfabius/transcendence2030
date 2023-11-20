using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
struct LightEffectsData
{
    public enum DiscreteMode { First, MidPoint, Last, Default = MidPoint }
    
    // Discrete values
    public bool enabled;
    public ShadowUpdateMode shadowUpdateMode;
    
    // Linear values
    public float dimmer;

    static T Select<T>(T a, T b, float t, DiscreteMode d)
    {
        if (d == DiscreteMode.First) return a;
        if (d == DiscreteMode.Last) return b;
        return t < 0.5f ? a : b;
    }

    internal static LightEffectsData Lerp(LightEffectsData a, LightEffectsData b, float t, DiscreteMode d = DiscreteMode.Default)
    {
        return new LightEffectsData
        {
            enabled = Select(a.enabled, b.enabled, t, d),
            shadowUpdateMode = Select(a.shadowUpdateMode, b.shadowUpdateMode, t, d),
            
            dimmer = Mathf.Lerp(a.dimmer, b.dimmer, t)
        };
    } 

    internal static LightEffectsData AddScaled(LightEffectsData a, LightEffectsData b, float s, DiscreteMode d = DiscreteMode.Default)
    {
        return new LightEffectsData
        {
            enabled = Select(a.enabled, b.enabled, s, d),
            shadowUpdateMode = Select(a.shadowUpdateMode, b.shadowUpdateMode, s, d),
            
            dimmer = a.dimmer + b.dimmer * s
        };
    } 

    internal static LightEffectsData Zero => new();
    internal static LightEffectsData Default => new()
    {
        enabled = true,
        shadowUpdateMode = ShadowUpdateMode.EveryFrame,
        dimmer = 1f
    };
}
