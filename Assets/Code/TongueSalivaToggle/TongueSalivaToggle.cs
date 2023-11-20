using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class TongueSalivaToggle : MonoBehaviour
{
    public SkinnedMeshRenderer target;

    private void OnEnable()
    {
        if (target == null) return;
    
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        target.GetPropertyBlock(block);
        block.SetFloat("_SalivaAlphaMultiplier", 1.0f);
        target.SetPropertyBlock(block);
    }

    private void LateUpdate()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        target.GetPropertyBlock(block);
        block.SetFloat("_SalivaAlphaMultiplier", 1.0f);
        target.SetPropertyBlock(block);
    }

    private void OnDisable()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        target.GetPropertyBlock(block);
        block.SetFloat("_SalivaAlphaMultiplier", 0.0f);
        target.SetPropertyBlock(block);
    }
}
