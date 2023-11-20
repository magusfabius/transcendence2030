using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class EnsureOverrideBakedLightingBindings
{
    static readonly int kMatricesBuffer = Shader.PropertyToID("_MatricesBuffer");
    static readonly int kMatricesIBuffer = Shader.PropertyToID("_MatricesIBuffer");
    static readonly int kMatricesWTWBuffer = Shader.PropertyToID("_MatricesWTWBuffer");
    static readonly int kMatricesWTWITBuffer = Shader.PropertyToID("_MatricesWTWITBuffer");
    static readonly int kInstanceHash = Shader.PropertyToID("_InstanceHash");
    static readonly int kInstanceHashAlignedSize = Shader.PropertyToID("_InstanceHashAlignedSize");

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod] static void EditorInitialize() => Initialize();
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] static void RuntimeInitialize() => Initialize();
    static void Initialize()
    {
        RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
        BindDefaultData();
    }

    static bool sHadActiveInstanceData;
    static bool sHasActiveInstanceData;
    static bool sDefaultDataBound;
    
    static void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    { 
        if ((sHadActiveInstanceData && !sHasActiveInstanceData) || (!sDefaultDataBound && !sHasActiveInstanceData) )
        {
            sHadActiveInstanceData = false;
            BindDefaultData();
        }
    }

    // TODO: Figure out a way to release this dummy buffer in editor
    static ComputeBuffer sDummyBuffer;
    static void BindDefaultData()
    {
        if (HDRenderPipeline.currentPipeline == null)
            return;

        var deferredShader = HDRenderPipeline.currentPipeline.defaultResources.shaders.deferredCS;
        var ssgiShader = HDRenderPipeline.currentPipeline.defaultResources.shaders.screenSpaceGlobalIlluminationCS;
        if (sDummyBuffer == null)
        {
            sDummyBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<Matrix4x4>());
            sDummyBuffer.SetData(new Matrix4x4[1]);
        }
        BindData(deferredShader, ssgiShader, sDummyBuffer, sDummyBuffer, sDummyBuffer, sDummyBuffer, sDummyBuffer, 1);
        sDefaultDataBound = true;
    }

    public static void UnbindInstanceData()
    {
        sHasActiveInstanceData = false;
    }

    public static void BindInstanceData(ComputeShader deferredShader, ComputeShader ssgiShader, ComputeBuffer matrices, ComputeBuffer matricesI, ComputeBuffer matricesWTW, ComputeBuffer matricesWTWIT, ComputeBuffer instanceHash, int instanceCount)
    {
        if (matrices != null && matricesI != null && matricesWTW != null && matricesWTWIT != null)
        {
            BindData(deferredShader, ssgiShader, matrices, matricesI, matricesWTW, matricesWTWIT, instanceHash, instanceCount);
            sHadActiveInstanceData = sHasActiveInstanceData = true;
        }
    }

    static void BindData(ComputeShader deferredShader, ComputeShader ssgiShader, ComputeBuffer matrices, ComputeBuffer matricesI, ComputeBuffer matricesWTW, ComputeBuffer matricesWTWIT, ComputeBuffer instanceHash, int instanceCount)
    {
        Shader.SetGlobalBuffer(kMatricesBuffer, matrices);
        Shader.SetGlobalBuffer(kMatricesIBuffer, matricesI);
        Shader.SetGlobalBuffer(kMatricesWTWBuffer, matricesWTW);
        Shader.SetGlobalBuffer(kMatricesWTWITBuffer, matricesWTWIT);
        Shader.SetGlobalBuffer(kInstanceHash, instanceHash);
        Shader.SetGlobalInteger(kInstanceHashAlignedSize, instanceCount);

        if (deferredShader)
        {
            for (var i = 0; i < 29; ++i)
            {
                deferredShader.SetBuffer(i, kMatricesBuffer, matrices);
                deferredShader.SetBuffer(i, kMatricesIBuffer, matricesI);
                deferredShader.SetBuffer(i, kMatricesWTWBuffer, matricesWTW);
                deferredShader.SetBuffer(i, kMatricesWTWITBuffer, matricesWTWIT);
                deferredShader.SetBuffer(i, kInstanceHash, instanceHash);
            }
            deferredShader.SetInt(kInstanceHashAlignedSize, instanceCount);
        }

        if (ssgiShader)
        {
            for (var i = 0; i < 4; ++i)
            {
                ssgiShader.SetBuffer(i, kMatricesBuffer, matrices);
                ssgiShader.SetBuffer(i, kMatricesIBuffer, matricesI);
                ssgiShader.SetBuffer(i, kMatricesWTWBuffer, matricesWTW);
                ssgiShader.SetBuffer(i, kMatricesWTWITBuffer, matricesWTWIT);
                ssgiShader.SetBuffer(i, kInstanceHash, instanceHash);
            }
            ssgiShader.SetInt(kInstanceHashAlignedSize, instanceCount);
        }

        sDefaultDataBound = false;
    }
}
