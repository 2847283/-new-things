# Fire Champion Gap Analysis - 2026-06-18

## Baseline

Original target: a Unity 2D Windows-first local EXE for a 1v1 side-view badminton game inspired by stickman badminton, with "feel first" as the core pillar. Required modes include quick match, local PvP, direct IP/LAN, tournament, practice sandbox, and interactive tutorial. Rules: 7 points, win by 2, hard cap 11, best-of-3 tournament matches, scorer serves, out/net/landing scoring. Roster: CORE, DASH, HEAVY, TRICK. Courts: dojo, rooftop, future. AI should learn player shot habits. Local profile should save nickname, bindings, settings, records, badges, and AI habits. Monetization should reserve cosmetics only, never strength.

Current implementation evidence:
- Main project docs: `README.md`, `outputs/LATEST_BUILD.txt`.
- Core implementation: `Assets/Scripts/FireChampionGame.cs`.
- Build path: `Assets/Editor/BuildFireChampion.cs`.
- Latest verified player package before this pass: `outputs/FireChampion-Windows-20260618-1332.zip`.

## Completed

- Windows EXE build exists, uses Direct3D 11, windowed 1280x720 defaults, and outputs under `outputs/FireChampionWindows`.
- Main modes are present: Quick AI, Local PvP, Network host/client, Tournament, Practice, Tutorial.
- Core rules are implemented: 7 points, win by 2, 11 hard cap, scorer serves, net/out/landing scoring, tournament best-of-3 match rules.
- Four characters exist and now have visible descriptions, playstyle notes, and gameplay differences.
- Three courts exist with code-drawn themed backgrounds and court modifiers.
- AI records the player's high/flat/smash habits and reads those ratios for positioning/shot choices.
- Profile saves nickname, key bindings, settings, match record, badges, and AI habits beside the EXE/project instead of C drive LocalLow.
- The prototype is visually upgraded from thin line figures to original procedural 2D athletes and themed courts.

## Missing Or Thin

- Cosmetic-only monetization is partially represented by visual-only tint configs and profile selections, but unlock data and a real cosmetic catalog are still absent.
- Network play is still alpha quality: host-authoritative snapshots sync more metadata, but relay, room codes, NAT traversal, interpolation, and full two-machine validation are absent.
- Network snapshot metadata now covers selected characters, cosmetics, court, rule flags, scoring rules, out-of-bounds mode, and shuttle spin.
- Practice mode now has target drills, landing feedback, hit timing feedback, and persistent aggregate practice history, but still lacks replay and a detailed per-shot timeline.
- Tutorial now has a five-step coach flow with input visualization, spatial target markers, stuck hints, and failure recovery nudges, but still lacks a dedicated staged tutorial scene.
- Tournament now has bracket presentation, opponent identity, round rewards, persistent history, and active-run resume support.
- Assets are still mostly procedural. A small original UI `Resources` library now exists for logo, role icons, and court badges, but authored animation sprites, audio cues, fonts, and prefabs are still absent.

## Experience Issues

- `FireChampionGame.cs` is a large single-file prototype mixing UI, rules, rendering, networking, AI, profile, audio, and data definitions.
- UI is IMGUI/GUILayout based with many fixed pixel areas. It is workable for prototype builds but fragile at unusual resolutions or long text values.
- The `screenShake` setting exists but previously had no visible effect.
- `serveFaults` was displayed in match summary but not incremented.
- `bestOf` match completion was hardcoded to "first to 2 games" instead of derived from the ruleset.
- Player nickname can be edited beyond the intended length during settings entry.
- `PlayTone` creates a new AudioClip for each tone and should eventually be cached or replaced by authored audio assets.
- Profile save failures are swallowed silently; this is safe for gameplay but weak for user trust.

## Highest Priority Improvements

1. Add practice target drills and basic feedback so sandbox mode better supports the "feel first" goal.
2. Add cosmetic-only data and UI selection that changes appearance without touching strength fields.
3. Sync network snapshot metadata for character selection, court, rules toggles, and shuttle spin.
4. Fix small correctness issues: `bestOf` calculation, serve fault stat, nickname clamping, and `screenShake` behavior.
5. Start low-risk modularization by extracting pure data/profile/network types out of `FireChampionGame.cs`.
6. Keep network labeled as alpha until a real two-machine LAN test and sync/interpolation pass are done.

## Implementation Plan For This Pass

