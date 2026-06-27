using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

public sealed class FireChampionQaCaptureConfig
{
    public string outputPath = "";
    public float delaySeconds = 1.25f;
    public bool quitAfterCapture;

    public bool IsRequested
    {
        get { return !string.IsNullOrEmpty(outputPath); }
    }
}

public static class FireChampionQaCapture
{
    public const string CaptureArgumentName = "-firechampion-qa-capture";
    public const string DelayArgumentName = "-firechampion-qa-capture-delay";
    public const string QuitAfterCaptureArgumentName = "-firechampion-qa-exit-after-capture";

    public static FireChampionQaCaptureConfig FromCommandLine()
    {
        return Parse(Environment.GetCommandLineArgs());
    }

    public static FireChampionQaCaptureConfig Parse(string[] args)
    {
        FireChampionQaCaptureConfig config = new FireChampionQaCaptureConfig();
        if (args == null)
        {
            return config;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string raw = args[i] ?? "";
            string current = Normalize(raw);
            if (current == "firechampionqacapture" || current == "firechampion-qa-capture")
            {
                config.outputPath = i + 1 < args.Length ? NormalizePath(args[i + 1]) : "";
                continue;
            }

            const string capturePrefix = "firechampion-qa-capture=";
            if (current.StartsWith(capturePrefix, StringComparison.Ordinal))
            {
                config.outputPath = NormalizePath(raw.Substring(raw.IndexOf('=') + 1));
                continue;
            }

            if (current == "firechampionqacapturedelay" || current == "firechampion-qa-capture-delay")
            {
                if (i + 1 < args.Length)
                {
                    config.delaySeconds = ParseDelay(args[i + 1], config.delaySeconds);
                }
                continue;
            }

            const string delayPrefix = "firechampion-qa-capture-delay=";
            if (current.StartsWith(delayPrefix, StringComparison.Ordinal))
            {
                config.delaySeconds = ParseDelay(raw.Substring(raw.IndexOf('=') + 1), config.delaySeconds);
                continue;
            }

            if (current == "firechampionqaexitaftercapture" || current == "firechampion-qa-exit-after-capture")
            {
                config.quitAfterCapture = true;
            }
        }

        return config;
    }

    public static IEnumerator CaptureAndMaybeQuit(FireChampionQaCaptureConfig config)
    {
        if (config == null || !config.IsRequested)
        {
            yield break;
        }

        string outputPath = config.outputPath;
        string directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0.0f, config.delaySeconds));
        yield return new WaitForEndOfFrame();

        ScreenCapture.CaptureScreenshot(outputPath);
        Debug.Log("Fire Champion QA screenshot requested: " + outputPath);

        if (!config.quitAfterCapture)
        {
            yield break;
        }

        float timeout = 3.0f;
        while (timeout > 0.0f && !File.Exists(outputPath))
        {
            timeout -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(0.35f);
        Application.Quit();
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        return Path.GetFullPath(path.Trim().Trim('"'));
    }

    private static float ParseDelay(string value, float fallback)
    {
        float parsed;
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            return Mathf.Clamp(parsed, 0.0f, 30.0f);
        }

        return fallback;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value.Trim().TrimStart('-', '/').ToLowerInvariant();
    }
}
