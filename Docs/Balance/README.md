# Normal-difficulty baseline — 2026-10-08

Latest three-player Hard evidence and reusable ledger checks: [HARD-THREE-PLAYER.md](HARD-THREE-PLAYER.md). The ten-wave results below are historical.

All 24 runs completed ten waves with 30 shared lives remaining: each of the 12 map-specific factions in solo and two-player configurations. All lanes remained active. There were no stalls, no ground leaks and no air leaks. Team gold conserved exactly in every run.

## What the driver does

The headless driver samples each unobstructed ground and flying route, then chooses affordable tower placements for diminishing-return route coverage. It uses real build orders, normal builder travel, faction restrictions, wallet deductions and wave rewards. It spends between waves and runs the actual fixed-step simulation throughout combat. It never teleports builders, adds gold, disables terrain, changes enemies or grants free towers. Two-player runs use separate wallets and builders; each player uses the same faction in that run.

The driver considers direct damage, firing interval, range and target eligibility. It does not evaluate splash, slow or chain utility, upgrades, selling, deliberate maze construction or mixed-faction synergy. It favors air coverage by a fixed factor of 1.5 and limits each player to 100 purchases per preparation phase. These are reproducible baseline strategies, not expert play or a comprehensive balance evaluation.

## Results

| Map | Faction | Players | Lives | Towers | Spent | Remaining |
|---|---|---:|---:|---:|---:|---:|
| Rimewatch | Rime Covenant | 1 | 30 | 139 | 3130 | 290 |
| Rimewatch | Stonebound | 1 | 30 | 112 | 3130 | 290 |
| Rimewatch | Ember Assembly | 1 | 30 | 116 | 3140 | 280 |
| Rimewatch | Volt Vanguard | 1 | 30 | 136 | 3125 | 295 |
| Ironfold | Pulse Foundry | 1 | 30 | 322 | 3220 | 540 |
| Ironfold | Blast Circuit | 1 | 30 | 322 | 3220 | 540 |
| Ironfold | Prism Division | 1 | 30 | 322 | 3220 | 540 |
| Ironfold | Horizon Guild | 1 | 30 | 335 | 3350 | 410 |
| Ironfold | Gravity Works | 1 | 30 | 245 | 3430 | 330 |
| Ironfold | Scrap Frontier | 1 | 30 | 322 | 2254 | 1506 |
| Ironfold | Overdrive Order | 1 | 30 | 309 | 3090 | 670 |
| Ironfold | Tidal Array | 1 | 30 | 322 | 3220 | 540 |
| Rimewatch | Rime Covenant | 2 | 30 | 139 | 3130 | 290 |
| Rimewatch | Stonebound | 2 | 30 | 112 | 3130 | 290 |
| Rimewatch | Ember Assembly | 2 | 30 | 116 | 3140 | 280 |
| Rimewatch | Volt Vanguard | 2 | 30 | 136 | 3125 | 295 |
| Ironfold | Pulse Foundry | 2 | 30 | 322 | 3220 | 540 |
| Ironfold | Blast Circuit | 2 | 30 | 322 | 3220 | 540 |
| Ironfold | Prism Division | 2 | 30 | 322 | 3220 | 540 |
| Ironfold | Horizon Guild | 2 | 30 | 335 | 3350 | 410 |
| Ironfold | Gravity Works | 2 | 30 | 244 | 3416 | 344 |
| Ironfold | Scrap Frontier | 2 | 30 | 322 | 2254 | 1506 |
| Ironfold | Overdrive Order | 2 | 30 | 309 | 3090 | 670 |
| Ironfold | Tidal Array | 2 | 30 | 322 | 3220 | 540 |

## Implications

A starter-heavy defense is a proven winning option in this ten-wave Normal campaign. The robot runs often used hundreds of inexpensive towers. This does not prove support towers or champions are useless: the driver deliberately does not value their special effects. It does show that stronger progression and more demanding wave design need evaluation before calling Normal balanced. No gameplay numbers were changed on the strength of this limited strategy.

Next priorities: compare intentional mazes against open coverage, exercise upgrades and signature towers, test mixed factions and Hard difficulty, and profile large tower counts in the rendered game. Longer progression should have its own paid-defense evidence before being shipped.

## Reproduce

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/howl-solo.json Normal 1
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/howl-two-player.json Normal 2
```

The command records results after each faction and emits progress on stderr. Exit 0 means the sweep completed without a stall or accounting error; inspect `Won` for victory, since a legitimate defeat is a balance result rather than a tool failure. Full placements and per-wave results are retained in the JSON files; summary.csv is a compact comparison. The initial solo JSON predates the added PlayerCount/Player fields; it is unambiguously a one-player run.


A further [12 Hard solo runs](HARD-BASELINE.md) also finished without leaks. Full data is in hard-solo.json; these use the same strategy and have the same limitations.

## Roster and upgrade follow-up

The [24 roster/upgrade campaigns](ROSTER-BASELINE.md) add paid champions, upgrades and mixed-faction two-player teams after the Quicksilver targeting fix. They produced 19 wins and five legitimate defeats, unlike this earlier coverage baseline. Read both methods before drawing balance conclusions.

The [paid maze baseline](MAZE-BASELINE.md) adds twelve Normal solo maze-first campaigns and exact-mask detour/reopening regressions.

The [three/four-player team baseline](TEAM-BASELINE.md) adds mixed-faction coverage and roster campaigns, with exact per-wallet audits after every wave.

The [twenty-wave baseline](TWENTY-WAVE-BASELINE.md) supersedes earlier ten-wave balance evidence for the current campaign and robot tuning. Earlier raw datasets remain historical evidence.
