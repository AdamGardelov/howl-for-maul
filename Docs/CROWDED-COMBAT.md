# Crowded combat presentation

Shots wholly outside the current camera no longer consume the 64-effect budget. Conservative frustum bounds preserve beams crossing the viewport and splash rings reaching its edge even when their endpoints/centres are outside. Culled events still advance the serial cursor: moving the camera cannot replay an old volley. Audio selection remains separate, and leak/destruction priority and pause/reset behavior remain unchanged. Projectile lines and splash rings do not cast or receive shadows.

Stable tower/enemy display updates reuse their live-ID and stale-ID collections. Removals are collected before mutating the view dictionaries. Builder materials update only when the displayed faction changes, avoiding renderer-array and name retrieval on every frame. These changes do not alter simulation, source masks, build placement, paid economy, faction stats, or navigation.

Unity compilation, all 64 pure simulation cases and all six `CrowdedPresentation` integration cases passed. Individual Unity XML case results are saved in Howl-Crowded-Unity-Tests.json. This is a focused run, not a fresh full-suite pass.

- 100 wholly offscreen shots followed by one visible shot create exactly one effect, aimed at the visible target. Moving the camera does not replay the hidden volley.
- Crossing beams and splash circles reaching the viewport survive conservative culling. Paused effects stay alive, visible volleys remain capped at 64, restart clears old effects, and no scenic/gameplay colliders are added.
- Two paid towers and two enemy views, warmed for 20 direct synchronization calls, allocate **0 managed bytes across 100 stable synchronization calls**, measured with `GC.GetAllocatedBytesForCurrentThread` on the Unity main thread.
- Selling one tower and killing one enemy remove only their matching views. Restart clears all views, reused IDs create new views correctly, and a new faction refreshes the reused builder's materials.
- Existing tests for camera-local audio cadence, global leak sound/priority, defeat/leak lifetime/budget, tower recoil, and paid wall siege/destruction all pass.

The fixtures exercise presentation and use synthetic volleys for the budget stress case; they are not new paid campaign wins. Gameplay and balance evidence remains in Balance/HARD-FOUR-PLAYER.md. Packages are recorded separately in Howl-Builds.json.

A live before/after frame-time profile was attempted first, but repeated main-thread eval timeouts during Play made it unsuitable for a matched comparison. No frame-time result is inferred from those attempts. The focused allocation regression measures only direct, warmed stable view synchronization; it does not imply the entire game or editor is allocation-free.
