using System;

public enum FireChampionQaScreen
{
    None,
    Settings,
    Network,
    Practice,
    Tutorial,
    Tournament,
    Pause,
    Summary,
    NetworkWaiting,
    VfxPreview,
    QuickMatch,
    AutoMatch
}

public enum FireChampionQaSummaryVariant
{
    Win,
    Loss,
    TournamentFinalWin,
    NetworkClient
}

public static class FireChampionQaLaunch
{
    public const string ArgumentName = "-firechampion-qa-screen";
    public const string CourtArgumentName = "-firechampion-qa-court";
    public const string SummaryArgumentName = "-firechampion-qa-summary";

    public static FireChampionQaScreen FromCommandLine()
    {
        return Parse(Environment.GetCommandLineArgs());
    }

    public static FireChampionQaScreen Parse(string[] args)
    {
        if (args == null)
        {
            return FireChampionQaScreen.None;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string current = Normalize(args[i]);
            if (current == "firechampionqascreen" || current == "firechampion-qa-screen")
            {
                return i + 1 < args.Length ? ParseValue(args[i + 1]) : FireChampionQaScreen.None;
            }

            const string prefix = "firechampion-qa-screen=";
            if (current.StartsWith(prefix, StringComparison.Ordinal))
            {
                return ParseValue(current.Substring(prefix.Length));
            }
        }

        return FireChampionQaScreen.None;
    }

    public static int CourtIndexFromCommandLine()
    {
        return ParseCourtIndex(Environment.GetCommandLineArgs());
    }

    public static FireChampionQaSummaryVariant SummaryVariantFromCommandLine()
    {
        return ParseSummaryVariant(Environment.GetCommandLineArgs());
    }

    public static int ParseCourtIndex(string[] args)
    {
        if (args == null)
        {
            return -1;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string current = Normalize(args[i]);
            if (current == "firechampionqacourt" || current == "firechampion-qa-court")
            {
                return i + 1 < args.Length ? ParseCourtValue(args[i + 1]) : -1;
            }

            const string prefix = "firechampion-qa-court=";
            if (current.StartsWith(prefix, StringComparison.Ordinal))
            {
                return ParseCourtValue(current.Substring(prefix.Length));
            }
        }

        return -1;
    }

    public static FireChampionQaSummaryVariant ParseSummaryVariant(string[] args)
    {
        if (args == null)
        {
            return FireChampionQaSummaryVariant.Win;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string current = Normalize(args[i]);
            if (current == "firechampionqasummary" || current == "firechampion-qa-summary")
            {
                return i + 1 < args.Length ? ParseSummaryValue(args[i + 1]) : FireChampionQaSummaryVariant.Win;
            }

            const string prefix = "firechampion-qa-summary=";
            if (current.StartsWith(prefix, StringComparison.Ordinal))
            {
                return ParseSummaryValue(current.Substring(prefix.Length));
            }
        }

        return FireChampionQaSummaryVariant.Win;
    }

    public static FireChampionQaScreen ParseValue(string value)
    {
        string normalized = Normalize(value);
        if (normalized == "settings") return FireChampionQaScreen.Settings;
        if (normalized == "network") return FireChampionQaScreen.Network;
        if (normalized == "practice") return FireChampionQaScreen.Practice;
        if (normalized == "tutorial") return FireChampionQaScreen.Tutorial;
        if (normalized == "tournament") return FireChampionQaScreen.Tournament;
        if (normalized == "pause") return FireChampionQaScreen.Pause;
        if (normalized == "summary") return FireChampionQaScreen.Summary;
        if (normalized == "networkwaiting" || normalized == "network-waiting") return FireChampionQaScreen.NetworkWaiting;
        if (normalized == "vfx" || normalized == "vfxpreview" || normalized == "vfx-preview") return FireChampionQaScreen.VfxPreview;
        if (normalized == "quick" || normalized == "quickai" || normalized == "quick-ai" || normalized == "quick-match") return FireChampionQaScreen.QuickMatch;
        if (normalized == "autoplay" || normalized == "auto-play" || normalized == "automatch" || normalized == "auto-match" || normalized == "long-run") return FireChampionQaScreen.AutoMatch;
        return FireChampionQaScreen.None;
    }

    private static int ParseCourtValue(string value)
    {
        string normalized = Normalize(value);
        if (normalized == "dojo") return 0;
        if (normalized == "rooftop" || normalized == "roof") return 1;
        if (normalized == "future" || normalized == "futurecourt" || normalized == "future-court") return 2;

        int index;
        if (int.TryParse(normalized, out index))
        {
            return index;
        }

        return -1;
    }

    private static FireChampionQaSummaryVariant ParseSummaryValue(string value)
    {
        string normalized = Normalize(value);
        if (normalized == "loss" || normalized == "lose" || normalized == "defeat") return FireChampionQaSummaryVariant.Loss;
        if (normalized == "tournament" || normalized == "tournamentfinal" || normalized == "tournament-final" || normalized == "final") return FireChampionQaSummaryVariant.TournamentFinalWin;
        if (normalized == "network" || normalized == "networkclient" || normalized == "network-client" || normalized == "client") return FireChampionQaSummaryVariant.NetworkClient;
        return FireChampionQaSummaryVariant.Win;
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
