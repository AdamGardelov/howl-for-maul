# Howl for Maul verification — complete procedural roster

All twelve factions now have original design-specific models: twenty winter towers and fifty-six Ironfold towers. This is a complete first procedural model pass, not finished League-quality art. Read IRON-MODELS.md and WINTER-MODELS.md.

## Automated tests

Fresh Unity 6000.3.25f1 suite: **71/71 passed**, including nine Play-mode integration cases. Compilation had zero errors/warnings. Exact result: Howl-Unity-Tests.json.

Paid presentation coverage buys all eight Ironfold rosters, checks each champion's six prerequisites, air targeting, cosmetic collider absence and champion upgrades. It preserves higher Gravity costs and Scrap's cheap opener. Winter models/upgrades, shared mesh cleanup, animation pause/resume, shot facing/recoil, setup flow and map switching remain covered. Simulation cases cover exact masks, all lanes, fixed team economy, ownership, half-cell paid construction, wall seams, siege/reopening, combat targeting and the captured 61-unit corner jam. The minimap case independently checks all 20,480 source cells, cache reuse and texture cleanup.

An additional focused rerun checks every raised scenic prop against both source masks (1/1 passed). Spawn/exit beacons were moved off walkable ground and the corridor-spanning arch removed. Read SCENERY-CLEARANCE.md.

Wave-result follow-up: 61 pure cases and the 70-case Unity suite passed. Recaps track actual rewards independently of spending, including partial defeats. Hard mixed Gravity/Scrap still matches its complete prior ledger. Native recap/terminal fixtures inspected; see WAVE-RESULTS.md.

Combat cues: real-hit/kill feedback, flight-height leaks, pause/setup/reset and the shared 64-effect budget pass the new integration case. Native staged visual capture inspected. See COMBAT-CUES.md.

## Paid campaigns

New Hard solo Prism adaptive campaign: twenty waves won, 30 lives, 324 purchases, 184 upgrades, 6740 spent and 364 gold left. New Hard mixed Gravity/Scrap campaign: twenty waves won, 30 lives, 324 purchases, 270 upgrades, each player spent 3372 and retained 180. No stalls or wallet errors. See Balance/PRISM-HARD.md and Balance/GRAVITY-SCRAP-HARD.md plus raw ledgers.

Earlier added coverage: four adaptive wins with 30 lives (Hard solo on both maps; Normal mixed two-player teams on both maps), documented in Balance/HARD-AND-MIXED.md. The earlier full Normal matrix remains 36 campaigns, 27 wins and nine defeats, zero stalls/wallet errors; all twelve factions won under adaptive spending. Roster-first strategies still lose. These bots know routes and often buy hundreds of cheap towers. These results do not settle final balance or establish beginner-friendly defenses.

## Visual and input checks

Every faction's lineup has been inspected at gameplay zoom; the six new Ironfold sets also have native 1920×884 close and normal-zoom captures. Paused lineups use normal purchases; they are not human playthroughs. Crowded diagnostic inspection used 324 injected towers and 74 high-health enemies, separately from economy tests. Ordinary health bars now shrink in the overview while the selected enemy stays readable. See Performance/LIVE-COMBAT.md.

An earlier quality package passed actual mouse-driven Start Match, build, select, upgrade and sell on virtual display :98 at 1440×900: gold 1200 → 1180 → 1160 → 1190. This does not verify native :0 desktop launching, Windows runtime or a full mouse-played match. See Performance/QUALITY-PASS.md.

## Performance

Rigid tower batching and permanent minimap caching reduced the matched paused 324-tower editor overview from 6206 to 4405 draw calls and median reported render time from 11.99 to 8.29 ms. With 2× MSAA and medium soft shadows the paused median was 8.33 ms, with slower tails. See Performance/TOWER-BATCHING.md, MINIMAP-CACHE.md and QUALITY-PASS.md.

The separate moving-combat diagnostic reported about 24.87 ms median render time with 324 towers and 74 enemies. That is not comparable to the paused fixture or a standalone FPS benchmark; crowded combat still merits profiling. All measurements are short local editor samples, not promises for other hardware.

Tower-query follow-up: spatial indexing reduced the matched crowded fixture from 14.548 to 2.914 ms per simulation step and median editor frame time from 27.927 to 14.253 ms. Exact geometry is unchanged; 20,670 differential queries and the full 66-case Unity suite passed. See Performance/Tower-Index/README.md for raw samples and limits.

## Packages and platform limits

Fresh Linux and Windows packages include source a56b875: all 76 models, tower collision indexing, repaint-only world overlays map props kept off walkable cells and wave income recaps. Both builds succeeded with zero errors; Linux reported one Pipeline-disabled warning, Windows 19 including unsupported package ray-tracing shaders. Windows runtime remains untested.

Fresh Linux packaged route/data smoke passed both maps without a display server, exit zero. The repeatable Tools/smoke-linux.sh checks both map markers and clean process exit. The preceding 465974e package passed mouse-driven Start Match/build beside wall/select/upgrade/sell on isolated :98, with 1200 → 1180 → 1160 → 1190 gold and normal close exiting zero. These are separate checks, not a full mouse-played campaign or a GPU benchmark.

Native default X11 startup remains unresolved. A native Wayland/OpenGL probe reached the smoke markers but crashed on shutdown (139); not a pass. Read Platform/README.md and Howl-Builds.json for evidence. Editor restored to StandaloneLinux64.

## Preserved constraints

Both supplied terrain masks remain authoritative. Every lane stays active; enemies move top to bottom. Team start is 1200 gold split across one to four wallets. Faction ownership, paid travel/construction/upgrades, flush wall placement, freeform mazing and blockage/siege are preserved. Online networking is deferred. No copyrighted reference assets were imported.

Map-tag follow-up: lane numbers and the exit now use compact dark-backed labels with light text; the exit is gold. Native 1920×884 overviews on both maps inspected. Tags are centered on their markers and clipped away from the sidebar/minimap. Clean compilation; this cosmetic follow-up did not rerun the 65/65 roster suite.

Rendering experiment: GPU instancing reduced draw calls but did not show a clear frame-time win against repeated SRP baselines. Both screenshot pairs were pixel-identical. Temporary changes restored; saved rendering configuration unchanged. See Performance/Instancing/README.md.
