using Cinemachine;
using UnityEngine;

public class EyeOffsetExtension : CinemachineExtension
{
    [SerializeField] StereoRenderSettings settings;
    [SerializeField] StereoRenderSettings settings2;
    [SerializeField] float blend;

    FocusCameraControl m_FocusCameraControl;
    
    protected override void OnEnable()
    {
        if (Camera.main)
        {
            m_FocusCameraControl = Camera.main.GetComponent<FocusCameraControl>();
        }

        blend = 0f;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
#if UNITY_EDITOR
        if (settings == null || settings.Eye == Camera.MonoOrStereoscopicEye.Mono ||
            stage != CinemachineCore.Stage.Finalize || !Application.isPlaying)
        {
            return;
        }

        if(settings.UseFocalPoint == false && settings2 != null && settings2.UseFocalPoint == false)
        {
            var a = Mathf.Clamp01(blend);
            var ipd = Mathf.Lerp(settings.IPD, settings2.IPD, a);
            DoParallelOffset(settings.Eye, ipd, ref state);
            return;
        }

        if (blend == 0f)
        {
            Apply(m_FocusCameraControl, settings, ref state);
        }
        else if (blend == 1f)
        {
            Apply(m_FocusCameraControl, settings2, ref state);
        }
        else
        {
            var s = CameraState.Default;
            Apply(m_FocusCameraControl, settings, ref s);
            
            var s2 = CameraState.Default;
            Apply(m_FocusCameraControl, settings2, ref s2);

            var a = Mathf.Clamp01(blend);
            state.PositionCorrection = Vector3.Lerp(s.PositionCorrection, s.PositionCorrection, a);
            state.OrientationCorrection = Quaternion.Slerp(s.OrientationCorrection, s.OrientationCorrection, a);
        }
#endif
    }

    static void Apply(FocusCameraControl focusCameraControl, StereoRenderSettings settings, ref CameraState state)
    {
        if (settings.UseFocalPoint)
        {
            DoSphericalOffset(focusCameraControl, settings, ref state);
        }
        else
        {
            DoParallelOffset(settings, ref state);
        }
    }

    static void DoSphericalOffset(FocusCameraControl focusCameraControl, StereoRenderSettings settings, ref CameraState state)
    {
        Debug.Assert(focusCameraControl);
        Debug.Assert(focusCameraControl.focusTarget);
        
        var focusTarget = focusCameraControl.focusTarget;
        
        var selfPosition = state.GetFinalPosition();
        var focusToSelfVector = selfPosition - focusTarget.position;

        var hipd = settings.IPD * 0.5f;
        var circ = 2f * focusToSelfVector.magnitude * Mathf.PI;
        var frac = hipd / circ;

        var angle = frac * 360f;
        angle *= settings.Eye == Camera.MonoOrStereoscopicEye.Left ? 1f : -11f;
        var rotationalCorrection = Quaternion.AngleAxis(angle, state.ReferenceUp);

        var focusToEyeVector = rotationalCorrection * focusToSelfVector;
        var eyePosition = focusTarget.position + focusToEyeVector;
        var eyeOffset = eyePosition - selfPosition;
        
        state.PositionCorrection += eyeOffset;
        state.OrientationCorrection *= rotationalCorrection;
    }

    static void DoParallelOffset(StereoRenderSettings settings, ref CameraState state)
    {
        DoParallelOffset(settings.Eye, settings.IPD, ref state);
    }

    static void DoParallelOffset(Camera.MonoOrStereoscopicEye eye, float ipd, ref CameraState state)
    {
        var orientation = state.GetFinalOrientation();
        var forward = orientation * Vector3.forward;
        var right = Vector3.Cross(state.ReferenceUp, forward);
        
        var halfOffset = right.normalized * (ipd * 0.5f);
        halfOffset *= eye == Camera.MonoOrStereoscopicEye.Left ? -1f : 1f;

        state.PositionCorrection += halfOffset;
    }
}
