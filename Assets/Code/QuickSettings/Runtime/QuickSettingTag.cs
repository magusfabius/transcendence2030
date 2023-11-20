using UnityEngine;

public class QuickSettingTag : MonoBehaviour
{
    public new string tag;
    public QuickSettings.Preset[] validPresets;

    public virtual void Action(bool apply)
    {
        gameObject.SetActive(apply);
    }
}
