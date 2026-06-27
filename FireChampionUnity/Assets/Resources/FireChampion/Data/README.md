# Fire Champion Balance Data

`fire_champion_balance.json` contains the main gameplay values that designers are expected to tune without editing C# code.

Runtime loading keeps a default fallback so the prototype can still start if the file is missing during development, but `FireChampionProjectValidator` now requires the Resources JSON to load successfully and reports the loaded character count/length in Unity logs. This prevents data-file regressions from being hidden by fallback defaults.

- `characters`: role names, descriptions, play-style guidance, ability breakdowns, strengths, weaknesses, recommended player type, core stats, and visual silhouette tuning.
- `rules`: match rules for quick play, tournament, practice, and tutorial.
- `ai`: base AI difficulty plus prediction, movement, swing, tactical drop/lift/smash shot-choice, and skill-use behavior. Tactical fields let AI punish deep positioning with drops and net-hugging positioning with lifts without hardcoding those thresholds in C#.
- `network`: direct-IP default port, lobby slider ranges, and diagnostics ping timing.
- `gameplay.court`: court physics, gravity, shuttle drag, net size, prediction step, and player bounds.
- `gameplay.practice`: free-practice ball-speed and energy slider defaults/ranges, plus recent-shot timeline capacity.
- `gameplay.energy`: energy cap, skill cost, passive gain, hit gain, and long-rally bonus.
- `gameplay.audio`: cue frequency, duration, and volume multiplier for serve, hit, smash, skill, ultimate, and score feedback. WAV assets use the same volume multipliers; synthesized tones remain as fallback if audio clips are missing.
- `gameplay.feedback`: banner duration, hit/score screen-shake magnitude, duration, fade timing, and VFX duration/radius/event-cap values used by both PNG VFX and procedural fallback effects.
- `gameplay.visuals`: procedural 2D player/court proportions, court line opacity, racket size, aura size, and swing trail strength.
- `gameplay.skill`: role skill durations, speed multipliers, cooldowns, and dash/core effects.
- `gameplay.swing`: serve/swing windows, hit-radius bonuses, sweet-spot ratio, and racket reach.
- `gameplay.shot`: serve, high, flat, drop, smash, spin, timing, and end-lag tuning.
- `courts`: court color and wind modifier values.
- `cosmetics`: visual-only outfit color catalog. Entries may include display text, an unlock label, tint color, and `tintBlend`, but must not contain strength or gameplay modifiers.

Character stat notes:

- `descriptionText`: one-line role fantasy shown in the character summary.
- `playStyleText`: practical advice for how the role should be played.
- `differenceText`: short, high-signal ability difference shown in the main menu.
- `abilityBreakdownText`: readable explanation of how the role's numbers change actual play.
- `passiveText`, `skillText`, `ultimateText`: role-specific ability descriptions.

- `moveSpeed`: horizontal movement speed.
- `jumpForce`: jump launch strength.
- `hitRadius`: racket sweet-spot size and forgiveness.
- `smashBonus`: extra smash speed.
- `smashEndLag`: recovery time added after smash.
- `controlBonus`: accuracy and shot-control modifier.
- `visualBodyScale`, `visualLimbScale`, `visualHeadScale`: non-gameplay silhouette differences used by the procedural 2D athlete renderer.
- `jerseyWhiteMix`: how much the character jersey blends toward white before cosmetic tinting.
