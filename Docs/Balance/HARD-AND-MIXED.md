# Hard solo and Normal mixed adaptive defenses

Four complete twenty-wave paid simulations passed with 30 lives, no stalls and no per-player or team wallet errors. Simulation code/data were unchanged from the earlier wall-placement baseline; the current model work is presentation-only.

| Map | Difficulty / players | Factions | Towers bought | Upgrades | Spent | Final wallets |
|---|---|---|---:|---:|---:|---|
| Rimewatch | Hard / 1 | Rime Covenant | 240 | 0 | 5,920 | 308 |
| Ironfold | Hard / 1 | Pulse Foundry | 324 | 184 | 6,740 | 364 |
| Rimewatch | Normal / 2 | Rime Covenant + Stonebound | 224 | 0 | 5,915 | 159 / 154 |
| Ironfold | Normal / 2 | Pulse Foundry + Blast Circuit | 324 | 206 | 6,740 | 182 / 182 |

The adaptive policy buys whole-map route coverage with normal builder travel, then spends leftover money on affordable upgrades when its sampled sites fill up. Each player uses their own wallet and faction. It never adds money, disables lanes or grants free towers. Individual wallet ledgers are audited after each wave and at completion. Hard uses the normal 1.4 health/siege scaling. All four runs exited 0.

These results demonstrate viable strategies for these four configurations. They do not establish that Hard is balanced, that all faction/team combinations win, or that roster-first strategies are fixed. The earlier eight Ironfold two-player roster defeats remain relevant. The algorithm's route knowledge and hundreds of towers are not representative of a new human player. It samples whole-unit, two-unit-spaced sites, not the full half-cell construction lattice. No new tuning is justified from these four runs alone.

Raw data: ADAPTIVE-HARD-SOLO.json and ADAPTIVE-NORMAL-MIXED.json, including every purchase/upgrade, wave outcome and wallet. Commands (from repository root):

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance hard-solo.json Hard 1 adaptive same '' 0
dotnet Headless/bin/Release/net10.0/HowlForMaul.Headless.dll --balance normal-mixed.json Normal 2 adaptive mixed '' 0
```

The empty map filter selects both maps; faction index 0 selects Rime Covenant/Pulse Foundry. Mixed mode selects the next faction for player 2. Next: crowded-map rendering checks at observed tower counts, broader faction/difficulty combinations, and starting-position coverage.
