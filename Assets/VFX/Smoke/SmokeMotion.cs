using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;

[ExecuteInEditMode]
public class SmokeMotion : MonoBehaviour
{
	public enum MotionMode { Realtime, Baked }

	[Tooltip("When using realtimine motion everything is capture and baked on the fly. To make it work in the editor, you need to enable Gizmos.")]
	public MotionMode Mode = MotionMode.Realtime;
	//public Material Material = null;
	public MeshRenderer MeshRenderer = null;

	[Header("Realtime Properties")]
	public int TextureScale = 32;

	[Header("Baked Properties (Tools->VFX->Smoke position baker)")]
	public PlayableDirector PlayableDirector = null;
	public int StartFrame = 0;
	public int EndFrame = 0;
	public float FPS = 0.0f;

	private Texture2D _realtimeTexture = null;
	private int _frame = 0;

	private MaterialPropertyBlock _materialPropertyBlock = null;

	/// <summary>
	/// Unity OnDrawGizmos.
	/// </summary>
	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		// TODO: This is super hacky but ensure continuous Update calls in editor. Not the right way to handle this realtime update in the editor
		if (Mode == MotionMode.Realtime)
		{
			if (!Application.isPlaying)
			{
				UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
				UnityEditor.SceneView.RepaintAll();
			}
		}
#endif
	}

	/// <summary>
	/// Unity Update.
	/// </summary>
	private void Update()
    {
		if (MeshRenderer != null)
		{ MeshRenderer.GetPropertyBlock(_materialPropertyBlock); }

		switch (Mode)
		{
			case MotionMode.Realtime:
				if (transform != null && _realtimeTexture != null /*&& Material != null*/)
				{
					BakePositionCPU(transform.position, _frame, _realtimeTexture);
					if (MeshRenderer != null)
					{
						_materialPropertyBlock.SetFloat("Frame", _frame);
						MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
					}
					//if (Material != null)
					//{
					//	Material.SetFloat("Frame", _frame);
					//}
					_frame++;
					if (_frame >= (_realtimeTexture.width * _realtimeTexture.height))
					{ _frame = 0; }
				}
				break;
			case MotionMode.Baked:
				if (PlayableDirector != null)
				{
					_frame = Mathf.Max(0, Mathf.Min(EndFrame - StartFrame, Mathf.RoundToInt((float)PlayableDirector.time * FPS) - StartFrame));
					if (MeshRenderer != null)
					{
						_materialPropertyBlock.SetFloat("Frame", _frame);
						MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
					}
					//if (Material != null)
					//{
					//	Material.SetFloat("Frame", _frame);
					//}
				}
				break;
		}
	}

	/// <summary>
	/// Unity OnEnable.
	/// </summary>
	private void OnEnable()
	{
		if (_materialPropertyBlock != null)
		{
			_materialPropertyBlock.Clear();
			_materialPropertyBlock = null;
		}

		if (MeshRenderer != null)
		{
			_materialPropertyBlock = new MaterialPropertyBlock();
			MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
		}

		if (Mode == MotionMode.Realtime)
		{
			if (_realtimeTexture != null)
			{
				DestroyImmediate(_realtimeTexture);
				_realtimeTexture = null;
			}
			_realtimeTexture = new Texture2D(TextureScale, TextureScale, TextureFormat.RGBAFloat, false, true);
			_realtimeTexture.filterMode = FilterMode.Point;
			_realtimeTexture.wrapMode = TextureWrapMode.Clamp;
			_realtimeTexture.hideFlags = HideFlags.DontSave;
			_realtimeTexture.name = "RealtimePositionsTexture";

			//if (Material != null)
			//{ Material.SetTexture("Positions", _realtimeTexture); }

			if (MeshRenderer != null)
			{
				_materialPropertyBlock.SetTexture("Positions", _realtimeTexture);
				MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
			}
		}

		_frame = 0;
	}

	/// <summary>
	/// Unity OnDisable.
	/// </summary>
	private void OnDisable()
	{
		if (_materialPropertyBlock != null)
		{
			_materialPropertyBlock.Clear();
			_materialPropertyBlock = null;
		}

		if (MeshRenderer != null)
		{ MeshRenderer.SetPropertyBlock(null); }

		if (_realtimeTexture != null)
		{
			DestroyImmediate(_realtimeTexture);
			_realtimeTexture = null;
		}

		//if (Material != null)
		//{ Material.SetTexture("Positions", null); }

		_frame = 0;
	}

	/// <summary>
	/// Unity OnValidate.
	/// </summary>
	private void OnValidate()
	{
		if (_materialPropertyBlock != null)
		{
			_materialPropertyBlock.Clear();
			_materialPropertyBlock = null;
		}

		if (MeshRenderer != null)
		{
			_materialPropertyBlock = new MaterialPropertyBlock();
			MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
		}
	}

	/// <summary>
	/// Write current position to the texture at a specific texel using the normalized frame.
	/// </summary>
	static public void BakePositionCPU(Vector3 position, int frame, Texture2D texture)
	{
		// TODO: CPU Texture writing is slow, move this to GPU
		int x = Mathf.FloorToInt(frame % texture.width);
		int y = Mathf.FloorToInt((float)frame / (float)texture.height);
		Color color = new Color(position.x, position.y, position.z, 1.0f);

		texture.SetPixel(x, y, color);
		texture.Apply(false, false);
	}
}
