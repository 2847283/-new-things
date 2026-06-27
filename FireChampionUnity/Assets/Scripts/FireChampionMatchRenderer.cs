using UnityEngine;

public struct FireChampionWorldRenderContext
{
    public Texture2D pixel;
    public Texture2D discTexture;
    public Texture2D courtSceneTexture;
    public Texture2D courtBackdropTexture;
    public BalanceGameplay gameplay;
    public CharacterConfig[] roster;
    public CourtConfig[] courts;
    public RulesetConfig rules;
    public PracticeDrillState practiceDrill;
    public ShuttleState shuttle;
    public GameMode mode;
    public int selectedCourt;
    public int tutorialStep;
    public float scale;
    public float courtBottom;
    public float courtHalfWidth;
    public float groundY;
    public float netHeight;
    public float courtPhase;
    public Vector2 screenShakeOffset;
}

public static class FireChampionMatchRenderer
{
    public static void DrawBackground(FireChampionWorldRenderContext context)
    {
        int court = SelectedCourtIndex(context);
        Color bg = context.courts[court].background;
        DrawRect(context, new Rect(0, 0, Screen.width, Screen.height), bg);
        DrawCourtSceneTexture(context);
        DrawRect(context, new Rect(0, context.courtBottom + 2, Screen.width, Screen.height - context.courtBottom), new Color(0.015f, 0.016f, 0.018f, 1f));
        DrawCourtBackdrop(context);

        if (context.rules.courtModifiersEnabled)
        {
            Color accent = context.courts[court].accent;
            float pulse = 0.35f + Mathf.Sin(context.courtPhase * context.courts[court].period) * 0.25f;
            DrawRect(context, new Rect(0, 0, Screen.width, Screen.height), new Color(accent.r, accent.g, accent.b, Mathf.Clamp01(pulse * 0.15f)));
        }
    }

    public static void DrawCourtSurface(FireChampionWorldRenderContext context, Vector2 leftBase, Vector2 rightBase)
    {
        int court = SelectedCourtIndex(context);
        Color accent = context.courts[court].accent;
        BalanceVisualTuning visuals = context.gameplay.visuals;
        float surfaceTopY = visuals.courtSurfaceTopY;
        Vector2 topLeft = WorldToScreen(context, new Vector2(-context.courtHalfWidth, surfaceTopY));
        Vector2 topRight = WorldToScreen(context, new Vector2(context.courtHalfWidth, surfaceTopY));
        Rect courtRect = Rect.MinMaxRect(leftBase.x, topLeft.y, rightBase.x, leftBase.y);
        Color wood = court == 1 ? new Color(0.10f, 0.13f, 0.16f, 1f) : new Color(0.23f, 0.15f, 0.08f, 1f);
        if (court == 2)
        {
            wood = new Color(0.05f, 0.055f, 0.09f, 1f);
        }

        DrawRect(context, courtRect, wood);
        DrawRect(context, new Rect(courtRect.x, courtRect.y, courtRect.width, courtRect.height * 0.18f), new Color(1f, 1f, 1f, visuals.courtTopHighlightAlpha));
        int stripeCount = Mathf.Max(1, visuals.courtStripeCount);
        for (int i = 0; i < stripeCount; i++)
        {
            float y = Mathf.Lerp(courtRect.y, courtRect.yMax, (i + 1) / (float)(stripeCount + 1));
            float shade = i % 2 == 0 ? visuals.courtStripeAlpha : visuals.courtAlternateStripeAlpha;
            DrawRect(context, new Rect(courtRect.x, y - 1, courtRect.width, courtRect.height / (stripeCount + 1)), new Color(1f, 1f, 1f, shade));
            DrawLine(context, new Vector2(courtRect.x, y), new Vector2(courtRect.xMax, y), 1, new Color(0, 0, 0, visuals.courtLineShadowAlpha));
        }

        Color line = Color.white;
        Color softLine = new Color(1f, 1f, 1f, visuals.courtSoftLineAlpha);
        Color glow = new Color(accent.r, accent.g, accent.b, visuals.courtGlowAlpha);
        DrawRect(context, new Rect(courtRect.x - 6, courtRect.y - 7, courtRect.width + 12, 6), glow);
        DrawLine(context, leftBase, rightBase, 5, line);
        DrawLine(context, WorldToScreen(context, new Vector2(-context.courtHalfWidth, 0)), WorldToScreen(context, new Vector2(-context.courtHalfWidth, surfaceTopY)), 4, line);
        DrawLine(context, WorldToScreen(context, new Vector2(context.courtHalfWidth, 0)), WorldToScreen(context, new Vector2(context.courtHalfWidth, surfaceTopY)), 4, line);
        DrawLine(context, topLeft, topRight, 3, softLine);
        DrawLine(context, WorldToScreen(context, new Vector2(-context.courtHalfWidth, 1.35f)), WorldToScreen(context, new Vector2(context.courtHalfWidth, 1.35f)), 2, new Color(1f, 1f, 1f, 0.22f));
        DrawLine(context, WorldToScreen(context, new Vector2(-4.0f, 0)), WorldToScreen(context, new Vector2(-4.0f, surfaceTopY)), 2, new Color(1f, 1f, 1f, visuals.courtLineShadowAlpha));
        DrawLine(context, WorldToScreen(context, new Vector2(4.0f, 0)), WorldToScreen(context, new Vector2(4.0f, surfaceTopY)), 2, new Color(1f, 1f, 1f, visuals.courtLineShadowAlpha));
    }

