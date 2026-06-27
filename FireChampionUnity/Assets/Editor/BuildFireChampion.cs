#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class BuildFireChampion
{
    [MenuItem("Fire Champion/Build Windows EXE")]
    public static void BuildWindowsExe()
    {
        const string scene = "Assets/Scenes/FireChampion.unity";
        string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "outputs", "FireChampionWindows"));
        string outputExe = Path.Combine(outputDir, "FireChampion.exe");

        FireChampionProjectValidator.ValidateOrThrow();
        ApplySafeWindowsPlayerSettings();
        Directory.CreateDirectory(outputDir);
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new[] { scene };
        options.locationPathName = outputExe;
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildReportGuard.Log(report.summary.result.ToString(), outputExe);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception("Fire Champion Windows build failed: " + report.summary.result);
        }
    }

    private static void ApplySafeWindowsPlayerSettings()
    {
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.useFlipModelSwapchain = false;
        PlayerSettings.usePlayerLog = false;
    }
}

public static class BuildReportGuard
{
    public static void Log(string result, string path)
    {
        UnityEngine.Debug.Log("Fire Champion Windows build result: " + result + " -> " + path);
    }
}
#endif
