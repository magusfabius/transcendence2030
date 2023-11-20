using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Serialization;

[ExecuteAlways]
[DisallowMultipleComponent]
public class FocusCameraControl : MonoBehaviour
{
    public enum Mode
    {
        FocusTarget,
        ManualRanges
    }

    [Tooltip("Global override volume priority")]
    public int volumePriority = 100;

    public Mode mode = Mode.FocusTarget;

    [Header("Focus Target")]
    [Tooltip("Focus distance target")]
    [FormerlySerializedAs("FocusTarget")]
    public Transform focusTarget;

    [Tooltip("Magic distance offset")]
    [Range(-1f, 1f)] 
    public float distanceOffset = -0.02f;

    [Header("Manual Ranges")]
    public float nearFocusStart = 0;
    public float nearFocusEnd = 4;
    public float farFocusStart = 10;
    public float farFocusEnd = 20;

    [Header("Debug")]
    [Tooltip("Enabling debug will leave the backing data visible and editable in the scene.")]
    public bool debug;

    GameObject m_Volume;
    VolumeProfile m_Profile;
    DepthOfField m_DepthOfField;

    HideFlags kHideFlags => debug ? HideFlags.None : HideFlags.NotEditable | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
    
    void OnEnable()
    {
        m_Volume = new GameObject("FocusCameraControlVolume") {hideFlags = kHideFlags};
        var volume = m_Volume.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = volumePriority;

        m_Profile = volume.sharedProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        m_Profile.hideFlags = kHideFlags;
        m_Profile.name= "DoFFocusDistanceOverride";
        
        m_DepthOfField = m_Profile.Add<DepthOfField>();
        m_DepthOfField.hideFlags = kHideFlags;

        m_DepthOfField.focusDistance.overrideState = true;

        m_DepthOfField.nearFocusStart.overrideState = true;
        m_DepthOfField.nearFocusEnd.overrideState = true;
        m_DepthOfField.farFocusStart.overrideState = true;
        m_DepthOfField.farFocusEnd.overrideState = true;
    }

    void OnDisable()
    {
        void SafeDestroy(Object obj) { if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj); }
        
        m_Profile.Remove<DepthOfField>();
        SafeDestroy(m_Volume);
        SafeDestroy(m_Profile);
        SafeDestroy(m_DepthOfField);
    }

    void LateUpdate()
    {
        if (mode == Mode.FocusTarget)
        {
            if (focusTarget)
            {
                m_DepthOfField.active = true;
                m_DepthOfField.focusMode.Override(DepthOfFieldMode.UsePhysicalCamera);
                m_DepthOfField.focusDistance.value = Vector3.Distance(transform.position, focusTarget.transform.position) + distanceOffset;
            }
            else
            {
                m_DepthOfField.active = false;
            }
        }
        else
        {
            m_DepthOfField.active = true;
            m_DepthOfField.focusMode.Override(DepthOfFieldMode.Manual);
            m_DepthOfField.nearFocusStart.value = nearFocusStart;
            m_DepthOfField.nearFocusEnd.value = nearFocusEnd;
            m_DepthOfField.farFocusStart.value = farFocusStart;
            m_DepthOfField.farFocusEnd.value = farFocusEnd;
        }
    }
}
