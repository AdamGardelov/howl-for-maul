# Pulse Foundry models

The first Ironfold faction now has seven distinct original procedural models: Fuse Cadet's barrel/antenna, Ironhand's gauntlets, Shear Sentinel's blade housing, Arc Ranger's rifle/pack, Flare Keeper's rocket pods, Rime Runner's cryo reservoirs/skates, and Echo Champion's twin cannons/crests. The models share owned mesh resources and the existing shot-driven weapon pivot. No simulation stats, map masks, footprints, income or costs changed.

Fresh Unity verification: 64/64 passed (24.51 seconds), including seven Play-mode integration tests. The new test rejects early champion construction, purchases all six prerequisites and the champion using normal builder travel, verifies each model and lack of colliders, and purchases a champion upgrade. Wallets: 505 after 695 in builds; 245 after the 260 upgrade. Upgraded champion identity and level are checked. Compilation had zero errors/warnings.

Two native 1920×884 captures were inspected at zoom 7 and normal zoom 11. All seven towers were bought in the separate visual fixture for 695 gold. Input was temporarily disabled to hide the placement ghost in paused captures. These are staged model checks, not full human playthroughs or performance benchmarks.

The complete winter packages at commit 107e0a7 remain the latest builds; Pulse is currently source/editor-only. Linux and Windows builds succeeded at that checkpoint; Linux packaged map smoke passed on isolated :98. Windows runtime is untested, and native :0 Linux startup remains affected by the earlier X video-mode failure. These limits are unchanged.

Next: mixed-player and Hard paid-defense exploration is underway. The remaining seven Ironfold factions still use generic role models. All four winter factions and Pulse have bespoke sets, but all art remains procedural prototype work.
