# Next verified work

Latest source milestone: all twelve factions now have original design-specific tower models (20 winter + 56 Ironfold). Fresh Unity suite 65/65 passed. Native close/normal captures inspected for all new sets. Read HOWL-VERIFICATION.md, IRON-MODELS.md and Performance/LIVE-COMBAT.md.

Fresh Linux/Windows packages include source 9e656e8; zero build errors and both Linux map smoke checks passed on :98. Map labels are now high-contrast and inspected on both maps. GPU instancing comparison completed with pixel-identical output but no clear frame-time gain; SRP batching retained. Read Performance/Instancing/README.md. Next: measure simulation, view synchronization, input, HUD and combat effects separately before optimizing crowded play. The first procedural roster is complete, but final art and balance are not. No need to invent additional factions or maps yet.

Hard solo Prism and Hard mixed Gravity/Scrap adaptive campaigns both won twenty waves with 30 lives and valid wallets. Earlier roster-first defeats remain; do not tune from one bot or claim final balance. Bot strategies heavily favor cheap maze towers and have whole-map knowledge.

The crowded diagnostic used 324 injected towers and 74 injected high-health enemies, not normal economy. It exposed overview health-bar clutter, now fixed by zoom scaling. Median live editor render time was about 24.87 ms; paused-map 8.33 ms must not be quoted as live-battle performance. Deeper profiling is still needed.

Live Unity editor is available through host-authorized CLI at port 7800. Sandbox-only process/network checks hide it. Editor transitions often produce initial five-second eval timeouts; wait, inspect state, retry only idempotent actions. The source folder is /home/adam/Documents/Dev/howl-for-maul and is registered in Hub. Restore StandaloneLinux64 after Windows builds. Preserve dirty scenes and user play state. Packages are current through the map-label checkpoint 9e656e8.

The virtual-display Linux Start Match/build/select/upgrade/sell mouse sequence passed on the earlier quality package. Native :0 video-mode startup remains broken; Windows has only been built. Keep these limits explicit. The isolated :98 display may still be running; it belongs to our verification work. Do not touch the user's :0 desktop for input automation.

Preserve supplied masks, all-active lanes, fixed four-player team economy, exclusive faction rosters, classic freeform mazing and exact wall seals. Online networking remains deferred. No copyrighted assets or additional automations. Save nonblocking questions in docs and continue authorized work without asking the user to test routine changes.
