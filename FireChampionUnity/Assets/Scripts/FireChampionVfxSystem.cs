using System.Collections.Generic;
using UnityEngine;

public enum FireChampionVfxKind
{
    Hit,
    Smash,
    Score,
    Skill
}

public sealed class FireChampionVfxEvent
{
    public FireChampionVfxKind kind;
    public Vector2 worldPosition;
    public Color color;
    public float age;
    public float duration;
    public float radius;
    public int facing;
    public bool strong;
}

public sealed class FireChampionVfxSystem
{
    private static readonly BalanceFeedbackTuning FallbackFeedback = new BalanceFeedbackTuning();
    private readonly List<FireChampionVfxEvent> events = new List<FireChampionVfxEvent>();

    public IReadOnlyList<FireChampionVfxEvent> Events
    {
        get { return events; }
    }

    public void Clear()
    {
        events.Clear();
    }

    public void Update(float dt)
    {
        if (dt <= 0.0f)
        {
            return;
        }

        for (int i = events.Count - 1; i >= 0; i--)
        {
            events[i].age += dt;
            if (events[i].age >= events[i].duration)
            {
                events.RemoveAt(i);
            }
        }
    }

    public void SpawnHit(Vector2 worldPosition, ShotType shot, bool goodTiming, Color accent, int facing, BalanceFeedbackTuning feedback)
    {
        BalanceFeedbackTuning tuning = EffectiveFeedback(feedback);
        FireChampionVfxKind kind = shot == ShotType.Smash ? FireChampionVfxKind.Smash : FireChampionVfxKind.Hit;
        Add(kind, worldPosition, goodTiming ? Color.white : accent, facing, goodTiming, tuning.hitVfxDuration, shot == ShotType.Smash ? tuning.smashVfxRadius : tuning.hitVfxRadius, tuning.maxVfxEvents);
    }

    public void SpawnScore(Vector2 worldPosition, Color accent, bool strong, BalanceFeedbackTuning feedback)
    {
        BalanceFeedbackTuning tuning = EffectiveFeedback(feedback);
        Add(FireChampionVfxKind.Score, worldPosition, accent, 1, strong, tuning.scoreVfxDuration, tuning.scoreVfxRadius, tuning.maxVfxEvents);
    }

    public void SpawnSkill(Vector2 worldPosition, Color accent, int facing, BalanceFeedbackTuning feedback)
    {
        BalanceFeedbackTuning tuning = EffectiveFeedback(feedback);
        Add(FireChampionVfxKind.Skill, worldPosition, accent, facing, true, tuning.skillVfxDuration, tuning.skillVfxRadius, tuning.maxVfxEvents);
    }

    private static BalanceFeedbackTuning EffectiveFeedback(BalanceFeedbackTuning feedback)
    {
        return feedback ?? FallbackFeedback;
    }

    private void Add(FireChampionVfxKind kind, Vector2 worldPosition, Color color, int facing, bool strong, float duration, float radius, int maxEvents)
    {
        int capacity = Mathf.Clamp(maxEvents, 4, 128);
        while (events.Count >= capacity)
        {
            events.RemoveAt(0);
        }

        events.Add(new FireChampionVfxEvent
        {
            kind = kind,
            worldPosition = worldPosition,
            color = color,
            facing = facing >= 0 ? 1 : -1,
            strong = strong,
            duration = Mathf.Max(0.05f, duration),
            radius = Mathf.Max(6.0f, radius)
        });
    }
}

public static class FireChampionVfxRenderer
{
    private static FireChampionVfxAssets assets;

    public static void Draw(FireChampionWorldRenderContext context, IReadOnlyList<FireChampionVfxEvent> events)
    {
        if (events == null)
        {
            return;
        }

        for (int i = 0; i < events.Count; i++)
        {
            DrawEvent(context, events[i]);
        }
    }

    private static void DrawEvent(FireChampionWorldRenderContext context, FireChampionVfxEvent vfx)
    {
        float t = Mathf.Clamp01(vfx.age / Mathf.Max(0.01f, vfx.duration));
        float alpha = Mathf.Clamp01(1.0f - t);
        Vector2 center = FireChampionMatchRenderer.WorldToScreen(context, vfx.worldPosition);
        if (vfx.kind == FireChampionVfxKind.Smash)
        {
            DrawSmash(context, vfx, center, t, alpha);
        }
        else if (vfx.kind == FireChampionVfxKind.Score)
        {
            DrawScore(context, vfx, center, t, alpha);
        }
        else if (vfx.kind == FireChampionVfxKind.Skill)
        {
            DrawSkill(context, vfx, center, t, alpha);
        }
        else
        {
            DrawHit(context, vfx, center, t, alpha);
        }
    }

