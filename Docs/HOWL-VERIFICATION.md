# Howl for Maul verification — complete procedural roster

All twelve factions now have original design-specific models: twenty winter towers and fifty-six Ironfold towers. This is a complete first procedural model pass, not finished League-quality art. Read IRON-MODELS.md and WINTER-MODELS.md.

## Automated tests

Historical complete Unity 6000.3.25f1 suite: **74/74 passed**, including twelve Play-mode integration cases, at the siege/audio checkpoint 44b2290. Complete per-case result: Howl-Unity-Tests-Complete.json, extracted from Unity’s NUnit XML. The CLI summary in Howl-Unity-Tests.json agrees, but its detail array retains only 63 entries after domain reloads. All 74 cases are confirmed in XML.

The subsequent camera controls compiled cleanly and passed all four Camera-filtered tests (gesture sampling, UI/setup/bounds, existing tower/camera behavior and camera-local audio). See Howl-Camera-Tests.json and CAMERA-CONTROLS.md. There are now 76 cases available; a complete 76-case run is not claimed. Actual packaged Space-drag, edge scrolling and Enter input passed on isolated :98 with a normal exit.

Paid presentation coverage buys all eight Ironfold rosters, checks each champion's six prerequisites, air targeting, cosmetic collider absence and champion upgrades. It preserves higher Gravity costs and Scrap's cheap opener. Winter models/upgrades, shared mesh cleanup, animation pause/resume, shot facing/recoil, setup flow and map switching remain covered. Simulation cases cover exact masks, all lanes, fixed team economy, ownership, half-cell paid construction, wall seams, siege/reopening, combat targeting and the captured 61-unit corner jam. The minimap case independently checks all 20,480 source cells, cache reuse and texture cleanup.

An additional focused rerun checks every raised scenic prop against both source masks (1/1 passed). Spawn/exit beacons were moved off walkable ground and the corridor-spanning arch removed. Read SCENERY-CLEARANCE.md.

Wave-result follow-up: 61 pure cases and the 70-case Unity suite passed. Recaps track actual rewards independently of spending, including partial defeats. Hard mixed Gravity/Scrap still matches its complete prior ledger. Native recap/terminal fixtures inspected; see WAVE-RESULTS.md.

Combat cues: real-hit/kill feedback, flight-height leaks, pause/setup/reset and the shared 64-effect budget pass the new integration case. Native staged visual capture inspected. See COMBAT-CUES.md.

Off-camera leak alerts: the new focused integration case passes (1/1), bringing the available suite to 72. The subsequent combined suite now passes 72/72. Native sidebar/focus fixtures inspected. A fresh packaged :98 mouse sequence also verified the grouped warning and View exit during natural wave progression, then closed with exit zero. See LEAK-ALERT.md.

Enemy silhouettes: narrower runners and heavier bevelled siege units retain existing collision/navigation. The enemy-presentation case passed (1/1); final cosmetic bevels compiled and were visually inspected afterward, then the combined 72/72 suite passed. See ENEMY-READABILITY.md.

Audio follow-up: the new focused integration case passes (1/1), covering burst limits, camera gating, mute/pause/setup, global breach priority and reset. The subsequent complete suite passed 74/74. Actual synthesized clips exported and checked for finite, bounded samples. See COMBAT-AUDIO.md.

## Paid campaigns

Four-player Hard follow-up: all twelve factions across three mixed teams clear twenty waves with 30 lives. All 240 wave-end player balances pass the independent ledger audit; 886 purchases and 512 upgrades. The new concurrent contested/enemy-blocked build-queue regression passes alongside all 64 pure cases; Unity compilation passed. No runtime changes or new player builds. See Balance/HARD-FOUR-PLAYER.md for raw results, reproduction and limitations.

New Hard solo Prism adaptive campaign: twenty waves won, 30 lives, 324 purchases, 184 upgrades, 6740 spent and 364 gold left. New Hard mixed Gravity/Scrap campaign: twenty waves won, 30 lives, 324 purchases, 270 upgrades, each player spent 3372 and retained 180. No stalls or wallet errors. See Balance/PRISM-HARD.md and Balance/GRAVITY-SCRAP-HARD.md plus raw ledgers.

Earlier added coverage: four adaptive wins with 30 lives (Hard solo on both maps; Normal mixed two-player teams on both maps), documented in Balance/HARD-AND-MIXED.md. The earlier full Normal matrix remains 36 campaigns, 27 wins and nine defeats, zero stalls/wallet errors; all twelve factions won under adaptive spending. Roster-first strategies still lose. These bots know routes and often buy hundreds of cheap towers. These results do not settle final balance or establish beginner-friendly defenses.

