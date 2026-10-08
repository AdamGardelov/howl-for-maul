# Scenery leaves buildable ground clear

The previous spawn/exit pillars had no colliders but all eighteen pillar centers occupied walkable source cells (eight on Rimewatch, ten on Ironfold). A tower could therefore visually intersect them. The exit arch also crossed a playable corridor above tower height.

Spawn and exit beacons now use the closest solid source-cell center on their respective side, within sixteen world units. Each complete horizontal footprint fits inside one half-unit cell, with a margin. Posts start at the terrain surface. Water/depression cells are excluded. If a future map has no suitable anchor, that marker is omitted rather than covering buildable ground. The overhead exit arch is removed; the labeled exit and paired beacons remain.

Source masks, lane spawns, route/exit coordinates, build rules and collision geometry are unchanged. The new Play-mode regression examines projected triangle bounds against both authoritative source masks, so even an overhead bridge with legal endpoints would fail. It checks trees, rocks, industrial props and the new beacons, plus absence of scenery colliders. Theme-specific decorative batches may be absent.

Verification: full Unity suite 67/67 passed. The containment test was then extended to all existing raised prop batches and independently rerun: 1/1 passed. Compilation had no errors. Exact results are in Howl-Unity-Tests.json and Howl-Scenery-Tests.json. Native 1920×884 overview and exit-close captures on both maps were inspected: lanes and the exit remain readable, with landmarks on blocked ground and no arch across the corridor.
