using UnityEngine;

[CreateAssetMenu]
public class StereoRenderSettings : ScriptableObject
{
    public Camera.MonoOrStereoscopicEye Eye => eye;
    public float IPD => ipd;
    public bool UseFocalPoint => useFocalPoint;
    
    [SerializeField] Camera.MonoOrStereoscopicEye eye = Camera.MonoOrStereoscopicEye.Mono;
    [SerializeField] float ipd = 0.03f;
    [SerializeField] bool useFocalPoint;
}
