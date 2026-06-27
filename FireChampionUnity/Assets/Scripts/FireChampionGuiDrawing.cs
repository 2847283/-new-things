using UnityEngine;

public static class FireChampionGuiDrawing
{
    public static Texture2D CreateDiscTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((1.0f - distance) * 12.0f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    public static void DrawPanel(Texture2D pixel, float x, float y, float w, float h, Color color)
    {
        DrawRect(pixel, new Rect(x, y, w, h), color);
    }

    public static void DrawRect(Texture2D pixel, Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = old;
    }

    public static void DrawDisc(Texture2D discTexture, Vector2 center, float radius, Color color)
    {
        DrawEllipse(discTexture, center, radius, radius, color);
    }

    public static void DrawEllipse(Texture2D discTexture, Vector2 center, float radiusX, float radiusY, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(center.x - radiusX, center.y - radiusY, radiusX * 2.0f, radiusY * 2.0f), discTexture);
        GUI.color = old;
    }

    public static void DrawCapsule(Texture2D pixel, Texture2D discTexture, Vector2 a, Vector2 b, float width, Color color)
    {
        DrawLine(pixel, a, b, width, color);
        DrawDisc(discTexture, a, width * 0.5f, color);
        DrawDisc(discTexture, b, width * 0.5f, color);
    }

    public static void DrawRectOutline(Texture2D pixel, Rect rect, float width, Color color)
    {
        DrawLine(pixel, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin), width, color);
        DrawLine(pixel, new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax), width, color);
        DrawLine(pixel, new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax), width, color);
        DrawLine(pixel, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin), width, color);
    }

    public static void DrawLine(Texture2D pixel, Vector2 a, Vector2 b, float width, Color color)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;
        GUI.color = color;
        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        float length = Vector2.Distance(a, b);
        GUIUtility.RotateAroundPivot(angle, a);
        GUI.DrawTexture(new Rect(a.x, a.y - width * 0.5f, length, width), pixel);
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    public static void DrawCircle(Texture2D pixel, Vector2 center, float radius, float width, Color color)
    {
        Vector2 prev = center + new Vector2(radius, 0);
        for (int i = 1; i <= 28; i++)
        {
            float a = i / 28.0f * Mathf.PI * 2.0f;
            Vector2 next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            DrawLine(pixel, prev, next, width, color);
            prev = next;
        }
    }
}
