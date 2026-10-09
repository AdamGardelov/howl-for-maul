# Large paid defenses in the Unity renderer

Current replay: see WORLD-COHESION.md and Howl-Cohesion-LargeScenes.json. The test now uses COHESION-HARD-CAMPAIGNS.json, with current 120/120 and 1,100/1,100 wallets, guards against stale rewards, and renders 311 / 336 paid towers. The measurements below are retained as the earlier historical checkpoint.

This diagnostic replays two actual Hard two-player campaigns from Balance/HARD-PAIRS-ADAPTIVE.json: Rime Covenant / Stonebound on Rimewatch and Prism Division / Horizon Guild on Ironfold. It reproduces purchases, normal builder travel, upgrades and every wave through the live Unity simulation. Wallets, tower IDs, paid costs, upgrade levels, kill counts and leak counts must agree with the saved .NET campaign; the test also checks the final win/lives and clean tower/enemy view removal on restart.

No money, tower models or completed campaign state is injected. The existing fixed 1,200 team budget, selected starts 7/0, all active lanes and faction rosters remain in force. For the captures the replay pauses on tick 450 of wave 20, after allowing the real views to synchronize. Captures render terrain and units only; the offscreen camera does not draw the IMGUI command HUD.

The test warms view synchronization with 20 calls, then measures 100 stable calls and their managed allocations. It also warms an offscreen 1280×720 render with three requests and records the median and maximum duration of nine render-request calls. These are editor CPU-side call timings on the recorded device, not complete game-frame timings, GPU timings, sustained FPS, load-time benchmarks or results for another platform. Readback/PNG encoding is excluded from the render-request timing. Combat simulation is paused during the measurement, so the timings do not include the fixed-step combat loop, new view creation or HUD drawing.

The active tower-renderer count includes enabled renderer components under active tower objects; it is not a measured draw-call count. The unique mesh count includes shared source and combined model meshes; it is not a triangle count. Existing per-design mesh sharing is exercised, and every rendered tower must match its simulation upgrade level and have no collision components.

Two harness issues were encountered before the completed run: loading the ledger before EnterPlayMode left a null reference after Unity's domain reload; the read now happens afterward. The next run reached the Rimewatch capture but exceeded Unity's default 180-second test timeout, which included around 100 seconds of Play-mode transition. The long campaign test now declares a 600-second timeout. Neither failed attempt is counted as a gameplay/graphics pass.

## Completed result

Verified against runtime b70de3c and the saved campaigns from source 739c974. The complete Unity test passed in 196.10 seconds, including Play-mode transitions. Both twenty-wave campaigns reproduced their saved purchases, upgrade costs/levels, builder travel ticks, kills, leaks and wallets. Rimewatch finished with 29 lives and Ironfold with 30. Restart removed all tower and enemy views after each campaign; the test restored the Rimewatch scene before leaving Play mode.

Measured on NVIDIA GeForce GTX 1080, OpenGLCore, in the live Linux Unity 6000.3.25f1 editor:

| Capture | Towers | Enemies | Active tower renderers | Unique meshes | View sync mean | Managed allocation / 100 calls | Render-request median / max |
|---|---:|---:|---:|---:|---:|---:|---:|
| Rimewatch | 226 | 44 | 1,534 | 30 | 0.229 ms | 0 bytes | 2.705 / 3.904 ms |
| Ironfold | 347 | 66 | 2,503 | 23 | 0.343 ms | 0 bytes | 4.604 / 4.731 ms |

Both 1280×720 offscreen captures were inspected. The paid towers, upgraded models and flying units appear on their expected maps; the captures intentionally omit the HUD. This provides a real rendered-scene check for the earlier headless campaigns, not a live human input session or sustained combat frame-time benchmark. No production performance optimization was justified solely by these small stable-sync costs; full combat/HUD/frame measurements remain a separate follow-up.

Only editor tests and documentation changed. Existing Linux-Snow / Windows-Snow packages still contain current runtime b70de3c. No new standalone Linux, Windows runtime, internet, audio-listening or full Unity-suite pass is claimed. The 69 pure simulation tests from the previous checkpoint were not rerun because their source and runtime source did not change.

## Reproduce

In the Unity Test Runner, run EditMode category `LargePaidScene`, or:

```sh
unity command run_tests --project-path /home/adam/Documents/Dev/howl-for-maul --format json -- --mode editor --filter PaidCampaignsRenderLateDefensesAndReleaseViews --filter_type testName --async_tests true
```

Use the existing editor, stopped before the run. The test has an explicit 600-second timeout. It writes `Temp/LargePaidScenes/report.json` and one PNG per map; a failed or interrupted run can leave a partial/older report, so only pair the files with a completed passing test result. Saved evidence: Howl-Large-Paid-Scenes.json and Howl-Large-Paid-Scenes-Unity.json. The initial harness failure and timeout are retained separately for audit.