    public static void DrawNet(FireChampionWorldRenderContext context)
    {
        Vector2 basePoint = WorldToScreen(context, new Vector2(0, 0));
        Vector2 topPoint = WorldToScreen(context, new Vector2(0, context.netHeight + 0.08f));
        float netHeight = basePoint.y - topPoint.y;
        Rect mesh = new Rect(basePoint.x - 13, topPoint.y, 26, netHeight);
        DrawRect(context, mesh, new Color(0.85f, 0.9f, 0.92f, 0.12f));
        for (int i = 1; i < 5; i++)
        {
            float y = Mathf.Lerp(mesh.yMin, mesh.yMax, i / 5.0f);
            DrawLine(context, new Vector2(mesh.xMin, y), new Vector2(mesh.xMax, y), 1, new Color(1f, 1f, 1f, 0.28f));
        }

        for (int i = 0; i < 4; i++)
        {
            float x = Mathf.Lerp(mesh.xMin + 5, mesh.xMax - 5, i / 3.0f);
            DrawLine(context, new Vector2(x, mesh.yMin), new Vector2(x, mesh.yMax), 1, new Color(1f, 1f, 1f, 0.22f));
        }

        DrawCapsule(context, basePoint, topPoint, 12, new Color(0.02f, 0.025f, 0.03f, 1f));
        DrawCapsule(context, basePoint, topPoint, 7, new Color(0.92f, 0.92f, 0.86f, 1f));
        DrawDisc(context, basePoint, 8, new Color(0.02f, 0.02f, 0.018f, 0.8f));
    }

    public static void DrawPracticeTarget(FireChampionWorldRenderContext context)
    {
        if (context.mode != GameMode.Practice)
        {
            return;
        }

        Vector2 center = WorldToScreen(context, new Vector2(context.practiceDrill.TargetX, 0.05f));
        float radius = context.practiceDrill.TargetRadius * context.scale;
        Color target = new Color(0.35f, 1.0f, 0.62f, 0.20f);
        DrawEllipse(context, center + new Vector2(0, 4), radius, radius * 0.34f, target);
        DrawCircle(context, center + new Vector2(0, 4), radius, 3, new Color(0.35f, 1.0f, 0.62f, 0.72f));
        DrawCircle(context, center + new Vector2(0, 4), radius * 0.52f, 2, new Color(1f, 1f, 1f, 0.38f));
    }

