using UnityEngine;

public sealed class FireChampionCourtAssets
{
    public const int BackgroundWidth = 1280;
    public const int BackgroundHeight = 720;

    public static readonly string[] RequiredResourcePaths =
    {
        "FireChampion/Courts/court_dojo_background",
        "FireChampion/Courts/court_rooftop_background",
        "FireChampion/Courts/court_future_background"
    };

    private readonly Texture2D[] backgrounds;

    private FireChampionCourtAssets(Texture2D[] backgrounds)
    {
        this.backgrounds = backgrounds;
    }

    public static FireChampionCourtAssets Load()
    {
        Texture2D[] loaded = new Texture2D[RequiredResourcePaths.Length];
        for (int i = 0; i < RequiredResourcePaths.Length; i++)
        {
            loaded[i] = Resources.Load<Texture2D>(RequiredResourcePaths[i]);
        }

        return new FireChampionCourtAssets(loaded);
    }

    public Texture2D Background(int index)
    {
        if (backgrounds == null || index < 0 || index >= backgrounds.Length)
        {
            return null;
        }

        return backgrounds[index];
    }
}