- Implement practice target and hit/landing feedback using the existing procedural renderer.
- Add a small cosmetic config list with visual-only palette choices and profile fields for selected cosmetic indices.
- Extend `GameSnapshot` encode/decode to sync character/court/rules/spin while remaining backward-compatible with older snapshot packets.
- Fix the small correctness issues listed above.
- Build, smoke test, package, and update delivery notes.

## Implemented In Gap Pass 1

- Practice sandbox now has a right-half landing target, hit/attempt accuracy, current streak, best streak, target refresh, and landing feedback banners.
- Cosmetic-only appearance selection now exists for P1 and P2/AI. It changes visual tint only and does not modify strength fields.
- Network snapshots now append metadata for characters, cosmetics, selected court, rule toggles, scoring rules, out-of-bounds mode, and shuttle spin.
- `DirectIpSession` status, remote input, stream references, and latest snapshot access are safer across the network thread and main thread.
- `bestOf` match completion now derives required games from the ruleset.
- Serve fault statistics now increment when the server loses the point before a rally hit.
- Nicknames are clamped live in settings, not only after profile load.
- The `screenShake` setting now visibly affects the game world on smashes and points.

## Implemented In Role/Resource Pass 2

- Added original UI resource PNGs under `Assets/Resources/FireChampion/UI/` for the main logo, four role icons, and three court badges.
- Added `FireChampionUiAssets` to load the UI resources through `Resources.Load` instead of keeping UI asset lookup inside the main game script.
- Added `PracticeDrillState` to move practice target scoring state out of the main game script.
- Expanded `CharacterConfig` with per-role description, playstyle, and ability-difference fields.
- The role info card now shows each character's role, description, recommended playstyle, ability differences, passive, small skill, full-energy skill, and stat flavor.
- The role info card is scrollable so the new role descriptions do not get clipped on smaller windows.
- Fixed the remaining practice target drawing references to use `PracticeDrillState.TargetX` and `TargetRadius`.
- Added `ToneAudioPlayer` so synthesized UI/gameplay tones are cached by frequency and duration instead of creating a new `AudioClip` on every hit or score.
- Unity build verification passed in `outputs/unity-role-resource-audio-pass-build.log` at 2026-06-18 15:03 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_role_resource_audio_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Tutorial/Tournament Pass 3

- Added `TournamentProgression` as a small data module for named opponents, round labels, scouting reports, and round difficulty.
- Tournament HUD now shows current round, opponent name, opponent style note, and difficulty.
- Tournament start banners and match summaries now mention the actual opponent and championship win state.
- Tournament AI difficulty and final-round badge/win checks now come from `TournamentProgression` instead of hardcoded round numbers.
- Tutorial coach now shows current progress, contextual correction hints, and live P1 key chips for A/D, W, S, F, and G.
- Unity build verification passed in `outputs/unity-tutorial-tournament-pass-build.log` at 2026-06-18 15:16 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_tutorial_tournament_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Profile Module Pass 4

- Moved `KeyBinding`, `ProfileData`, and `ProfileStore` out of `FireChampionGame.cs` into `Assets/Scripts/ProfileData.cs`.
- `FireChampionGame.cs` is smaller and no longer owns local profile serialization details.
- `ProfileStore.Save` now returns success/failure and records `LastStatus` plus `LastOperationSucceeded`.
- The main menu now shows the current profile directory and latest profile status, so save/load failures are visible instead of silently swallowed.
- Profile storage still resolves beside `Application.dataPath` under `FireChampionRecords`, keeping player data with the executable/project rather than C drive LocalLow.
- Unity build verification passed in `outputs/unity-profile-module-pass-build.log` at 2026-06-18 15:22 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_profile_module_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Network Diagnostics Pass 5

- Added `NetworkDiagnostics` to track connection time, sent/received input packets, sent/received snapshot packets, and latest send/receive age.
- Direct IP host/client sessions now mark diagnostics on connect, send, and receive.
- The network menu now shows live diagnostics below the connection status.
- Network matches now show a small in-match diagnostics panel with connection status and packet health.
- Waiting-for-connection UI now includes diagnostics, making failed joins and silent connections easier to understand.
- Connected sessions show a warning when no peer data has been received for more than 3 seconds.
- Unity build verification passed in `outputs/unity-network-diagnostics-pass-build.log` at 2026-06-18 15:28 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_network_diagnostics_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Practice Hit Feedback Pass 6

