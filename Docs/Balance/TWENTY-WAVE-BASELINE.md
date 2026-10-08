# Twenty-wave paid campaign checks

Normal difficulty, unchanged 1,200 team gold, all lanes active, real builder travel and purchases. Each wave audits individual wallets and the team ledger. These runs use the calibrated twenty-wave campaign and the updated faction catalog.

Ironfold waves 11–20 use 65% of the winter late-wave health budget. Its four separated approaches and different rosters need their own calibration. Enemy counts, kill income, wave rewards and every active lane remain unchanged. Difficulty scaling applies on top of authored map health. The first ten waves retain their previous values.

The initial uncalibrated draft exposed late Ironfold defeats. It also exposed a verifier limitation: the coverage policy exhausted its sampled build sites and left earned gold unspent. The adaptive policy adds normal paid upgrades of affordable owned weapons with route exposure. This does not add resources or modify combat. Other policy results are retained to show strategy sensitivity.

A dense wave exposed a 61-unit corner deadlock. A captured-state regression reproduced it before the fix and now verifies every unit exits without overlap, terrain penetration or false siege. The final datasets below were rerun after adding deterministic, collision-checked yielding.

Coverage and maze-first policies spend on diminishing-return sampled route coverage. Maze-first first pays for an eighteen-piece zig-zag. Roster-first buys one of every design and then saves for upgrades/champions; it deliberately delays other spending. Adaptive starts with coverage and reinvests remaining money in upgrades. None is a human strategy or an exhaustive balance search. Two-player rosters use consecutive faction pairs cyclically; other runs are solo.

| Policy | Map | Faction / first faction | Outcome | Lives | Paid upgrades |
|---|---|---|---|---|---|
| maze (1p) | Rimewatch | Rime Covenant | Win | 30 | 0 |
| maze (1p) | Rimewatch | Stonebound | Win | 30 | 0 |
| maze (1p) | Rimewatch | Ember Assembly | Win | 30 | 0 |
| maze (1p) | Rimewatch | Volt Vanguard | Win | 30 | 0 |
| maze (1p) | Ironfold | Pulse Foundry | Win | 30 | 0 |
| maze (1p) | Ironfold | Blast Circuit | Defeat at wave 19 | 0 | 0 |
| maze (1p) | Ironfold | Prism Division | Win | 23 | 0 |
| maze (1p) | Ironfold | Horizon Guild | Win | 30 | 0 |
| maze (1p) | Ironfold | Gravity Works | Win | 5 | 0 |
| maze (1p) | Ironfold | Scrap Frontier | Win | 26 | 0 |
| maze (1p) | Ironfold | Overdrive Order | Win | 21 | 0 |
| maze (1p) | Ironfold | Tidal Array | Win | 23 | 0 |
| roster (2p) | Rimewatch | Rime Covenant | Win | 8 | 96 |
| roster (2p) | Rimewatch | Stonebound | Win | 15 | 86 |
| roster (2p) | Rimewatch | Ember Assembly | Win | 30 | 96 |
| roster (2p) | Rimewatch | Volt Vanguard | Win | 30 | 110 |
| roster (2p) | Ironfold | Pulse Foundry | Defeat at wave 13 | 0 | 10 |
| roster (2p) | Ironfold | Blast Circuit | Defeat at wave 12 | 0 | 8 |
| roster (2p) | Ironfold | Prism Division | Defeat at wave 13 | 0 | 10 |
| roster (2p) | Ironfold | Horizon Guild | Defeat at wave 13 | 0 | 8 |
| roster (2p) | Ironfold | Gravity Works | Defeat at wave 12 | 0 | 7 |
| roster (2p) | Ironfold | Scrap Frontier | Defeat at wave 12 | 0 | 9 |
| roster (2p) | Ironfold | Overdrive Order | Defeat at wave 13 | 0 | 10 |
| roster (2p) | Ironfold | Tidal Array | Defeat at wave 13 | 0 | 10 |
| adaptive (1p) | Rimewatch | Rime Covenant | Win | 30 | 0 |
| adaptive (1p) | Rimewatch | Stonebound | Win | 30 | 0 |
| adaptive (1p) | Rimewatch | Ember Assembly | Win | 30 | 0 |
| adaptive (1p) | Rimewatch | Volt Vanguard | Win | 30 | 0 |
| adaptive (1p) | Ironfold | Pulse Foundry | Win | 30 | 184 |
| adaptive (1p) | Ironfold | Blast Circuit | Win | 30 | 191 |
| adaptive (1p) | Ironfold | Prism Division | Win | 30 | 184 |
| adaptive (1p) | Ironfold | Horizon Guild | Win | 30 | 230 |
| adaptive (1p) | Ironfold | Gravity Works | Win | 30 | 115 |
| adaptive (1p) | Ironfold | Scrap Frontier | Win | 30 | 385 |
| adaptive (1p) | Ironfold | Overdrive Order | Win | 30 | 124 |
| adaptive (1p) | Ironfold | Tidal Array | Win | 30 | 184 |

36 completed campaigns; 27 wins, 9 defeats. No stalls or wallet-accounting failures. 2057 paid upgrades. Defeats are reported, not counted as passes or erased.

The route-scoring cache stores only invariant base-design range/target influence. Placement legality and diminishing-return weights are recalculated. Completed unchanged Rimewatch coverage and maze runs were compared against the uncached draft and matched exactly, including purchases, ticks and wallets.

Reproduce with `dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance result.json Normal 1 adaptive`; substitute `coverage` or `maze`, or use `Normal 2 roster mixed`.
