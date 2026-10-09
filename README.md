# Howl for Maul

An original maze tower-defense prototype inspired by the cooperative mauls played as Warcraft III custom maps. Build winding defenses, upgrade towers, and catch enemies that survive into downstream areas.

Built with Unity 6.3 LTS, C# and URP for Windows and Ubuntu Linux. Game code, procedural visuals and synthesized effects are original; no Warcraft III or Mega Man assets are included. The soundtrack uses attributed Scott Buckley tracks under CC BY 4.0; see THIRD-PARTY-NOTICES.md.

**Current status:** playable solo/local prototype with direct host/join multiplayer. Online players share a lobby, choose factions and unique starts, vote difficulty, and vote pause/resume. Direct connections need a reachable host; relay/public matchmaking is not configured. See [online play](Docs/ONLINE-PLAY.md) for setup and tested scope.

## Play

Open this project with **Unity 6000.3.25f1**, choose **Howl for Maul → Open game**, then press Play. The scene generates the selected map at runtime.

Choose **Rimewatch** (three upper lanes) or **Ironfold** (four upper lanes), both reconstructed from the supplied map layouts. Every lane stays active at every player count. Start solo or create/join a lobby, then choose factions, starting positions and difficulty before spawning. The maps retain downstream defense areas and one bottom exit. Rimewatch is dressed with frost ferns and blue-flame lanterns; Ironfold has copper scrub and warm braziers. Decorative props stay on blocked terrain so the buildable map remains clear.

Solo starts with the full **1,200 gold** team budget. Two players receive 600 each, three receive 400 each, four receive 300 each. Kill income and wave rewards are split without losing integer remainders. Every enemy reaching the final exit removes one of 30 shared lives. Finish twenty waves with lives remaining to win. Waves 5, 10, 15 and 20 fly. Later waves alternate fast rushes, dense swarms and tough siege units. Before launching, a compact forecast shows AIR/GROUND, scaled health, enemy count and targeting counts. Click it for expanded faction-specific advice. The team defense count distinguishes air and ground targeting; it does not measure whether towers cover the route.

Gameplay uses the full map viewport with a compact top status strip and bottom-right tower grid. Esc opens the game menu for settings, New Game/Leave and Quit; solo pauses, while an online match uses the team pause vote. Tab opens the optional detailed panel. The standalone menu also offers a fullscreen-window toggle.

## Controls

| Action | Control |
|---|---|
| Build selected tower | Left click on an empty cell |
| Queue another build | Shift + left click |
| Move builder / cancel its queue | Right click, or M then click |
| Cancel construction / clear selection | Cancel Orders button |
| Open / close game menu | Escape |
| Open / close detailed panel | Tab |
| Select tower | Click an existing tower |
| Upgrade selected tower | U or its sidebar button |
| Sell your tower | X then click, or selected-tower button |
| Select tower design | 1–7 within your faction roster |
| Build mode | B |
| Inspect an enemy | Ctrl + click |
| Reveal all health bars | Hold Alt |
| Launch next wave | Enter |
| Open expanded wave advice | Click the pre-wave forecast |
| Pause / resume (majority vote online) | P |
| Pan | Screen edges / Space + left drag / middle drag / WASD / arrows |
| Faster keyboard / edge pan | Hold Shift |
| Rotate camera left / right | Hold Q / E |
| Reset camera angle without moving | R / minimap north button |
| Restore default orientation at builder | Home |
| Whole-map overview | End |
| Tactical-map pan | Click or drag on the minimap |
| Zoom | Wheel |
| Grid / clearance overlay | G / F |

Build orders are charged when they succeed, not when queued. New unmodified build orders replace the current queue. Each queued order remembers its tower design. At arrival, an enemy-blocked, occupied or unaffordable order is skipped without charging; the builder continues in click order. Skipped orders are discarded rather than retried. The compact portrait grid shows the remaining queue count. Moving cancels the queue. Orders wait while paused. Every queued footprint is outlined and numbered: gold for the current order, teal for later orders.

Opening **New match / setup** freezes the current match. **Return to match** keeps its towers, gold, wave and pause state; **Start new match** applies the selected options to a fresh defense. Changing maps closes the current match. Starting or resetting clears Sell/Move mode and selections. Escape opens the paused game menu and preserves queued construction. Use Cancel Orders or move the builder to cancel it.

