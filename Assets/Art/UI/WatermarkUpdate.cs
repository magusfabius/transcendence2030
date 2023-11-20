using UnityEngine;
using UnityEngine.UI;

class WatermarkUpdate : MonoBehaviour
{
    public string template;
    public Text target;
}

#if UNITY_EDITOR
class WatermarkUpdateProcessor : UnityEditor.Build.IProcessSceneWithReport
{
    public int callbackOrder => 0;
    
    public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, UnityEditor.Build.Reporting.BuildReport report)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var watermark = root.GetComponent<WatermarkUpdate>();
            
            if(watermark == null)
                continue;

            if (watermark.target != null && !string.IsNullOrEmpty(watermark.template))
                watermark.target.text = string.Format(watermark.template, System.DateTime.Today.Date.ToString("MMMM d. yyyy", System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
#endif
