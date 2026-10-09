# Two-player Hard paid campaigns

Verified 2026-10-09 against source dc243c4, with runtime still b70de3c. This checkpoint adds regression coverage and campaign evidence, not gameplay tuning or a new player package.

Six pairs cover all twelve factions on both maps. Every run uses Hard difficulty, selected starts 7 and 0, all three/four active lanes, and the normal 1,200 team budget split into two 600-gold wallets. Builders travel and pay normally. All spending and rewards are audited per owner before and after each wave. An independent calculation over the saved purchase/upgrade/wave ledgers confirms all 476 wave-end wallet balances across the twelve runs.

| Team | 48-tower compact result | Unrestricted adaptive result | Adaptive purchases / upgrades |
|---|---|---|---:|
| Rime Covenant / Stonebound | Defeat on 18 | Win, 29 lives | 226 / 0 |
| Ember Assembly / Volt Vanguard | Defeat on 20 | Win, 30 lives | 233 / 0 |
| Pulse Foundry / Blast Circuit | Win, 2 lives | Win, 30 lives | 336 / 205 |
| Prism Division / Horizon Guild | Win, 30 lives | Win, 30 lives | 347 / 237 |
| Gravity Works / Scrap Frontier | Defeat on 20 | Win, 30 lives | 336 / 255 |
| Overdrive Order / Tidal Array | Win, 30 lives | Win, 30 lives | 336 / 182 |

The compact strategy is `compact-invest`: new footprints and upgrades compete for each spending decision, with an artificial 48-standing-tower team cap (24 per owner). It wins three of six campaigns. The existing unrestricted `adaptive` strategy buys coverage first, then upgrades when sampled sites fill; it wins all six. Both the spending heuristic and available tower count differ, so this is not a controlled experiment isolating tower count. Neither strategy is a skilled player substitute.

Compact warning points remain visible: Rime/Stonebound leaks 2 on wave 16, 8 on 17 and 20 on 18; Ember/Volt leaks 21 on 18, 3 on 19 and the final 6 on 20; Gravity/Scrap leaks 29 on 18 and its final life on 20. Pulse/Blast leaks 28 on the finale. A loss stops the simulation, so those final-wave leak counts are not a prediction of how many of the remaining enemies would have escaped. No faction buff was made to force this diagnostic to win.

Totals: compact runs attempt 118 waves and clear 115, with 288 purchases, 488 upgrades and 36,576 gold spent. Adaptive runs clear all 120 waves, with 1,814 purchases, 879 upgrades and 38,777 gold spent. No run stalls or fails accounting. All six adaptive wins use hundreds of towers, whole-map route knowledge and unlimited preparation time; they do not establish approachable human difficulty, human building speed or rendering performance at those tower counts. There are no live mid-wave sales/rebuilds in these campaigns. These are paid simulation tests, not online sessions.

## End-of-match queue regression

`CampaignCases.FinishedTeamQueues` runs victory and defeat fixtures for each player count from one through four. Every owner purchases and upgrades a tower and issues three distant build orders. The terminal wave ends while those orders are pending. The test verifies every owner's queue clears, unbuilt orders charge nothing, exact kill/completion rewards go to the correct wallets, further builds/upgrades/sales/wave launches are rejected, builders and simulation ticks remain frozen, and restart resets towers, queues, results and the fixed team budget.

All 69 pure tests pass. Unity compilation succeeds and the same focused terminal-queue regression passes under Unity (0.060 seconds). No runtime code changed, so the existing Linux-Snow / Windows-Snow packages remain current runtime b70de3c; no rebuild or new graphics/platform/network claim is made. The user's running player and live editor session were preserved.

## Reproduce

From the repository root:

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release

dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/rime-hard-compact.json Hard 2 compact-invest mixed Rimewatch 0 7,0
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/rime-hard-adaptive.json Hard 2 adaptive mixed Rimewatch 0 7,0
```

Repeat both strategies with Rimewatch faction 2 and Ironfold factions 0, 2, 4 and 6, using separate output files. Exit zero means no stall/accounting error; inspect `Won` to distinguish an ordinary defeat. Full records: HARD-PAIRS-COMPACT.json and HARD-PAIRS-ADAPTIVE.json; summary: HARD-PAIRS-SUMMARY.json. Pure and focused Unity results are HARD-PAIRS-PURE.txt and HARD-PAIRS-UNITY.json.

Next useful follow-through: measure actual crowded-scene rendering with a representative paid layout before treating the hundreds-of-towers strategy as a smooth graphical playthrough; continue testing compact ground coverage and flight coverage separately without tuning stats solely for the bot.
