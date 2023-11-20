using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OverrideBakedLightingTransform))]
public class OverrideBakedLightingTransformEd : Editor
{
     static readonly string[] kValidShaders = { "HDRP/Lit", "Shader Graphs/LitSpecial" };
          
     public override void OnInspectorGUI()
     {
          using (new EditorGUI.DisabledScope(true))
          {
               EditorGUILayout.PropertyField(serializedObject.FindProperty("renderers"));
               EditorGUILayout.PropertyField(serializedObject.FindProperty("transforms"));
               EditorGUILayout.PropertyField(serializedObject.FindProperty("transformsI"));
          }

          EditorGUILayout.Space();

          var overrideTarget = (OverrideBakedLightingTransform)target;

          if (overrideTarget.isCaptured)
          {
               if (GUILayout.Button("Release"))
               {
                    overrideTarget.Release();

                    overrideTarget.renderers = System.Array.Empty<Renderer>();
                    overrideTarget.transforms = System.Array.Empty<Matrix4x4>(); 
                    overrideTarget.transformsI = System.Array.Empty<Matrix4x4>(); 
                    overrideTarget.isCaptured = false;
               }
          }
          else
          {
               if (GUILayout.Button("Capture"))
               {
                    var supportedShaders = kValidShaders.Select(Shader.Find).ToArray();
                    
                    overrideTarget.renderers = overrideTarget.GetComponentsInChildren<Renderer>()
                         .Where(r => (r is MeshRenderer mr && mr.receiveGI == ReceiveGI.LightProbes) /*|| r is SkinnedMeshRenderer*/)
                         //.Where(r => r.sharedMaterials.Any(m => m != null && supportedShaders.Contains(m.shader)))
                         .Where(r => !r.GetComponent<BlockOverrideBakedLighting>() && !r.GetComponentsInParent<BlockOverrideBakedLighting>().Any(b => b.recursively))
                         .ToArray();
                    overrideTarget.CaptureIndicesFromRenderers();
                    overrideTarget.transforms = overrideTarget.renderers.Select(r => r.transform.localToWorldMatrix).ToArray();
                    overrideTarget.transformsI = overrideTarget.renderers.Select(r => r.transform.worldToLocalMatrix).ToArray(); 
                    overrideTarget.isCaptured = true;
                    
                    overrideTarget.Refresh();
               }

               if (overrideTarget.rendererIndices.Length == 0)
               {
                    if (GUILayout.Button("Refresh"))
                         overrideTarget.CaptureIndicesFromRenderers();
               }
          }
     }
}