Towers have distinct wall, sentry, control, artillery, relay, interceptor and champion silhouettes, faction colors and illuminated upgrade tiers. Weapons turn toward actual shots and recoil briefly; the animation follows simulation speed and freezes in pause/setup. Home resets orientation and focuses the builder; End restores the overview; the minimap outlines the camera view. Ground enemies use orange armored crawler silhouettes, with narrow swept-fin runners and broad shielded siege units; flying enemies have purple animated wings and hover above the battlefield. Health bars prioritize selected units, damaged towers and siege attackers while suppressing overlaps; hold Alt to reveal all visible health bars, including healthy units. Ctrl-click inspection follows the projected model at every zoom. Cyan markers indicate slowing, and a red crest indicates siege. Splash and chain effects follow ground/flight height and freeze while paused. Damage briefly flashes the enemy crest; amber/violet shards mark defeats and red rings mark leaks, within a shared 64-effect budget. A sidebar breach warning groups recent leaks and lets you focus the exit, including when it happens off camera. Camera-local weapon sounds are rate-limited, while a grouped breach tone alerts you to leaks elsewhere. Pause/setup and the combat-sound toggle mute the source. All twelve factions now have original design-specific models: twenty winter towers and fifty-six Ironfold towers. See Docs/IRON-MODELS.md and Docs/WINTER-MODELS.md. The first stylized visual pass adds continuous terrain shading, beveled cliffs, layered snow pines, faceted tower shapes and matte lighting. These original procedural models remain prototype art; see Docs/ART-DIRECTION.md.

Full route blockage is allowed: ground enemies find a player-built obstruction to attack. Selling or destruction opens the route again. Congestion alone does not trigger siege. Permanent terrain cannot be built on, sold, damaged or crossed by ground units. Intermediate route points can be built over; ground enemies pass through a nearby clear area instead. Spawns and the final exit stay protected. Towers can align with map walls: placement snaps to one unit on Rimewatch and half a unit on Ironfold, preserving the same tower sizes. See [wall placement verification](Docs/WALL-PLACEMENT.md).

The HUD keeps gold, lives, wave controls and action messages visible while the construction roster scrolls. Cards show role, targeting, price and lock/affordability state. Selecting a tower brings its upgrade inspector into view. See [HUD update and verification limits](Docs/HUD-UPDATE.md).

## Towers

| Faction | Main roles |
|---|---|
| Rime Covenant | Slow, ground splash, dedicated air needles |
| Stonebound | Durable maze walls, heavy ground splash, sky projectiles |
| Ember Assembly | Rapid fire, ground artillery, fast anti-air |
| Volt Vanguard | Arm-cannon sentries, chain attacks, long-range air defense |

Rimewatch factions have five exclusive designs. Ironfold instead offers eight robot factions: Pulse Foundry, Blast Circuit, Prism Division, Horizon Guild, Gravity Works, Scrap Frontier, Overdrive Order and Tidal Array. Each has six regular designs and a powerful champion unlocked by owning all six. Every roster includes affordable maze construction and an air specialist. These are original interpretations, not a claimed transcription of any one historical version. See [research and decisions](Docs/MAUL-RESEARCH.md), [the battlefield update](Docs/BATTLEFIELD-UPDATE.md) and [current twenty-wave balance results](Docs/Balance/TWENTY-WAVE-BASELINE.md).

After each wave, the sidebar reports defeated enemies, leaks and earned bounty/bonus income. In local multi-player matches it shows your share and the team total, independently of what you spent during combat.

Before buying, the sidebar shows damage, firing interval, direct DPS, range, targeting and special effects. Locked champions list the specific towers you still need to own. Losing a prerequisite relocks new champions but keeps existing ones. A queued champion is skipped without charging if a prerequisite is missing on arrival; later orders continue. Replacing the missing design restores the unlock, and you can issue a new champion order. Wave previews show difficulty-scaled health and siege damage, speed, spawn interval and the next flying wave.

Purchased towers can be upgraded twice. Upgrades improve health and weapon damage/range, costing the original tower price times its current level. Sale refunds include part of the upgrade investment; the selected-tower inspector shows the exact amount and owner, and disables actions unavailable to the current player. Players may build anywhere on open terrain, but can sell or upgrade only their own towers. The P1–P4 sidebar buttons switch local control; they are not a network lobby.

## Maps and tuning

Choose **Howl for Maul → Select map parameters** to inspect Rimewatch.asset. Runtime data is copied so playing does not modify the asset. The unrestricted Maze Lab is a development fixture; it is no longer offered in normal match setup.