    public static void DrawTutorialTargetMarker(FireChampionWorldRenderContext context)
    {
        if (context.mode != GameMode.Tutorial || context.tutorialStep >= TutorialCoachData.StepCount)
        {
            return;
        }

        TutorialCoachStep step = TutorialCoachData.Step(context.tutorialStep);
        Vector2 center = WorldToScreen(context, step.markerWorld);
        float pulse = 0.65f + Mathf.Sin(context.courtPhase * 5.8f) * 0.22f;
        float radius = step.markerRadius * context.scale * (0.92f + pulse * 0.12f);
        Color accent = new Color(1.0f, 0.86f, 0.34f, 0.76f);
        DrawEllipse(context, center + new Vector2(0, 5), radius, radius * 0.34f, new Color(1.0f, 0.82f, 0.22f, 0.18f));
        DrawCircle(context, center + new Vector2(0, 5), radius, 3, accent);
        DrawCircle(context, center + new Vector2(0, 5), radius * 0.54f, 2, new Color(1f, 1f, 1f, 0.34f));

        Vector2 arrowTop = center + new Vector2(0, -54 - pulse * 8);
        Vector2 arrowPoint = center + new Vector2(0, -18);
        DrawLine(context, arrowTop, arrowPoint, 5, accent);
        DrawLine(context, arrowPoint, arrowPoint + new Vector2(-12, -13), 4, accent);
        DrawLine(context, arrowPoint, arrowPoint + new Vector2(12, -13), 4, accent);

        float labelW = 118;
        DrawPanel(context, center.x - labelW * 0.5f, center.y - 92, labelW, 30, new Color(0.01f, 0.012f, 0.014f, 0.78f));
        GUI.Label(new Rect(center.x - labelW * 0.5f + 8, center.y - 87, labelW - 16, 22), "<color=white><b>" + step.markerLabel + "</b></color>");
    }

