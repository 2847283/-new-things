# Fire Champion Court Backgrounds

This folder contains project-owned 1280x720 PNG court backgrounds used by `FireChampionCourtAssets`.

Runtime load paths:

- `Resources.Load<Texture2D>("FireChampion/Courts/court_dojo_background")`
- `Resources.Load<Texture2D>("FireChampion/Courts/court_rooftop_background")`
- `Resources.Load<Texture2D>("FireChampion/Courts/court_future_background")`

These files are deterministic prototype drawings generated for this project. They are not downloaded from the web and are not copied from third-party art.

The match renderer draws these as full-scene background art, then layers gameplay-critical court lines, players, net, shuttle, VFX, and HUD above them. If a background texture is missing, the renderer falls back to the existing procedural background and subtle court badge layer.
