# Paid adaptation before the final flying wave

Verified 2026-10-09, continuing source 1f932c9 with runtime still 0ca580c. Changes are confined to the headless diagnostic and evidence. No tower/enemy stats, map masks, game rules or player packages changed.

## Why Stonebound finished with two lives

Its saved `compact-roles` defense leaked 14 enemies on the fast ground wave 18 and another 14 on the final flying wave 20. It kept five Worldroots, which cannot shoot air. The new `compact-transition` diagnostic plays identically until only flying waves remain, sells its own ground-only towers, and reinvests the normal quoted refunds using flight-route coverage. In the current twenty-wave campaign the transition happens only before wave 20. It never strips ground defense before waves 5, 10 or 15.

Stonebound's first nineteen wave records, purchases and upgrades remain identical. Selling five Worldroots returns 873 gold (134 + 269 + 269 + 67 + 134). The builder purchases five 55-gold Crag Hurlers through normal travel and invests in normal paid upgrades. All 90 finale enemies die, no finale enemies leak, and the campaign finishes with 16 lives instead of two. The ground-wave leaks remain unresolved by this tactic.

## Matched comparisons

The baseline is `compact-roles` throughout; the intervention is the same strategy until the terminal flight transition. Fourteen matched pairs cover all twelve solo factions and two mixed-faction pairs. All pre-finale histories and transactions match exactly, including the early-defeat records. Both strategies win 12 of 14 runs; neither stalls or fails an ownership/accounting check. The intervention improves survival margin for Stonebound and Ember, but does not rescue defeats before wave 20.

| Map | Factions | Baseline lives | Rebuild lives | Sales | Refunds | Result |
|---|---|---:|---:|---:|---:|---|
| Rimewatch | Rime Covenant | 0 | 0 | 0 | 0 | Defeat on 18 |
| Rimewatch | Stonebound | 2 | 16 | 5 | 873 | Win |
| Rimewatch | Ember Assembly | 5 | 15 | 10 | 2100 | Win |
| Rimewatch | Volt Vanguard | 30 | 30 | 0 | 0 | Win |
| Ironfold | Pulse Foundry | 30 | 30 | 4 | 287 | Win |
| Ironfold | Blast Circuit | 0 | 0 | 0 | 0 | Defeat on 18 |
| Ironfold | Prism Division | 30 | 30 | 0 | 0 | Win |
| Ironfold | Horizon Guild | 30 | 30 | 0 | 0 | Win |
| Ironfold | Gravity Works | 17 | 17 | 4 | 344 | Win |
| Ironfold | Scrap Frontier | 26 | 26 | 0 | 0 | Win |
| Ironfold | Overdrive Order | 30 | 30 | 6 | 369 | Win |
| Ironfold | Tidal Array | 30 | 30 | 0 | 0 | Win |
| Rimewatch | Rime Covenant / Stonebound | 10 | 10 | 6 | 914 | Win |
| Ironfold | Gravity Works / Scrap Frontier | 28 | 28 | 3 | 295 | Win |

Across both groups: 28 campaigns, 552 waves attempted (four runs defeat on wave 18), 1,382 paid purchases, 1,902 paid upgrades, 38 sales and 5,182 refunded gold. Every sale checks actual ownership, removal and exact credit to its owner while all other wallets stay unchanged. Subsequent purchases, kill rewards and completion bonuses pass separate per-wallet and team ledgers. Refunds do not advance the shared reward cursor. `Spent` remains gross construction/upgrade expenditure; `Refunded` and `PlayerRefunds` are separate credit totals.

The mixed Rime team refunds 579 to Rime Covenant and 335 to Stonebound; the Gravity/Scrap team refunds 295 only to Gravity Works. This tests asymmetric refunds alongside continuously split team rewards. It does not share the sale value across teammates.

The rerun of the original Stonebound control matches every field in the preceding selected-start record; only the new empty sale/refund fields are additional. Influence caches reset between factions and when switching to flight-only samples, so a whole-roster sweep cannot inherit another faction's sample indices.

## Limits and interpretation

Rime Covenant and Blast Circuit lose on wave 18 under this particular `compact-roles` planner in both groups. Their separate `compact-invest` wins remain valid; do not report the intervention as a universal strategy or replace the earlier results. Ember improves from five to fifteen lives, but still leaks on earlier ground waves. Teams that already catch the finale see no life improvement. These results support a player adaptation option, not a faction buff or a human difficulty verdict.

The bot has whole-map route knowledge, infinite preparation time and a diagnostic cap of 48 standing towers per team. Sold towers no longer occupy that cap, so cumulative purchases can exceed 48. It does not make live sales under enemy pressure or demonstrate human build speed. All lanes stay active; players still receive the fixed 1,200-gold starting team budget and keep exclusive rosters. No network, graphics, Unity integration or new platform pass is claimed. All 67 pure gameplay tests pass; headless compilation has zero errors and the existing PBKDF2 deprecation warning.

For a player trying Stonebound: preserve the ground defense through Gatebreakers (wave 19). Once the preview confirms the final Storm host is flying, consider selling ground-only Worldroots and using the displayed refunds for Crag Hurlers and air-defense upgrades. Place the new towers along the actual flight route and finish construction before launching. The exact bot results depend on its full layout; five arbitrary Hurlers are not a guaranteed victory.

## Reproduce

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/stone-control.json Normal 1 compact-roles mixed Rimewatch 1 1
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/stone-rebuild.json Normal 1 compact-transition mixed Rimewatch 1 1
```

For the solo sweep, use an empty map filter, faction -1 and starting position 1: `Normal 1 compact-transition mixed "" -1 1`; repeat with `compact-roles`. Mixed pairs use `Normal 2 STRATEGY mixed Rimewatch 0 7,0` and `Normal 2 STRATEGY mixed Ironfold 4 7,0`. Preserve separate output paths. Exact full ledgers are FINALE-REBUILD-CAMPAIGNS.json; the machine-readable paired comparison is FINALE-REBUILD-COMPARISON.json.
