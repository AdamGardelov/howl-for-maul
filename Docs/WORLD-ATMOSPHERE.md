# Camera, exterior scenery and audio

R resets camera yaw without changing focus or zoom. The minimap has a north/reset button; Home still returns to the builder and End fits the map. The map and minimap stay fixed north-up when the camera rotates.

Four batched exterior meshes continue terrain outside the playable rectangle: snow ridges and pines for Rimewatch; rock and copper outcrops for Ironfold. They have no colliders, do not enter the minimap render layer, and do not alter the source terrain masks or build/navigation bounds. Existing scenery remains inside blocked mask cells. This is another procedural art pass, not final hand-painted art.

Each tower design has its own cached original synthesized cue, combining faction register, weapon role, noise, harmonics and pulse envelope. Weapon events now carry design identity; projectile and splash colors follow faction accents. The existing camera-local voice limit and leak priority are retained. Music and effects have independent volume controls in the menu. Two unchanged Scott Buckley tracks stream from disk: Snowfall for Rimewatch, Signal to Noise for Ironfold. Credit, license links and source hashes are in THIRD-PARTY-NOTICES.md and MUSIC-SOURCES.json, with attribution in the menu. Music continues quietly through a pause/menu.

The both-map Unity atmosphere regression passed: all exterior triangles remain outside the playable rectangle, no colliders/minimap contamination, reset preserves focus/zoom, and all 76 cues have distinct sample hashes, finite bounded amplitude and cached reuse. This verifies synthesis and playback configuration, not a subjective listening review. A separate Unity setup/music case and packaged visual/input evidence are recorded once completed.

The staged solo Unity case also passed: chosen faction/start, 1,200 gold and three active lanes, streamed audio import/playback volume, pause/resume, frozen solo menu and session cleanup. Packaged graphical and network verification still pending for this source checkpoint.

Visual review rejected the initial plain-blue exterior. The refined version uses the same original snow/slate palette and broad texture wash as the map, with tiered snow-covered pines. Its both-map clearance/reset/76-sound regression passed again (102.28 seconds).
