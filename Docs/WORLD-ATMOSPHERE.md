# Camera, exterior scenery and audio

R resets camera yaw without changing focus or zoom. The minimap has a north/reset button; Home still returns to the builder and End fits the map. The map and minimap stay fixed north-up when the camera rotates.

Four batched exterior meshes continue terrain outside the playable rectangle: snow ridges and pines for Rimewatch; rock and copper outcrops for Ironfold. They have no colliders, do not enter the minimap render layer, and do not alter the source terrain masks or build/navigation bounds. Existing scenery remains inside blocked mask cells. This is another procedural art pass, not final hand-painted art.

Each tower design has its own cached original synthesized cue, combining faction register, weapon role, noise, harmonics and pulse envelope. Weapon events now carry design identity; projectile and splash colors follow faction accents. The existing camera-local voice limit and leak priority are retained. Music and effects have independent volume controls in the menu. Two unchanged Scott Buckley tracks stream from disk: Snowfall for Rimewatch, Signal to Noise for Ironfold. Credit, license links and source hashes are in THIRD-PARTY-NOTICES.md and MUSIC-SOURCES.json, with attribution in the menu. Music continues quietly through a pause/menu.

The both-map Unity atmosphere regression passed: all exterior triangles remain outside the playable rectangle, no colliders/minimap contamination, reset preserves focus/zoom, and all 76 cues have distinct sample hashes, finite bounded amplitude and cached reuse. This verifies synthesis and playback configuration, not a subjective listening review. Separate setup/music and packaged visual/input evidence follows.

The staged solo Unity case also passed: chosen faction/start, 1,200 gold and three active lanes, streamed audio import/playback volume, pause/resume, frozen solo menu and session cleanup. Final packaged graphical and network results follow below.

Visual review rejected the initial plain-blue exterior. The refined version uses the same original snow/slate palette and broad texture wash as the map, with tiered snow-covered pines. Its both-map clearance/reset/76-sound regression passed again (102.28 seconds).

## Packaged source 0ca580c

Linux-Online and Windows-Online builds succeeded with zero errors. Corrected packaged multiplayer probes passed on both maps: two actual Unity processes per map, authenticated lobby/setup, two paid purchases from separate wallets, active waves and ordered state digests, majority pause/resume, departure pause and remaining-player recovery. Both clients and both hosts exited zero. Both-map data/route smoke passed on isolated display :98, exit zero. The earlier failed probe is recorded in ONLINE-PLAY.md and is not counted as a pass.

Actual isolated graphical input verified staged solo faction/start/difficulty, close perspective, Q/E and R restoration, and three Shift-queued Snow Cairns becoming three paid towers with gold 1200 → 1185 and all markers clearing. UI and both map exteriors were captured and inspected. Windows remains build-tested only; all network checks so far are same-machine loopback.

Final client presentation regression passed (120.17 seconds): a real TCP client switches from Rimewatch to the host's Ironfold scene while retaining its session, then adopts slot 1, its chosen faction/start and 600-gold wallet. It does not advance without host frames; its settings menu does not stop shared ticks; majority pause and leave cleanup pass. See Howl-Online-Client-Unity-Test.json.

Final graphical source 0ca580c check also exercised actual Host/Ready/Begin UI, Pulse Foundry selection, Central Spine start and Normal vote on Ironfold. Q followed by the minimap north button restored the view. Both close scenery views were inspected. The game-menu Quit button closed normally with exit zero, and the player log contains no game exceptions. These checks ran on the owned isolated Xvfb display with llvmpipe; the user's existing desktop player was left running.
