using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

//[ExecuteInEditMode]
public class ui_controller : MonoBehaviour
{
    public static readonly string[] presetNames = { "Low", "Medium", "High", "Ultra", "Custom" };
    public static readonly string[] qualitySettingsNamesOff = { "Off", "Low", "Medium", "High" };
    public static readonly string[] qualitySettingsNames = { "Low", "Medium", "High" };
    public static readonly string[] dlssSettingsNames = { "Ultra Performance", "Max Performance", "Balanced", "Max Quality" };
    public static readonly string[] textureSettingsNames = { "Quarter", "Half", "Full" };
    public static readonly string[] toggleNames = { "Off", "On" };
    public static readonly string[] raytraceNames = { "Raytrace Off", "Raytrace On" };
    public static readonly string[] displayModeNames = { "Windowed", "Fullscreen Windowed" };
    public static readonly string[] upscaleModeNames = { "NATIVE", /*"TAAU",*/ "FSR", "DLSS" };
    public static readonly string[] vsyncNames = { "Off", "Every", "Other", "Third", "Fourth" };

    private int menuMoveDir;

    public static System.Func<Caps> ReadSystemCaps;
    public static System.Action<Caps, SettingsValues> WriteChangedSettings;
    public static System.Func<Caps, SettingsValues> ReadCurrentSettings;
    public System.Action OnRequestedQuit;
    public System.Action OnRequestedClose;
    

    public Button goBackButton;

    public Button quitButton, quitButtonNo, quitButtonYes;
    public VisualElement groupConfirmQuit;

    #region Settings Variables
    //Preset Settings
    public Button presetsButtonPrev, presetsButtonNext;
    public Label presetsLableDisplaySetting;
    public VisualElement[] presetsIndicators;

    //Display Mode Settings
    public Button displayModeButtonPrev, displayModeButtonNext;
    public Label displayModeLableDisplaySetting;
    public VisualElement[] displayModeIndicators;

    //Upscale Mode Settings
    public Button upscaleModeButtonPrev, upscaleModeButtonNext;
    public Label upscaleModeLableDisplaySetting;
    public VisualElement[] upscaleModeIndicators;

    //Resolutions Settings
    public Button resolutionsButtonPrev, resolutionsButtonNext;
    public Label resolutionsLableDisplaySetting;
    public VisualElement resolutionsSettings;
    public VisualElement resolutionsIndicatorsGroup;
    public VisualElement[] resolutionsIndicators;
    List<string> resolutionNames = new List<string>();
    string[] resolutionsNamesArray;

    //Resolution Scale Settings
    public Button resolutionScaleButtonPrev, resolutionScaleButtonNext;
    public Label resolutionScaleLableDisplaySetting;
    public ProgressBar resolutionScaleIndicator;
    public VisualElement resolutonScaleSettings;

    //DLSS Settings
    public Button dlssButtonPrev, dlssButtonNext;
    public Label dlssLableDisplaySetting;
    public VisualElement[] dlssIndicators;
    public VisualElement dlssSettings;

    //Shadows Settings
    public Button shadowsButtonPrev, shadowsButtonNext;
    public Label shadowsLableDisplaySetting;
    public VisualElement[] shadowsIndicators;

    //Shadows Settings - RT
    public Button shadowsRtButtonPrev, shadowsRtButtonNext;
    public Label shadowsRtLableDisplaySetting;
    public VisualElement[] shadowsRtIndicators;

    //AO Settings
    public Button aoButtonPrev, aoButtonNext;
    public Label aoLableDisplaySetting;
    public VisualElement[] aoIndicators;

    //AO Settings - RT
    public Button aoRtButtonPrev, aoRtButtonNext;
    public Label aoRtLableDisplaySetting;
    public VisualElement[] aoRtIndicators;

    //Reflections Settings
    public Button reflectionsButtonPrev, reflectionsButtonNext;
    public Label reflectionsLableDisplaySetting;
    public VisualElement[] reflectionsIndicators;

    //Reflections Settings - RT
    public Button reflectionsRtButtonPrev, reflectionsRtButtonNext;
    public Label reflectionsRtLableDisplaySetting;
    public VisualElement[] reflectionsRtIndicators;

