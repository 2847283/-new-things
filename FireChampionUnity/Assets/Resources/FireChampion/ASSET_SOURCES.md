# Fire Champion Asset Sources

This file records the source, license status, and runtime use of art/audio resources that ship with the Unity project.

## Current Included Assets

| Folder | Files | Source | License / Use | Runtime use |
| --- | --- | --- | --- | --- |
| `Assets/Resources/FireChampion/UI` | `logo_fire_champion.png`, `role_core.png`, `role_dash.png`, `role_heavy.png`, `role_trick.png`, `court_dojo.png`, `court_rooftop.png`, `court_future.png` | Original procedural PNG drawings generated for this project on 2026-06-18. Not downloaded from the web and not copied from third-party art. | Project-owned original prototype art. Cosmetic/UI use only. | Loaded by `FireChampionUiAssets` through `Resources.Load<Texture2D>("FireChampion/UI/<name>")`; court badges are used in menu previews and as subtle in-match backdrop theme art. |
| `Assets/Resources/FireChampion/Courts` | `court_dojo_background.png`, `court_rooftop_background.png`, `court_future_background.png` | Original deterministic 1280x720 PNG drawings generated for this project on 2026-06-27. Not downloaded from the web and not copied from third-party art. | Project-owned original prototype court art. Visual presentation only. | Loaded by `FireChampionCourtAssets` through `Resources.Load<Texture2D>("FireChampion/Courts/<name>")`; rendered as full-scene match backgrounds with procedural fallback. |
| `Assets/Resources/FireChampion/VFX` | `vfx_hit_sweet.png`, `vfx_hit_solid.png`, `vfx_smash_trail.png`, `vfx_dash_burst.png`, `vfx_trick_spin.png`, `vfx_score_burst.png` | Original transparent PNG drawings generated for this project on 2026-06-27 with deterministic local drawing code. Not downloaded from the web, not copied from third-party art, and not produced from a copyrighted reference. | Project-owned original prototype VFX art. Visual feedback only. | Loaded by `FireChampionVfxAssets` through `Resources.Load<Texture2D>("FireChampion/VFX/<name>")`; rendered by `FireChampionVfxRenderer` with procedural fallback effects still present. |
| `Assets/Resources/FireChampion/Audio` | `sfx_serve.wav`, `sfx_hit_normal.wav`, `sfx_hit_smash.wav`, `sfx_skill.wav`, `sfx_ultimate.wav`, `sfx_score_left.wav`, `sfx_score_right.wav` | Original generated WAV cues created for this project on 2026-06-27 with deterministic local synthesis. Not downloaded from the web and not copied from third-party samples. | Project-owned original prototype audio. Feedback only. | Loaded by `FireChampionAudioAssets` through `Resources.Load<AudioClip>("FireChampion/Audio/<name>")`; the existing `ToneAudioPlayer` synthesized tone path remains as fallback. |

## Current Runtime Procedural Art

These visuals are drawn by code and do not reference external image assets:

- Player athletes, rackets, limbs, heads, shoes, and role glow in `FireChampionMatchRenderer.DrawPlayer`.
- Shuttle and trail in `FireChampionMatchRenderer.DrawShuttle`.
- Court surface, net, court lines, and procedural fallback backdrops in `FireChampionMatchRenderer`; the backdrop can also layer in the selected court badge PNG when full-scene court art is unavailable.
- Score HUD panels, role labels, network status, and energy bars in `FireChampionHudRenderer`.
- Hit, smash, score, and skill fallback VFX in `FireChampionVfxSystem` / `FireChampionVfxRenderer` through `FireChampionGuiDrawing`; these remain available if the PNG VFX resources fail to load.
- Remaining screen-space menu/overlay panels and input chips in `FireChampionGame` through `FireChampionGuiDrawing`.
- Synthesized fallback tones in `ToneAudioPlayer`; these remain available if the WAV audio resources fail to load.

## Source Policy

- Web assets must only be added when the source page has clear license terms that allow the intended use.
- For every web asset, add the original URL, author, license, download date, and any required attribution to this file before the asset is referenced by code.
- Generated assets must include the generation date, prompt/brief document, output file names, and whether any manual edits were made.
- Cosmetic assets must not alter gameplay stats, collision, speed, energy, hit radius, AI behavior, or scoring rules.
- Keep runtime-loadable assets under `Assets/Resources/FireChampion/<category>` with lowercase snake_case file names.

## Planned Asset Folders

| Folder | Planned contents | Status |
| --- | --- | --- |
| `Assets/Resources/FireChampion/Characters` | Future role spritesheets or cutout body-part atlases for CORE, DASH, HEAVY, and TRICK. | Not generated yet. Awaiting approved asset brief. |
| `Assets/Resources/FireChampion/Courts` | Full-scene court backgrounds for dojo, rooftop, and future court. | Initial project-owned deterministic PNG pass generated and integrated on 2026-06-27; future polish can replace these files without changing gameplay logic. |
| `Assets/Resources/FireChampion/VFX` | Transparent hit sparks, smash trails, score burst, and skill auras. | Initial project-owned PNG pass generated and integrated on 2026-06-27; future polish can replace these files without changing gameplay logic. |
| `Assets/Resources/FireChampion/Audio` | Serve, hit, smash, score, skill, and ultimate short WAV cues. | Initial project-owned WAV pass generated and integrated on 2026-06-27; synthesized tones remain as fallback. |

## Next Approved-Needed Brief

The next recommended art pass is documented in `docs/ArtDirectionAndAssetBriefs-20260619.md`. It should be approved before image generation or third-party download/import work starts.
