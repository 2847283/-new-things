using UnityEngine;

public struct FireChampionMenuLayout
{
    public Rect mainPanel;
    public Rect mainContent;
    public Rect infoPanel;
    public Rect infoContent;
    public bool showInfoCard;
}

public struct FireChampionPlayingOverlayLayout
{
    public Rect bannerPanel;
    public Rect bannerLabel;
    public Rect practicePanel;
    public Rect practiceContent;
    public Rect tutorialPanel;
    public Rect tutorialContent;
    public Rect tournamentPanel;
    public Rect tournamentContent;
    public Rect networkDiagnosticsPanel;
    public Rect networkDiagnosticsContent;
    public Rect networkWaitingPanel;
    public Rect networkWaitingContent;
    public Rect pausePanel;
    public Rect pauseContent;
    public Rect summaryPanel;
    public Rect summaryContent;
}

public sealed class FireChampionLayoutAuditReport
{
    public int resolutionCount;
    public int failures;
    public int infoHiddenCount;
    public string firstFailure;

    public string Summary()
    {
        return "resolutions=" + resolutionCount
            + ", failures=" + failures
            + ", infoHidden=" + infoHiddenCount
            + (string.IsNullOrEmpty(firstFailure) ? "" : ", firstFailure=" + firstFailure);
    }

    public void Fail(string message)
    {
        failures++;
        if (string.IsNullOrEmpty(firstFailure))
        {
            firstFailure = message;
        }
    }
}

public static class FireChampionLayout
{
    private const float OuterMargin = 34.0f;
    private const float MainPanelTop = 28.0f;
    private const float MainPanelPreferredWidth = 380.0f;
    private const float MainPanelMinWidth = 300.0f;
    private const float MainContentInsetX = 24.0f;
    private const float MainContentTopInset = 26.0f;
    private const float MainContentBottomInset = 44.0f;
    private const float InfoPanelPreferredWidth = 420.0f;
    private const float InfoPanelMinWidth = 340.0f;
    private const float InfoPanelGap = 24.0f;
    private const float InfoPanelTop = 42.0f;
    private const float InfoPanelMaxHeight = 760.0f;
    private const float OverlayMargin = 34.0f;

    public static FireChampionMenuLayout MainMenu(float screenWidth, float screenHeight)
    {
        FireChampionMenuLayout layout = new FireChampionMenuLayout();
        float usableWidth = Mathf.Max(MainPanelMinWidth, screenWidth - OuterMargin * 2.0f);
        float mainWidth = Mathf.Min(MainPanelPreferredWidth, usableWidth);
        float panelHeight = Mathf.Max(320.0f, screenHeight - MainPanelTop * 2.0f);
        layout.mainPanel = new Rect(OuterMargin, MainPanelTop, mainWidth, panelHeight);
        layout.mainContent = new Rect(
            layout.mainPanel.x + MainContentInsetX,
            layout.mainPanel.y + MainContentTopInset,
            Mathf.Max(220.0f, layout.mainPanel.width - MainContentInsetX * 2.0f),
            Mathf.Max(220.0f, layout.mainPanel.height - MainContentTopInset - MainContentBottomInset));

        float infoAvailable = screenWidth - layout.mainPanel.xMax - InfoPanelGap - OuterMargin;
        layout.showInfoCard = screenWidth >= 1024.0f && screenHeight >= 560.0f && infoAvailable >= InfoPanelMinWidth;
        if (layout.showInfoCard)
        {
            float infoWidth = Mathf.Min(InfoPanelPreferredWidth, infoAvailable);
            float infoHeight = Mathf.Min(Mathf.Max(320.0f, screenHeight - InfoPanelTop * 2.0f), InfoPanelMaxHeight);
            layout.infoPanel = new Rect(screenWidth - OuterMargin - infoWidth, InfoPanelTop, infoWidth, infoHeight);
            layout.infoContent = new Rect(
                layout.infoPanel.x + 25.0f,
                layout.infoPanel.y + 24.0f,
                Mathf.Max(260.0f, layout.infoPanel.width - 50.0f),
                Mathf.Max(260.0f, layout.infoPanel.height - 48.0f));
        }
        else
        {
            layout.infoPanel = new Rect(0, 0, 0, 0);
            layout.infoContent = new Rect(0, 0, 0, 0);
        }

        return layout;
    }