    public static void DrawPlayer(FireChampionWorldRenderContext context, PlayerActor player, Color accent, Vector2 racketWorldPosition)
    {
        CharacterConfig character = context.roster[player.characterIndex];
        string code = character.code;
        float facing = player.facing >= 0 ? 1f : -1f;
        BalanceVisualTuning visuals = context.gameplay.visuals;
        float bodyScale = Mathf.Clamp(character.visualBodyScale, 0.72f, 1.35f);
        float limbScale = Mathf.Clamp(character.visualLimbScale, 0.72f, 1.35f);
        float headScale = Mathf.Clamp(character.visualHeadScale, 0.72f, 1.28f);
        Color outline = new Color(0.018f, 0.02f, 0.024f, 0.98f);
        Color skin = player.side == Side.Left ? new Color(1.0f, 0.72f, 0.52f, 1f) : new Color(0.88f, 0.62f, 0.46f, 1f);
        Color jersey = Color.Lerp(accent, Color.white, Mathf.Clamp01(character.jerseyWhiteMix));
        Color jerseyDark = Color.Lerp(accent, Color.black, 0.45f);
        Color shorts = code == "HEAVY" ? new Color(0.16f, 0.085f, 0.055f, 1f) : new Color(0.055f, 0.065f, 0.085f, 1f);
        Vector2 foot = WorldToScreen(context, new Vector2(player.x, player.y));
        Vector2 hip = WorldToScreen(context, new Vector2(player.x, player.y + 0.66f));
        Vector2 waist = WorldToScreen(context, new Vector2(player.x, player.y + 0.90f));
        Vector2 chest = WorldToScreen(context, new Vector2(player.x, player.y + 1.22f));
        Vector2 neck = WorldToScreen(context, new Vector2(player.x, player.y + 1.40f));
        Vector2 head = WorldToScreen(context, new Vector2(player.x + facing * 0.02f, player.y + 1.70f));
        Vector2 shoulderFront = WorldToScreen(context, new Vector2(player.x + facing * 0.20f, player.y + 1.28f));
        Vector2 shoulderBack = WorldToScreen(context, new Vector2(player.x - facing * 0.22f, player.y + 1.22f));
        Vector2 hand = WorldToScreen(context, new Vector2(player.x + facing * 0.44f, player.y + 1.18f));
        Vector2 backHand = WorldToScreen(context, new Vector2(player.x - facing * 0.36f, player.y + 0.98f));
        Vector2 racket = WorldToScreen(context, racketWorldPosition);
        Vector2 kneeA = WorldToScreen(context, new Vector2(player.x - 0.26f, player.y + 0.36f));
        Vector2 kneeB = WorldToScreen(context, new Vector2(player.x + 0.26f, player.y + 0.34f));
        Vector2 shoeA = WorldToScreen(context, new Vector2(player.x - 0.43f, player.y + 0.05f));
        Vector2 shoeB = WorldToScreen(context, new Vector2(player.x + 0.43f, player.y + 0.05f));
        float bodyWidth = visuals.bodyWidth * bodyScale;
        float limbWidth = visuals.limbWidth * limbScale;

        DrawEllipse(context, foot + new Vector2(0, 9), visuals.playerShadowWidth * bodyScale, visuals.playerShadowHeight, new Color(0f, 0f, 0f, 0.34f));
        if (player.skillTimer > 0 || player.ultimateTimer > 0)
        {
            float pulse = visuals.skillAuraAlpha + Mathf.Sin(context.courtPhase * 12.0f) * 0.08f;
            DrawEllipse(context, foot + new Vector2(0, 8), visuals.skillAuraWidth * bodyScale, visuals.skillAuraHeight, new Color(accent.r, accent.g, accent.b, pulse));
            DrawCircle(context, WorldToScreen(context, new Vector2(player.x, player.y + 0.95f)), 40 * bodyScale, 3, new Color(accent.r, accent.g, accent.b, 0.52f));
        }

        DrawCapsule(context, hip, kneeA, limbWidth + 5, outline);
        DrawCapsule(context, kneeA, shoeA, limbWidth + 3, outline);
        DrawCapsule(context, hip, kneeB, limbWidth + 5, outline);
        DrawCapsule(context, kneeB, shoeB, limbWidth + 3, outline);
        DrawCapsule(context, hip, kneeA, limbWidth, shorts);
        DrawCapsule(context, kneeA, shoeA, limbWidth - 1, skin);
        DrawCapsule(context, hip, kneeB, limbWidth, shorts);
        DrawCapsule(context, kneeB, shoeB, limbWidth - 1, skin);
        DrawEllipse(context, shoeA + new Vector2(-facing * 4, 2), 17, 7, outline);
        DrawEllipse(context, shoeA + new Vector2(-facing * 4, 0), 13, 5, Color.white);
        DrawEllipse(context, shoeB + new Vector2(facing * 4, 2), 17, 7, outline);
        DrawEllipse(context, shoeB + new Vector2(facing * 4, 0), 13, 5, Color.white);

        DrawCapsule(context, shoulderBack, backHand, limbWidth + 4, outline);
        DrawCapsule(context, shoulderBack, backHand, limbWidth - 1, skin);
        DrawCapsule(context, hip, chest, bodyWidth + 7, outline);
        DrawCapsule(context, hip, chest, bodyWidth, jersey);
        DrawCapsule(context, waist, chest + new Vector2(facing * 5, -5), 7, new Color(1f, 1f, 1f, 0.52f));
        DrawCapsule(context, hip + new Vector2(-facing * 8, -1), waist + new Vector2(-facing * 5, -2), 10, shorts);
        DrawCapsule(context, shoulderFront, hand, limbWidth + 5, outline);
        DrawCapsule(context, shoulderFront, hand, limbWidth, skin);

        if (code == "HEAVY")
        {
            DrawDisc(context, shoulderFront, 10, new Color(1f, 0.75f, 0.48f, 1f));
            DrawDisc(context, shoulderBack, 9, new Color(1f, 0.75f, 0.48f, 0.9f));
        }

        DrawDisc(context, neck, 9, outline);
        DrawDisc(context, neck, 6, skin);
        float headRadius = visuals.headRadius * headScale;
        DrawDisc(context, head, headRadius + 5, outline);
        DrawDisc(context, head, headRadius, skin);
        DrawEllipse(context, head + new Vector2(-facing * 3, -13 * headScale), 18 * headScale, 9 * headScale, new Color(0.045f, 0.032f, 0.026f, 1f));
        DrawCapsule(context, head + new Vector2(-14, -7), head + new Vector2(14, -6), 5, jerseyDark);
        DrawDisc(context, head + new Vector2(facing * 7, -1), 2.5f, outline);
        DrawCapsule(context, head + new Vector2(facing * 2, 8), head + new Vector2(facing * 9, 7), 2, new Color(0.45f, 0.18f, 0.14f, 0.8f));

        DrawCapsule(context, hand, racket, 5, outline);
        DrawCapsule(context, hand, racket, 3, new Color(0.92f, 0.78f, 0.52f, 1f));
        DrawEllipse(context, racket, visuals.racketOuterRadiusX, visuals.racketOuterRadiusY, new Color(0.02f, 0.025f, 0.03f, 0.95f));
        DrawEllipse(context, racket, visuals.racketInnerRadiusX, visuals.racketInnerRadiusY, new Color(accent.r, accent.g, accent.b, player.swingTimer > 0 ? 0.72f : 0.36f));
        DrawLine(context, racket + new Vector2(-10, 0), racket + new Vector2(10, 0), 1, new Color(1f, 1f, 1f, 0.56f));
        DrawLine(context, racket + new Vector2(0, -14), racket + new Vector2(0, 14), 1, new Color(1f, 1f, 1f, 0.48f));

        if (player.swingTimer > 0)
        {
            float arc = visuals.swingArcBaseRadius + player.swingTimer * visuals.swingArcTimerScale;
            DrawCircle(context, racket + new Vector2(facing * 3, 0), arc, 4, new Color(accent.r, accent.g, accent.b, 0.34f));
            DrawLine(context, hand, racket + new Vector2(facing * 22, -12), 4, new Color(accent.r, accent.g, accent.b, 0.40f));
        }

        if (code == "DASH" && player.skillTimer > 0)
        {
            Vector2 back = foot - new Vector2(facing * 22, 8);
            DrawCapsule(context, back, back - new Vector2(facing * 42, -5), 5, new Color(accent.r, accent.g, accent.b, 0.62f));
            DrawCapsule(context, back + new Vector2(0, -13), back - new Vector2(facing * 34, 8), 3, new Color(accent.r, accent.g, accent.b, 0.46f));
        }

        if (code == "TRICK" && (player.skillTimer > 0 || player.ultimateTimer > 0))
        {
            Vector2 orbit = WorldToScreen(context, new Vector2(player.x + facing * 0.45f, player.y + 1.25f));
            DrawCircle(context, orbit, 30, 3, new Color(accent.r, accent.g, accent.b, 0.58f));
            DrawDisc(context, orbit + new Vector2(Mathf.Cos(context.courtPhase * 6f) * 28, Mathf.Sin(context.courtPhase * 6f) * 18), 5, new Color(accent.r, accent.g, accent.b, 0.82f));
        }
    }

