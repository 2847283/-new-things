using System.Globalization;
using UnityEngine;

public sealed class FireChampionGame : MonoBehaviour
{
    private float CourtHalfWidth = 8.0f;
    private float GroundY = 0.0f;
    private float NetHeight = 1.25f;
    private float NetWidth = 0.12f;
    private float Gravity = -9.4f;
    private float ShuttleDrag = 0.72f;

    private FireChampionBalanceConfig balance;
    private BalanceGameplay gameplay;
    private CharacterConfig[] roster;
    private CourtConfig[] courts;
    private CosmeticConfig[] cosmetics;

    private ProfileData profile;
    private RulesetConfig rules;
    private RulesetConfig networkRules;
    private PlayerActor leftPlayer;
    private PlayerActor rightPlayer;
    private ShuttleState shuttle;
    private MatchStats stats;
    private DirectIpSession network;
    private AudioSource audioSource;
    private ToneAudioPlayer tonePlayer;
    private FireChampionAudioAssets audioAssets;
    private Texture2D pixel;
    private Texture2D discTexture;
    private FireChampionUiAssets uiAssets;
    private FireChampionCourtAssets courtAssets;
    private readonly PracticeDrillState practiceDrill = new PracticeDrillState();
    private readonly HitTimingFeedback hitTimingFeedback = new HitTimingFeedback();
    private readonly PracticeSessionStats practiceSessionStats = new PracticeSessionStats();
    private readonly PracticeShotTimeline practiceShotTimeline = new PracticeShotTimeline();
    private readonly FireChampionVfxSystem vfxSystem = new FireChampionVfxSystem();

