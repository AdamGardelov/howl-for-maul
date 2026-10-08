# Next verified work

## Latest state

- Full-map HUD: permanent sidebar removed from play, compact top/bottom controls, contextual tower inspector, Tab details and Esc paused game menu. Overview reserves space for HUD strips. Four focused checks passed, then the final boundary/overview test passed again. Read FULL-MAP-HUD.md.

- Source 9f228e1 adds original themed dressing: Rimewatch has 58 frost-plant clusters and 9 blue lanterns; Ironfold has 8 copper scrub clusters and 5 warm braziers. Deterministic placement, five batched material groups, no colliders or dynamic lights. Flame brightness freezes on pause. Read THEMED-SCENERY.md.
- Current Linux and Windows packages contain 485bb5e. Both builds succeeded with zero errors. Linux both-map data smoke and actual compact-HUD build/upgrade/menu/New Game return/Tab/overview input passed, gold 1,200 → 1,180 → 1,160, normal close exit zero. Windows runtime remains untested. Read Howl-Builds.json for exact per-check provenance.
- Final scenery compilation, expanded both-map prop-clearance test, native overview/close-up inspection and live pause/flicker fixtures passed. Last full Unity suite was 77/77 at the preceding exit/terrain/progression checkpoint; no new full-suite claim for this scenery-only pass.
- Rimewatch exit is centered at (31, 8.5). Wave details give faction-aware suggestions and team targeting counts. Read EXIT-TERRAIN-PROGRESSION.md. All 62 pure cases passed at that checkpoint.
- Latest paid compact role-scoring matrix wins 10/12 factions. Alternative Blast strategy wins; compact solo Rime remains unresolved. Rime's larger paid maze clears twenty waves with 30 lives. Compact mixed pairs win on both maps. No balance stats changed to force a bot strategy to pass.
- Camera edge scrolling, Space + left-drag, Enter wave launch, placement hints and owner/refund controls are implemented. Their historical packaged input results retain exact source checkpoints in Howl-Builds.json.

## Next priorities

1. Preserve user Play sessions and unsaved scenes. Current packages are verified through 485bb5e.
2. Continue faction/maze progression and meaningful paid-defense testing; compact Rime is a diagnostic, not a game tower limit.
3. Continue the original minimalistic art direction while keeping silhouettes readable and every scenic footprint on blocked mask cells. No final-art or performance promise is implied by the procedural pass.

## Verification and environment

Live Unity 6000.3.25f1 is available through the host-authorized CLI at port 7800. Sandbox-only process/network checks hide it. Editor/domain transitions can time out; inspect state and retry only idempotent actions. After a build-target transition, a stale Pipeline request may exist with no active Unity test job. Confirm no active test, Play session, compilation or update before recovering it; never duplicate an active run.

The source folder is /home/adam/Documents/Dev/howl-for-maul and is registered in Hub. No further rename is needed. Restore StandaloneLinux64 after Windows builds. The owned isolated display :98 uses llvmpipe at 1440×900. It has no window manager: keyboard tests must explicitly focus the exact Howl for Maul test window. Never direct automated input to the user's :0 desktop.

Native default X11 startup remains unresolved. Older native Wayland/OpenGL smoke reached map markers but crashed at shutdown (139); Wayland/Vulkan exited zero with a protocol warning and is not a graphical-input pass. The initial audio-package headless smoke exited 133 after its map markers. Explicit data-only smoke now skips presentation/audio and passed three repeated checks. Windows is build-tested only. Read Platform/README.md.

Read README.md, HOWL-VERIFICATION.md and MAUL-RESEARCH.md before work. Preserve supplied map masks, all-active top-to-bottom lanes, 1,200 team gold split over 1–4 wallets, independent faction ownership/builders, paid construction/upgrades and freeform mazing. Flush wall seals and half-cell Ironfold placement remain mandatory. Congestion does not trigger siege; a complete blockage does. Online networking remains deferred. No copyrighted assets or new automations. Save nonblocking questions in project docs.
