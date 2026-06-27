using UnityEngine;

public struct FireChampionHudRenderContext
{
    public Texture2D pixel;
    public Texture2D discTexture;
    public BalanceGameplay gameplay;
    public CharacterConfig[] roster;
    public CourtConfig[] courts;
    public RulesetConfig rules;
    public PlayerActor leftPlayer;
    public PlayerActor rightPlayer;
    public GameMode mode;
    public int selectedCourt;
    public int leftScore;
    public int rightScore;
    public int leftGames;
    public int rightGames;
    public float courtPhase;
    public bool highContrast;
    public string leftDisplayName;
    public string rightDisplayName;
    public string modeName;
    public string tournamentSuffix;
    public string networkStatus;
    public Color leftAccent;
    public Color rightAccent;
}

public static class FireChampionHudRenderer
{
    public static void DrawHud(FireChampionHudRenderContext context)
    {
        Color panel = context.highContrast ? new Color(0, 0, 0, 0.92f) : new Color(0.015f, 0.017f, 0.022f, 0.76f);
        float boardX = Screen.width * 0.5f - 280;
        Color courtAccent = SafeCourtAccent(context);
        DrawRect(context, new Rect(boardX + 8, 20, 560, 78), new Color(0, 0, 0, 0.24f));
        DrawPanel(context, boardX, 14, 560, 78, panel);
        DrawRectOutline(context, new Rect(boardX, 14, 560, 78), 2, new Color(courtAccent.r, courtAccent.g, courtAccent.b, 0.46f));
        DrawRect(context, new Rect(boardX, 14, 116, 4), context.leftAccent);
        DrawRect(context, new Rect(boardX + 444, 14, 116, 4), context.rightAccent);

        string setText = context.rules.bestOf > 1 ? " 局 " + context.leftGames + ":" + context.rightGames : "";
        GUI.Label(new Rect(boardX + 22, 29, 170, 23), "<b>" + SafeText(context.leftDisplayName, "P1") + "</b>  " + SafeRosterCode(context, context.leftPlayer));
        GUI.Label(new Rect(boardX + 368, 29, 170, 23), "<b>" + SafeText(context.rightDisplayName, "P2") + "</b>  " + SafeRosterCode(context, context.rightPlayer));
        GUI.Label(new Rect(boardX + 206, 23, 150, 35), "<size=29><b>" + context.leftScore + "  :  " + context.rightScore + "</b></size>");
        GUI.Label(new Rect(boardX + 185, 59, 330, 22), SafeText(context.modeName, "比赛") + SafeText(context.tournamentSuffix, "") + " · " + SafeCourtName(context) + setText + " · ESC 暂停");

        bool narrowHud = Screen.width < 1120.0f;
        float energyY = narrowHud ? 102.0f : 32.0f;
        DrawEnergyBar(context, 38, energyY, context.leftPlayer, context.leftAccent);
        DrawEnergyBar(context, Screen.width - 246, energyY, context.rightPlayer, context.rightAccent);

        if (!narrowHud && (context.mode == GameMode.NetworkHost || context.mode == GameMode.NetworkClient))
        {
            GUI.Label(new Rect(Screen.width - 350, 10, 330, 24), "网络: " + SafeText(context.networkStatus, "未连接"));
        }
    }

    public static void DrawEnergyBar(FireChampionHudRenderContext context, float x, float y, PlayerActor player, Color accent)
    {
        Color panel = context.highContrast ? new Color(0, 0, 0, 0.88f) : new Color(0.015f, 0.017f, 0.022f, 0.66f);
        DrawRect(context, new Rect(x + 4, y + 5, 208, 46), new Color(0, 0, 0, 0.22f));
        DrawPanel(context, x, y, 208, 46, panel);
        GUI.Label(new Rect(x + 10, y + 3, 190, 18), SafeRosterCode(context, player) + " 能量");

        int segmentCount = Mathf.Max(1, Mathf.RoundToInt(context.gameplay.energy.maxEnergy));
        float segmentGap = 8.0f;
        float segmentWidth = (178.0f - segmentGap * (segmentCount - 1)) / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            Rect segment = new Rect(x + 10 + i * (segmentWidth + segmentGap), y + 24, segmentWidth, 12);
            DrawCapsule(context, new Vector2(segment.x + 6, segment.center.y), new Vector2(segment.xMax - 6, segment.center.y), 12, new Color(1, 1, 1, 0.18f));
            float fill = Mathf.Clamp01(player.energy - i);
            if (fill > 0)
            {
                float fillEnd = Mathf.Lerp(segment.x + 6, segment.xMax - 6, fill);
                DrawCapsule(context, new Vector2(segment.x + 6, segment.center.y), new Vector2(fillEnd, segment.center.y), 12, accent);
            }
        }

        if (player.energy >= context.gameplay.energy.skillCost)
        {
            DrawRectOutline(context, new Rect(x + 8, y + 22, 58, 16), 1.5f, new Color(accent.r, accent.g, accent.b, 0.55f));
        }

        if (player.energy >= context.gameplay.energy.ultimateCost)
        {
            float pulse = 0.55f + Mathf.Sin(context.courtPhase * 8.0f) * 0.25f;
            DrawRectOutline(context, new Rect(x + 7, y + 21, 190, 18), 2, new Color(accent.r, accent.g, accent.b, pulse));
            DrawEllipse(context, new Vector2(x + 198, y + 30), 12, 12, new Color(accent.r, accent.g, accent.b, 0.34f));
        }
    }

    private static string SafeRosterCode(FireChampionHudRenderContext context, PlayerActor player)
    {
        if (context.roster == null || context.roster.Length == 0)
        {
            return "ROLE";
        }

        int index = Mathf.Clamp(player.characterIndex, 0, context.roster.Length - 1);
        return SafeText(context.roster[index].code, "ROLE");
    }

    private static string SafeCourtName(FireChampionHudRenderContext context)
    {
        if (context.courts == null || context.courts.Length == 0)
        {
            return "球场";
        }

        int index = Mathf.Clamp(context.selectedCourt, 0, context.courts.Length - 1);
        return SafeText(context.courts[index].name, "球场");
    }

    private static Color SafeCourtAccent(FireChampionHudRenderContext context)
    {
        if (context.courts == null || context.courts.Length == 0)
        {
            return Color.white;
        }

        int index = Mathf.Clamp(context.selectedCourt, 0, context.courts.Length - 1);
        return context.courts[index].accent;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    private static void DrawPanel(FireChampionHudRenderContext context, float x, float y, float w, float h, Color color)
    {
        FireChampionGuiDrawing.DrawPanel(context.pixel, x, y, w, h, color);
    }

    private static void DrawRect(FireChampionHudRenderContext context, Rect rect, Color color)
    {
        FireChampionGuiDrawing.DrawRect(context.pixel, rect, color);
    }

    private static void DrawEllipse(FireChampionHudRenderContext context, Vector2 center, float radiusX, float radiusY, Color color)
    {
        FireChampionGuiDrawing.DrawEllipse(context.discTexture, center, radiusX, radiusY, color);
    }

    private static void DrawCapsule(FireChampionHudRenderContext context, Vector2 a, Vector2 b, float width, Color color)
    {
        FireChampionGuiDrawing.DrawCapsule(context.pixel, context.discTexture, a, b, width, color);
    }

    private static void DrawRectOutline(FireChampionHudRenderContext context, Rect rect, float width, Color color)
    {
        FireChampionGuiDrawing.DrawRectOutline(context.pixel, rect, width, color);
    }
}
