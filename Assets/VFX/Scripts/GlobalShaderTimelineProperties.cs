using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GlobalShaderTimelineProperties : MonoBehaviour
{
	public PlayableDirector PlayableDirector = null;

	/// <summary>
	/// Unity Update.
	/// </summary>
	private void Update()
	{
		if (PlayableDirector)
		{ Shader.SetGlobalFloat("_TimeTimeline", (float)PlayableDirector.time); }
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(GlobalShaderTimelineProperties))]
[CanEditMultipleObjects]
public class GlobalShaderTimelinePropertiesEditor : Editor
{
	private SerializedProperty _playableDirectorProperty = null;
	private PlayableDirector _playableDirector = null;

	/// <summary>
	/// Unity OnEnable.
	/// </summary>
	void OnEnable()
	{
		_playableDirectorProperty = serializedObject.FindProperty("PlayableDirector");
		_playableDirector = _playableDirectorProperty.objectReferenceValue as PlayableDirector;
	}

	/// <summary>
	/// Unity OnInspectorGUI.
	/// </summary>
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(_playableDirectorProperty);
		if (EditorGUI.EndChangeCheck())
		{ _playableDirector = _playableDirectorProperty.objectReferenceValue as PlayableDirector; }

		EditorGUILayout.LabelField("Time", _playableDirector != null ? ((float)_playableDirector.time).ToString() : "-" );

		serializedObject.ApplyModifiedProperties();
	}
}
#endif