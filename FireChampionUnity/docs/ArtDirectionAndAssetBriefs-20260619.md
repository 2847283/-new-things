# Fire Champion Art Direction And Asset Briefs - 2026-06-19

## Purpose

The original target asks for a Unity 2D badminton game that should not remain a line-only prototype. The current game has procedural athletes, themed court logic, UI icons and badges, first-pass full-scene court backgrounds, first-pass VFX sprites, and first-pass authored audio cues, but it still lacks authored character spritesheets and final polish-grade art/audio.

This document defines the next art direction and asset briefs. It is intentionally prompt-ready but not generation-approved yet. Per the 2D asset workflow, image generation should start only after the user approves the relevant Asset Brief.

## Current Visual Direction

- Sport: fast side-view 1v1 badminton with arcade readability.
- Mood: energetic, clean, slightly stylized, not gritty.
- Shape language: readable silhouettes, strong role color accents, broad motion trails, clear shuttle visibility.
- Palette:
  - CORE: white/silver, balanced and clean.
  - DASH: cyan/blue, speed and snap.
  - HEAVY: orange/red, impact and weight.
  - TRICK: violet/lavender, spin and misdirection.
  - Dojo: warm wood, paper lantern gold, dark neutral background.
  - Rooftop: cool blue night, city lights, wind accents.
  - Future court: magenta/cyan neon, dark graphite base.
- Avoid:
  - Photorealism.
  - Busy backgrounds that hide the shuttle.
  - Text inside generated art.
  - Extra limbs, unreadable poses, tiny character details.
  - UI assets that imply paid power or gameplay strength.

## Priority

1. Transparent VFX sprites: highest immediate value for feel, lowest gameplay risk. Initial project-owned PNG pass integrated on 2026-06-27.
2. Full court background plates: high visual impact, no gameplay stat risk. Initial project-owned PNG pass integrated on 2026-06-27.
3. Character cutout sprites or body-part atlas: high value, higher integration risk.
4. Authored audio: high feel value. Initial project-owned WAV pass integrated on 2026-06-27; later polish can replace these cues.

## Asset Brief A - VFX Sprite Pack

- Category: 2D game VFX raster assets.
- Subtype: Transparent PNG sprite pack.
- Game context: Side-view 2D arcade badminton game, fast racket impacts and readable shuttle movement.
- Use in game: Replace or augment code-drawn hit/score/skill feedback with authored transparent sprites.
- Camera/view: Screen-facing 2D effects, no perspective scene.
- Style: Stylized sports arcade effects, crisp edges, light bloom, limited palette, no text.
- Canvas/size: 256x256 PNG per effect, transparent background.
- Background: Transparent.
- Required content:
  - `vfx_hit_sweet.png`: compact white/gold starburst for perfect timing.
  - `vfx_hit_solid.png`: smaller blue-white impact spark for normal contact.
  - `vfx_smash_trail.png`: orange/red speed slash for HEAVY-style smash.
  - `vfx_dash_burst.png`: cyan forward burst for DASH skill.
  - `vfx_trick_spin.png`: violet spiral feather arc for TRICK spin.
  - `vfx_score_burst.png`: celebratory but non-text score pop.
- Palette/materials: additive-looking glow, feather streaks, racket impact arcs, no hard black outlines.
- Avoid: text, numbers, logos, character bodies, shuttle duplicates, huge opaque blocks.
- References: Current project colors in `Assets/Resources/FireChampion/Data/fire_champion_balance.json`.
- Output count: 6 PNG files.
- Defaults applied: Recommend transparent PNGs first because they improve feel without changing gameplay data.

## Asset Brief B - Court Background Plates

- Category: 2D environment raster assets.
- Subtype: Side-view court background plates.
- Game context: Three existing courts with gameplay modifiers: dojo, rooftop, future court.
- Use in game: Future replacement or layering behind the existing procedural court lines and net.
- Camera/view: Wide 16:9 side-view background, horizon high enough to keep the playable floor readable.
- Style: Stylized 2D sports venue art, clean silhouettes, moderate detail, strong but controlled lighting.
- Canvas/size: Current integrated pass is 1280x720 PNG per court for the Windows prototype; later polish can replace these with 1920x1080 source plates if needed.
- Background: Opaque full-scene background.
- Required content:
  - `court_dojo_bg.png`: indoor wooden dojo, paper lanterns, warm floor, no readable text.
  - `court_rooftop_bg.png`: night rooftop badminton court, city skyline, wind ribbons, no brand signs.
  - `court_future_bg.png`: neon future arena, cyan/magenta lighting, dark graphite court.
