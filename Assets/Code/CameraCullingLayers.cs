using UnityEngine;

public class CameraCullingLayers : MonoBehaviour
{
    public LayerMask addLayers;
    public LayerMask removeLayers;
    
    LayerMask m_StashedLayers;

    void OnEnable()
    {
        var c = Camera.main;
        if (c)
        {
            m_StashedLayers = c.cullingMask;
            c.cullingMask = (m_StashedLayers & ~removeLayers) | addLayers;
        }
    }
    
    void OnDisable()
    {
        var c = Camera.main;
        if (c)
        {
            c.cullingMask = m_StashedLayers;
        }
    }
}
