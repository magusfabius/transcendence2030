using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuickSettings))]
public class QuickSettingsEd : Editor
{
    new QuickSettings target => (QuickSettings) base.target;
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("editorPlaymodeDefaultQuality"));
        
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hdrpRaster"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hdrpRaytrace"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hdrpXboxSeries"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hdrpPlaystation"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("general"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ambientOcclusion"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("globalIllumination"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reflections"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shadows"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hair"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directionalShadowRaySamples"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetDisable"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("domeLights"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("filmGrain"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moBlur"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dof"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lensFlare"));

        void DoPreset(string presetName, string groupName)
        {
            using var hs = new EditorGUILayout.HorizontalScope();
            
            var spPreset = serializedObject.FindProperty(presetName);
            
            using(new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(spPreset);

            if (GUILayout.Button("P", GUILayout.MaxWidth(20f)))
            {
                spPreset.enumValueIndex = (int) QuickSettings.Prev(
                    (GameObject) serializedObject.FindProperty(groupName).objectReferenceValue,
                    (QuickSettings.Preset)spPreset.enumValueIndex
                );
            }
            if (GUILayout.Button("N", GUILayout.MaxWidth(20f)))
            {
                spPreset.enumValueIndex = (int) QuickSettings.Next(
                    (GameObject) serializedObject.FindProperty(groupName).objectReferenceValue,
                    (QuickSettings.Preset)spPreset.enumValueIndex
                );
            }
        }

        DoPreset("defaultGeneral", "general");
        DoPreset("defaultAO", "ambientOcclusion");
        DoPreset("defaultGI", "globalIllumination");
        DoPreset("defaultReflections", "reflections");
        DoPreset("defaultShadows", "shadows");
        DoPreset("defaultHair", "hair");
        DoPreset("defaultDirectionalShadowRaySamples", "directionalShadowRaySamples");
        DoPreset("defaultTargetDisable", "targetDisable");
        DoPreset("defaultDomeLights", "domeLights");
        DoPreset("defaultFilmGrain", "filmGrain");
        DoPreset("defaultMoBlur", "moBlur");
        DoPreset("defaultDoF", "dof");
        DoPreset("defaultLensFlare", "lensFlare");

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
            target.Apply();
        }

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if(GUILayout.Button("Default"))
                target.defaultQuickInputs = QuickInputs.NewDefault();
            
            if(GUILayout.Button("Clear"))
                target.defaultQuickInputs = null;

            if (GUILayout.Button("Import"))
            {
                var path = EditorUtility.OpenFilePanelWithFilters("Import QuickInputs..", $"{Application.dataPath}/../PlayerScripts/", new[] {"JSON", "json"});
                if(!string.IsNullOrEmpty(path))
                    target.defaultQuickInputs = QuickInputs.Produce(path);
            }
        }
        
        var spDefaultQI = serializedObject.FindProperty("defaultQuickInputs");
        EditorGUILayout.PropertyField(spDefaultQI, true);

        serializedObject.ApplyModifiedProperties();
    }
}