- Added `HitTimingFeedback` to classify practice hits as sweet-spot, solid, or edge hits based on racket distance and current hit radius.
- Free practice now displays last hit quality, shot type, sweet-spot rate, clean-hit rate, and best clean-hit streak.
- Free practice now gives contextual timing/positioning hints after each player hit.
- Practice reset now clears both landing target stats and hit timing stats.
- The hit flow now reuses a single hit-distance calculation instead of measuring racket distance twice.
- Unity build verification passed in `outputs/unity-practice-hit-feedback-pass-build.log` at 2026-06-18 15:33 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_practice_hit_feedback_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Practice History Pass 7

- Added `PracticeSessionStats` and `PracticeSessionSummary` to capture a free-practice session without putting more aggregation logic into the main game script.
- `ProfileData` now persists lifetime practice sessions, target attempts/hits, best target streak, hit contacts, sweet-spot hits, solid/edge hits, and best clean-hit streak.
- Free practice now shows saved lifetime target accuracy, sweet-spot rate, clean-hit rate, and best clean streak alongside the current session.
- The main menu profile card now shows practice history summary and best practice streaks.
- Practice reset, practice return-to-menu, pause restart/return, and application shutdown now try to commit the current practice session before clearing or leaving the mode.
- Settings now include a `清空练习历史` action for resetting only the practice history fields.
- Unity build verification passed in `outputs/unity-practice-history-pass-build.log` at 2026-06-18 23:34 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_practice_history_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Tournament Bracket/Reward Pass 8

- Expanded `TournamentProgression` so each tournament round owns its opponent, scouting note, difficulty, and cosmetic-only honor medal reward.
- Added persistent tournament history fields to `ProfileData`: runs started, rounds won, best round reached, finals reached, runner-up count, and honor medals.
- Starting a new tournament run now records the run in the local profile and saves it beside the executable/project.
- Tournament match results now persist round wins, best progress, runner-up/finals history, and medal rewards without affecting character strength.
- The in-match tournament panel now shows the full three-round bracket, current/cleared/locked state, opponent names, and per-round medal rewards.
- The main menu profile card now shows tournament championships, best reached round, and medal count.
- Tournament summaries now show medals earned on wins, historical progress on losses, and the summary retry button restarts a fresh tournament run.
- Unity build verification passed in `outputs/unity-tournament-bracket-pass-build.log` at 2026-06-18 23:42 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_tournament_bracket_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Implemented In Tutorial Coach Pass 9

- Added `TutorialCoachData` and `TutorialCoachStep` so tutorial text, hints, stuck hints, marker labels, marker positions, and marker radius live outside the main game script.
- Tutorial mode now draws a pulsing on-court target marker and arrow for the current step.
- Tutorial coach UI now shows the current step title from data, live progress, base hint, and an extra coach recovery line when the player stalls or fails a rally.
- Tutorial steps now use `TutorialCoachData.StepCount` instead of scattered hardcoded step counts.
- Rally resets in tutorial mode now record the failure reason and immediately surface a recovery hint instead of waiting silently for the player to infer the fix.
- Unity build verification passed in `outputs/unity-tutorial-coach-pass-build.log` at 2026-06-18 23:50 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_tutorial_coach_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Remaining Risks After Tutorial Coach Pass 9

- The core game is still mostly in one large `FireChampionGame.cs` file, though profile, practice drill, audio tones, UI resource loading, and tournament progression have been extracted. Data/network/rendering extraction remains recommended.
- Network mode is still alpha. Diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Practice mode has target drills, hit-timing feedback, and persisted aggregate history, but still lacks replay and a deeper per-shot timeline over time.
- Practice landing evaluation still depends on the current landing reason string, so a future rules/event extraction should replace that text-coupled hook.
- Tutorial now has input visualization, spatial markers, stuck hints, and failure recovery nudges, but still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament now has a visible bracket, cosmetic-only honor medals, persistent history, and active-run resume support after returning to menu or restarting.
- Audio is still synthesized at runtime. Tone clips are now cached, but authored sound effects would still improve feel and polish.

## Implemented In Role Balance Data Pass 10

- Added `Assets/Resources/FireChampion/Data/fire_champion_balance.json` as the first designer-editable balance file.
- Moved the four role profiles into data: display name, combat role, description, playstyle, ability differences, strengths, weaknesses, recommended player type, passive, small skill, full-energy skill, stat flavor, and core stats.
- Added `FireChampionBalanceConfig` to load the JSON through `Resources.Load`, with default fallback values if the data file is missing or invalid.
- Main menu role buttons now show each role's tactical identity, and the selected P1/P2角色 area shows a short description plus ability-difference summary.
- The role info card now includes explicit strengths, weaknesses, and recommended player type for CORE, DASH, HEAVY, and TRICK.
- Match rules, tutorial/practice/quick AI difficulty, direct-IP default port/slider ranges, and court wind/color values now also read from the balance config.
- Added `Assets/Resources/FireChampion/Data/README.md` to document which fields are intended for balance tuning.
- Unity build verification passed in `outputs/unity-role-balance-pass-build.log` at 2026-06-19 10:13 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_role_balance_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Remaining Risks After Role Balance Data Pass 10

