using UnityEngine;

public class QSLTTargetDisable : QuickSettingLogicTag
{
    public Renderer[] renderers = System.Array.Empty<Renderer>();
    public MonoBehaviour[] behaviours = System.Array.Empty<MonoBehaviour>();
    public GameObject[] gameObjects = System.Array.Empty<GameObject>();

    public override void Action(bool apply)
    {
        foreach (var r in renderers)
        {
            if (r)
            {
                r.enabled = !apply;
                Debug.Log($"[QSLTTargetDisable] {(apply ? "Apply" : "Revert")}: Setting {r.name}.enabled = {!apply}.");
            }
        }

        foreach (var b in behaviours)
        {
            if (b)
            {
                b.enabled = !apply;
                Debug.Log($"[QSLTTargetDisable] {(apply ? "Apply" : "Revert")}: Setting {b.name}.enabled = {!apply}.");
            }
        }

        foreach (var go in gameObjects)
        {
            if (go)
            {
                go.SetActive(!apply);
                Debug.Log($"[QSLTTargetDisable] {(apply ? "Apply" : "Revert")}: Setting {go.name}.activeSelf = {!apply}.");
            }
        }
    }
}
