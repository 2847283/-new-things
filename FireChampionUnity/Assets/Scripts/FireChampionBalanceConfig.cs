using System;
using UnityEngine;

public sealed class FireChampionBalanceLoadReport
{
    public FireChampionBalanceConfig config;
    public bool resourceFound;
    public bool loadedFromResource;
    public bool usedFallback;
    public string resourcePath;
    public string message;
    public int textLength;

    public bool IsValidResourceLoad
    {
        get { return config != null && resourceFound && loadedFromResource && !usedFallback; }
    }
}

[Serializable]
public sealed class FireChampionBalanceConfig
{
    public const string ResourcePath = "FireChampion/Data/fire_champion_balance";

    public BalanceRules rules = new BalanceRules();
    public BalanceAi ai = new BalanceAi();
    public BalanceNetwork network = new BalanceNetwork();
    public BalanceGameplay gameplay = new BalanceGameplay();
    public BalanceCharacter[] characters = new BalanceCharacter[0];
    public BalanceCourt[] courts = new BalanceCourt[0];
    public BalanceCosmetic[] cosmetics = new BalanceCosmetic[0];

    public static FireChampionBalanceConfig Load()
    {
        FireChampionBalanceLoadReport report = LoadWithReport(true);
        if (report.usedFallback && !string.IsNullOrEmpty(report.message))
        {
            Debug.LogWarning("Fire Champion balance config fallback: " + report.message);
        }

        return report.config;
    }

    public static FireChampionBalanceLoadReport LoadWithReport(bool allowFallback)
    {
        FireChampionBalanceLoadReport report = new FireChampionBalanceLoadReport();
        report.resourcePath = ResourcePath;
        TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
        if (asset == null || string.IsNullOrWhiteSpace(asset.text))
        {
            return CompleteLoadFailure(report, asset == null ? "Resource not found: " + ResourcePath : "Resource text is empty: " + ResourcePath, allowFallback);
        }

        report.resourceFound = true;
        report.textLength = asset.text.Length;
        try
        {
            FireChampionBalanceConfig config = JsonUtility.FromJson<FireChampionBalanceConfig>(asset.text);
            if (config == null)
            {
                return CompleteLoadFailure(report, "JsonUtility returned null for " + ResourcePath, allowFallback);
            }

            config.Normalize();
            report.config = config;
            report.loadedFromResource = true;
            report.usedFallback = false;
            report.message = "Loaded from Resources/" + ResourcePath + " (" + report.textLength + " chars).";
            return report;
        }
        catch (Exception ex)
        {
            return CompleteLoadFailure(report, "JSON parse failed for " + ResourcePath + ": " + ex.Message, allowFallback);
        }
    }

    private static FireChampionBalanceLoadReport CompleteLoadFailure(FireChampionBalanceLoadReport report, string message, bool allowFallback)
    {
        report.loadedFromResource = false;
        report.message = message;
        if (allowFallback)
        {
            report.config = CreateDefault();
            report.usedFallback = true;
        }

        return report;
    }