- Major role, rule, AI, network slider, and court values now live in `fire_champion_balance.json`, but lower-level shot tuning constants inside the rally/skill code still need a future extraction pass.
- The core game is still mostly in one large `FireChampionGame.cs` file, though balance data, profile, practice drill, audio tones, UI resource loading, tournament progression, and tutorial coach data have been extracted.
- Network mode is still alpha. Diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Gameplay Balance Data Pass 11

- Expanded `fire_champion_balance.json` with a `gameplay` section for court physics, free-practice tuning, energy economy, skill timing, swing windows, racket reach, serve values, shot speeds, spin, smash end-lag, and shuttle prediction.
- Expanded the `ai` section with behavior tuning values for prediction cadence, target offset, learned-habit counters, movement deadzone, jump/swing probability, shot choice, skill use, and adaptation banner cadence.
- `FireChampionGame` now reads the core feel constants from `BalanceGameplay` instead of duplicating them in the match loop.
- Practice sandbox ball-speed and energy sliders now use data-driven default values and ranges.
- Energy costs, max energy, passive gain, good/normal hit gain, CORE precision bonus, and long-rally bonus are now data-driven.
- Role skill durations, dash distance, skill cooldowns, ultimate duration, movement multipliers, and CORE end-lag reduction are now data-driven.
- Serve timing, swing buffer, hit active window, sweet-spot ratio, role hit-radius bonuses, and racket reach/bob are now data-driven.
- High, flat, drop, and smash shot values are now data-driven, including timing multipliers, control bonuses, trick spin, heavy smash pressure, empowered smash cost/bonus, and smash recovery.
- The energy HUD now derives its segment count from the configured max energy instead of assuming exactly three segments.
- Added `gameplay` and expanded `ai` notes to `Assets/Resources/FireChampion/Data/README.md`.
- Unity build verification passed in `outputs/unity-gameplay-balance-pass-build.log` at 2026-06-19 10:27 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_gameplay_balance_smoke.log`; startup reached engine initialization with no searched errors or exceptions.

## Remaining Risks After Gameplay Balance Data Pass 11

- Most player-facing role, rule, AI, physics, energy, skill, swing, and shot values now live in `fire_champion_balance.json`; some fallback defaults remain in C# so the game can still boot if the data file is missing.
- Visual layout, procedural art drawing sizes, tone frequencies, and screen-shake/audio feel values are still mostly embedded in code and could be extracted in a future polish/config pass.
- The core game is still mostly in one large `FireChampionGame.cs` file. Balance data, profile, practice drill, audio tones, UI resource loading, tournament progression, and tutorial coach data have been extracted, but rendering/network/match-flow extraction remains recommended.
- Network mode is still alpha. Diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Network Module Pass 12

- Moved `DirectIpSession` out of `FireChampionGame.cs` into `Assets/Scripts/DirectIpSession.cs`.
- `FireChampionGame.cs` no longer carries the direct-IP socket/thread implementation or the unused network/IO usings.
- The direct-IP class keeps the existing host, join, stop, input send, snapshot send, snapshot receive, and diagnostics behavior unchanged.
- Unity build verification passed in `outputs/unity-network-module-pass-build.log` at 2026-06-19 10:36 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_network_module_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Network Module Pass 12

- `FireChampionGame.cs` is smaller, but rendering, match-state flow, rule events, input mapping, and HUD drawing are still concentrated in the main script.
- Network mode remains prototype-level direct IP/LAN sync; diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Most player-facing tuning is data-driven, but visual layout, procedural art drawing sizes, tone frequencies, and screen-shake/audio feel values are still mostly embedded in code.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Runtime Types Pass 13

- Moved runtime enums, rules config, character/court/cosmetic config types, player actor state, shuttle state, input packets, match stats, and network snapshot encoding out of `FireChampionGame.cs` into `Assets/Scripts/FireChampionRuntimeTypes.cs`.
- Moved the scene bootstrapper into `Assets/Scripts/FireChampionRuntimeBootstrap.cs`.
- `FireChampionGame.cs` now focuses more tightly on orchestration, input, UI, rendering, AI, and match flow instead of owning all runtime data models.
- The moved snapshot encode/decode behavior remains unchanged so direct-IP compatibility is preserved.
- Unity build verification passed in `outputs/unity-runtime-types-pass-build.log` at 2026-06-19 10:44 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_runtime_types_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Runtime Types Pass 13

- `FireChampionGame.cs` is still the largest file. Rendering, match-state flow, scoring/rule events, input mapping, AI decision-making, and IMGUI screen drawing should be extracted in future focused passes.
- Network mode remains prototype-level direct IP/LAN sync; diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Most player-facing tuning is data-driven, but visual layout, procedural art drawing sizes, tone frequencies, and screen-shake/audio feel values are still mostly embedded in code.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Input Mapper Pass 14

- Added `Assets/Scripts/FireChampionInputMapper.cs` to centralize keyboard input reads, tutorial key-highlight state, rebind key capture, and key-target assignment.
- `FireChampionGame.cs` now calls the input mapper instead of owning per-frame binding reads and the string-to-key assignment chain.
- Settings key rows now use shared target constants such as `FireChampionInputMapper.P1Left`, reducing scattered key target strings.
- Direct-IP client input still sends the same `GameInput` packet structure, preserving network packet compatibility.
- Unity build verification passed in `outputs/unity-input-mapper-pass-build.log` at 2026-06-19 10:49 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_input_mapper_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Input Mapper Pass 14

- `FireChampionGame.cs` still owns IMGUI screens, gameplay flow, AI decision-making, scoring, and procedural rendering. These should continue to be extracted in focused passes.
- The current input module still uses Unity's legacy `Input` API because the project is built around IMGUI and existing key bindings; a later input-system migration would need a separate compatibility plan.
- Network mode remains prototype-level direct IP/LAN sync; diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Most player-facing tuning is data-driven, but visual layout, procedural art drawing sizes, tone frequencies, and screen-shake/audio feel values are still mostly embedded in code.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Match Rules Pass 15

- Added `Assets/Scripts/FireChampionMatchRules.cs` for pure match-rule calculations.
- Moved game-win checks, best-of match target calculation, next-game continuation checks, and serve-fault detection out of `FireChampionGame.cs`.
- `FireChampionGame.cs` still owns match flow and side effects, but it now calls the focused match-rule module for score/rules decisions.
- Existing score, hard-cap, win-by-two, best-of, and serve-fault behavior was preserved.
- Unity build verification passed in `outputs/unity-match-rules-pass-build.log` at 2026-06-19 15:02 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_match_rules_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Match Rules Pass 15

- `FireChampionGame.cs` still owns match flow side effects such as profile updates, tournament rewards, summaries, tutorial/practice hooks, and network snapshots.
- The next useful modularization step is a dedicated match-flow or scoring-event layer so practice/tutorial/tournament/network reactions can subscribe to clearer events.
- Network mode remains prototype-level direct IP/LAN sync; diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Most player-facing tuning is data-driven, but visual layout, procedural art drawing sizes, tone frequencies, and screen-shake/audio feel values are still mostly embedded in code.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing in this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Project Validator Pass 16

- Added `Assets/Editor/FireChampionProjectValidator.cs` with a Unity editor menu item: `Fire Champion > Validate Project`.
- The validator checks the startup scene, balance JSON asset, required UI resources, role configs, court configs, core ruleset invariants, basic gameplay tuning invariants, sample match-rule outcomes, and profile tournament progression.
- `BuildFireChampion.BuildWindowsExe` now runs `FireChampionProjectValidator.ValidateOrThrow()` before applying player settings and building Windows output.
- Validation can be run in batchmode with `-executeMethod FireChampionProjectValidator.ValidateOrThrow`.
- Standalone validation passed in `outputs/unity-project-validator-pass.log`.
- Build-with-validation passed in `outputs/unity-project-validator-build.log`.
- Hidden EXE smoke test passed in `outputs/FireChampion_project_validator_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Project Validator Pass 16

