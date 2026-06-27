# Fire Champion Current Gap Audit - 2026-06-19

## Scope

This audit compares the current Unity project against the original target: a Windows-first Unity 2D local EXE for a feel-first 1v1 side-view badminton game, with quick match, local two-player, direct IP/LAN, tournament, free practice, interactive tutorial, four distinct roles, three courts, local profile persistence, player-habit AI, and cosmetic-only monetization hooks.

Evidence checked in this pass:

- `README.md`
- `docs/GapAnalysis-20260618.md`
- `outputs/LATEST_BUILD.txt`
- `outputs/unity-input-mapper-pass-build.log`
- `outputs/FireChampion_input_mapper_smoke.log`
- `outputs/unity-role-descriptions-validator.log`
- `outputs/unity-role-descriptions-build.log`
- `outputs/FireChampion_role_descriptions_smoke.log`
- `outputs/unity-ai-simulation-validator.log`
- `outputs/unity-ai-simulation-build.log`
- `outputs/FireChampion_ai_simulation_smoke.log`
- `docs/LayoutStaticAudit-20260619.md`
- `Assets/Scripts/*.cs`
- `Assets/Resources/FireChampion/Data/fire_champion_balance.json`
- `Assets/Resources/FireChampion/UI/*`

Important verification boundary: the latest evidence proves Unity build success, player startup, visible main-menu screenshot passes at 960x540, 1024x576, 1280x720, 1366x768, and 1920x1080 after the fixed-header menu pass, a 45-second real Player auto-match smoke, and a 1280x720 internal Player screenshot of the PNG VFX preview. It does not prove full manual play quality, two-machine LAN reliability, every possible localization/window-size layout, profiler budgets, or multi-hour stability.

## Current Coverage Matrix