    //Lighting Quality Settings
    public Button lightingQualityButtonPrev, lightingQualityButtonNext;
    public Label lightingQualityLableDisplaySetting;
    public VisualElement[] lightingQualityIndicators;

    //Material Quality Settings
    public Button materialQualityButtonPrev, materialQualityButtonNext;
    public Label materialQualityLableDisplaySetting;
    public VisualElement[] materialQualityIndicators;

    //Textures Size Settings
    public Button texturesSizeButtonPrev, texturesSizeButtonNext;
    public Label texturesSizeLableDisplaySetting;
    public VisualElement[] texturesSizeIndicators;

    //Postprocessing Settings
    public Button postprocessingButtonPrev, postprocessingButtonNext;
    public Label postprocessingLableDisplaySetting;
    public VisualElement[] postprocessingIndicators;

    //Hair Settings
    public Button hairButtonPrev, hairButtonNext;
    public Label hairLableDisplaySetting;
    public VisualElement[] hairIndicators;

    //Vsync Settings
    public Button vsyncButtonPrev, vsyncButtonNext;
    public Label vsyncLableDisplaySetting;
    public VisualElement[] vsyncIndicators;

    //ShowFPS Settings
    public Button fpsButtonPrev, fpsButtonNext;
    public Label fpsLableDisplaySetting;
    public VisualElement[] fpsIndicators;
    #endregion

    public struct Caps
    {
        public bool IsRaytracingSupported;
        public bool IsDLSSSupported;
        public string DesktopInfo;
        public List<Resolution> SupportedWindowedResolutions;
        public List<Resolution> SupportedFullscreenResolutions;
    }
    
    public struct SettingsValues
    {
        public int presetsCurrentSettingIndex;

        public int displayModeCurrentSettingIndex;

        public float resolutionScale;

        public int resolutionsCurrentSettingsIndex;

        public int dlssCurrentSettingIndex;

        public int shadowsCurrentSettingIndex;
        public int shadowsRtCurrentSettingIndex;

        public int aoCurrentSettingIndex;
        public int aoRtCurrentSettingIndex;

        public int reflectionsCurrentSettingIndex;
        public int reflectionsRtCurrentSettingIndex;

        public int lightingQualityCurrentSettingIndex;
        public int materialQualityCurrentSettingIndex;
        public int texturesSizeCurrentSettingIndex;

        public int postprocessingCurrentSettingIndex;

        public int hairCurrentSettingIndex;

        public int upscaleModeCurrentSettingIndex;

        public int vsyncCurrentSettingIndex;

        public int fpsCurrentSettingIndex;
    }

    SettingsValues settingsValues;
    Caps caps;