- The validator checks project integrity and core invariants, but it is not a substitute for manual gameplay QA, two-machine LAN testing, profiler captures, or multi-resolution UI checks.
- `FireChampionGame.cs` still owns match flow side effects, AI decisions, IMGUI screens, and procedural rendering.
- Network mode remains prototype-level direct IP/LAN sync; diagnostics are visible, but there is still no interpolation, explicit ping/round-trip latency, reconnection flow, relay, room code, NAT traversal, or full two-machine LAN verification.
- Latest zip deliverables were stale at this point; they were refreshed after AI Controller Pass 19.
- Tutorial still lacks a dedicated staged tutorial scene and authored tutorial animations.
- Tournament resume was still missing after this pass; it is addressed in Tournament Resume Pass 17.
- Audio is still synthesized at runtime; authored sound effects would improve feel and polish.

## Implemented In Tournament Resume Pass 17

- Added active tournament state to `ProfileData`: whether a run is in progress and which round should resume.
- Starting a tournament now saves round 0 as the active run; winning a non-final round saves the next round; losing or winning the final clears the active run.
- Main menu now shows `继续火柴冠军赛` when an active run exists, plus a separate restart button for a fresh run.
- Returning to the main menu after a non-final win preserves the unlocked next round, and restarting the game can resume from that round.
- Project validation now includes profile progression checks for starting a tournament, advancing after a non-final win, clearing after a loss, and clearing after a final win.