Compact-defense follow-up: 10/12 Normal solo wins with a slot-aware bot; splash/slow-aware Stonebound adds an eleventh faction win. Both tested compact two-player mixes win with audited wallets. Solo Rime remains unresolved under the artificial 48-tower diagnostic limit. The previous Hard Gravity/Scrap adaptive ledger still matches exactly. See Balance/COMPACT-DEFENSE.md.

## Visual and input checks

Every faction's lineup has been inspected at gameplay zoom; the six new Ironfold sets also have native 1920×884 close and normal-zoom captures. Paused lineups use normal purchases; they are not human playthroughs. Crowded diagnostic inspection used 324 injected towers and 74 high-health enemies, separately from economy tests. Ordinary health bars now shrink in the overview while the selected enemy stays readable. See Performance/LIVE-COMBAT.md.

An earlier quality package passed actual mouse-driven Start Match, build, select, upgrade and sell on virtual display :98 at 1440×900: gold 1200 → 1180 → 1160 → 1190. This does not verify native :0 desktop launching, Windows runtime or a full mouse-played match. See Performance/QUALITY-PASS.md.

## Performance

Rigid tower batching and permanent minimap caching reduced the matched paused 324-tower editor overview from 6206 to 4405 draw calls and median reported render time from 11.99 to 8.29 ms. With 2× MSAA and medium soft shadows the paused median was 8.33 ms, with slower tails. See Performance/TOWER-BATCHING.md, MINIMAP-CACHE.md and QUALITY-PASS.md.

The separate moving-combat diagnostic reported about 24.87 ms median render time with 324 towers and 74 enemies. That is not comparable to the paused fixture or a standalone FPS benchmark; crowded combat still merits profiling. All measurements are short local editor samples, not promises for other hardware.

Tower-query follow-up: spatial indexing reduced the matched crowded fixture from 14.548 to 2.914 ms per simulation step and median editor frame time from 27.927 to 14.253 ms. Exact geometry is unchanged; 20,670 differential queries and the full 66-case Unity suite passed. See Performance/Tower-Index/README.md for raw samples and limits.

## Packages and platform limits

Fresh Linux and Windows packages include source 1c112bc: all 76 models, tower collision indexing, repaint-only world overlays map props kept off walkable cells wave income recaps bounded combat cues off-camera leak alerts refined enemy silhouettes, paced combat audio, siege feedback faster camera controls, placement hints, owner/refund inspector, centered Rimewatch exit, terrain material washes, wave-planning advice, themed plants/braziers and full-map compact HUD with paused Esc menu. Both builds succeeded with zero errors; Linux reported one Pipeline-disabled warning, Windows 19 including unsupported package ray-tracing shaders. Windows runtime remains untested.

Fresh Linux packaged route/data smoke passed both maps without a display server, exit zero. The repeatable Tools/smoke-linux.sh checks data-only initialization, both map markers and clean process exit. An initial audio-shutdown crash (133) was caught; the explicit smoke path now skips presentation and passed three consecutive runs. See Platform/headless-audio-shutdown.json. The preceding 465974e package passed mouse-driven Start Match/build beside wall/select/upgrade/sell on isolated :98, with 1200 → 1180 → 1160 → 1190 gold and normal close exiting zero. These are separate checks, not a full mouse-played campaign or a GPU benchmark.

Native default X11 startup remains unresolved. A native Wayland/OpenGL probe reached the smoke markers but crashed on shutdown (139); not a pass. Read Platform/README.md and Howl-Builds.json for evidence. Editor restored to StandaloneLinux64.

## Preserved constraints

Both supplied terrain masks remain authoritative. Every lane stays active; enemies move top to bottom. Team start is 1200 gold split across one to four wallets. Faction ownership, paid travel/construction/upgrades, flush wall placement, freeform mazing and blockage/siege are preserved. Online networking is deferred. No copyrighted reference assets were imported.

Map-tag follow-up: lane numbers and the exit now use compact dark-backed labels with light text; the exit is gold. Native 1920×884 overviews on both maps inspected. Tags are centered on their markers and clipped away from the sidebar/minimap. Clean compilation; this cosmetic follow-up did not rerun the 65/65 roster suite.

Rendering experiment: GPU instancing reduced draw calls but did not show a clear frame-time win against repeated SRP baselines. Both screenshot pairs were pixel-identical. Temporary changes restored; saved rendering configuration unchanged. See Performance/Instancing/README.md.

