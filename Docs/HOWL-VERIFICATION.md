Latest: [dedicated title screen](TITLE-SCREEN.md), Linux-Title build verified. Publishing held for [relay online work](RELAY-NEXT.md).

Latest checkpoint: [centred setup preview](SETUP-PREVIEW.md), Linux-Preview build; logo and automatic wave source included. See that note for verification limits.

Latest development checkpoint: [faction gallery, classic HUD and solo Last Stand](FACTION-GALLERY-HUD.md). Older measurements below retain their original configurations and are not new-economy results.

# Howl for Maul verification — complete procedural roster

All twelve factions now have original design-specific models: twenty winter towers and fifty-six Ironfold towers. This is a complete first procedural model pass, not finished League-quality art. Read IRON-MODELS.md and WINTER-MODELS.md.

Latest speed checkpoint: [MATCH-SPEED.md](MATCH-SPEED.md) records the four speed settings, unchanged fixed-step results, host authority, focused Unity test and package evidence. Mobile is on hold at the user's request.

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

Historical Linux and Windows packages at source 1c112bc included: all 76 models, tower collision indexing, repaint-only world overlays map props kept off walkable cells wave income recaps bounded combat cues off-camera leak alerts refined enemy silhouettes, paced combat audio, siege feedback faster camera controls, placement hints, owner/refund inspector, centered Rimewatch exit, terrain material washes, wave-planning advice, themed plants/braziers and full-map compact HUD with paused Esc menu. Both builds succeeded with zero errors; Linux reported one Pipeline-disabled warning, Windows 19 including unsupported package ray-tracing shaders. Windows runtime remains untested.

Fresh Linux packaged route/data smoke passed both maps without a display server, exit zero. The repeatable Tools/smoke-linux.sh checks data-only initialization, both map markers and clean process exit. An initial audio-shutdown crash (133) was caught; the explicit smoke path now skips presentation and passed three consecutive runs. See Platform/headless-audio-shutdown.json. The preceding 465974e package passed mouse-driven Start Match/build beside wall/select/upgrade/sell on isolated :98, with 1200 → 1180 → 1160 → 1190 gold and normal close exiting zero. These are separate checks, not a full mouse-played campaign or a GPU benchmark.

Native default X11 startup remains unresolved. A native Wayland/OpenGL probe reached the smoke markers but crashed on shutdown (139); not a pass. Read Platform/README.md and Howl-Builds.json for evidence. Editor restored to StandaloneLinux64.

## Preserved constraints

Both supplied terrain masks remain authoritative. Every lane stays active; enemies move top to bottom. Current team starts are 240 Rimewatch / 2,200 Ironfold gold split across one to four wallets; old campaign totals below retain their historical budgets. Faction ownership, paid travel/construction/upgrades, flush wall placement, freeform mazing and blockage/siege are preserved. Online networking is now authorized; see ONLINE-PLAY.md. No copyrighted reference assets were imported.

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

## Crowded combat presentation

Compilation, 64/64 pure cases and 6/6 focused Unity cases passed. The new regressions verify offscreen-volley budget protection, crossing beams/edge splashes, no replay when panning, pause/reset/cap behavior, zero managed bytes in 100 warmed stable view updates, exact sale/death cleanup and faction recolouring after restart. Existing combat audio, leak priority, recoil and paid siege checks pass. See CROWDED-COMBAT.md and Howl-Crowded-Unity-Tests.json. No frame-rate improvement or new full-suite claim is inferred.

Crowded-combat packages use source 3522f2c. Both builds passed with zero errors. Both-map Linux data smoke and actual isolated Linux purchase/selection/removal passed, 1200 → 1195 → 1198, with the sold model and inspector disappearing and normal exit zero. No packaged combat-wave or native-desktop/Windows runtime claim; see Howl-Builds.json.

## Health readability, perspective inspection and stonework

64 pure cases and five focused Unity cases pass across the saved runs. Legal projected crowds verify overlap suppression, Alt reveal, selection priority, HUD clipping and zero allocations in 100 warmed layout calls. Ground/flying inspection passes three zoom levels and camera rotation without changing the match. Camera safety, both-map prop clearance, all 76 portraits and rendered minimaps also pass. Initial health test fixture was corrected after illegal overlapping spawns were rejected. Read READABILITY-AND-STONEWORK.md and Howl-Readability-Unity-Tests.json.

Paid investment diagnostics close the compact Rime gap (48 towers, 87 upgrades, twenty waves, 14 lives) with no stat changes. The new strategy wins 11/12 solo runs and both mixed pairs; Stonebound retains its earlier winning strategy. Across saved strategies all twelve factions have a compact Normal solo win. Full ledgers and intermediate failures: Balance/COMPACT-INVESTMENT.md.

