# Next verified work

## Latest state

- Selected-start follow-up: twenty Normal paid campaigns cover every start on both maps, all twelve factions, and nondefault two/three/four-player teams. All finish twenty waves, with exact wallets and normal builder travel; Stonebound survives with two lives. Harness now records chosen starts/travel and rejects invalid filters instead of silently completing zero runs. 67 pure cases pass. Read Balance/SELECTED-STARTS.md. Tests/harness/docs only; packages remain runtime 0ca580c.

- Latest runtime source 0ca580c adds direct host/join, shared faction → unique start → difficulty setup, authoritative paid commands and majority pause/resume. The original online deferral is superseded. Separate .NET and packaged Unity process checks pass on both maps; three/four-player tests cover budgets, capacity and votes. R camera reset, textured exterior terrain, original tower sound identities, faction attack colors and licensed map music are implemented. Read ONLINE-PLAY.md / WORLD-ATMOSPHERE.md. New packages: Builds/Linux-Online and Windows-Online. Existing user player and older packages preserved. Internet routing/relay and Windows runtime are untested. Current Unity target restored to Linux. Remote-client scene/session/ownership/host-clock regression also passed; final graphical Host/setup/camera/queue/Quit check exited zero.


- Construction feedback source 945da86: Quit, all queued footprints and legal intermediate checkpoint placement. 67 pure cases, focused queue Unity case and 12 paid Normal campaigns verified. New packages are in Linux-Next / Windows-Next to preserve the user’s existing running player. Both isolated graphical Quit checks and both-map data check pass; no-display bootstrap crashes remain. Continue the expanded camera/scenery/audio/online request in NEXT-PLAYTEST-REQUESTS.md.

- Three-player compact follow-up: five Normal twenty-wave wins spanning all twelve factions, 48 towers per team, 436 total upgrades and 300 wave-end wallet checks. Rime/Stonebound/Ember finishes with nine lives after final-wave leaks; the other four teams keep 30. Read Balance/COMPACT-THREE-PLAYER.md. New paid champion queue recovery covers sold/destroyed prerequisites across all eight robot factions; all 65 pure cases pass. Read CHAMPION-QUEUE-RECOVERY.md for verification. Tests/docs only; player packages remain 110633b.

- Readability/stonework follow-through: overlapping health bars are suppressed, selection stays clear, Alt reveals all bars, and perspective Ctrl-click inspection follows ground/flying models. Existing-tower hover no longer looks like a rejected placement. Corrected terrain texture thresholds restore paving joints, snow edges and wall courses. 64 pure cases and five focused Unity cases pass across the recorded runs. Read READABILITY-AND-STONEWORK.md. Current packages contain 110633b. Both builds, both-map Linux smoke and three isolated Linux input/visual sessions passed: paid purchases/removal, Alt reveal, both-map stonework and live-wave ground Ctrl-click inspection. All normal closes exit zero; Windows is build-tested only.
- Paid investment diagnostic: first compact Rime solo win, 48 towers, 87 upgrades, 14 lives. New strategy wins 11/12 solo factions and both tested mixed pairs; all twelve have a compact win across saved strategies. No game stats changed. Read Balance/COMPACT-INVESTMENT.md.

- Crowded-combat presentation: offscreen shots no longer consume the visible effect budget; crossing beams and edge splashes remain visible. Stable view synchronization reuses buffers and builder tints, with zero managed bytes in the warmed 100-call regression. Compilation, 64 pure cases and six focused Unity cases pass. Read CROWDED-COMBAT.md. The preceding packages contain 3522f2c; both builds, both-map Linux smoke and isolated Linux paid build/select/remove (1200 → 1195 → 1198, exit zero) passed. Windows is build-tested only.

- New perspective inspection camera and themed stonework: smooth lower close view, ground-ray dragging, fit-to-projected-corners overview, fixed terrain and north-up minimap. New paving/cliff materials, inset trim, forge pillars, leaf veins and foundation details. 64 pure tests and three focused depth integration checks passed; final pillar refinement passed the mask-clearance case again. Read DEPTH-AND-TERRAIN.md. The preceding Linux/Windows packages contain 4763c56. Linux both-map smoke and actual close-zoom construction/removal, camera gestures and both-map visual inspection passed; Windows is build-tested only.

- Three four-player Hard paid campaigns now win all twenty waves with 30 lives, covering all twelve factions across both maps. Independent wallets audited throughout; 64/64 pure tests pass including contested/enemy-blocked concurrent queues. Unity compilation passed. Read Balance/HARD-FOUR-PLAYER.md. No runtime change or package rebuild in this checkpoint.
- Camera preference is now resolved by the authorized close-perspective request; see QUESTIONS.md and DEPTH-AND-TERRAIN.md.