    public static FireChampionBalanceConfig CreateDefault()
    {
        FireChampionBalanceConfig config = new FireChampionBalanceConfig();
        config.rules = BalanceRules.CreateDefault();
        config.ai = new BalanceAi();
        config.network = new BalanceNetwork();
        config.gameplay = new BalanceGameplay();
        config.characters = new BalanceCharacter[]
        {
            new BalanceCharacter
            {
                code = "CORE",
                displayName = "稳定核心",
                role = "均衡/容错",
                descriptionText = "全能型选手，适合先把站位、回合和精准击球练稳。",
                playStyleText = "用高远球和稳定平抽拖住回合，等对手失位后再下压。",
                differenceText = "甜区最大，精准击球回能更快；速度和扣杀保持标准。",
                abilityBreakdownText = "移速中等，跳跃中上，甜区最大，控球偏强；没有极端爆发，但最不容易空拍。",
                passiveText = "击球甜区大，精准击球回能略高。",
                skillText = "专注架势：短时间扩大接球范围。",
                ultimateText = "完美姿态：下一拍更稳，扣杀也可强化。",
                strengthsText = "容错最高，救球稳定，适合拉长回合和练基础。",
                weaknessText = "爆发和抢点都不极端，遇到高压快攻需要提前站位。",
                recommendedText = "推荐新手、稳健型玩家、想先掌握手感的玩家。",
                statText = "速度★★★  扣杀★★★  控球★★★★  容错★★★★★",
                moveSpeed = 5.45f,
                jumpForce = 6.55f,
                hitRadius = 0.88f,
                smashBonus = 0.0f,
                smashEndLag = 0.02f,
                controlBonus = 0.35f,
                visualBodyScale = 1.0f,
                visualLimbScale = 1.0f,
                visualHeadScale = 1.0f,
                jerseyWhiteMix = 0.44f,
                accent = BalanceColor.FromColor(new Color(0.95f, 0.95f, 0.95f, 1.0f))
            },
            new BalanceCharacter
            {
                code = "DASH",
                displayName = "疾风突击",
                role = "速度/抢点",
                descriptionText = "快攻型选手，靠启动速度抢早点击球并连续压迫。",
                playStyleText = "提前站位抢平抽，技能冲到球前，用低冷却维持节奏。",
                differenceText = "移动最快但甜区最小，扣杀略弱；技能期间平抽更快。",
                abilityBreakdownText = "移速最高，后摇最短，追球最强；甜区最小、扣杀偏弱，失位后容错最低。",
                passiveText = "移动最快，但击球甜区较小。",
                skillText = "瞬步：向面朝方向冲刺并短暂加速。",
                ultimateText = "闪电节奏：短时间挥拍冷却大幅降低。",
                strengthsText = "启动最快，抢网前和追平抽最强，能连续压缩对手时间。",
                weaknessText = "甜区小、力量略低，乱挥会更容易失误。",
                recommendedText = "推荐喜欢主动进攻、压节奏、打快球的玩家。",
                statText = "速度★★★★★  扣杀★★★  控球★★  容错★★",
                moveSpeed = 6.35f,
                jumpForce = 6.35f,
                hitRadius = 0.78f,
                smashBonus = -0.2f,
                smashEndLag = 0.0f,
                controlBonus = 0.05f,
                visualBodyScale = 0.92f,
                visualLimbScale = 0.90f,
                visualHeadScale = 0.96f,
                jerseyWhiteMix = 0.10f,
                accent = BalanceColor.FromColor(new Color(0.35f, 0.85f, 1.0f, 1.0f))
            },
            new BalanceCharacter
            {
                code = "HEAVY",
                displayName = "重炮守门",
                role = "力量/压制",
                descriptionText = "重炮型选手，牺牲机动性换取最强扣杀威胁。",
                playStyleText = "先用稳健防守等高球，抓到半场球后用重扣终结。",
                differenceText = "速度最低、后摇最长，但扣杀最快最沉，下压角度更凶。",
                abilityBreakdownText = "移速最低，跳跃偏低，扣杀加成最高；击球后摇最长，强在一拍终结。",
                passiveText = "扣杀最重，移动慢且后摇更长。",
                skillText = "铁腕蓄力：短时间扩大击球并强化扣杀。",
                ultimateText = "雷霆扣杀：满能量扣杀速度和下压更强。",
                strengthsText = "终结能力最高，防守反击和半场高球惩罚最强。",
                weaknessText = "转身和补位慢，扣杀后摇长，落点判断失误会被调动。",
                recommendedText = "推荐喜欢一锤定音、等机会重扣的玩家。",
                statText = "速度★★  扣杀★★★★★  控球★★  容错★★★",
                moveSpeed = 4.85f,
                jumpForce = 6.15f,
                hitRadius = 0.82f,
                smashBonus = 1.45f,
                smashEndLag = 0.14f,
                controlBonus = -0.1f,
                visualBodyScale = 1.18f,
                visualLimbScale = 1.14f,
                visualHeadScale = 1.02f,
                jerseyWhiteMix = 0.12f,
                accent = BalanceColor.FromColor(new Color(1.0f, 0.52f, 0.28f, 1.0f))
            },
            new BalanceCharacter
            {
                code = "TRICK",
                displayName = "幻线路师",
                role = "控球/骗位",
                descriptionText = "变化型选手，利用短吊、旋转和高跳制造反向跑动。",
                playStyleText = "多用吊球和高远球调动对手，满能量时用旋转改变落点。",
                differenceText = "跳跃最高、控球最高、旋转最强；基础甜区偏小。",
                abilityBreakdownText = "跳跃最高，控球加成最高，落点变化最多；扣杀普通，依赖连续调动和预判。",
                passiveText = "控球加成最高，平抽和吊球更刁钻。",
                skillText = "假动作：下一拍优先变成短吊。",
                ultimateText = "旋羽幻线：下一拍附加强旋转和抬升。",
                strengthsText = "线路变化最多，短吊、高远和旋转能迫使对手反复跑动。",
                weaknessText = "直接终结能力不如重炮，依赖预判和连续调动。",
                recommendedText = "推荐喜欢骗位、控落点、打策略回合的玩家。",
                statText = "速度★★★  扣杀★★★  控球★★★★★  容错★★★",
                moveSpeed = 5.35f,
                jumpForce = 6.8f,
                hitRadius = 0.8f,
                smashBonus = 0.15f,
                smashEndLag = 0.04f,
                controlBonus = 0.8f,
                visualBodyScale = 0.98f,
                visualLimbScale = 0.95f,
                visualHeadScale = 1.0f,
                jerseyWhiteMix = 0.20f,
                accent = BalanceColor.FromColor(new Color(0.75f, 0.65f, 1.0f, 1.0f))
            }
        };
        config.courts = new BalanceCourt[]
        {
            new BalanceCourt { name = "道场", background = BalanceColor.FromColor(new Color(0.08f, 0.075f, 0.06f, 1.0f)), accent = BalanceColor.FromColor(new Color(1.0f, 0.86f, 0.45f, 1.0f)), windX = 0.04f, windY = 0.02f, period = 1.4f },
            new BalanceCourt { name = "屋顶", background = BalanceColor.FromColor(new Color(0.045f, 0.06f, 0.075f, 1.0f)), accent = BalanceColor.FromColor(new Color(0.6f, 0.9f, 1.0f, 1.0f)), windX = 0.16f, windY = 0.03f, period = 0.9f },
            new BalanceCourt { name = "未来场", background = BalanceColor.FromColor(new Color(0.035f, 0.035f, 0.055f, 1.0f)), accent = BalanceColor.FromColor(new Color(0.95f, 0.45f, 1.0f, 1.0f)), windX = 0.08f, windY = 0.09f, period = 1.8f }
        };
        config.cosmetics = new BalanceCosmetic[]
        {
            new BalanceCosmetic { id = "default", displayName = "默认队服", description = "使用角色原本配色。", unlockLabel = "默认可用", tintBlend = 0.0f, tint = BalanceColor.FromColor(Color.white) },
            new BalanceCosmetic { id = "flame", displayName = "火羽红", description = "红金运动队服，只改变视觉色彩。", unlockLabel = "外观预留", tintBlend = 0.72f, tint = BalanceColor.FromColor(new Color(1.0f, 0.28f, 0.16f, 1.0f)) },
            new BalanceCosmetic { id = "aqua", displayName = "霓虹蓝", description = "蓝青运动队服，只改变视觉色彩。", unlockLabel = "外观预留", tintBlend = 0.72f, tint = BalanceColor.FromColor(new Color(0.22f, 0.85f, 1.0f, 1.0f)) },
            new BalanceCosmetic { id = "violet", displayName = "幻紫光", description = "紫色运动队服，只改变视觉色彩。", unlockLabel = "外观预留", tintBlend = 0.72f, tint = BalanceColor.FromColor(new Color(0.8f, 0.48f, 1.0f, 1.0f)) }
        };
        return config;
    }

