# Verification — 6 October 2026

## Passed

- Unity 6000.3.25f1 imported the project and compiled the final C# scripts with zero errors and zero compiler warnings.
- **22/22 headless simulation tests** passed with .NET 10. The same **22/22 tests passed in Unity EditMode**, after the final collision optimization, in 3.92 seconds total on this machine.
- Tests cover straight routes; horizontal/vertical detours; swept diagonal clearance; global connectivity changing with radius; long zig-zags; complete blockage; cached-field invalidation on sale/destruction; siege and resumption; congestion; 208 simultaneous enemies; flight checkpoints; footprint overlap; repeatable fixed-step state; downstream ground checkpoints; live build/sell; combat accounting; field reuse; and no high-speed tunnelling.
- The Unity CLI confirmed changing Time.frameCount during Play. This was an advancing game, not a frozen screenshot.
- Live test: a 25-enemy wave faced a complete wall of 20 towers with tower weapons disabled. Enemies destroyed one tower; all 25 reached the exit. Final state: 19 towers, 0 active enemies, 25 leaks, 0 blocked enemies.
- Live zig-zag test: enemies followed the detour, and closing a downstream gap during the wave changed all 24 active enemies to blocked state. Siege target selection was visible in state and diagnostic lines.
- The final live game console had zero errors and zero warnings before player builds.
- The Game view was captured and visually inspected. A discovered sidebar overlap at a 691×328 viewport was fixed with responsive HUD scaling and a dedicated camera viewport; the corrected capture is `Preview.png`.
- **Linux Mono build succeeded**, 91,922,647 bytes, zero build errors.
- **Windows Mono build succeeded**, 95,665,199 bytes, zero build errors.

## Build warnings

Linux reported one warning: the optional Unity Pipeline automation server is disabled in Player builds because no runtime configuration is installed. This is intentional; the game does not depend on that server.

Windows reported the same warning plus 18 Unity shader warnings for unavailable ray-tracing shader compilation while cross-building from Linux. The prototype uses ordinary URP raster rendering and no ray-tracing features. These are preserved in `Build-Results.json`; a successful build is not a substitute for testing on a Windows machine.

## Standalone runtime limitations

The Linux player was launched for a native smoke test. The host desktop reported **0×0 resolution and no active outputs** through xrandr. Default XWayland startup failed in XF86VidModeGetModeLine; native Wayland reported no displays. A display-free `-batchmode -nographics` attempt then crashed in Unity's native engine startup before game initialization. No successful standalone runtime check is claimed.

Retest the Linux executable with an active display. Windows was cross-built but cannot be natively playtested on this Ubuntu host. Editor Play mode and rendering-independent simulation tests are the verified gameplay paths for this delivery.

## Developer behavior

Unity script recompilation during Play discards the non-serialized simulation. The prototype stops updating with a one-time warning instead of repeatedly throwing; stop and re-enter Play to restart. Saved map parameters are unaffected. No hot-reload persistence is promised.

## Evidence

- `Unity-Test-Results.json`: final Unity test cases and durations.
- `Headless-Test-Results.txt`: final .NET runner output.
- `Build-Results.json`: concise platform build outcomes and warning messages.
- `Preview.png`: corrected live Unity Game view.
- `ACCEPTANCE.md`: remaining interactive controls and native-platform acceptance checklist.

## Follow-up: apparent tower clipping — 7 October 2026

Reproduced the four-tower user layout at cells (3,9), (3,10), (3,11), (3,13). An isolated ground-unit replay crossed x=3.5 through the gap at y=12.18. Independent point-to-rectangle measurements found a minimum surface clearance of 0.009999469 world units and no penetration in that replay. Rendered X/Z tower bounds matched authoritative footprints.

The F navigation view now lowers tower meshes, outlines their collision rectangles, and draws ground-unit radius rings every frame. This makes gaps behind tall tower meshes visible without changing collision or routing. Tower views also refresh position from simulation every frame and stay grounded as their visual height changes. Unity compiled with zero errors/warnings; the updated paused-wave display was visually inspected with no runtime exceptions. The four-tower layout was restored in the editor. Existing standalone builds predate this presentation-only update.


## 2026-10-07 — shared-defense expansion

- Default map: Frostline Crossing, 42×24, three connected defense areas, four ground checkpoints and ten waves (5/10 flying).
- Implemented pure-simulation builder commands/movement, construction costs/refunds, rewards, lives, victory and defeat. Original Maze Lab retained through a runtime map switch.
- .NET simulation suite: **29/29 passed**. Unity suite: **30/30 passed**, including entering Play mode, building a tower, checking drone/tower visuals and switching both directions between maps. Unity test duration: 8.71 seconds.
- Unity compilation: zero errors and zero warnings. Live console: zero errors during the gameplay checks.
- Live Unity reference match: **victory, 255 kills, 0 leaks, 30 lives, 810 gold, 15 purchased towers, 8,344 fixed ticks**. All construction used actual builder orders; no money injection. See Shared-Defense-Live-Result.json and the documented layout.
- Rendered victory view inspected. Fixed overexposed ground colors and faint map labels. Captured shared-defense preview is stored as Shared-Defense-Preview.png.
- Automated switching exposed a scene bootstrap bug (RuntimeInitializeOnLoadMethod only initialized the first scene). Fixed by registering a scene-loaded callback and protected with the Play-mode integration regression.
- Native Linux window smoke testing remains unavailable: the active Xwayland display reports current 0×0 with no active outputs. This does not invalidate the actual Unity editor Play-mode runs, but no successful standalone window run is claimed. Windows is cross-built, not executed on this Linux host.
- Economy and difficulty are provisional. The reference layout clears all ten waves without additional spending; this establishes a functional baseline rather than final balance.
- Updated Linux build: **Succeeded**, 91,934,355 bytes, 0 errors, 1 warning (optional editor automation intentionally disabled in the player).
- Updated Windows build: **Succeeded**, 95,676,907 bytes, 0 errors, 19 warnings (the same automation warning plus unsupported compilation of unused Unity ray-tracing shaders on this Linux host). This prototype uses URP raster rendering.
