using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class CameraToCMProperties : MonoBehaviour
{
    private Camera m_Camera;
    private CinemachineVirtualCamera m_CMCamera;
    private void OnEnable()
    {
        m_Camera = gameObject.GetComponent<Camera>();
        m_CMCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
    }

    private void LateUpdate()
    {
        if(!m_Camera && !m_CMCamera) return;
        
        m_CMCamera.m_Lens.FieldOfView = m_Camera.fieldOfView;
        // m_CMCamera.m_Lens..focalLength = m_Camera.focalLength;
        // m_CMCamera.m_Lens.NearClipPlane = m_Camera.nearClipPlane;
        // m_CMCamera.m_Lens.FarClipPlane = m_Camera.farClipPlane;
    }
}
