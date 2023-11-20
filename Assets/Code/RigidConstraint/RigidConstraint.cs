using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class RigidConstraint : MonoBehaviour
{
    public Transform target;
    public bool useLocalSpace;
    Transform m_Transform;

    void OnEnable() => m_Transform = transform;

    void LateUpdate()
    {
        if (target)
        {
            if (useLocalSpace)
            {
                m_Transform.localPosition = target.localPosition;
                m_Transform.localRotation = target.localRotation;
            }
            else
            {
                m_Transform.SetPositionAndRotation(target.position, target.rotation);
            }
        }
    }
}
