# Paid zig-zag maze verification

The new optional `maze` campaign strategy constructs three alternating arms in the upper-left corridor before buying coverage defenses. Every tower is purchased through ordinary builder orders and completion-time charges, using the faction's cheapest design. Source map masks, every active lane and the fixed team budget are unchanged.

## Exact fixture

Coordinates are world-grid tower cells. Each row leaves a one-unit opening at the opposite end from the previous row.

| Map | Corridor cells | Wall rows (top to bottom) | Openings (top to bottom) | Paid maze pieces |
|---|---|---|---|---|
| Rimewatch | x = 6 through 12 | y = 50, 46, 42 | x = 12, 6, 12 | 18 |
| Ironfold | x = 8 through 14 | y = 56, 52, 48 | x = 14, 8, 14 | 18 |

An initial Ironfold test fixture stopped one cell short of the corridor edge; its detour assertion failed. Correcting the fixture to the full supplied corridor made it pass. No terrain or gameplay rule was changed to accommodate the test.

## Geometry regression

On each actual reference mask, the test pays for the maze, checks exact spending, then sends one ground unit and one flyer from every lane. Weapons are disabled only in this geometry regression so units cannot be killed before their route is measured. Every unit exits within the time limit; no open-maze enemy enters siege. The upper-left ground unit takes over 60 additional simulation ticks (over two seconds). Flight times and other-lane traversal times match the empty-map baseline. Selling every owned maze piece restores the original ground traversal time, exercising navigation-cache invalidation as well as the reopened path.

This geometry regression uses the first faction on each map. The full combat sweep below separately covers every faction with weapons enabled.

## Paid combat sweep

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-maze-solo.json Normal 1 maze
```

After constructing the fixture, the driver uses its existing greedy route-coverage purchases before every wave. It does not upgrade, sell, adapt its aim to utility effects, or impose a rule preserving the initial openings when buying later towers. Those additional defenses may alter the initial route. The strategy adds one upstream maze, not mazes in every lane. All lanes remain active. This checkpoint tests solo Normal only.

Raw JSON records all placements, costs, waves and final results. Team gold plus spending must equal starting funds, kill income and completed-wave rewards. A legitimate defeat is distinct from a stall or accounting failure.

| Map | Faction | Outcome | Lives | Spending |
|---|---|---|---:|---:|
| Rimewatch | Rime Covenant | Win | 30 | 3140 |
| Rimewatch | Stonebound | Win | 30 | 3133 |
| Rimewatch | Ember Assembly | Win | 30 | 3130 |
| Rimewatch | Volt Vanguard | Win | 30 | 3137 |
| Ironfold | Pulse Foundry | Win | 30 | 3400 |
| Ironfold | Blast Circuit | Win | 30 | 3400 |
| Ironfold | Prism Division | Win | 30 | 3400 |
| Ironfold | Horizon Guild | Win | 30 | 3430 |
| Ironfold | Gravity Works | Win | 30 | 3430 |
| Ironfold | Scrap Frontier | Win | 30 | 2380 |
| Ironfold | Overdrive Order | Win | 30 | 3270 |
| Ironfold | Tidal Array | Win | 30 | 3400 |

Completed 12 campaigns: 12 victories, 0 defeats, no stalls or accounting failures.

This establishes a reproducible paid maze strategy and a regression for freeform routing. It does not establish that mazing is necessary or optimally balanced: the earlier coverage-only strategy also won. The roster/upgrade strategy's five defeats remain relevant. Compare adaptive air coverage and multiple downstream mazes before changing global difficulty or expanding beyond ten waves.

Only tests, the headless strategy driver and documentation changed. Runtime game source and map assets are unchanged, so existing desktop build/smoke evidence remains applicable; no new player build is claimed for this checkpoint.
