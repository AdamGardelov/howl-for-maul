# Howl for Maul verification — team economy checkpoint

Unity 6000.3.25f1 passed **55/55 tests**, including actual Play-mode integration. The standalone .NET suite passed **51/51**. Exact Unity results are in Howl-Unity-Tests.json.

Coverage includes collision-mask fidelity, spatial-index equivalence, every map lane, siege and congestion behavior, difficulty, conserved economy, ownership, paid builder queues, upgrades, combat effects, faction restrictions and champion prerequisites. New regression coverage checks that difficulty-scaled wave previews match actual spawns, cannot mutate source data, forecast air waves, and list only the current player's missing prerequisites.

## Three- and four-player paid campaigns

The headless driver now audits every individual wallet after each wave and at the end, alongside its team-total check. Three-player coverage, four-player coverage and four-player roster sweeps completed 36 mixed-faction campaigns: 36 wins, 0 defeats, no stalls or accounting failures. They recorded 360 wave-end wallet snapshots, 104 paid upgrades and 28 owned champion purchases. The fixed 1,200-gold team start and all-active lanes are unchanged. See Balance/TEAM-BASELINE.md and raw JSON for exact player order, results and strategy limitations.

This checkpoint changes only the headless verification driver and documentation. The 51-case headless suite passed again. Unity's latest 55/55 suite and desktop build/smoke evidence belong to the preceding match-flow runtime checkpoint and remain applicable; they were not rerun or rebuilt for this driver-only change.

## Returnable setup and clean interaction state

Opening setup during a match now exposes separate Return to match and Start new match buttons. Returning retains the same simulation, paid towers, gold, wave and pause state. Team/faction/difficulty/start choices remain pending until a new match is started. Map changes still load a fresh setup and close the old match; the setup panel explicitly says so. The initial setup cannot return to an unstarted match.

Starting/resetting clears Sell/Move mode, selections and stale hover state. Escape cancels construction and clears those modes/selections too. Setup changes reset the sidebar scroll position, while return/start actions stay outside its scrolling area. Combat feedback now freezes in setup along with the simulation.

The new real Play-mode test uses a paid tower and active wave to check simulation identity, unchanged tick/gold/towers while setup is open, deferred faction changes, and mode/selection cleanup on start/reset/cancel. The effect test additionally checks setup freezing. All 55 Unity tests and 51 headless tests pass. A native 1206×426 visual check verified both fixed footer buttons and the map-change explanation; the capture is in chat outputs/match-setup-return.png. No simulation balance values changed and no campaign reruns were needed for this interaction fix.

## Deliberate paid mazes

The new reference-map regression builds and pays for three alternating wall arms, measures all ground/flying lanes, verifies a real detour without siege, and sells the walls to verify the original path is restored. Weapons are disabled only for this geometry measurement. A separate twelve-faction solo Normal sweep uses paid maze-first construction with normal combat, followed by coverage defenses. See Balance/MAZE-BASELINE.md for exact layouts, complete results and limits.

That earlier paid-maze checkpoint changed only tests, the headless driver and documentation. The current match-flow checkpoint updates presentation code and has fresh desktop build evidence below.

## Flight-aware and pausable combat feedback

Flying splash rings now appear at flight height. Chain events preserve whether their source is a ground enemy or flyer, so mixed-flight chains begin at the struck enemy rather than at a generic tower muzzle height. The cosmetic clock freezes while paused, follows game speed, and can finish fading after the match ends. Expired effects are removed before new ones are allocated, and the 64-object effect budget includes both beams and splash rings.

A new pure simulation case verifies both directions of mixed-flight chain metadata. A real Play-mode case verifies rendered ring/beam heights, survival through a real-time pause, expiry after resuming, the 63-beams-plus-splash budget boundary, and cleanup on a new match. All 53 Unity and 50 headless cases pass.

The native 1206×480 paused Game View was inspected and saved as chat outputs/combat-effects.png. This is a controlled effect fixture anchored to a paid tower, not a campaign playthrough or a claim that the starter tower has splash/chain weapons. No damage, targeting, economy, navigation, map or wave tuning changed. The earlier campaign sweeps were not rerun for this cosmetic change.