    public CharacterConfig[] CreateCharacterRoster()
    {
        CharacterConfig[] roster = new CharacterConfig[characters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            BalanceCharacter item = characters[i];
            roster[i] = item.ToCharacterConfig();
        }

        return roster;
    }

    public CourtConfig[] CreateCourtConfigs()
    {
        CourtConfig[] result = new CourtConfig[courts.Length];
        for (int i = 0; i < courts.Length; i++)
        {
            BalanceCourt item = courts[i];
            result[i] = new CourtConfig(item.name, item.background.ToColor(new Color(0.08f, 0.075f, 0.06f, 1.0f)), item.accent.ToColor(new Color(1.0f, 0.86f, 0.45f, 1.0f)), item.windX, item.windY, item.period);
        }

        return result;
    }

    public CosmeticConfig[] CreateCosmetics()
    {
        CosmeticConfig[] result = new CosmeticConfig[cosmetics.Length];
        for (int i = 0; i < cosmetics.Length; i++)
        {
            BalanceCosmetic item = cosmetics[i];
            result[i] = item.ToCosmeticConfig();
        }

        return result;
    }

    public RulesetConfig StandardRules()
    {
        return rules.standard.ToRuleset();
    }

    public RulesetConfig TournamentRules()
    {
        return rules.tournament.ToRuleset();
    }

    public RulesetConfig PracticeRules()
    {
        return rules.practice.ToRuleset();
    }

    public RulesetConfig TutorialRules()
    {
        return rules.tutorial.ToRuleset();
    }

    private void Normalize()
    {
        FireChampionBalanceConfig defaults = CreateDefault();
        if (rules == null) rules = defaults.rules;
        rules.Normalize(defaults.rules);
        if (ai == null) ai = defaults.ai;
        ai.Normalize(defaults.ai);
        if (network == null) network = defaults.network;
        network.Normalize(defaults.network);
        if (gameplay == null) gameplay = defaults.gameplay;
        gameplay.Normalize(defaults.gameplay);
        if (characters == null || characters.Length == 0) characters = defaults.characters;
        for (int i = 0; i < characters.Length; i++)
        {
            BalanceCharacter fallback = FindDefaultCharacter(defaults.characters, characters[i] == null ? null : characters[i].code, i);
            if (characters[i] == null)
            {
                characters[i] = fallback;
            }
            else
            {
                characters[i].Normalize(fallback);
            }
        }

        if (courts == null || courts.Length == 0) courts = defaults.courts;
        if (cosmetics == null || cosmetics.Length == 0) cosmetics = defaults.cosmetics;
        for (int i = 0; i < cosmetics.Length; i++)
        {
            BalanceCosmetic fallback = i < defaults.cosmetics.Length ? defaults.cosmetics[i] : defaults.cosmetics[0];
            if (cosmetics[i] == null)
            {
                cosmetics[i] = fallback;
            }
            else
            {
                cosmetics[i].Normalize(fallback);
            }
        }
    }

    private static BalanceCharacter FindDefaultCharacter(BalanceCharacter[] defaults, string code, int index)
    {
        if (defaults == null || defaults.Length == 0)
        {
            return new BalanceCharacter();
        }

        for (int i = 0; i < defaults.Length; i++)
        {
            if (string.Equals(defaults[i].code, code, StringComparison.OrdinalIgnoreCase))
            {
                return defaults[i];
            }
        }

        return defaults[Mathf.Clamp(index, 0, defaults.Length - 1)];
    }
}

[Serializable]
public sealed class BalanceGameplay
{
    public BalanceCourtPhysics court = new BalanceCourtPhysics();
    public BalancePracticeTuning practice = new BalancePracticeTuning();
    public BalanceEnergyTuning energy = new BalanceEnergyTuning();
    public BalanceSkillTuning skill = new BalanceSkillTuning();
    public BalanceSwingTuning swing = new BalanceSwingTuning();
    public BalanceShotTuning shot = new BalanceShotTuning();
    public BalanceAudioTuning audio = new BalanceAudioTuning();
    public BalanceFeedbackTuning feedback = new BalanceFeedbackTuning();
    public BalanceVisualTuning visuals = new BalanceVisualTuning();

    public void Normalize(BalanceGameplay defaults)
    {
        if (court == null) court = defaults.court;
        if (practice == null) practice = defaults.practice;
        practice.Normalize(defaults.practice);
        if (energy == null) energy = defaults.energy;
        if (skill == null) skill = defaults.skill;
        if (swing == null) swing = defaults.swing;
        if (shot == null) shot = defaults.shot;
        if (audio == null) audio = defaults.audio;
        audio.Normalize(defaults.audio);
        if (feedback == null) feedback = defaults.feedback;
        feedback.Normalize(defaults.feedback);
        if (visuals == null) visuals = defaults.visuals;
        visuals.Normalize(defaults.visuals);
    }
}

