# Ember Assembly model checkpoint

Five original procedural models now distinguish Ember Assembly: a twin-nozzle Cinder Watch, braced Coal Bastion with banked coals, twin-chimney Furnace Mouth, tall Flare Lance, and open Meteor Crucible with a suspended core. The two artillery designs have separate silhouettes. Shared meshes, shot-facing/recoil and upgrade markers remain in use.

No simulation, footprint, map-mask, economy or targeting changes. All geometry is cosmetic; primitive colliders are disabled immediately and destroyed.

Verification: clean Unity compilation, zero errors/warnings; 63/63 Unity tests passed, duration 24.21 seconds. The extended integration case purchases all five designs, verifies their distinct parts and lack of colliders, buys an upgrade for each, and verifies model identity and level 2. The final wallet is 710: 245 spent on towers and 245 on their upgrades. Existing wall-seam, half-cell, movement and combat regressions passed in the full suite.

A native 1920×884 Game-view capture at zoom 7 was visually inspected. The five unupgraded towers in that separate fixture cost 245, leaving 955 gold. Input was temporarily disabled to hide the cursor ghost during the paused capture. Normal setup was restored afterward. This is a staged visual check, not a full human playthrough or performance test.

Packaged builds were not refreshed for this presentation-only update. Builds and Howl-Builds.json still describe the Rime checkpoint f83c435; use Unity Play to see Stonebound and Ember. Windows runtime remains untested and the earlier native Linux X video-mode startup limitation remains unresolved. Balance campaigns were not rerun because no simulation data changed.

Rime Covenant, Stonebound and Ember Assembly now have bespoke models. Volt Vanguard and all eight Ironfold factions retain the earlier role models. Art remains procedural prototype work, short of the finished stylized target.
