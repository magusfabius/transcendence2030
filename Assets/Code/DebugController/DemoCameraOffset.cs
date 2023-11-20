using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NonSerializedAttribute = System.NonSerializedAttribute;

[AddComponentMenu("")] // Only available in CM extension UI
public class DemoCameraOffset : CinemachineExtension
{
    [SerializeField] private Vector3 translationalOffsetLimits = new(2f, 2f, 0f);
    [SerializeField] private Vector3 rotationalOffsetLimits = new(15f, 15f, 0f);

    [NonSerialized] public Vector2 translationalOffset;
    [NonSerialized] public Vector2 rotationalOffset;

    const CinemachineCore.Stage kApplyAfterStage = CinemachineCore.Stage.Aim;

    internal static List<DemoCameraOffset> sInstances = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        
        translationalOffset = Vector2.zero;
        rotationalOffset = Vector2.zero;
        
        sInstances.Add(this);
    }

    void OnDisable()
    {
        sInstances.Remove(this);
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == kApplyAfterStage)
        {
            state.PositionCorrection += state.RawOrientation * Vector3.Scale(translationalOffsetLimits, new Vector3(translationalOffset.x, translationalOffset.y, 0f));
            state.OrientationCorrection *= Quaternion.Euler(Vector3.Scale(rotationalOffsetLimits, new Vector3(rotationalOffset.y, rotationalOffset.x, 0f)));
        }
    }
}