- Palette/materials: match current court accent colors; keep the central shuttle area low-detail.
- Avoid: spectators blocking gameplay, text, logos, photoreal buildings, excessive visual noise.
- References: Existing court badges under `Assets/Resources/FireChampion/UI`.
- Output count: 3 PNG files.
- Defaults applied: Use background plates before full tile/sprite integration to reduce gameplay risk.

## Asset Brief C - Role Character Sprite Direction

- Category: 2D character raster assets.
- Subtype: Future cutout spritesheet or body-part atlas.
- Game context: Four playable role identities: CORE, DASH, HEAVY, TRICK.
- Use in game: Future replacement for procedural athlete drawing while preserving existing collision and movement.
- Camera/view: Side-view readable player silhouettes.
- Style: Stylized 2D sports character, clean body shapes, squash/stretch friendly, no thin stick-only limbs.
- Canvas/size: Recommended first pass is one 512x512 transparent reference pose per role, not a full animation sheet yet.
- Background: Transparent.
- Required content:
  - CORE: balanced stance, white/silver uniform, calm all-rounder.
  - DASH: forward lean, cyan accents, light running shoes, speed silhouette.
  - HEAVY: broad stance, orange accents, heavier racket pose, powerful shoulders.
  - TRICK: angled stance, violet accents, deceptive wrist/racket pose.
- Animation/sheet details: Do not generate full gameplay sheet until one reference pose per role is approved.
- Master/reference plan: Generate and approve one master pose per role before derived animation frames.
- Palette/materials: Role accent colors, simple sport uniforms, no sponsor text.
- Avoid: extra fingers/limbs, unreadable racket, tiny facial details, asymmetrical gear that complicates mirroring.
- References: Role descriptions and ability differences in `fire_champion_balance.json`.
- Output count: 4 reference PNGs first.
- Defaults applied: Reference poses first because full spritesheets need an approved visual identity.

## Asset Brief D - Authored Audio Direction

- Category: Game audio assets.
- Subtype: Short UI/gameplay SFX.
- Game context: Current runtime uses synthesized cached tones; authored audio would improve impact and polish.
- Use in game: Future replacement or layering over `ToneAudioPlayer`.
- Format: WAV or OGG, mono or stereo, 44.1 kHz, short one-shot clips.
- Required content:
  - Serve tap.
  - Normal hit.
  - Sweet-spot hit.
  - Smash hit.
  - Skill activate.
  - Score confirm.
  - UI select.
- Avoid: spoken words, copyrighted samples, long tails, harsh clipping, inconsistent loudness.
- References: Current cue roles and relative brightness under `gameplay.audio` in `fire_champion_balance.json`.
- Output count: 7 audio clips.
- Defaults applied: Keep synthesized tones as fallback until authored clips are imported and volume-tested.

## Approval Gate

Before generating or importing assets:

- Choose one brief to start with. Recommended: Asset Brief A - VFX Sprite Pack.
- Confirm whether to proceed as written or revise style/size/file list.
- After approval, write final image/audio prompts or source-search criteria.
- Save all outputs under the planned `Assets/Resources/FireChampion/<category>` folder.
- Update `Assets/Resources/FireChampion/ASSET_SOURCES.md` with generation prompt/source details before code references the files.

## 2026-06-27 Implementation Note

Asset Brief A now has a first in-project technical-art pass. Six transparent 256x256 PNGs were generated locally under `Assets/Resources/FireChampion/VFX` and loaded through `FireChampionVfxAssets`. The renderer still keeps the procedural VFX as a fallback layer, so missing/replaced PNGs do not change scoring, hit detection, AI, timing, or character stats.

This is not the final polished VFX pack. It is a production-safe bridge from code-only effects to authored raster effects and should be followed by active-rally visual QA and a later style-polish pass if the user wants more painterly or animation-frame-based effects.

Asset Brief D now also has a first in-project generated WAV pass. Seven short cues were created under `Assets/Resources/FireChampion/Audio` and loaded through `FireChampionAudioAssets`. The existing synthesized tone path remains as a fallback, so missing or replaced audio files do not alter gameplay logic.

Asset Brief B now has a first in-project deterministic court-background pass. Three 1280x720 PNGs were created under `Assets/Resources/FireChampion/Courts` and loaded through `FireChampionCourtAssets`. The match renderer draws them as full-scene match backgrounds, then layers gameplay-critical court lines, players, net, shuttle, VFX, and HUD above them. Missing court backgrounds fall back to the existing procedural/backdrop-badge presentation.

## Source-Only Verification Status

This document started as source-only planning work. As of 2026-06-27, Asset Brief A has a first implemented PNG pass, Asset Brief B has a first implemented court-background pass, and Asset Brief D has a first generated WAV pass. The remaining character brief is still source-only until its assets are generated/imported and referenced by code. Unity validation/build is still required after any future asset import or code integration.
