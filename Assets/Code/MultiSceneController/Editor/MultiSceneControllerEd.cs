using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Timeline;
using UnityEngine.Playables;

#pragma warning disable CS0618

[CustomEditor(typeof(MultiSceneController))]
public class MultiSceneControllerEd : Editor {
    [MenuItem("Enemies/Load Scenes", priority = 10)]
    public static void LoadEnemiesScenes() { DoLoadScenes("Assets/Scenes/Bootstrap.unity"); }

    [MenuItem("Enemies/Open Timeline", priority = 11)]
    public static void OpenEnemiesTimeline()
    {
	    var loader = FindObjectOfType<PlayerLoader>();
	    if (loader == null)
	    {
		    Debug.LogError("Loader not found, is the Bootstrap scene loaded?");
		    return;
	    }
	    
	    var player = FindObjectsByType<PlayableDirector>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
		    .FirstOrDefault(pd => pd.playableAsset == loader.mainDirectorAsset);
	    if (player == null)
	    {
		    Debug.LogError("Player not found, is the Enemies scene loaded?");
		    return;
	    }

	    var timelineEditorWindow = (TimelineEditorWindow)EditorWindow.GetWindow(typeof(TimelineEditorWindow).Assembly.GetType("UnityEditor.Timeline.TimelineWindow"));
	    timelineEditorWindow.SetTimeline(player);
	    timelineEditorWindow.locked = true;
	    timelineEditorWindow.playbackControls.SetCurrentTime(21.0, TimelinePlaybackControls.Context.Global);
    }

    public static void DoLoadScenes(string masterScene) {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(masterScene, UnityEditor.SceneManagement.OpenSceneMode.Single);
        var instance = Object.FindObjectOfType<MultiSceneController>();

		LoadEditorScenes(instance.mainScenePath);
    }

    static void DoLoadScenes(string masterScene, string groupName)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(masterScene, UnityEditor.SceneManagement.OpenSceneMode.Single);
        var instance = Object.FindObjectOfType<MultiSceneController>();
        var sceneSet = instance.editorScenePaths.Where(x => x.groupName == groupName).FirstOrDefault();

        LoadEditorScenes(sceneSet);
    }

    public override void OnInspectorGUI() {
		var t = target as MultiSceneController;

		DrawDefaultInspector();
		EditorGUILayout.Space();

		GUI.enabled = !Application.isPlaying && t.mainScenePath.scenePaths != null && t.mainScenePath.scenePaths.Length > 0;
		if(GUILayout.Button("Load Default Scenes"))
		{
		    LoadEditorScenes(t.mainScenePath);
		}
	}

    public static void LoadEditorScenes(MultiSceneController.ScenePathList scenePathList)
    {
		var scenePaths = scenePathList.scenePaths;
        var scenes = new List<UnityEditor.SceneManagement.SceneSetup>(scenePaths.Length);
        for (int i = 0; i < scenePaths.Length; ++i)
        {
			if(string.IsNullOrEmpty(scenePaths[i]))
				continue;

			var scene = new UnityEditor.SceneManagement.SceneSetup();
            scene.path = scenePaths[i];
            scene.isActive = i == scenePathList.activeSceneIndex;
            scene.isLoaded = true;
            scene.isSubScene = false;
			scenes.Add(scene);
        }

        Debug.LogFormat("Restoring {0} editor scenes.", scenes.Count);
        UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(scenes.ToArray());
    }
}