## Remaining Risks After Tournament Resume Pass 17

- Tournament resume still needs manual visible UI QA across application restart, accidental restart, loss, and final win.
- At this point, active-run restart confirmation was still missing; this is addressed in Tournament Restart Confirmation Pass 18.
- Direct IP remains an alpha feature and still needs a real two-machine LAN test.
- `FireChampionGame.cs` remains the largest script and still owns rendering, match orchestration, summary side effects, and AI decisions.

## Implemented In Tournament Restart Confirmation Pass 18

- Main menu restart now uses a two-step confirmation when an active tournament run exists.
- Canceling the confirmation leaves the active tournament profile state untouched.
- Confirming restart intentionally starts a fresh run and overwrites the active tournament state.
- Starting or resuming any match clears the transient confirmation UI state so it cannot leak between screens.

## Remaining Risks After Tournament Restart Confirmation Pass 18

- The restart confirmation still needs visible UI QA at common window sizes and after returning from other screens.
- Tournament resume still needs manual save/load QA across application restart, loss, final win, and fresh-run overwrite.
- Direct IP remains an alpha feature and still needs a real two-machine LAN test.
- `FireChampionGame.cs` remains the largest script and still owns rendering, match orchestration, summary side effects, and AI decisions.

## Implemented In AI Controller Pass 19

- Added `Assets/Scripts/FireChampionAiController.cs` for AI input decisions.
- Moved AI target selection, home positioning, target clamping, landing prediction, jump/swing/skill input choice, shot vertical choice, and adaptation-banner signaling out of `FireChampionGame.cs`.
- `FireChampionGame.cs` now builds a small AI context from current match state and delegates decision-making to the AI controller.
- Project validation now includes a deterministic AI controller check for home-target movement and court clamping.
- Existing AI tuning values remain in `fire_champion_balance.json`; this pass is a modularization pass, not a rebalance pass.

## Remaining Risks After AI Controller Pass 19

- Unity validation, Windows build, and hidden EXE smoke passed after the AI controller extraction.
- AI still needs manual playtest and a future deterministic gameplay simulation before balance changes can be compared confidently.
- `FireChampionGame.cs` remains the largest script and still owns rendering, match orchestration, summary side effects, and IMGUI screens.
- Direct IP remains an alpha feature and still needs a real two-machine LAN test.

## Implemented In Role Descriptions / Match Flow Pass 20

- Confirmed the four role profiles are data-driven in `Assets/Resources/FireChampion/Data/fire_champion_balance.json`.
- CORE/DASH/HEAVY/TRICK each have display name, tactical role, description, playstyle, ability-difference text, strengths, weakness, recommended player type, passive, small skill, full-energy skill, stat flavor, and core ability values.
- The main menu selected-role summaries and right-side role card show the role explanation, ability differences, and speed/smash/control/forgiveness ability bars.
- Added `Assets/Scripts/FireChampionMatchFlow.cs` for completed-match side effects: win/loss records, tournament rewards/resume cleanup, badge updates, and summary text.
- `FireChampionGame.cs` now delegates completed-match profile/tournament/summary handling to the focused match-flow module.
- Project validation now includes completed-match flow checks for a quick-match win and final tournament win.
- Unity validation passed in `outputs/unity-role-descriptions-validator.log` at 2026-06-19 21:20 Asia/Shanghai.
- Windows build passed in `outputs/unity-role-descriptions-build.log` at 2026-06-19 21:21 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_role_descriptions_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After Role Descriptions / Match Flow Pass 20