    void OnEnable()
    {
        if (ReadSystemCaps != null)
        {
            caps = ReadSystemCaps();
        }
        
        Debug.Log($"UI Caps RT Support Detected: {caps.IsRaytracingSupported}");

        var root = GetComponent<UIDocument>().rootVisualElement;

        #region Main Buttons: Go Back & Quit

        goBackButton = root.Q<Button>("button-go-back");
        goBackButton.clicked += GoBack;

        quitButton = root.Q<Button>("button-quit");
        quitButton.clicked += Quit;

        groupConfirmQuit = root.Q<VisualElement>("group-confirm-quit");

        quitButtonNo = root.Q<Button>("button-quit-no");
        quitButtonNo.clicked += CancelQuit;

        quitButtonYes = root.Q<Button>("button-quit-yes");
        quitButtonYes.clicked += ConfirmQuit;

        #endregion

        #region Settings Buttons

        GetMenuElements("quality-presets", ref presetsButtonPrev, ref presetsButtonNext, ref presetsLableDisplaySetting, ref presetsIndicators);
        presetsButtonPrev.clickable.clickedWithEventInfo += ChangePresetSettings;
        presetsButtonNext.clickable.clickedWithEventInfo += ChangePresetSettings;

        GetMenuElements("display-mode-settings", ref displayModeButtonPrev, ref displayModeButtonNext, ref displayModeLableDisplaySetting, ref displayModeIndicators);
        displayModeButtonPrev.clickable.clickedWithEventInfo += ChangeDisplayModeSettings;
        displayModeButtonNext.clickable.clickedWithEventInfo += ChangeDisplayModeSettings;

        //remove dlss option if it's not supported
        if (!caps.IsDLSSSupported)
        {
            dlssIndicators = root.Q<VisualElement>("upscale-mode-settings").Q<VisualElement>("group-indicators").Children().ToArray();
            if (dlssIndicators[3] != null) dlssIndicators[3].RemoveFromHierarchy();
        }

        GetMenuElements("upscale-mode-settings", ref upscaleModeButtonPrev, ref upscaleModeButtonNext, ref upscaleModeLableDisplaySetting, ref upscaleModeIndicators);
        upscaleModeButtonPrev.clickable.clickedWithEventInfo += ChangeUpscaleModeSettings;
        upscaleModeButtonNext.clickable.clickedWithEventInfo += ChangeUpscaleModeSettings;

        //Resolutions
        resolutionsSettings = root.Q<VisualElement>("resolutions-settings");
        resolutionsButtonPrev = resolutionsSettings.Q<Button>("button-prev");
        resolutionsButtonNext = resolutionsSettings.Q<Button>("button-next");
        resolutionsLableDisplaySetting = resolutionsSettings.Q<Label>("label-current-setting-name");
        resolutionsIndicatorsGroup = resolutionsSettings.Q<VisualElement>("group-indicators");
        resolutionsButtonPrev.clickable.clickedWithEventInfo += ChangeResolutionSettings;
        resolutionsButtonNext.clickable.clickedWithEventInfo += ChangeResolutionSettings;

        //Resolution Scale
        resolutionScaleIndicator = root.Q<ProgressBar>("progressbar-resolution-scale");
        resolutionScaleIndicator.RegisterCallback<ClickEvent>(ChangeResolutionScaleSettingsSlider);
        resolutionScaleButtonPrev = root.Q<Button>("button-resolution-scale-prev");
        resolutionScaleButtonNext = root.Q<Button>("button-resolution-scale-next");
        resolutonScaleSettings = root.Q<VisualElement>("resolution-scale-settings");
        resolutionScaleButtonPrev.clickable.clickedWithEventInfo += ChangeResolutionScaleSettings;
        resolutionScaleButtonNext.clickable.clickedWithEventInfo += ChangeResolutionScaleSettings;



        GetMenuElements("dlss-settings", ref dlssButtonPrev, ref dlssButtonNext, ref dlssLableDisplaySetting, ref dlssIndicators);
        dlssSettings = root.Q<VisualElement>("dlss-settings");
        dlssButtonPrev.clickable.clickedWithEventInfo += ChangeDlssSettings;
        dlssButtonNext.clickable.clickedWithEventInfo += ChangeDlssSettings;

        GetMenuElements("shadows-settings", ref shadowsButtonPrev, ref shadowsButtonNext, ref shadowsLableDisplaySetting, ref shadowsIndicators);
        shadowsButtonPrev.clickable.clickedWithEventInfo += ChangeShadowsSettings;
        shadowsButtonNext.clickable.clickedWithEventInfo += ChangeShadowsSettings;

        GetMenuElements("shadows-settings-rt", ref shadowsRtButtonPrev, ref shadowsRtButtonNext, ref shadowsRtLableDisplaySetting, ref shadowsRtIndicators);
        shadowsRtButtonPrev.clickable.clickedWithEventInfo += ChangeShadowsRtSettings;
        shadowsRtButtonNext.clickable.clickedWithEventInfo += ChangeShadowsRtSettings;

        GetMenuElements("ao-settings", ref aoButtonPrev, ref aoButtonNext, ref aoLableDisplaySetting, ref aoIndicators);
        aoButtonPrev.clickable.clickedWithEventInfo += ChangeAoSettings;
        aoButtonNext.clickable.clickedWithEventInfo += ChangeAoSettings;

        GetMenuElements("ao-settings-rt", ref aoRtButtonPrev, ref aoRtButtonNext, ref aoRtLableDisplaySetting, ref aoRtIndicators);
        aoRtButtonPrev.clickable.clickedWithEventInfo += ChangeAoRtSettings;
        aoRtButtonNext.clickable.clickedWithEventInfo += ChangeAoRtSettings;

        GetMenuElements("reflections-settings", ref reflectionsButtonPrev, ref reflectionsButtonNext, ref reflectionsLableDisplaySetting, ref reflectionsIndicators);
        reflectionsButtonPrev.clickable.clickedWithEventInfo += ChangeReflectionsSettings;
        reflectionsButtonNext.clickable.clickedWithEventInfo += ChangeReflectionsSettings;

        GetMenuElements("reflections-settings-rt", ref reflectionsRtButtonPrev, ref reflectionsRtButtonNext, ref reflectionsRtLableDisplaySetting, ref reflectionsRtIndicators);
        reflectionsRtButtonPrev.clickable.clickedWithEventInfo += ChangeReflectionsRtSettings;
        reflectionsRtButtonNext.clickable.clickedWithEventInfo += ChangeReflectionsRtSettings;

        GetMenuElements("lighting-quality-settings", ref lightingQualityButtonPrev, ref lightingQualityButtonNext, ref lightingQualityLableDisplaySetting, ref lightingQualityIndicators);
        lightingQualityButtonPrev.clickable.clickedWithEventInfo += ChangeLightingQualitySettings;
        lightingQualityButtonNext.clickable.clickedWithEventInfo += ChangeLightingQualitySettings;

        GetMenuElements("material-quality-settings", ref materialQualityButtonPrev, ref materialQualityButtonNext, ref materialQualityLableDisplaySetting, ref materialQualityIndicators);
        materialQualityButtonPrev.clickable.clickedWithEventInfo += ChangeMaterialQualitySettings;
        materialQualityButtonNext.clickable.clickedWithEventInfo += ChangeMaterialQualitySettings;

        GetMenuElements("textures-size-settings", ref texturesSizeButtonPrev, ref texturesSizeButtonNext, ref texturesSizeLableDisplaySetting, ref texturesSizeIndicators);
        texturesSizeButtonPrev.clickable.clickedWithEventInfo += ChangeTexturesSizeSettings;
        texturesSizeButtonNext.clickable.clickedWithEventInfo += ChangeTexturesSizeSettings;

        GetMenuElements("postprocessing-settings", ref postprocessingButtonPrev, ref postprocessingButtonNext, ref postprocessingLableDisplaySetting, ref postprocessingIndicators);
        postprocessingButtonPrev.clickable.clickedWithEventInfo += ChangePostprocessingSettings;
        postprocessingButtonNext.clickable.clickedWithEventInfo += ChangePostprocessingSettings;

        GetMenuElements("hair-settings", ref hairButtonPrev, ref hairButtonNext, ref hairLableDisplaySetting, ref hairIndicators);
        hairButtonPrev.clickable.clickedWithEventInfo += ChangeHairSettings;
        hairButtonNext.clickable.clickedWithEventInfo += ChangeHairSettings;

        GetMenuElements("vsync-settings", ref vsyncButtonPrev, ref vsyncButtonNext, ref vsyncLableDisplaySetting, ref vsyncIndicators);
        vsyncButtonPrev.clickable.clickedWithEventInfo += ChangeVsyncSettings;
        vsyncButtonNext.clickable.clickedWithEventInfo += ChangeVsyncSettings;

        GetMenuElements("fps-settings", ref fpsButtonPrev, ref fpsButtonNext, ref fpsLableDisplaySetting, ref fpsIndicators);
        fpsButtonPrev.clickable.clickedWithEventInfo += ChangeFpsSettings;
        fpsButtonNext.clickable.clickedWithEventInfo += ChangeFpsSettings;

        #endregion

        BuildScreenResolutions();

        if (ReadCurrentSettings != null)
        {
            settingsValues = ReadCurrentSettings(caps);
            UpdateAllSettings();
        }

        if (!caps.IsRaytracingSupported)
        {
            root.Q<VisualElement>("shadows-settings-rt").SetEnabled(false);
            root.Q<VisualElement>("shadows-settings-rt").Q<Label>("label-current-setting-name").text = raytraceNames[0].ToUpperInvariant();

            root.Q<VisualElement>("ao-settings-rt").SetEnabled(false);
            root.Q<VisualElement>("ao-settings-rt").Q<Label>("label-current-setting-name").text = raytraceNames[0].ToUpperInvariant();

            root.Q<VisualElement>("reflections-settings-rt").SetEnabled(false);
            root.Q<VisualElement>("reflections-settings-rt").Q<Label>("label-current-setting-name").text = raytraceNames[0].ToUpperInvariant();
        }
    }

