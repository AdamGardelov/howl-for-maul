# Four-player Hard paid campaigns

Verified 2026-10-08 against runtime source 1c112bc (documentation checkpoint fc6c806). No runtime balance changes were made for these results.

All twelve factions participated across three mixed teams. All lanes remained active, each player started with 300 of the fixed 1,200 team gold, and construction/upgrades used normal paid operations.

| Map / team | Waves | Lives | Purchases | Upgrades | Team spending | Final wallets |
|---|---:|---:|---:|---:|---:|---|
| Rimewatch: Rime, Stonebound, Ember, Volt | 20 | 30 | 227 | 0 | 5,900 | 87 / 77 / 82 / 82 |
| Ironfold: Pulse, Blast, Prism, Horizon | 20 | 30 | 335 | 250 | 6,720 | 96 / 96 / 96 / 96 |
| Ironfold: Gravity, Scrap, Overdrive, Tidal | 20 | 30 | 324 | 262 | 6,720 | 96 / 96 / 96 / 96 |

Three wins, no stalls or wallet errors. The harness audits spending and rewards for each independent wallet after every wave: 60 wave-end audits containing 240 player balances. Totals: 886 purchases, 512 upgrades and 19,340 gold spent. Raw purchase, upgrade and wave ledgers are in HARD-FOUR-PLAYER-FIRST.json and HARD-FOUR-PLAYER-SECOND.json.

Reproduce from the repository root:

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/howl-first-four.json Hard 4 adaptive mixed '' 0
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/howl-last-four.json Hard 4 adaptive mixed Ironfold 4
```

The adaptive bot knows enemy routes, buys between waves and uses hundreds of towers. This is paid simulation coverage, not a human playthrough, online multiplayer test or proof of final difficulty balance. At this historical checkpoint, compact solo Rime was unresolved under the artificial 48-tower diagnostic limit. The subsequent COMPACT-INVESTMENT.md records its winning strategy.

## Concurrent build queues

A new regression queues one contested cell and two private cells for each of four builders, then blocks one private order with a stationary enemy. It verifies one owner wins the contested cell, skipped orders charge nothing, subsequent orders complete, foreign sales fail, local player selection remains unchanged and the exact team balance is 1,160 after eight 5-gold towers.

All 64 pure simulation cases passed; complete output: ../Howl-Queue-Team-Tests.txt. Unity recompiled successfully with no errors (../Howl-Queue-Team-Compile.json). A full Unity suite was not rerun for this test-only addition. Existing player packages remain at source 1c112bc.
