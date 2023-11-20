using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(ReadMe))]
[InitializeOnLoad]
public class ReadMeWindow : Editor
{
    static string kShowedReadmeSessionStateName = "ReadMeWindow.showedReadme-v1";

    static float kSpace = 16f;

    static ReadMeWindow()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(kShowedReadmeSessionStateName, true);

            // if (readme && !readme.loadedLayout)
            // {
            //     LoadLayout();
            //     readme.loadedLayout = true;
            // }
        }
    }

    static void LoadLayout()
    {
        var assembly = typeof(EditorApplication).Assembly;
        var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
    }

    static ReadMe SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("ReadMe t:ReadMe");
        if (ids.Length == 1)
        {
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
            Selection.objects = new [] { readmeObject };
            return (ReadMe)readmeObject;
        }

        Debug.Log("Couldn't find a readme");
        return null;
    }
    
    class ParsedReadme
    {
        public class Section
        {
            public string heading;
            public string text;
            public string linkText;
            public string url;
            public int textLines;
        }
        
        public readonly Texture2D icon;
        public readonly string title;
        public readonly Section[] sections;
        
        public ParsedReadme(ReadMe readme)
        {
            icon = readme.thumb;

            var lines = readme.readme.text.Split(new []{"\n", "\r", "\r\n"}, StringSplitOptions.None);

            var sections = new List<Section>();
            var currentSection = default(Section);
            for (var i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];

                if (i == 0 && line.StartsWith("#"))
                {
                    title = line.Replace("#", "").Trim();
                }
                else
                {
                    if (line.StartsWith("##"))
                    {
                        currentSection = new Section { heading = line.Replace("#", "").Trim(), text = string.Empty };
                        sections.Add(currentSection);
                    }
                    else if (currentSection != null)
                    {
                        if (currentSection.textLines > 0 || !string.IsNullOrEmpty(line))
                        {
                            currentSection.text += line + "\n";
                            currentSection.textLines++;
                        }
                    }
                }
            }

            this.sections = sections.ToArray();
        }
    }

    new ParsedReadme target => new ParsedReadme((ReadMe)base.target);

    public override void OnInspectorGUI()
    {
        Init();
        
        var readme = target;

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, HeadingStyle);
            }
            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, BodyStyle);
            }
            if (!string.IsNullOrEmpty(section.linkText))
            {
                if (LinkLabel(new GUIContent(section.linkText)))
                {
                    Application.OpenURL(section.url);
                }
            }
            GUILayout.Space(kSpace);
        }
    }

    protected override void OnHeaderGUI()
    {
        Init();
        
        var readme = target;

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            GUILayout.Label(readme.title, TitleStyle);
        }
        GUILayout.EndHorizontal();
    }

    bool m_Initialized;

    GUIStyle LinkStyle { get { return m_LinkStyle; } }
    [SerializeField] GUIStyle m_LinkStyle;

    GUIStyle TitleStyle { get { return m_TitleStyle; } }
    [SerializeField] GUIStyle m_TitleStyle;

    GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
    [SerializeField] GUIStyle m_HeadingStyle;

    GUIStyle BodyStyle { get { return m_BodyStyle; } }
    [SerializeField] GUIStyle m_BodyStyle;

    void Init()
    {
        if (m_Initialized)
            return;
        
        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;
        m_BodyStyle.margin = new RectOffset(m_BodyStyle.margin.left, m_BodyStyle.margin.right, m_BodyStyle.margin.top, m_BodyStyle.margin.bottom / 4); 

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;

        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_Initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, LinkStyle);
    }
}