- Role text and ability bars are implemented, but the four roles still need hands-on balance playtests across all modes and courts.
- `FireChampionGame.cs` remains the largest script and still owns IMGUI screens, procedural rendering, and broad match orchestration.
- Direct IP remains an alpha feature and still needs a real two-machine LAN test.
- Visual identity is clearer than the original line-only prototype, but full authored 2D spritesheets, VFX, fonts, and sound effects are still future work.

## Implemented In AI Simulation Pass 21

- Added `Assets/Scripts/FireChampionAiSimulation.cs` for deterministic scripted AI decision coverage.
- The simulation covers CORE/DASH/HEAVY/TRICK across all three current courts.
- Each role/court pair runs home recovery, reachable incoming shot, high threat, skill-ready idle, and learned-habit adaptation scenarios.
- The project validator now fails if the scripted AI pass does not produce expected movement, swing, jump, skill, skill-hold, adaptation-banner, or target-clamp behavior.
- Unity validation passed in `outputs/unity-ai-simulation-validator.log` at 2026-06-19 21:30 Asia/Shanghai.
- The validator logged: `characters=4, courts=3, scenarios=60, movement=60, swing=24, jump=12, skill=12, skillHold=21, adaptation=12, clampViolations=0`.
- Windows build passed in `outputs/unity-ai-simulation-build.log` at 2026-06-19 21:31 Asia/Shanghai.
- Hidden EXE smoke test passed in `outputs/FireChampion_ai_simulation_smoke.log`; startup reached engine initialization with no searched runtime errors or exceptions.

## Remaining Risks After AI Simulation Pass 21

- The AI simulation checks decision-level behavior, not complete rally physics or scoring outcomes.
- AI still needs hands-on balance tuning for perceived pressure, fairness, and role matchup feel.
- `FireChampionGame.cs` remains the largest script and still owns IMGUI screens, procedural rendering, and broad match orchestration.
- Direct IP remains an alpha feature and still needs a real two-machine LAN test.

## Implemented In Layout Source Pass 22

- Added `Assets/Scripts/FireChampionLayout.cs` for main-menu layout calculation and static layout auditing.
- `DrawMainMenu` now uses the calculated main panel/content rectangles instead of duplicating fixed menu panel dimensions.
- `DrawInfoCard` now uses the calculated info panel/content rectangles.
- Narrow windows hide the right-side role information card rather than letting it overlap the main menu.
- `FireChampionProjectValidator` now includes a layout audit hook for 960x540, 1024x576, 1280x720, 1366x768, and 1920x1080.
- Static layout calculation found no panel overlap in those target sizes; details are in `docs/LayoutStaticAudit-20260619.md`.

## Remaining Risks After Layout Source Pass 22

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.
- IMGUI screens beyond the main menu still need a broader visual QA pass.
- `FireChampionGame.cs` remains the largest script and still owns most IMGUI screens, procedural rendering, and broad match orchestration.

## Implemented In Audio Data Source Pass 23

- Added `gameplay.audio` to `Assets/Resources/FireChampion/Data/fire_champion_balance.json`.
- Added `BalanceAudioTuning` and `BalanceToneCue` to `FireChampionBalanceConfig.cs`.
- Serve, normal hit, smash hit, small skill, ultimate, left-score, and right-score synthesized tone cues are now data-driven.
- `FireChampionGame.cs` now calls named audio cue data instead of hard-coded `PlayTone(frequency, duration)` values.
- `FireChampionProjectValidator` now includes audio cue sanity checks for positive frequency/duration, non-negative volume multiplier, and a brighter smash-hit cue than normal-hit cue.
- Static JSON parsing passed, confirming `gameplay.audio` exists and smash-hit frequency is higher than normal-hit frequency.

## Remaining Risks After Audio Data Source Pass 23

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.
- The game still uses synthesized tones rather than authored sound assets.
- `ToneAudioPlayer` still has low-level sample-rate and waveform implementation constants in code; authored audio import would likely replace this path later.

## Implemented In Feedback Data Source Pass 24

- Added `gameplay.feedback` to `Assets/Resources/FireChampion/Data/fire_champion_balance.json`.
- Added `BalanceFeedbackTuning` to `FireChampionBalanceConfig.cs`.
- Banner duration, screen-shake fade duration, smash-hit shake, score shake, and smash-score shake values are now data-driven.
- `FireChampionGame.cs` now reads feedback values from `gameplay.feedback` instead of hard-coded literals.
- `FireChampionProjectValidator` now includes feedback sanity checks for positive durations, non-negative magnitudes, and smash-score shake being at least normal score shake.
- Static JSON parsing passed, confirming `gameplay.feedback` exists and current values preserve the previous behavior.

