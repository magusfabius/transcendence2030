using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class SettingsBuildPostProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    static readonly string sDesktopConfigFolder = "Config";
    static readonly string[] sStandalonePCFiles = {"README-Enemies.txt", "Enemies-Ultra-4K.bat", "Enemies-High-4K.bat", "Enemies-Medium-QHD.bat", "Enemies-Low-FHD.bat"};
    static readonly string[] sStandaloneOSXFiles = { "README-Enemies.txt", "Enemies-Ultra-4K.command", "Enemies-High-4K.command", "Enemies-Medium-QHD.command", "Enemies-Low-FHD.command" };
    static readonly string[] sXboxSeriesFiles = {"TargetXboxSeriesS.json", "TargetXboxSeriesX.json"};
    static readonly string[] sXboxSeriesDevFiles = {};
    static readonly string[] sPlaystationFiles = {"TargetPlaystation5.json"};
    static readonly string[] sPlaystationDevFiles = {};

    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report)
    {
        // Nothing to do in playmode
        if (report == null) return;

        var pathToBuiltProject = report.summary.outputPath;
        var target = report.summary.platform;

        // PS5 needs to delete extra files or incremental builds fail
        if (target != BuildTarget.PS5)
            return;

        Debug.Log($"PRE-PROCESS pathToBuiltProject: {pathToBuiltProject}");
        
        var buildFolder = pathToBuiltProject;
        buildFolder += "/Build";
        
        var buildFilePaths = System.IO.Directory.GetFiles(buildFolder, "*");
        foreach (var buildFilePath in buildFilePaths)
        {
            var buildFilename = System.IO.Path.GetFileName(buildFilePath);
            if (!sPlaystationFiles.Contains(buildFilename))
                continue;

            Debug.Log($"Deleting {buildFilePath}");
            System.IO.File.Delete(buildFilePath);
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        // Nothing to do in playmode
        if (report == null) return;
        
        var pathToBuiltProject = report.summary.outputPath;
        var target = report.summary.platform;
        var isDevelopment = (report.summary.options & BuildOptions.Development) != BuildOptions.None;
        
        if (target == BuildTarget.StandaloneWindows)
        {
            Debug.LogWarning("Warning! 32-bit Windows builds are not supported.");
        }

        if (target == BuildTarget.StandaloneOSX || target == BuildTarget.StandaloneLinux64)
        {
            Debug.LogWarning($"Warning, build target {target} is not officially supported or tested.");
        }

        Debug.Log($"POST-PROCESS pathToBuiltProject: {pathToBuiltProject}");
        
        if (target != BuildTarget.StandaloneWindows64 && target != BuildTarget.PS5 && target != BuildTarget.GameCoreXboxSeries && target != BuildTarget.StandaloneOSX)
            return;
        
        var buildFolder = pathToBuiltProject;
        if(target == BuildTarget.StandaloneWindows64) buildFolder = System.IO.Path.GetDirectoryName(buildFolder);
        if(target == BuildTarget.StandaloneOSX) buildFolder = System.IO.Path.GetDirectoryName(buildFolder);
        if(target == BuildTarget.GameCoreXboxSeries) buildFolder += "/Loose";
        if(target == BuildTarget.PS5) buildFolder += "/Build";

        var scriptsFolder = System.IO.Path.Combine(Application.dataPath, "..", "Assets", "Meta", "PlayerScripts");
        Debug.Log($"Copy {scriptsFolder} -> {buildFolder}");

        var scriptFilePaths = System.IO.Directory.GetFiles(scriptsFolder, "*");
        foreach (var scriptFilePath in scriptFilePaths)
        {
            var scriptFile = scriptFilePath.Substring(scriptsFolder.Length + 1);

            if (target == BuildTarget.StandaloneWindows64 && !sStandalonePCFiles.Contains(scriptFile))
            {
                Debug.Log($"Skipping {scriptFilePath}; not needed in standalone pc build.");
                continue;
            }
            
            if (target == BuildTarget.StandaloneOSX && !sStandaloneOSXFiles.Contains(scriptFile))
            {
                Debug.Log($"Skipping {scriptFilePath}; not needed in standalone osx build.");
                continue;
            }

            if (target == BuildTarget.GameCoreXboxSeries && !sXboxSeriesFiles.Contains(scriptFile) && (!isDevelopment || !sXboxSeriesDevFiles.Contains(scriptFile)))
            {
                Debug.Log($"Skipping {scriptFilePath}; not needed in xbox build.");
                continue;
            }
            if (target == BuildTarget.PS5 && !sPlaystationFiles.Contains(scriptFile) && (!isDevelopment || !sPlaystationDevFiles.Contains(scriptFile)))
            {
                Debug.Log($"Skipping {scriptFilePath}; not needed in playstation build.");
                continue;
            }

            CopyToWritableFile(scriptFilePath, System.IO.Path.Combine(buildFolder, scriptFile));
        }
        
#if PLATFORM_STANDALONE
        // Copy config files only for desktop platforms (and currently only Windows 64-bit). Consoles currently run fixes configs.
        buildFolder = System.IO.Path.Combine(buildFolder, sDesktopConfigFolder);
        scriptsFolder = System.IO.Path.Combine(scriptsFolder, sDesktopConfigFolder);
        
        System.IO.Directory.CreateDirectory(buildFolder);
        
        var configFilePaths = System.IO.Directory.GetFiles(scriptsFolder, "*");
        foreach (var configFilePath in configFilePaths)
        {
            var configFile = configFilePath.Substring(scriptsFolder.Length + 1);
            CopyToWritableFile(configFilePath, System.IO.Path.Combine(buildFolder, configFile));
        }
#endif
    }
        
    static void CopyToWritableFile(string from, string to)
    {
        Debug.Log($"Copying {from} -> {to}");
            
        System.IO.File.Copy(from, to, true);

        // Make target file writable after copy (if it originate from Perforce, e.g., it's likely to be read-only)
        var attrs = System.IO.File.GetAttributes(to);
        attrs &= ~System.IO.FileAttributes.ReadOnly;
        System.IO.File.SetAttributes(to, attrs);
    }
}