- Earlier command-HUD pass: modern stone/brass command HUD with left rendered minimap, right tower portraits plus Remove, and a clear centre except for small contextual controls. Q/E rotates the view; Home resets it. Four camera cases and the both-map rendering/cleanup case pass. Read TOWER-PORTRAIT-GRID.md. Earlier packages contain source 1c112bc; Linux actual input checks and both-map smoke passed, with exact 5-gold wall charge and 3-gold removal refund. Windows is build-tested only.

- Shift-click queue feedback: compact pending count, explicit skipped-order status and new blocked-footprint FIFO/payment regression. All 63 pure simulation cases pass. Read BUILD-QUEUE.md.

- Full-map HUD: permanent sidebar removed from play, compact top/bottom controls, contextual tower inspector, Tab details and Esc paused game menu. Overview reserves space for HUD strips. Four focused checks passed, then the final boundary/overview test passed again. Read FULL-MAP-HUD.md.

- Source 9f228e1 adds original themed dressing: Rimewatch has 58 frost-plant clusters and 9 blue lanterns; Ironfold has 8 copper scrub clusters and 5 warm braziers. Deterministic placement, five batched material groups, no colliders or dynamic lights. Flame brightness freezes on pause. Read THEMED-SCENERY.md.
- The preceding Linux and Windows packages contained 485bb5e. Both builds succeeded with zero errors. Linux both-map data smoke and actual compact-HUD build/upgrade/menu/New Game return/Tab/overview input passed, gold 1,200 → 1,180 → 1,160, normal close exit zero. Windows runtime remains untested. Read Howl-Builds.json for exact per-check provenance.
- Final scenery compilation, expanded both-map prop-clearance test, native overview/close-up inspection and live pause/flicker fixtures passed. Last full Unity suite was 77/77 at the preceding exit/terrain/progression checkpoint; no new full-suite claim for this scenery-only pass.
- Rimewatch exit is centered at (31, 8.5). Wave details give faction-aware suggestions and team targeting counts. Read EXIT-TERRAIN-PROGRESSION.md. All 62 pure cases passed at that checkpoint.
- Earlier paid compact role-scoring matrix won 10/12 factions. The new investment strategy resolves the former compact Rime gap; see the latest evidence above. Rime's larger paid maze clears twenty waves with 30 lives. Compact mixed pairs win on both maps. No balance stats changed to force a bot strategy to pass.
- Camera edge scrolling, Space + left-drag, Enter wave launch, placement hints and owner/refund controls are implemented. Their historical packaged input results retain exact source checkpoints in Howl-Builds.json.

## Next priorities

1. Preserve user Play sessions and unsaved scenes. Use the new Online package directories for current source; do not overwrite the older running player.
2. Continue faction/maze progression and meaningful paid-defense testing; use human sessions to judge beginner difficulty; compact limits are diagnostics only.
3. Continue the original minimalistic art direction while keeping silhouettes readable and every scenic footprint on blocked mask cells. No final-art or performance promise is implied by the procedural pass.

## Verification and environment

Live Unity 6000.3.25f1 is available through the host-authorized CLI at port 7800. Sandbox-only process/network checks hide it. Editor/domain transitions can time out; inspect state and retry only idempotent actions. After a build-target transition, a stale Pipeline request may exist with no active Unity test job. Confirm no active test, Play session, compilation or update before recovering it; never duplicate an active run.

The source folder is /home/adam/Documents/Dev/howl-for-maul and is registered in Hub. No further rename is needed. Restore StandaloneLinux64 after Windows builds. The owned isolated display :98 uses llvmpipe at 1440×900. It has no window manager: keyboard tests must explicitly focus the exact Howl for Maul test window. Never direct automated input to the user's :0 desktop.

Native default X11 startup remains unresolved. Older native Wayland/OpenGL smoke reached map markers but crashed at shutdown (139); Wayland/Vulkan exited zero with a protocol warning and is not a graphical-input pass. The initial audio-package headless smoke exited 133 after its map markers. Explicit data-only smoke now skips presentation/audio and passed three repeated checks. Windows is build-tested only. Read Platform/README.md.

Read README.md, HOWL-VERIFICATION.md and MAUL-RESEARCH.md before work. Preserve supplied map masks, all-active top-to-bottom lanes, 1,200 team gold split over 1–4 wallets, independent faction ownership/builders, paid construction/upgrades and freeform mazing. Flush wall seals and half-cell Ironfold placement remain mandatory. Congestion does not trigger siege; a complete blockage does. Online networking is now authorized; the latest user request supersedes the original deferral. See NEXT-PLAYTEST-REQUESTS.md. No copyrighted assets or new automations. Save nonblocking questions in project docs.
