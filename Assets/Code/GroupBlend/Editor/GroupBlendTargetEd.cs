using UnityEditor;

[CustomEditor(typeof(GroupBlendTarget))]
class GroupBlendTargetEd : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weight"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetProbes"), true);
        }
    }
}
