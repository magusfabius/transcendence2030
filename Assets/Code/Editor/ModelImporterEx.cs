using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

class ModelImporterEx : AssetPostprocessor
{
    const string kNoRaytracing = "noraytrace";
    const string kContributeGIReceiveLightProbes = "cgirlp";

    static string AddKey(string existing, string add)
    {
        if (string.IsNullOrEmpty(existing))
            return add;
        if (!existing.Contains(add))
            return existing + ";" + add;
        return existing;
    }

    static string RemoveKey(string existing, string remove)
    {
        if (!string.IsNullOrEmpty(existing))
            return existing.Replace(remove, "");
        return string.Empty;
    }

    //[MenuItem("Tools/Mark Asset as No-Raytrace")]
    static void MarkNoRaytrace()
    {
        foreach (var o in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            var assetImporter = (ModelImporter) AssetImporter.GetAtPath(path);
            assetImporter.userData = AddKey(assetImporter.userData, kNoRaytracing);
            assetImporter.SaveAndReimport();
        }
    }

    //[MenuItem("Tools/Clear Asset from No-Raytrace")]
    static void ClearNoRaytrace()
    {
        foreach (var o in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            var assetImporter = (ModelImporter) AssetImporter.GetAtPath(path);
            assetImporter.userData = RemoveKey(assetImporter.userData, kContributeGIReceiveLightProbes);
            assetImporter.SaveAndReimport();
        }
    }

    //[MenuItem("Tools/Mark Asset as CGIRLP")]
    static void MarkContributeGIReceiveLightProbes()
    {
        foreach (var o in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            var assetImporter = (ModelImporter) AssetImporter.GetAtPath(path);
            assetImporter.userData = AddKey(assetImporter.userData, kContributeGIReceiveLightProbes);
            assetImporter.SaveAndReimport();
        }
    }

    //[MenuItem("Tools/Clear Asset from CGIRLP")]
    static void ClearContributeGIReceiveLightProbes()
    {
        foreach (var o in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            var assetImporter = (ModelImporter) AssetImporter.GetAtPath(path);
            assetImporter.userData = RemoveKey(assetImporter.userData, kContributeGIReceiveLightProbes);
            assetImporter.SaveAndReimport();
        }
    }

    void ApplySettingsContributeGIReceiveLightProbes(GameObject gameObject)
    {
        void ApplySettings(MeshRenderer renderer, bool contribute, bool receive, bool probes)
        {
            if(contribute)
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) | StaticEditorFlags.ContributeGI);
            else
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & ~StaticEditorFlags.ContributeGI);

            if(probes)
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) | StaticEditorFlags.ReflectionProbeStatic);
            else
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & ~StaticEditorFlags.ReflectionProbeStatic);

            renderer.receiveGI = receive ? ReceiveGI.LightProbes : ReceiveGI.Lightmaps;
        }
        
        int appliedCount = 0, appliedNoLod = 0, appliedLod0 = 0, appliedLodX = 0, lodSkipCount = 0;
        foreach (var meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            var parentLODGroup = meshRenderer.GetComponentInParent<LODGroup>();
            if (parentLODGroup != null)
            {
                var lods = parentLODGroup.GetLODs();
                for (var i = 0; i < lods.Length; ++i)
                {
                    if (System.Array.IndexOf(lods[i].renderers, meshRenderer) != -1)
                    {
                        ++lodSkipCount;
                        goto loop;
                    }
                }
            }
            
            ApplySettings(meshRenderer, true, true, true);
            
            ++appliedNoLod;
            ++appliedCount;
            
            loop:
            ;
        }

        foreach (var lodGroup in gameObject.GetComponentsInChildren<LODGroup>())
        {
            var lods = lodGroup.GetLODs();
            for (var i = 0; i < lods.Length; ++i)
            {
                foreach (var renderer in lods[i].renderers)
                {
                    if (renderer is MeshRenderer meshRenderer)
                    {
                        ApplySettings(meshRenderer, i == 0, true, i == 0);

                        ++appliedCount;
                        if (i == 0) ++appliedLod0;
                        else ++appliedLodX;
                    }
                }
            }
        }
        
        if(appliedCount > 0)
            Debug.Log($"Applied GI settings to {appliedCount} renderers. {appliedNoLod} with no LOD, {appliedLod0} LOD0, {appliedLodX} other LODs. (Initially skipped {lodSkipCount} renderers in LODGroup)");
    }

    void OnPostprocessModel(GameObject gameObject)
    {
        if (assetImporter.userData == null)
            return;
        
        if (assetImporter.userData.Contains(kContributeGIReceiveLightProbes))
        {
            ApplySettingsContributeGIReceiveLightProbes(gameObject);
        }


        if (assetImporter.userData.Contains(kNoRaytracing))
        {
            ApplyNoRaytracing(gameObject);
        }
    }

    void ApplyNoRaytracing(GameObject gameObject)
    {
        foreach (var skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Debug.Log($"Turning off ray-tracing for {gameObject.name} : {skinnedMeshRenderer.name}");
            skinnedMeshRenderer.rayTracingMode = RayTracingMode.Off;
        }
    }
}