Map layout, terrain, lane routes, builder starts, tower catalog and waves are data-driven. Map assets with SelectableMap enabled appear in setup automatically. See [map authoring](Docs/MAP-AUTHORING.md) and [design direction](Docs/DESIGN-NOTES.md).

Difficulty scales enemy health and siege damage to 70%, 100% or 140%; it never disables lanes. These are provisional tuning values, not final balance.

## Verify and build

With the .NET 10 SDK:

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release
```

The headless runner executes pure simulation cases. Its optional `--balance` mode exercises paid defenses across both maps and every faction; see [the coverage baseline](Docs/Balance/README.md) and [24 roster/upgrade campaigns](Docs/Balance/ROSTER-BASELINE.md) for commands, results and limitations. The latter includes real champion purchases, paid upgrades and mixed-faction teams. The [paid maze baseline](Docs/Balance/MAZE-BASELINE.md) adds deliberate zig-zag construction and route/reopening checks. The [team baseline](Docs/Balance/TEAM-BASELINE.md) covers three/four-player mixed teams with per-player wallet audits. The [compact-defense diagnostics](Docs/Balance/COMPACT-DEFENSE.md) compare 48-tower bots, including wins with all twelve factions across several strategies and mixed teams; the [paid investment follow-up](Docs/Balance/COMPACT-INVESTMENT.md) closes the Rime diagnostic gap without changing game stats. Unity's **Window → General → Test Runner → EditMode → Run All** additionally checks real Play-mode setup, builder/tower visuals, chosen starts, map switching and difficulty. Stop Play before starting Edit-mode tests.

Use **Howl for Maul → Build Linux** or **Build Windows** for standard output directories. Windows requires the Windows Mono build module. The verified direct-online packages were built with explicit separate paths to preserve the running older player:

- `Builds/Linux-Online/HowlForMaul` (latest direct-online checkpoint)
- `Builds/Linux/HowlForMaul` (older preserved package)
- `Builds/Windows-Online/HowlForMaul.exe` (latest direct-online checkpoint)
- `Builds/Windows/HowlForMaul.exe` (older preserved package)

After building Linux, run `./Tools/smoke-linux.sh` to check both packaged maps without a display server. The explicit smoke mode skips presentation/audio startup; the script requires that isolation marker, both route/data checks and a clean exit. It is not a graphics or audio test. See [platform evidence](Docs/Platform/README.md).

Keep each executable with its accompanying data and runtime files. Do not run a second Unity editor against the same project. Stop and restart Play after changing scripts; simulation state does not survive a domain reload.

The internal C# namespaces/assembly names retain `FrostMaze` for serialized compatibility. The product, repository and build names are **Howl for Maul**.

See [verification](Docs/HOWL-VERIFICATION.md) for actual test/build results and limitations. Historical FrostMaze documents describe earlier prototypes, not the current match rules.

The tower picker is a compact bottom-right portrait grid. Click a tower image or use its number key; hover for its name, role and details. Costs and locked states stay visible on each tile.

The minimap stays on the left and renders the actual scenery from above. The middle stays clear except for contextual upgrade/removal controls on a selected tower. The command grid includes Remove [X]; hover a tower in removal mode to see its refund.

Zoom in for a lower perspective view of the 3D models; zoom out for a steep overview. Rimewatch has frost-worn paving and layered icy stonework; Ironfold has weathered foundry slate and copper trim. See Docs/DEPTH-AND-TERRAIN.md.

The [three-player compact campaigns](Docs/Balance/COMPACT-THREE-PLAYER.md) cover all twelve factions with separate 400-gold starting wallets. The [champion queue recovery regression](Docs/CHAMPION-QUEUE-RECOVERY.md) verifies paid rebuilding after a sold or destroyed prerequisite.

The landscape now continues beyond the playable boundary. Read [world atmosphere](Docs/WORLD-ATMOSPHERE.md) for scenery, tower sound identities and soundtrack attribution.

The [selected-start paid campaigns](Docs/Balance/SELECTED-STARTS.md) cover every starting position on both maps, all twelve factions and nondefault mixed-team starts. The balance harness accepts a final comma-separated start-index argument and records builder travel and wallet audits.

The [final-air rebuild comparison](Docs/Balance/FINALE-REBUILD.md) tests paid sales and replacement air defenses with exact owner-only refunds. It improves Stonebound’s and Ember’s finale survival without changing game stats; earlier-wave defeats are retained in the results.
