using UnityEngine;

public sealed class FireChampionAiSimulationReport
{
    public int characterCount;
    public int courtCount;
    public int scenarioCount;
    public int movementDecisions;
    public int swingDecisions;
    public int jumpDecisions;
    public int tacticalDropDecisions;
    public int skillDecisions;
    public int skillHoldDecisions;
    public int adaptationBannerDecisions;
    public int targetClampViolations;
    public int rallyScenarioCount;
    public int rallyFrameCount;
    public int rallyContacts;
    public int rallyAiContacts;
    public int rallyFeederContacts;
    public int rallyMisses;
    public int rallyLongestExchange;
    public int rallyTargetClampViolations;
    public int rallyOutOfBounds;
    public int rallyInvalidFrames;

    public string Summary()
    {
        return "characters=" + characterCount
            + ", courts=" + courtCount
            + ", scenarios=" + scenarioCount
            + ", movement=" + movementDecisions
            + ", swing=" + swingDecisions
            + ", jump=" + jumpDecisions
            + ", tacticalDrop=" + tacticalDropDecisions
            + ", skill=" + skillDecisions
            + ", skillHold=" + skillHoldDecisions
            + ", adaptation=" + adaptationBannerDecisions
            + ", clampViolations=" + targetClampViolations
            + ", rallyScenarios=" + rallyScenarioCount
            + ", rallyFrames=" + rallyFrameCount
            + ", rallyContacts=" + rallyContacts
            + ", rallyAiContacts=" + rallyAiContacts
            + ", rallyFeederContacts=" + rallyFeederContacts
            + ", rallyMisses=" + rallyMisses
            + ", rallyLongestExchange=" + rallyLongestExchange
            + ", rallyClampViolations=" + rallyTargetClampViolations
            + ", rallyOut=" + rallyOutOfBounds
            + ", rallyInvalid=" + rallyInvalidFrames;
    }
}

public static class FireChampionAiSimulation
{
    private const float AiStartX = 4.8f;
    private const float OpponentStartX = -4.3f;
    private const float RacketY = 1.22f;
    private const float RallyStep = 0.016f;
    private const int RallyFrameBudget = 420;
    private const float RallyFeederX = -4.6f;
    private const float RallyFeederRacketY = 1.18f;
    private const float RallyLaunchY = 1.35f;

    public static FireChampionAiSimulationReport Run(FireChampionBalanceConfig balance)
    {
        FireChampionAiSimulationReport report = new FireChampionAiSimulationReport();
        balance = balance ?? FireChampionBalanceConfig.CreateDefault();

        CharacterConfig[] roster = balance.CreateCharacterRoster();
        CourtConfig[] courts = balance.CreateCourtConfigs();
        report.characterCount = roster == null ? 0 : roster.Length;
        report.courtCount = courts == null ? 0 : courts.Length;
        if (roster == null || courts == null || roster.Length == 0 || courts.Length == 0)
        {
            return report;
        }

        UnityEngine.Random.State previousRandomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(17491);
        try
        {
            BalanceAi tuning = CreateDeterministicTuning(balance.ai);
            RulesetConfig rules = RulesetConfig.Practice();
            for (int roleIndex = 0; roleIndex < roster.Length; roleIndex++)
            {
                for (int courtIndex = 0; courtIndex < courts.Length; courtIndex++)
                {
                    RunHomeRecoveryScenario(report, balance, tuning, rules, roster[roleIndex], roleIndex, courts[courtIndex]);
                    RunReachSwingScenario(report, balance, tuning, rules, roster[roleIndex], roleIndex, courts[courtIndex]);
                    RunHighThreatScenario(report, balance, tuning, rules, roster[roleIndex], roleIndex, courts[courtIndex]);
                    RunSkillReadyScenario(report, balance, tuning, rules, roster[roleIndex], roleIndex, courts[courtIndex]);
                    RunAdaptationScenario(report, balance, tuning, rules, roster[roleIndex], roleIndex, courts[courtIndex]);
                    RunRallyHealthScenario(report, balance, tuning, rules, roster, roleIndex, courts[courtIndex], courtIndex);
                }
            }
        }
        finally
        {
            UnityEngine.Random.state = previousRandomState;
        }

        return report;
    }

