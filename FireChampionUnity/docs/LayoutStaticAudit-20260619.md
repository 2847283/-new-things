# Fire Champion Layout Static Audit - 2026-06-19

## Scope

This note records the source-level layout pass that was made after the latest verified Windows build.

Changed source:

- `Assets/Scripts/FireChampionLayout.cs`
- `Assets/Scripts/FireChampionLayout.cs.meta`
- `Assets/Scripts/FireChampionGame.cs`
- `Assets/Editor/FireChampionProjectValidator.cs`

## What Changed

- Main-menu layout calculations were moved into `FireChampionLayout`.
- The left main menu panel now uses a calculated content rectangle instead of hard-coded `34/58/380/330` style values throughout `DrawMainMenu`.
- The right role information panel now uses the same layout result.
- On narrow windows, the role information panel is hidden instead of overlapping the main menu.
- `FireChampionProjectValidator` now includes a layout audit hook for common menu resolutions.

## Static Layout Results

The following layout calculations were checked outside Unity:

| Resolution | Main Panel | Info Card | Overlap |
| --- | --- | --- | --- |
| 960x540 | visible | hidden intentionally | no |
| 1024x576 | visible | visible | no |
| 1280x720 | visible | visible | no |
| 1366x768 | visible | visible | no |
| 1920x1080 | visible | visible | no |

Additional static checks:

- `FireChampionLayout.cs`, `FireChampionGame.cs`, and `FireChampionProjectValidator.cs` have balanced braces.
- Source search found no merge-conflict markers in the touched script paths.
- `FireChampionLayout.cs.meta` exists.

## Verification Boundary

Unity batchmode validation/build could not be run for this layout pass because the current Codex/Unity command path is blocked by account usage limits until 2026-06-25 09:56.

Therefore this pass is source-level only. The latest verified Windows player remains the AI Simulation Pass 21 build recorded in `outputs/LATEST_BUILD.txt`.

## Required Follow-Up

When Unity batchmode is available again, run:

```powershell
& 'F:\Unity\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'F:\东西\智能体\项目\ai小玩意\FireChampionUnity' -executeMethod FireChampionProjectValidator.ValidateOrThrow -logFile 'F:\东西\智能体\项目\ai小玩意\outputs\unity-layout-validator.log'
```

Then run the Windows build and hidden EXE smoke test before treating the layout pass as verified.
