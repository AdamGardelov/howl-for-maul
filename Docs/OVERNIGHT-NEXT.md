# Next verified work

1. Baseline complete: 24 Normal paid-defense campaigns (12 factions × solo/two-player), all wins with 30 lives. See Docs/Balance. A further 12 Hard solo runs also won without leaks. Next compare intentional mazes, upgrades/champions and mixed factions; do not call the game balanced from this limited driver.
2. Pre-purchase tower stats, exact missing champion prerequisites and difficulty-correct wave forecasts are now implemented. Ground crawlers, winged flyers, slow/siege feedback and enemy health bars are now implemented and visually checked on both maps (49 Unity tests pass). Next improve tower role/upgrade silhouettes, especially the eight Ironfold rosters; prototype art is not final.
3. Expand progression deliberately after balance evidence. Historical Mega Man reference has 35 waves and late lumber, while this checkpoint has ten.
4. Preserve source mask geometry, all-active lanes, independent builders, full-blockade siege and local-only scope.
5. Folder rename complete: /home/adam/Documents/Dev/howl-for-maul. Unity Hub points to the new path. The editor was saved and gracefully closed first.

Tools: do not assume an editor is still open; check Unity CLI status before use. Current desktop display is 0×0. The scratch-only Xvfb binary is under the chat work/virtual-display/runtime/usr/bin. Do not assume a display server persists: the latest checks used a temporary :98 server with cleanup. No invisible editor or display is intentionally left running. No system packages were installed. Latest test/build evidence is in Docs. The downloaded historical map and MPQ reader remain in chat scratch space and must never be committed as game assets.

Eight hourly continuation runs were scheduled in this chat at the user's request for overnight work. Do not duplicate that automation. Push verified source checkpoints to the existing SSH origin.

Latest checkpoint: enemy readability. 49/49 Unity tests and 47/47 headless tests; fresh Linux/Windows builds; updated Linux packaged-map smoke passed. Controlled native visual samples are in chat outputs/enemy-readability-rimewatch.png and enemy-readability-ironfold.png. Windows runtime remains untested. The test fixture initially spawned enemies before starting a wave; corrected to use StartWave before the presentation samples. No simulation tuning changed. Virtual display :98 was held in a foreground tool session for reliability; shut it down after closing all Unity sessions.
