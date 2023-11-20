using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SafeDisableDeactivate))]
public class SafeDisableDeactivateEd : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("renderers"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("noRTRenderers"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gameObjects"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("noRTGameObjects"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("activeInEditorPlayMode"));
        using(new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activeOutsidePlaymode"));

        serializedObject.ApplyModifiedProperties();
        
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Activate"))
            {
                var t = (SafeDisableDeactivate) target;
                t.SetActivate(t.activeOutsidePlaymode = true);
            }
            if (GUILayout.Button("Deactivate"))
            {
                var t = (SafeDisableDeactivate) target;
                t.SetActivate(t.activeOutsidePlaymode = false);
            }
        }
    }
}
