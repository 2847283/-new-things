using UnityEngine;

public sealed class FireChampionVfxAssets
{
    public static readonly string[] RequiredResourcePaths =
    {
        "FireChampion/VFX/vfx_hit_sweet",
        "FireChampion/VFX/vfx_hit_solid",
        "FireChampion/VFX/vfx_smash_trail",
        "FireChampion/VFX/vfx_dash_burst",
        "FireChampion/VFX/vfx_trick_spin",
        "FireChampion/VFX/vfx_score_burst"
    };

    public Texture2D HitSweet { get; private set; }
    public Texture2D HitSolid { get; private set; }
    public Texture2D SmashTrail { get; private set; }
    public Texture2D DashBurst { get; private set; }
    public Texture2D TrickSpin { get; private set; }
    public Texture2D ScoreBurst { get; private set; }

    private FireChampionVfxAssets(Texture2D hitSweet, Texture2D hitSolid, Texture2D smashTrail, Texture2D dashBurst, Texture2D trickSpin, Texture2D scoreBurst)
    {
        HitSweet = hitSweet;
        HitSolid = hitSolid;
        SmashTrail = smashTrail;
        DashBurst = dashBurst;
        TrickSpin = trickSpin;
        ScoreBurst = scoreBurst;
    }

    public static FireChampionVfxAssets Load()
    {
        return new FireChampionVfxAssets(
            Resources.Load<Texture2D>(RequiredResourcePaths[0]),
            Resources.Load<Texture2D>(RequiredResourcePaths[1]),
            Resources.Load<Texture2D>(RequiredResourcePaths[2]),
            Resources.Load<Texture2D>(RequiredResourcePaths[3]),
            Resources.Load<Texture2D>(RequiredResourcePaths[4]),
            Resources.Load<Texture2D>(RequiredResourcePaths[5]));
    }

    public Texture2D Hit(bool sweetSpot)
    {
        return sweetSpot && HitSweet != null ? HitSweet : HitSolid;
    }

    public Texture2D Skill(Color accent)
    {
        if (IsViolet(accent) && TrickSpin != null)
        {
            return TrickSpin;
        }

        if (IsCyan(accent) && DashBurst != null)
        {
            return DashBurst;
        }

        return DashBurst != null ? DashBurst : TrickSpin;
    }

    private static bool IsCyan(Color color)
    {
        return color.b >= color.r && color.g >= color.r * 0.85f;
    }

    private static bool IsViolet(Color color)
    {
        return color.b >= color.g && color.r >= color.g * 0.65f;
    }
}
