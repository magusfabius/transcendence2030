//#define LOOK_NAV_ONLY
#define NO_MOVEMENT

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

class DemoController : MonoBehaviour
{
    internal static DemoController Instance { get; private set; }

    [SerializeField] float analogueAxisMoveMaxDelta = 2f;
    [SerializeField] float analogueAxisLookMaxDelta = 2f;
    [SerializeField] float analogueAxisTriggerMaxDelta = 2f;

    [SerializeField] float analogueKeyAcceleration = 3f;
    [SerializeField] float analogueKeyGravity = 1f;
    [SerializeField] float mouseDiscRadius = 400f;
    [SerializeField] float mouseGravity = 1f;
    [SerializeField] float scrubbingSpeed = 5f;

    [Header("Bindings")]
    [SerializeField] PlayableDirector masterDirector;
    [SerializeField] Renderer[] characterParts = System.Array.Empty<Renderer>();
    [SerializeField] GameObject[] characterPartsGO = System.Array.Empty<GameObject>();
    
    float m_KeyboardMoveWImmediate;
    float m_KeyboardMoveAImmediate;
    float m_KeyboardMoveSImmediate;
    float m_KeyboardMoveDImmediate;
    float m_KeyboardMoveWInterpolated;
    float m_KeyboardMoveAInterpolated;
    float m_KeyboardMoveSInterpolated;
    float m_KeyboardMoveDInterpolated;

    bool m_MousePressed;
    Vector2 m_MouseDownPosition;
    Vector2 m_MouseMovedImmediate;
    Vector2 m_MouseMovedInterpolated;

    Vector2 m_LastInputCameraMove;
    Vector2 m_LastInputCameraLook;

    Vector2 m_InputCameraMove;
    Vector2 m_InputCameraLook;
    float m_InputPlaybackScrub;
    bool m_InputPlaybackPlayPause;

    bool m_LastInputWasGamePad;

    bool m_DriftBackBlocked;
    bool m_CharacterEnabled = true;
    
    ui_controller m_UIController;
    bool m_DidPauseForUI;
    
    void Awake()
    {
        Debug.Assert(Instance == null);
        Instance = this;

#if !LOOK_NAV_ONLY
        TimelineRangeLooper.sTimelineLooping += () => ToggleCharacter(true);
#endif
        
        foreach (var root in SceneManager.GetSceneAt(0).GetRootGameObjects())
        {
            m_UIController = root.GetComponentInChildren<ui_controller>(true);

            if (m_UIController)
            {
                m_UIController.OnRequestedQuit += QuitRequested;
                m_UIController.OnRequestedClose += ToggleMenu;
                break;
            }
        }
    }

    void OnDestroy()
    {
        Debug.Assert(Instance == this);
        Instance = null;
    }

    void QuitRequested()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    void ToggleMenu()
    {
        if (!m_UIController)
            return;

        var target = m_UIController.gameObject;
        var newState = !target.activeSelf;

        if (newState)
        {
            if (masterDirector != null && masterDirector.state == PlayState.Playing)
            {
                masterDirector.Pause();
                m_DidPauseForUI = true;
            }
        }
        else if(m_DidPauseForUI)
        {
            masterDirector.Play();
            m_DidPauseForUI = false;
        }
        
        target.SetActive(!target.activeSelf);
    }

    void Update()
    {
        float ClampedExtents(float oldValue, float newValue, float maxDelta, bool noGravity = false)
        {
            if (noGravity && Mathf.Abs(newValue) < 1e-3f)
                return oldValue;

            var deltaTimed = maxDelta * Time.deltaTime;
            var clampedDelta = Mathf.Clamp(newValue - oldValue, -deltaTimed, deltaTimed);
            var r = oldValue + clampedDelta;
            return Mathf.Abs(r) < 1e-4f ? 0f : r;
        }

        float MaxExtents(float oldValue, float newValue)
        {
            var maxValue = Mathf.Abs(newValue) > Mathf.Abs(oldValue) ? newValue : oldValue;
            return maxValue;
        }

        var gamePad = Gamepad.current;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (gamePad != null && gamePad.wasUpdatedThisFrame)
            m_LastInputWasGamePad = true;

        if (keyboard != null && keyboard.anyKey.isPressed)
            m_LastInputWasGamePad = false;

        if (mouse != null && mouse.leftButton.isPressed)
            m_LastInputWasGamePad = false;

        m_InputCameraMove = default;
        m_InputCameraLook = default;
        m_InputPlaybackScrub = default;
        m_InputPlaybackPlayPause = default;

        var characterToggled = false;
        var menuToggled = false;

        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame && keyboard.shiftKey.isPressed)
            {
                QuitRequested();
            }