[Serializable]
public sealed class BalanceCourtPhysics
{
    public float courtHalfWidth = 8.0f;
    public float groundY = 0.0f;
    public float netHeight = 1.25f;
    public float netWidth = 0.12f;
    public float gravity = -9.4f;
    public float shuttleDrag = 0.72f;
    public float maxFrameDelta = 0.033f;
    public float networkSendInterval = 0.05f;
    public float playerSpawnX = 5.6f;
    public float playerBackMargin = 0.6f;
    public float centerGuardMargin = 0.65f;
    public float outOfBoundsMargin = 0.18f;
    public float gravityMultiplier = 1.2f;
    public float spinInfluenceX = 0.95f;
    public float spinInfluenceY = 0.08f;
    public float spinDecay = 0.7f;
    public float spinActiveThreshold = 0.01f;
    public float predictionStep = 0.025f;
    public int predictionIterations = 160;
    public float predictionGroundY = 0.35f;
    public float predictionInnerClamp = 1.0f;
    public float predictionBackMargin = 0.8f;
}

[Serializable]
public sealed class BalancePracticeTuning
{
    public float defaultBallSpeed = 1.0f;
    public float defaultEnergyMultiplier = 1.0f;
    public float ballSpeedMin = 0.5f;
    public float ballSpeedMax = 1.8f;
    public float energyMultiplierMin = 0.5f;
    public float energyMultiplierMax = 3.0f;
    public int timelineCapacity = 6;

    public void Normalize(BalancePracticeTuning defaults)
    {
        if (defaultBallSpeed <= 0.0f) defaultBallSpeed = defaults.defaultBallSpeed;
        if (defaultEnergyMultiplier <= 0.0f) defaultEnergyMultiplier = defaults.defaultEnergyMultiplier;
        if (ballSpeedMin <= 0.0f) ballSpeedMin = defaults.ballSpeedMin;
        if (ballSpeedMax < ballSpeedMin) ballSpeedMax = defaults.ballSpeedMax;
        if (energyMultiplierMin <= 0.0f) energyMultiplierMin = defaults.energyMultiplierMin;
        if (energyMultiplierMax < energyMultiplierMin) energyMultiplierMax = defaults.energyMultiplierMax;
        if (timelineCapacity <= 0) timelineCapacity = defaults.timelineCapacity;
    }
}

[Serializable]
public sealed class BalanceAudioTuning
{
    public BalanceToneCue ultimate = new BalanceToneCue(560.0f, 0.08f, 1.0f);
    public BalanceToneCue skill = new BalanceToneCue(460.0f, 0.06f, 1.0f);
    public BalanceToneCue serve = new BalanceToneCue(720.0f, 0.04f, 1.0f);
    public BalanceToneCue normalHit = new BalanceToneCue(820.0f, 0.04f, 1.0f);
    public BalanceToneCue smashHit = new BalanceToneCue(1080.0f, 0.055f, 1.0f);
    public BalanceToneCue scoreLeft = new BalanceToneCue(660.0f, 0.12f, 1.0f);
    public BalanceToneCue scoreRight = new BalanceToneCue(420.0f, 0.12f, 1.0f);

    public void Normalize(BalanceAudioTuning defaults)
    {
        if (ultimate == null) ultimate = defaults.ultimate;
        if (skill == null) skill = defaults.skill;
        if (serve == null) serve = defaults.serve;
        if (normalHit == null) normalHit = defaults.normalHit;
        if (smashHit == null) smashHit = defaults.smashHit;
        if (scoreLeft == null) scoreLeft = defaults.scoreLeft;
        if (scoreRight == null) scoreRight = defaults.scoreRight;
    }
}

[Serializable]
public sealed class BalanceFeedbackTuning
{
    public float bannerDuration = 1.45f;
    public float screenShakeFadeDuration = 0.22f;
    public float hitShakeMagnitude = 4.0f;
    public float hitShakeDuration = 0.12f;
    public float scoreShakeMagnitude = 3.0f;
    public float smashScoreShakeMagnitude = 6.0f;
    public float scoreShakeDuration = 0.18f;
    public float hitVfxDuration = 0.22f;
    public float scoreVfxDuration = 0.42f;
    public float skillVfxDuration = 0.34f;
    public float hitVfxRadius = 28.0f;
    public float smashVfxRadius = 44.0f;
    public float scoreVfxRadius = 54.0f;
    public float skillVfxRadius = 42.0f;
    public int maxVfxEvents = 48;

    public void Normalize(BalanceFeedbackTuning defaults)
    {
        if (bannerDuration <= 0.0f) bannerDuration = defaults.bannerDuration;
        if (screenShakeFadeDuration <= 0.0f) screenShakeFadeDuration = defaults.screenShakeFadeDuration;
        if (hitShakeDuration <= 0.0f) hitShakeDuration = defaults.hitShakeDuration;
        if (scoreShakeDuration <= 0.0f) scoreShakeDuration = defaults.scoreShakeDuration;
        if (hitShakeMagnitude < 0.0f) hitShakeMagnitude = defaults.hitShakeMagnitude;
        if (scoreShakeMagnitude < 0.0f) scoreShakeMagnitude = defaults.scoreShakeMagnitude;
        if (smashScoreShakeMagnitude < 0.0f) smashScoreShakeMagnitude = defaults.smashScoreShakeMagnitude;
        if (hitVfxDuration <= 0.0f) hitVfxDuration = defaults.hitVfxDuration;
        if (scoreVfxDuration <= 0.0f) scoreVfxDuration = defaults.scoreVfxDuration;
        if (skillVfxDuration <= 0.0f) skillVfxDuration = defaults.skillVfxDuration;
        if (hitVfxRadius <= 0.0f) hitVfxRadius = defaults.hitVfxRadius;
        if (smashVfxRadius < hitVfxRadius) smashVfxRadius = defaults.smashVfxRadius;
        if (scoreVfxRadius <= 0.0f) scoreVfxRadius = defaults.scoreVfxRadius;
        if (skillVfxRadius <= 0.0f) skillVfxRadius = defaults.skillVfxRadius;
        if (maxVfxEvents < 4) maxVfxEvents = defaults.maxVfxEvents;
    }
}

