using UnityEngine;

public struct FireChampionAiContext
{
    public BalanceAi tuning;
    public BalanceGameplay gameplay;
    public RulesetConfig rules;
    public CharacterConfig character;
    public ProfileData profile;
    public ShuttleState shuttle;
    public Vector2 racketPosition;
    public Vector2 courtWind;
    public float gravity;
    public float shuttleDrag;
    public float courtSpeedMultiplier;
    public float courtHalfWidth;
    public float bannerTimer;
    public float deltaTime;
}

public struct FireChampionAiDecision
{
    public GameInput input;
    public bool showAdaptationBanner;
}

public static class FireChampionAiController
{
    public const string AdaptationBanner = "AI 正在适应你的常用球路";

    public static FireChampionAiDecision BuildInput(PlayerActor ai, PlayerActor opponent, FireChampionAiContext context)
    {
        BalanceAi tuning = context.tuning ?? new BalanceAi();
        BalanceGameplay gameplay = context.gameplay ?? new BalanceGameplay();
        RulesetConfig rules = context.rules ?? RulesetConfig.Standard();
        ProfileData profile = context.profile ?? new ProfileData();
        ShuttleState shuttle = context.shuttle;
        float skill = Mathf.Clamp01(ai.aiDifficulty);
        bool aiOnRight = ai.side == Side.Right;
        bool ballOnAiHalf = aiOnRight ? shuttle.position.x > -tuning.ballHalfOffset : shuttle.position.x < tuning.ballHalfOffset;
        bool movingTowardAi = aiOnRight ? shuttle.velocity.x > -tuning.movingTowardThreshold : shuttle.velocity.x < tuning.movingTowardThreshold;
        bool threatening = ballOnAiHalf || movingTowardAi || Mathf.Abs(shuttle.position.x) < tuning.centerThreatRange;

        ai.aiThinkTimer -= context.deltaTime;
        if (ai.aiThinkTimer <= 0.0f)
        {
            ai.aiThinkTimer = Mathf.Lerp(tuning.thinkIntervalEasy, tuning.thinkIntervalHard, skill);
            ai.aiTargetX = threatening ? PredictLandingXFor(ai.side, context) : HomeX(tuning, ai.side);
            float habit = profile.DominantShotBias();
            if (habit > tuning.habitThreshold)
            {
                ai.aiTargetX += ai.side == Side.Right ? tuning.habitOffset : -tuning.habitOffset;
            }

            if (threatening && shuttle.position.y > tuning.highThreatY)
            {
                ai.aiTargetX += (aiOnRight ? -1.0f : 1.0f) * Mathf.Lerp(tuning.highThreatOffsetEasy, tuning.highThreatOffsetHard, skill);
            }

            if (profile.aiSmashRatio > tuning.smashRatioThreshold)
            {
                ai.aiTargetX += aiOnRight ? tuning.smashCounterOffset : -tuning.smashCounterOffset;
            }
            else if (profile.aiFlatRatio > tuning.flatRatioThreshold)
            {
                ai.aiTargetX += aiOnRight ? -tuning.flatCounterOffset : tuning.flatCounterOffset;
            }

            if (UnityEngine.Random.value < Mathf.Lerp(tuning.randomMistakeChanceEasy, tuning.randomMistakeChanceHard, skill))
            {
                float error = Mathf.Lerp(tuning.randomErrorEasy, tuning.randomErrorHard, skill);
                ai.aiTargetX += UnityEngine.Random.Range(-error, error);
            }

            ai.aiTargetX = ClampTargetX(tuning, ai.side, ai.aiTargetX, context.courtHalfWidth);
        }

        GameInput input = new GameInput();
        float diff = ai.aiTargetX - ai.x;
        if (Mathf.Abs(diff) > Mathf.Lerp(tuning.movementDeadzoneEasy, tuning.movementDeadzoneHard, skill))
        {
            input.horizontal = diff > 0 ? 1 : -1;
        }

        float distance = Vector2.Distance(shuttle.position, context.racketPosition);
        float hitRadius = context.character == null ? 0.82f : context.character.hitRadius;
        bool ballComing = movingTowardAi && (aiOnRight ? shuttle.position.x > -tuning.ballComingLaneOffset : shuttle.position.x < tuning.ballComingLaneOffset);
        bool closeLane = Mathf.Abs(shuttle.position.x - ai.x) < Mathf.Lerp(tuning.closeLaneEasy, tuning.closeLaneHard, skill);
        if (ballComing && closeLane && shuttle.position.y > tuning.jumpBallY)
        {
            input.jumpPressed = ai.grounded && UnityEngine.Random.value < Mathf.Lerp(tuning.jumpChanceEasy, tuning.jumpChanceHard, skill);
            if (shuttle.position.y > Mathf.Lerp(tuning.smashPrepYEasy, tuning.smashPrepYHard, skill) && UnityEngine.Random.value < Mathf.Lerp(tuning.smashPrepChanceEasy, tuning.smashPrepChanceHard, skill))
            {
                input.vertical = -1;
            }
        }
        else if (ballComing && closeLane && shuttle.position.y < tuning.lowLiftY)
        {
            input.vertical = 1;
        }

        if (ballComing && distance < hitRadius * Mathf.Lerp(tuning.swingReachEasy, tuning.swingReachHard, skill))
        {
            input.swingPressed = UnityEngine.Random.value < Mathf.Lerp(tuning.swingChanceEasy, tuning.swingChanceHard, skill);
            if (input.swingPressed && input.vertical == 0)
            {
                ApplyShotChoice(ref input, ai, opponent, context, skill);
            }
        }

        if (rules.abilitiesEnabled && input.swingPressed && input.vertical < 0 && ai.energy >= gameplay.energy.ultimateCost && UnityEngine.Random.value < Mathf.Lerp(tuning.skillHoldChanceEasy, tuning.skillHoldChanceHard, skill))
        {
            input.skillHeld = true;
        }

        if (rules.abilitiesEnabled && !input.swingPressed && ai.energy >= gameplay.energy.skillCost && UnityEngine.Random.value < context.deltaTime * Mathf.Lerp(tuning.skillUseChanceEasy, tuning.skillUseChanceHard, skill))
        {
            input.skillPressed = true;
        }

        FireChampionAiDecision decision = new FireChampionAiDecision();
        decision.input = input;
        decision.showAdaptationBanner = context.bannerTimer <= 0.0f && profile.TotalAiSamples() > tuning.adaptationBannerSampleThreshold && UnityEngine.Random.value < context.deltaTime * tuning.adaptationBannerChancePerSecond;
        return decision;
    }