    private static BalanceAi CreateDeterministicTuning(BalanceAi source)
    {
        BalanceAi tuning = source == null ? new BalanceAi() : JsonUtility.FromJson<BalanceAi>(JsonUtility.ToJson(source));
        tuning.randomMistakeChanceEasy = 0.0f;
        tuning.randomMistakeChanceHard = 0.0f;
        tuning.movementDeadzoneEasy = 0.01f;
        tuning.movementDeadzoneHard = 0.01f;
        tuning.jumpChanceEasy = 1.0f;
        tuning.jumpChanceHard = 1.0f;
        tuning.smashPrepChanceEasy = 1.0f;
        tuning.smashPrepChanceHard = 1.0f;
        tuning.swingChanceEasy = 1.0f;
        tuning.swingChanceHard = 1.0f;
        tuning.tacticalDropChanceEasy = 1.0f;
        tuning.tacticalDropChanceHard = 1.0f;
        tuning.tacticalLiftChanceEasy = 1.0f;
        tuning.tacticalLiftChanceHard = 1.0f;
        tuning.skillHoldChanceEasy = 1.0f;
        tuning.skillHoldChanceHard = 1.0f;
        tuning.skillUseChanceEasy = 1.0f;
        tuning.skillUseChanceHard = 1.0f;
        tuning.adaptationBannerChancePerSecond = 1.0f;
        return tuning;
    }