    public static FireChampionLayoutAuditReport AuditCommonResolutions()
    {
        FireChampionLayoutAuditReport report = new FireChampionLayoutAuditReport();
        AuditResolution(report, 960.0f, 540.0f, true);
        AuditResolution(report, 1024.0f, 576.0f, false);
        AuditResolution(report, 1280.0f, 720.0f, false);
        AuditResolution(report, 1366.0f, 768.0f, false);
        AuditResolution(report, 1920.0f, 1080.0f, false);
        return report;
    }

    public static FireChampionPlayingOverlayLayout PlayingOverlay(float screenWidth, float screenHeight)
    {
        FireChampionPlayingOverlayLayout layout = new FireChampionPlayingOverlayLayout();
        float safeWidth = Mathf.Max(640.0f, screenWidth);
        float safeHeight = Mathf.Max(420.0f, screenHeight);

        layout.bannerPanel = Centered(safeWidth, 68.0f, 420.0f, 40.0f);
        layout.bannerLabel = Inset(layout.bannerPanel, 20.0f, 8.0f, 20.0f, 8.0f);

        float practiceHeight = Mathf.Min(420.0f, safeHeight - 120.0f);
        float practiceY = Mathf.Clamp(safeHeight - practiceHeight - 40.0f, 104.0f, safeHeight - practiceHeight - 16.0f);
        layout.practicePanel = new Rect(safeWidth - OverlayMargin - 316.0f, practiceY, 316.0f, practiceHeight);
        layout.practiceContent = Inset(layout.practicePanel, 20.0f, 16.0f, 18.0f, 16.0f);

        float tutorialWidth = Mathf.Min(650.0f, safeWidth - OverlayMargin * 2.0f);
        float tutorialHeight = Mathf.Min(286.0f, safeHeight - 160.0f);
        float tutorialY = Mathf.Clamp(safeHeight - tutorialHeight - 40.0f, 120.0f, safeHeight - tutorialHeight - 16.0f);
        layout.tutorialPanel = new Rect(OverlayMargin, tutorialY, tutorialWidth, tutorialHeight);
        layout.tutorialContent = Inset(layout.tutorialPanel, 24.0f, 18.0f, 24.0f, 24.0f);

        float tournamentWidth = Mathf.Min(380.0f, safeWidth - OverlayMargin * 2.0f);
        layout.tournamentPanel = new Rect(safeWidth - OverlayMargin - tournamentWidth, 106.0f, tournamentWidth, 228.0f);
        layout.tournamentContent = Inset(layout.tournamentPanel, 24.0f, 16.0f, 26.0f, 16.0f);

        float diagnosticsWidth = Mathf.Min(340.0f, safeWidth - OverlayMargin * 2.0f);
        layout.networkDiagnosticsPanel = new Rect(safeWidth - OverlayMargin - diagnosticsWidth, 106.0f, diagnosticsWidth, 184.0f);
        layout.networkDiagnosticsContent = Inset(layout.networkDiagnosticsPanel, 22.0f, 16.0f, 20.0f, 14.0f);

        layout.networkWaitingPanel = Centered(safeWidth, safeHeight * 0.5f - 115.0f, Mathf.Min(480.0f, safeWidth - OverlayMargin * 2.0f), 230.0f);
        layout.networkWaitingContent = Inset(layout.networkWaitingPanel, 30.0f, 25.0f, 30.0f, 25.0f);

        layout.pausePanel = Centered(safeWidth, safeHeight * 0.5f - 120.0f, Mathf.Min(360.0f, safeWidth - OverlayMargin * 2.0f), 220.0f);
        layout.pauseContent = Inset(layout.pausePanel, 30.0f, 28.0f, 30.0f, 22.0f);

        layout.summaryPanel = Centered(safeWidth, safeHeight * 0.5f - 180.0f, Mathf.Min(480.0f, safeWidth - OverlayMargin * 2.0f), 350.0f);
        layout.summaryContent = Inset(layout.summaryPanel, 35.0f, 30.0f, 35.0f, 20.0f);
        return layout;
    }

