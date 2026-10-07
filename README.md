# Howl for Maul

An original maze tower-defense prototype inspired by the cooperative mauls played as Warcraft III custom maps. Build winding defenses, upgrade towers, and catch enemies that survive into downstream areas.

Built with Unity 6.3 LTS, C# and URP for Windows and Ubuntu Linux. All code, procedural visuals and synthesized sounds are original; no Warcraft III or Mega Man assets are included.

**Current status:** playable offline prototype. The setup can simulate 1–4 player slots with independent builders, wallets and tower ownership on one computer. Online multiplayer is not implemented.

## Play

Open this project with **Unity 6000.3.25f1**, choose **Howl for Maul → Open maze lab**, then press Play. The scene generates the selected map at runtime.

Choose **Rimewatch** (three upper lanes) or **Ironfold** (four upper lanes), both reconstructed from the supplied map layouts. Every lane stays active at every player count. Select difficulty, an original builder faction for each player, and—with multiple player slots—starting positions before spawning. The maps retain downstream defense areas and one bottom exit.

Solo starts with the full **1,200 gold** team budget. Two players receive 600 each, three receive 400 each, four receive 300 each. Kill income and wave rewards are split without losing integer remainders. Every enemy reaching the final exit removes one of 30 shared lives. Finish ten waves with lives remaining to win. Waves 5 and 10 fly.

## Controls

| Action | Control |
|---|---|
| Build selected tower | Left click on an empty cell |
| Queue another build | Shift + left click |
| Move builder / cancel its queue | Right click, or M then click |
| Cancel construction / clear selection | Escape |
| Select tower | Click an existing tower |
| Upgrade selected tower | U or its sidebar button |
| Sell your tower | X then click, or selected-tower button |
| Select tower design | 1–7 within your faction roster |
| Build mode | B |
| Inspect an enemy | Ctrl + click |
| Launch next wave | Space |
| Pause | P |
| Pan | WASD / arrows / middle drag |
| Focus active builder | Home |
| Tactical-map pan | Click or drag on the minimap |
| Zoom | Wheel |
| Grid / clearance overlay | G / F |

Build orders are charged when they succeed, not when queued. New unmodified build orders replace the current queue. Each queued order remembers its tower design. Insufficient funds or occupied terrain at arrival rejects that construction without charging. Moving cancels the queue. Orders wait while paused.

Full route blockage is allowed: ground enemies find a player-built obstruction to attack. Selling or destruction opens the route again. Congestion alone does not trigger siege. Permanent terrain cannot be built on, sold, damaged or crossed by ground units.

## Towers

| Faction | Main roles |
|---|---|
| Rime Covenant | Slow, ground splash, dedicated air needles |
| Stonebound | Durable maze walls, heavy ground splash, sky projectiles |
| Ember Assembly | Rapid fire, ground artillery, fast anti-air |
| Volt Vanguard | Arm-cannon sentries, chain attacks, long-range air defense |

Rimewatch factions have five exclusive designs. Ironfold instead offers eight robot factions: Pulse Foundry, Blast Circuit, Prism Division, Horizon Guild, Gravity Works, Scrap Frontier, Overdrive Order and Tidal Array. Each has six regular designs and a powerful champion unlocked by owning all six. Every roster includes affordable maze construction and an air specialist. These are original interpretations, not a claimed transcription of any one historical version. See [research and decisions](Docs/MAUL-RESEARCH.md).

Purchased towers can be upgraded twice. Upgrades improve health and weapon damage/range, costing the original tower price times its current level. Sale refunds include part of the upgrade investment. Players may build anywhere on open terrain, but can sell or upgrade only their own towers. The P1–P4 sidebar buttons switch local control; they are not a network lobby.

## Maps and tuning

Choose **Howl for Maul → Select map parameters** to inspect Rimewatch.asset. Runtime data is copied so playing does not modify the asset. The original unrestricted Maze Lab remains accessible through the sidebar.

Map layout, terrain, lane routes, builder starts, tower catalog and waves are data-driven. Map assets with SelectableMap enabled appear in setup automatically. See [map authoring](Docs/MAP-AUTHORING.md) and [design direction](Docs/DESIGN-NOTES.md).

Difficulty scales enemy health and siege damage to 70%, 100% or 140%; it never disables lanes. These are provisional tuning values, not final balance.

## Verify and build

With the .NET 10 SDK:

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release
```

The headless runner executes pure simulation cases. Its optional `--balance` mode exercises paid defenses across both maps and every faction; see [the 24-run baseline](Docs/Balance/README.md) for commands, results and limitations. Unity's **Window → General → Test Runner → EditMode → Run All** additionally checks real Play-mode setup, builder/tower visuals, chosen starts, map switching and difficulty. Stop Play before starting Edit-mode tests.

Use **Howl for Maul → Build Linux** or **Build Windows**. Windows requires the Windows Mono build module. Output executables:

- `Builds/Linux/HowlForMaul`
- `Builds/Windows/HowlForMaul.exe`

Keep each executable with its accompanying data and runtime files. Do not run a second Unity editor against the same project. Stop and restart Play after changing scripts; simulation state does not survive a domain reload.

The internal C# namespaces/assembly names retain `FrostMaze` for serialized compatibility. The product, repository and build names are **Howl for Maul**.

See [verification](Docs/HOWL-VERIFICATION.md) for actual test/build results and limitations. Historical FrostMaze documents describe earlier prototypes, not the current match rules.