[Serializable]
public sealed class BalanceVisualTuning
{
    public float courtSurfaceTopY = 2.8f;
    public int courtStripeCount = 7;
    public float courtTopHighlightAlpha = 0.035f;
    public float courtStripeAlpha = 0.055f;
    public float courtAlternateStripeAlpha = 0.025f;
    public float courtLineShadowAlpha = 0.18f;
    public float courtSoftLineAlpha = 0.38f;
    public float courtGlowAlpha = 0.34f;
    public float playerShadowWidth = 38.0f;
    public float playerShadowHeight = 10.0f;
    public float skillAuraWidth = 48.0f;
    public float skillAuraHeight = 14.0f;
    public float skillAuraAlpha = 0.32f;
    public float bodyWidth = 20.0f;
    public float limbWidth = 10.0f;
    public float headRadius = 18.0f;
    public float racketOuterRadiusX = 18.0f;
    public float racketOuterRadiusY = 24.0f;
    public float racketInnerRadiusX = 14.0f;
    public float racketInnerRadiusY = 20.0f;
    public float swingArcBaseRadius = 26.0f;
    public float swingArcTimerScale = 75.0f;

    public void Normalize(BalanceVisualTuning defaults)
    {
        if (courtSurfaceTopY <= 0.0f) courtSurfaceTopY = defaults.courtSurfaceTopY;
        if (courtStripeCount < 1) courtStripeCount = defaults.courtStripeCount;
        courtTopHighlightAlpha = ClampAlphaWithDefault(courtTopHighlightAlpha, defaults.courtTopHighlightAlpha);
        courtStripeAlpha = ClampAlphaWithDefault(courtStripeAlpha, defaults.courtStripeAlpha);
        courtAlternateStripeAlpha = ClampAlphaWithDefault(courtAlternateStripeAlpha, defaults.courtAlternateStripeAlpha);
        courtLineShadowAlpha = ClampAlphaWithDefault(courtLineShadowAlpha, defaults.courtLineShadowAlpha);
        courtSoftLineAlpha = ClampAlphaWithDefault(courtSoftLineAlpha, defaults.courtSoftLineAlpha);
        courtGlowAlpha = ClampAlphaWithDefault(courtGlowAlpha, defaults.courtGlowAlpha);
        if (playerShadowWidth <= 0.0f) playerShadowWidth = defaults.playerShadowWidth;
        if (playerShadowHeight <= 0.0f) playerShadowHeight = defaults.playerShadowHeight;
        if (skillAuraWidth <= 0.0f) skillAuraWidth = defaults.skillAuraWidth;
        if (skillAuraHeight <= 0.0f) skillAuraHeight = defaults.skillAuraHeight;
        skillAuraAlpha = ClampAlphaWithDefault(skillAuraAlpha, defaults.skillAuraAlpha);
        if (bodyWidth <= 0.0f) bodyWidth = defaults.bodyWidth;
        if (limbWidth <= 0.0f) limbWidth = defaults.limbWidth;
        if (headRadius <= 0.0f) headRadius = defaults.headRadius;
        if (racketOuterRadiusX <= 0.0f) racketOuterRadiusX = defaults.racketOuterRadiusX;
        if (racketOuterRadiusY <= 0.0f) racketOuterRadiusY = defaults.racketOuterRadiusY;
        if (racketInnerRadiusX <= 0.0f) racketInnerRadiusX = defaults.racketInnerRadiusX;
        if (racketInnerRadiusY <= 0.0f) racketInnerRadiusY = defaults.racketInnerRadiusY;
        if (swingArcBaseRadius <= 0.0f) swingArcBaseRadius = defaults.swingArcBaseRadius;
        if (swingArcTimerScale <= 0.0f) swingArcTimerScale = defaults.swingArcTimerScale;
    }

    private static float ClampAlphaWithDefault(float value, float fallback)
    {
        if (value <= 0.0f)
        {
            value = fallback;
        }

        return Mathf.Clamp01(value);
    }
}

[Serializable]
public sealed class BalanceToneCue
{
    public float frequency;
    public float duration;
    public float volumeMultiplier = 1.0f;

    public BalanceToneCue()
    {
    }

    public BalanceToneCue(float frequency, float duration, float volumeMultiplier)
    {
        this.frequency = frequency;
        this.duration = duration;
        this.volumeMultiplier = volumeMultiplier;
    }
}

[Serializable]
public sealed class BalanceEnergyTuning
{
    public float maxEnergy = 3.0f;
    public float skillCost = 1.0f;
    public float ultimateCost = 3.0f;
    public float passiveGainPerSecond = 0.055f;
    public float goodHitGain = 0.38f;
    public float normalHitGain = 0.2f;
    public float coreGoodHitBonus = 0.10f;
    public int longRallyHitThreshold = 8;
    public float longRallyBonus = 0.08f;
}