    private static void AuditResolution(FireChampionLayoutAuditReport report, float width, float height, bool narrow)
    {
        report.resolutionCount++;
        FireChampionMenuLayout layout = MainMenu(width, height);
        RequireInside(report, layout.mainPanel, width, height, "main panel " + width + "x" + height);
        RequireInside(report, layout.mainContent, width, height, "main content " + width + "x" + height);
        RequirePositive(report, layout.mainPanel, "main panel " + width + "x" + height);
        RequirePositive(report, layout.mainContent, "main content " + width + "x" + height);

        if (layout.showInfoCard)
        {
            RequireInside(report, layout.infoPanel, width, height, "info panel " + width + "x" + height);
            RequireInside(report, layout.infoContent, width, height, "info content " + width + "x" + height);
            RequirePositive(report, layout.infoPanel, "info panel " + width + "x" + height);
            RequirePositive(report, layout.infoContent, "info content " + width + "x" + height);
            if (layout.mainPanel.Overlaps(layout.infoPanel))
            {
                report.Fail("main and info panels overlap at " + width + "x" + height);
            }
        }
        else
        {
            report.infoHiddenCount++;
            if (!narrow)
            {
                report.Fail("info card hidden unexpectedly at " + width + "x" + height);
            }
        }

        FireChampionPlayingOverlayLayout overlay = PlayingOverlay(width, height);
        RequireOverlay(report, overlay.bannerPanel, overlay.bannerLabel, width, height, "banner " + width + "x" + height);
        RequireOverlay(report, overlay.practicePanel, overlay.practiceContent, width, height, "practice " + width + "x" + height);
        RequireOverlay(report, overlay.tutorialPanel, overlay.tutorialContent, width, height, "tutorial " + width + "x" + height);
        RequireOverlay(report, overlay.tournamentPanel, overlay.tournamentContent, width, height, "tournament " + width + "x" + height);
        RequireOverlay(report, overlay.networkDiagnosticsPanel, overlay.networkDiagnosticsContent, width, height, "network diagnostics " + width + "x" + height);
        RequireOverlay(report, overlay.networkWaitingPanel, overlay.networkWaitingContent, width, height, "network waiting " + width + "x" + height);
        RequireOverlay(report, overlay.pausePanel, overlay.pauseContent, width, height, "pause " + width + "x" + height);
        RequireOverlay(report, overlay.summaryPanel, overlay.summaryContent, width, height, "summary " + width + "x" + height);
    }

    private static Rect Centered(float screenWidth, float y, float width, float height)
    {
        return new Rect(screenWidth * 0.5f - width * 0.5f, y, width, height);
    }

    private static Rect Inset(Rect rect, float left, float top, float right, float bottom)
    {
        return new Rect(
            rect.x + left,
            rect.y + top,
            Mathf.Max(1.0f, rect.width - left - right),
            Mathf.Max(1.0f, rect.height - top - bottom));
    }

    private static void RequireOverlay(FireChampionLayoutAuditReport report, Rect panel, Rect content, float width, float height, string label)
    {
        RequireInside(report, panel, width, height, label + " panel");
        RequireInside(report, content, width, height, label + " content");
        RequirePositive(report, panel, label + " panel");
        RequirePositive(report, content, label + " content");
        if (!panel.Contains(new Vector2(content.xMin, content.yMin)) || !panel.Contains(new Vector2(content.xMax, content.yMax)))
        {
            report.Fail(label + " content is outside panel");
        }
    }

    private static void RequireInside(FireChampionLayoutAuditReport report, Rect rect, float width, float height, string label)
    {
        if (rect.xMin < -0.01f || rect.yMin < -0.01f || rect.xMax > width + 0.01f || rect.yMax > height + 0.01f)
        {
            report.Fail(label + " is outside screen");
        }
    }

    private static void RequirePositive(FireChampionLayoutAuditReport report, Rect rect, string label)
    {
        if (rect.width <= 0.0f || rect.height <= 0.0f)
        {
            report.Fail(label + " has non-positive size");
        }
    }
}