| Original requirement | Current state | Evidence | Gap / risk |
| --- | --- | --- | --- |
| Unity 2D, Windows-first local EXE | Implemented, buildable, and packaged. Windows build forces Direct3D 11 and writes to `outputs/FireChampionWindows`. Build now runs project validation first. | `BuildFireChampion.cs`, `FireChampionProjectValidator.cs`, `outputs/unity-overlay-renderer-build-20260620-230132.log`, `outputs/LATEST_BUILD.txt` | Current verified state is the unzipped build plus the latest Windows/source zip packages listed in `outputs/LATEST_BUILD.txt`. |
| Quick match vs AI | Implemented, with a scripted AI decision audit covering four roles, three courts, common ball situations, a real Windows Player quick-match QA path that auto-serves into a visible match state, and a QA-only auto-match path that lets both sides play for longer smoke runs. A 2026-06-27 verified pass also added data-driven tactical shot choice: deep opponents can be punished with drops, net-hugging opponents can be lifted over, and high balls can receive extra pressure-smash bias. | `README.md`, `FireChampionGame.cs`, `FireChampionAiController.cs`, `FireChampionAiSimulation.cs`, `fire_champion_balance.json`, `outputs/unity-ai-tactical-validator-20260627-r2.log`, `outputs/unity-ai-tactical-build-20260627-r2.log`, `outputs/FireChampion_ai_tactical_autoplay_capture_20260627.png` | AI quality still needs hands-on tuning and a longer subjective playtest. Tactical AI is now covered by Unity validator/build and a short Player auto-match screenshot smoke; the AI simulation reported `tacticalDrop=12`, `rallyFrames=5040`, `rallyAiContacts=44`, `rallyLongestExchange=8`, and zero clamp/invalid-frame violations. |
| Local two-player | Implemented. | `README.md`, input bindings in `ProfileData.cs`, input mapper in `FireChampionInputMapper.cs` | Needs manual local two-player playtest after recent input extraction. |
| Direct IP / LAN multiplayer | Prototype implemented with host/client, diagnostics, input packets, snapshots, metadata sync, and RTT ping/pong diagnostics. | `DirectIpSession.cs`, `NetworkDiagnostics.cs`, `GameSnapshot`, README networking notes | Still alpha: no relay, room code, NAT traversal, interpolation, reconnect flow, or two-machine LAN validation. |
| Tournament mode | Implemented with three named opponents, bracket panel, round difficulty, medals/history, active-round resume, and restart confirmation for active runs. | `TournamentProgression.cs`, `ProfileData.cs`, README, gap history | Still needs richer tournament presentation, authored rewards, and manual save/load QA through visible UI. |
| Free sandbox practice | Implemented with targets, target accuracy, hit-timing feedback, a recent-shot timeline, and persistent practice history. | `PracticeDrillState.cs`, `HitTimingFeedback.cs`, `PracticeShotTimeline.cs`, `PracticeSessionStats.cs`, README | No replay system or deeper long-term practice analytics/charts yet. |
| Interactive tutorial | Implemented with staged coach text, input chips, markers, stuck hints, failure recovery. | `TutorialCoachData.cs`, README | Still not a dedicated authored tutorial scene; no tutorial animations. |
| 7 points, win by 2, 11 cap, scorer serves, out/net/landing scoring | Implemented and moved into data-driven/default rules paths. | `FireChampionRuntimeTypes.cs`, `fire_champion_balance.json`, gap history | Needs manual edge-case pass for deuce/cap/network snapshot consistency. |
| Tournament best-of-3 | Implemented through tournament rules. | `RulesetConfig.Tournament()`, gap history | Needs manual edge-case pass for match transitions, resume, and final-round cleanup. |
| Four roles CORE/DASH/HEAVY/TRICK with ability differences | Implemented with descriptions, strengths, weaknesses, recommended player type, skills, stats, data-driven tuning, and validator-covered visual silhouette differences. | `fire_champion_balance.json`, `FireChampionBalanceConfig.cs`, README | Needs balance playtest and authored animation/visual identity beyond procedural drawing. |
| Three courts: dojo, rooftop, future | Implemented with colors, wind/court modifiers, badges, procedural fallback layers, and first-pass 1280x720 full-scene PNG backgrounds for all three courts. | `fire_champion_balance.json`, `Assets/Resources/FireChampion/UI`, `Assets/Resources/FireChampion/Courts`, README | Court backgrounds are now asset-backed, but they are still prototype deterministic drawings and need higher-fidelity authored polish. |
| AI records player shot habits | Implemented for high/flat/smash habits and saved in profile. | `ProfileData.cs`, AI logic in `FireChampionGame.cs` | Habit model is simple; no detailed placement/style history yet. |
| Local profile: nickname, keys, settings, records, badges, AI habits | Implemented and saved beside executable/project, not C drive LocalLow. | `ProfileData.cs`, README, `outputs/LATEST_BUILD.txt` | Needs manual save/load test through visible UI after latest input module pass. |
| Cosmetic-only monetization reserve | Implemented as visual-only tint selections, profile fields, and a validator-covered data-driven cosmetic catalog. | `CosmeticConfig`, `fire_champion_balance.json`, README | No real-money store, unlock progression, item rarity, or asset-backed skins yet. |
| Feel-first game handling | Core loop has directional/timed shots, short swing window, input buffering, energy/skills, strengthened AI, data-driven tuning, PNG hit/smash/score/skill VFX with procedural fallback, and WAV feedback cues with synthesized fallback. A dedicated QA launch now freezes a representative VFX preview and the Player can capture it internally without relying on OS window handles. | `FireChampionGame.cs`, `FireChampionVfxSystem.cs`, `FireChampionVfxAssets.cs`, `FireChampionAudioAssets.cs`, `FireChampionQaCapture.cs`, `fire_champion_balance.json`, README, `outputs/FireChampion_internal_vfx_capture_20260627.png` | This is the most important subjective requirement and still needs manual playtest, frame-by-frame tuning, and possibly higher-fidelity authored animation/audio feedback. |
| Art should not remain a line-only game | Improved from thin line figures to procedural 2D athletes, themed courts, UI logo/icons/badges, first-pass full-scene court background PNGs, validator-covered data-driven athlete silhouettes/court/racket/trail proportions, transparent PNG VFX resources with procedural fallback, and visible VFX/character/court QA evidence from a real Windows Player window. | README, `Assets/Resources/FireChampion/UI`, `Assets/Resources/FireChampion/Courts`, `Assets/Resources/FireChampion/VFX`, `FireChampionCourtAssets.cs`, `FireChampionVfxAssets.cs`, `FireChampionVfxSystem.cs`, `fire_champion_balance.json`, `outputs/FireChampion_visible_qa_vfx_preview_dpi_1280x720_20260622-210627.png` | Still lacks authored character spritesheets, animation sets, final font/UI skin, and polished animation-grade court art. |
| Modular, maintainable code | Improved over many passes: profile, network, data config, tutorial data, tournament data, practice modules, practice shot timeline, input mapper, runtime types, match rules, AI input decisions, completed-match side effects, IMGUI drawing primitives, main/playing layout, overlay rendering, match-world rendering, player character rendering, and match HUD rendering extracted. | Script list, gap history | `FireChampionGame.cs` remains largest and still owns menu/practice/tutorial/tournament overlay details plus broad match orchestration. |
| Game values in separate file | Mostly implemented through `fire_champion_balance.json`. Audio cue tones, banner/screen-shake feedback, procedural VFX timing/radius/event-cap tuning, procedural 2D visual proportions, tactical AI shot choice, and visual-only cosmetic catalog data are now in balance data. Validator now requires the Resources JSON to load successfully instead of silently accepting fallback defaults. | `Assets/Resources/FireChampion/Data/README.md`, `FireChampionBalanceConfig.cs`, `outputs/unity-balance-load-report-validator-20260627-r2.log` | Some non-menu UI layout values remain in code. |
| Bug/quality checks | Latest Unity validation, Windows build, visible main-menu QA, targeted non-main-menu visible QA, VFX-preview visible QA, quick-match visible Player QA, 45-second auto-match Player QA, internal Player screenshot capture, summary backdrop cleanup, and summary variant Player QA passes succeeded. A Unity editor validator now checks key resources, balance data resource loading, role/court configs, ruleset invariants, sample match-rule outcomes, profile progression, AI controller basics, scripted AI decision simulation, continuous rally health, responsive main/playing layout, overlay summary-action routing, VFX lifecycle/event-cap behavior, practice timeline behavior, completed-match flow, QA launch argument parsing, QA summary/court argument parsing, and QA screenshot argument parsing. | `outputs/unity-summary-variants-validator-20260627-r2.log`, `outputs/unity-summary-variants-build-20260627-r2.log`, `outputs/FireChampion_summary_variant_win_capture_20260627_r2.png`, `outputs/FireChampion_summary_variant_loss_capture_20260627_r2.png`, `outputs/FireChampion_summary_variant_tournament-final_capture_20260627_r2.png`, `outputs/FireChampion_summary_variant_network-client_capture_20260627_r2.png`, plus earlier evidence listed in `outputs/LATEST_BUILD.txt` | No full manual test plan completion, no profiler capture, no two-machine network test, and no multi-hour stability test. |