[Serializable]
public sealed class BalanceSkillTuning
{
    public float dashSpeedMultiplier = 1.32f;
    public float heavySpeedMultiplier = 0.88f;
    public float dashUltimateSpeedMultiplier = 1.12f;
    public float endLagMoveMultiplier = 0.42f;
    public float dashUltimateDuration = 3.8f;
    public float defaultUltimateDuration = 4.8f;
    public float ultimateCooldown = 0.6f;
    public float skillCooldown = 0.5f;
    public float dashSkillDuration = 1.35f;
    public float coreSkillDuration = 2.4f;
    public float heavySkillDuration = 2.1f;
    public float trickSkillDuration = 2.2f;
    public float dashStepDistance = 1.35f;
    public float coreEndLagReduction = 0.12f;
}

[Serializable]
public sealed class BalanceSwingTuning
{
    public float serveActiveWindow = 0.18f;
    public float serveCooldown = 0.18f;
    public float bufferTime = 0.09f;
    public float hitActiveWindow = 0.17f;
    public float hitCooldown = 0.24f;
    public float swingPoseTimer = 0.2f;
    public float coreSkillHitRadiusBonus = 0.22f;
    public float heavySkillHitRadiusBonus = 0.10f;
    public float coreUltimateHitRadiusBonus = 0.12f;
    public float sweetSpotRatio = 0.47f;
    public float dashUltimateCooldownMultiplier = 0.58f;
    public float coreSkillCooldownMultiplier = 0.88f;
    public float racketBaseReach = 0.62f;
    public float racketSwingReachBonus = 0.26f;
    public float racketHeight = 1.25f;
    public float racketSwingBobFrequency = 26.0f;
    public float racketSwingBobAmount = 0.05f;
    public float networkJumpPulseTime = 0.1f;
    public float networkSwingPulseTime = 0.12f;
    public float networkSkillPulseTime = 0.1f;
}

[Serializable]
public sealed class BalanceShotTuning
{
    public float aiServeChancePerSecond = 1.8f;
    public float serveHighLift = 5.6f;
    public float serveNormalLift = 3.7f;
    public float serveDownForward = 7.2f;
    public float serveNormalForward = 6.1f;
    public float servePositionForwardOffset = 0.15f;
    public float servePositionYOffset = 0.03f;
    public float smashInputHeight = 1.05f;
    public float heavySkillSmashHeight = 0.72f;
    public float goodTimingMultiplier = 1.13f;
    public float weakTimingMultiplier = 0.92f;
    public float highForward = 5.7f;
    public float highLift = 8.2f;
    public float coreSkillHighForwardBonus = 0.25f;
    public float coreUltimateHighForwardBonus = 0.35f;
    public float coreUltimateHighLiftBonus = 0.35f;
    public float flatSpeed = 10.4f;
    public float flatLift = 2.35f;
    public float dashSkillFlatBonus = 0.85f;
    public float dashUltimateFlatBonus = 1.25f;
    public float coreSkillFlatLiftBonus = 0.2f;
    public float dropForward = 4.8f;
    public float dropLift = 3.25f;
    public float dropSpin = 0.8f;
    public float trickDropForward = 3.65f;
    public float trickDropLift = 1.85f;
    public float trickDropSpin = 1.65f;
    public float trickUltimateDropForward = 3.25f;
    public float trickUltimateDropLift = 2.35f;
    public float trickUltimateDropSpin = 2.45f;
    public float smashPower = 12.6f;
    public float empoweredSmashBonus = 3.2f;
    public float heavySkillSmashBonus = 1.35f;
    public float heavyUltimateSmashBonus = 2.25f;
    public float dashSkillSmashBonus = 0.55f;
    public float coreUltimateSmashBonus = 0.9f;
    public float primedUltimateSmashBonus = 1.4f;
    public float farSmashNetDistance = 3.0f;
    public float farSmashVertical = 0.45f;
    public float nearSmashVertical = -1.85f;
    public float heavySmashVerticalBonus = -0.55f;
    public float empoweredSmashVerticalBonus = -0.65f;
    public float smashBaseEndLag = 0.22f;
    public float empoweredSmashEndLag = 0.12f;
    public float heavySkillEndLagReduction = 0.05f;
    public float dashUltimateEndLagMultiplier = 0.58f;
    public float minimumEndLag = 0.08f;
    public float coreUltimateNonSmashMultiplier = 1.10f;
    public float trickUltimateSpinBonus = 1.75f;
    public float trickUltimateLiftBonus = 1.0f;
    public float hitPositionForwardOffset = 0.18f;
    public float hitPositionYOffset = 0.02f;
}

[Serializable]
public sealed class BalanceRules
{
    public BalanceRuleset standard = BalanceRuleset.Standard();
    public BalanceRuleset tournament = BalanceRuleset.Tournament();
    public BalanceRuleset practice = BalanceRuleset.Practice();
    public BalanceRuleset tutorial = BalanceRuleset.Tutorial();

    public static BalanceRules CreateDefault()
    {
        return new BalanceRules();
    }

    public void Normalize(BalanceRules defaults)
    {
        if (standard == null) standard = defaults.standard;
        if (tournament == null) tournament = defaults.tournament;
        if (practice == null) practice = defaults.practice;
        if (tutorial == null) tutorial = defaults.tutorial;
    }
}

