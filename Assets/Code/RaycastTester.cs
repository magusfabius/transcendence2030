using UnityEngine;

public class RaycastTester : MonoBehaviour
{
    public float radius = 0.05f;
    public float raylen = 0.5f;
    public bool backfaces;

    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        var right = transform.right;
        var forward = transform.forward;

        var bf = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = backfaces;
        
        Gizmos.color = Physics.CheckSphere(position, radius) ? Color.red : Color.green;
        Gizmos.DrawSphere(position, radius);

        var hits = Physics.RaycastAll(position, forward, raylen, Physics.DefaultRaycastLayers);
        for(var i = 0; i < hits.Length; ++i)
        {
            var hit = hits[i];
            var delta = right * i * 0.01f;
            Gizmos.color = Vector3.Dot(hit.normal, forward) > 0f ? Color.magenta : Color.red;
            Gizmos.DrawLine(position + delta, hit.point + delta);
        }

        Physics.queriesHitBackfaces = bf;
    }
}

#if false
#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RaycastTester))]
class RaycastTesterEd : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        UnityEditor.EditorGUILayout.Space();

        if (GUILayout.Button("Cast"))
        {
            var t = (RaycastTester) target;
            var p = t.transform.position;
            var f = t.transform.forward;
            var r = t.radius;
            var l = t.raylen;

            if (Physics.CheckSphere(p, r))
            {
                Debug.DrawLine(Vector3.left * r * 0.5f, Vector3.right * r * 0.5f, Color.cyan, 10f);
                Debug.DrawLine(Vector3.up * r * 0.5f, Vector3.down * r * 0.5f, Color.cyan, 10f);
            }

        }
    }
}
#endif
#endif