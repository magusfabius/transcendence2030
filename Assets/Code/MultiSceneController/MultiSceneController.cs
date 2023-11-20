
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneController : MonoBehaviour
{
	[Serializable]
	public struct ScenePathList
	{
		public string 	groupName;
		public int		activeSceneIndex;
		public string[] scenePaths;
	}

    public ScenePathList	mainScenePath;
	public ScenePathList[]	editorScenePaths;
}
