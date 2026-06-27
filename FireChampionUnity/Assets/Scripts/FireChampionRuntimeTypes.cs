using System;
using System.Globalization;
using UnityEngine;

public enum ScreenState
{
    MainMenu,
    Settings,
    Network,
    Playing,
    Summary
}

public enum GameMode
{
    QuickAi,
    LocalPvp,
    Tournament,
    Practice,
    Tutorial,
    NetworkHost,
    NetworkClient
}

public enum Side
{
    Left,
    Right
}

public enum ShotType
{
    High,
    Flat,
    Smash,
    Drop
}

[Serializable]
public sealed class RulesetConfig
{
    public int pointsToWin = 7;
    public int winBy = 2;
    public int hardCap = 11;
    public int bestOf = 1;
    public bool outOfBounds = true;
    public bool abilitiesEnabled;
    public bool courtModifiersEnabled;
    public float ballSpeedMultiplier = 1.0f;
    public float energyMultiplier = 1.0f;

    public static RulesetConfig Standard()
    {
        return new RulesetConfig();
    }

    public static RulesetConfig Tournament()
    {
        RulesetConfig cfg = new RulesetConfig();
        cfg.bestOf = 3;
        cfg.abilitiesEnabled = true;
        cfg.courtModifiersEnabled = true;
        return cfg;
    }

    public static RulesetConfig Practice()
    {
        RulesetConfig cfg = new RulesetConfig();
        cfg.abilitiesEnabled = true;
        cfg.courtModifiersEnabled = true;
        cfg.bestOf = 1;
        return cfg;
    }

    public static RulesetConfig Tutorial()
    {
        RulesetConfig cfg = Practice();
        cfg.ballSpeedMultiplier = 0.86f;
        cfg.energyMultiplier = 2.2f;
        return cfg;
    }

    public RulesetConfig Copy()
    {
        RulesetConfig cfg = new RulesetConfig();
        cfg.pointsToWin = pointsToWin;
        cfg.winBy = winBy;
        cfg.hardCap = hardCap;
        cfg.bestOf = bestOf;
        cfg.outOfBounds = outOfBounds;
        cfg.abilitiesEnabled = abilitiesEnabled;
        cfg.courtModifiersEnabled = courtModifiersEnabled;
        cfg.ballSpeedMultiplier = ballSpeedMultiplier;
        cfg.energyMultiplier = energyMultiplier;
        return cfg;
    }
}

[Serializable]
public sealed class CharacterConfig
{
    public string code;
    public string displayName;
    public string role;
    public string descriptionText;
    public string playStyleText;
    public string differenceText;
    public string abilityBreakdownText;
    public string passiveText;
    public string skillText;
    public string ultimateText;
    public string strengthsText;
    public string weaknessText;
    public string recommendedText;
    public string statText;
    public float moveSpeed;
    public float jumpForce;
    public float hitRadius;
    public float smashBonus;
    public float smashEndLag;
    public float controlBonus;
    public float visualBodyScale = 1.0f;
    public float visualLimbScale = 1.0f;
    public float visualHeadScale = 1.0f;
    public float jerseyWhiteMix = 0.16f;
    public Color accent;

    public CharacterConfig(string code, string displayName, string role, string descriptionText, string playStyleText, string differenceText, string abilityBreakdownText, string passiveText, string skillText, string ultimateText, string statText, float moveSpeed, float jumpForce, float hitRadius, float smashBonus, float smashEndLag, float controlBonus, Color accent, string strengthsText = "", string weaknessText = "", string recommendedText = "", float visualBodyScale = 1.0f, float visualLimbScale = 1.0f, float visualHeadScale = 1.0f, float jerseyWhiteMix = 0.16f)
    {
        this.code = code;
        this.displayName = displayName;
        this.role = role;
        this.descriptionText = descriptionText;
        this.playStyleText = playStyleText;
        this.differenceText = differenceText;
        this.abilityBreakdownText = abilityBreakdownText;
        this.passiveText = passiveText;
        this.skillText = skillText;
        this.ultimateText = ultimateText;
        this.strengthsText = strengthsText;
        this.weaknessText = weaknessText;
        this.recommendedText = recommendedText;
        this.statText = statText;
        this.moveSpeed = moveSpeed;
        this.jumpForce = jumpForce;
        this.hitRadius = hitRadius;
        this.smashBonus = smashBonus;
        this.smashEndLag = smashEndLag;
        this.controlBonus = controlBonus;
        this.accent = accent;
        this.visualBodyScale = visualBodyScale;
        this.visualLimbScale = visualLimbScale;
        this.visualHeadScale = visualHeadScale;
        this.jerseyWhiteMix = jerseyWhiteMix;
    }

    public static CharacterConfig[] CreateDefaultRoster()
    {
        return FireChampionBalanceConfig.CreateDefault().CreateCharacterRoster();
    }
}

[Serializable]
public sealed class CourtConfig
{
    public string name;
    public Color background;
    public Color accent;
    public float windX;
    public float windY;
    public float period;

