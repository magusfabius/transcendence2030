#define WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

[ExecuteAlways]
public class OverrideBakedLightingTransform : MonoBehaviour //, ITimeControl
{
    static readonly int kObjectID = Shader.PropertyToID("_ObjectID");
    
    [SerializeField] public bool isCaptured;
    [SerializeField] public Renderer[] renderers = System.Array.Empty<Renderer>();
    [SerializeField] public HashKeyValue[] rendererIndices = System.Array.Empty<HashKeyValue>();
    [SerializeField] public Matrix4x4[] transforms = System.Array.Empty<Matrix4x4>();
    [SerializeField] public Matrix4x4[] transformsI = System.Array.Empty<Matrix4x4>();
    [SerializeField] public Matrix4x4[] transformsWorldToWorld = System.Array.Empty<Matrix4x4>();
    [SerializeField] public Matrix4x4[] transformsWorldToWorldIT = System.Array.Empty<Matrix4x4>();

    [HideInInspector] [SerializeField] ComputeShader deferredComputeShader;
    [HideInInspector] [SerializeField] ComputeShader ssgiComputeShader;
    
    MaterialPropertyBlock m_MaterialPropertyBlock;
    TransformAccessArray m_TransformAccess;
    NativeArray<float4x4> m_Transforms;
#if !WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
    NativeArray<float4x4> m_transformsWorldToWorld;
    NativeArray<float4x4> m_transformsWorldToWorldIT;
#endif
    JobHandle m_UpdateWorldToWorldHandle;
    bool m_JobScheduled;

    ComputeBuffer m_MatricesBuffer;
    ComputeBuffer m_MatricesIBuffer;
    ComputeBuffer m_MatricesWorldToWorldBuffer;
    ComputeBuffer m_MatricesWorldToWorldITBuffer;

    [System.Serializable]
    public struct HashKeyValue
    {
        public uint Key;
        public uint Value;
    }
    ComputeBuffer m_InstanceHash;

    void Awake() { CaptureIndicesFromRenderers(); Allocate(); Clear(); } 
    void OnEnable() => Set();
    void OnDisable() => Clear();
    void OnDestroy() => Free();
    void LateUpdate() => UpdateWorldToWorldSchedule();
    
    public void Refresh()
    {
        Free();
        
        if(enabled)
            Set();
    }

    public void Release()
    {
        Clear();
        Free();
    }
    
    void Allocate()
    {
        if(m_MaterialPropertyBlock == null)
            m_MaterialPropertyBlock = new MaterialPropertyBlock();

        if (!m_TransformAccess.isCreated || m_TransformAccess.length != transforms.Length)
        {
            if(m_TransformAccess.isCreated)
                m_TransformAccess.Dispose();

            if (m_Transforms.IsCreated)
                m_Transforms.Dispose();

            if (transforms.Length > 0)
            {
                var rendererTransforms = new Transform[renderers.Length];

                m_Transforms = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);
                for (int i = 0, n = renderers.Length; i < n; ++i)
                {
                    rendererTransforms[i] = renderers[i] != null ? renderers[i].transform : transform;
                    m_Transforms[i] = transforms[i];
                }
                
                m_TransformAccess = new TransformAccessArray(rendererTransforms);
            }
        }

        if (transformsWorldToWorld == null || transformsWorldToWorld.Length != transforms.Length)
        {
            transformsWorldToWorld = new Matrix4x4[transforms.Length];
            transformsWorldToWorldIT = new Matrix4x4[transforms.Length];
        }

#if !WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
        if (m_transformsWorldToWorld.IsCreated)
            m_transformsWorldToWorld.Dispose();
        m_transformsWorldToWorld = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);


        if (m_transformsWorldToWorldIT.IsCreated)
            m_transformsWorldToWorldIT.Dispose();
        m_transformsWorldToWorldIT = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);