Latest combined regression: 74/74 passed after combat audio and siege feedback, with every individual case confirmed from Unity XML. See SIEGE-FEEDBACK.md. Camera traversal subsequently passed its four focused checks and packaged input sequence.

Placement-hint follow-up: compilation and native staged valid/invalid tooltip inspection passed. It reuses existing placement validation and changes no simulation rules. The subsequent afe6680 Linux package verified the live inspect/checkpoint hints during actual input; no new full-suite pass is claimed for this addition. See PLACEMENT-HINTS.md.

Tower ownership inspector: compilation, all 61 pure cases, and native owner/non-owner captures passed. Actual paid fixture: P1 600 → 560 after tower + upgrade, P2 stays 600, refund 30, next upgrade 40. No mutation of ownership/refund rules. See TOWER-OWNERSHIP-UI.md.

Packaged inspector follow-up (afe6680): actual Linux build/select/U-upgrade/sell sequence passed on isolated :98. Gold 1200 → 1180 → 1160 → 1190 agrees with the quoted 15/30 gold refunds. Normal close exited zero; both map/data smoke checks passed.

Exit/terrain/progression follow-up: 62/62 pure cases and 77/77 Unity cases passed (all individual XML results preserved). Final material falloff correction separately compiled and inspected in both native map views. No scenic colliders or geometry changes. Paid campaign evidence and limits: EXIT-TERRAIN-PROGRESSION.md.

Themed scenery: final compilation, expanded prop-clearance regression on both maps, native overview/close-up inspection and live pause/flicker fixtures passed. No gameplay changes; the prior full 77-case suite is historical. Read THEMED-SCENERY.md.

Full-map HUD follow-up: clean compilation, four focused camera/audio/tower checks and final updated camera/menu/overview assertions passed. Native menu, rosters and paid inspector captures inspected. Read FULL-MAP-HUD.md; previous 77-case suite remains historical.

Packaged compact-HUD verification (485bb5e): both-map data smoke passed; actual isolated Linux build/upgrade, Esc menu, New Game setup/Return preserving the match, Tab details and final overview were captured and inspected. Gold 1200 → 1180 → 1160; normal close exited zero. Fullscreen mode switching and Windows runtime are unverified. See FULL-MAP-HUD.md and Howl-Builds.json.

Build queue follow-up: 63/63 pure simulation cases passed, including the new enemy-blocked middle order, click-order completion, no skipped charge/retry and unreserved wallet exhaustion. Compact HUD exposes the pending count. See BUILD-QUEUE.md.

Command-HUD/rotation/minimap follow-up: final Unity compilation passed. All four camera-filtered cases passed, independently confirmed in XML, including rotation, screen-relative drag, overview bounds and modal/focus safety. The separate rendering integration passed on both maps: all 76 actual-model portraits, non-flat 512×512 scenery snapshots, cache reuse, unchanged gold/towers/ticks and cleanup. Raw results: Howl-Command-Camera-Tests.json and Howl-Command-Render-Tests.json. The prior full suite remains historical. Package verification is recorded separately after the final visual/input check.

Packaged command-HUD verification (1c112bc): both builds succeeded with zero errors; both Linux data-only map checks passed. Actual isolated Linux portrait selection/build/removal returned the exact 5-gold charge and 3-gold refund (1200 → 1195 → 1198), with correct contextual controls, left minimap pan, Esc menu, E rotation, End overview and Home reset. Separate Ironfold launch visually verified its eight-slot grid and rendered minimap. Both graphical processes exited zero. See Howl-Builds.json; Windows remains build-tested only.

## Perspective inspection and terrain pass

All 64 pure cases pass. Three focused Unity checks cover smooth zoom, ground-ray picking/dragging, rotation and protected UI, both-map scenery clearance, all 76 portraits, rendered minimaps and texture cleanup. A distant-ray precision failure was fixed through proportional near clipping and the original strict tolerance passed. The final forge-pillar geometry passed both-map clearance again. See DEPTH-AND-TERRAIN.md and Howl-Depth-Tests.json; no new full-suite or native-platform claim.

Final depth/terrain packages use source 4763c56. Both platform builds passed with zero errors, Linux both-map data smoke passed, and actual isolated Linux clicks confirmed the 5-gold close-view purchase and 3-gold refund plus two Ironfold purchases. Wheel, orbit, drag, edge, Home/End and menu controls were inspected in the player. Both normal closes exited zero. Windows runtime and native desktop compatibility remain unverified.
