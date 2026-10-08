# Blast Circuit models and expanded paid defenses

Blast Circuit now has seven distinct original procedural models: dome cannon, crash hammer, turbine vanes, boiler, winged Quicksilver, clustered Rootguard barrels, and fortress-like Citadel Champion. Tracked chassis distinguish the faction from Pulse Foundry. No gameplay data, footprints, map masks or collision changed.

Fresh Unity compilation was clean; 64/64 tests passed (24.30 seconds). The robot model integration case now purchases all seven Blast designs, checks their geometry and lack of colliders, verifies 505 gold remains, and confirms Quicksilver retains air-only splash. A native 1920×884 paid lineup was inspected at zoom 7, with 695 gold spent. Input was temporarily disabled to hide the hover preview in the paused fixture. This is not a human playthrough or performance benchmark.

Four new twenty-wave paid adaptive campaigns won with all 30 lives, no stalls, and no individual/team wallet errors: Hard solo Rime Covenant and Pulse Foundry, and Normal two-player Rime/Stonebound and Pulse/Blast. Raw ledgers and limitations are in Balance/HARD-AND-MIXED.md. No tuning changed; the previous roster-first failures still stand.

All four winter factions plus Pulse and Blast now have original model sets; six Ironfold factions remain generic. Packaged builds still correspond to complete-winter commit 107e0a7 and exclude both new robot sets. Use Unity Play to view them. Windows runtime remains untested; the earlier native Linux X video-mode launch issue is unresolved.

Next: measure crowded-map rendering at the 224–324 tower counts seen in paid campaigns, then improve any measured bottleneck before adding more visual complexity.