    private ScreenState screen = ScreenState.MainMenu;
    private ScreenState lastObservedScreen = (ScreenState)(-1);
    private GameMode mode = GameMode.QuickAi;
    private GameInput leftInput;
    private GameInput rightInput;
    private int leftScore;
    private int rightScore;
    private int leftGames;
    private int rightGames;
    private int tournamentRound;
    private float scale;
    private float courtBottom;
    private float bannerTimer;
    private float networkSendTimer;
    private string banner = "";
    private string summaryTitle = "";
    private string summaryDetails = "";
    private string joinIp = "127.0.0.1";
    private string portText = "";
    private string rebindTarget = "";
    private bool waitingForServe;
    private Side server = Side.Left;
    private Side lastHitter = Side.Left;
    private bool matchOver;
    private bool lastMatchPlayerWon;
    private bool paused;
    private bool showCourtRulesInPvp;
    private bool showAbilitiesInPvp;
    private bool practiceFeeder = true;
    private bool confirmTournamentRestart;
    private bool qaFreezeMatch;
    private bool qaAutoPlay;
    private bool qaSuppressProfileSave;
    private float sandboxBallSpeed = 1.0f;
    private float sandboxEnergy = 1.0f;
    private float courtPhase;
    private float screenShakeTimer;
    private float screenShakeMagnitude;
    private Vector2 screenShakeOffset;
    private Vector2 mainMenuScroll;
    private Vector2 infoScroll;
    private Vector2 settingsScroll;
    private Vector2 practiceToolsScroll;
    private Vector2 tournamentPanelScroll;
    private int tutorialStep;
    private int tutorialLeftHits;
    private int tutorialSkillBaseline;
    private float tutorialStepTimer;
    private string tutorialRecoveryHint = "";
    private string tutorialFailureReason = "";
    private float networkJumpPulseTimer;
    private float networkSwingPulseTimer;
    private float networkSkillPulseTimer;
    private bool tutorialMoved;
    private bool tutorialJumped;
    private ShotType tutorialLastLeftShot = ShotType.Flat;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
        pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        pixel.SetPixel(0, 0, Color.white);
        pixel.Apply();
        discTexture = FireChampionGuiDrawing.CreateDiscTexture(96);
        uiAssets = FireChampionUiAssets.Load();
        courtAssets = FireChampionCourtAssets.Load();
        balance = FireChampionBalanceConfig.Load();
        gameplay = balance.gameplay;
        ApplyGameplayTuning();
        roster = balance.CreateCharacterRoster();
        courts = balance.CreateCourtConfigs();
        cosmetics = balance.CreateCosmetics();
        networkRules = balance.StandardRules();
        portText = balance.network.defaultPort.ToString(CultureInfo.InvariantCulture);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        tonePlayer = new ToneAudioPlayer();
        audioAssets = FireChampionAudioAssets.Load();
        profile = ProfileStore.Load();
        NormalizeProfile();
        ApplyQaCourtOverride(FireChampionQaLaunch.CourtIndexFromCommandLine());
        rules = balance.StandardRules();
        network = new DirectIpSession(balance.network.pingIntervalSeconds, balance.network.pingRetrySeconds);
        StartMatch(GameMode.QuickAi);
        screen = ScreenState.MainMenu;
        ApplyQaLaunch(FireChampionQaLaunch.FromCommandLine());
        ApplyQaCapture(FireChampionQaCapture.FromCommandLine());
    }

    private void ApplyGameplayTuning()
    {
        CourtHalfWidth = gameplay.court.courtHalfWidth;
        GroundY = gameplay.court.groundY;
        NetHeight = gameplay.court.netHeight;
        NetWidth = gameplay.court.netWidth;
        Gravity = gameplay.court.gravity;
        ShuttleDrag = gameplay.court.shuttleDrag;
        sandboxBallSpeed = gameplay.practice.defaultBallSpeed;
        sandboxEnergy = gameplay.practice.defaultEnergyMultiplier;
    }

    private void OnDestroy()
    {
        if (network != null)
        {
            network.Stop();
        }

        if (qaSuppressProfileSave)
        {
            return;
        }

        CommitPracticeSessionToProfile(false);
        ProfileStore.Save(profile);
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, gameplay.court.maxFrameDelta);
        courtPhase += dt;
        UpdateScreenShake(dt);
        if (!qaFreezeMatch)
        {
            vfxSystem.Update(dt);
        }
        if (!string.IsNullOrEmpty(rebindTarget))
        {
            CaptureRebindKey();
            return;
        }

        if (screen != ScreenState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
        }

        if (mode == GameMode.NetworkHost || mode == GameMode.NetworkClient)
        {
            network.Tick();
        }

        if (qaFreezeMatch || paused || matchOver)
        {
            return;
        }

        if ((mode == GameMode.NetworkHost || mode == GameMode.NetworkClient) && !network.IsConnected)
        {
            return;
        }

        if (mode == GameMode.NetworkClient)
        {
            UpdateNetworkClient(dt);
            return;
        }

        ReadInputs();
        if (qaAutoPlay)
        {
            leftInput = BuildAiInput(leftPlayer, rightPlayer, dt);
        }

        if (mode == GameMode.NetworkHost)
        {
            rightInput = network.RemoteInput;
        }
        else if (rightPlayer.isAi)
        {
            rightInput = BuildAiInput(rightPlayer, leftPlayer, dt);
        }

        UpdatePlayer(leftPlayer, leftInput, dt);
        UpdatePlayer(rightPlayer, rightInput, dt);
        UpdateSkill(leftPlayer, leftInput, dt);
        UpdateSkill(rightPlayer, rightInput, dt);
        UpdateServeOrRally(dt);
        UpdateEnergy(dt);
        UpdateTutorialCoachTimer(dt);
        UpdateTutorialProgress();

        if (mode == GameMode.NetworkHost)
        {
            networkSendTimer -= dt;
            if (networkSendTimer <= 0.0f)
            {
                networkSendTimer = gameplay.court.networkSendInterval;
                network.SendSnapshot(CaptureSnapshot());
            }
        }
    }

    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(rebindTarget))
        {
            CaptureRebindKey();
        }

        GUI.skin.label.fontSize = 18;
        GUI.skin.label.richText = true;
        GUI.skin.label.wordWrap = true;
        GUI.skin.button.fontSize = 17;
        GUI.skin.box.fontSize = 17;
        GUI.skin.box.richText = true;
        GUI.skin.textField.fontSize = 16;
        HandleScreenEntry();

        bool matchView = screen == ScreenState.Playing || screen == ScreenState.Summary;
        if (matchView)
        {
            DrawGameWorld();
            if (screen == ScreenState.Summary)
            {
                FireChampionOverlayRenderer.DrawSummaryBackdrop(pixel, lastMatchPlayerWon);
            }
            else
            {
                DrawHud();
            }
        }
        else
        {
            DrawMenuBackdrop();
        }

        switch (screen)
        {
            case ScreenState.MainMenu:
                DrawMainMenu();
                break;
            case ScreenState.Settings:
                DrawSettings();
                break;
            case ScreenState.Network:
                DrawNetworkMenu();
                break;
            case ScreenState.Playing:
                DrawPlayingOverlay();
                break;
            case ScreenState.Summary:
                DrawSummary();
                break;
        }
    }

    private void HandleScreenEntry()
    {
        if (screen == lastObservedScreen)
        {
            return;
        }

        if (screen == ScreenState.MainMenu)
        {
            mainMenuScroll = Vector2.zero;
            infoScroll = Vector2.zero;
        }
        else if (screen == ScreenState.Settings)
        {
            settingsScroll = Vector2.zero;
        }

        lastObservedScreen = screen;
    }

    private void ApplyQaLaunch(FireChampionQaScreen qaScreen)
    {
        if (qaScreen == FireChampionQaScreen.None)
        {
            return;
        }

        qaSuppressProfileSave = true;

        if (qaScreen == FireChampionQaScreen.Settings)
        {
            screen = ScreenState.Settings;
            return;
        }

        if (qaScreen == FireChampionQaScreen.Network)
        {
            screen = ScreenState.Network;
            return;
        }

        if (qaScreen == FireChampionQaScreen.Practice)
        {
            StartMatch(GameMode.Practice);
            return;
        }

        if (qaScreen == FireChampionQaScreen.Tutorial)
        {
            StartMatch(GameMode.Tutorial);
            return;
        }

        if (qaScreen == FireChampionQaScreen.Tournament)
        {
            tournamentRound = Mathf.Clamp(tournamentRound, 0, TournamentProgression.RoundCount - 1);
            StartMatch(GameMode.Tournament);
            return;
        }

        if (qaScreen == FireChampionQaScreen.Pause)
        {
            StartMatch(GameMode.QuickAi);
            paused = true;
            return;
        }

        if (qaScreen == FireChampionQaScreen.Summary)
        {
            PrepareQaSummary(FireChampionQaLaunch.SummaryVariantFromCommandLine());
            return;
        }

        if (qaScreen == FireChampionQaScreen.VfxPreview)
        {
            StartMatch(GameMode.Practice);
            PrepareQaVfxPreview();
            return;
        }

        if (qaScreen == FireChampionQaScreen.QuickMatch)
        {
            StartMatch(GameMode.QuickAi);
            PrepareQaQuickMatch();
            return;
        }

        if (qaScreen == FireChampionQaScreen.AutoMatch)
        {
            StartMatch(GameMode.QuickAi);
            PrepareQaAutoMatch();
            return;
        }

        if (qaScreen == FireChampionQaScreen.NetworkWaiting)
        {
            StartMatch(GameMode.NetworkHost);
        }
    }

    private void ApplyQaCourtOverride(int courtIndex)
    {
        if (courtIndex < 0 || profile == null || courts == null || courts.Length == 0)
        {
            return;
        }

        profile.selectedCourt = Mathf.Clamp(courtIndex, 0, courts.Length - 1);
    }

    private void ApplyQaCapture(FireChampionQaCaptureConfig config)
    {
        if (config == null || !config.IsRequested)
        {
            return;
        }

        qaSuppressProfileSave = true;
        StartCoroutine(FireChampionQaCapture.CaptureAndMaybeQuit(config));
    }

    private void PrepareQaSummary(FireChampionQaSummaryVariant variant)
    {
        if (variant == FireChampionQaSummaryVariant.TournamentFinalWin)
        {
            tournamentRound = TournamentProgression.RoundCount - 1;
            StartMatch(GameMode.Tournament);
        }
        else if (variant == FireChampionQaSummaryVariant.NetworkClient)
        {
            StartMatch(GameMode.NetworkClient);
        }
        else
        {
            StartMatch(GameMode.QuickAi);
        }

        bool playerWon = variant != FireChampionQaSummaryVariant.Loss;
        leftScore = playerWon ? rules.pointsToWin : Mathf.Max(0, rules.pointsToWin - 2);
        rightScore = playerWon ? Mathf.Max(0, rules.pointsToWin - 2) : rules.pointsToWin;
        stats.longestRally = 7;
        stats.smashWinners = playerWon ? 2 : 1;
        stats.errors = playerWon ? 3 : 6;
        stats.skillsUsed = 2;
        stats.serveFaults = 1;
        matchOver = true;
        lastMatchPlayerWon = playerWon;
        waitingForServe = false;
        qaFreezeMatch = true;
        summaryTitle = playerWon ? "胜利!" : "惜败";
        if (variant == FireChampionQaSummaryVariant.TournamentFinalWin)
        {
            summaryDetails = "QA 锦标赛终局 · 比分 " + leftScore + ":" + rightScore;
        }
        else if (variant == FireChampionQaSummaryVariant.NetworkClient)
        {
            summaryDetails = "QA 联机结算 · 比分 " + leftScore + ":" + rightScore;
        }
        else if (variant == FireChampionQaSummaryVariant.Loss)
        {
            summaryDetails = "QA 失败结算 · 比分 " + leftScore + ":" + rightScore;
        }
        else
        {
            summaryDetails = "QA 结算预览 · 比分 " + leftScore + ":" + rightScore;
        }

        screen = ScreenState.Summary;
    }

    private void PrepareQaVfxPreview()
    {
        waitingForServe = false;
        paused = false;
        qaFreezeMatch = true;
        leftPlayer.x = -2.65f;
        leftPlayer.y = 0.0f;
        leftPlayer.facing = 1;
        leftPlayer.energy = gameplay.energy.ultimateCost;
        leftPlayer.swingTimer = gameplay.swing.swingPoseTimer;
        rightPlayer.x = 2.65f;
        rightPlayer.y = 0.0f;
        rightPlayer.facing = -1;
        rightPlayer.energy = gameplay.energy.skillCost;
        rightPlayer.swingTimer = gameplay.swing.swingPoseTimer;
        shuttle.position = new Vector2(0.15f, 1.85f);
        shuttle.velocity = new Vector2(3.2f, -1.0f);
        shuttle.spin = 0.7f;
        vfxSystem.Clear();
        vfxSystem.SpawnHit(new Vector2(-2.2f, 1.3f), ShotType.Flat, true, ActorAccent(leftPlayer), leftPlayer.facing, gameplay.feedback);
        vfxSystem.SpawnHit(new Vector2(0.05f, 1.75f), ShotType.Smash, true, ActorAccent(rightPlayer), rightPlayer.facing, gameplay.feedback);
        vfxSystem.SpawnSkill(new Vector2(-2.65f, 0.92f), ActorAccent(leftPlayer), leftPlayer.facing, gameplay.feedback);
        vfxSystem.SpawnScore(new Vector2(2.15f, 0.42f), ActorAccent(rightPlayer), true, gameplay.feedback);
        ShowBanner("QA VFX 预览: 击球/扣杀/技能/得分反馈");
    }

    private void PrepareQaQuickMatch()
    {
        paused = false;
        GameInput serveInput = new GameInput { vertical = 1 };
        Serve(leftPlayer, serveInput);
        ShowBanner("QA 快速比赛: 自动发球烟测");
    }

    private void PrepareQaAutoMatch()
    {
        qaAutoPlay = true;
        paused = false;
        GameInput serveInput = new GameInput { vertical = 1 };
        Serve(leftPlayer, serveInput);
        ShowBanner("QA 自动对局: 长运行烟测");
    }

    private void DrawMenuBackdrop()
    {
        int courtIndex = profile == null ? 0 : Mathf.Clamp(profile.selectedCourt, 0, courts.Length - 1);
        Color bg = courts[courtIndex].background;
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(bg.r * 0.45f, bg.g * 0.45f, bg.b * 0.45f, 1f));
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0.012f, 0.014f, 0.016f, 0.86f));

        float bandTop = Mathf.Max(160.0f, Screen.height - 280.0f);
        DrawRect(new Rect(0, bandTop, Screen.width, Screen.height - bandTop), new Color(0.02f, 0.022f, 0.026f, 0.82f));
        DrawRect(new Rect(0, bandTop, Screen.width, 3), new Color(1f, 1f, 1f, 0.12f));

        Color accent = courts[courtIndex].accent;
        DrawRect(new Rect(0, bandTop + 56, Screen.width, 4), new Color(accent.r, accent.g, accent.b, 0.18f));
        DrawRect(new Rect(0, bandTop + 112, Screen.width, 2), new Color(1f, 1f, 1f, 0.08f));
    }

    private void DrawMainMenu()
    {
        FireChampionMenuLayout layout = FireChampionLayout.MainMenu(Screen.width, Screen.height);
        DrawPanel(layout.mainPanel.x, layout.mainPanel.y, layout.mainPanel.width, layout.mainPanel.height, new Color(0.03f, 0.035f, 0.04f, 0.94f));
        float headerHeight = Mathf.Clamp(layout.mainContent.height * 0.34f, 128.0f, 168.0f);
        float headerGap = 8.0f;
        Rect headerRect = new Rect(layout.mainContent.x, layout.mainContent.y, layout.mainContent.width, headerHeight);
        Rect menuScrollRect = new Rect(
            layout.mainContent.x,
            headerRect.yMax + headerGap,
            layout.mainContent.width,
            Mathf.Max(96.0f, layout.mainContent.height - headerHeight - headerGap));

        GUILayout.BeginArea(headerRect);
        if (uiAssets != null && uiAssets.Logo != null)
        {
            float logoWidth = Mathf.Min(300.0f, headerRect.width);
            float logoHeight = Mathf.Clamp(headerRect.height - 78.0f, 48.0f, 78.0f);
            Rect logoRect = GUILayoutUtility.GetRect(logoWidth, logoHeight, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            GUI.DrawTexture(logoRect, uiAssets.Logo, ScaleMode.ScaleToFit, true);
        }
        else
        {
            Space(10);
        }

        GUILayout.Label("<size=30><b>火柴冠军赛</b></size>");
        GUILayout.Label("轻竞技 1v1 羽毛球 · Unity 2D 原型");
        GUILayout.EndArea();

        GUILayout.BeginArea(menuScrollRect);
        mainMenuScroll = GUILayout.BeginScrollView(mainMenuScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
        Space(10);

        GUILayout.Label("<b>角色与球场</b>");
        if (GUILayout.Button("P1 角色: " + CharacterButtonText(roster[profile.selectedLeftCharacter])))
        {
            profile.selectedLeftCharacter = (profile.selectedLeftCharacter + 1) % roster.Length;
            ProfileStore.Save(profile);
        }

        if (GUILayout.Button("P2/AI 角色: " + CharacterButtonText(roster[profile.selectedRightCharacter])))
        {
            profile.selectedRightCharacter = (profile.selectedRightCharacter + 1) % roster.Length;
            ProfileStore.Save(profile);
        }

        DrawSelectedCharacterSummary("P1", roster[profile.selectedLeftCharacter]);
        DrawSelectedCharacterSummary("P2/AI", roster[profile.selectedRightCharacter]);
        Space(4);

        if (GUILayout.Button("P1 外观: " + CosmeticButtonText(profile.selectedLeftCosmetic)))
        {
            profile.selectedLeftCosmetic = (profile.selectedLeftCosmetic + 1) % cosmetics.Length;
            ProfileStore.Save(profile);
        }

        if (GUILayout.Button("P2/AI 外观: " + CosmeticButtonText(profile.selectedRightCosmetic)))
        {
            profile.selectedRightCosmetic = (profile.selectedRightCosmetic + 1) % cosmetics.Length;
            ProfileStore.Save(profile);
        }

        if (GUILayout.Button("球场: " + courts[profile.selectedCourt].name))
        {
            profile.selectedCourt = (profile.selectedCourt + 1) % courts.Length;
            ProfileStore.Save(profile);
        }

        Texture2D courtBadge = uiAssets == null ? null : uiAssets.CourtBadge(profile.selectedCourt);
        if (courtBadge != null)
        {
            Rect courtBadgeRect = GUILayoutUtility.GetRect(188, 70, GUILayout.Width(188), GUILayout.Height(70));
            GUI.DrawTexture(courtBadgeRect, courtBadge, ScaleMode.ScaleToFit, true);
        }

        showAbilitiesInPvp = GUILayout.Toggle(showAbilitiesInPvp, "PVP 启用角色能力");
        showCourtRulesInPvp = GUILayout.Toggle(showCourtRulesInPvp, "PVP 启用场地扰动");
        Space(12);

        if (GUILayout.Button("快速比赛 vs AI", GUILayout.Height(38)))
        {
            StartMatch(GameMode.QuickAi);
        }

        if (GUILayout.Button("本地双人", GUILayout.Height(38)))
        {
            StartMatch(GameMode.LocalPvp);
        }

        if (GUILayout.Button("好友直连 / 局域网 IP", GUILayout.Height(38)))
        {
            screen = ScreenState.Network;
        }

        if (profile.HasActiveTournamentRun(TournamentProgression.RoundCount))
        {
            if (GUILayout.Button("继续火柴冠军赛 · " + TournamentProgression.RoundLabel(profile.CurrentTournamentRound(TournamentProgression.RoundCount)), GUILayout.Height(38)))
            {
                ResumeTournamentRun();
            }

            if (confirmTournamentRestart)
            {
                GUILayout.Label("<size=13>重新开始会覆盖当前锦标赛进度。</size>");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("确认重开", GUILayout.Height(30)))
                {
                    StartNewTournamentRun();
                }

                if (GUILayout.Button("取消", GUILayout.Height(30)))
                {
                    confirmTournamentRestart = false;
                }

                GUILayout.EndHorizontal();
            }
            else if (GUILayout.Button("重新开始火柴冠军赛", GUILayout.Height(30)))
            {
                confirmTournamentRestart = true;
            }
        }
        else if (GUILayout.Button("火柴冠军赛 · 锦标赛", GUILayout.Height(38)))
        {
            StartNewTournamentRun();
        }

        if (GUILayout.Button("自由沙盒练习", GUILayout.Height(38)))
        {
            StartMatch(GameMode.Practice);
        }

        if (GUILayout.Button("交互教程", GUILayout.Height(38)))
        {
            StartMatch(GameMode.Tutorial);
        }

        if (GUILayout.Button("设置 / 键位 / 档案", GUILayout.Height(34)))
        {
            screen = ScreenState.Settings;
        }

        if (GUILayout.Button("退出", GUILayout.Height(30)))
        {
            ProfileStore.Save(profile);
            Application.Quit();
        }

        Space(8);
        GUILayout.Label("存档: " + ProfileStore.DataDirectory);
        GUILayout.Label("存档状态: " + ProfileStore.LastStatus);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        DrawInfoCard(layout);
    }

    private void NormalizeProfile()
    {
        if (profile == null)
        {
            profile = new ProfileData();
        }

        if (profile.p1 == null) profile.p1 = KeyBinding.DefaultP1();
        if (profile.p2 == null) profile.p2 = KeyBinding.DefaultP2();
        profile.selectedLeftCharacter = Mathf.Clamp(profile.selectedLeftCharacter, 0, roster.Length - 1);
        profile.selectedRightCharacter = Mathf.Clamp(profile.selectedRightCharacter, 0, roster.Length - 1);
        profile.selectedLeftCosmetic = Mathf.Clamp(profile.selectedLeftCosmetic, 0, cosmetics.Length - 1);
        profile.selectedRightCosmetic = Mathf.Clamp(profile.selectedRightCosmetic, 0, cosmetics.Length - 1);
        profile.selectedCourt = Mathf.Clamp(profile.selectedCourt, 0, courts.Length - 1);
        profile.nickname = ClampNickname(profile.nickname);
        profile.NormalizeTournamentRun(TournamentProgression.RoundCount);
    }

    private string ClampNickname(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Player";
        }

        value = value.Trim();
        return value.Length > 24 ? value.Substring(0, 24) : value;
    }

    private void DrawInfoCard(FireChampionMenuLayout layout)
    {
        if (!layout.showInfoCard)
        {
            return;
        }

        DrawPanel(layout.infoPanel.x, layout.infoPanel.y, layout.infoPanel.width, layout.infoPanel.height, new Color(0.94f, 0.94f, 0.9f, 0.92f));
        GUILayout.BeginArea(layout.infoContent);
        GUI.color = Color.black;
        infoScroll = GUILayout.BeginScrollView(infoScroll, false, true);
        GUILayout.Label("<b>角色说明</b>");
        for (int i = 0; i < roster.Length; i++)
        {
            DrawCharacterInfoLine(i);
        }

        GUILayout.Label("<b>外观预留</b>");
        GUILayout.Label("<size=14>外观只改变球衣/拖尾色彩，不修改速度、甜区、扣杀或控球数值。</size>");
        Space(4);

        Space(8);
        GUILayout.Label("<b>默认键位</b>");
        GUILayout.Label("P1: A/D 移动 · W 跳 · S 下压 · F 挥拍 · G 技能");
        GUILayout.Label("P2: ←/→ 移动 · ↑ 跳 · ↓ 下压 · K 挥拍 · L 技能");
        Space(8);
        GUILayout.Label("<b>规则</b>");
        GUILayout.Label("7 分制，领先 2 分获胜，11 分封顶。界外/触网/落地判分。");
        GUILayout.Label("能量 3 段：1 段小技能，满 3 段可强化扣杀或角色强化技。");
        Space(8);
        GUILayout.Label("<b>当前档案</b>");
        GUILayout.Label("昵称: " + profile.nickname + " · 胜场: " + profile.totalWins + " · 徽章: " + profile.badges);
        GUILayout.Label("锦标赛: 冠军 " + profile.tournamentWins + " · 最远 " + TournamentProgression.ProgressLabel(profile.tournamentBestRoundReached) + " · 奖牌 " + profile.tournamentMedals);
        if (profile.HasActiveTournamentRun(TournamentProgression.RoundCount))
        {
            GUILayout.Label("当前锦标赛: " + TournamentProgression.RoundLabel(profile.CurrentTournamentRound(TournamentProgression.RoundCount)) + " 可继续");
        }

        GUILayout.Label("练习: " + profile.practiceSessions + " 段 · 落点 " + profile.PracticeTargetAccuracyPercent().ToString("0") + "% · 甜区 " + profile.PracticeSweetSpotRatePercent().ToString("0") + "%");
        GUILayout.Label("练习最佳: 落点连中 " + profile.practiceBestTargetStreak + " · 稳定连击 " + profile.practiceBestCleanStreak);
        GUILayout.EndScrollView();
        GUI.color = Color.white;
        GUILayout.EndArea();
    }

    private string CharacterButtonText(CharacterConfig cfg)
    {
        return cfg.code + " · " + cfg.displayName + "（" + cfg.role + "）";
    }

    private void DrawSelectedCharacterSummary(string label, CharacterConfig cfg)
    {
        GUILayout.Label("<size=13><b>" + label + " " + cfg.displayName + "</b> · " + cfg.descriptionText + "</size>");
        GUILayout.Label("<size=12>定位: " + cfg.role + " · 能力差异: " + cfg.differenceText + "</size>");
        if (Screen.height >= 680.0f && !string.IsNullOrEmpty(cfg.abilityBreakdownText))
        {
            GUILayout.Label("<size=12>能力拆解: " + cfg.abilityBreakdownText + "</size>");
        }

        GUILayout.Label("<size=12>" + cfg.statText + "</size>");
    }

    private string CosmeticButtonText(int index)
    {
        CosmeticConfig cfg = cosmetics[Mathf.Clamp(index, 0, cosmetics.Length - 1)];
        return string.IsNullOrEmpty(cfg.unlockLabel) ? cfg.displayName : cfg.displayName + " · " + cfg.unlockLabel;
    }

    private Color ActorAccent(PlayerActor player)
    {
        Color baseAccent = roster[player.characterIndex].accent;
        int cosmeticIndex = player.side == Side.Left ? profile.selectedLeftCosmetic : profile.selectedRightCosmetic;
        CosmeticConfig cosmetic = cosmetics[Mathf.Clamp(cosmeticIndex, 0, cosmetics.Length - 1)];
        if (string.Equals(cosmetic.id, "default", System.StringComparison.OrdinalIgnoreCase))
        {
            return baseAccent;
        }

        return Color.Lerp(baseAccent, cosmetic.tint, Mathf.Clamp01(cosmetic.tintBlend));
    }

    private void DrawCharacterInfoLine(int index)
    {
        CharacterConfig cfg = roster[index];
        GUILayout.BeginHorizontal();
        Rect iconRect = GUILayoutUtility.GetRect(46, 46, GUILayout.Width(46), GUILayout.Height(46));
        Texture2D icon = uiAssets == null ? null : uiAssets.RoleIcon(index);
        if (icon != null)
        {
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        }

        GUILayout.BeginVertical();
        GUILayout.Label("<b>" + cfg.code + " · " + cfg.displayName + "</b>  " + cfg.role);
        GUILayout.Label("<size=14>" + cfg.descriptionText + "</size>");
        GUILayout.Label("<size=14>打法: " + cfg.playStyleText + "</size>");
        GUILayout.Label("<size=14>差异: " + cfg.differenceText + "</size>");
        GUILayout.Label("<size=14>能力: " + cfg.abilityBreakdownText + "</size>");
        GUILayout.Label("<size=14>优势: " + cfg.strengthsText + "</size>");
        GUILayout.Label("<size=14>短板: " + cfg.weaknessText + "</size>");
        GUILayout.Label("<size=14>适合: " + cfg.recommendedText + "</size>");
        GUILayout.Label("<size=14>被动: " + cfg.passiveText + "</size>");
        GUILayout.Label("<size=14>小技能: " + cfg.skillText + "</size>");
        GUILayout.Label("<size=14>满能量: " + cfg.ultimateText + "</size>");
        GUILayout.Label("<size=13>" + cfg.statText + "</size>");
        DrawCharacterAbilityBars(cfg, false);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        Space(4);
    }

    private void DrawCharacterAbilityBars(CharacterConfig cfg, bool compact)
    {
        float labelWidth = compact ? 38.0f : 46.0f;
        float height = compact ? 10.0f : 12.0f;
        DrawAbilityBar("速度", Ability01(cfg.moveSpeed, 4.7f, 6.4f), cfg.accent, labelWidth, height);
        DrawAbilityBar("扣杀", Ability01(cfg.smashBonus, -0.25f, 1.5f), cfg.accent, labelWidth, height);
        DrawAbilityBar("控球", Ability01(cfg.controlBonus, -0.15f, 0.85f), cfg.accent, labelWidth, height);
        DrawAbilityBar("容错", Ability01(cfg.hitRadius - cfg.smashEndLag * 0.35f, 0.73f, 0.88f), cfg.accent, labelWidth, height);
    }

    private void DrawAbilityBar(string label, float value, Color accent, float labelWidth, float height)
    {
        GUILayout.BeginHorizontal(GUILayout.Height(height + 3.0f));
        GUILayout.Label("<size=11>" + label + "</size>", GUILayout.Width(labelWidth));
        Rect barRect = GUILayoutUtility.GetRect(100.0f, height, GUILayout.ExpandWidth(true), GUILayout.Height(height));
        DrawRect(barRect, new Color(0f, 0f, 0f, 0.34f));
        Rect fillRect = new Rect(barRect.x + 1.0f, barRect.y + 1.0f, Mathf.Max(0.0f, (barRect.width - 2.0f) * value), Mathf.Max(0.0f, barRect.height - 2.0f));
        DrawRect(fillRect, new Color(accent.r, accent.g, accent.b, 0.78f));
        GUILayout.Label("<size=11>" + Mathf.RoundToInt(value * 100.0f).ToString(CultureInfo.InvariantCulture) + "</size>", GUILayout.Width(28));
        GUILayout.EndHorizontal();
    }

    private float Ability01(float value, float min, float max)
    {
        if (max <= min)
        {
            return 0.0f;
        }

        return Mathf.Clamp01((value - min) / (max - min));
    }

    private void DrawSettings()
    {
        DrawPanel(34, 28, 590, Screen.height - 56, new Color(0.03f, 0.035f, 0.04f, 0.96f));
        GUILayout.BeginArea(new Rect(58, 54, 540, Screen.height - 100));
        settingsScroll = GUILayout.BeginScrollView(settingsScroll, false, true);
        GUILayout.Label("<size=28><b>设置</b></size>");
        profile.nickname = ClampNickname(LabeledText("本地昵称", profile.nickname));
        profile.masterVolume = LabeledSlider("主音量", profile.masterVolume, 0.0f, 1.0f);
        profile.screenShake = GUILayout.Toggle(profile.screenShake, "屏幕震动");
        profile.highContrast = GUILayout.Toggle(profile.highContrast, "高对比 HUD");
        Space(10);
        GUILayout.Label("<b>P1 键位</b>");
        DrawBindingRow("P1 左", FireChampionInputMapper.P1Left, profile.p1.left);
        DrawBindingRow("P1 右", FireChampionInputMapper.P1Right, profile.p1.right);
        DrawBindingRow("P1 跳/上", FireChampionInputMapper.P1Up, profile.p1.up);
        DrawBindingRow("P1 下压", FireChampionInputMapper.P1Down, profile.p1.down);
        DrawBindingRow("P1 挥拍", FireChampionInputMapper.P1Swing, profile.p1.swing);
        DrawBindingRow("P1 技能", FireChampionInputMapper.P1Skill, profile.p1.skill);
        Space(6);
        GUILayout.Label("<b>P2 键位</b>");
        DrawBindingRow("P2 左", FireChampionInputMapper.P2Left, profile.p2.left);
        DrawBindingRow("P2 右", FireChampionInputMapper.P2Right, profile.p2.right);
        DrawBindingRow("P2 跳/上", FireChampionInputMapper.P2Up, profile.p2.up);
        DrawBindingRow("P2 下压", FireChampionInputMapper.P2Down, profile.p2.down);
        DrawBindingRow("P2 挥拍", FireChampionInputMapper.P2Swing, profile.p2.swing);
        DrawBindingRow("P2 技能", FireChampionInputMapper.P2Skill, profile.p2.skill);
        Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("保存设置"))
        {
            ProfileStore.Save(profile);
            ShowBanner("设置已保存");
        }

        if (GUILayout.Button("重置 AI 习惯"))
        {
            profile.ResetAiMemory();
            ProfileStore.Save(profile);
            ShowBanner("AI 记忆已重置");
        }

        if (GUILayout.Button("重置键位"))
        {
            profile.p1 = KeyBinding.DefaultP1();
            profile.p2 = KeyBinding.DefaultP2();
            ProfileStore.Save(profile);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("清空练习历史"))
        {
            profile.ResetPracticeHistory();
            ProfileStore.Save(profile);
            ShowBanner("练习历史已清空");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("返回主菜单", GUILayout.Height(34)))
        {
            ProfileStore.Save(profile);
            screen = ScreenState.MainMenu;
        }

        if (!string.IsNullOrEmpty(rebindTarget))
        {
            GUILayout.Label("<color=yellow>按下一个新按键绑定: " + rebindTarget + "</color>");
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawNetworkMenu()
    {
        DrawPanel(42, 34, 560, 520, new Color(0.03f, 0.035f, 0.04f, 0.96f));
        GUILayout.BeginArea(new Rect(66, 60, 510, 470));
        GUILayout.Label("<size=28><b>好友直连 / 局域网 IP</b></size>");
        GUILayout.Label("首版目标：同一局域网或可直连公网 IP。未来可替换为中继/房间码。");
        Space(8);
        portText = LabeledText("端口", portText);
        joinIp = LabeledText("加入 IP", joinIp);
        networkRules.pointsToWin = Mathf.RoundToInt(LabeledSlider("目标分", networkRules.pointsToWin, balance.network.pointsMin, balance.network.pointsMax));
        networkRules.energyMultiplier = LabeledSlider("能量倍率", networkRules.energyMultiplier, balance.network.energyMultiplierMin, balance.network.energyMultiplierMax);
        networkRules.ballSpeedMultiplier = LabeledSlider("球速倍率", networkRules.ballSpeedMultiplier, balance.network.ballSpeedMultiplierMin, balance.network.ballSpeedMultiplierMax);
        networkRules.abilitiesEnabled = GUILayout.Toggle(networkRules.abilitiesEnabled, "启用角色能力");
        networkRules.courtModifiersEnabled = GUILayout.Toggle(networkRules.courtModifiersEnabled, "启用场地扰动");

        Space(8);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建主机", GUILayout.Height(36)))
        {
            int port = ParsePort();
            network.Host(port);
            StartMatch(GameMode.NetworkHost);
            ShowBanner("主机已创建，等待客机输入 IP 加入");
        }

        if (GUILayout.Button("加入主机", GUILayout.Height(36)))
        {
            int port = ParsePort();
            network.Join(joinIp, port);
            StartMatch(GameMode.NetworkClient);
            ShowBanner("正在连接 " + joinIp + ":" + port);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("网络状态: " + network.Status);
        GUILayout.Label("<size=14>" + network.Diagnostics.BuildSummary(network.IsConnected) + "</size>");
        if (GUILayout.Button("返回主菜单", GUILayout.Height(34)))
        {
            network.Stop();
            screen = ScreenState.MainMenu;
        }
        GUILayout.EndArea();
    }

    private void DrawNetworkDiagnosticsPanel()
    {
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        DrawPanel(layout.networkDiagnosticsPanel.x, layout.networkDiagnosticsPanel.y, layout.networkDiagnosticsPanel.width, layout.networkDiagnosticsPanel.height, new Color(0.02f, 0.02f, 0.025f, 0.84f));
        GUILayout.BeginArea(layout.networkDiagnosticsContent);
        GUILayout.Label("<b>联机诊断</b>");
        GUILayout.Label("<size=14>状态: " + network.Status + "</size>");
        GUILayout.Label("<size=13>" + network.Diagnostics.BuildSummary(network.IsConnected) + "</size>");
        GUILayout.EndArea();
    }

    private void DrawPlayingOverlay()
    {
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        if (bannerTimer > 0.0f)
        {
            bannerTimer -= Time.deltaTime;
            FireChampionOverlayRenderer.DrawBanner(pixel, layout, banner);
        }

        if (!qaFreezeMatch && mode == GameMode.Practice)
        {
            DrawPracticeTools();
        }

        if (!qaFreezeMatch && mode == GameMode.Tutorial)
        {
            DrawTutorialCoach();
        }

        if (!qaFreezeMatch && mode == GameMode.Tournament)
        {
            DrawTournamentPanel();
        }

        bool networkMode = mode == GameMode.NetworkHost || mode == GameMode.NetworkClient;
        bool networkWaiting = networkMode && !network.IsConnected;
        if (networkMode && !networkWaiting)
        {
            DrawNetworkDiagnosticsPanel();
        }

        if (networkWaiting)
        {
            FireChampionNetworkWaitingAction action = FireChampionOverlayRenderer.DrawNetworkWaiting(pixel, layout, network.Status, network.Diagnostics.BuildSummary(network.IsConnected));
            if (action == FireChampionNetworkWaitingAction.Cancel)
            {
                network.Stop();
                screen = ScreenState.Network;
            }
        }

        if (paused && !qaFreezeMatch)
        {
            FireChampionPauseAction action = FireChampionOverlayRenderer.DrawPause(pixel, layout);
            if (action == FireChampionPauseAction.Resume)
            {
                paused = false;
            }
            else if (action == FireChampionPauseAction.RestartMatch)
            {
                CommitPracticeSessionToProfile(true);
                BeginMatch();
                if (mode == GameMode.Practice)
                {
                    ResetPracticeDrill();
                }

                paused = false;
            }
            else if (action == FireChampionPauseAction.MainMenu)
            {
                CommitPracticeSessionToProfile(true);
                network.Stop();
                screen = ScreenState.MainMenu;
                paused = false;
            }
        }
    }

    private void DrawPracticeTools()
    {
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        DrawPanel(layout.practicePanel.x, layout.practicePanel.y, layout.practicePanel.width, layout.practicePanel.height, new Color(0.02f, 0.02f, 0.025f, 0.88f));
        GUILayout.BeginArea(layout.practiceContent);
        practiceToolsScroll = GUILayout.BeginScrollView(practiceToolsScroll);
        GUILayout.Label("<b>自由沙盒</b>");
        sandboxBallSpeed = LabeledSlider("球速", sandboxBallSpeed, gameplay.practice.ballSpeedMin, gameplay.practice.ballSpeedMax);
        sandboxEnergy = LabeledSlider("能量倍率", sandboxEnergy, gameplay.practice.energyMultiplierMin, gameplay.practice.energyMultiplierMax);
        practiceFeeder = GUILayout.Toggle(practiceFeeder, "AI 喂球/陪练");
        Space(4);
        float accuracy = practiceDrill.Attempts <= 0 ? 0 : practiceDrill.Hits * 100.0f / practiceDrill.Attempts;
        GUILayout.Label("落点目标: " + practiceDrill.Hits + "/" + practiceDrill.Attempts + "  命中率 " + accuracy.ToString("0") + "%");
        GUILayout.Label("连续命中: " + practiceDrill.Streak + "  最佳: " + practiceDrill.BestStreak);
        GUILayout.Label("击球手感: " + hitTimingFeedback.LastFeedback);
        GUILayout.Label("甜区率 " + hitTimingFeedback.SweetSpotRate().ToString("0") + "% · 稳定率 " + hitTimingFeedback.CleanRate().ToString("0") + "% · 最佳连击 " + hitTimingFeedback.BestCleanStreak);
        GUILayout.Label("<size=14>" + hitTimingFeedback.LastHint + "</size>");
        DrawPracticeTimeline();
        GUILayout.Label("<size=14>练习历史: " + profile.practiceSessions + " 段 · 落点 " + profile.practiceTargetHits + "/" + profile.practiceTargetAttempts + " (" + profile.PracticeTargetAccuracyPercent().ToString("0") + "%)</size>");
        GUILayout.Label("<size=14>历史手感: 甜区 " + profile.PracticeSweetSpotRatePercent().ToString("0") + "% · 稳定 " + profile.PracticeCleanRatePercent().ToString("0") + "% · 最佳连击 " + profile.practiceBestCleanStreak + "</size>");
        if (GUILayout.Button("换一个落点目标"))
        {
            practiceDrill.RandomizeTarget(CourtHalfWidth);
            ShowBanner("练习目标已更新");
        }

        if (GUILayout.Button("重置练习统计"))
        {
            ResetPracticeDrill();
            ShowBanner("练习统计已重置");
        }

        if (GUILayout.Button("重置球"))
        {
            BeginServe(Side.Left);
        }
        if (GUILayout.Button("返回主菜单"))
        {
            CommitPracticeSessionToProfile(true);
            screen = ScreenState.MainMenu;
            paused = false;
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawPracticeTimeline()
    {
        Space(2);
        GUILayout.Label("<b>最近击球</b>");
        if (practiceShotTimeline.Count == 0)
        {
            GUILayout.Label("<size=13>还没有记录。用 P1 击球后这里会显示质量和落点。</size>");
            return;
        }

        int rows = Mathf.Min(4, practiceShotTimeline.Count);
        for (int i = 0; i < rows; i++)
        {
            GUILayout.Label("<size=13>" + practiceShotTimeline.Entries[i].BuildLine() + "</size>");
        }
    }

    private void DrawTutorialCoach()
    {
        TutorialCoachStep step = TutorialCoachData.Step(tutorialStep);
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        DrawPanel(layout.tutorialPanel.x, layout.tutorialPanel.y, layout.tutorialPanel.width, layout.tutorialPanel.height, new Color(0.02f, 0.02f, 0.025f, 0.9f));
        GUILayout.BeginArea(layout.tutorialContent);
        GUILayout.Label("<b>交互教程 " + Mathf.Min(tutorialStep + 1, TutorialCoachData.StepCount) + "/" + TutorialCoachData.StepCount + " · " + step.title + "</b>");
        GUILayout.Label(TutorialText());
        GUILayout.Label("<size=14>" + TutorialProgressText() + "</size>");
        GUILayout.Label("<size=14>提示: " + TutorialHintText() + "</size>");
        if (!string.IsNullOrEmpty(tutorialRecoveryHint))
        {
            GUILayout.Label("<color=yellow><size=14>教练: " + tutorialRecoveryHint + "</size></color>");
        }

        DrawTutorialInputHints();
        Space(6);
        GUILayout.Label("目标: 移动、发球、击球、扣杀、技能，掌握后可直接进入快速比赛。");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("重置教程"))
        {
            ResetTutorialState();
            BeginServe(Side.Left);
        }

        if (GUILayout.Button("跳过本步"))
        {
            AdvanceTutorial("已跳过当前步骤");
        }

        if (GUILayout.Button("完成并返回主菜单"))
        {
            screen = ScreenState.MainMenu;
            paused = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawTutorialInputHints()
    {
        TutorialInputState inputState = FireChampionInputMapper.ReadTutorialState(profile.p1);
        GUILayout.BeginHorizontal();
        DrawKeyChip("A/D", inputState.moving, "移动");
        DrawKeyChip("W", inputState.jumping, "跳跃/挑高");
        DrawKeyChip("S", inputState.pressingDown, "下压");
        DrawKeyChip("F", inputState.swinging, "挥拍");
        DrawKeyChip("G", inputState.usingSkill, "技能");
        GUILayout.EndHorizontal();
    }

    private void DrawKeyChip(string key, bool active, string label)
    {
        Color oldColor = GUI.color;
        GUI.color = active ? new Color(0.35f, 1.0f, 0.62f, 1.0f) : new Color(1f, 1f, 1f, 0.82f);
        GUILayout.Box("<b>" + key + "</b>\n<size=12>" + label + "</size>", GUILayout.Width(92), GUILayout.Height(42));
        GUI.color = oldColor;
    }

    private void DrawTournamentPanel()
    {
        TournamentOpponent opponent = TournamentProgression.OpponentForRound(tournamentRound);
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        DrawPanel(layout.tournamentPanel.x, layout.tournamentPanel.y, layout.tournamentPanel.width, layout.tournamentPanel.height, new Color(0.02f, 0.02f, 0.025f, 0.86f));
        GUILayout.BeginArea(layout.tournamentContent);
        tournamentPanelScroll = GUILayout.BeginScrollView(tournamentPanelScroll, false, true);
        GUILayout.Label("<b>锦标赛 · " + TournamentProgression.RoundLabel(tournamentRound) + "</b>");
        GUILayout.Label("对手: " + opponent.displayName + "  难度 " + Mathf.RoundToInt(opponent.difficulty * 100) + "%");
        GUILayout.Label("<size=14>" + opponent.scoutingReport + "</size>");
        Space(5);
        GUILayout.Label("<size=14>赛程与奖励（只记录荣誉/外观，不加属性）</size>");
        for (int i = 0; i < TournamentProgression.RoundCount; i++)
        {
            GUILayout.Label("<size=14>" + TournamentBracketLine(i) + "</size>");
        }

        GUILayout.Label("<size=14>历史: 开赛 " + profile.tournamentRunsStarted + " · 过关 " + profile.tournamentRoundsWon + " · 奖牌 " + profile.tournamentMedals + "</size>");
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private string TournamentBracketLine(int round)
    {
        TournamentOpponent opponent = TournamentProgression.OpponentForRound(round);
        string state = round < tournamentRound ? "已晋级" : (round == tournamentRound ? "当前" : "待挑战");
        return state + " · " + opponent.roundName + " · " + opponent.displayName + " · " + TournamentProgression.RewardTextForRound(round);
    }

    private void DrawSummary()
    {
        FireChampionPlayingOverlayLayout layout = FireChampionLayout.PlayingOverlay(Screen.width, Screen.height);
        FireChampionSummaryAction action = FireChampionOverlayRenderer.DrawSummary(new FireChampionSummaryRenderContext
        {
            pixel = pixel,
            layout = layout,
            summaryTitle = summaryTitle,
            summaryDetails = summaryDetails,
            stats = stats,
            mode = mode,
            lastMatchPlayerWon = lastMatchPlayerWon,
            tournamentComplete = IsTournamentComplete()
        });
        ApplySummaryAction(action);
    }

    private void ApplySummaryAction(FireChampionSummaryAction action)
    {
        if (action == FireChampionSummaryAction.NextTournamentMatch)
        {
            tournamentRound++;
            profile.SetTournamentResumeRound(tournamentRound, TournamentProgression.RoundCount);
            ProfileStore.Save(profile);
            StartMatch(GameMode.Tournament);
        }
        else if (action == FireChampionSummaryAction.RestartTournament)
        {
            StartNewTournamentRun();
        }
        else if (action == FireChampionSummaryAction.NetworkMenu)
        {
            network.Stop();
            screen = ScreenState.Network;
        }
        else if (action == FireChampionSummaryAction.Rematch)
        {
            StartMatch(mode);
        }
        else if (action == FireChampionSummaryAction.MainMenu)
        {
            network.Stop();
            screen = ScreenState.MainMenu;
        }
    }

    private void DrawGameWorld()
    {
        scale = Mathf.Min(Screen.width / 19.5f, (Screen.height - 165.0f) / 5.7f);
        courtBottom = Screen.height - 88.0f;

        FireChampionWorldRenderContext render = WorldRenderContext();
        FireChampionMatchRenderer.DrawBackground(render);

        Vector2 leftBase = FireChampionMatchRenderer.WorldToScreen(render, new Vector2(-CourtHalfWidth, 0));
        Vector2 rightBase = FireChampionMatchRenderer.WorldToScreen(render, new Vector2(CourtHalfWidth, 0));
        FireChampionMatchRenderer.DrawCourtSurface(render, leftBase, rightBase);
        FireChampionMatchRenderer.DrawNet(render);
        FireChampionMatchRenderer.DrawPracticeTarget(render);
        FireChampionMatchRenderer.DrawTutorialTargetMarker(render);

        FireChampionMatchRenderer.DrawPlayer(render, leftPlayer, ActorAccent(leftPlayer), RacketPosition(leftPlayer));
        FireChampionMatchRenderer.DrawPlayer(render, rightPlayer, ActorAccent(rightPlayer), RacketPosition(rightPlayer));
        FireChampionVfxRenderer.Draw(render, vfxSystem.Events);
        FireChampionMatchRenderer.DrawShuttle(render);

        if (screen == ScreenState.Playing && waitingForServe)
        {
            string serveText = server == Side.Left ? "P1 发球" : "P2/AI 发球";
            DrawPanel(Screen.width * 0.5f - 90, 116, 180, 36, new Color(0, 0, 0, 0.72f));
            GUI.Label(new Rect(Screen.width * 0.5f - 72, 123, 160, 24), "<color=white>" + serveText + "</color>");
        }
    }

    private FireChampionWorldRenderContext WorldRenderContext()
    {
        return new FireChampionWorldRenderContext
        {
            pixel = pixel,
            discTexture = discTexture,
            courtSceneTexture = courtAssets == null ? null : courtAssets.Background(profile.selectedCourt),
            courtBackdropTexture = uiAssets == null ? null : uiAssets.CourtBadge(profile.selectedCourt),
            gameplay = gameplay,
            roster = roster,
            courts = courts,
            rules = rules,
            practiceDrill = practiceDrill,
            shuttle = shuttle,
            mode = mode,
            selectedCourt = profile.selectedCourt,
            tutorialStep = tutorialStep,
            scale = scale,
            courtBottom = courtBottom,
            courtHalfWidth = CourtHalfWidth,
            groundY = GroundY,
            netHeight = NetHeight,
            courtPhase = courtPhase,
            screenShakeOffset = screenShakeOffset
        };
    }

    private void DrawHud()
    {
        FireChampionHudRenderer.DrawHud(HudRenderContext());
    }

    private FireChampionHudRenderContext HudRenderContext()
    {
        return new FireChampionHudRenderContext
        {
            pixel = pixel,
            discTexture = discTexture,
            gameplay = gameplay,
            roster = roster,
            courts = courts,
            rules = rules,
            leftPlayer = leftPlayer,
            rightPlayer = rightPlayer,
            mode = mode,
            selectedCourt = profile.selectedCourt,
            leftScore = leftScore,
            rightScore = rightScore,
            leftGames = leftGames,
            rightGames = rightGames,
            courtPhase = courtPhase,
            highContrast = profile.highContrast,
            leftDisplayName = profile.nickname,
            rightDisplayName = TournamentOpponentName(),
            modeName = ModeName(mode),
            tournamentSuffix = TournamentHudSuffix(),
            networkStatus = network.Status,
            leftAccent = ActorAccent(leftPlayer),
            rightAccent = ActorAccent(rightPlayer)
        };
    }

    private string TournamentOpponentName()
    {
        if (mode == GameMode.Tournament)
        {
            return TournamentProgression.OpponentForRound(tournamentRound).displayName;
        }

        return rightPlayer.isAi ? "AI" : "P2";
    }

    private string TournamentHudSuffix()
    {
        return mode == GameMode.Tournament ? " · " + TournamentProgression.RoundLabel(tournamentRound) : "";
    }

    private void DrawEnergyBar(float x, float y, PlayerActor player)
    {
        FireChampionHudRenderer.DrawEnergyBar(HudRenderContext(), x, y, player, ActorAccent(player));
    }

    private void StartMatch(GameMode nextMode)
    {
        qaFreezeMatch = false;
        qaAutoPlay = false;
        confirmTournamentRestart = false;
        mode = nextMode;
        if (mode == GameMode.Tournament)
        {
            rules = balance.TournamentRules();
        }
        else if (mode == GameMode.Tutorial)
        {
            rules = balance.TutorialRules();
        }
        else if (mode == GameMode.Practice)
        {
            rules = balance.PracticeRules();
        }
        else if (mode == GameMode.LocalPvp || mode == GameMode.NetworkHost || mode == GameMode.NetworkClient)
        {
            rules = mode == GameMode.LocalPvp ? balance.StandardRules() : networkRules.Copy();
            rules.abilitiesEnabled = showAbilitiesInPvp || rules.abilitiesEnabled;
            rules.courtModifiersEnabled = showCourtRulesInPvp || rules.courtModifiersEnabled;
        }
        else
        {
            rules = balance.StandardRules();
            rules.abilitiesEnabled = true;
            rules.courtModifiersEnabled = true;
        }

        BeginMatch();
        screen = ScreenState.Playing;
        paused = false;
        matchOver = false;
        lastMatchPlayerWon = false;
        tournamentPanelScroll = Vector2.zero;
        if (mode == GameMode.Tutorial)
        {
            ResetTutorialState();
            ShowBanner("跟随教练提示完成教程");
        }
        else if (mode == GameMode.Tournament)
        {
            TournamentOpponent opponent = TournamentProgression.OpponentForRound(tournamentRound);
            ShowBanner(TournamentProgression.RoundLabel(tournamentRound) + ": 对阵 " + opponent.displayName);
        }
        else if (mode == GameMode.Practice)
        {
            ResetPracticeDrill();
            ShowBanner("自由沙盒: 练习把球打进右半场目标圈");
        }
    }

    private void StartNewTournamentRun()
    {
        confirmTournamentRestart = false;
        tournamentRound = 0;
        profile.StartTournamentRun(TournamentProgression.RoundCount);
        ProfileStore.Save(profile);
        StartMatch(GameMode.Tournament);
    }

    private void ResumeTournamentRun()
    {
        confirmTournamentRestart = false;
        if (!profile.HasActiveTournamentRun(TournamentProgression.RoundCount))
        {
            StartNewTournamentRun();
            return;
        }

        tournamentRound = profile.CurrentTournamentRound(TournamentProgression.RoundCount);
        profile.SetTournamentResumeRound(tournamentRound, TournamentProgression.RoundCount);
        ProfileStore.Save(profile);
        StartMatch(GameMode.Tournament);
    }

    private void BeginMatch()
    {
        leftScore = 0;
        rightScore = 0;
        leftGames = 0;
        rightGames = 0;
        stats = new MatchStats();
        matchOver = false;
        leftPlayer = new PlayerActor(Side.Left, profile.selectedLeftCharacter, -gameplay.court.playerSpawnX);
        rightPlayer = new PlayerActor(Side.Right, profile.selectedRightCharacter, gameplay.court.playerSpawnX);
        rightPlayer.isAi = mode == GameMode.QuickAi || mode == GameMode.Tournament || mode == GameMode.Tutorial || (mode == GameMode.Practice && practiceFeeder);
        rightPlayer.aiDifficulty = AiDifficultyForCurrentMode();
        ApplyCharacterStats(leftPlayer);
        ApplyCharacterStats(rightPlayer);
        vfxSystem.Clear();
        BeginServe(Side.Left);
    }

    private void ResetPracticeDrill()
    {
        CommitPracticeSessionToProfile(true);
        practiceDrill.Reset(CourtHalfWidth);
        hitTimingFeedback.Reset();
        practiceShotTimeline.Reset();
        practiceSessionStats.BeginNewSession();
    }

    private bool CommitPracticeSessionToProfile(bool saveImmediately)
    {
        if (mode != GameMode.Practice)
        {
            return false;
        }

        bool committed = practiceSessionStats.CommitToProfile(profile, practiceDrill, hitTimingFeedback);
        if (committed && saveImmediately)
        {
            ProfileStore.Save(profile);
        }

        return committed;
    }

    private float AiDifficultyForCurrentMode()
    {
        if (mode == GameMode.Tutorial)
        {
            return balance.ai.tutorialDifficulty;
        }

        if (mode == GameMode.Practice)
        {
            return balance.ai.practiceDifficulty;
        }

        if (mode == GameMode.Tournament)
        {
            return TournamentProgression.DifficultyForRound(tournamentRound);
        }

        return balance.ai.quickMatchDifficulty;
    }

    private void BeginGameOnly()
    {
        leftScore = 0;
        rightScore = 0;
        leftPlayer.ResetForPoint(-gameplay.court.playerSpawnX);
        rightPlayer.ResetForPoint(gameplay.court.playerSpawnX);
        BeginServe(server);
    }

    private void BeginServe(Side servingSide)
    {
        waitingForServe = true;
        server = servingSide;
        lastHitter = servingSide;
        shuttle = new ShuttleState();
        PlayerActor p = servingSide == Side.Left ? leftPlayer : rightPlayer;
        shuttle.position = RacketPosition(p);
        shuttle.velocity = Vector2.zero;
        ShowBanner((servingSide == Side.Left ? "P1" : "P2") + " 发球");
    }

    private void ReadInputs()
    {
        leftInput = FireChampionInputMapper.Read(profile.p1);
        rightInput = FireChampionInputMapper.Read(profile.p2);
    }

    private void UpdatePlayer(PlayerActor p, GameInput input, float dt)
    {
        CharacterConfig cfg = roster[p.characterIndex];
        float speed = cfg.moveSpeed;
        if (p.skillTimer > 0 && cfg.code == "DASH")
        {
            speed *= gameplay.skill.dashSpeedMultiplier;
        }
        else if (p.skillTimer > 0 && cfg.code == "HEAVY")
        {
            speed *= gameplay.skill.heavySpeedMultiplier;
        }

        if (p.ultimateTimer > 0 && cfg.code == "DASH" && rules.abilitiesEnabled)
        {
            speed *= gameplay.skill.dashUltimateSpeedMultiplier;
        }

        if (p.endLag > 0)
        {
            speed *= gameplay.skill.endLagMoveMultiplier;
            p.endLag -= dt;
        }

        p.x += input.horizontal * speed * dt;
        ClampPlayerToSide(p);

        if (input.horizontal != 0)
        {
            p.facing = input.horizontal > 0 ? 1 : -1;
        }
        else
        {
            p.facing = p.side == Side.Left ? 1 : -1;
        }

        if (input.jumpPressed && p.grounded)
        {
            p.vy = cfg.jumpForce;
            p.grounded = false;
        }

        p.vy += Gravity * dt * gameplay.court.gravityMultiplier;
        p.y += p.vy * dt;
        if (p.y <= GroundY)
        {
            p.y = GroundY;
            p.vy = 0;
            p.grounded = true;
        }

        p.swingTimer = Mathf.Max(0, p.swingTimer - dt);
        p.swingActiveTimer = Mathf.Max(0, p.swingActiveTimer - dt);
        p.swingBufferTimer = Mathf.Max(0, p.swingBufferTimer - dt);
        p.swingCooldown = Mathf.Max(0, p.swingCooldown - dt);
        p.skillTimer = Mathf.Max(0, p.skillTimer - dt);
        p.ultimateTimer = Mathf.Max(0, p.ultimateTimer - dt);
    }

    private void ClampPlayerToSide(PlayerActor p)
    {
        float minX = p.side == Side.Left ? -CourtHalfWidth + gameplay.court.playerBackMargin : gameplay.court.centerGuardMargin;
        float maxX = p.side == Side.Left ? -gameplay.court.centerGuardMargin : CourtHalfWidth - gameplay.court.playerBackMargin;
        p.x = Mathf.Clamp(p.x, minX, maxX);
    }

    private void UpdateSkill(PlayerActor p, GameInput input, float dt)
    {
        if (!rules.abilitiesEnabled || !input.skillPressed || p.skillCooldown > 0)
        {
            p.skillCooldown = Mathf.Max(0, p.skillCooldown - dt);
            return;
        }

        bool savingEnergyForHit = input.skillHeld && (input.swingPressed || p.swingActiveTimer > 0);
        if (p.energy >= gameplay.energy.ultimateCost && !savingEnergyForHit)
        {
            p.energy -= gameplay.energy.ultimateCost;
            string code = roster[p.characterIndex].code;
            p.ultimateTimer = UltimateDurationFor(code);
            p.skillCooldown = gameplay.skill.ultimateCooldown;
            stats.skillsUsed++;
            ShowBanner(roster[p.characterIndex].code + " 满能量强化");
            PlayTone(FireChampionAudioCue.Ultimate, gameplay.audio.ultimate);
            vfxSystem.SpawnSkill(new Vector2(p.x, p.y + 0.95f), ActorAccent(p), p.facing, gameplay.feedback);
        }
        else if (p.energy >= gameplay.energy.skillCost)
        {
            p.energy -= gameplay.energy.skillCost;
            string code = roster[p.characterIndex].code;
            p.skillTimer = SkillDurationFor(code);
            p.skillCooldown = gameplay.skill.skillCooldown;
            stats.skillsUsed++;
            if (code == "DASH")
            {
                p.x += p.facing * gameplay.skill.dashStepDistance;
                ClampPlayerToSide(p);
            }
            else if (code == "CORE")
            {
                p.endLag = Mathf.Max(0, p.endLag - gameplay.skill.coreEndLagReduction);
            }

            ShowBanner(roster[p.characterIndex].code + " 小技能");
            PlayTone(FireChampionAudioCue.Skill, gameplay.audio.skill);
            vfxSystem.SpawnSkill(new Vector2(p.x, p.y + 0.95f), ActorAccent(p), p.facing, gameplay.feedback);
        }
    }

    private float SkillDurationFor(string code)
    {
        if (code == "DASH") return gameplay.skill.dashSkillDuration;
        if (code == "CORE") return gameplay.skill.coreSkillDuration;
        if (code == "HEAVY") return gameplay.skill.heavySkillDuration;
        return gameplay.skill.trickSkillDuration;
    }

    private float UltimateDurationFor(string code)
    {
        return code == "DASH" ? gameplay.skill.dashUltimateDuration : gameplay.skill.defaultUltimateDuration;
    }

    private void UpdateServeOrRally(float dt)
    {
        if (waitingForServe)
        {
            PlayerActor p = server == Side.Left ? leftPlayer : rightPlayer;
            GameInput input = server == Side.Left ? leftInput : rightInput;
            shuttle.position = RacketPosition(p);
            if (input.swingPressed || ((p.isAi || qaAutoPlay) && UnityEngine.Random.value < dt * gameplay.shot.aiServeChancePerSecond))
            {
                Serve(p, input);
            }
            return;
        }

        TryHit(leftPlayer, leftInput);
        TryHit(rightPlayer, rightInput);

        Vector2 old = shuttle.position;
        float courtSpeed = CurrentCourtSpeedMultiplier();
        Vector2 wind = CurrentCourtWind();
        shuttle.velocity += new Vector2(wind.x, Gravity + wind.y) * dt;
        if (Mathf.Abs(shuttle.spin) > gameplay.court.spinActiveThreshold)
        {
            shuttle.velocity += new Vector2(shuttle.spin * gameplay.court.spinInfluenceX, Mathf.Abs(shuttle.spin) * gameplay.court.spinInfluenceY) * dt;
        }

        shuttle.velocity *= Mathf.Pow(ShuttleDrag, dt);
        shuttle.position += shuttle.velocity * dt * courtSpeed;
        shuttle.spin *= Mathf.Pow(gameplay.court.spinDecay, dt);

        if (CrossedNetTooLow(old, shuttle.position))
        {
            stats.errors++;
            ScorePoint(Opposite(lastHitter), "触网");
            return;
        }

        if (rules.outOfBounds && Mathf.Abs(shuttle.position.x) > CourtHalfWidth + gameplay.court.outOfBoundsMargin)
        {
            stats.errors++;
            ScorePoint(Opposite(lastHitter), "界外");
            return;
        }

        if (shuttle.position.y <= GroundY)
        {
            Side landedSide = shuttle.position.x < 0 ? Side.Left : Side.Right;
            stats.errors++;
            ScorePoint(Opposite(landedSide), "落地");
        }
    }

    private void Serve(PlayerActor p, GameInput input)
    {
        waitingForServe = false;
        StartSwing(p, gameplay.swing.serveActiveWindow, gameplay.swing.serveCooldown);
        lastHitter = p.side;
        int dir = p.side == Side.Left ? 1 : -1;
        float high = input.vertical > 0 ? gameplay.shot.serveHighLift : gameplay.shot.serveNormalLift;
        float forward = input.vertical < 0 ? gameplay.shot.serveDownForward : gameplay.shot.serveNormalForward;
        shuttle.velocity = new Vector2(dir * forward, high);
        shuttle.position = RacketPosition(p) + new Vector2(dir * gameplay.shot.servePositionForwardOffset, gameplay.shot.servePositionYOffset);
        stats.rallyHits = 0;
        PlayTone(FireChampionAudioCue.Serve, gameplay.audio.serve);
    }

    private void TryHit(PlayerActor p, GameInput input)
    {
        if (input.swingPressed)
        {
            p.swingBufferTimer = gameplay.swing.bufferTime;
            p.bufferedSwingVertical = input.vertical;
            p.bufferedSwingSkillHeld = input.skillHeld;
            p.bufferedSwingDropIntent = input.dropIntent;
        }

        if (p.swingBufferTimer > 0 && p.swingCooldown <= 0 && p.swingActiveTimer <= 0)
        {
            StartSwing(p, gameplay.swing.hitActiveWindow, gameplay.swing.hitCooldown);
            p.swingBufferTimer = 0;
        }

        if (p.swingActiveTimer <= 0 || p.swingConsumed)
        {
            return;
        }

        Vector2 racket = RacketPosition(p);
        CharacterConfig cfg = roster[p.characterIndex];
        float hitRadius = cfg.hitRadius;
        if (p.skillTimer > 0 && cfg.code == "CORE")
        {
            hitRadius += gameplay.swing.coreSkillHitRadiusBonus;
        }
        else if (p.skillTimer > 0 && cfg.code == "HEAVY")
        {
            hitRadius += gameplay.swing.heavySkillHitRadiusBonus;
        }
        else if (p.ultimateTimer > 0 && cfg.code == "CORE" && rules.abilitiesEnabled)
        {
            hitRadius += gameplay.swing.coreUltimateHitRadiusBonus;
        }

        float hitDistance = Vector2.Distance(shuttle.position, racket);
        if (hitDistance > hitRadius)
        {
            return;
        }

        bool goodTiming = hitDistance < hitRadius * gameplay.swing.sweetSpotRatio;
        GameInput shotInput = input;
        shotInput.vertical = p.bufferedSwingVertical;
        shotInput.skillHeld = p.bufferedSwingSkillHeld;
        shotInput.dropIntent = p.bufferedSwingDropIntent;
        ShotType shot = ChooseShotType(p, shotInput);
        ApplyShot(p, shot, goodTiming, shotInput);
        vfxSystem.SpawnHit(racket, shot, goodTiming, ActorAccent(p), p.facing, gameplay.feedback);
        if (mode == GameMode.Practice && p.side == Side.Left)
        {
            hitTimingFeedback.RecordHit(hitDistance, hitRadius, shot, goodTiming);
            practiceShotTimeline.RecordContact(shot, hitTimingFeedback.LastQualityPercent, hitTimingFeedback.LastFeedback, hitTimingFeedback.LastHint, gameplay.practice.timelineCapacity);
        }

        p.swingConsumed = true;
        p.swingActiveTimer = 0;
        profile.RecordHumanShot(p.side == Side.Left, shot);
        stats.rallyHits++;
        stats.longestRally = Mathf.Max(stats.longestRally, stats.rallyHits);
        if (mode == GameMode.Tutorial && p.side == Side.Left)
        {
            tutorialLeftHits++;
            tutorialLastLeftShot = shot;
        }

        float hitEnergy = goodTiming ? gameplay.energy.goodHitGain : gameplay.energy.normalHitGain;
        if (cfg.code == "CORE" && goodTiming)
        {
            hitEnergy += gameplay.energy.coreGoodHitBonus;
        }

        AddEnergy(p, hitEnergy);
        if (stats.rallyHits >= gameplay.energy.longRallyHitThreshold)
        {
            AddEnergy(p, gameplay.energy.longRallyBonus);
        }

        PlayTone(shot == ShotType.Smash ? FireChampionAudioCue.SmashHit : FireChampionAudioCue.NormalHit, shot == ShotType.Smash ? gameplay.audio.smashHit : gameplay.audio.normalHit);
    }

    private ShotType ChooseShotType(PlayerActor p, GameInput input)
    {
        CharacterConfig cfg = roster[p.characterIndex];
        if (input.vertical > 0)
        {
            return ShotType.High;
        }

        if (input.dropIntent)
        {
            return ShotType.Drop;
        }

        if (input.vertical < 0 && shuttle.position.y > gameplay.shot.smashInputHeight)
        {
            return ShotType.Smash;
        }

        if (input.vertical < 0 && cfg.code == "HEAVY" && rules.abilitiesEnabled && (p.skillTimer > 0 || p.ultimateTimer > 0) && shuttle.position.y > gameplay.shot.heavySkillSmashHeight)
        {
            return ShotType.Smash;
        }

        if (cfg.code == "TRICK" && p.skillTimer > 0)
        {
            return ShotType.Drop;
        }

        return ShotType.Flat;
    }

    private void ApplyShot(PlayerActor p, ShotType shot, bool goodTiming, GameInput input)
    {
        CharacterConfig cfg = roster[p.characterIndex];
        int dir = p.side == Side.Left ? 1 : -1;
        float timing = goodTiming ? gameplay.shot.goodTimingMultiplier : gameplay.shot.weakTimingMultiplier;
        bool primedSmash = p.ultimateTimer > 0 && rules.abilitiesEnabled;
        bool empoweredSmash = rules.abilitiesEnabled && input.skillHeld && shot == ShotType.Smash && (p.energy >= gameplay.energy.ultimateCost || primedSmash);
        bool coreSkill = rules.abilitiesEnabled && cfg.code == "CORE" && p.skillTimer > 0;
        bool coreUltimate = rules.abilitiesEnabled && cfg.code == "CORE" && p.ultimateTimer > 0;
        bool dashSkill = rules.abilitiesEnabled && cfg.code == "DASH" && p.skillTimer > 0;
        bool dashUltimate = rules.abilitiesEnabled && cfg.code == "DASH" && p.ultimateTimer > 0;
        bool heavySkill = rules.abilitiesEnabled && cfg.code == "HEAVY" && p.skillTimer > 0;
        bool heavyUltimate = rules.abilitiesEnabled && cfg.code == "HEAVY" && p.ultimateTimer > 0;
        bool trickSkill = rules.abilitiesEnabled && cfg.code == "TRICK" && p.skillTimer > 0;
        bool trickUltimate = rules.abilitiesEnabled && cfg.code == "TRICK" && p.ultimateTimer > 0;
        if (empoweredSmash)
        {
            if (p.energy >= gameplay.energy.ultimateCost)
            {
                p.energy -= gameplay.energy.ultimateCost;
            }
        }

        if (shot == ShotType.High)
        {
            float forward = gameplay.shot.highForward + cfg.controlBonus + (coreSkill ? gameplay.shot.coreSkillHighForwardBonus : 0.0f) + (coreUltimate ? gameplay.shot.coreUltimateHighForwardBonus : 0.0f);
            float lift = gameplay.shot.highLift + (coreUltimate ? gameplay.shot.coreUltimateHighLiftBonus : 0.0f);
            shuttle.velocity = new Vector2(dir * forward * timing, lift * timing);
        }
        else if (shot == ShotType.Flat)
        {
            float flatSpeed = gameplay.shot.flatSpeed + cfg.controlBonus + (dashSkill ? gameplay.shot.dashSkillFlatBonus : 0.0f) + (dashUltimate ? gameplay.shot.dashUltimateFlatBonus : 0.0f);
            float flatLift = gameplay.shot.flatLift + (coreSkill ? gameplay.shot.coreSkillFlatLiftBonus : 0.0f);
            shuttle.velocity = new Vector2(dir * flatSpeed * timing, flatLift * timing);
        }
        else if (shot == ShotType.Drop)
        {
            float dropForward = trickSkill || trickUltimate ? gameplay.shot.trickDropForward : gameplay.shot.dropForward;
            float dropLift = trickSkill || trickUltimate ? gameplay.shot.trickDropLift : gameplay.shot.dropLift;
            float spin = trickSkill || trickUltimate ? gameplay.shot.trickDropSpin : gameplay.shot.dropSpin;
            if (trickUltimate)
            {
                dropForward = gameplay.shot.trickUltimateDropForward;
                dropLift = gameplay.shot.trickUltimateDropLift;
                spin = gameplay.shot.trickUltimateDropSpin;
            }

            shuttle.velocity = new Vector2(dir * dropForward * timing, dropLift);
            shuttle.spin += dir * spin;
        }
        else
        {
            float smashPower = gameplay.shot.smashPower + cfg.smashBonus + (empoweredSmash ? gameplay.shot.empoweredSmashBonus : 0.0f);
            if (heavySkill)
            {
                smashPower += gameplay.shot.heavySkillSmashBonus;
            }

            if (heavyUltimate)
            {
                smashPower += gameplay.shot.heavyUltimateSmashBonus;
            }

            if (dashSkill)
            {
                smashPower += gameplay.shot.dashSkillSmashBonus;
            }

            if (coreUltimate)
            {
                smashPower += gameplay.shot.coreUltimateSmashBonus;
            }

            smashPower *= timing;
            if (p.ultimateTimer > 0 && rules.abilitiesEnabled)
            {
                smashPower += gameplay.shot.primedUltimateSmashBonus;
                p.ultimateTimer = 0;
            }

            float distanceToNet = Mathf.Abs(p.x);
            float vertical = distanceToNet > gameplay.shot.farSmashNetDistance ? gameplay.shot.farSmashVertical : gameplay.shot.nearSmashVertical;
            if (heavySkill || heavyUltimate)
            {
                vertical += gameplay.shot.heavySmashVerticalBonus;
            }

            shuttle.velocity = new Vector2(dir * smashPower, vertical + (empoweredSmash ? gameplay.shot.empoweredSmashVerticalBonus : 0.0f));
            float lag = gameplay.shot.smashBaseEndLag + cfg.smashEndLag + (empoweredSmash ? gameplay.shot.empoweredSmashEndLag : 0.0f);
            if (heavySkill)
            {
                lag -= gameplay.shot.heavySkillEndLagReduction;
            }

            if (dashUltimate)
            {
                lag *= gameplay.shot.dashUltimateEndLagMultiplier;
            }

            p.endLag = Mathf.Max(gameplay.shot.minimumEndLag, lag);
        }

        if (cfg.code == "CORE" && coreUltimate && shot != ShotType.Smash)
        {
            shuttle.velocity *= gameplay.shot.coreUltimateNonSmashMultiplier;
            p.ultimateTimer = 0;
        }

        if (cfg.code == "TRICK" && trickUltimate && rules.abilitiesEnabled)
        {
            shuttle.spin += dir * gameplay.shot.trickUltimateSpinBonus;
            shuttle.velocity.y += gameplay.shot.trickUltimateLiftBonus;
            p.ultimateTimer = 0;
        }

        shuttle.position = RacketPosition(p) + new Vector2(dir * gameplay.shot.hitPositionForwardOffset, gameplay.shot.hitPositionYOffset);
        lastHitter = p.side;
        if (shot == ShotType.Smash)
        {
            stats.smashes++;
            TriggerScreenShake(gameplay.feedback.hitShakeMagnitude, gameplay.feedback.hitShakeDuration);
        }
    }

    private void StartSwing(PlayerActor p, float activeWindow, float cooldown)
    {
        CharacterConfig cfg = roster[p.characterIndex];
        if (rules.abilitiesEnabled && cfg.code == "DASH" && p.ultimateTimer > 0)
        {
            cooldown *= gameplay.swing.dashUltimateCooldownMultiplier;
        }
        else if (rules.abilitiesEnabled && cfg.code == "CORE" && p.skillTimer > 0)
        {
            cooldown *= gameplay.swing.coreSkillCooldownMultiplier;
        }

        p.swingTimer = gameplay.swing.swingPoseTimer;
        p.swingActiveTimer = activeWindow;
        p.swingCooldown = cooldown;
        p.swingConsumed = false;
    }

    private void UpdateEnergy(float dt)
    {
        float gain = gameplay.energy.passiveGainPerSecond * dt * (mode == GameMode.Practice || mode == GameMode.Tutorial ? sandboxEnergy : 1.0f);
        AddEnergy(leftPlayer, gain);
        AddEnergy(rightPlayer, gain);
    }

    private void AddEnergy(PlayerActor p, float amount)
    {
        p.energy = Mathf.Clamp(p.energy + amount * rules.energyMultiplier, 0, gameplay.energy.maxEnergy);
    }

    private void UpdateScreenShake(float dt)
    {
        if (!profile.screenShake || screenShakeTimer <= 0.0f)
        {
            screenShakeTimer = 0.0f;
            screenShakeMagnitude = 0.0f;
            screenShakeOffset = Vector2.zero;
            return;
        }

        screenShakeTimer = Mathf.Max(0, screenShakeTimer - dt);
        float fade = Mathf.Clamp01(screenShakeTimer / gameplay.feedback.screenShakeFadeDuration);
        screenShakeOffset = UnityEngine.Random.insideUnitCircle * screenShakeMagnitude * fade;
    }

    private void TriggerScreenShake(float magnitude, float duration)
    {
        if (!profile.screenShake)
        {
            return;
        }

        screenShakeMagnitude = Mathf.Max(screenShakeMagnitude, magnitude);
        screenShakeTimer = Mathf.Max(screenShakeTimer, duration);
    }

    private void ScorePoint(Side scorer, string reason)
    {
        if (mode == GameMode.Practice || mode == GameMode.Tutorial)
        {
            bool practiceFeedbackShown = mode == GameMode.Practice && TryEvaluatePracticeLanding(reason);
            if (mode == GameMode.Tutorial)
            {
                MarkTutorialFailure(reason);
            }

            BeginServe(Side.Left);
            if (practiceFeedbackShown)
            {
                ShowBanner(practiceDrill.LastFeedback);
            }
            else
            {
                ShowBanner((mode == GameMode.Tutorial ? "教程重置: " : "练习重置: ") + reason);
            }

            return;
        }

        if (FireChampionMatchRules.IsServeFault(stats.rallyHits, scorer, lastHitter))
        {
            stats.serveFaults++;
        }

        if (scorer == Side.Left)
        {
            leftScore++;
        }
        else
        {
            rightScore++;
        }

        bool smashWinner = lastHitter == scorer && shuttle.velocity.y < -1.0f;
        if (smashWinner)
        {
            stats.smashWinners++;
        }

        PlayTone(scorer == Side.Left ? FireChampionAudioCue.ScoreLeft : FireChampionAudioCue.ScoreRight, scorer == Side.Left ? gameplay.audio.scoreLeft : gameplay.audio.scoreRight);
        TriggerScreenShake(smashWinner ? gameplay.feedback.smashScoreShakeMagnitude : gameplay.feedback.scoreShakeMagnitude, gameplay.feedback.scoreShakeDuration);
        vfxSystem.SpawnScore(shuttle.position, scorer == Side.Left ? ActorAccent(leftPlayer) : ActorAccent(rightPlayer), smashWinner, gameplay.feedback);
        ShowBanner((scorer == Side.Left ? "P1" : "P2") + " 得分 · " + reason);

        if (FireChampionMatchRules.IsGameWon(rules, leftScore, rightScore))
        {
            leftGames++;
            FinishGameOrMatch(Side.Left);
            return;
        }

        if (FireChampionMatchRules.IsGameWon(rules, rightScore, leftScore))
        {
            rightGames++;
            FinishGameOrMatch(Side.Right);
            return;
        }

        BeginServe(scorer);
    }

    private bool TryEvaluatePracticeLanding(string reason)
    {
        if (reason != "落地")
        {
            return false;
        }

        bool evaluated = practiceDrill.TryEvaluateLanding(shuttle.position.x, lastHitter, CourtHalfWidth);
        if (evaluated)
        {
            practiceShotTimeline.RecordTargetResult(practiceDrill.LastTargetHit, practiceDrill.LastTargetMiss);
        }

        return evaluated;
    }

    private void FinishGameOrMatch(Side winner)
    {
        if (FireChampionMatchRules.ShouldStartNextGame(rules, leftGames, rightGames))
        {
            server = winner;
            BeginGameOnly();
            ShowBanner("本局结束，下一局开始");
            return;
        }

        matchOver = true;
        FireChampionMatchCompletion completion = FireChampionMatchFlow.ApplyCompletedMatch(profile, mode, tournamentRound, winner, leftScore, rightScore, leftGames, rightGames, stats);
        lastMatchPlayerWon = completion.playerWon;
        if (!qaSuppressProfileSave)
        {
            ProfileStore.Save(profile);
        }
        summaryTitle = completion.summaryTitle;
        summaryDetails = completion.summaryDetails;

        screen = ScreenState.Summary;
        if (mode == GameMode.NetworkHost)
        {
            network.SendSnapshot(CaptureSnapshot());
        }
    }

    private bool IsTournamentComplete()
    {
        return mode == GameMode.Tournament && TournamentProgression.IsFinalRound(tournamentRound);
    }

    private void UpdateTutorialProgress()
    {
        if (mode != GameMode.Tutorial || tutorialStep >= TutorialCoachData.StepCount)
        {
            return;
        }

        tutorialMoved = tutorialMoved || Mathf.Abs(leftInput.horizontal) > 0;
        tutorialJumped = tutorialJumped || leftInput.jumpPressed;

        if (tutorialStep == 0 && tutorialMoved && tutorialJumped)
        {
            AdvanceTutorial("移动和跳跃完成");
        }
        else if (tutorialStep == 1 && !waitingForServe && lastHitter == Side.Left)
        {
            AdvanceTutorial("发球完成");
        }
        else if (tutorialStep == 2 && tutorialLeftHits > 0)
        {
            AdvanceTutorial("成功击球");
        }
        else if (tutorialStep == 3 && tutorialLastLeftShot == ShotType.Smash)
        {
            AdvanceTutorial("扣杀完成");
        }
        else if (tutorialStep == 4 && stats != null && stats.skillsUsed > tutorialSkillBaseline)
        {
            tutorialStep = TutorialCoachData.StepCount;
            tutorialStepTimer = 0;
            tutorialRecoveryHint = "";
            ShowBanner("教程完成，可以自由练习或返回主菜单");
        }
    }

    private void UpdateTutorialCoachTimer(float dt)
    {
        if (mode != GameMode.Tutorial || tutorialStep >= TutorialCoachData.StepCount)
        {
            tutorialRecoveryHint = "";
            tutorialFailureReason = "";
            return;
        }

        tutorialStepTimer += dt;
        TutorialCoachStep step = TutorialCoachData.Step(tutorialStep);
        if (!string.IsNullOrEmpty(tutorialFailureReason))
        {
            tutorialRecoveryHint = "刚才" + tutorialFailureReason + "。 " + step.stuckHint;
        }
        else
        {
            tutorialRecoveryHint = tutorialStepTimer >= 7.5f ? step.stuckHint : "";
        }
    }

    private void MarkTutorialFailure(string reason)
    {
        tutorialFailureReason = string.IsNullOrEmpty(reason) ? "回合中断" : reason;
        tutorialStepTimer = 7.5f;
    }

    private void AdvanceTutorial(string message)
    {
        tutorialStep = Mathf.Min(tutorialStep + 1, TutorialCoachData.StepCount);
        tutorialStepTimer = 0;
        tutorialRecoveryHint = "";
        tutorialFailureReason = "";
        ShowBanner(message);
    }

    private void ResetTutorialState()
    {
        tutorialStep = 0;
        tutorialLeftHits = 0;
        tutorialSkillBaseline = stats == null ? 0 : stats.skillsUsed;
        tutorialMoved = false;
        tutorialJumped = false;
        tutorialLastLeftShot = ShotType.Flat;
        tutorialStepTimer = 0;
        tutorialRecoveryHint = "";
        tutorialFailureReason = "";
    }

    private string TutorialText()
    {
        if (tutorialStep < TutorialCoachData.StepCount)
        {
            return TutorialCoachData.Step(tutorialStep).instruction;
        }

        return "教程完成。继续练习回合，或返回主菜单进入快速比赛/本地双人。";
    }

    private string TutorialProgressText()
    {
        if (tutorialStep == 0)
        {
            return "进度: 移动 " + DoneText(tutorialMoved) + " · 跳跃 " + DoneText(tutorialJumped);
        }

        if (tutorialStep == 1)
        {
            return waitingForServe ? "进度: 等待发球，按 F 出球" : "进度: 发球已完成";
        }

        if (tutorialStep == 2)
        {
            return "进度: 已成功击球 " + tutorialLeftHits + " 次";
        }

        if (tutorialStep == 3)
        {
            return "进度: 最近一次击球 " + ShotLabel(tutorialLastLeftShot);
        }

        if (tutorialStep == 4)
        {
            int skillUses = stats == null ? 0 : Mathf.Max(0, stats.skillsUsed - tutorialSkillBaseline);
            return "进度: 技能使用 " + skillUses + " 次 · 当前能量 " + leftPlayer.energy.ToString("0.0") + "/" + gameplay.energy.maxEnergy.ToString("0.#");
        }

        return "进度: 教程完成";
    }

    private string TutorialHintText()
    {
        if (tutorialStep < TutorialCoachData.StepCount)
        {
            return TutorialCoachData.Step(tutorialStep).hint;
        }

        return "可以继续练习，也可以返回主菜单开始正式比赛。";
    }

    private string DoneText(bool done)
    {
        return done ? "完成" : "未完成";
    }

    private string ShotLabel(ShotType shot)
    {
        if (shot == ShotType.High) return "高远/挑高";
        if (shot == ShotType.Drop) return "短吊";
        if (shot == ShotType.Smash) return "扣杀";
        return "平抽";
    }

    private GameInput BuildAiInput(PlayerActor ai, PlayerActor opponent, float dt)
    {
        FireChampionAiContext context = new FireChampionAiContext();
        context.tuning = balance.ai;
        context.gameplay = gameplay;
        context.rules = rules;
        context.character = roster[Mathf.Clamp(ai.characterIndex, 0, roster.Length - 1)];
        context.profile = profile;
        context.shuttle = shuttle;
        context.racketPosition = RacketPosition(ai);
        context.courtWind = CurrentCourtWind();
        context.gravity = Gravity;
        context.shuttleDrag = ShuttleDrag;
        context.courtSpeedMultiplier = CurrentCourtSpeedMultiplier();
        context.courtHalfWidth = CourtHalfWidth;
        context.bannerTimer = bannerTimer;
        context.deltaTime = dt;

        FireChampionAiDecision decision = FireChampionAiController.BuildInput(ai, opponent, context);
        if (decision.showAdaptationBanner)
        {
            ShowBanner(FireChampionAiController.AdaptationBanner);
        }

        return decision.input;
    }

    private float CurrentCourtSpeedMultiplier()
    {
        return rules.ballSpeedMultiplier * (mode == GameMode.Practice ? sandboxBallSpeed : 1.0f);
    }

    private Vector2 CurrentCourtWind()
    {
        if (!rules.courtModifiersEnabled)
        {
            return Vector2.zero;
        }

        CourtConfig court = courts[profile.selectedCourt];
        float wave = Mathf.Sin(courtPhase * court.period);
        return new Vector2(wave * court.windX, Mathf.Cos(courtPhase * court.period * 0.7f) * court.windY);
    }

    private bool CrossedNetTooLow(Vector2 oldPosition, Vector2 newPosition)
    {
        if ((oldPosition.x < -NetWidth && newPosition.x >= -NetWidth) || (oldPosition.x > NetWidth && newPosition.x <= NetWidth))
        {
            float t = Mathf.InverseLerp(oldPosition.x, newPosition.x, 0);
            float y = Mathf.Lerp(oldPosition.y, newPosition.y, t);
            return y < NetHeight;
        }

        return false;
    }

    private Vector2 RacketPosition(PlayerActor p)
    {
        float reach = gameplay.swing.racketBaseReach + (p.swingTimer > 0 ? gameplay.swing.racketSwingReachBonus : 0.0f);
        return new Vector2(p.x + p.facing * reach, p.y + gameplay.swing.racketHeight + Mathf.Sin(p.swingTimer * gameplay.swing.racketSwingBobFrequency) * gameplay.swing.racketSwingBobAmount);
    }

    private Side Opposite(Side side)
    {
        return side == Side.Left ? Side.Right : Side.Left;
    }

    private void ApplyCharacterStats(PlayerActor p)
    {
        CharacterConfig cfg = roster[p.characterIndex];
        p.moveSpeed = cfg.moveSpeed;
        p.jumpForce = cfg.jumpForce;
        p.energy = 0.0f;
    }

    private void UpdateNetworkClient(float dt)
    {
        GameInput input = FireChampionInputMapper.Read(profile.p2);
        if (input.jumpPressed) networkJumpPulseTimer = gameplay.swing.networkJumpPulseTime;
        if (input.swingPressed) networkSwingPulseTimer = gameplay.swing.networkSwingPulseTime;
        if (input.skillPressed) networkSkillPulseTimer = gameplay.swing.networkSkillPulseTime;
        networkJumpPulseTimer = Mathf.Max(0, networkJumpPulseTimer - dt);
        networkSwingPulseTimer = Mathf.Max(0, networkSwingPulseTimer - dt);
        networkSkillPulseTimer = Mathf.Max(0, networkSkillPulseTimer - dt);
        input.jumpPressed = networkJumpPulseTimer > 0;
        input.swingPressed = networkSwingPulseTimer > 0;
        input.skillPressed = networkSkillPulseTimer > 0;
        network.SendInput(input);
        GameSnapshot snapshot;
        if (network.TryGetSnapshot(out snapshot))
        {
            ApplySnapshot(snapshot);
        }
    }

    private GameSnapshot CaptureSnapshot()
    {
        GameSnapshot s = new GameSnapshot();
        s.leftX = leftPlayer.x;
        s.leftY = leftPlayer.y;
        s.leftFacing = leftPlayer.facing;
        s.rightX = rightPlayer.x;
        s.rightY = rightPlayer.y;
        s.rightFacing = rightPlayer.facing;
        s.ballX = shuttle.position.x;
        s.ballY = shuttle.position.y;
        s.ballVx = shuttle.velocity.x;
        s.ballVy = shuttle.velocity.y;
        s.ballSpin = shuttle.spin;
        s.leftScore = leftScore;
        s.rightScore = rightScore;
        s.leftEnergy = leftPlayer.energy;
        s.rightEnergy = rightPlayer.energy;
        s.waitingServe = waitingForServe;
        s.server = server == Side.Left ? 0 : 1;
        s.leftGames = leftGames;
        s.rightGames = rightGames;
        s.matchOver = matchOver;
        s.summaryTitle = summaryTitle;
        s.summaryDetails = summaryDetails;
        s.leftCharacter = leftPlayer.characterIndex;
        s.rightCharacter = rightPlayer.characterIndex;
        s.selectedCourt = profile.selectedCourt;
        s.leftCosmetic = profile.selectedLeftCosmetic;
        s.rightCosmetic = profile.selectedRightCosmetic;
        s.abilitiesEnabled = rules.abilitiesEnabled;
        s.courtModifiersEnabled = rules.courtModifiersEnabled;
        s.pointsToWin = rules.pointsToWin;
        s.winBy = rules.winBy;
        s.hardCap = rules.hardCap;
        s.bestOf = rules.bestOf;
        s.outOfBounds = rules.outOfBounds;
        return s;
    }

    private void ApplySnapshot(GameSnapshot s)
    {
        leftPlayer.x = s.leftX;
        leftPlayer.y = s.leftY;
        leftPlayer.facing = s.leftFacing;
        rightPlayer.x = s.rightX;
        rightPlayer.y = s.rightY;
        rightPlayer.facing = s.rightFacing;
        shuttle.position = new Vector2(s.ballX, s.ballY);
        shuttle.velocity = new Vector2(s.ballVx, s.ballVy);
        if (s.hasMetadata)
        {
            shuttle.spin = s.ballSpin;
            leftPlayer.characterIndex = Mathf.Clamp(s.leftCharacter, 0, roster.Length - 1);
            rightPlayer.characterIndex = Mathf.Clamp(s.rightCharacter, 0, roster.Length - 1);
            profile.selectedLeftCharacter = leftPlayer.characterIndex;
            profile.selectedRightCharacter = rightPlayer.characterIndex;
            profile.selectedCourt = Mathf.Clamp(s.selectedCourt, 0, courts.Length - 1);
            profile.selectedLeftCosmetic = Mathf.Clamp(s.leftCosmetic, 0, cosmetics.Length - 1);
            profile.selectedRightCosmetic = Mathf.Clamp(s.rightCosmetic, 0, cosmetics.Length - 1);
            rules.pointsToWin = Mathf.Max(1, s.pointsToWin);
            rules.winBy = Mathf.Max(1, s.winBy);
            rules.hardCap = Mathf.Max(rules.pointsToWin, s.hardCap);
            rules.bestOf = Mathf.Max(1, s.bestOf);
            rules.outOfBounds = s.outOfBounds;
            rules.abilitiesEnabled = s.abilitiesEnabled;
            rules.courtModifiersEnabled = s.courtModifiersEnabled;
            ApplyCharacterStats(leftPlayer);
            ApplyCharacterStats(rightPlayer);
        }

        leftScore = s.leftScore;
        rightScore = s.rightScore;
        leftPlayer.energy = s.leftEnergy;
        rightPlayer.energy = s.rightEnergy;
        waitingForServe = s.waitingServe;
        server = s.server == 0 ? Side.Left : Side.Right;
        leftGames = s.leftGames;
        rightGames = s.rightGames;
        matchOver = s.matchOver;
        if (s.matchOver)
        {
            summaryTitle = string.IsNullOrEmpty(s.summaryTitle) ? "比赛结束" : s.summaryTitle;
            summaryDetails = string.IsNullOrEmpty(s.summaryDetails) ? "比分 " + leftScore + ":" + rightScore : s.summaryDetails;
            screen = ScreenState.Summary;
        }
    }

    private void DrawCourtSurface(Vector2 leftBase, Vector2 rightBase)
    {
        FireChampionMatchRenderer.DrawCourtSurface(WorldRenderContext(), leftBase, rightBase);
    }

    private void DrawNet()
    {
        FireChampionMatchRenderer.DrawNet(WorldRenderContext());
    }

    private void DrawPracticeTarget()
    {
        FireChampionMatchRenderer.DrawPracticeTarget(WorldRenderContext());
    }

    private void DrawTutorialTargetMarker()
    {
        FireChampionMatchRenderer.DrawTutorialTargetMarker(WorldRenderContext());
    }

    private void DrawStickman(PlayerActor p, Color accent)
    {
        FireChampionMatchRenderer.DrawPlayer(WorldRenderContext(), p, accent, RacketPosition(p));
    }

    private void DrawShuttle()
    {
        FireChampionMatchRenderer.DrawShuttle(WorldRenderContext());
    }

    private void DrawCourtBackdrop()
    {
        FireChampionMatchRenderer.DrawCourtBackdrop(WorldRenderContext());
    }

    private Vector2 WorldToScreen(Vector2 world)
    {
        return FireChampionMatchRenderer.WorldToScreen(WorldRenderContext(), world);
    }

    private void DrawPanel(float x, float y, float w, float h, Color color)
    {
        FireChampionGuiDrawing.DrawPanel(pixel, x, y, w, h, color);
    }

    private void DrawRect(Rect rect, Color color)
    {
        FireChampionGuiDrawing.DrawRect(pixel, rect, color);
    }

    private void DrawDisc(Vector2 center, float radius, Color color)
    {
        FireChampionGuiDrawing.DrawDisc(discTexture, center, radius, color);
    }

    private void DrawEllipse(Vector2 center, float radiusX, float radiusY, Color color)
    {
        FireChampionGuiDrawing.DrawEllipse(discTexture, center, radiusX, radiusY, color);
    }

    private void DrawCapsule(Vector2 a, Vector2 b, float width, Color color)
    {
        FireChampionGuiDrawing.DrawCapsule(pixel, discTexture, a, b, width, color);
    }

    private void DrawRectOutline(Rect rect, float width, Color color)
    {
        FireChampionGuiDrawing.DrawRectOutline(pixel, rect, width, color);
    }

    private void DrawLine(Vector2 a, Vector2 b, float width, Color color)
    {
        FireChampionGuiDrawing.DrawLine(pixel, a, b, width, color);
    }

    private void DrawCircle(Vector2 center, float radius, float width, Color color)
    {
        FireChampionGuiDrawing.DrawCircle(pixel, center, radius, width, color);
    }

    private string LabeledText(string label, string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(90));
        string next = GUILayout.TextField(value, GUILayout.Width(230));
        GUILayout.EndHorizontal();
        return next;
    }

    private float LabeledSlider(string label, float value, float min, float max)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label + ": " + value.ToString("0.00"), GUILayout.Width(145));
        float next = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(230));
        GUILayout.EndHorizontal();
        return next;
    }

    private void DrawBindingRow(string label, string target, KeyCode current)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(90));
        if (GUILayout.Button(current.ToString(), GUILayout.Width(140)))
        {
            rebindTarget = target;
        }
        GUILayout.EndHorizontal();
    }

    private void CaptureRebindKey()
    {
        if (Event.current == null || Event.current.type != EventType.KeyDown)
        {
            return;
        }

        KeyCode key;
        if (!FireChampionInputMapper.TryGetRebindKey(Event.current, out key))
        {
            return;
        }

        if (FireChampionInputMapper.Assign(profile, rebindTarget, key))
        {
            ProfileStore.Save(profile);
        }
        rebindTarget = "";
    }

    private void ShowBanner(string message)
    {
        banner = message;
        bannerTimer = gameplay.feedback.bannerDuration;
    }

    private void PlayTone(FireChampionAudioCue audioCue, BalanceToneCue cue)
    {
        if (cue == null)
        {
            return;
        }

        float volume = profile.masterVolume * cue.volumeMultiplier;
        if (audioAssets != null && audioAssets.Play(audioSource, audioCue, volume))
        {
            return;
        }

        tonePlayer.Play(audioSource, cue.frequency, cue.duration, volume);
    }

    private int ParsePort()
    {
        int port;
        if (!int.TryParse(portText, out port))
        {
            port = balance.network.defaultPort;
        }

        return Mathf.Clamp(port, balance.network.portMin, balance.network.portMax);
    }

    private string ModeName(GameMode currentMode)
    {
        if (currentMode == GameMode.QuickAi) return "快速比赛";
        if (currentMode == GameMode.LocalPvp) return "本地双人";
        if (currentMode == GameMode.Tournament) return "火柴冠军赛";
        if (currentMode == GameMode.Practice) return "自由沙盒";
        if (currentMode == GameMode.Tutorial) return "交互教程";
        if (currentMode == GameMode.NetworkHost) return "直连主机";
        return "直连客机";
    }

    private void Space(int pixels)
    {
        GUILayout.Space(pixels);
    }
}
