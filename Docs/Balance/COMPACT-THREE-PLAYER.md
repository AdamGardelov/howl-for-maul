# Three-player compact paid campaigns

Verified 2026-10-08 using the runtime and compact-invest harness from source 110633b (preceding documentation checkpoint 9d69f92). No game or strategy numbers changed for these results.

Five Normal campaigns cover all twelve factions across both maps. Three players receive 400 gold each from the fixed 1,200 team budget. Every lane stays active. Each builder buys up to sixteen standing towers under this diagnostic's 48-tower team cap, then spends on normal paid upgrades. All five runs win twenty waves; none stalls or fails an accounting audit.

| Map | Factions | Lives | Upgrades | Team spending | Final wallets |
|---|---|---:|---:|---:|---|
| Rimewatch | Rime Covenant / Stonebound / Ember Assembly | 9 | 82 | 5525 | 112 / 462 / 87 |
| Rimewatch | Volt Vanguard / Rime Covenant / Stonebound | 30 | 81 | 5810 | 126 / 136 / 156 |
| Ironfold | Pulse Foundry / Blast Circuit / Prism Division | 30 | 92 | 6180 | 268 / 128 / 528 |
| Ironfold | Horizon Guild / Gravity Works / Scrap Frontier | 30 | 88 | 6496 | 268 / 148 / 192 |
| Ironfold | Overdrive Order / Tidal Array / Pulse Foundry | 30 | 93 | 6040 | 408 / 528 / 128 |

Totals: 240 paid purchases, 436 upgrades and 30,051 gold spent. There are 100 wave-end ledger audits covering 300 individual player balances. Full construction, upgrade and wave records are in COMPACT-THREE-PLAYER.json.

Rime Covenant / Stonebound / Ember Assembly leaks 21 enemies on the final flying wave and finishes with nine lives. Volt Vanguard / Rime Covenant / Stonebound has no leaks. All three Ironfold teams keep 30 lives. This is evidence of differing results with these compositions and this bot, not proof that one faction is required or that human difficulty is settled. The planner knows empty-map routes, purchases between waves, and does not perform live rebuilding or deliberate zig-zag placement. The tower cap is never imposed on normal play. These are local simulation teams, not online multiplayer.

## Reproduce

Run each map/faction pair below with the command, replacing MAP and FACTION. Factions wrap through the roster for the three slots.

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance /tmp/three-player.json Normal 3 compact-invest mixed MAP FACTION
```

- Rimewatch 0: Rime Covenant / Stonebound / Ember Assembly.
- Rimewatch 3: Volt Vanguard / Rime Covenant / Stonebound.
- Ironfold 0: Pulse Foundry / Blast Circuit / Prism Division.
- Ironfold 3: Horizon Guild / Gravity Works / Scrap Frontier.
- Ironfold 6: Overdrive Order / Tidal Array / Pulse Foundry.

Use a different output path for each run to retain every ledger. Exit zero means no stall/accounting failure; inspect `Won` for victory.

The separate paid champion recovery regression is documented in ../CHAMPION-QUEUE-RECOVERY.md. This checkpoint changes tests and documentation only; the current player packages still contain 110633b.