Readability/stonework packages use source 110633b. Linux and Windows builds have zero errors; Linux both-map data smoke passes. Three isolated Linux graphical sessions verify Rime purchase/select/Alt/remove (1200 → 1195 → 1198), two Ironfold purchases (1200 → 1180), both-map close/overview visuals and actual ground-enemy Ctrl-click during paused wave 1 without spending gold or adding orders. All three normal closes exit zero. Windows runtime and native desktop compatibility remain unverified. See READABILITY-AND-STONEWORK.md and Howl-Builds.json.

## Three-player compact campaigns and champion queue recovery

Five paid three-player Normal campaigns span both maps and all twelve factions: all win twenty waves, with no stalls or wallet errors. Rime/Stonebound/Ember finishes with nine lives after 21 final-air-wave leaks; the other four teams keep 30. Totals: 240 purchases, 436 upgrades, 30,051 gold spent and 300 individual wave-end wallet checks. The 48-tower cap and route-aware bot are diagnostics, not game restrictions or proof of human balance. See Balance/COMPACT-THREE-PLAYER.md and its complete ledgers.

A new real-Ironfold paid queue regression checks all eight robot factions after sale and injected destruction of a prerequisite (sixteen scenarios). Existing champions remain; queued champions skip without charge; subsequent replacement orders complete and restore the unlock; a newly issued champion order succeeds with an exact wallet ledger. All 65 pure cases, Unity compilation and the focused Unity case pass. See CHAMPION-QUEUE-RECOVERY.md and its result records. Tests/documentation only; existing packages remain source 110633b and no new platform or full-suite result is claimed.

## Packaged construction feedback — source 945da86

Linux-Next and Windows-Next builds succeeded with zero errors. Actual isolated Linux input showed three numbered queued footprints, then three paid towers (1200 → 1185 gold) and no remaining markers. Both game-menu Quit and initial-setup Quit exited zero. Both-map data smoke passed on isolated display :98. Two no-display starts crashed in native PlayerMain before game initialization (139); these are failures, not passes. Windows runtime is untested. The user’s existing player and standard package directory were preserved. New executable: Builds/Linux-Next/HowlForMaul.

## Direct online and atmosphere source checkpoint

67/67 pure gameplay cases pass. Real TCP/.NET checks pass two-process paid builds, ownership rejection, ordered ticks/digests, pause/resume and disconnect recovery on both maps; three/four-player checks verify budgets, capacity, unique starts, difficulty ties, vote expiry and malformed inputs. Three focused Unity cases pass separately: exterior/camera/all 76 sound identities; staged solo setup/selected spawn/streaming music/menu pause; combat audio voice limits/camera filtering/leak priority. No new full-suite claim. Read ONLINE-PLAY.md and WORLD-ATMOSPHERE.md. New packaged and internet/Windows runtime results are not implied.

## Packaged source 0ca580c

Linux-Online and Windows-Online builds succeeded with zero errors. Corrected packaged multiplayer probes passed on both maps: two actual Unity processes per map, authenticated lobby/setup, two paid purchases from separate wallets, active waves and ordered state digests, majority pause/resume, departure pause and remaining-player recovery. Both clients and both hosts exited zero. Both-map data/route smoke passed on isolated display :98, exit zero. The earlier failed probe is recorded in ONLINE-PLAY.md and is not counted as a pass.

Actual isolated graphical input verified staged solo faction/start/difficulty, close perspective, Q/E and R restoration, and three Shift-queued Snow Cairns becoming three paid towers with gold 1200 → 1185 and all markers clearing. UI and both map exteriors were captured and inspected. Windows remains build-tested only; all network checks so far are same-machine loopback.

Final client presentation regression passed (120.17 seconds): a real TCP client switches from Rimewatch to the host's Ironfold scene while retaining its session, then adopts slot 1, its chosen faction/start and 600-gold wallet. It does not advance without host frames; its settings menu does not stop shared ticks; majority pause and leave cleanup pass. See Howl-Online-Client-Unity-Test.json.

Final graphical source 0ca580c check also exercised actual Host/Ready/Begin UI, Pulse Foundry selection, Central Spine start and Normal vote on Ironfold. Q followed by the minimap north button restored the view. Both close scenery views were inspected. The game-menu Quit button closed normally with exit zero, and the player log contains no game exceptions. These checks ran on the owned isolated Xvfb display with llvmpipe; the user's existing desktop player was left running.

## Selectable-start paid campaigns