## Combat fixes and progression evidence

Blast Circuit's Quicksilver now targets flying enemies and applies splash only to flyers. Its generated definition and shipped Ironfold asset previously had both target flags off. Chain volleys now discard all excess visual events, keeping the shot history at 128 rather than allowing multiple-event volleys to grow it indefinitely. New pure regressions demonstrate the failures before the fixes and pass afterward. Real Play-mode integration verifies the serialized roster targeting as well.

24 additional Normal paid-defense campaigns exercise full rosters, champion unlocks, upgrades and mixed-faction two-player teams: 19 wins, five legitimate defeats, no stalls or accounting failures. They include 24 paid champion purchases and 636 paid upgrades. See Balance/ROSTER-BASELINE.md and raw JSON for methods and exact results. No global balance values changed. Prior 36-run coverage results below are historical and predate this targeting correction.

## Enemy presentation

Ground spheres are now original armored crawlers; air enemies have wider winged silhouettes, simulation-timed flapping and hover. They face their movement direction. Red crests indicate blocked/siege state and cyan markers indicate active slow effects. Damaged or selected enemies show health bars, with overlays excluded from the sidebar and minimap. Cosmetic meshes add no colliders and do not change navigation or balance.

The new real Play-mode regression checks ground/air model selection, movement alignment, slow expiry feedback, simulation-timed pause behavior, absence of cosmetic colliders, flight height and complete cleanup on a new match. The first test run exposed a fixture error (manual spawn before launching a wave); after fixing the fixture, the complete suite passed 49/49.

Native 1206×534 Game View captures were inspected on both Rimewatch and Ironfold. These are controlled presentation samples containing damaged, slowed and blocked enemies, not additional paid-defense campaigns or proof of final art quality. Screenshots are in the chat outputs. This previous presentation-only checkpoint did not change gameplay parameters. Its screenshots are not captures of the new roster campaigns.

## Player information

Before purchase, tower readouts show damage, interval, direct DPS, range, health, target types and special effects. Locked champions list the exact towers their owner still needs. Selected towers show next-upgrade values. Wave previews share their difficulty-scaled data path with actual spawning and announce the next flying wave. Preparation advice is hidden after a match ends.

Live visual checks covered Hard wave health/siege values, slowing tower stats, and an Ironfold champion list after buying one prerequisite. The purchased tower disappeared from the missing list. Native-resolution captures are saved in the chat outputs. No gameplay balance values changed.

## Campaign evidence

The balance driver completed **36 paid-defense campaigns**: all 12 map-specific factions on Normal with one and two players, plus all 12 on Hard solo. Every run cleared ten waves with 30 lives, no stalls, and exact team-gold conservation. It uses real builder orders and no gameplay overrides. See Balance/README.md and Balance/HARD-BASELINE.md for exact placements, wave results, method and limitations.

These earlier runs are reproducible starter-heavy winning strategies, not proof of final balance. The bot does not score utility effects, use upgrades, deliberately maze, or mix factions. The Hard multiplier lengthened combat but did not cause leaks. More demanding progression and wave variety need evaluation.

## Desktop builds

At the preceding match-flow runtime checkpoint, Linux and Windows builds succeeded; Howl-Builds.json records their evidence. The actual updated Linux executable passed its packaged-map smoke test, loading both maps and traversing every ground and flying route before exiting 0. The test used an isolated virtual display because the desktop had no usable screen dimensions.

Linux reports the expected warning that editor automation is disabled in player builds. Windows additionally reports unsupported ray-tracing shader warnings from Unity packages. The game uses standard URP rendering, not ray tracing. Windows execution has not been tested on Windows. A packaged smoke test is not a full human desktop playthrough.

## Project state and remaining work

The project resides at `/home/adam/Documents/Dev/howl-for-maul` and Unity Hub points to it. The rename was done after saving and gracefully closing the editor, and reopening/importing the renamed project succeeded.

Online networking remains deferred; player slots are local controls. Both campaigns remain ten waves. Longer historical progression, research/lumber, armor counterplay and hero systems are not implemented. Art remains original procedural prototype art.