## Completed Content

- Windows player build pipeline exists and completes successfully.
- Main modes exist: quick AI, local PvP, direct IP/LAN host/client, tournament, practice, tutorial.
- Core scoring rules, serve flow, out/net/landing scoring, and tournament match rules exist.
- Four roles have visible gameplay identity, role descriptions, ability text, strengths/weaknesses, and stat differences.
- Role/rule/AI/network/gameplay tuning is mostly data-driven through `fire_champion_balance.json`.
- Balance data loading now returns a `FireChampionBalanceLoadReport`; runtime `Load()` still falls back to defaults for resilience, but `FireChampionProjectValidator` requires the Resources JSON to be present, non-empty, parsed by `JsonUtility`, and not replaced by fallback data.
- Visual-only cosmetics now live in `fire_champion_balance.json` as a catalog with IDs, display names, descriptions, unlock labels, and tint colors.
- Local profile saves beside the EXE/project rather than C drive LocalLow.
- AI habit memory is persisted and used by AI decisions.
- AI now has a deterministic script-level simulation audit for movement, swing, jump, skill, adaptation banner, and target clamp behavior across the current roster/courts.
- The AI audit now also has a source-level continuous rally health path that tracks rally frames, AI contacts, feeder returns, multi-contact exchanges, target clamp violations, out-of-bounds resets, and invalid physics frames.
- AI shot choice now has a small tactical layer: `GameInput.dropIntent` can request a short drop, the AI controller chooses drops/lifts/smashes from opponent positioning through data-driven `BalanceAi` fields, and the AI simulation/validator source has a `tacticalDropDecisions` check path.
- Practice mode now supports targets, timing feedback, a recent-shot timeline, and long-term history.
- Tutorial mode now gives coach text, input visualization, target markers, stuck hints, and recovery hints.
- Tournament mode now has opponents, bracket display, difficulty ramp, medals, saved history, an active-run resume entry, and confirmation before overwriting an active run.
- Current Windows and Unity source zip packages were regenerated after the 2026-06-20 overlay-renderer extraction validation/build/smoke pass.
- Visual presentation improved with procedural athletes, themed courts, UI logo, role icons, court badges, full-scene court background PNGs, and source-level role silhouette tuning.
- Initial transparent PNG VFX resources now exist under `Assets/Resources/FireChampion/VFX` for sweet hits, solid hits, smash trails, dash burst, trick spin, and score burst. `FireChampionVfxAssets` loads them and `FireChampionVfxRenderer` renders them while keeping the procedural VFX as fallback.
- Initial generated WAV audio resources now exist under `Assets/Resources/FireChampion/Audio` for serve, normal hit, smash hit, skill, ultimate, and left/right score cues. `FireChampionAudioAssets` loads them while `ToneAudioPlayer` remains available as fallback.
- Internal QA screenshots now use `Assets/Scripts/FireChampionQaCapture.cs` with `-firechampion-qa-capture`, `-firechampion-qa-capture-delay`, and `-firechampion-qa-exit-after-capture`, avoiding fragile OS window-handle capture for Player QA.
- Asset-source tracking now exists in `Assets/Resources/FireChampion/ASSET_SOURCES.md`, and prompt-ready but approval-gated briefs for VFX, court backgrounds, role references, and authored audio are recorded in `docs/ArtDirectionAndAssetBriefs-20260619.md`.
- IMGUI drawing primitives now live in `Assets/Scripts/FireChampionGuiDrawing.cs`, reducing the amount of low-level rendering code inside `FireChampionGame.cs`.
- Match-world rendering for backgrounds, court surface, net, practice target, tutorial marker, shuttle drawing, player character drawing, and world-to-screen conversion now lives in `Assets/Scripts/FireChampionMatchRenderer.cs`; `RacketPosition` remains in `FireChampionGame.cs` and is passed into the renderer so hit logic stays with gameplay.
- Match HUD rendering for score, role labels, court/set text, network status, and energy bars now lives in `Assets/Scripts/FireChampionHudRenderer.cs`; `FireChampionGame.cs` now assembles the HUD context and keeps gameplay ownership of score, energy, mode, and network state.
- Practice recent-shot records now live in `Assets/Scripts/PracticeShotTimeline.cs`; `FireChampionGame.cs` records P1 practice contacts and target results while keeping long-term aggregation in `PracticeSessionStats.cs`.
- Main menu and playing overlay layout calculation now live in `Assets/Scripts/FireChampionLayout.cs`; the validator checks common resolutions for menu panels plus practice, tutorial, tournament, network, pause, and summary overlays. The main menu now keeps the Logo/title fixed in the first view while the left content area scrolls vertically, so lower buttons remain reachable and small windows do not open looking pre-scrolled.
- Banner, network-waiting, pause, and match-summary overlay drawing now lives in `Assets/Scripts/FireChampionOverlayRenderer.cs`; button clicks return action enums and `FireChampionGame.cs` remains responsible for state changes.
- Procedural hit/smash/score/skill VFX now live in `Assets/Scripts/FireChampionVfxSystem.cs`, with feedback timing, radius, and event cap stored in `fire_champion_balance.json`.
- Source now includes a development-only QA launch parser in `Assets/Scripts/FireChampionQaLaunch.cs`, allowing screenshot runs to open settings, network, practice, tutorial, tournament, pause, summary, network-waiting, VFX-preview, quick-match, and auto-match views directly through `-firechampion-qa-screen`; this is covered by Unity validation and the latest visible QA passes.
- The VFX-preview QA path freezes a representative practice-state scene so hit sparks, smash slash, skill aura, score burst, shuttle, athletes, and court art can be captured from the real Windows Player without waiting for a manual rally setup.
- The quick-match QA path starts a real Quick AI match and auto-serves the first shuttle so the built Player can be captured after actual scoring/HUD/rally-state updates.
- The auto-match QA path temporarily feeds AI input to P1 as well as P2, can run longer visible smoke tests, and suppresses QA profile saves so test runs do not mutate the player profile on disk.
- Code has been modularized in multiple low-risk passes:
  - `ProfileData.cs`
  - `DirectIpSession.cs`
  - `NetworkDiagnostics.cs`
  - `FireChampionBalanceConfig.cs`
  - `FireChampionUiAssets.cs`
  - `PracticeDrillState.cs`
  - `PracticeSessionStats.cs`
  - `PracticeShotTimeline.cs`
  - `HitTimingFeedback.cs`
  - `ToneAudioPlayer.cs`
  - `FireChampionAudioAssets.cs`
  - `TournamentProgression.cs`
  - `TutorialCoachData.cs`
  - `FireChampionRuntimeTypes.cs`
  - `FireChampionRuntimeBootstrap.cs`
  - `FireChampionInputMapper.cs`
  - `FireChampionMatchRules.cs`
  - `FireChampionAiController.cs`
  - `FireChampionAiSimulation.cs`
  - `FireChampionLayout.cs`
  - `FireChampionOverlayRenderer.cs`
  - `FireChampionMatchFlow.cs`
  - `FireChampionGuiDrawing.cs`
