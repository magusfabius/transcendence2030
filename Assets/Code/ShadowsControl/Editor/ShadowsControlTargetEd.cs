using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShadowsControlTarget))]
class ShadowsControlTargetEd : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Shadow Update Stats", UnityEditor.EditorStyles.boldLabel);

        var t = (ShadowsControlTarget)target;
        if (!t.enabled)
        {
            EditorGUILayout.LabelField("Control Disabled");
        }
        else
        {
            EditorGUILayout.LabelField("Current Frame", Time.renderedFrameCount.ToString());

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Last Frame Updated", t.LastRenderedFrameUpdated.ToString());
            EditorGUILayout.LabelField("Last Update Mode", t.LastUpdateMode.ToString());
            
            if(t.LastUpdateMode == ShadowsControlTarget.Mode.Sub)
                EditorGUILayout.LabelField("Last Update Sub Index", t.LastUpdateSubIndex.ToString());
        }
    }
}