    private static void DrawHit(FireChampionWorldRenderContext context, FireChampionVfxEvent vfx, Vector2 center, float t, float alpha)
    {
        float radius = Mathf.Lerp(vfx.radius * 0.45f, vfx.radius, t);
        Texture2D texture = Assets.Hit(vfx.strong);
        if (texture != null)
        {
            Color tint = vfx.strong ? Color.white : Color.Lerp(Color.white, vfx.color, 0.45f);
            DrawSprite(texture, center, radius * 2.35f, radius * 2.35f, WithAlpha(tint, alpha * 0.9f), 0.0f);
        }

        Color ray = WithAlpha(vfx.color, alpha * 0.75f);
        for (int i = 0; i < 6; i++)
        {
            float angle = (i / 6.0f) * Mathf.PI * 2.0f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            DrawLine(context, center + dir * radius * 0.25f, center + dir * radius, vfx.strong ? 4.0f : 2.5f, ray);
        }

        DrawEllipse(context, center, radius * 0.34f, radius * 0.24f, WithAlpha(vfx.color, alpha * 0.28f));
    }

    private static void DrawSmash(FireChampionWorldRenderContext context, FireChampionVfxEvent vfx, Vector2 center, float t, float alpha)
    {
        float length = Mathf.Lerp(vfx.radius * 0.9f, vfx.radius * 1.6f, t);
        Vector2 slash = new Vector2(vfx.facing * length, -length * 0.34f);
        if (Assets.SmashTrail != null)
        {
            float angle = vfx.facing >= 0 ? -15.0f : 165.0f;
            DrawSprite(Assets.SmashTrail, center + new Vector2(vfx.facing * 8.0f, -10.0f), length * 2.15f, vfx.radius * 1.45f, WithAlpha(Color.white, alpha * 0.95f), angle);
        }

        Color hot = WithAlpha(new Color(1.0f, 0.42f, 0.16f, 1.0f), alpha * 0.8f);
        DrawLine(context, center - slash * 0.5f, center + slash * 0.5f, 8.0f, hot);
        DrawLine(context, center - slash * 0.28f + new Vector2(0, -10), center + slash * 0.55f + new Vector2(0, -10), 3.0f, WithAlpha(vfx.color, alpha * 0.55f));
    }

    private static void DrawScore(FireChampionWorldRenderContext context, FireChampionVfxEvent vfx, Vector2 center, float t, float alpha)
    {
        float radius = Mathf.Lerp(vfx.radius * 0.55f, vfx.radius * (vfx.strong ? 1.45f : 1.15f), t);
        if (Assets.ScoreBurst != null)
        {
            DrawSprite(Assets.ScoreBurst, center, radius * 2.15f, radius * 2.15f, WithAlpha(Color.white, alpha * 0.9f), 0.0f);
        }

        Color ring = WithAlpha(vfx.color, alpha * 0.68f);
        DrawCircle(context, center, radius, vfx.strong ? 5.0f : 3.0f, ring);
        for (int i = 0; i < 8; i++)
        {
            float angle = (i / 8.0f) * Mathf.PI * 2.0f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            DrawLine(context, center + dir * radius * 0.55f, center + dir * radius * 0.95f, 2.5f, WithAlpha(Color.white, alpha * 0.5f));
        }
    }

    private static void DrawSkill(FireChampionWorldRenderContext context, FireChampionVfxEvent vfx, Vector2 center, float t, float alpha)
    {
        float radius = Mathf.Lerp(vfx.radius * 0.65f, vfx.radius * 1.25f, t);
        Texture2D texture = Assets.Skill(vfx.color);
        if (texture != null)
        {
            DrawSprite(texture, center + new Vector2(0, 18), radius * 2.3f, radius * 2.3f, WithAlpha(Color.Lerp(Color.white, vfx.color, 0.35f), alpha * 0.86f), vfx.facing * 8.0f);
        }

        DrawEllipse(context, center + new Vector2(0, 18), radius, radius * 0.34f, WithAlpha(vfx.color, alpha * 0.32f));
        DrawCircle(context, center + new Vector2(0, 18), radius * 0.62f, 3.0f, WithAlpha(vfx.color, alpha * 0.72f));
    }

    private static FireChampionVfxAssets Assets
    {
        get
        {
            if (assets == null)
            {
                assets = FireChampionVfxAssets.Load();
            }

            return assets;
        }
    }

    private static void DrawSprite(Texture2D texture, Vector2 center, float width, float height, Color color, float angleDegrees)
    {
        if (texture == null)
        {
            return;
        }

        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;
        GUI.color = color;
        GUIUtility.RotateAroundPivot(angleDegrees, center);
        GUI.DrawTexture(new Rect(center.x - width * 0.5f, center.y - height * 0.5f, width, height), texture, ScaleMode.StretchToFill, true);
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }

    private static void DrawEllipse(FireChampionWorldRenderContext context, Vector2 center, float radiusX, float radiusY, Color color)
    {
        FireChampionGuiDrawing.DrawEllipse(context.discTexture, center, radiusX, radiusY, color);
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