[Serializable]
public sealed class BalanceRuleset
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

    public static BalanceRuleset Standard()
    {
        return new BalanceRuleset();
    }

    public static BalanceRuleset Tournament()
    {
        return new BalanceRuleset { bestOf = 3, abilitiesEnabled = true, courtModifiersEnabled = true };
    }

    public static BalanceRuleset Practice()
    {
        return new BalanceRuleset { bestOf = 1, abilitiesEnabled = true, courtModifiersEnabled = true };
    }

    public static BalanceRuleset Tutorial()
    {
        return new BalanceRuleset { bestOf = 1, abilitiesEnabled = true, courtModifiersEnabled = true, ballSpeedMultiplier = 0.86f, energyMultiplier = 2.2f };
    }

    public RulesetConfig ToRuleset()
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
public sealed class BalanceAi
{
    public float tutorialDifficulty = 0.48f;
    public float practiceDifficulty = 0.68f;
    public float quickMatchDifficulty = 0.82f;
    public float ballHalfOffset = 0.25f;
    public float movingTowardThreshold = 0.2f;
    public float centerThreatRange = 1.0f;
    public float thinkIntervalEasy = 0.18f;
    public float thinkIntervalHard = 0.045f;
    public float habitThreshold = 0.3f;
    public float habitOffset = 0.55f;
    public float highThreatY = 1.25f;
    public float highThreatOffsetEasy = 0.16f;
    public float highThreatOffsetHard = 0.44f;
    public float smashRatioThreshold = 0.38f;
    public float smashCounterOffset = 0.38f;
    public float flatRatioThreshold = 0.38f;
    public float flatCounterOffset = 0.22f;
    public float randomMistakeChanceEasy = 0.48f;
    public float randomMistakeChanceHard = 0.06f;
    public float randomErrorEasy = 0.9f;
    public float randomErrorHard = 0.18f;
    public float movementDeadzoneEasy = 0.22f;
    public float movementDeadzoneHard = 0.08f;
    public float ballComingLaneOffset = 0.6f;
    public float closeLaneEasy = 1.05f;
    public float closeLaneHard = 1.72f;
    public float jumpBallY = 1.35f;
    public float jumpChanceEasy = 0.34f;
    public float jumpChanceHard = 0.92f;
    public float smashPrepYEasy = 1.85f;
    public float smashPrepYHard = 1.25f;
    public float smashPrepChanceEasy = 0.38f;
    public float smashPrepChanceHard = 0.78f;
    public float lowLiftY = 0.88f;
    public float swingReachEasy = 0.92f;
    public float swingReachHard = 1.28f;
    public float swingChanceEasy = 0.68f;
    public float swingChanceHard = 0.98f;
    public float skillHoldChanceEasy = 0.28f;
    public float skillHoldChanceHard = 0.72f;
    public float skillUseChanceEasy = 0.22f;
    public float skillUseChanceHard = 0.68f;
    public int adaptationBannerSampleThreshold = 8;
    public float adaptationBannerChancePerSecond = 0.08f;
    public float smashableYEasy = 1.55f;
    public float smashableYHard = 1.15f;
    public float opponentOutMin = 1.6f;
    public float opponentOutMax = 4.8f;
    public float smashChanceEasy = 0.28f;
    public float smashChanceHard = 0.66f;
    public float opponentOutSmashBonus = 0.18f;
    public float pressureSmashChanceBonus = 0.10f;
    public float tacticalDropOpponentBackX = 3.25f;
    public float tacticalDropMaxY = 1.55f;
    public float tacticalDropChanceEasy = 0.10f;
    public float tacticalDropChanceHard = 0.46f;
    public float tacticalLiftOpponentFrontX = 2.05f;
    public float tacticalLiftChanceEasy = 0.18f;
    public float tacticalLiftChanceHard = 0.56f;
    public float lowShotLiftY = 0.9f;
    public float antiSmashRatioThreshold = 0.42f;
    public float antiSmashLiftChanceEasy = 0.16f;
    public float antiSmashLiftChanceHard = 0.32f;
    public float randomLiftChanceEasy = 0.14f;
    public float randomLiftChanceHard = 0.07f;
    public float homeX = 5.15f;
    public float clampInner = 0.85f;
    public float clampBackMargin = 0.75f;

    public void Normalize(BalanceAi defaults)
    {
        if (tutorialDifficulty <= 0.0f) tutorialDifficulty = defaults.tutorialDifficulty;
        if (practiceDifficulty <= 0.0f) practiceDifficulty = defaults.practiceDifficulty;
        if (quickMatchDifficulty <= 0.0f) quickMatchDifficulty = defaults.quickMatchDifficulty;
        pressureSmashChanceBonus = Mathf.Clamp01(pressureSmashChanceBonus);
        if (tacticalDropOpponentBackX <= 0.0f) tacticalDropOpponentBackX = defaults.tacticalDropOpponentBackX;
        if (tacticalDropMaxY <= 0.0f) tacticalDropMaxY = defaults.tacticalDropMaxY;
        tacticalDropChanceEasy = Mathf.Clamp01(tacticalDropChanceEasy);
        tacticalDropChanceHard = Mathf.Clamp01(tacticalDropChanceHard);
        if (tacticalDropChanceHard < tacticalDropChanceEasy) tacticalDropChanceHard = defaults.tacticalDropChanceHard;
        if (tacticalLiftOpponentFrontX <= 0.0f) tacticalLiftOpponentFrontX = defaults.tacticalLiftOpponentFrontX;
        tacticalLiftChanceEasy = Mathf.Clamp01(tacticalLiftChanceEasy);
        tacticalLiftChanceHard = Mathf.Clamp01(tacticalLiftChanceHard);
        if (tacticalLiftChanceHard < tacticalLiftChanceEasy) tacticalLiftChanceHard = defaults.tacticalLiftChanceHard;
    }
}

[Serializable]
public sealed class BalanceNetwork
{
    public int defaultPort = 27777;
    public int portMin = 1024;
    public int portMax = 65535;
    public int pointsMin = 5;
    public int pointsMax = 11;
    public float energyMultiplierMin = 0.5f;
    public float energyMultiplierMax = 2.0f;
    public float ballSpeedMultiplierMin = 0.75f;
    public float ballSpeedMultiplierMax = 1.5f;
    public float pingIntervalSeconds = 1.0f;
    public float pingRetrySeconds = 2.5f;

