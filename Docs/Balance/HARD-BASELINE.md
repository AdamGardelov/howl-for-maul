# Hard-difficulty baseline — 2026-10-08

All 12 map-specific factions won all ten waves on Hard with 30 lives. This extends the earlier 24 Normal runs to 36 recorded paid-defense campaigns. The driver, route sampling and purchasing strategy are unchanged; Hard uses the game’s actual 140% enemy health and siege damage. No balance overrides or free purchases were used.

| Map | Faction | Lives | Towers purchased | Normal combat seconds | Hard combat seconds |
|---|---|---:|---:|---:|---:|
| Rimewatch | Rime Covenant | 30 | 139 | 159.1 | 180.0 |
| Rimewatch | Stonebound | 30 | 112 | 176.8 | 207.2 |
| Rimewatch | Ember Assembly | 30 | 116 | 162.9 | 180.5 |
| Rimewatch | Volt Vanguard | 30 | 136 | 156.5 | 169.1 |
| Ironfold | Pulse Foundry | 30 | 322 | 177.9 | 205.4 |
| Ironfold | Blast Circuit | 30 | 322 | 196.4 | 232.6 |
| Ironfold | Prism Division | 30 | 322 | 181.6 | 213.8 |
| Ironfold | Horizon Guild | 30 | 335 | 175.8 | 205.2 |
| Ironfold | Gravity Works | 30 | 245 | 219.7 | 272.0 |
| Ironfold | Scrap Frontier | 30 | 322 | 170.5 | 195.1 |
| Ironfold | Overdrive Order | 30 | 309 | 185.0 | 212.8 |
| Ironfold | Tidal Array | 30 | 322 | 181.6 | 213.8 |

Combat seconds are fixed-step simulation time across all waves, excluding preparation and builder travel. They are not benchmark wall-clock timings.

The stronger enemies still do not force this baseline to abandon starter-heavy coverage. A health multiplier alone therefore does not establish satisfying progression in the ten-wave prototype. This does not measure player difficulty, prove all strategies work, or show that utility towers are unnecessary: this specific driver does not score their effects. Preserve the team-budget rule and evaluate longer progression, enemy variety, intentional mazes and upgrade/capstone strategies before changing tuning.

Reproduce with:

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/howl-hard.json Hard 1
```