    void BuildScreenResolutions()
    {
        resolutionNames.Clear();
        resolutionsIndicatorsGroup.Clear();
        
        var isWindowed = settingsValues.displayModeCurrentSettingIndex == 0;
        //var resolutions = isWindowed ? caps.SupportedWindowedResolutions : caps.SupportedFullscreenResolutions;
        var resolutions = caps.SupportedFullscreenResolutions;
        
        foreach (var res in resolutions)
        {
            //Add the names of the resolutions to be displayed in the UI
            resolutionNames.Add(res.width + " x " + res.height);

            //Add the UI indicators
            var indicator = new VisualElement();
            indicator.AddToClassList("info-setting-indicator");
            resolutionsIndicatorsGroup.Add(indicator);

            resolutionsNamesArray = resolutionNames.ToArray();
            resolutionsIndicators = resolutionsIndicatorsGroup.Q<VisualElement>("group-indicators").Children().ToArray();
        }
    }


    void GoBack()
    {
        //Debug.Log("Close menu");
        OnRequestedClose?.Invoke();
    }

    void Quit()
    {
        quitButton.style.display = DisplayStyle.None;
        groupConfirmQuit.style.display = DisplayStyle.Flex;

        //Debug.Log("Initialize quit");
    }

    void CancelQuit()
    {
        groupConfirmQuit.style.display = DisplayStyle.None;
        quitButton.style.display = DisplayStyle.Flex;

        //Debug.Log("Quitting is canceled");
    }