    public static void DrawShuttle(FireChampionWorldRenderContext context)
    {
        Vector2 s = WorldToScreen(context, context.shuttle.position);
        Vector2 shadow = WorldToScreen(context, new Vector2(context.shuttle.position.x, context.groundY));
        DrawEllipse(context, shadow + new Vector2(0, 7), Mathf.Clamp(18 - context.shuttle.position.y * 2.2f, 6, 18), 5, new Color(0, 0, 0, 0.28f));

        float speed = context.shuttle.velocity.magnitude;
        Vector2 direction = speed > 0.05f ? new Vector2(context.shuttle.velocity.x, -context.shuttle.velocity.y).normalized : Vector2.right;
        Vector2 perp = new Vector2(-direction.y, direction.x);
        float tailLength = Mathf.Clamp(20 + speed * 5.5f, 22, 68);
        Color fastTrail = speed > 7.4f ? new Color(1f, 0.62f, 0.24f, 0.54f) : new Color(0.85f, 0.95f, 1f, 0.42f);
        DrawCapsule(context, s - direction * 5, s - direction * tailLength, Mathf.Clamp(5 + speed * 0.14f, 5, 9), fastTrail);
        DrawCapsule(context, s + perp * 5 - direction * 8, s + perp * 9 - direction * tailLength * 0.55f, 4, new Color(1f, 1f, 1f, 0.56f));
        DrawCapsule(context, s - perp * 5 - direction * 8, s - perp * 9 - direction * tailLength * 0.55f, 4, new Color(1f, 1f, 1f, 0.50f));
        DrawCapsule(context, s, s - direction * 16, 6, new Color(0.94f, 0.96f, 0.86f, 0.92f));
        DrawDisc(context, s, 9, new Color(0.02f, 0.025f, 0.03f, 1f));
        DrawDisc(context, s, 6, new Color(1f, 0.96f, 0.78f, 1f));
    }

