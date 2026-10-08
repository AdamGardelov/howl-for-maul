# Stonebound model checkpoint

Five original procedural models now distinguish Stonebound from Rime Covenant: Pebble Warden has a squat hopper and launcher body, Basalt Ward has three sealed stone slabs, Quake Idol is a broad-browed monolith, Crag Hurler holds a sky boulder in a raised cradle, and Worldroot has spreading stone roots and a green crown. The two splash towers deliberately have different bodies. Existing shot-facing/recoil, upgrade markers and shared mesh ownership remain in use.

Only presentation and its integration assertions changed. No prices, damage, targeting, footprint sizes, map masks or enemy movement changed. Cosmetic primitive colliders are disabled immediately and destroyed.

Verification: clean Unity compilation; full Unity suite 63/63 passed, duration 24.23 seconds. The expanded Play-mode case buys all five designs through the builder, checks distinct model parts and absence of colliders, and verifies the remaining wallet is 959 (241 spent from 1,200). Existing map-wall and half-cell regressions passed in the same suite.

A native 1920×884 Game-view capture was visually inspected at zoom 7. It shows the paid five-tower lineup and builder, with no wave active. Input was temporarily disabled to hide the hover ghost during the paused capture; normal setup was restored afterward. This is a staged visual check, not a human playthrough or crowded-battle performance test.

No builds or balance campaigns were rerun for this presentation-only checkpoint. Builds/Linux and Builds/Windows and Howl-Builds.json remain the preceding Rime model checkpoint (f83c435). Test the new Stonebound art in Unity Play mode. Windows runtime remains untested; the earlier native Linux X video-mode startup failure remains unresolved.

Remaining visual work: Ember Assembly, Volt Vanguard and the eight Ironfold factions still use the earlier generic role models. All visuals remain early procedural art, below the requested finished stylized target.
