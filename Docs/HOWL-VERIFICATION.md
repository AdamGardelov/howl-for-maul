# Howl for Maul verification — planning readouts checkpoint

Unity 6000.3.25f1 passed **48/48 tests**, including actual Play-mode integration. The standalone .NET suite passed **47/47**. Exact Unity results are in Howl-Unity-Tests.json.

Coverage includes collision-mask fidelity, spatial-index equivalence, every map lane, siege and congestion behavior, difficulty, conserved economy, ownership, paid builder queues, upgrades, combat effects, faction restrictions and champion prerequisites. New regression coverage checks that difficulty-scaled wave previews match actual spawns, cannot mutate source data, forecast air waves, and list only the current player's missing prerequisites.

## Player information

Before purchase, tower readouts show damage, interval, direct DPS, range, health, target types and special effects. Locked champions list the exact towers their owner still needs. Selected towers show next-upgrade values. Wave previews share their difficulty-scaled data path with actual spawning and announce the next flying wave. Preparation advice is hidden after a match ends.

Live visual checks covered Hard wave health/siege values, slowing tower stats, and an Ironfold champion list after buying one prerequisite. The purchased tower disappeared from the missing list. Native-resolution captures are saved in the chat outputs. No gameplay balance values changed.

## Campaign evidence

The balance driver completed **36 paid-defense campaigns**: all 12 map-specific factions on Normal with one and two players, plus all 12 on Hard solo. Every run cleared ten waves with 30 lives, no stalls, and exact team-gold conservation. It uses real builder orders and no gameplay overrides. See Balance/README.md and Balance/HARD-BASELINE.md for exact placements, wave results, method and limitations.

These are reproducible starter-heavy winning strategies, not proof of final balance. The bot does not score utility effects, use upgrades, deliberately maze, or mix factions. The Hard multiplier lengthened combat but did not cause leaks. More demanding progression and wave variety need evaluation.

## Desktop builds

Fresh Linux and Windows builds succeeded; Howl-Builds.json records their evidence. The actual updated Linux executable passed its packaged-map smoke test, loading both maps and traversing every ground and flying route before exiting 0. The test used an isolated virtual display because the desktop had no usable screen dimensions.

Linux reports the expected warning that editor automation is disabled in player builds. Windows additionally reports unsupported ray-tracing shader warnings from Unity packages. The game uses standard URP rendering, not ray tracing. Windows execution has not been tested on Windows. A packaged smoke test is not a full human desktop playthrough.

## Project state and remaining work

The project resides at `/home/adam/Documents/Dev/howl-for-maul` and Unity Hub points to it. The rename was done after saving and gracefully closing the editor, and reopening/importing the renamed project succeeded.

Online networking remains deferred; player slots are local controls. Both campaigns remain ten waves. Longer historical progression, research/lumber, armor counterplay and hero systems are not implemented. Art remains original procedural prototype art.
