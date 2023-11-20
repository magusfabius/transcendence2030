using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

#if UNITY_EDITOR
    using System.Linq;
#endif

[ExecuteAlways]
public class SafeDisableDeactivate : MonoBehaviour
{
    public Renderer[] renderers = System.Array.Empty<Renderer>();
    public MeshRenderer[] noRTRenderers = System.Array.Empty<MeshRenderer>();
    public GameObject[] gameObjects = System.Array.Empty<GameObject>();
    public GameObject[] noRTGameObjects = System.Array.Empty<GameObject>();
    public bool activeInEditorPlayMode;
    public bool activeOutsidePlaymode;

#if UNITY_EDITOR
    Renderer[] m_AppliedRenderers;
    MeshRenderer[] m_AppliedNoRTRenderers;
    GameObject[] m_AppliedGameObjects;
    GameObject[] m_AppliedNoRTGameObjects;
    
    void OnValidate()
    {
        if (!Application.isPlaying && activeOutsidePlaymode)
        {
            if (!renderers.SequenceEqual(m_AppliedRenderers) || !noRTRenderers.SequenceEqual(m_AppliedNoRTRenderers) || !gameObjects.SequenceEqual(m_AppliedGameObjects) || !noRTGameObjects.SequenceEqual(m_AppliedNoRTGameObjects))
            {
                SetActivate(true);

                m_AppliedRenderers = (Renderer[])renderers.Clone();
                m_AppliedNoRTRenderers = (MeshRenderer[])noRTRenderers.Clone();
                m_AppliedGameObjects = (GameObject[])gameObjects.Clone();
                m_AppliedNoRTGameObjects = (GameObject[])noRTGameObjects.Clone();
            }
        }
    }
#endif

    void OnEnable()
    {
        if((Application.isPlaying && (!Application.isEditor || activeInEditorPlayMode)) || activeOutsidePlaymode)
            SetActivate(true);
    }

    void OnDisable()
    {
        if((Application.isPlaying && (!Application.isEditor || activeInEditorPlayMode)) || activeOutsidePlaymode)
            SetActivate(false);
    }

    public void SetActivate(bool disable)
    {
        foreach (var r in renderers)
        {
            if (r) r.enabled = !disable;
        }

        if (SystemInfo.supportsRayTracing)
        {
            foreach (var r in noRTRenderers)
            {
                if (r) r.rayTracingMode = disable ? RayTracingMode.Off : RayTracingMode.DynamicTransform;
            }
        }

        foreach (var go in gameObjects)
        {
            if (go) go.SetActive(!disable);
        }

        if (SystemInfo.supportsRayTracing)
        {
            var childRenderers = new List<Renderer>();
            foreach (var go in noRTGameObjects)
            {
                if (go)
                {
                    go.GetComponentsInChildren(true, childRenderers);
                    foreach (var r in childRenderers)
                        r.rayTracingMode = disable ? RayTracingMode.Off : (r is SkinnedMeshRenderer ? RayTracingMode.DynamicGeometry : RayTracingMode.DynamicTransform);
                }
            }
        }
    }
}
