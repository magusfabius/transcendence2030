#define NO_MENUS

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#if ENABLE_ENTITIES
using Unity.Entities;
#endif

internal class PlayerLoopStripper : MonoBehaviour
{
    private void Start()
    {
        StripPlayerLoop();
    }

#if UNITY_EDITOR && !NO_MENUS
    [UnityEditor.MenuItem("Tools/Player Loop/List")]
    private static void ListPlayerLoop()
    {
        ListPlayerLoop(GetPlayerLoopSystem());
    }

    private static void ListPlayerLoop(PlayerLoopSystem pls, int depth = 0)
    {
        if(pls.type != null)
            Debug.Log("".PadLeft(depth * 2) + $"{pls.type.Name}");

        var subSystemCount = pls.subSystemList?.Length;
        for (var i = 0; i < subSystemCount; ++i)
            ListPlayerLoop(pls.subSystemList[i], depth + 1);
    }
#endif

#if UNITY_EDITOR && !NO_MENUS
    [UnityEditor.MenuItem("Tools/Player Loop/Strip")]
#endif
    private static void StripPlayerLoop()
    {
        var playerLoop = GetPlayerLoopSystem();
        
        for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
        {
            if (playerLoop.subSystemList[i].type == typeof(EarlyUpdate))
            {
                var newSubsystemList = new List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);
                newSubsystemList.RemoveAll(pls => pls.type == typeof(EarlyUpdate.Physics2DEarlyUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(EarlyUpdate.PhysicsResetInterpolatedTransformPosition));
                UpdateSubSystems(ref playerLoop.subSystemList[i], newSubsystemList);
            }
            else if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
            {
                var newSubsystemList = new List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);
                newSubsystemList.RemoveAll(pls => pls.type == typeof(FixedUpdate.PhysicsFixedUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(FixedUpdate.Physics2DFixedUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(FixedUpdate.ScriptRunDelayedFixedFrameRate));
                UpdateSubSystems(ref playerLoop.subSystemList[i], newSubsystemList);
            }
            else if (playerLoop.subSystemList[i].type == typeof(PreUpdate))
            {
                var newSubsystemList = new List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);
                newSubsystemList.RemoveAll(pls => pls.type == typeof(PreUpdate.PhysicsUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(PreUpdate.Physics2DUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(PreUpdate.SendMouseEvents));
                UpdateSubSystems(ref playerLoop.subSystemList[i], newSubsystemList);
            }
            else if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
            {
                var newSubsystemList = new List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);
                newSubsystemList.RemoveAll(pls => pls.type == typeof(PostLateUpdate.PhysicsSkinnedClothBeginUpdate));
                newSubsystemList.RemoveAll(pls => pls.type == typeof(PostLateUpdate.PhysicsSkinnedClothFinishUpdate));
                UpdateSubSystems(ref playerLoop.subSystemList[i], newSubsystemList);
            }
        }

        SetPlayerLoop(playerLoop);
    }

    static void UpdateSubSystems(ref PlayerLoopSystem system, List<PlayerLoopSystem> subSystems)
    {
        //Debug.LogFormat("Stripped {0} sub-systems from player loop system {1}.", system.subSystemList.Length - subSystems.Count, system.type.Name);
        system.subSystemList = subSystems.ToArray();
    }

    static PlayerLoopSystem GetPlayerLoopSystem()
    {
        return
#if ENABLE_ENTITIES
        World.DefaultGameObjectInjectionWorld != null ? ScriptBehaviourUpdateOrder.CurrentPlayerLoop : 
#endif
        PlayerLoop.GetCurrentPlayerLoop();
    }

    static void SetPlayerLoop(PlayerLoopSystem playerLoopSystem)
    {
#if ENABLE_ENTITIES
        if(World.DefaultGameObjectInjectionWorld != null)
            ScriptBehaviourUpdateOrder.SetPlayerLoop(playerLoopSystem);
        else
#endif
        PlayerLoop.SetPlayerLoop(playerLoopSystem);
    }
}