    public static void DrawCourtBackdrop(FireChampionWorldRenderContext context)
    {
        int court = SelectedCourtIndex(context);
        Color accent = context.courts[court].accent;
        Color faint = new Color(accent.r, accent.g, accent.b, 0.22f);
        float horizon = context.courtBottom - context.scale * 2.82f;
        DrawRect(context, new Rect(0, horizon - 42, Screen.width, 42), new Color(0, 0, 0, 0.18f));
        DrawCourtBackdropTexture(context, horizon, accent);

        if (court == 0)
        {
            DrawRect(context, new Rect(0, horizon - 112, Screen.width, 70), new Color(0.16f, 0.105f, 0.055f, 0.34f));
            for (int i = 0; i < 7; i++)
            {
                float x = i * Screen.width / 6.0f;
                DrawRect(context, new Rect(x - 4, horizon - 108, 8, 82), new Color(0.42f, 0.25f, 0.12f, 0.34f));
            }

            for (int i = 0; i < 6; i++)
            {
                float x = 88 + i * (Screen.width - 176) / 5.0f;
                DrawEllipse(context, new Vector2(x, horizon - 72), 13, 17, new Color(1f, 0.75f, 0.35f, 0.22f));
                DrawRect(context, new Rect(x - 1, horizon - 95, 2, 20), new Color(1f, 0.75f, 0.35f, 0.28f));
            }

            for (int i = 0; i < 6; i++)
            {
                float y = 0.35f + i * 0.38f;
                DrawLine(context, WorldToScreen(context, new Vector2(-context.courtHalfWidth, y)), WorldToScreen(context, new Vector2(context.courtHalfWidth, y)), 1, faint);
            }

            DrawLine(context, WorldToScreen(context, new Vector2(-7.2f, 2.65f)), WorldToScreen(context, new Vector2(7.2f, 2.65f)), 8, new Color(0.72f, 0.48f, 0.25f, 0.35f));
        }
        else if (court == 1)
        {
            DrawRect(context, new Rect(0, horizon - 90, Screen.width, 90), new Color(0.06f, 0.10f, 0.14f, 0.38f));
            DrawRect(context, new Rect(0, horizon, Screen.width, 4), new Color(0.72f, 0.86f, 0.92f, 0.30f));
            for (int i = 0; i < 10; i++)
            {
                float w = 42 + (i % 3) * 24;
                float h = 44 + (i % 4) * 20;
                float x = 28 + i * (Screen.width - 56) / 10.0f;
                DrawRect(context, new Rect(x, horizon - h, w, h), new Color(0.015f, 0.022f, 0.032f, 0.68f));
                if (i % 2 == 0)
                {
                    DrawRect(context, new Rect(x + 9, horizon - h + 12, w - 18, 3), new Color(accent.r, accent.g, accent.b, 0.25f));
                }
            }
        }
        else
        {
            DrawRect(context, new Rect(0, horizon - 118, Screen.width, 118), new Color(0.02f, 0.025f, 0.06f, 0.44f));
            for (int i = -8; i <= 8; i += 2)
            {
                DrawLine(context, WorldToScreen(context, new Vector2(i, 0)), WorldToScreen(context, new Vector2(i * 0.55f, 2.8f)), 1, faint);
            }

            for (int i = 1; i <= 5; i++)
            {
                float y = i * 0.48f;
                DrawLine(context, WorldToScreen(context, new Vector2(-context.courtHalfWidth, y)), WorldToScreen(context, new Vector2(context.courtHalfWidth, y)), 1, faint);
            }

            for (int i = 0; i < 5; i++)
            {
                float y = horizon - 96 + i * 20;
                DrawLine(context, new Vector2(0, y), new Vector2(Screen.width, y + Mathf.Sin(context.courtPhase + i) * 12), 2, new Color(accent.r, accent.g, accent.b, 0.16f));
            }
        }
    }