    private static void RunHomeRecoveryScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court)
    {
        ShuttleState shuttle = new ShuttleState
        {
            position = new Vector2(-4.2f, 1.1f),
            velocity = new Vector2(-1.3f, 0.0f),
            spin = 0.0f
        };
        FireChampionAiDecision decision = RunScenario(report, balance, tuning, rules, character, roleIndex, court, shuttle, new ProfileData(), 0.016f, 0.0f, tuning.skillUseChanceHard + 1.0f);
        CountDecision(report, decision);
    }

    private static void RunReachSwingScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court)
    {
        ShuttleState shuttle = new ShuttleState
        {
            position = new Vector2(AiStartX + 0.05f, RacketY),
            velocity = new Vector2(1.8f, 0.15f),
            spin = 0.0f
        };
        FireChampionAiDecision decision = RunScenario(report, balance, tuning, rules, character, roleIndex, court, shuttle, new ProfileData(), 0.016f, balance.gameplay.energy.ultimateCost, tuning.skillUseChanceHard + 1.0f);
        CountDecision(report, decision);
    }

    private static void RunHighThreatScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court)
    {
        ShuttleState shuttle = new ShuttleState
        {
            position = new Vector2(AiStartX - 0.1f, Mathf.Max(1.8f, tuning.jumpBallY + 0.25f)),
            velocity = new Vector2(2.0f, -0.35f),
            spin = 0.0f
        };
        FireChampionAiDecision decision = RunScenario(report, balance, tuning, rules, character, roleIndex, court, shuttle, new ProfileData(), 0.016f, balance.gameplay.energy.ultimateCost, tuning.skillUseChanceHard + 1.0f);
        CountDecision(report, decision);
    }

    private static void RunSkillReadyScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court)
    {
        ShuttleState shuttle = new ShuttleState
        {
            position = new Vector2(-4.4f, 1.2f),
            velocity = new Vector2(-1.1f, 0.0f),
            spin = 0.0f
        };
        FireChampionAiDecision decision = RunScenario(report, balance, tuning, rules, character, roleIndex, court, shuttle, new ProfileData(), 1.0f, balance.gameplay.energy.skillCost, 0.0f);
        CountDecision(report, decision);
    }

    private static void RunAdaptationScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court)
    {
        ShuttleState shuttle = new ShuttleState
        {
            position = new Vector2(-4.0f, 1.0f),
            velocity = new Vector2(-1.0f, 0.0f),
            spin = 0.0f
        };
        FireChampionAiDecision decision = RunScenario(report, balance, tuning, rules, character, roleIndex, court, shuttle, CreateHabitProfile(tuning), 1.0f, 0.0f, 0.0f);
        CountDecision(report, decision);
    }

    private static FireChampionAiDecision RunScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig character, int roleIndex, CourtConfig court, ShuttleState shuttle, ProfileData profile, float deltaTime, float energy, float bannerTimer)
    {
        PlayerActor ai = new PlayerActor(Side.Right, roleIndex, AiStartX);
        ai.isAi = true;
        ai.aiDifficulty = 1.0f;
        ai.aiThinkTimer = 0.0f;
        ai.energy = energy;

        PlayerActor opponent = new PlayerActor(Side.Left, 0, OpponentStartX);
        FireChampionAiContext context = new FireChampionAiContext();
        context.tuning = tuning;
        context.gameplay = balance.gameplay;
        context.rules = rules;
        context.character = character;
        context.profile = profile;
        context.shuttle = shuttle;
        context.racketPosition = new Vector2(ai.x, RacketY);
        context.courtWind = RepresentativeCourtWind(court);
        context.gravity = balance.gameplay.court.gravity;
        context.shuttleDrag = balance.gameplay.court.shuttleDrag;
        context.courtSpeedMultiplier = rules.ballSpeedMultiplier;
        context.courtHalfWidth = balance.gameplay.court.courtHalfWidth;
        context.bannerTimer = bannerTimer;
        context.deltaTime = deltaTime;

        FireChampionAiDecision decision = FireChampionAiController.BuildInput(ai, opponent, context);
        report.scenarioCount++;
        float backLimit = balance.gameplay.court.courtHalfWidth - tuning.clampBackMargin + 0.001f;
        if (ai.aiTargetX < tuning.clampInner - 0.001f || ai.aiTargetX > backLimit)
        {
            report.targetClampViolations++;
        }

        return decision;
    }

    private static void RunRallyHealthScenario(FireChampionAiSimulationReport report, FireChampionBalanceConfig balance, BalanceAi tuning, RulesetConfig rules, CharacterConfig[] roster, int roleIndex, CourtConfig court, int courtIndex)
    {
        CharacterConfig aiCharacter = roster[roleIndex];
        CharacterConfig feederCharacter = roster[0];
        PlayerActor ai = new PlayerActor(Side.Right, roleIndex, AiStartX);
        ai.isAi = true;
        ai.aiDifficulty = 1.0f;
        ai.energy = balance.gameplay.energy.ultimateCost;

        PlayerActor feeder = new PlayerActor(Side.Left, 0, OpponentStartX);
        ShuttleState shuttle = new ShuttleState();
        LaunchRallyFeederShot(ref shuttle, 0);
        int exchange = 0;
        Vector2 courtWind = RepresentativeCourtWind(court);
        report.rallyScenarioCount++;

        for (int frame = 0; frame < RallyFrameBudget; frame++)
        {
            report.rallyFrameCount++;
            if (!IsFinite(shuttle.position) || !IsFinite(shuttle.velocity))
            {
                report.rallyInvalidFrames++;
                exchange = 0;
                LaunchRallyFeederShot(ref shuttle, frame + courtIndex);
                continue;
            }

            FireChampionAiContext context = new FireChampionAiContext();
            context.tuning = tuning;
            context.gameplay = balance.gameplay;
            context.rules = rules;
            context.character = aiCharacter;
            context.profile = new ProfileData();
            context.shuttle = shuttle;
            context.racketPosition = new Vector2(ai.x, ai.y + RacketY);
            context.courtWind = courtWind;
            context.gravity = balance.gameplay.court.gravity;
            context.shuttleDrag = balance.gameplay.court.shuttleDrag;
            context.courtSpeedMultiplier = rules.ballSpeedMultiplier;
            context.courtHalfWidth = balance.gameplay.court.courtHalfWidth;
            context.bannerTimer = 1.0f;
            context.deltaTime = RallyStep;

            FireChampionAiDecision decision = FireChampionAiController.BuildInput(ai, feeder, context);
            StepRallyActor(ai, aiCharacter, decision.input, balance.gameplay, RallyStep, balance.gameplay.court.courtHalfWidth);
            CountRallyClamp(report, tuning, ai, balance.gameplay.court.courtHalfWidth);

            Vector2 aiRacket = new Vector2(ai.x, ai.y + RacketY);
            if (decision.input.swingPressed && RallyHitInReach(shuttle, aiRacket, aiCharacter, tuning))
            {
                ApplyRallyAiHit(ref shuttle, aiRacket, aiCharacter, decision.input, balance.gameplay);
                exchange++;
                report.rallyContacts++;
                report.rallyAiContacts++;
                report.rallyLongestExchange = Mathf.Max(report.rallyLongestExchange, exchange);
                ai.energy = Mathf.Min(balance.gameplay.energy.maxEnergy, ai.energy + balance.gameplay.energy.normalHitGain);
                continue;
            }

            Vector2 feederRacket = new Vector2(RallyFeederX, RallyFeederRacketY);
            if (RallyFeederCanReturn(shuttle, feederRacket, feederCharacter))
            {
                LaunchRallyFeederShot(ref shuttle, frame + courtIndex);
                exchange++;
                report.rallyContacts++;
                report.rallyFeederContacts++;
                report.rallyLongestExchange = Mathf.Max(report.rallyLongestExchange, exchange);
                continue;
            }

            StepRallyShuttle(ref shuttle, balance.gameplay, courtWind, rules.ballSpeedMultiplier, RallyStep);
            if (RallyBallDead(shuttle, balance.gameplay))
            {
                if (Mathf.Abs(shuttle.position.x) > balance.gameplay.court.courtHalfWidth + balance.gameplay.court.outOfBoundsMargin)
                {
                    report.rallyOutOfBounds++;
                }

                report.rallyMisses++;
                report.rallyLongestExchange = Mathf.Max(report.rallyLongestExchange, exchange);
                exchange = 0;
                ai.energy = balance.gameplay.energy.ultimateCost;
                LaunchRallyFeederShot(ref shuttle, frame + roleIndex + courtIndex);
            }
        }
    }

    private static void StepRallyActor(PlayerActor player, CharacterConfig character, GameInput input, BalanceGameplay gameplay, float dt, float courtHalfWidth)
    {
        float speed = character == null ? 5.2f : character.moveSpeed;
        player.x += input.horizontal * speed * dt;
        if (player.side == Side.Right)
        {
            player.x = Mathf.Clamp(player.x, gameplay.court.centerGuardMargin, courtHalfWidth - gameplay.court.playerBackMargin);
        }
        else
        {
            player.x = Mathf.Clamp(player.x, -courtHalfWidth + gameplay.court.playerBackMargin, -gameplay.court.centerGuardMargin);
        }

        if (input.jumpPressed && player.grounded)
        {
            player.vy = character == null ? 6.3f : character.jumpForce;
            player.grounded = false;
        }

        if (!player.grounded)
        {
            player.vy += gameplay.court.gravity * gameplay.court.gravityMultiplier * dt;
            player.y += player.vy * dt;
            if (player.y <= gameplay.court.groundY)
            {
                player.y = gameplay.court.groundY;
                player.vy = 0.0f;
                player.grounded = true;
            }
        }
    }

    private static void StepRallyShuttle(ref ShuttleState shuttle, BalanceGameplay gameplay, Vector2 courtWind, float speedMultiplier, float dt)
    {
        if (Mathf.Abs(shuttle.spin) > gameplay.court.spinActiveThreshold)
        {
            shuttle.velocity += new Vector2(shuttle.spin * gameplay.court.spinInfluenceX, shuttle.spin * gameplay.court.spinInfluenceY) * dt;
            shuttle.spin *= Mathf.Pow(gameplay.court.spinDecay, dt);
        }

        shuttle.velocity += new Vector2(courtWind.x, gameplay.court.gravity + courtWind.y) * dt;
        shuttle.velocity *= Mathf.Pow(gameplay.court.shuttleDrag, dt);
        shuttle.position += shuttle.velocity * dt * speedMultiplier;
    }

    private static bool RallyHitInReach(ShuttleState shuttle, Vector2 racketPosition, CharacterConfig character, BalanceAi tuning)
    {
        float radius = (character == null ? 0.82f : character.hitRadius) * Mathf.Max(1.0f, tuning.swingReachHard) + 0.08f;
        return shuttle.velocity.x > -tuning.movingTowardThreshold && Vector2.Distance(shuttle.position, racketPosition) <= radius;
    }

    private static bool RallyFeederCanReturn(ShuttleState shuttle, Vector2 racketPosition, CharacterConfig feederCharacter)
    {
        float radius = (feederCharacter == null ? 0.86f : feederCharacter.hitRadius) + 0.18f;
        return shuttle.velocity.x < 0.0f && Vector2.Distance(shuttle.position, racketPosition) <= radius;
    }

    private static void ApplyRallyAiHit(ref ShuttleState shuttle, Vector2 racketPosition, CharacterConfig character, GameInput input, BalanceGameplay gameplay)
    {
        BalanceShotTuning shot = gameplay.shot;
        float forward;
        float lift;
        float spin;
        if (input.dropIntent)
        {
            forward = shot.dropForward;
            lift = shot.dropLift;
            spin = 0.42f;
        }
        else if (input.vertical < 0)
        {
            forward = Mathf.Max(6.8f, shot.smashPower + (character == null ? 0.0f : character.smashBonus));
            lift = Mathf.Lerp(shot.farSmashVertical, shot.nearSmashVertical, 0.45f);
            spin = -0.24f;
        }
        else if (input.vertical > 0)
        {
            forward = shot.highForward;
            lift = shot.highLift * 0.72f;
            spin = 0.18f;
        }
        else
        {
            forward = shot.flatSpeed;
            lift = shot.flatLift;
            spin = 0.0f;
        }

        shuttle.position = racketPosition + new Vector2(-shot.hitPositionForwardOffset, shot.hitPositionYOffset);
        shuttle.velocity = new Vector2(-forward, lift);
        shuttle.spin = spin;
    }

    private static void LaunchRallyFeederShot(ref ShuttleState shuttle, int seed)
    {
        float xJitter = (seed % 3 - 1) * 0.08f;
        float liftJitter = (seed % 4) * 0.08f;
        shuttle.position = new Vector2(RallyFeederX + xJitter, RallyLaunchY);
        shuttle.velocity = new Vector2(7.35f + xJitter, 5.75f + liftJitter);
        shuttle.spin = seed % 2 == 0 ? 0.08f : -0.08f;
    }

    private static bool RallyBallDead(ShuttleState shuttle, BalanceGameplay gameplay)
    {
        return shuttle.position.y <= gameplay.court.groundY
            || Mathf.Abs(shuttle.position.x) > gameplay.court.courtHalfWidth + gameplay.court.outOfBoundsMargin
            || shuttle.position.y > 8.0f;
    }

    private static bool IsFinite(Vector2 value)
    {
        return !float.IsNaN(value.x)
            && !float.IsNaN(value.y)
            && !float.IsInfinity(value.x)
            && !float.IsInfinity(value.y);
    }

    private static void CountRallyClamp(FireChampionAiSimulationReport report, BalanceAi tuning, PlayerActor ai, float courtHalfWidth)
    {
        float backLimit = courtHalfWidth - tuning.clampBackMargin + 0.001f;
        if (ai.aiTargetX < tuning.clampInner - 0.001f || ai.aiTargetX > backLimit)
        {
            report.rallyTargetClampViolations++;
        }
    }

    private static Vector2 RepresentativeCourtWind(CourtConfig court)
    {
        if (court == null)
        {
            return Vector2.zero;
        }

        return new Vector2(court.windX * 0.35f, court.windY * 0.5f);
    }

    private static ProfileData CreateHabitProfile(BalanceAi tuning)
    {
        ProfileData profile = new ProfileData();
        int samples = Mathf.Max(10, tuning.adaptationBannerSampleThreshold + 2);
        for (int i = 0; i < samples; i++)
        {
            profile.RecordHumanShot(true, ShotType.Smash);
        }

        return profile;
    }

    private static void CountDecision(FireChampionAiSimulationReport report, FireChampionAiDecision decision)
    {
        if (decision.input.horizontal != 0)
        {
            report.movementDecisions++;
        }

        if (decision.input.swingPressed)
        {
            report.swingDecisions++;
        }

        if (decision.input.jumpPressed)
        {
            report.jumpDecisions++;
        }

        if (decision.input.dropIntent)
        {
            report.tacticalDropDecisions++;
        }

        if (decision.input.skillPressed)
        {
            report.skillDecisions++;
        }

        if (decision.input.skillHeld)
        {
            report.skillHoldDecisions++;
        }

        if (decision.showAdaptationBanner)
        {
            report.adaptationBannerDecisions++;
        }
    }
}
