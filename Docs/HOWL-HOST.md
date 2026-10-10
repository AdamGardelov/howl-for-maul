# The Howl host — first creature identity pass

The enemies now belong to the same woodland-and-hearth world as the defenders. Eight shared creature bodies replace the four generic crawler/runner/breaker/drifter variations. Ironfold uses soot-brown hides, weathered bone and ochre wings; Rimewatch uses blue-grey hides, pale stone, snowy back tufts and icicle accents. These are original mesh recipes, not imported reference assets.

| Creature | Recognizable features | Campaign use |
| --- | --- | --- |
| Hearthgnawer | Low hunched scavenger, folded ears, long muzzle and shaggy spine | Ordinary patrols, waves 1–4 and 6–9 |
| Gloamwing | Rounded moth wings, eyespots, antennae and visible veins | Air waves 5, 10 and 15 |
| Hollow Warden | Tall hollow mask, antlers, tattered mantle and crooked arms | Reinforced/veteran waves 11 and 16 |
| Thornrunner | Lean hound, high shoulders, thorn mane and trailing tail | Fast waves 12 and 18 |
| Ashling swarm | Low split carapace, six legs and mandibles | Swarm wave 13 |
| Cairnback | Stone-backed boar, broad snout and tusks | Siege waves 14 and 17 |
| Gatebreaker | Upright ram-headed giant, boulder shoulders and heavy fists | Wave 19 |
| Storm Herald | Long-necked corvid, antler mask, layered wing markings and forked tail | Final air wave 20 |

Wave 20 remains a host of thirty flying enemies. This pass adds a distinctive finale body, not a single boss or new ability. The named forms use the existing campaign wave identities, with role fallbacks for diagnostic/custom waves. Details and enemy inspection show the creature name.

## Motion and footprint

Ground creatures have alternating gaits, runners a faster stride, swarm creatures a quick scuttle, and flyers articulated wings. Actual hits and melee strikes produce recoil/twist with warm eye feedback; slow status retains its cyan ring. All motion follows simulation ticks, including pause, rather than a separate wall clock.

Ground models are fitted to the existing collision disc using a conservative bound that includes walking and attack/recoil angles; separate vertical scaling gives brutes and wardens readable height. Tests inspect every rendered body vertex across 72 sampled phases for each ground form on both maps. None of the cosmetics has a collider. Flying bodies remain at the established flight height and have larger aerial silhouettes.

All eight models are prepared once per map. Instances share combined meshes, textures and materials, with at most nine body renderers and an optional slow-status ring. Both stable and moving pose tests allocate zero managed bytes after warm-up. This bounds the presentation work; it is not a hardware FPS guarantee.

## What was checked

- 81/81 targeted Unity cases, including every simulation case, both full historical Hard two-player campaign replays, live-wave paid construction, and enemy hit/defeat/leak/siege/pause/reset behavior.
- 75/75 pure simulation cases. Campaign ledgers retain the same paid construction, wallet totals, builder travel, upgrades, kills, leaks and results. The captured late-game scenes contain 339 Rimewatch towers / 32 enemies and 336 Ironfold towers / 45 enemies, with zero stable view-sync allocation.
- Linux full build and packaged data/menu checks. An explicit enemy diagnostic places sixteen actual campaign enemies around four paid defenses, advances ninety real combat ticks, and captures both maps at normal/close zoom and 960×600. This intentionally mixed host is an art/combat fixture, not a naturally occurring wave.
- Two-process LAN loopback checks on both maps. No network protocol or gameplay source was changed.
- Windows cross-build succeeded. Native Windows, hardware GPU performance and a match between friends on separate networks remain unverified in this pass.

The first focused run found two stale combat fixtures: a six-tower seal no longer closed the current eight-cell-wide Rimewatch exit, and an implicit default-map spawn fell on blocked Ironfold terrain. The fixtures now use the full paid eight-tower seal and an explicitly selected Rimewatch hit-cue site. Their gameplay assertions remain intact. The next focused run and final combined suite passed.

One data-only package check completed its assertions and then crashed in Unity native audio teardown while quitting inside its startup callback. An unchanged retry and normal game sessions exited cleanly. The diagnostic now schedules its exit after two normal player frames; three fresh final data checks exit with code 0. The original failure log is retained. This is a diagnostic shutdown hardening, not evidence that every possible engine shutdown fault is resolved.

[Exact evidence, source/package hashes and screenshots](Verification/Howl-Host/Verification.json). [Ironfold gallery](Verification/Howl-Host/Roster-Ironfold-Roster.png) and [Rimewatch gallery](Verification/Howl-Host/Roster-Rimewatch-Roster.png) are magnified staged renders; [packaged Ironfold combat](Verification/Howl-Host/Ironfold-Paid-Combat.png) and [Rimewatch combat](Verification/Howl-Host/Rimewatch-Paid-Combat.png) show the playing camera.

## Limits and next art work

This is the first enemy identity pass, using shared procedural anatomy and simple joint animation. It is not final sculpted/rigged character art. Wide overview zoom still prioritizes path and wave readability over face detail. Species-specific vocalizations, stronger death animation and human recognition testing remain useful next work. Existing attack/impact/defeat/leak sounds and the credited Scott Buckley soundtrack remain unchanged.

Local Linux-World and Windows-World builds contain this pass; prior Playtest 3 packages are preserved as `*-World-61996b0`. Published Playtest 3 downloads have not been replaced. Use one matching package version for everyone in an online session.
