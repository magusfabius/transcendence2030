using UnityEngine;

[ExecuteAlways]
public class ProbeVolumesProbeKiller : MonoBehaviour
{
    public Vector3 Position => transform.position;
    public float Radius => transform.lossyScale.x;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.matrix = Matrix4x4.TRS(Position, Quaternion.identity, Vector3.one * Radius);
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
        Gizmos.matrix = Matrix4x4.TRS(Position, Quaternion.Euler(0f, 30f, 0f), Vector3.one * Radius);
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
        Gizmos.matrix = Matrix4x4.TRS(Position, Quaternion.Euler(0f, 60f, 0f), Vector3.one * Radius);
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }

    void OnEnable() => SetInstanceData();
    void OnDisable() => SetData(Vector3.zero, 0f);
    void LateUpdate() => SetInstanceData();

    void SetInstanceData() => SetData(Position, Radius);
    
    static void SetData(Vector3 center, float radius)
    {
        Shader.SetGlobalVector("_ProbeKillPositionRadius", new Vector4(center.x, center.y, center.z, radius * radius));
    }
}