Twenty Normal campaigns passed twenty waves from all eight starts on each map, covering all twelve factions plus nondefault mixed two/three/four-player starts. All lanes accounted for; 960 purchases, 1,403 upgrades and 1,080 before/after-wave wallet checks. Stonebound's two-life finish remains a narrow bot win, not a balance guarantee. The headless harness records chosen starts and paid travel, and checks spawn/restart coordinates; twelve malformed requests are rejected without changing saved results. All 67 pure cases pass. See Balance/SELECTED-STARTS.md. No runtime or package changes and no new Unity/network/platform claims.

## Paid final-air adaptation

Fourteen matched control/intervention pairs (28 campaigns) verify paid final-wave selling/rebuilding, exact owner-only refunds and unchanged first-nineteen-wave histories. Stonebound finishes with 16 rather than two lives, Ember with 15 rather than five. Both groups win 12/14; Rime and Blast lose on wave 18 under this particular planner. Across the runs, 38 sales refund 5,182 gold with exact independent-wallet and team conservation. All 67 pure cases pass. See Balance/FINALE-REBUILD.md. Headless harness/docs only; player packages remain runtime 0ca580c.

## Preparation forecast

68 pure cases and the focused Unity forecast case pass (100.90 seconds), with clean Unity compilation. New compact preparation card shows scaled wave stats and target-capable team counts; clicking opens advice. Final-air advice uses owned, paid upgrade refunds without altering the match. Active combat has no leftover forecast hit area. Read WAVE-FORECAST.md. Package results are recorded separately after visual/input checks.

## Packaged source f9a858d

Linux-WavePreview and Windows-WavePreview builds succeeded with zero errors. Linux emitted two warnings (Pipeline disabled, plus an uncompiled-code-change warning about postprocessing/import code); Windows emitted the nineteen recorded Pipeline/ray-tracing warnings. No postprocessing/import code changed. Actual Linux input confirms the new runtime card is present and functional. Exact records are in Howl-Wave-Forecast-Packages.json.

On the owned isolated 1440×900 llvmpipe display, staged solo Stonebound setup showed 24 ground enemies, 35 HP and all three Rimewatch lanes. Clicking the forecast opened expanded advice without building or spending. A paid Pebble Warden changed gold from 1,200 to 1,175 and targeting count from zero to one. Enter launched wave 1 and removed the card; game-menu Quit exited zero with no game exception in the player log. Both-map data smoke also exited zero. Unity target was restored to Linux. Existing player/package directories were preserved. Final-air refund text is verified in the pure regression, not in a manually played final-wave screen. Windows runtime and current native desktop/internet compatibility remain untested.

## Snow readability

The new SnowSurfaceRepeatsContinuouslyAcrossExteriorTiles Unity regression passed (1.59 seconds), checking both axes, opposite edges, translated samples and both sides of seams. The initial scenery-test submission timed out during a domain reload; the editor was confirmed idle before the successful retry. No duplicate test job was launched.

Final Linux-Snow and Windows-Snow packages built with zero errors. Linux emitted one Pipeline-disabled warning; Windows emitted nineteen Pipeline/ray-tracing warnings, retained in Howl-Snow-Packages.json. Both packages contain source b70de3c and music attribution files. Unity was restored to StandaloneLinux64.

Actual Linux input on the owned isolated 1440×900 llvmpipe display verified staged Stonebound setup, a paid Pebble Warden (1,200 → 1,175 gold), overview/close camera views and the rendered minimap. Final captures show subdued snow without the initial exterior repeat seams. Leave Match and map switching showed Ironfold's retained slate/copper palette. Main-menu Quit exited zero, with no game exceptions in the player log. Both-map data smoke also exited zero. Existing user player and older packages were preserved.

Windows runtime, native desktop compatibility and networking were not retested in this texture pass. The 68 pure cases passed before the periodic-noise-only refinement; simulation source did not change. This is not a new full Unity-suite, performance or final-art claim.

## Two-player Hard and terminal queues

Two-player Hard follow-through: six pairs cover all twelve factions with starts 7/0 and independent 600-gold wallets. Compact investment wins 3/6; unrestricted adaptive wins 6/6 using 226–347 purchases. Twelve campaigns, 238 attempted waves and 476 independently recalculated wave-end wallets; no stalls/accounting failures. New terminal-queue regression covers victory/defeat with 1–4 owners; 69 pure cases and focused Unity case pass. Read Balance/HARD-PAIRS.md for failures and exact limits. Tests/docs only; packages remain runtime b70de3c.

## Large paid rendered scenes

