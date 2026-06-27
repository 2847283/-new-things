#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FireChampionProjectValidator
{
    private const string ScenePath = "Assets/Scenes/FireChampion.unity";
    private const string BalanceAssetPath = "Assets/Resources/FireChampion/Data/fire_champion_balance.json";
    private const string AssetSourcesPath = "Assets/Resources/FireChampion/ASSET_SOURCES.md";

    private static readonly string[] RequiredUiResources =
    {
        "FireChampion/UI/logo_fire_champion",
        "FireChampion/UI/role_core",
        "FireChampion/UI/role_dash",
        "FireChampion/UI/role_heavy",
        "FireChampion/UI/role_trick",
        "FireChampion/UI/court_dojo",
        "FireChampion/UI/court_rooftop",
        "FireChampion/UI/court_future"
    };

    [MenuItem("Fire Champion/Validate Project")]
    public static void ValidateOrThrow()
    {
        List<string> errors = new List<string>();
        ValidateAssets(errors);
        ValidateQaLaunch(errors);
        ValidateLayout(errors);
        ValidateBalance(errors);
        ValidateMatchRules(errors);
        ValidateProfileProgression(errors);
        ValidateAiController(errors);
        ValidateAiSimulation(errors);
        FireChampionBalanceConfig vfxBalance = FireChampionBalanceConfig.Load();
        if (vfxBalance != null && vfxBalance.gameplay != null)
        {
            ValidateVfxSystem(errors, vfxBalance.gameplay.feedback);
        }
        ValidateOverlayRenderer(errors);
        ValidatePracticeShotTimeline(errors);
        ValidateMatchFlow(errors);

        if (errors.Count > 0)
        {
            throw new Exception("Fire Champion project validation failed:\n- " + string.Join("\n- ", errors.ToArray()));
        }

        Debug.Log("Fire Champion project validation passed.");
    }

    private static void ValidateQaLaunch(List<string> errors)
    {
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "settings" }) == FireChampionQaScreen.Settings,
            "QA launch check failed: settings argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "network-waiting" }) == FireChampionQaScreen.NetworkWaiting,
            "QA launch check failed: network-waiting argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { "-firechampion-qa-screen=summary" }) == FireChampionQaScreen.Summary,
            "QA launch check failed: inline summary argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "vfx-preview" }) == FireChampionQaScreen.VfxPreview,
            "QA launch check failed: vfx-preview argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "quick-ai" }) == FireChampionQaScreen.QuickMatch,
            "QA launch check failed: quick-ai argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "auto-match" }) == FireChampionQaScreen.AutoMatch,
            "QA launch check failed: auto-match argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.Parse(new[] { FireChampionQaLaunch.ArgumentName, "unknown" }) == FireChampionQaScreen.None,
            "QA launch check failed: unknown argument should be ignored.");
        Require(errors,
            FireChampionQaLaunch.ParseCourtIndex(new[] { FireChampionQaLaunch.CourtArgumentName, "dojo" }) == 0,
            "QA court check failed: dojo argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseCourtIndex(new[] { FireChampionQaLaunch.CourtArgumentName, "rooftop" }) == 1,
            "QA court check failed: rooftop argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseCourtIndex(new[] { "-firechampion-qa-court=future" }) == 2,
            "QA court check failed: inline future argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseCourtIndex(new[] { FireChampionQaLaunch.CourtArgumentName, "99" }) == 99,
            "QA court check failed: numeric court argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseCourtIndex(new[] { FireChampionQaLaunch.CourtArgumentName, "unknown" }) == -1,
            "QA court check failed: unknown argument should be ignored.");
        Require(errors,
            FireChampionQaLaunch.ParseSummaryVariant(new[] { FireChampionQaLaunch.SummaryArgumentName, "loss" }) == FireChampionQaSummaryVariant.Loss,
            "QA summary check failed: loss argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseSummaryVariant(new[] { "-firechampion-qa-summary=tournament-final" }) == FireChampionQaSummaryVariant.TournamentFinalWin,
            "QA summary check failed: inline tournament-final argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseSummaryVariant(new[] { FireChampionQaLaunch.SummaryArgumentName, "network-client" }) == FireChampionQaSummaryVariant.NetworkClient,
            "QA summary check failed: network-client argument did not parse.");
        Require(errors,
            FireChampionQaLaunch.ParseSummaryVariant(new[] { FireChampionQaLaunch.SummaryArgumentName, "unknown" }) == FireChampionQaSummaryVariant.Win,
            "QA summary check failed: unknown argument should fall back to win.");

        FireChampionQaCaptureConfig capture = FireChampionQaCapture.Parse(new[] { FireChampionQaCapture.CaptureArgumentName, "qa.png", FireChampionQaCapture.DelayArgumentName, "0.25", FireChampionQaCapture.QuitAfterCaptureArgumentName });
        Require(errors,
            capture.IsRequested && capture.outputPath.EndsWith("qa.png", StringComparison.OrdinalIgnoreCase),
            "QA capture check failed: screenshot path argument did not parse.");
        Require(errors,
            Mathf.Abs(capture.delaySeconds - 0.25f) < 0.001f,
            "QA capture check failed: delay argument did not parse.");
        Require(errors,
            capture.quitAfterCapture,
            "QA capture check failed: quit-after-capture flag did not parse.");
    }

    private static void ValidateAssets(List<string> errors)
    {
        Require(errors, AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null, "Missing startup scene: " + ScenePath);
        Require(errors, AssetDatabase.LoadAssetAtPath<TextAsset>(BalanceAssetPath) != null, "Missing balance data: " + BalanceAssetPath);
        Require(errors, AssetDatabase.LoadAssetAtPath<TextAsset>(AssetSourcesPath) != null, "Missing asset source manifest: " + AssetSourcesPath);

        for (int i = 0; i < RequiredUiResources.Length; i++)
        {
            Texture2D texture = Resources.Load<Texture2D>(RequiredUiResources[i]);
            Require(errors, texture != null, "Missing UI resource: Resources/" + RequiredUiResources[i]);
        }

        for (int i = 0; i < FireChampionVfxAssets.RequiredResourcePaths.Length; i++)
        {
            string path = FireChampionVfxAssets.RequiredResourcePaths[i];
            Texture2D texture = Resources.Load<Texture2D>(path);
            Require(errors, texture != null, "Missing VFX resource: Resources/" + path);
            if (texture != null)
            {
                Require(errors, texture.width == 256 && texture.height == 256, "VFX resource should be 256x256: Resources/" + path);
            }
        }

        for (int i = 0; i < FireChampionCourtAssets.RequiredResourcePaths.Length; i++)
        {
            string path = FireChampionCourtAssets.RequiredResourcePaths[i];
            Texture2D texture = Resources.Load<Texture2D>(path);
            Require(errors, texture != null, "Missing court background resource: Resources/" + path);
            if (texture != null)
            {
                Require(errors,
                    texture.width == FireChampionCourtAssets.BackgroundWidth && texture.height == FireChampionCourtAssets.BackgroundHeight,
                    "Court background resource should be 1280x720: Resources/" + path);
            }
        }

        for (int i = 0; i < FireChampionAudioAssets.RequiredResourcePaths.Length; i++)
        {
            string path = FireChampionAudioAssets.RequiredResourcePaths[i];
            AudioClip clip = Resources.Load<AudioClip>(path);
            Require(errors, clip != null, "Missing audio resource: Resources/" + path);
            if (clip != null)
            {
                Require(errors, clip.samples > 0 && clip.length >= 0.03f, "Audio resource should be a non-empty short cue: Resources/" + path);
            }
        }
    }

    private static void ValidateLayout(List<string> errors)
    {
        FireChampionLayoutAuditReport report = FireChampionLayout.AuditCommonResolutions();
        Require(errors, report.failures == 0, "Layout audit failed: " + report.Summary());
        Require(errors, report.resolutionCount >= 5, "Layout audit failed: not enough screen sizes checked.");
        Require(errors, report.infoHiddenCount == 1, "Layout audit failed: expected only the narrow layout to hide the info card. " + report.Summary());
        Debug.Log("Fire Champion layout audit passed: " + report.Summary());
    }

    private static void ValidateBalance(List<string> errors)
    {
        FireChampionBalanceLoadReport loadReport = FireChampionBalanceConfig.LoadWithReport(false);
        Require(errors, loadReport.resourceFound, "Balance config resource was not found: Resources/" + FireChampionBalanceConfig.ResourcePath);
        Require(errors, loadReport.textLength > 0, "Balance config resource is empty: Resources/" + FireChampionBalanceConfig.ResourcePath);
        Require(errors, loadReport.IsValidResourceLoad, "Balance config must load from JSON without falling back to defaults. " + loadReport.message);
        Debug.Log("Fire Champion balance data loaded: " + loadReport.message);
        FireChampionBalanceConfig balance = loadReport.config;
        if (balance == null)
        {
            return;
        }

        CharacterConfig[] roster = balance.CreateCharacterRoster();
        CourtConfig[] courts = balance.CreateCourtConfigs();
        CosmeticConfig[] cosmetics = balance.CreateCosmetics();
        Require(errors, roster != null && roster.Length >= 4, "Expected at least four character configs.");
        Require(errors, courts != null && courts.Length >= 3, "Expected at least three court configs.");
        Require(errors, cosmetics != null && cosmetics.Length >= 1, "Expected at least one cosmetic config.");

        if (roster != null)
        {
            for (int i = 0; i < roster.Length; i++)
            {
                CharacterConfig character = roster[i];
                Require(errors, character != null, "Character config " + i + " is null.");
                if (character == null)
                {
                    continue;
                }

                Require(errors, !string.IsNullOrEmpty(character.code), "Character " + i + " has no code.");
                Require(errors, !string.IsNullOrEmpty(character.displayName), "Character " + character.code + " has no display name.");
                Require(errors, !string.IsNullOrEmpty(character.role), "Character " + character.code + " has no role.");
                Require(errors, !string.IsNullOrEmpty(character.descriptionText), "Character " + character.code + " has no description.");
                Require(errors, !string.IsNullOrEmpty(character.playStyleText), "Character " + character.code + " has no play-style text.");
                Require(errors, !string.IsNullOrEmpty(character.differenceText), "Character " + character.code + " has no difference text.");
                Require(errors, !string.IsNullOrEmpty(character.abilityBreakdownText), "Character " + character.code + " has no ability breakdown text.");
                Require(errors, !string.IsNullOrEmpty(character.passiveText), "Character " + character.code + " has no passive text.");
                Require(errors, !string.IsNullOrEmpty(character.skillText), "Character " + character.code + " has no skill text.");
                Require(errors, !string.IsNullOrEmpty(character.ultimateText), "Character " + character.code + " has no ultimate text.");
                Require(errors, !string.IsNullOrEmpty(character.strengthsText), "Character " + character.code + " has no strengths text.");
                Require(errors, !string.IsNullOrEmpty(character.weaknessText), "Character " + character.code + " has no weakness text.");
                Require(errors, !string.IsNullOrEmpty(character.recommendedText), "Character " + character.code + " has no recommended-player text.");
                Require(errors, !string.IsNullOrEmpty(character.statText), "Character " + character.code + " has no stat summary text.");
                Require(errors, character.moveSpeed > 0, "Character " + character.code + " has non-positive move speed.");
                Require(errors, character.jumpForce > 0, "Character " + character.code + " has non-positive jump force.");
                Require(errors, character.hitRadius > 0, "Character " + character.code + " has non-positive hit radius.");
                Require(errors, character.visualBodyScale > 0, "Character " + character.code + " has non-positive visual body scale.");
                Require(errors, character.visualLimbScale > 0, "Character " + character.code + " has non-positive visual limb scale.");
                Require(errors, character.visualHeadScale > 0, "Character " + character.code + " has non-positive visual head scale.");
                Require(errors, character.jerseyWhiteMix >= 0.0f && character.jerseyWhiteMix <= 1.0f, "Character " + character.code + " jersey mix must be 0..1.");
            }

            ValidateCharacterRoleDifferences(errors, roster);
        }

        if (courts != null)
        {
            for (int i = 0; i < courts.Length; i++)
            {
                CourtConfig court = courts[i];
                Require(errors, court != null, "Court config " + i + " is null.");
                if (court != null)
                {
                    Require(errors, !string.IsNullOrEmpty(court.name), "Court " + i + " has no name.");
                    Require(errors, court.period > 0, "Court " + court.name + " has non-positive wind period.");
                }
            }
        }

        ValidateRuleset(errors, balance.rules.standard, "standard");
        ValidateRuleset(errors, balance.rules.tournament, "tournament");
        ValidateRuleset(errors, balance.rules.practice, "practice");
        ValidateRuleset(errors, balance.rules.tutorial, "tutorial");
        ValidateCosmetics(errors, cosmetics);
        ValidateAiTuning(errors, balance.ai);
        Require(errors, balance.network.pingIntervalSeconds > 0.0f, "Network ping interval must be positive.");
        Require(errors, balance.network.pingRetrySeconds > balance.network.pingIntervalSeconds, "Network ping retry should be longer than the ping interval.");
        Require(errors, balance.gameplay.energy.maxEnergy > 0, "Max energy must be positive.");
        Require(errors, balance.gameplay.energy.skillCost > 0, "Skill cost must be positive.");
        Require(errors, balance.gameplay.practice.timelineCapacity >= 3 && balance.gameplay.practice.timelineCapacity <= 12, "Practice timeline capacity should be between 3 and 12.");
        Require(errors, balance.gameplay.swing.hitActiveWindow > 0, "Hit active window must be positive.");
        Require(errors, balance.gameplay.shot.smashPower > balance.gameplay.shot.flatSpeed, "Smash power should exceed flat shot speed.");
        Require(errors, balance.gameplay.shot.dropForward > 0, "Drop forward speed must be positive.");
        ValidateAudioCue(errors, balance.gameplay.audio.serve, "serve");
        ValidateAudioCue(errors, balance.gameplay.audio.normalHit, "normalHit");
        ValidateAudioCue(errors, balance.gameplay.audio.smashHit, "smashHit");
        ValidateAudioCue(errors, balance.gameplay.audio.scoreLeft, "scoreLeft");
        ValidateAudioCue(errors, balance.gameplay.audio.scoreRight, "scoreRight");
        Require(errors, balance.gameplay.audio.smashHit.frequency > balance.gameplay.audio.normalHit.frequency, "Audio check failed: smash hit cue should be brighter than normal hit.");
        ValidateFeedback(errors, balance.gameplay.feedback);
        ValidateVisuals(errors, balance.gameplay.visuals);
    }

    private static void ValidateAiTuning(List<string> errors, BalanceAi ai)
    {
        Require(errors, ai != null, "AI tuning is missing.");
        if (ai == null)
        {
            return;
        }

        Require(errors, IsAlpha(ai.pressureSmashChanceBonus), "AI tuning check failed: pressure smash bonus must be 0..1.");
        Require(errors, ai.tacticalDropOpponentBackX > ai.tacticalLiftOpponentFrontX, "AI tuning check failed: drop should target deeper opponents than lift.");
        Require(errors, ai.tacticalDropMaxY > 0.0f, "AI tuning check failed: tactical drop max height must be positive.");
        Require(errors, IsAlpha(ai.tacticalDropChanceEasy) && IsAlpha(ai.tacticalDropChanceHard), "AI tuning check failed: tactical drop chance must be 0..1.");
        Require(errors, ai.tacticalDropChanceHard >= ai.tacticalDropChanceEasy, "AI tuning check failed: hard tactical drop chance should be >= easy.");
        Require(errors, IsAlpha(ai.tacticalLiftChanceEasy) && IsAlpha(ai.tacticalLiftChanceHard), "AI tuning check failed: tactical lift chance must be 0..1.");
        Require(errors, ai.tacticalLiftChanceHard >= ai.tacticalLiftChanceEasy, "AI tuning check failed: hard tactical lift chance should be >= easy.");
    }

    private static void ValidateCharacterRoleDifferences(List<string> errors, CharacterConfig[] roster)
    {
        CharacterConfig core = FindCharacter(roster, "CORE");
        CharacterConfig dash = FindCharacter(roster, "DASH");
        CharacterConfig heavy = FindCharacter(roster, "HEAVY");
        CharacterConfig trick = FindCharacter(roster, "TRICK");
        Require(errors, core != null && dash != null && heavy != null && trick != null, "Expected CORE, DASH, HEAVY, and TRICK character codes.");
        if (core == null || dash == null || heavy == null || trick == null)
        {
            return;
        }

        Require(errors, core.hitRadius > dash.hitRadius && core.hitRadius > heavy.hitRadius && core.hitRadius > trick.hitRadius, "Role difference check failed: CORE should have the largest sweet spot.");
        Require(errors, dash.moveSpeed > core.moveSpeed && dash.moveSpeed > heavy.moveSpeed && dash.moveSpeed > trick.moveSpeed, "Role difference check failed: DASH should be the fastest.");
        Require(errors, heavy.smashBonus > core.smashBonus && heavy.smashBonus > dash.smashBonus && heavy.smashBonus > trick.smashBonus, "Role difference check failed: HEAVY should have the strongest smash.");
        Require(errors, heavy.moveSpeed < core.moveSpeed && heavy.moveSpeed < dash.moveSpeed && heavy.moveSpeed < trick.moveSpeed, "Role difference check failed: HEAVY should be the slowest.");
        Require(errors, trick.controlBonus > core.controlBonus && trick.controlBonus > dash.controlBonus && trick.controlBonus > heavy.controlBonus, "Role difference check failed: TRICK should have the strongest control.");
        Require(errors, trick.jumpForce > core.jumpForce && trick.jumpForce > dash.jumpForce && trick.jumpForce > heavy.jumpForce, "Role difference check failed: TRICK should jump the highest.");
        Require(errors, dash.visualBodyScale < core.visualBodyScale, "Role visual check failed: DASH should have a lighter silhouette than CORE.");
        Require(errors, heavy.visualBodyScale > core.visualBodyScale, "Role visual check failed: HEAVY should have the largest silhouette.");
        Require(errors, heavy.visualLimbScale > dash.visualLimbScale, "Role visual check failed: HEAVY limbs should read heavier than DASH.");
        Require(errors, trick.visualLimbScale <= core.visualLimbScale, "Role visual check failed: TRICK should keep a nimble silhouette.");
    }

    private static CharacterConfig FindCharacter(CharacterConfig[] roster, string code)
    {
        if (roster == null)
        {
            return null;
        }

        for (int i = 0; i < roster.Length; i++)
        {
            if (roster[i] != null && string.Equals(roster[i].code, code, StringComparison.OrdinalIgnoreCase))
            {
                return roster[i];
            }
        }

        return null;
    }

    private static void ValidateAudioCue(List<string> errors, BalanceToneCue cue, string label)
    {
        Require(errors, cue != null, "Missing audio cue: " + label);
        if (cue == null)
        {
            return;
        }

        Require(errors, cue.frequency > 0.0f, "Audio cue " + label + " frequency must be positive.");
        Require(errors, cue.duration > 0.0f, "Audio cue " + label + " duration must be positive.");
        Require(errors, cue.volumeMultiplier >= 0.0f, "Audio cue " + label + " volume multiplier must be non-negative.");
    }

    private static void ValidateFeedback(List<string> errors, BalanceFeedbackTuning feedback)
    {
        Require(errors, feedback != null, "Missing gameplay feedback tuning.");
        if (feedback == null)
        {
            return;
        }

        Require(errors, feedback.bannerDuration > 0.0f, "Feedback check failed: banner duration must be positive.");
        Require(errors, feedback.screenShakeFadeDuration > 0.0f, "Feedback check failed: screen shake fade duration must be positive.");
        Require(errors, feedback.hitShakeDuration > 0.0f, "Feedback check failed: hit shake duration must be positive.");
        Require(errors, feedback.scoreShakeDuration > 0.0f, "Feedback check failed: score shake duration must be positive.");
        Require(errors, feedback.hitShakeMagnitude >= 0.0f, "Feedback check failed: hit shake magnitude must be non-negative.");
        Require(errors, feedback.scoreShakeMagnitude >= 0.0f, "Feedback check failed: score shake magnitude must be non-negative.");
        Require(errors, feedback.smashScoreShakeMagnitude >= feedback.scoreShakeMagnitude, "Feedback check failed: smash-score shake should be at least normal score shake.");
        Require(errors, feedback.hitVfxDuration > 0.0f, "Feedback check failed: hit VFX duration must be positive.");
        Require(errors, feedback.scoreVfxDuration >= feedback.hitVfxDuration, "Feedback check failed: score VFX should last at least as long as hit VFX.");
        Require(errors, feedback.skillVfxDuration > 0.0f, "Feedback check failed: skill VFX duration must be positive.");
        Require(errors, feedback.hitVfxRadius > 0.0f, "Feedback check failed: hit VFX radius must be positive.");
        Require(errors, feedback.smashVfxRadius >= feedback.hitVfxRadius, "Feedback check failed: smash VFX radius should be at least hit radius.");
        Require(errors, feedback.scoreVfxRadius > 0.0f, "Feedback check failed: score VFX radius must be positive.");
        Require(errors, feedback.skillVfxRadius > 0.0f, "Feedback check failed: skill VFX radius must be positive.");
        Require(errors, feedback.maxVfxEvents >= 4 && feedback.maxVfxEvents <= 128, "Feedback check failed: VFX event cap should stay within 4..128.");
    }

    private static void ValidateVisuals(List<string> errors, BalanceVisualTuning visuals)
    {
        Require(errors, visuals != null, "Missing gameplay visual tuning.");
        if (visuals == null)
        {
            return;
        }

        Require(errors, visuals.courtSurfaceTopY > 0.0f, "Visual check failed: court surface top must be positive.");
        Require(errors, visuals.courtStripeCount >= 1, "Visual check failed: court stripe count must be at least one.");
        Require(errors, visuals.bodyWidth > 0.0f, "Visual check failed: body width must be positive.");
        Require(errors, visuals.limbWidth > 0.0f, "Visual check failed: limb width must be positive.");
        Require(errors, visuals.headRadius > 0.0f, "Visual check failed: head radius must be positive.");
        Require(errors, visuals.playerShadowWidth > 0.0f && visuals.playerShadowHeight > 0.0f, "Visual check failed: player shadow dimensions must be positive.");
        Require(errors, visuals.skillAuraWidth > 0.0f && visuals.skillAuraHeight > 0.0f, "Visual check failed: skill aura dimensions must be positive.");
        Require(errors, visuals.racketOuterRadiusX > visuals.racketInnerRadiusX, "Visual check failed: racket outer width should exceed inner width.");
        Require(errors, visuals.racketOuterRadiusY > visuals.racketInnerRadiusY, "Visual check failed: racket outer height should exceed inner height.");
        Require(errors, visuals.swingArcBaseRadius > 0.0f && visuals.swingArcTimerScale > 0.0f, "Visual check failed: swing arc tuning must be positive.");
        Require(errors, IsAlpha(visuals.courtTopHighlightAlpha), "Visual check failed: court top highlight alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.courtStripeAlpha), "Visual check failed: court stripe alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.courtAlternateStripeAlpha), "Visual check failed: court alternate stripe alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.courtLineShadowAlpha), "Visual check failed: court line shadow alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.courtSoftLineAlpha), "Visual check failed: court soft line alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.courtGlowAlpha), "Visual check failed: court glow alpha must be 0..1.");
        Require(errors, IsAlpha(visuals.skillAuraAlpha), "Visual check failed: skill aura alpha must be 0..1.");
    }

    private static void ValidateCosmetics(List<string> errors, CosmeticConfig[] cosmetics)
    {
        Require(errors, cosmetics != null && cosmetics.Length > 0, "Cosmetic check failed: catalog is empty.");
        if (cosmetics == null || cosmetics.Length == 0)
        {
            return;
        }

        HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool hasDefault = false;
        for (int i = 0; i < cosmetics.Length; i++)
        {
            CosmeticConfig cosmetic = cosmetics[i];
            Require(errors, cosmetic != null, "Cosmetic check failed: entry " + i + " is null.");
            if (cosmetic == null)
            {
                continue;
            }

            Require(errors, !string.IsNullOrEmpty(cosmetic.id), "Cosmetic check failed: entry " + i + " has no id.");
            Require(errors, !string.IsNullOrEmpty(cosmetic.displayName), "Cosmetic check failed: " + cosmetic.id + " has no display name.");
            Require(errors, !string.IsNullOrEmpty(cosmetic.description), "Cosmetic check failed: " + cosmetic.id + " has no description.");
            Require(errors, !string.IsNullOrEmpty(cosmetic.unlockLabel), "Cosmetic check failed: " + cosmetic.id + " has no unlock label.");
            Require(errors, IsAlpha(cosmetic.tint.a), "Cosmetic check failed: " + cosmetic.id + " tint alpha must be 0..1.");
            Require(errors, cosmetic.tintBlend >= 0.0f && cosmetic.tintBlend <= 1.0f, "Cosmetic check failed: " + cosmetic.id + " tint blend must be 0..1.");
            if (!string.IsNullOrEmpty(cosmetic.id))
            {
                Require(errors, ids.Add(cosmetic.id), "Cosmetic check failed: duplicate id " + cosmetic.id + ".");
                if (string.Equals(cosmetic.id, "default", StringComparison.OrdinalIgnoreCase))
                {
                    hasDefault = true;
                    Require(errors, cosmetic.tintBlend <= 0.0f, "Cosmetic check failed: default cosmetic should not tint the role color.");
                }
            }
        }

        Require(errors, hasDefault, "Cosmetic check failed: catalog must include a default cosmetic.");
    }

    private static bool IsAlpha(float value)
    {
        return value >= 0.0f && value <= 1.0f;
    }

    private static void ValidateRuleset(List<string> errors, BalanceRuleset rules, string label)
    {
        Require(errors, rules != null, "Missing ruleset: " + label);
        if (rules == null)
        {
            return;
        }

        Require(errors, rules.pointsToWin > 0, label + " pointsToWin must be positive.");
        Require(errors, rules.winBy > 0, label + " winBy must be positive.");
        Require(errors, rules.hardCap >= rules.pointsToWin, label + " hardCap must be >= pointsToWin.");
        Require(errors, rules.bestOf > 0, label + " bestOf must be positive.");
        Require(errors, rules.bestOf % 2 == 1, label + " bestOf should be odd.");
    }

    private static void ValidateMatchRules(List<string> errors)
    {
        RulesetConfig rules = new RulesetConfig();
        Require(errors, FireChampionMatchRules.IsGameWon(rules, 7, 5), "Rule check failed: 7-5 should win.");
        Require(errors, !FireChampionMatchRules.IsGameWon(rules, 7, 6), "Rule check failed: 7-6 should not win before win-by-two.");
        Require(errors, FireChampionMatchRules.IsGameWon(rules, 11, 10), "Rule check failed: hard cap should win.");
        Require(errors, FireChampionMatchRules.GamesNeededToWinMatch(RulesetConfig.Tournament()) == 2, "Rule check failed: best-of-three should require two games.");
        Require(errors, FireChampionMatchRules.IsServeFault(0, Side.Right, Side.Left), "Rule check failed: zero-hit point against server should be serve fault.");
    }

    private static void ValidateProfileProgression(List<string> errors)
    {
        ProfileData profile = new ProfileData();
        profile.StartTournamentRun(3);
        Require(errors, profile.HasActiveTournamentRun(3), "Profile check failed: new tournament run should be active.");
        Require(errors, profile.CurrentTournamentRound(3) == 0, "Profile check failed: new tournament run should start at round 0.");

        int medals = profile.RecordTournamentMatchResult(0, true, false, 1, 3);
        Require(errors, medals == 1, "Profile check failed: non-final tournament win should grant medals.");
        Require(errors, profile.HasActiveTournamentRun(3), "Profile check failed: non-final tournament win should keep run active.");
        Require(errors, profile.CurrentTournamentRound(3) == 1, "Profile check failed: non-final tournament win should resume next round.");

        profile.RecordTournamentMatchResult(1, false, false, 2, 3);
        Require(errors, !profile.HasActiveTournamentRun(3), "Profile check failed: tournament loss should clear active run.");

        profile.StartTournamentRun(3);
        profile.SetTournamentResumeRound(2, 3);
        profile.RecordTournamentMatchResult(2, true, true, 5, 3);
        Require(errors, !profile.HasActiveTournamentRun(3), "Profile check failed: final tournament win should clear active run.");
    }

    private static void ValidateAiController(List<string> errors)
    {
        FireChampionBalanceConfig balance = FireChampionBalanceConfig.CreateDefault();
        BalanceAi tuning = new BalanceAi();
        tuning.randomMistakeChanceEasy = 0.0f;
        tuning.randomMistakeChanceHard = 0.0f;
        tuning.movementDeadzoneEasy = 0.01f;
        tuning.movementDeadzoneHard = 0.01f;

        PlayerActor ai = new PlayerActor(Side.Right, 0, 3.0f);
        PlayerActor opponent = new PlayerActor(Side.Left, 0, -3.0f);
        ai.aiDifficulty = 1.0f;
        ai.aiThinkTimer = 0.0f;

        FireChampionAiContext context = new FireChampionAiContext();
        context.tuning = tuning;
        context.gameplay = balance.gameplay;
        context.rules = RulesetConfig.Standard();
        context.character = balance.CreateCharacterRoster()[0];
        context.profile = new ProfileData();
        context.shuttle = new ShuttleState
        {
            position = new Vector2(-3.0f, 1.0f),
            velocity = new Vector2(-1.0f, 0.0f),
            spin = 0.0f
        };
        context.racketPosition = new Vector2(ai.x, 1.2f);
        context.courtWind = Vector2.zero;
        context.gravity = -9.4f;
        context.shuttleDrag = 0.72f;
        context.courtSpeedMultiplier = 1.0f;
        context.courtHalfWidth = balance.gameplay.court.courtHalfWidth;
        context.bannerTimer = 1.0f;
        context.deltaTime = 0.016f;

        FireChampionAiDecision decision = FireChampionAiController.BuildInput(ai, opponent, context);
        Require(errors, decision.input.horizontal > 0, "AI check failed: right-side AI should move toward home target when not threatened.");
        Require(errors, ai.aiTargetX > ai.x, "AI check failed: home target should be to the right of the sampled AI position.");
        Require(errors, FireChampionAiController.ClampTargetX(tuning, Side.Right, 100.0f, 8.0f) <= 8.0f, "AI check failed: right clamp should stay inside court.");
        Require(errors, FireChampionAiController.ClampTargetX(tuning, Side.Left, -100.0f, 8.0f) >= -8.0f, "AI check failed: left clamp should stay inside court.");
        GameInput tacticalInput = new GameInput { horizontal = 1, vertical = 0, swingPressed = true, dropIntent = true };
        GameInput decodedInput = GameInput.Decode(tacticalInput.Encode());
        Require(errors, decodedInput.dropIntent, "Input check failed: tactical drop intent should round-trip through network encoding.");
        Require(errors, !GameInput.Decode("0,0,0,0,0,0").dropIntent, "Input check failed: legacy six-field network input should decode without drop intent.");
    }

    private static void ValidateAiSimulation(List<string> errors)
    {
        FireChampionBalanceConfig balance = FireChampionBalanceConfig.Load();
        FireChampionAiSimulationReport report = FireChampionAiSimulation.Run(balance);
        int expectedCharacterCourts = report.characterCount * report.courtCount;
        Require(errors, report.characterCount >= 4, "AI simulation check failed: expected at least four characters.");
        Require(errors, report.courtCount >= 3, "AI simulation check failed: expected at least three courts.");
        Require(errors, report.scenarioCount >= expectedCharacterCourts * 5, "AI simulation check failed: not enough scripted scenarios ran.");
        Require(errors, report.movementDecisions >= expectedCharacterCourts, "AI simulation check failed: home recovery scenarios should create movement decisions.");
        Require(errors, report.swingDecisions >= expectedCharacterCourts, "AI simulation check failed: reachable incoming shots should create swing decisions.");
        Require(errors, report.jumpDecisions >= expectedCharacterCourts, "AI simulation check failed: high incoming shots should create jump decisions.");
        Require(errors, report.tacticalDropDecisions >= expectedCharacterCourts, "AI simulation check failed: deep-opponent scenarios should create tactical drop decisions.");
        Require(errors, report.skillDecisions >= expectedCharacterCourts, "AI simulation check failed: skill-ready idle scenarios should create skill decisions.");
        Require(errors, report.skillHoldDecisions >= expectedCharacterCourts, "AI simulation check failed: empowered high-shot scenarios should hold skill.");
        Require(errors, report.adaptationBannerDecisions >= expectedCharacterCourts, "AI simulation check failed: learned-habit scenarios should trigger adaptation banner decisions.");
        Require(errors, report.targetClampViolations == 0, "AI simulation check failed: target clamp violations = " + report.targetClampViolations + ".");
        Require(errors, report.rallyScenarioCount >= expectedCharacterCourts, "AI rally health check failed: expected one rally scenario per character/court.");
        Require(errors, report.rallyFrameCount >= expectedCharacterCourts * 300, "AI rally health check failed: too few rally frames ran.");
        Require(errors, report.rallyAiContacts >= expectedCharacterCourts, "AI rally health check failed: each character/court should produce at least one AI contact on average.");
        Require(errors, report.rallyFeederContacts >= expectedCharacterCourts, "AI rally health check failed: scripted feeder should keep rallies alive.");
        Require(errors, report.rallyLongestExchange >= 2, "AI rally health check failed: no multi-contact exchange was observed.");
        Require(errors, report.rallyTargetClampViolations == 0, "AI rally health check failed: target clamp violations = " + report.rallyTargetClampViolations + ".");
        Require(errors, report.rallyInvalidFrames == 0, "AI rally health check failed: invalid physics frames = " + report.rallyInvalidFrames + ".");
        Debug.Log("Fire Champion AI simulation passed: " + report.Summary());
    }

    private static void ValidateMatchFlow(List<string> errors)
    {
        ProfileData quickProfile = new ProfileData();
        MatchStats quickStats = new MatchStats();
        quickStats.smashWinners = 3;
        FireChampionMatchCompletion quick = FireChampionMatchFlow.ApplyCompletedMatch(quickProfile, GameMode.QuickAi, 0, Side.Left, 7, 4, 1, 0, quickStats);
        Require(errors, quick.playerWon, "Match flow check failed: left-side quick match should be a player win.");
        Require(errors, quickProfile.totalWins == 1 && quickProfile.totalLosses == 0, "Match flow check failed: quick win should update total wins.");
        Require(errors, quickProfile.badges >= 2, "Match flow check failed: first win and smash badge should be recorded.");
        Require(errors, quick.summaryTitle == "胜利！", "Match flow check failed: quick win summary title should be victory.");
        Require(errors, quick.summaryDetails.Contains("比分 7:4"), "Match flow check failed: quick summary should include final score.");

        ProfileData tournamentProfile = new ProfileData();
        tournamentProfile.StartTournamentRun(TournamentProgression.RoundCount);
        tournamentProfile.SetTournamentResumeRound(TournamentProgression.RoundCount - 1, TournamentProgression.RoundCount);
        FireChampionMatchCompletion tournament = FireChampionMatchFlow.ApplyCompletedMatch(
            tournamentProfile,
            GameMode.Tournament,
            TournamentProgression.RoundCount - 1,
            Side.Left,
            7,
            5,
            2,
            1,
            new MatchStats());
        Require(errors, tournament.playerWon, "Match flow check failed: final tournament win should be a player win.");
        Require(errors, tournamentProfile.tournamentWins == 1, "Match flow check failed: final tournament win should increment championships.");
        Require(errors, tournamentProfile.tournamentMedals >= TournamentProgression.RewardMedalsForRound(TournamentProgression.RoundCount - 1), "Match flow check failed: final tournament win should grant medals.");
        Require(errors, !tournamentProfile.HasActiveTournamentRun(TournamentProgression.RoundCount), "Match flow check failed: final tournament win should clear active run.");
        Require(errors, tournamentProfile.badges >= 3, "Match flow check failed: final tournament win should record champion badge.");
        Require(errors, tournament.summaryDetails.Contains("冠军"), "Match flow check failed: final tournament summary should mention championship.");
    }

    private static void ValidatePracticeShotTimeline(List<string> errors)
    {
        PracticeShotTimeline timeline = new PracticeShotTimeline();
        timeline.RecordContact(ShotType.Flat, 72, "稳定命中", "继续保持", 3);
        timeline.RecordContact(ShotType.Drop, 61, "稳定命中", "控制落点", 3);
        timeline.RecordContact(ShotType.Smash, 94, "甜区命中", "保持节奏", 3);
        timeline.RecordContact(ShotType.High, 48, "擦边命中", "提前站位", 3);

        Require(errors, timeline.Count == 3, "Practice timeline should trim to configured capacity.");
        Require(errors, timeline.Entries[0].sequence == 4, "Practice timeline should keep newest entries first.");
        timeline.RecordTargetResult(true, 0.2f);
        Require(errors, timeline.Entries[0].targetEvaluated && timeline.Entries[0].targetHit, "Practice timeline should write target hit to newest pending entry.");
        timeline.RecordTargetResult(false, 1.4f);
        Require(errors, timeline.Entries[1].targetEvaluated && !timeline.Entries[1].targetHit && timeline.Entries[1].targetMissMeters > 1.0f, "Practice timeline should write the next target result to the next pending entry.");
        timeline.Reset();
        Require(errors, timeline.Count == 0, "Practice timeline reset should clear all entries.");

        Debug.Log("Fire Champion practice timeline validation passed.");
    }

    private static void ValidateVfxSystem(List<string> errors, BalanceFeedbackTuning feedback)
    {
        Require(errors, feedback != null, "VFX system check failed: feedback tuning is missing.");
        if (feedback == null)
        {
            return;
        }

        FireChampionVfxSystem system = new FireChampionVfxSystem();
        for (int i = 0; i < feedback.maxVfxEvents + 3; i++)
        {
            system.SpawnHit(Vector2.zero, ShotType.Flat, true, Color.white, 1, feedback);
        }

        Require(errors, system.Events.Count == feedback.maxVfxEvents, "VFX system check failed: event cap should trim oldest effects.");
        system.Clear();

        system.SpawnHit(Vector2.zero, ShotType.Flat, true, Color.white, 1, feedback);
        system.SpawnHit(Vector2.right, ShotType.Smash, false, Color.red, -1, feedback);
        system.SpawnScore(Vector2.up, Color.cyan, true, feedback);
        system.SpawnSkill(Vector2.one, Color.magenta, 1, feedback);
        Require(errors, system.Events.Count == 4, "VFX system check failed: expected four spawned VFX events.");
        Require(errors, system.Events[0].duration > 0.0f && system.Events[0].radius > 0.0f, "VFX system check failed: event timing/radius should be positive.");
        system.Update(Mathf.Max(feedback.scoreVfxDuration, feedback.skillVfxDuration) + 0.5f);
        Require(errors, system.Events.Count == 0, "VFX system check failed: expired events should be removed.");
        Debug.Log("Fire Champion VFX system validation passed.");
    }

    private static void ValidateOverlayRenderer(List<string> errors)
    {
        Require(errors,
            FireChampionOverlayRenderer.PrimarySummaryActionForState(GameMode.Tournament, true, false) == FireChampionSummaryAction.NextTournamentMatch,
            "Overlay renderer check failed: tournament win before final should continue to next match.");
        Require(errors,
            FireChampionOverlayRenderer.PrimarySummaryActionForState(GameMode.Tournament, true, true) == FireChampionSummaryAction.RestartTournament,
            "Overlay renderer check failed: completed tournament should offer restart.");
        Require(errors,
            FireChampionOverlayRenderer.PrimarySummaryActionForState(GameMode.NetworkClient, false, false) == FireChampionSummaryAction.NetworkMenu,
            "Overlay renderer check failed: network client summary should return to network menu.");
        Require(errors,
            FireChampionOverlayRenderer.PrimarySummaryActionForState(GameMode.QuickAi, false, false) == FireChampionSummaryAction.Rematch,
            "Overlay renderer check failed: quick match summary should offer rematch.");
        Require(errors,
            !string.IsNullOrEmpty(FireChampionOverlayRenderer.PrimarySummaryButtonLabel(FireChampionSummaryAction.Rematch)),
            "Overlay renderer check failed: primary summary action label should not be empty.");
        Debug.Log("Fire Champion overlay renderer validation passed.");
    }

    private static void Require(List<string> errors, bool condition, string message)
    {
        if (!condition)
        {
            errors.Add(message);
        }
    }
}
#endif
