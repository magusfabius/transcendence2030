using UnityEngine;

[ExecuteAlways]
public class CounterTransform : MonoBehaviour
{
    [SerializeField] Transform target;
    
    private void LateUpdate()
    {
        if (target)
        {
            transform.localPosition = -target.localPosition;
            transform.localRotation = Quaternion.Inverse(target.localRotation);
            
        }
    }
}