    public CourtConfig(string name, Color background, Color accent, float windX, float windY, float period)
    {
        this.name = name;
        this.background = background;
        this.accent = accent;
        this.windX = windX;
        this.windY = windY;
        this.period = period;
    }

    public static CourtConfig[] CreateDefaultCourts()
    {
        return FireChampionBalanceConfig.CreateDefault().CreateCourtConfigs();
    }
}

[Serializable]
public sealed class CosmeticConfig
{
    public string id;
    public string displayName;
    public string description;
    public string unlockLabel;
    public Color tint;
    public float tintBlend;

    public CosmeticConfig(string id, string displayName, string description, Color tint, string unlockLabel = "默认可用", float tintBlend = 0.72f)
    {
        this.id = id;
        this.displayName = displayName;
        this.description = description;
        this.unlockLabel = unlockLabel;
        this.tint = tint;
        this.tintBlend = tintBlend;
    }

    public static CosmeticConfig[] CreateDefaultCosmetics()
    {
        return FireChampionBalanceConfig.CreateDefault().CreateCosmetics();
    }
}

public sealed class PlayerActor
{
    public Side side;
    public int characterIndex;
    public float x;
    public float y;
    public float vy;
    public float moveSpeed;
    public float jumpForce;
    public float energy;
    public float swingTimer;
    public float swingActiveTimer;
    public float swingBufferTimer;
    public float swingCooldown;
    public int bufferedSwingVertical;
    public bool bufferedSwingSkillHeld;
    public bool bufferedSwingDropIntent;
    public float skillTimer;
    public float skillCooldown;
    public float ultimateTimer;
    public float endLag;
    public int facing;
    public bool grounded = true;
    public bool swingConsumed;
    public bool isAi;
    public float aiDifficulty;
    public float aiThinkTimer;
    public float aiTargetX;

    public PlayerActor(Side side, int characterIndex, float startX)
    {
        this.side = side;
        this.characterIndex = characterIndex;
        facing = side == Side.Left ? 1 : -1;
        ResetForPoint(startX);
    }

    public void ResetForPoint(float startX)
    {
        x = startX;
        y = 0;
        vy = 0;
        grounded = true;
        energy = 0;
        swingTimer = 0;
        swingActiveTimer = 0;
        swingBufferTimer = 0;
        swingCooldown = 0;
        bufferedSwingVertical = 0;
        bufferedSwingSkillHeld = false;
        bufferedSwingDropIntent = false;
        swingConsumed = false;
        skillTimer = 0;
        skillCooldown = 0;
        ultimateTimer = 0;
        endLag = 0;
        aiTargetX = startX;
    }
}

public struct ShuttleState
{
    public Vector2 position;
    public Vector2 velocity;
    public float spin;
}

public struct GameInput
{
    public int horizontal;
    public int vertical;
    public bool jumpPressed;
    public bool swingPressed;
    public bool skillPressed;
    public bool skillHeld;
    public bool dropIntent;

    public string Encode()
    {
        return horizontal + "," + vertical + "," + (jumpPressed ? 1 : 0) + "," + (swingPressed ? 1 : 0) + "," + (skillPressed ? 1 : 0) + "," + (skillHeld ? 1 : 0) + "," + (dropIntent ? 1 : 0);
    }

    public static GameInput Decode(string value)
    {
        GameInput input = new GameInput();
        string[] parts = value.Split(',');
        if (parts.Length >= 6)
        {
            int.TryParse(parts[0], out input.horizontal);
            int.TryParse(parts[1], out input.vertical);
            input.jumpPressed = parts[2] == "1";
            input.swingPressed = parts[3] == "1";
            input.skillPressed = parts[4] == "1";
            input.skillHeld = parts[5] == "1";
            if (parts.Length >= 7)
            {
                input.dropIntent = parts[6] == "1";
            }
        }

        return input;
    }
}

public sealed class MatchStats
{
    public int rallyHits;
    public int longestRally;
    public int smashes;
    public int smashWinners;
    public int errors;
    public int skillsUsed;
    public int serveFaults;
}

public sealed class GameSnapshot
{
    public float leftX;
    public float leftY;
    public int leftFacing;
    public float rightX;
    public float rightY;
    public int rightFacing;
    public float ballX;
    public float ballY;
    public float ballVx;
    public float ballVy;
    public float ballSpin;
    public int leftScore;
    public int rightScore;
    public float leftEnergy;
    public float rightEnergy;
    public bool waitingServe;
    public int server;
    public int leftGames;
    public int rightGames;
    public bool matchOver;
    public string summaryTitle = "";
    public string summaryDetails = "";
    public bool hasMetadata;
    public int leftCharacter;
    public int rightCharacter;
    public int selectedCourt;
    public int leftCosmetic;
    public int rightCosmetic;
    public bool abilitiesEnabled;
    public bool courtModifiersEnabled;
    public int pointsToWin;
    public int winBy;
    public int hardCap;
    public int bestOf;
    public bool outOfBounds;