## Remaining Risks After Feedback Data Source Pass 24

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.
- Broader visual effects, authored VFX sprites, and full visual QA are still future work.

## Implemented In Role Ability Breakdown Source Pass 25

- Added `abilityBreakdownText` to `CharacterConfig`, `BalanceCharacter`, the default character configs, and `fire_champion_balance.json`.
- CORE, DASH, HEAVY, and TRICK now each explain how their numbers affect actual play: speed, jump, sweet spot, control, smash power, and recovery tradeoffs.
- The selected P1/P2 role summary can show the ability breakdown on taller windows, while the right-side role info card always includes it.
- `FireChampionProjectValidator` now checks that all character explanation fields are present and that the key role identities match the actual numeric balance: CORE has the largest sweet spot, DASH is fastest, HEAVY has strongest smash and lowest speed, and TRICK has highest control and jump.
- `Assets/Resources/FireChampion/Data/README.md` now documents the role-description fields.

## Remaining Risks After Role Ability Breakdown Source Pass 25

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.
- Role text now better explains the intended differences, but hands-on balance playtests are still required.

## Implemented In Asset Source / Art Brief Source Pass 26

- Added `Assets/Resources/FireChampion/ASSET_SOURCES.md` to record current art/audio source status, license policy, planned resource folders, and future import rules.
- Updated `Assets/Resources/FireChampion/UI/README.md` to point to the project-level asset source manifest.
- Added `docs/ArtDirectionAndAssetBriefs-20260619.md` with approval-gated briefs for transparent VFX sprites, full court background plates, role reference sprites, and authored audio direction.
- Updated `FireChampionProjectValidator` so future Unity validation checks that the project-level asset source manifest exists.
- Updated README/current audit notes so asset provenance and future generated/imported resources are easier to track.

## Remaining Risks After Asset Source / Art Brief Source Pass 26

- This pass is source-level planning and validation setup only. It does not add new runtime PNG/audio assets yet.
- Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- Per the 2D asset workflow, the VFX/court/character/audio briefs still need user approval before generating or importing assets.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.

## Implemented In AI Rally Health Source Pass 27

- Expanded `FireChampionAiSimulationReport` with continuous rally health metrics: rally scenario count, frame count, total contacts, AI contacts, scripted feeder contacts, misses, longest exchange, target-clamp violations, out-of-bounds resets, and invalid physics frames.
- Added a lightweight rally-health path to `FireChampionAiSimulation` that runs one continuous feeder-vs-AI sequence per role/court pair while reusing `FireChampionAiController.BuildInput`.
- The rally-health path steps AI movement, jump physics, shuttle physics, spin decay, scripted feeder returns, and AI returns without changing runtime match behavior.
- `FireChampionProjectValidator` now has future Unity checks for rally scenario coverage, frame count, AI contacts, feeder contacts, multi-contact exchange length, target clamping, and invalid physics frames.

## Remaining Risks After AI Rally Health Source Pass 27

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- The rally-health check is a lightweight deterministic guard, not a replacement for manual feel testing or complete match simulation.
- Thresholds may need tuning after Unity validation runs with the real engine.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.

## Implemented In Network RTT Diagnostics Source Pass 28

- Added `Ping` and `Pong` packet kinds to `NetworkPacketKind` for direct-IP diagnostics.
- `DirectIpSession` now sends compatible `P|<id>|<ticks>` ping packets and answers with `O|<id>|<ticks>` pong packets without changing the existing `I|` input or `S|` snapshot packet formats.
- `NetworkDiagnostics` now records sent pings, received pongs, last RTT, best RTT, and worst RTT, and displays them in the existing network diagnostic summary.
- The in-match network diagnostics panel and waiting-for-connection panel are slightly taller so the new RTT line has room.
- `pingIntervalSeconds` and `pingRetrySeconds` now live in `fire_champion_balance.json` under `network`, with defaults in `BalanceNetwork` and validator checks for sane values.

## Remaining Risks After Network RTT Diagnostics Source Pass 28

- This pass is source-level only. Unity batchmode validation/build/smoke could not be run because the current command path is blocked by account usage limits until 2026-06-25 09:56.
- RTT display still needs a real host/client LAN test to confirm values update visibly and do not clutter the UI.
- The direct-IP mode still lacks interpolation, reconnect flow, NAT traversal, relay, and room code support.
- The latest verified Windows player remains the AI Simulation Pass 21 build in `outputs/LATEST_BUILD.txt`.