    public static float HomeX(BalanceAi tuning, Side side)
    {
        tuning = tuning ?? new BalanceAi();
        return side == Side.Right ? tuning.homeX : -tuning.homeX;
    }

    public static float ClampTargetX(BalanceAi tuning, Side side, float x, float courtHalfWidth)
    {
        tuning = tuning ?? new BalanceAi();
        if (side == Side.Right)
        {
            return Mathf.Clamp(x, tuning.clampInner, courtHalfWidth - tuning.clampBackMargin);
        }

        return Mathf.Clamp(x, -courtHalfWidth + tuning.clampBackMargin, -tuning.clampInner);
    }

    public static float PredictLandingXFor(Side side, FireChampionAiContext context)
    {
        BalanceGameplay gameplay = context.gameplay ?? new BalanceGameplay();
        Vector2 pos = context.shuttle.position;
        Vector2 vel = context.shuttle.velocity;
        for (int i = 0; i < gameplay.court.predictionIterations; i++)
        {
            vel += new Vector2(context.courtWind.x, context.gravity + context.courtWind.y) * gameplay.court.predictionStep;
            vel *= Mathf.Pow(context.shuttleDrag, gameplay.court.predictionStep);
            pos += vel * gameplay.court.predictionStep * context.courtSpeedMultiplier;
            if (pos.y <= gameplay.court.predictionGroundY)
            {
                break;
            }
        }

        if (side == Side.Right)
        {
            return Mathf.Clamp(pos.x, gameplay.court.predictionInnerClamp, context.courtHalfWidth - gameplay.court.predictionBackMargin);
        }

        return Mathf.Clamp(pos.x, -context.courtHalfWidth + gameplay.court.predictionBackMargin, -gameplay.court.predictionInnerClamp);
    }

    private static void ApplyShotChoice(ref GameInput input, PlayerActor ai, PlayerActor opponent, FireChampionAiContext context, float skill)
    {
        BalanceAi tuning = context.tuning ?? new BalanceAi();
        ProfileData profile = context.profile ?? new ProfileData();
        ShuttleState shuttle = context.shuttle;
        float opponentDepth = Mathf.Abs(opponent.x);
        if (opponentDepth >= tuning.tacticalDropOpponentBackX
            && shuttle.position.y <= tuning.tacticalDropMaxY
            && UnityEngine.Random.value < Mathf.Lerp(tuning.tacticalDropChanceEasy, tuning.tacticalDropChanceHard, skill))
        {
            input.dropIntent = true;
            input.vertical = 0;
            return;
        }

        if (opponentDepth <= tuning.tacticalLiftOpponentFrontX
            && UnityEngine.Random.value < Mathf.Lerp(tuning.tacticalLiftChanceEasy, tuning.tacticalLiftChanceHard, skill))
        {
            input.vertical = 1;
            return;
        }

        bool canSmash = shuttle.position.y > Mathf.Lerp(tuning.smashableYEasy, tuning.smashableYHard, skill);
        float opponentOutOfPosition = Mathf.InverseLerp(tuning.opponentOutMin, tuning.opponentOutMax, Mathf.Abs(opponent.x));
        float smashChance = Mathf.Lerp(tuning.smashChanceEasy, tuning.smashChanceHard, skill) + opponentOutOfPosition * tuning.opponentOutSmashBonus + tuning.pressureSmashChanceBonus * skill;
        if (canSmash && UnityEngine.Random.value < smashChance)
        {
            input.vertical = -1;
            return;
        }

        if (shuttle.position.y < tuning.lowShotLiftY)
        {
            input.vertical = 1;
            return;
        }

        if (profile.aiSmashRatio > tuning.antiSmashRatioThreshold && UnityEngine.Random.value < Mathf.Lerp(tuning.antiSmashLiftChanceEasy, tuning.antiSmashLiftChanceHard, skill))
        {
            input.vertical = 1;
            return;
        }

        input.vertical = UnityEngine.Random.value < Mathf.Lerp(tuning.randomLiftChanceEasy, tuning.randomLiftChanceHard, skill) ? 1 : 0;
    }
}
