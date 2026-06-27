using UnityEngine;

public enum FireChampionNetworkWaitingAction
{
    None,
    Cancel
}

public enum FireChampionPauseAction
{
    None,
    Resume,
    RestartMatch,
    MainMenu
}

public enum FireChampionSummaryAction
{
    None,
    NextTournamentMatch,
    RestartTournament,
    NetworkMenu,
    Rematch,
    MainMenu
}

public struct FireChampionSummaryRenderContext
{
    public Texture2D pixel;
    public FireChampionPlayingOverlayLayout layout;
    public string summaryTitle;
    public string summaryDetails;
    public MatchStats stats;
    public GameMode mode;
    public bool lastMatchPlayerWon;
    public bool tournamentComplete;
}

public static class FireChampionOverlayRenderer
{
    private static readonly Color Panel = new Color(0.02f, 0.02f, 0.025f, 0.94f);
    private static readonly Color SummaryPanel = new Color(0.02f, 0.02f, 0.025f, 0.95f);
    private static readonly Color BannerPanel = new Color(0f, 0f, 0f, 0.74f);
    private static readonly Color SummaryBackdrop = new Color(0f, 0f, 0f, 0.62f);
    private static readonly Color SummaryWinTint = new Color(0.12f, 0.22f, 0.12f, 0.20f);
    private static readonly Color SummaryLossTint = new Color(0.22f, 0.09f, 0.08f, 0.20f);

    public static void DrawBanner(Texture2D pixel, FireChampionPlayingOverlayLayout layout, string banner)
    {
        DrawPanel(pixel, layout.bannerPanel, BannerPanel);
        GUI.Label(layout.bannerLabel, "<color=white><b>" + SafeText(banner, "") + "</b></color>");
    }

    public static void DrawSummaryBackdrop(Texture2D pixel, bool playerWon)
    {
        Rect fullScreen = new Rect(0, 0, Screen.width, Screen.height);
        FireChampionGuiDrawing.DrawRect(pixel, fullScreen, SummaryBackdrop);
        FireChampionGuiDrawing.DrawRect(pixel, fullScreen, playerWon ? SummaryWinTint : SummaryLossTint);

        float bandHeight = Mathf.Clamp(Screen.height * 0.16f, 72.0f, 150.0f);
        FireChampionGuiDrawing.DrawRect(pixel, new Rect(0, 0, Screen.width, bandHeight), new Color(0f, 0f, 0f, 0.22f));
        FireChampionGuiDrawing.DrawRect(pixel, new Rect(0, Screen.height - bandHeight, Screen.width, bandHeight), new Color(0f, 0f, 0f, 0.26f));
    }

    public static FireChampionNetworkWaitingAction DrawNetworkWaiting(Texture2D pixel, FireChampionPlayingOverlayLayout layout, string status, string diagnostics)
    {
        DrawPanel(pixel, layout.networkWaitingPanel, Panel);
        GUILayout.BeginArea(layout.networkWaitingContent);
        GUILayout.Label("<size=22><b>等待联机连接</b></size>");
        GUILayout.Label(SafeText(status, "等待连接..."));
        GUILayout.Label("<size=13>" + SafeText(diagnostics, "") + "</size>");
        FireChampionNetworkWaitingAction action = GUILayout.Button("取消并返回联机菜单")
            ? FireChampionNetworkWaitingAction.Cancel
            : FireChampionNetworkWaitingAction.None;
        GUILayout.EndArea();
        return action;
    }

    public static FireChampionPauseAction DrawPause(Texture2D pixel, FireChampionPlayingOverlayLayout layout)
    {
        DrawPanel(pixel, layout.pausePanel, Panel);
        GUILayout.BeginArea(layout.pauseContent);
        GUILayout.Label("<size=26><b>暂停</b></size>");
        FireChampionPauseAction action = FireChampionPauseAction.None;
        if (GUILayout.Button("继续"))
        {
            action = FireChampionPauseAction.Resume;
        }
        else if (GUILayout.Button("重新开始本场"))
        {
            action = FireChampionPauseAction.RestartMatch;
        }
        else if (GUILayout.Button("返回主菜单"))
        {
            action = FireChampionPauseAction.MainMenu;
        }

        GUILayout.EndArea();
        return action;
    }

    public static FireChampionSummaryAction DrawSummary(FireChampionSummaryRenderContext context)
    {
        DrawPanel(context.pixel, context.layout.summaryPanel, SummaryPanel);
        GUILayout.BeginArea(context.layout.summaryContent);
        GUILayout.Label("<size=28><b>" + SafeText(context.summaryTitle, "比赛结束") + "</b></size>");
        GUILayout.Label(SafeText(context.summaryDetails, ""));
        GUILayout.Space(8);
        GUILayout.Label("最长回合: " + context.stats.longestRally + " 拍");
        GUILayout.Label("扣杀得分: " + context.stats.smashWinners + " · 失误: " + context.stats.errors);
        GUILayout.Label("技能使用: " + context.stats.skillsUsed + " · 发球犯规: " + context.stats.serveFaults);
        GUILayout.Space(10);

        FireChampionSummaryAction action = DrawSummaryPrimaryAction(context);
        if (GUILayout.Button("返回主菜单", GUILayout.Height(34)))
        {
            action = FireChampionSummaryAction.MainMenu;
        }

        GUILayout.EndArea();
        return action;
    }

    public static FireChampionSummaryAction PrimarySummaryActionForState(GameMode mode, bool lastMatchPlayerWon, bool tournamentComplete)
    {
        if (mode == GameMode.Tournament && lastMatchPlayerWon && !tournamentComplete)
        {
            return FireChampionSummaryAction.NextTournamentMatch;
        }

        if (mode == GameMode.Tournament)
        {
            return FireChampionSummaryAction.RestartTournament;
        }

        if (mode == GameMode.NetworkClient)
        {
            return FireChampionSummaryAction.NetworkMenu;
        }

        return FireChampionSummaryAction.Rematch;
    }

    public static string PrimarySummaryButtonLabel(FireChampionSummaryAction action)
    {
        if (action == FireChampionSummaryAction.NextTournamentMatch)
        {
            return "下一场";
        }

        if (action == FireChampionSummaryAction.RestartTournament)
        {
            return "重新开始锦标赛";
        }

        if (action == FireChampionSummaryAction.NetworkMenu)
        {
            return "返回网络菜单";
        }

        if (action == FireChampionSummaryAction.Rematch)
        {
            return "立即重赛";
        }

        return "";
    }

    private static FireChampionSummaryAction DrawSummaryPrimaryAction(FireChampionSummaryRenderContext context)
    {
        FireChampionSummaryAction action = PrimarySummaryActionForState(context.mode, context.lastMatchPlayerWon, context.tournamentComplete);
        return GUILayout.Button(PrimarySummaryButtonLabel(action), GUILayout.Height(36))
            ? action
            : FireChampionSummaryAction.None;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    private static void DrawPanel(Texture2D pixel, Rect rect, Color color)
    {
        FireChampionGuiDrawing.DrawPanel(pixel, rect.x, rect.y, rect.width, rect.height, color);
    }
}
