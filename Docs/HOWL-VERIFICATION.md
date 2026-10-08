# Howl for Maul verification — complete procedural roster

All twelve factions now have original design-specific models: twenty winter towers and fifty-six Ironfold towers. This is a complete first procedural model pass, not finished League-quality art. Read IRON-MODELS.md and WINTER-MODELS.md.

## Automated tests

Fresh Unity 6000.3.25f1 suite: **65/65 passed**, including seven Play-mode integration cases. Tool-reported duration 25.30 seconds; wall time includes editor reloads. Compilation had zero errors/warnings. Exact result: Howl-Unity-Tests.json.

Paid presentation coverage buys all eight Ironfold rosters, checks each champion's six prerequisites, air targeting, cosmetic collider absence and champion upgrades. It preserves higher Gravity costs and Scrap's cheap opener. Winter models/upgrades, shared mesh cleanup, animation pause/resume, shot facing/recoil, setup flow and map switching remain covered. Simulation cases cover exact masks, all lanes, fixed team economy, ownership, half-cell paid construction, wall seams, siege/reopening, combat targeting and the captured 61-unit corner jam. The minimap case independently checks all 20,480 source cells, cache reuse and texture cleanup.

## Paid campaigns

New Hard solo Prism adaptive campaign: twenty waves won, 30 lives, 324 purchases, 184 upgrades, 6740 spent and 364 gold left. New Hard mixed Gravity/Scrap campaign: twenty waves won, 30 lives, 324 purchases, 270 upgrades, each player spent 3372 and retained 180. No stalls or wallet errors. See Balance/PRISM-HARD.md and Balance/GRAVITY-SCRAP-HARD.md plus raw ledgers.

Earlier added coverage: four adaptive wins with 30 lives (Hard solo on both maps; Normal mixed two-player teams on both maps), documented in Balance/HARD-AND-MIXED.md. The earlier full Normal matrix remains 36 campaigns, 27 wins and nine defeats, zero stalls/wallet errors; all twelve factions won under adaptive spending. Roster-first strategies still lose. These bots know routes and often buy hundreds of cheap towers. These results do not settle final balance or establish beginner-friendly defenses.

## Visual and input checks

Every faction's lineup has been inspected at gameplay zoom; the six new Ironfold sets also have native 1920×884 close and normal-zoom captures. Paused lineups use normal purchases; they are not human playthroughs. Crowded diagnostic inspection used 324 injected towers and 74 high-health enemies, separately from economy tests. Ordinary health bars now shrink in the overview while the selected enemy stays readable. See Performance/LIVE-COMBAT.md.

An earlier quality package passed actual mouse-driven Start Match, build, select, upgrade and sell on virtual display :98 at 1440×900: gold 1200 → 1180 → 1160 → 1190. This does not verify native :0 desktop launching, Windows runtime or a full mouse-played match. See Performance/QUALITY-PASS.md.

## Performance

Rigid tower batching and permanent minimap caching reduced the matched paused 324-tower editor overview from 6206 to 4405 draw calls and median reported render time from 11.99 to 8.29 ms. With 2× MSAA and medium soft shadows the paused median was 8.33 ms, with slower tails. See Performance/TOWER-BATCHING.md, MINIMAP-CACHE.md and QUALITY-PASS.md.

The separate moving-combat diagnostic reported about 24.87 ms median render time with 324 towers and 74 enemies. That is not comparable to the paused fixture or a standalone FPS benchmark; crowded combat still merits profiling. All measurements are short local editor samples, not promises for other hardware.

## Packages and platform limits

Current packages are the preceding graphics-quality checkpoint, before Prism, Horizon, the final four Ironfold sets and compact health bars. A packaging follow-up is next. Those Linux/Windows builds succeeded with zero errors and 1/19 warnings. Windows runtime is untested. Linux route/data smoke passed both maps on isolated :98, exit 0. Earlier native :0 attempts failed with XFree86-VidModeExtension BadValue before game code; that issue is unresolved. Use Unity Play here. Read Howl-Builds.json for exact package evidence.

## Preserved constraints

Both supplied terrain masks remain authoritative. Every lane stays active; enemies move top to bottom. Team start is 1200 gold split across one to four wallets. Faction ownership, paid travel/construction/upgrades, flush wall placement, freeform mazing and blockage/siege are preserved. Online networking is deferred. No copyrighted reference assets were imported.