    private static void DrawCourtBackdropTexture(FireChampionWorldRenderContext context, float horizon, Color accent)
    {
        if (context.courtSceneTexture != null)
        {
            return;
        }

        Texture2D texture = context.courtBackdropTexture;
        if (texture == null || texture.width <= 0 || texture.height <= 0)
        {
            return;
        }

        float aspect = Mathf.Clamp(texture.width / (float)texture.height, 0.75f, 3.2f);
        float width = Mathf.Clamp(Screen.width * 0.31f, 220f, 390f);
        float height = Mathf.Clamp(width / aspect, 86f, 180f);
        width = height * aspect;
        float x = Screen.width * 0.5f - width * 0.5f;
        float y = Mathf.Clamp(horizon - height - 50f, 108f, Mathf.Max(108f, horizon - height - 12f));
        Rect frame = new Rect(x - 13f, y - 11f, width + 26f, height + 22f);
        Rect image = new Rect(x, y, width, height);

        DrawPanel(context, frame.x, frame.y, frame.width, frame.height, new Color(0f, 0f, 0f, 0.25f));
        DrawRect(context, new Rect(frame.x, frame.yMax - 5f, frame.width, 5f), new Color(accent.r, accent.g, accent.b, 0.20f));

        Color previous = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.38f);
        GUI.DrawTexture(image, texture, ScaleMode.ScaleToFit, true);
        GUI.color = previous;
    }

    private static void DrawCourtSceneTexture(FireChampionWorldRenderContext context)
    {
        Texture2D texture = context.courtSceneTexture;
        if (texture == null || texture.width <= 0 || texture.height <= 0)
        {
            return;
        }

        Color previous = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.82f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.ScaleAndCrop, true);
        GUI.color = previous;
        DrawRect(context, new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.16f));
    }

    public static Vector2 WorldToScreen(FireChampionWorldRenderContext context, Vector2 world)
    {
        return new Vector2(Screen.width * 0.5f + world.x * context.scale, context.courtBottom - world.y * context.scale) + context.screenShakeOffset;
    }

    private static int SelectedCourtIndex(FireChampionWorldRenderContext context)
    {
        return Mathf.Clamp(context.selectedCourt, 0, context.courts.Length - 1);
    }

    private static void DrawPanel(FireChampionWorldRenderContext context, float x, float y, float w, float h, Color color)
    {
        FireChampionGuiDrawing.DrawPanel(context.pixel, x, y, w, h, color);
    }

    private static void DrawRect(FireChampionWorldRenderContext context, Rect rect, Color color)
    {
        FireChampionGuiDrawing.DrawRect(context.pixel, rect, color);
    }

    private static void DrawDisc(FireChampionWorldRenderContext context, Vector2 center, float radius, Color color)
    {
        FireChampionGuiDrawing.DrawDisc(context.discTexture, center, radius, color);
    }

    private static void DrawEllipse(FireChampionWorldRenderContext context, Vector2 center, float radiusX, float radiusY, Color color)
    {
        FireChampionGuiDrawing.DrawEllipse(context.discTexture, center, radiusX, radiusY, color);
    }

    private static void DrawCapsule(FireChampionWorldRenderContext context, Vector2 a, Vector2 b, float width, Color color)
    {
        FireChampionGuiDrawing.DrawCapsule(context.pixel, context.discTexture, a, b, width, color);
    }

    private static void DrawLine(FireChampionWorldRenderContext context, Vector2 a, Vector2 b, float width, Color color)
    {
        FireChampionGuiDrawing.DrawLine(context.pixel, a, b, width, color);
    }

    private static void DrawCircle(FireChampionWorldRenderContext context, Vector2 center, float radius, float width, Color color)
    {
        FireChampionGuiDrawing.DrawCircle(context.pixel, center, radius, width, color);
    }
}