- `FireChampionMatchRenderer.cs`
- `FireChampionVfxSystem.cs`
- `FireChampionQaLaunch.cs`
- `FireChampionQaCapture.cs`
- `FireChampionProjectValidator.cs`

## Incomplete Or Thin Areas

- Full gameplay feel remains under-verified. Build/smoke success, the quick-match auto-serve Player QA, and the 45-second auto-match Player QA prove short automated main-flow stability, but they do not prove that movement, timing windows, AI pressure, and role balance feel right in hand.
- The 2026-06-27 tactical AI pass is now verified by Unity validator/build plus a short Player auto-match screenshot smoke. The first build attempt hit a Unity/Bee `AccessViolationException` after validation passed, but the immediate r2 retry succeeded and is the authoritative build evidence.
- Network mode is still a prototype. The UI is honest about direct IP/LAN and now has source-level RTT diagnostics, but public-network usability is not solved.
- Current verified zip deliverables were regenerated after the PNG VFX, generated WAV audio, internal QA screenshot, tactical AI, balance-load-report, court-background, summary-backdrop, and summary-variant passes. They include the responsive main-menu layout, playing overlay layout, overlay-renderer extraction, audio-data, feedback-data, role-ability-breakdown, asset-source/brief, AI rally-health, tactical AI shot choice, strict balance JSON loading validation, network RTT diagnostics, visual-data, cosmetic-catalog, GUI drawing extraction, match renderer extraction, player-renderer extraction, HUD renderer extraction, practice timeline, procedural VFX fallback, transparent PNG VFX, WAV audio cues with synthesized fallback, QA launch, internal QA capture, settings/tournament scroll, narrow-HUD overlap, VFX-preview QA, quick-match QA, auto-match QA, full-scene court background QA, summary backdrop QA, and summary variant QA passes.
- UI is still IMGUI with fixed pixel rectangles. It works for the prototype, its drawing primitives are centralized, main/playing layout calculations are validated, match/player world plus match HUD rendering are isolated, main-menu visible QA covers 960x540, 1024x576, 1280x720, 1366x768, and 1920x1080 for the fixed-header layout, and targeted non-main-menu QA now covers settings, network, practice, tutorial, tournament, pause, summary, summary win/loss/tournament-final/network-client variants, summary backdrop cleanup, and network-waiting views. Broader visible QA is still needed for localization-length changes, every settings scroll position, connected-network states, and polished release expectations.
- The responsive main-menu, playing overlay layout, settings scroll, tournament scroll, and narrow-HUD overlap fixes are covered by Unity validation/build plus screenshot evidence.
- Visuals are improved but still partly procedural. The game no longer reads as only thin line art, the procedural athlete/court proportions are now partly data-driven, initial transparent PNG VFX are integrated, first-pass full-scene court background PNGs now appear inside the match backdrop, and the next art briefs/source policy are documented, but it is not yet a full authored 2D sprite/animation production.
- Audio now has a first project-owned generated WAV pass for serve, hits, score, skill, and ultimate cues, with synthesized cached tones retained as fallback. It still lacks final mixed/polished authored SFX, UI/crowd ambience, and loudness balancing by ear.
- Tournament resume now exists for the active round, and restarting an active run asks for confirmation. It still needs manual end-to-end visible UI QA for save/load, cancel, and actually played loss/final-win flows.
- Practice now has a lightweight recent-shot timeline, but still lacks a true replay/per-shot trajectory viewer and long-term training charts.
- Cosmetics are now a data-driven visual-only catalog, but not a real unlock progression, shop, item-rarity, or asset-backed skin system.
- Automated coverage is still thin, but no longer absent: current verification includes Unity build, smoke logs, project validator checks, scripted AI decision simulation, code inspection, and manual reasoning.

