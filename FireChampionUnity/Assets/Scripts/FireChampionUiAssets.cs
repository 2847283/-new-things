using UnityEngine;

public sealed class FireChampionUiAssets
{
    private readonly Texture2D[] roleIcons;
    private readonly Texture2D[] courtBadges;

    public Texture2D Logo { get; private set; }

    private FireChampionUiAssets(Texture2D logo, Texture2D[] roleIcons, Texture2D[] courtBadges)
    {
        Logo = logo;
        this.roleIcons = roleIcons;
        this.courtBadges = courtBadges;
    }

    public static FireChampionUiAssets Load()
    {
        Texture2D logo = Resources.Load<Texture2D>("FireChampion/UI/logo_fire_champion");
        Texture2D[] loadedRoleIcons = new Texture2D[]
        {
            Resources.Load<Texture2D>("FireChampion/UI/role_core"),
            Resources.Load<Texture2D>("FireChampion/UI/role_dash"),
            Resources.Load<Texture2D>("FireChampion/UI/role_heavy"),
            Resources.Load<Texture2D>("FireChampion/UI/role_trick")
        };
        Texture2D[] loadedCourtBadges = new Texture2D[]
        {
            Resources.Load<Texture2D>("FireChampion/UI/court_dojo"),
            Resources.Load<Texture2D>("FireChampion/UI/court_rooftop"),
            Resources.Load<Texture2D>("FireChampion/UI/court_future")
        };
        return new FireChampionUiAssets(logo, loadedRoleIcons, loadedCourtBadges);
    }

    public Texture2D RoleIcon(int index)
    {
        if (roleIcons == null || index < 0 || index >= roleIcons.Length)
        {
            return null;
        }

        return roleIcons[index];
    }

    public Texture2D CourtBadge(int index)
    {
        if (courtBadges == null || index < 0 || index >= courtBadges.Length)
        {
            return null;
        }

        return courtBadges[index];
    }
}