#endif

        if (m_MatricesBuffer == null || m_MatricesBuffer.count != transforms.Length)
        {
            m_MatricesBuffer?.Dispose();
            m_MatricesIBuffer?.Dispose();
            m_MatricesWorldToWorldBuffer?.Dispose();
            m_MatricesWorldToWorldITBuffer?.Dispose();
            m_MatricesBuffer = m_MatricesIBuffer = null;
            m_MatricesWorldToWorldBuffer = m_MatricesWorldToWorldITBuffer = null;

            if (transforms.Length > 0)
            {
                m_MatricesBuffer = new ComputeBuffer(transforms.Length, UnsafeUtility.SizeOf<Matrix4x4>());
                m_MatricesIBuffer = new ComputeBuffer(transformsI.Length, UnsafeUtility.SizeOf<Matrix4x4>());
#if WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
                m_MatricesWorldToWorldBuffer = new ComputeBuffer(transforms.Length, UnsafeUtility.SizeOf<Matrix4x4>(), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
                m_MatricesWorldToWorldITBuffer = new ComputeBuffer(transforms.Length, UnsafeUtility.SizeOf<Matrix4x4>(), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
#else
                m_MatricesWorldToWorldBuffer = new ComputeBuffer(transforms.Length, UnsafeUtility.SizeOf<Matrix4x4>());
                m_MatricesWorldToWorldITBuffer = new ComputeBuffer(transforms.Length, UnsafeUtility.SizeOf<Matrix4x4>());
#endif
            }
        }

        if (m_InstanceHash == null || rendererIndices.Length != (uint)Mathf.NextPowerOfTwo(transforms.Length))
        {
            m_InstanceHash = new ComputeBuffer(rendererIndices.Length, UnsafeUtility.SizeOf<HashKeyValue>(), ComputeBufferType.Structured);
        }
    }

    void Free()
    {
        m_MaterialPropertyBlock = null;
        
        if(m_TransformAccess.isCreated)
            m_TransformAccess.Dispose();
        
        m_MatricesBuffer?.Dispose();
        m_MatricesIBuffer?.Dispose();
        m_MatricesWorldToWorldBuffer?.Dispose();
        m_MatricesWorldToWorldITBuffer?.Dispose();
        m_MatricesBuffer = m_MatricesIBuffer = null;
        m_MatricesWorldToWorldBuffer = m_MatricesWorldToWorldITBuffer = null;

        if (m_Transforms.IsCreated)
            m_Transforms.Dispose();

#if !WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
        if (m_transformsWorldToWorld.IsCreated)
            m_transformsWorldToWorld.Dispose();

        if (m_transformsWorldToWorldIT.IsCreated)
            m_transformsWorldToWorldIT.Dispose();
#endif
        
        m_InstanceHash?.Dispose();
        m_InstanceHash = null;
    }
    
    void Set()
    {
        if(renderers.Length == 0)
            return;
        
        Allocate();

        EnsureOverrideBakedLightingBindings.BindInstanceData(deferredComputeShader, ssgiComputeShader, 
            m_MatricesBuffer, m_MatricesIBuffer, m_MatricesWorldToWorldBuffer, m_MatricesWorldToWorldITBuffer,
            m_InstanceHash, Mathf.NextPowerOfTwo(renderers.Length));

        m_MatricesBuffer.SetData(transforms);
        m_MatricesIBuffer.SetData(transformsI);
        m_InstanceHash.SetData(rendererIndices);
        
        if(rendererIndices.Length == 0)
            m_InstanceHash.SetData(new HashKeyValue[Mathf.NextPowerOfTwo(renderers.Length)]);

        // for (int i = 0, n = renderers.Length; i < n; ++i)
        // {
        //     var r = renderers[i];
        //     if(r == null)
        //         continue;
        //     
        //     m_MaterialPropertyBlock.SetInteger(kObjectID, i + 1);
        //     r.SetPropertyBlock(m_MaterialPropertyBlock);
        // }

        UpdateWorldToWorldSchedule();

        RenderPipelineManager.beginFrameRendering += HandleBeginFrameRendering;
    }

    private void HandleBeginFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
    {
        UpdateWorldToWorldComplete();
    }

    void Clear()
    {
        RenderPipelineManager.beginFrameRendering -= HandleBeginFrameRendering;
        
        UpdateWorldToWorldComplete();

        EnsureOverrideBakedLightingBindings.UnbindInstanceData();

        // if (m_MaterialPropertyBlock == null)
        //     m_MaterialPropertyBlock = new MaterialPropertyBlock();
        //
        // m_MaterialPropertyBlock.SetInteger(kObjectID, 0);
        //
        // foreach (var r in renderers)
        // {
        //     if(r == null)
        //         continue;
        //     
        //     //r.SetPropertyBlock(m_MaterialPropertyBlock);
        //     r.SetPropertyBlock(null);
        // }
    }

    void UpdateWorldToWorld()
    {
        if (m_MatricesWorldToWorldBuffer == null || m_MatricesWorldToWorldITBuffer == null)
            return;

        var worldToWorlds = transformsWorldToWorld;
        var worldToWorldITs = transformsWorldToWorldIT;

        for (int i = 0, n = renderers.Length; i < n; ++i)
        {
            var rTransform = renderers[i] ? renderers[i].transform : transform;
            var rWorldToLocal = rTransform.worldToLocalMatrix;
            
            var wtw = transforms[i] * rWorldToLocal;
            worldToWorlds[i] = wtw;

            var wtwIT = rWorldToLocal.inverse.transpose * transforms[i].inverse.transpose;
            worldToWorldITs[i] = wtwIT;
        }

        m_MatricesWorldToWorldBuffer.SetData(worldToWorlds);
        m_MatricesWorldToWorldITBuffer.SetData(worldToWorldITs);
    }

    void UpdateWorldToWorldSchedule()
    {
        if (m_MatricesWorldToWorldBuffer == null || m_MatricesWorldToWorldITBuffer == null)
            return;

        UpdateWorldToWorldComplete();

        m_UpdateWorldToWorldHandle = new OverrideBakedLightingTransformJob()
        {
            transforms = m_Transforms,
#if WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
            worldToWorlds = m_MatricesWorldToWorldBuffer.BeginWrite<float4x4>(0, renderers.Length),
            worldToWorldITs = m_MatricesWorldToWorldITBuffer.BeginWrite<float4x4>(0, renderers.Length)
#else
            worldToWorlds = m_transformsWorldToWorld,
            worldToWorldITs = m_transformsWorldToWorldIT
#endif
        }.ScheduleReadOnly(m_TransformAccess, 64);

        m_JobScheduled = true;
        JobHandle.ScheduleBatchedJobs();
    }

    void UpdateWorldToWorldComplete()
    {
        if (m_MatricesWorldToWorldBuffer == null || m_MatricesWorldToWorldITBuffer == null)
            return;

        if (!m_JobScheduled)
            return;

        m_JobScheduled = false;

        m_UpdateWorldToWorldHandle.Complete();

#if WRITE_DIRECTLY_TO_COMPUTE_BUFFERS
        m_MatricesWorldToWorldBuffer.EndWrite<float4x4>(renderers.Length);
        m_MatricesWorldToWorldITBuffer.EndWrite<float4x4>(renderers.Length);
#else
        m_MatricesWorldToWorldBuffer.SetData(m_transformsWorldToWorld);
        m_MatricesWorldToWorldITBuffer.SetData(m_transformsWorldToWorldIT);
#endif
    }

    public void CaptureIndicesFromRenderers()
    {
        static uint hash(uint k, uint size)
        {
            k ^= k >> 16;
            k *= 0x85ebca6b;
            k ^= k >> 13;
            k *= 0xc2b2ae35;
            k ^= k >> 16;
            return k & (size - 1);
        }
          
        static void insert(HashKeyValue[] hashArray, uint key, uint value)
        {
            uint size = (uint)hashArray.Length;
            uint slot = hash(key, size);

            while(true)
            {
                ref var c = ref hashArray[slot];

                if (c.Key == key)
                {
                    Debug.LogWarning("Unexpected collision");
                    return;
                }
                    
                if (c.Key == 0u)
                {
                    c.Key = key;
                    c.Value = value;
                    return;
                }

                slot = (slot + 1) & (size - 1);
            }
        }
          
        var size = Mathf.NextPowerOfTwo(renderers.Length);
        var array = rendererIndices = new HashKeyValue[size];

        for (var i = 0; i < renderers.Length; ++i)
        {
            var key = (uint)renderers[i].GetInstanceID();
            insert(array, key, (uint)(i + 1));
        }
    }
    
    // public void SetTime(double time) => DoUpdate();
    // public void OnControlTimeStart() => enabled = true;
    // public void OnControlTimeStop() => enabled = false;
}