## Inconsistencies With The Original Target

- The original target implied a polished local EXE. The project is currently a playable prototype with several polished-feeling systems, but UI, art, audio, and networking still read as prototype-quality.
- Direct friend/LAN play exists, but "friend direct IP" outside a LAN still depends on user-managed port forwarding and firewall configuration.
- Cosmetic monetization is only represented structurally; it is not a content-complete cosmetic system.
- "Feel first" is addressed technically through timing windows, buffers, AI tuning, and feedback, but the final proof requires hands-on tuning sessions, not only code inspection.

## Highest Priority Next Work

1. Manual gameplay QA pass: quick AI now has short and 45-second automatic visible smoke evidence, but quick AI/local PvP/practice/tutorial/tournament resume still need human hands-on review with exact mode/role/court notes.
2. Manual VFX/feel QA during unscripted real rallies: the frozen VFX-preview screen is now visibly verified, but active rally timing still needs human hands-on review.
3. Continue visible layout QA for deeper states: settings after scrolling to the bottom, tournament resume/restart confirmation, connected-network diagnostics, late tutorial steps, and actually played summary flows. QA summary screenshots now verify the cleaned-up darkened backdrop, hidden live HUD/serve prompt, loss branch, tournament-final branch, and network-client branch.
4. Continue modularization by extracting the remaining practice/tutorial/tournament overlay details, summary/menu UI, and the remaining `DrawGameWorld` orchestration from `FireChampionGame.cs`.
5. Continue separating match orchestration from presentation so future authored sprites/VFX can be imported without touching scoring or input logic.
6. Run active-rally QA against the new PNG VFX pass, then decide whether to polish these files directly or replace them with a higher-fidelity approved VFX sprite pack.
7. Run a real two-machine LAN test and document host/client setup, firewall prompts, and observed packet diagnostics.