            if (keyboard.enterKey.wasPressedThisFrame && keyboard.altKey.isPressed)
            {
                QuickSettings.Instance.ToggleFullscreen();

                // Force full re-init
                if (m_UIController && m_UIController.isActiveAndEnabled)
                {
                    m_UIController.gameObject.SetActive(!m_UIController.gameObject.activeSelf);
                    m_UIController.gameObject.SetActive(!m_UIController.gameObject.activeSelf);
                }
            }

            if (keyboard.escapeKey.wasPressedThisFrame && !keyboard.shiftKey.isPressed)
            {
                menuToggled = true;
            }
        }

        if (gamePad != null)
        {
            if (gamePad.startButton.wasPressedThisFrame)
            {
                menuToggled = true;
            }
        }

        if (menuToggled)
        {
            ToggleMenu();
        }

        if (m_UIController && m_UIController.isActiveAndEnabled)
        {
            // Block input while menu is active.
            return;
        }

        if (keyboard != null && !m_LastInputWasGamePad)
        {
#if !NO_MOVEMENT
            void ReadAndInterpolateKey(ref float interpolated, ref float immediate, KeyControl key)
            {
                var value = key.ReadValue();
                var acceleration = value == 0f ? -analogueKeyGravity : analogueKeyAcceleration;
                interpolated = Mathf.Clamp01(interpolated + acceleration * Time.deltaTime);
                immediate = value;
            }

            ReadAndInterpolateKey(ref m_KeyboardMoveWInterpolated, ref m_KeyboardMoveWImmediate, keyboard.wKey);
            ReadAndInterpolateKey(ref m_KeyboardMoveAInterpolated, ref m_KeyboardMoveAImmediate, keyboard.aKey);
            ReadAndInterpolateKey(ref m_KeyboardMoveSInterpolated, ref m_KeyboardMoveSImmediate, keyboard.sKey);
            ReadAndInterpolateKey(ref m_KeyboardMoveDInterpolated, ref m_KeyboardMoveDImmediate, keyboard.dKey);
            
            var leftHorizontal = m_KeyboardMoveDInterpolated - m_KeyboardMoveAInterpolated;
            m_InputCameraMove.x = MaxExtents(m_InputCameraMove.x, leftHorizontal);

            var leftVertical = m_KeyboardMoveWInterpolated - m_KeyboardMoveSInterpolated;
            m_InputCameraMove.y = MaxExtents(m_InputCameraMove.y, leftVertical);
#endif
            var scrubValue = (keyboard.commaKey.isPressed ? -1f : 0f) + (keyboard.periodKey.isPressed ? 1f : 0f);
            m_InputPlaybackScrub = MaxExtents(m_InputPlaybackScrub, scrubValue);

#if !LOOK_NAV_ONLY
            characterToggled |= keyboard.yKey.wasPressedThisFrame;
#endif
            
            m_InputPlaybackPlayPause = m_InputPlaybackPlayPause || keyboard.spaceKey.wasPressedThisFrame;

            if (keyboard.bKey.wasPressedThisFrame)
            {
                m_DriftBackBlocked = !m_DriftBackBlocked;
            }
        }

        if (mouse != null && !m_LastInputWasGamePad)
        {
            if (m_MousePressed && !mouse.leftButton.isPressed)
            {
                m_MousePressed = false;
            }

            if (!m_MousePressed && mouse.leftButton.wasPressedThisFrame)
            {
                m_MousePressed = true;
                m_MouseDownPosition = mouse.position.ReadValue();
            }
            
            if (m_MousePressed)
            {
                var position = mouse.position.ReadValue();
                var vector = position - m_MouseDownPosition;
                var normalizedVector = Vector2.ClampMagnitude(vector / mouseDiscRadius, 1f);
                
                m_MouseMovedImmediate = position;
                m_MouseMovedInterpolated = normalizedVector;
            }
            else
            {
                var x = m_MouseMovedInterpolated.x - Mathf.Sign(m_MouseMovedInterpolated.x) * mouseGravity * Time.deltaTime;
                var y = m_MouseMovedInterpolated.y - Mathf.Sign(m_MouseMovedInterpolated.y) * mouseGravity * Time.deltaTime;
                if (Mathf.Sign(x) != Mathf.Sign(m_MouseMovedInterpolated.x)) x = 0f;
                if (Mathf.Sign(y) != Mathf.Sign(m_MouseMovedInterpolated.y)) y = 0f;
                m_MouseMovedInterpolated.Set(x, y);
            }

            m_InputCameraLook.x = MaxExtents(m_InputCameraLook.x, m_MouseMovedInterpolated.x);
            m_InputCameraLook.y = MaxExtents(m_InputCameraLook.y, -m_MouseMovedInterpolated.y);
        }
        
        if (gamePad != null && m_LastInputWasGamePad)
        {
#if !NO_MOVEMENT
            var leftStick = gamePad.leftStick.ReadValue();
            m_InputCameraMove.x = ClampedExtents(m_LastInputCameraMove.x, leftStick.x, analogueAxisMoveMaxDelta, m_DriftBackBlocked);
            m_InputCameraMove.y = ClampedExtents(m_LastInputCameraMove.y, leftStick.y, analogueAxisMoveMaxDelta, m_DriftBackBlocked);
#endif
            var rightStick = gamePad.rightStick.ReadValue();
            m_InputCameraLook.x = ClampedExtents(m_LastInputCameraLook.x, rightStick.x, analogueAxisLookMaxDelta, m_DriftBackBlocked);
            m_InputCameraLook.y = ClampedExtents(m_LastInputCameraLook.y, -rightStick.y, analogueAxisLookMaxDelta, m_DriftBackBlocked);
            var shoulderValue = gamePad.rightShoulder.ReadValue() - gamePad.leftShoulder.ReadValue();
            m_InputPlaybackScrub = MaxExtents(m_InputPlaybackScrub, shoulderValue);
            m_InputPlaybackPlayPause = m_InputPlaybackPlayPause || gamePad.buttonSouth.wasPressedThisFrame;

#if !LOOK_NAV_ONLY
            characterToggled |= gamePad.buttonNorth.wasPressedThisFrame;
#endif
            
            if (gamePad.buttonEast.wasPressedThisFrame)
            {
                m_DriftBackBlocked = !m_DriftBackBlocked;
            }
        }
        