    public void Normalize(BalanceNetwork defaults)
    {
        if (defaultPort <= 0) defaultPort = defaults.defaultPort;
        if (portMin <= 0) portMin = defaults.portMin;
        if (portMax < portMin) portMax = defaults.portMax;
        if (pointsMin <= 0) pointsMin = defaults.pointsMin;
        if (pointsMax < pointsMin) pointsMax = defaults.pointsMax;
        if (energyMultiplierMin <= 0.0f) energyMultiplierMin = defaults.energyMultiplierMin;
        if (energyMultiplierMax < energyMultiplierMin) energyMultiplierMax = defaults.energyMultiplierMax;
        if (ballSpeedMultiplierMin <= 0.0f) ballSpeedMultiplierMin = defaults.ballSpeedMultiplierMin;
        if (ballSpeedMultiplierMax < ballSpeedMultiplierMin) ballSpeedMultiplierMax = defaults.ballSpeedMultiplierMax;
        if (pingIntervalSeconds <= 0.0f) pingIntervalSeconds = defaults.pingIntervalSeconds;
        if (pingRetrySeconds <= pingIntervalSeconds) pingRetrySeconds = Mathf.Max(defaults.pingRetrySeconds, pingIntervalSeconds + 0.1f);
    }
}

[Serializable]
public sealed class BalanceCharacter
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
    public BalanceColor accent;

    public void Normalize(BalanceCharacter defaults)
    {
        if (defaults == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(code)) code = defaults.code;
        if (string.IsNullOrEmpty(displayName)) displayName = defaults.displayName;
        if (string.IsNullOrEmpty(role)) role = defaults.role;
        if (string.IsNullOrEmpty(descriptionText)) descriptionText = defaults.descriptionText;
        if (string.IsNullOrEmpty(playStyleText)) playStyleText = defaults.playStyleText;
        if (string.IsNullOrEmpty(differenceText)) differenceText = defaults.differenceText;
        if (string.IsNullOrEmpty(abilityBreakdownText)) abilityBreakdownText = defaults.abilityBreakdownText;
        if (string.IsNullOrEmpty(passiveText)) passiveText = defaults.passiveText;
        if (string.IsNullOrEmpty(skillText)) skillText = defaults.skillText;
        if (string.IsNullOrEmpty(ultimateText)) ultimateText = defaults.ultimateText;
        if (string.IsNullOrEmpty(strengthsText)) strengthsText = defaults.strengthsText;
        if (string.IsNullOrEmpty(weaknessText)) weaknessText = defaults.weaknessText;
        if (string.IsNullOrEmpty(recommendedText)) recommendedText = defaults.recommendedText;
        if (string.IsNullOrEmpty(statText)) statText = defaults.statText;
        if (moveSpeed <= 0.0f) moveSpeed = defaults.moveSpeed;
        if (jumpForce <= 0.0f) jumpForce = defaults.jumpForce;
        if (hitRadius <= 0.0f) hitRadius = defaults.hitRadius;
        if (visualBodyScale <= 0.0f) visualBodyScale = defaults.visualBodyScale;
        if (visualLimbScale <= 0.0f) visualLimbScale = defaults.visualLimbScale;
        if (visualHeadScale <= 0.0f) visualHeadScale = defaults.visualHeadScale;
        if (jerseyWhiteMix <= 0.0f) jerseyWhiteMix = defaults.jerseyWhiteMix;
        jerseyWhiteMix = Mathf.Clamp01(jerseyWhiteMix);
    }

    public CharacterConfig ToCharacterConfig()
    {
        return new CharacterConfig(code, displayName, role, descriptionText, playStyleText, differenceText, abilityBreakdownText, passiveText, skillText, ultimateText, statText, moveSpeed, jumpForce, hitRadius, smashBonus, smashEndLag, controlBonus, accent.ToColor(Color.white), strengthsText, weaknessText, recommendedText, visualBodyScale, visualLimbScale, visualHeadScale, jerseyWhiteMix);
    }
}

[Serializable]
public sealed class BalanceCourt
{
    public string name;
    public BalanceColor background;
    public BalanceColor accent;
    public float windX;
    public float windY;
    public float period;
}

[Serializable]
public sealed class BalanceCosmetic
{
    public string id;
    public string displayName;
    public string description;
    public string unlockLabel;
    public float tintBlend = 0.72f;
    public BalanceColor tint;

    public void Normalize(BalanceCosmetic defaults)
    {
        if (defaults == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(id)) id = defaults.id;
        if (string.IsNullOrEmpty(displayName)) displayName = defaults.displayName;
        if (string.IsNullOrEmpty(description)) description = defaults.description;
        if (string.IsNullOrEmpty(unlockLabel)) unlockLabel = defaults.unlockLabel;
        if (tintBlend < 0.0f || tintBlend > 1.0f) tintBlend = defaults.tintBlend;
        if (tint.a <= 0.0f) tint = defaults.tint;
    }

    public CosmeticConfig ToCosmeticConfig()
    {
        return new CosmeticConfig(id, displayName, description, tint.ToColor(Color.white), unlockLabel, Mathf.Clamp01(tintBlend));
    }
}

[Serializable]
public struct BalanceColor
{
    public float r;
    public float g;
    public float b;
    public float a;

    public static BalanceColor FromColor(Color color)
    {
        return new BalanceColor { r = color.r, g = color.g, b = color.b, a = color.a };
    }

    public Color ToColor(Color fallback)
    {
        float alpha = a <= 0.0f ? fallback.a : a;
        return new Color(r, g, b, alpha);
    }
}
