# Compact paid investment diagnostics

The new `compact-invest` harness strategy compares a paid upgrade with a new tower before every purchase. It scores marginal route coverage, damage roles and extra upgrade range, using diminishing returns and square-root cost weighting. Air samples have weight 0.9 versus ground 1.0. These are bot heuristics, not gameplay damage multipliers. The 48-standing-tower team limit remains a diagnostic only; normal play has no such limit.

All builds use normal builder travel, faction ownership, prices and wallet rules. Every wave audits independent wallets and the fixed 1,200-gold team budget. Map masks, all-active lanes, enemy stats, tower stats and economy are unchanged.

## Solo Normal matrix

| Map | Faction | Result | Lives | Purchases | Upgrades | Gold spent |
|---|---|---|---:|---:|---:|---:|
| Rimewatch | Rime Covenant | Win | 14 | 48 | 87 | 5920 |
| Rimewatch | Stonebound | Defeat on wave 20 | 0 | 48 | 69 | 5905 |
| Rimewatch | Ember Assembly | Win | 13 | 48 | 50 | 5925 |
| Rimewatch | Volt Vanguard | Win | 30 | 48 | 30 | 5925 |
| Ironfold | Pulse Foundry | Win | 30 | 48 | 44 | 6730 |
| Ironfold | Blast Circuit | Win | 30 | 48 | 46 | 6740 |
| Ironfold | Prism Division | Win | 30 | 48 | 96 | 5760 |
| Ironfold | Horizon Guild | Win | 30 | 48 | 35 | 6740 |
| Ironfold | Gravity Works | Win | 30 | 48 | 55 | 6744 |
| Ironfold | Scrap Frontier | Win | 30 | 48 | 81 | 6739 |
| Ironfold | Overdrive Order | Win | 30 | 48 | 81 | 6740 |
| Ironfold | Tidal Array | Win | 30 | 48 | 96 | 5760 |

Eleven of twelve solo runs won; none stalled or failed accounting. All eight Ironfold factions finished with 30 lives. Stonebound loses on the final flying wave with this strategy; its previously verified `compact-roles` win remains valid. Do not replace faction-specific choices with one supposedly universal bot.

Rime Covenant now has its first saved compact solo win: 48 purchased towers (31 Shard Sentries, 9 Hail Bells, 7 Aurora Needles and 1 Rime Binder), 87 paid upgrades, 5,920 gold spent and 14 lives after twenty waves. It leaked 3 enemies on wave 18 and 13 on wave 20. Together with the earlier strategies, all twelve factions now have at least one Normal compact solo win. This closes the specific diagnostic gap; it does not prove beginner-friendly difficulty.

## Why the intermediate losses matter

An air weight of 1.5 bought 15 Aurora Needles and lost to ground waves by wave 18. Weight 0.65 bought only five Needles, survived waves 1–19 without leaks, then lost to the final flying wave. Weight 0.9 bought seven Needles and survived the campaign. Full ledgers for all three attempts are retained in COMPACT-INVEST-DIAGNOSTICS.json. No game stats were adjusted to turn these comparisons into a win.

## Two-player Normal checks

- Rime Covenant + Stonebound: twenty-wave win, 12 lives, 48 purchases, 88 upgrades, 5885 team gold spent, final wallets 141 / 166.
- Gravity Works + Scrap Frontier: twenty-wave win, 30 lives, 48 purchases, 90 upgrades, 6396 team gold spent, final wallets 288 / 420.

Both use 24 standing towers per player and normal independent builders. Raw records: COMPACT-INVEST-PAIRS.json. The solo matrix is COMPACT-INVEST-NORMAL.json.

## Reproduce and interpret

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/invest.json Normal 1 compact-invest
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/rime-pair.json Normal 2 compact-invest mixed Rimewatch 0
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/iron-pair.json Normal 2 compact-invest mixed Ironfold 4
```

The bot knows empty-map routes, buys between waves, and has not been tested as a human teaching guide. It does not sell/rebuild, react to a live crowd, or deliberately construct a shared zig-zag. These runs verify paid simulation outcomes, not online multiplayer, frame rate or a manually played campaign. Existing coverage/roster/maze/adaptive/compact strategies remain available. All 64 pure simulation regressions also pass in this checkpoint.
