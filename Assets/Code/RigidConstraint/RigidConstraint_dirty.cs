//temporary workround for the dome light parenting issue
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class RigidConstraint_dirty : MonoBehaviour
{
    public Transform target;

    Transform m_Transform;

    void OnEnable() => m_Transform = transform;

    void LateUpdate()
    {
        if (target)
            m_Transform.SetPositionAndRotation(target.position, target.rotation);
    }
}