## Current Verification Status

Passed:

- Unity batch build: `outputs/unity-input-mapper-pass-build.log`
- Headless EXE smoke: `outputs/FireChampion_input_mapper_smoke.log`
- Latest Unity batch build after match-rule extraction: `outputs/unity-match-rules-pass-build.log`
- Latest headless EXE smoke after match-rule extraction: `outputs/FireChampion_match_rules_smoke.log`
- Project validator: `outputs/unity-project-validator-pass.log`
- Build with preflight validation: `outputs/unity-project-validator-build.log`
- Latest headless EXE smoke after validator integration: `outputs/FireChampion_project_validator_smoke.log`
- Tournament resume validator: `outputs/unity-tournament-resume-validator.log`
- Tournament resume build: `outputs/unity-tournament-resume-build.log`
- Latest headless EXE smoke after tournament resume: `outputs/FireChampion_tournament_resume_smoke.log`
- Tournament restart confirmation validator: `outputs/unity-tournament-restart-confirm-validator.log`
- Tournament restart confirmation build: `outputs/unity-tournament-restart-confirm-build.log`
- Latest headless EXE smoke after restart confirmation: `outputs/FireChampion_tournament_restart_confirm_smoke.log`
- AI Controller Pass 19 validator: `outputs/unity-ai-controller-validator.log`
- AI Controller Pass 19 build: `outputs/unity-ai-controller-build.log`
- Latest headless EXE smoke after AI controller extraction: `outputs/FireChampion_ai_controller_smoke.log`
- Role descriptions / match-flow validator: `outputs/unity-role-descriptions-validator.log`
- Role descriptions / match-flow build: `outputs/unity-role-descriptions-build.log`
- Latest headless EXE smoke after role-description verification: `outputs/FireChampion_role_descriptions_smoke.log`
- AI simulation validator: `outputs/unity-ai-simulation-validator.log`
- AI simulation build: `outputs/unity-ai-simulation-build.log`
- Latest headless EXE smoke after AI simulation integration: `outputs/FireChampion_ai_simulation_smoke.log`
- 2026-06-20 Unity retry validator: `outputs/unity-retry-validator-20260620-1048.log`
- 2026-06-20 Unity retry build: `outputs/unity-retry-build-20260620-1048.log`
- 2026-06-20 headless EXE smoke: `outputs/FireChampion_retry_smoke_20260620-1055.log`
- GUI drawing extraction validator: `outputs/unity-gui-drawing-validator-20260620-1107.log`
- GUI drawing extraction build: `outputs/unity-gui-drawing-build-20260620-1107.log`
- GUI drawing extraction headless EXE smoke: `outputs/FireChampion_gui_drawing_smoke_20260620-1107.log`
- Match renderer extraction validator: `outputs/unity-match-renderer-validator-20260620-1118.log`
- Match renderer extraction build: `outputs/unity-match-renderer-build-20260620-1118.log`
- Match renderer extraction headless EXE smoke: `outputs/FireChampion_match_renderer_smoke_20260620-1118.log`
- Player renderer extraction validator: `outputs/unity-player-renderer-validator-20260620-1606.log`
- Player renderer extraction build: `outputs/unity-player-renderer-build-20260620-1606.log`
- Player renderer extraction headless EXE smoke: `outputs/FireChampion_player_renderer_smoke_20260620-1606.log`
- HUD renderer extraction validator: `outputs/unity-hud-renderer-validator-20260620-1617.log`
- HUD renderer extraction build: `outputs/unity-hud-renderer-build-20260620-1617.log`
- HUD renderer extraction headless EXE smoke: `outputs/FireChampion_hud_renderer_smoke_20260620-1617.log`
- Practice timeline validator: `outputs/unity-practice-timeline-validator-20260620-1629.log`
- Practice timeline build: `outputs/unity-practice-timeline-build-20260620-1629.log`
- Practice timeline headless EXE smoke: `outputs/FireChampion_practice_timeline_smoke_20260620-1629.log`
- Playing overlay layout validator: `outputs/unity-playing-overlay-layout-validator-20260620-1637.log`
- Playing overlay layout build: `outputs/unity-playing-overlay-layout-build-20260620-1637.log`
- Playing overlay layout headless EXE smoke: `outputs/FireChampion_playing_overlay_layout_smoke_20260620-1637.log`
- Procedural VFX validator: `outputs/unity-procedural-vfx-validator-20260620-224439.log`
- Procedural VFX build: `outputs/unity-procedural-vfx-build-20260620-224511.log`
- Procedural VFX headless EXE smoke: `outputs/FireChampion_procedural_vfx_smoke_20260620-224544.log`
- Overlay renderer validator: `outputs/unity-overlay-renderer-validator-20260620-225432.log`
- Overlay renderer build: `outputs/unity-overlay-renderer-build-20260620-230132.log`
- Overlay renderer headless EXE smoke: `outputs/FireChampion_overlay_renderer_smoke_20260620-230735.log`
- Main-menu scroll validator: `outputs/unity-main-menu-scroll-polish-validator-20260620-231756.log`
- Main-menu scroll build: `outputs/unity-main-menu-scroll-polish-build-20260620-231809.log`
- Main-menu visible 1280x720 QA screenshot: `outputs/FireChampion_visible_qa_menu_scroll_1280x720_20260620-231832.png`
- Fixed-header main-menu validator: `outputs/unity-menu-fixed-header-validator-20260620-233045.log`
- Fixed-header main-menu build: `outputs/unity-menu-fixed-header-build-20260620-233045.log`
- Fixed-header visible main-menu QA screenshots: `outputs/FireChampion_visible_qa_menu_fixed_header_960x540_20260620-233045.png`, `outputs/FireChampion_visible_qa_menu_fixed_header_1024x576_20260620-233045.png`, `outputs/FireChampion_visible_qa_menu_fixed_header_1280x720_20260620-233045.png`, `outputs/FireChampion_visible_qa_menu_fixed_header_1366x768_20260620-233045.png`, and `outputs/FireChampion_visible_qa_menu_fixed_header_1920x1080_20260620-233045.png`
- Non-main-menu QA launch validator: `outputs/unity-network-hud-label-validator-20260621-215504.log`
- Non-main-menu QA launch build: `outputs/unity-network-hud-label-build-20260621-215504.log`
- Non-main-menu visible QA screenshots include: `outputs/FireChampion_visible_qa3_network_1280x720_20260621-212915.png`, `outputs/FireChampion_visible_qa3_practice_1280x720_20260621-212915.png`, `outputs/FireChampion_visible_qa3_tutorial_1280x720_20260621-212915.png`, `outputs/FireChampion_visible_qa3_pause_1280x720_20260621-212915.png`, `outputs/FireChampion_visible_qa3_summary_1280x720_20260621-212915.png`, `outputs/FireChampion_visible_qa_settings_scrollfix_960x540_20260621-214110.png`, `outputs/FireChampion_visible_qa_tournament_hudfix_960x540_20260621-214614.png`, and `outputs/FireChampion_visible_qa_network_waiting_final_960x540_20260621-215504.png`
- Layout audit is now covered by validator output; `docs/LayoutStaticAudit-20260619.md` remains the static design note.
- Asset source/brief check is covered through `Assets/Resources/FireChampion/ASSET_SOURCES.md` and `docs/ArtDirectionAndAssetBriefs-20260619.md`.
- AI rally-health validation now reports rally frames, contacts, feeder returns, longest exchange, clamp violations, out-of-bounds resets, and invalid frames.
- Network RTT diagnostics are now covered by the latest validator; `DirectIpSession.cs` adds ping/pong packets and `NetworkDiagnostics.cs` reports RTT plus Ping/Pong counts.
- Visual data validation now covers `gameplay.visuals` plus per-character silhouette parameters.
- Cosmetic catalog validation now checks default entry, duplicate IDs, display text, unlock labels, tint alpha, and tint blending.
- Current Windows and Unity source zips: see `outputs/LATEST_BUILD.txt`
- Packaging check: Windows zip contains `FireChampion.exe` and excludes `FireChampionRecords`; source zip contains role balance data, `Assets\Scripts\FireChampionOverlayRenderer.cs`, `Assets\Scripts\FireChampionVfxSystem.cs`, `Assets\Scripts\FireChampionAiController.cs`, `Assets\Scripts\FireChampionAiSimulation.cs`, `Assets\Scripts\FireChampionMatchFlow.cs`, and excludes `Library`.
- Smoke error scan: no matches for `Exception`, `Error`, `Failed`, `Missing`, `NullReference`, `Crash`, `Duplicate`, `Json`, or `TypeLoad`.
- Static search: no open-task markers or merge-conflict markers found in source/resource/docs paths checked.
- Procedural VFX source pass static checks: UTF-8 JSON parse, brace counts, and conflict-marker search passed locally before Unity validation/build.
- VFX-preview QA launch validation/build passed in `outputs/unity-retry-validator-20260621-235159.log` and `outputs/unity-retry-build-20260621-235224.log`.
- VFX-preview player smoke stayed running after 8 seconds and was closed intentionally in `outputs/player-retry-smoke-20260621-235331.log`.
- VFX-preview visible Player-window screenshot passed manual review in `outputs/FireChampion_visible_qa_vfx_preview_dpi_1280x720_20260622-210627.png`; its run log `outputs/FireChampion_visible_qa_vfx_preview_dpi_1280x720_20260622-210627.log` had no matches for Exception/Error/Crash/Fatal/NullReference/MissingMethod/FileNotFound/DllNotFound/ArgumentException/Assertion.
- PNG VFX/internal capture validator passed in `outputs/unity-qa-capture-validator-20260627-r2.log`.
- PNG VFX/internal capture build passed in `outputs/unity-qa-capture-build-20260627.log`; the build report includes `FireChampionQaCapture.cs`, `UnityEngine.ScreenCaptureModule.dll`, and all six VFX PNG resources.
- Internal Player screenshot capture passed: `outputs/FireChampion_internal_vfx_capture_20260627.png` was written by the Player at 1280x720 and manually reviewed for visible PNG VFX.
- Quick-match QA launch validation/build passed in `outputs/unity-quick-match-qa-validator-20260622-211737.log` and `outputs/unity-quick-match-qa-build-20260622-211937.log`.
- Quick-match visible Player-window screenshot passed manual review in `outputs/FireChampion_visible_qa_quick_match_1280x720_20260622-212033.png`; its run log `outputs/FireChampion_visible_qa_quick_match_1280x720_20260622-212033.log` had no matches for Exception/Error/Crash/Fatal/NullReference/MissingMethod/FileNotFound/DllNotFound/ArgumentException/Assertion.
- Auto-match QA launch validation/build passed in `outputs/unity-autoplay-qa-validator-20260622-213034.log` and `outputs/unity-autoplay-qa-build-20260622-213102.log`.
- Auto-match 45-second Player-window smoke stayed running, produced `outputs/FireChampion_autoplay_longrun_45s_20260622-213200.png`, and its run log `outputs/FireChampion_autoplay_longrun_45s_20260622-213200.log` had no matches for Exception/Error/Crash/Fatal/NullReference/MissingMethod/FileNotFound/DllNotFound/ArgumentException/Assertion.
- Auto-match QA profile protection was checked in the same run: `FireChampionRecords/fire_champion_profile.json` existed before and after, with unchanged length and LastWriteTime ticks.
- Balance-load-report validator passed in `outputs/unity-balance-load-report-validator-20260627-r2.log`; the log explicitly reports `Loaded from Resources/FireChampion/Data/fire_champion_balance (15332 chars)`, then passes tactical AI simulation and project validation.
- Balance-load-report Windows build passed in `outputs/unity-balance-load-report-build-20260627.log`.
- Balance-load-report Player auto-match screenshot passed: `outputs/FireChampion_balance_load_autoplay_capture_20260627.png` was written by the Player and manually reviewed for visible match scene; `outputs/FireChampion_balance_load_player_20260627.log` had no real error matches and no FireChampion process remained.
- Summary backdrop cleanup validator/build passed in `outputs/unity-summary-backdrop-validator-20260627-r2.log` and `outputs/unity-summary-backdrop-build-20260627-r2.log`.
- Summary backdrop Player screenshot passed: `outputs/FireChampion_summary_backdrop_capture_20260627_r2.png` was written by the Player and manually reviewed for a darkened match backdrop, no live score HUD, no serve prompt residue, readable summary card, and visible future-court context; `outputs/FireChampion_summary_backdrop_player_20260627_r2.log` had no real error matches and no FireChampion process remained.
- Summary variant validator/build passed in `outputs/unity-summary-variants-validator-20260627-r2.log` and `outputs/unity-summary-variants-build-20260627-r2.log`; the build reports `Build Finished, Result: Success` and `Fire Champion Windows build result: Succeeded`.
- Summary variant Player screenshots passed for win/loss/tournament-final/network-client: `outputs/FireChampion_summary_variant_win_capture_20260627_r2.png`, `outputs/FireChampion_summary_variant_loss_capture_20260627_r2.png`, `outputs/FireChampion_summary_variant_tournament-final_capture_20260627_r2.png`, and `outputs/FireChampion_summary_variant_network-client_capture_20260627_r2.png` were written by the Player and manually reviewed for the correct tint, score, copy, and branch-specific first button; the corresponding `_player_20260627_r2.log` files had no real error matches and no FireChampion process remained.

Known warning:

- Unity build log includes licensing client handshake/access-token warnings, but the build still reports `Build Finished, Result: Success` and `Fire Champion Windows build result: Succeeded`.

Not yet proven:

- Full manual playthrough beyond the short automatic quick-match and 45-second auto-match smokes.
- Long-session stability beyond the 45-second auto-match smoke.
- Performance/profiler budget.
- Two-machine LAN.
- Manual visual review of PNG VFX timing/readability during unscripted real rallies.