    void ConfirmQuit()
    {
        //Debug.Log("Quitting is confirmed!");
        OnRequestedQuit?.Invoke();
    }

    void GetMenuElements(string settingName, ref Button buttonPrev, ref Button buttonNext, ref Label displaySetting, ref VisualElement[] indicators)
    {
        var settingRoot = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(settingName);

        buttonPrev = settingRoot.Q<Button>("button-prev");
        buttonNext = settingRoot.Q<Button>("button-next");
        displaySetting = settingRoot.Q<Label>("label-current-setting-name");
        indicators = settingRoot.Q<VisualElement>("group-indicators").Children().ToArray();
    }

    //Sets if the values of the setting changes forward (1) or backward (-1)
    void SetMenuDir(EventBase obj)
    {
        var button = (Button)obj.target;

        if (button.name == "button-prev") menuMoveDir = -1;
        else if (button.name == "button-next") menuMoveDir = 1;
    }

    #region Setting Methods
    void ChangePresetSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(presetNames, presetsLableDisplaySetting, presetsIndicators, ref settingsValues.presetsCurrentSettingIndex, false);
    }

    void ChangeResolutionSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(resolutionsNamesArray, resolutionsLableDisplaySetting, resolutionsIndicators, ref settingsValues.resolutionsCurrentSettingsIndex, false);
    }

    void ChangeResolutionScaleSettings(EventBase obj)
    {
        var button = (Button)obj.target;

        if (button.name == "button-resolution-scale-prev") menuMoveDir = -1;
        else if (button.name == "button-resolution-scale-next") menuMoveDir = 1;

        var p = Mathf.Clamp(settingsValues.resolutionScale + 5.0f * menuMoveDir, 0, 100);
        ChangeResolutionScaleValue(p);
        PushUpdates();
    }

    void ChangeUpscaleModeSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(upscaleModeNames, upscaleModeLableDisplaySetting, upscaleModeIndicators, ref settingsValues.upscaleModeCurrentSettingIndex, false);
    }

    void ChangeResolutionScaleSettingsSlider(ClickEvent evt)
    {
        float p = evt.localPosition.x / resolutionScaleIndicator.worldBound.width * 100;
        ChangeResolutionScaleValue(p);
        PushUpdates();
    }

    void ChangeResolutionScaleValue(float percent)
    {
        settingsValues.resolutionScale = percent;
        resolutionScaleIndicator.value = settingsValues.resolutionScale;
        resolutionScaleIndicator.title = (int)settingsValues.resolutionScale + "%";
    }

    void ChangeDisplayModeSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(displayModeNames, displayModeLableDisplaySetting, displayModeIndicators, ref settingsValues.displayModeCurrentSettingIndex, false);
    }
    
    void ChangeDlssSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(dlssSettingsNames, dlssLableDisplaySetting, dlssIndicators, ref settingsValues.dlssCurrentSettingIndex);
    }

    void ChangeShadowsSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNames, shadowsLableDisplaySetting, shadowsIndicators, ref settingsValues.shadowsCurrentSettingIndex);
    }

    void ChangeShadowsRtSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(raytraceNames, shadowsRtLableDisplaySetting, shadowsRtIndicators, ref settingsValues.shadowsRtCurrentSettingIndex);
    }

    void ChangeAoSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNamesOff, aoLableDisplaySetting, aoIndicators, ref settingsValues.aoCurrentSettingIndex);
    }

    void ChangeAoRtSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(raytraceNames, aoRtLableDisplaySetting, aoRtIndicators, ref settingsValues.aoRtCurrentSettingIndex);
    }

    void ChangeReflectionsSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNamesOff, reflectionsLableDisplaySetting, reflectionsIndicators, ref settingsValues.reflectionsCurrentSettingIndex);
    }

    void ChangeReflectionsRtSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(raytraceNames, reflectionsRtLableDisplaySetting, reflectionsRtIndicators, ref settingsValues.reflectionsRtCurrentSettingIndex);
    }

    void ChangeHairSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNames, hairLableDisplaySetting, hairIndicators, ref settingsValues.hairCurrentSettingIndex);
    }

    void ChangeLightingQualitySettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNames, lightingQualityLableDisplaySetting, lightingQualityIndicators, ref settingsValues.lightingQualityCurrentSettingIndex);
    }

    void ChangeMaterialQualitySettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNames, materialQualityLableDisplaySetting, materialQualityIndicators, ref settingsValues.materialQualityCurrentSettingIndex);
    }

    void ChangeTexturesSizeSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(textureSettingsNames, texturesSizeLableDisplaySetting, texturesSizeIndicators, ref settingsValues.texturesSizeCurrentSettingIndex);
    }

    void ChangeVsyncSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(toggleNames, vsyncLableDisplaySetting, vsyncIndicators, ref settingsValues.vsyncCurrentSettingIndex, false);
    }

    void ChangePostprocessingSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(qualitySettingsNames, postprocessingLableDisplaySetting, postprocessingIndicators, ref settingsValues.postprocessingCurrentSettingIndex);
    }

    void ChangeFpsSettings(EventBase obj)
    {
        SetMenuDir(obj);
        ChangeSetting(toggleNames, fpsLableDisplaySetting, fpsIndicators, ref settingsValues.fpsCurrentSettingIndex, false);
    }
    #endregion

    void ChangeSetting(string[] settingNames, Label currentSettingLabel, VisualElement[] indicators, ref int settingIndex, bool isCustomChange = true)
    {
        settingIndex += menuMoveDir;
        settingIndex = Mathf.Clamp(settingIndex, 0, indicators.Length - 1);
        
        PushUpdates(isCustomChange);
    }
    

    void PushUpdates(bool isCustomChange = true)
    {
        if (isCustomChange)
        {
            Debug.Log("forcing custom quality");
            settingsValues.presetsCurrentSettingIndex = presetNames.Length - 1;
        }

        WriteChangedSettings?.Invoke(caps, settingsValues);

        if (ReadCurrentSettings != null)
            settingsValues = ReadCurrentSettings(caps);
    
        UpdateAllSettings();
    }

    void UpdateSettingIndicators(int activeIndex, Label label,  VisualElement[] indicators, string[] names)
    {
        label.text = names[activeIndex].ToUpperInvariant();

        foreach (var ve in indicators)
            ve.RemoveFromClassList("info-setting-indicator-active");

        indicators[activeIndex].AddToClassList("info-setting-indicator-active");
    }

    void UpdateUI()
    {
        //Check the Upscale Mode
        int upscaleMode = settingsValues.upscaleModeCurrentSettingIndex;
        if (upscaleMode == 1 /*|| upscaleMode == 2*/)
        {
            resolutonScaleSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            dlssSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            
            dlssSettings.style.display = DisplayStyle.None;
            resolutonScaleSettings.style.display = DisplayStyle.Flex;
        }
        else if(upscaleMode == 2)
        {
            resolutonScaleSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            dlssSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);

            resolutonScaleSettings.style.display = DisplayStyle.None;
            dlssSettings.style.display = DisplayStyle.Flex;
        }
        else
        {
            resolutonScaleSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
            dlssSettings.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        }
    }
    
    void UpdateAllSettings()
    {
        UpdateSettingIndicators(settingsValues.presetsCurrentSettingIndex, presetsLableDisplaySetting, presetsIndicators, presetNames);
        UpdateSettingIndicators(settingsValues.displayModeCurrentSettingIndex, displayModeLableDisplaySetting, displayModeIndicators, displayModeNames);
        UpdateSettingIndicators(settingsValues.resolutionsCurrentSettingsIndex, resolutionsLableDisplaySetting, resolutionsIndicators, resolutionsNamesArray);
        UpdateSettingIndicators(settingsValues.dlssCurrentSettingIndex, dlssLableDisplaySetting, dlssIndicators, dlssSettingsNames);
        UpdateSettingIndicators(settingsValues.shadowsCurrentSettingIndex, shadowsLableDisplaySetting, shadowsIndicators, qualitySettingsNames);
        UpdateSettingIndicators(settingsValues.shadowsRtCurrentSettingIndex, shadowsRtLableDisplaySetting, shadowsRtIndicators, raytraceNames);
        UpdateSettingIndicators(settingsValues.aoCurrentSettingIndex, aoLableDisplaySetting, aoIndicators, qualitySettingsNamesOff);
        UpdateSettingIndicators(settingsValues.aoRtCurrentSettingIndex, aoRtLableDisplaySetting, aoRtIndicators, raytraceNames);
        UpdateSettingIndicators(settingsValues.reflectionsCurrentSettingIndex, reflectionsLableDisplaySetting, reflectionsIndicators, qualitySettingsNamesOff);
        UpdateSettingIndicators(settingsValues.reflectionsRtCurrentSettingIndex, reflectionsRtLableDisplaySetting, reflectionsRtIndicators, raytraceNames);
        UpdateSettingIndicators(settingsValues.hairCurrentSettingIndex, hairLableDisplaySetting, hairIndicators, qualitySettingsNames);
        UpdateSettingIndicators(settingsValues.lightingQualityCurrentSettingIndex, lightingQualityLableDisplaySetting, lightingQualityIndicators, qualitySettingsNames);
        UpdateSettingIndicators(settingsValues.materialQualityCurrentSettingIndex, materialQualityLableDisplaySetting, materialQualityIndicators, qualitySettingsNames);
        UpdateSettingIndicators(settingsValues.texturesSizeCurrentSettingIndex, texturesSizeLableDisplaySetting, texturesSizeIndicators, textureSettingsNames);
        UpdateSettingIndicators(settingsValues.upscaleModeCurrentSettingIndex, upscaleModeLableDisplaySetting, upscaleModeIndicators, upscaleModeNames);
        UpdateSettingIndicators(settingsValues.vsyncCurrentSettingIndex, vsyncLableDisplaySetting, vsyncIndicators, vsyncNames);
        UpdateSettingIndicators(settingsValues.postprocessingCurrentSettingIndex, postprocessingLableDisplaySetting, postprocessingIndicators, qualitySettingsNames);
        UpdateSettingIndicators(settingsValues.fpsCurrentSettingIndex, fpsLableDisplaySetting, fpsIndicators, toggleNames);
        ChangeResolutionScaleValue(settingsValues.resolutionScale);
        UpdateUI();
    }
}
