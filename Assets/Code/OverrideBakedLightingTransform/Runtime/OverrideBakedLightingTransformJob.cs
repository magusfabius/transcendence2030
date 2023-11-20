using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile(DisableSafetyChecks = true)]
public struct OverrideBakedLightingTransformJob : IJobParallelForTransform
{
    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float4x4> transforms;
    [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float4x4> worldToWorlds;
    [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float4x4> worldToWorldITs;

    public void Execute(int i, TransformAccess transform)
    {
        var rWorldToLocal = (float4x4)transform.worldToLocalMatrix;

        var wtw = math.mul(transforms[i], rWorldToLocal);
        worldToWorlds[i] = wtw;

        var wtwIT = math.mul(math.transpose(math.inverse(rWorldToLocal)), math.transpose(math.inverse(transforms[i])));
        worldToWorldITs[i] = wtwIT;
    }
}
