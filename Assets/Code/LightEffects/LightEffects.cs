using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Timeline;

#if UNITY_EDITOR
    using UnityEditor;
#endif

class LightEffects : MonoBehaviour
{
    [SerializeField] HDAdditionalLightData[] targetHDLights;

    internal LightEffectsData AppliedData { get; private set; }
    internal bool HasAppliedData { get; private set; }

    void Reset()
    {
        targetHDLights = new HDAdditionalLightData[0];
        AppliedData = LightEffectsData.Zero;
    }

    void OnValidate() => HasAppliedData = false;

    internal void ApplyData(LightEffectsData data)
    {
        if(!HasAppliedData)
            AppliedData = data;
        
        foreach(var hdAdditionalLightData in targetHDLights)
        {
            if(AppliedData.enabled != data.enabled) hdAdditionalLightData.GetComponent<Light>().enabled = data.enabled;
            hdAdditionalLightData.shadowUpdateMode = data.shadowUpdateMode;
            hdAdditionalLightData.SetLightDimmer(data.dimmer, data.dimmer);
        }
        
        AppliedData = data;
        HasAppliedData = true;
    }
    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        foreach (var hdAdditionalLightData in targetHDLights)
        {
            driver.AddFromName(hdAdditionalLightData.GetComponent<Light>(), "m_Enabled");
            driver.AddFromName(hdAdditionalLightData, "m_ShadowUpdateMode");
            driver.AddFromName(hdAdditionalLightData, "m_LightDimmer");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LightEffects))]
class LightEffectsEd : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        var t = (LightEffects)target;

        if (!t.HasAppliedData)
        {
            EditorGUILayout.HelpBox("No applied values yet.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.LabelField("Applied Values", EditorStyles.boldLabel);
        EditorGUILayout.Toggle("enabled:", t.AppliedData.enabled);
        EditorGUILayout.EnumPopup("shadowUpdateMode:", t.AppliedData.shadowUpdateMode);
        EditorGUILayout.FloatField("dimmer:", t.AppliedData.dimmer);
    }
}

#endif