Large paid Unity replay passed: both twenty-wave Hard campaigns reproduce saved purchases, builder travel, upgrades, kill/leak totals and wallets. Late-wave captures contain 226 towers / 44 enemies on Rimewatch and 347 / 66 on Ironfold. Warmed stable view sync allocates zero managed bytes; restart clears all unit views. GTX 1080/OpenGL editor captures inspected. Read LARGE-PAID-SCENES.md for exact timing scope and the two resolved harness failures. Tests/docs only; packages remain runtime b70de3c. Full combat/HUD frame-time profiling remains unverified.


## Compact tower information

Compact tower tooltips now show shared weapon stats, named missing owned champion requirements and exact gold shortfalls. All-roster focused Unity regression passes (1.79 s), including teammate exclusion and sale relocking. Final Linux/Windows packages contain db0369c; both builds and Linux normal paid purchase/live tooltip/Quit plus both-map data smoke pass. Windows runtime remains untested. Read TOWER-TOOLTIPS.md. New packages: Linux-TowerInfo / Windows-TowerInfo; editor target restored to Linux.


## Three-player Hard and wave-income auditing

Three-player Hard coverage: five mixed teams cover all twelve factions with 400 gold each and starts 7/0/4. Compact-invest wins 3/5 (both Rimewatch teams lose on wave 18); unrestricted adaptive wins 5/5 with 30 lives. New headless summary checks and independent saved-ledger auditor verify 588 wallets and 588 incomes, including 62 unequal reward splits. A paid refund control brings totals to 608 each. All 69 pure cases and eight auditor corruption/acceptance tests pass. Read Balance/HARD-THREE-PLAYER.md. Harness/tests/docs only; player packages remain runtime db0369c.


## Shared exit maze and paid air transition

Paid shared Rimewatch exit maze: ten owner-paid pieces delay all three ground lanes, leave flyers unchanged and reopen exactly after owner sales. All 70 pure cases and the focused Unity case pass. Ten new paid campaigns plus two retained controls show Rime/Stonebound/Ember can win compact Hard with three lives after a final-air rebuild; Volt/Rime/Stonebound still loses on 19. All four maze/rebuild pairs match before the finale; 702 wallets/incomes audited. Read Balance/SHARED-EXIT-MAZE.md for losses and limits. Tests/harness only; packages remain db0369c.


## Solo and LAN menu clarity

Menu clarity source 1aad187: Play Solo, Multiplayer / LAN, map selection first, and explicit same-keyboard local-slot test wording. Both desktop builds and Linux menu/form/staged solo/Quit plus both-map data smoke pass; Windows runtime untested. New packages Linux-Menu / Windows-Menu, editor restored to Linux. Read MENU-CLARITY.md. MOBILE-STATUS.md records Android/iOS as unimplemented future scope, including input/UI/device-test gaps.


## World cohesion

All six atmosphere priorities have an implementation and focused verification: grouped scenery and landmarks, surface shoulders, warm/cool lighting, actor movement/construction, solid projectiles/hit reactions, and quiet local ambience. The final scenery/ambience cases pass; all 76 towers/twelve builders and 72 armed projectile signatures pass. Current-economy two-player Hard campaigns clear twenty waves on both maps; 80 wallets and 80 incomes independently audited. Corrected Unity paid replay passes with 311/336 towers, exact purchase/travel/upgrade/accounting reproduction and cleanup. The obsolete 600-gold replay fixture and cached harness were corrected, not counted as passes. See WORLD-COHESION.md for timings, failures and scoped performance limits. Linux-Cohesion contains runtime source f2eee1f. Build passed with zero errors/one Pipeline warning; both packaged data/route checks and clean exit passed. Editor captures are separate evidence from data-only package smoke. Windows/network/native audio listening were not retested.

Final world-cohesion route follow-up: tall landmarks avoid flight corridors; low groundcover replaces trees underneath. Every new composition triangle passes independent blocked-ground and flight-clearance checks on both maps, alongside ambience, paid construction, hit reactions and cleanup. At least nine Ironfold groups remain after safe resampling. The preceding failed density result is retained. See Howl-Cohesion-AirClearance.json.


## Faction gallery and classic HUD

Faction gallery/classic HUD source 6a4abd9: actual portraits and pre-choice stats, automatic solo Last Stand, compact host speed selector and visible Scott Buckley credits. Focused Unity solo case, headless network checks, both desktop builds, Linux GUI/small-window input, both-map data smoke and packaged two-process checks pass. Windows runtime/real internet play unverified. Matching packages Linux-Gallery / Windows-Gallery include attribution and a friends guide. See FACTION-GALLERY-HUD.md and FRIENDS-RELEASE.md. Editor restored to Linux; scheduler/mobile stay paused.