#if !LOOK_NAV_ONLY
        if (characterToggled)
        {
            ToggleCharacter();
        }
#endif
        
        if (masterDirector)
        {
            if (m_InputPlaybackScrub != 0f)
            {
                masterDirector.time = System.Math.Clamp(masterDirector.time + m_InputPlaybackScrub * Time.deltaTime * scrubbingSpeed, 0, masterDirector.duration);

                if (masterDirector.state == PlayState.Paused)
                {
                    masterDirector.Evaluate();
                }
            }
            
            if (m_InputPlaybackPlayPause)
            {
                if (masterDirector.state == PlayState.Paused)
                {
                    masterDirector.Play();
                }
                else
                {
                    masterDirector.Pause();
                }
            }
        }

        foreach (var demoCameraOffset in DemoCameraOffset.sInstances)
        {
#if !NO_MOVEMENT
            demoCameraOffset.translationalOffset = m_InputCameraMove;
#endif
            demoCameraOffset.rotationalOffset = m_InputCameraLook;
        }
        
        m_LastInputCameraMove = m_InputCameraMove;
        m_LastInputCameraLook = m_InputCameraLook;
    }

#if !LOOK_NAV_ONLY
    void ToggleCharacter(bool forceActive = false)
    {
        var newState = forceActive || !m_CharacterEnabled;

        foreach (var part in characterParts)
        {
            part.enabled = newState;
        }

        foreach (var partGO in characterPartsGO)
        {
            partGO.SetActive(newState);
        }

        m_CharacterEnabled = newState;
    }
#endif

    void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        // Avoid crash on shutdown bug until it gets fixed
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }
}