    public string Encode()
    {
        CultureInfo c = CultureInfo.InvariantCulture;
        return string.Join("|", new string[]
        {
            leftX.ToString(c), leftY.ToString(c), leftFacing.ToString(c),
            rightX.ToString(c), rightY.ToString(c), rightFacing.ToString(c),
            ballX.ToString(c), ballY.ToString(c), ballVx.ToString(c), ballVy.ToString(c),
            leftScore.ToString(c), rightScore.ToString(c),
            leftEnergy.ToString(c), rightEnergy.ToString(c),
            waitingServe ? "1" : "0", server.ToString(c),
            leftGames.ToString(c), rightGames.ToString(c),
            matchOver ? "1" : "0", Escape(summaryTitle), Escape(summaryDetails),
            leftCharacter.ToString(c), rightCharacter.ToString(c), selectedCourt.ToString(c),
            abilitiesEnabled ? "1" : "0", courtModifiersEnabled ? "1" : "0", ballSpin.ToString(c),
            leftCosmetic.ToString(c), rightCosmetic.ToString(c),
            pointsToWin.ToString(c), winBy.ToString(c), hardCap.ToString(c), bestOf.ToString(c),
            outOfBounds ? "1" : "0"
        });
    }

    public static bool TryDecode(string packet, out GameSnapshot snapshot)
    {
        snapshot = new GameSnapshot();
        string[] p = packet.Split('|');
        if (p.Length < 16)
        {
            return false;
        }

        CultureInfo c = CultureInfo.InvariantCulture;
        bool ok = float.TryParse(p[0], NumberStyles.Float, c, out snapshot.leftX)
            && float.TryParse(p[1], NumberStyles.Float, c, out snapshot.leftY)
            && int.TryParse(p[2], out snapshot.leftFacing)
            && float.TryParse(p[3], NumberStyles.Float, c, out snapshot.rightX)
            && float.TryParse(p[4], NumberStyles.Float, c, out snapshot.rightY)
            && int.TryParse(p[5], out snapshot.rightFacing)
            && float.TryParse(p[6], NumberStyles.Float, c, out snapshot.ballX)
            && float.TryParse(p[7], NumberStyles.Float, c, out snapshot.ballY)
            && float.TryParse(p[8], NumberStyles.Float, c, out snapshot.ballVx)
            && float.TryParse(p[9], NumberStyles.Float, c, out snapshot.ballVy)
            && int.TryParse(p[10], out snapshot.leftScore)
            && int.TryParse(p[11], out snapshot.rightScore)
            && float.TryParse(p[12], NumberStyles.Float, c, out snapshot.leftEnergy)
            && float.TryParse(p[13], NumberStyles.Float, c, out snapshot.rightEnergy)
            && TryParseBool01(p[14], out snapshot.waitingServe)
            && int.TryParse(p[15], out snapshot.server);

        if (!ok)
        {
            return false;
        }

        if (p.Length >= 21)
        {
            ok = int.TryParse(p[16], out snapshot.leftGames)
                && int.TryParse(p[17], out snapshot.rightGames)
                && TryParseBool01(p[18], out snapshot.matchOver);
            if (!ok)
            {
                return false;
            }

            snapshot.summaryTitle = Unescape(p[19]);
            snapshot.summaryDetails = Unescape(p[20]);
        }

        if (p.Length >= 34)
        {
            ok = int.TryParse(p[21], out snapshot.leftCharacter)
                && int.TryParse(p[22], out snapshot.rightCharacter)
                && int.TryParse(p[23], out snapshot.selectedCourt)
                && TryParseBool01(p[24], out snapshot.abilitiesEnabled)
                && TryParseBool01(p[25], out snapshot.courtModifiersEnabled)
                && float.TryParse(p[26], NumberStyles.Float, c, out snapshot.ballSpin)
                && int.TryParse(p[27], out snapshot.leftCosmetic)
                && int.TryParse(p[28], out snapshot.rightCosmetic)
                && int.TryParse(p[29], out snapshot.pointsToWin)
                && int.TryParse(p[30], out snapshot.winBy)
                && int.TryParse(p[31], out snapshot.hardCap)
                && int.TryParse(p[32], out snapshot.bestOf)
                && TryParseBool01(p[33], out snapshot.outOfBounds);
            if (!ok)
            {
                return false;
            }

            snapshot.hasMetadata = true;
        }

        return true;
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value.Replace("%", "%25").Replace("|", "%7C").Replace("\n", "%0A").Replace("\r", "%0D");
    }

    private static string Unescape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value.Replace("%0D", "\r").Replace("%0A", "\n").Replace("%7C", "|").Replace("%25", "%");
    }

    private static bool TryParseBool01(string value, out bool result)
    {
        if (value == "1")
        {
            result = true;
            return true;
        }

        if (value == "0")
        {
            result = false;
            return true;
        }

        result = false;
        return false;
    }
}
