# Compact paid-defense diagnostics

The harness can now limit its purchases to 48 standing towers across the team, split equally among active player slots. This is a diagnostic constraint in Headless/BalanceSweep.cs, not a gameplay tower limit. All construction uses normal builder travel, prices, upgrades, ownership and wallets. No simulation, tower catalog, map or wave values changed.

## Normal solo matrix

The compact-value strategy completed twenty waves with 10 of 12 factions. No stalls or wallet-ledger errors occurred. Every run built 48 towers.

| Map | Faction | Result | Waves reached | Lives left | Upgrades |
|---|---|---|---:|---:|---:|
| Ironfold | Blast Circuit | Win | 20 | 8 | 41 |
| Ironfold | Gravity Works | Win | 20 | 23 | 29 |
| Ironfold | Horizon Guild | Win | 20 | 30 | 29 |
| Ironfold | Overdrive Order | Win | 20 | 17 | 60 |
| Ironfold | Prism Division | Win | 20 | 30 | 37 |
| Ironfold | Pulse Foundry | Win | 20 | 30 | 46 |
| Ironfold | Scrap Frontier | Win | 20 | 21 | 85 |
| Ironfold | Tidal Array | Win | 20 | 30 | 44 |
| Rimewatch | Ember Assembly | Win | 20 | 19 | 90 |
| Rimewatch | Rime Covenant | Defeat | 18 | 0 | 89 |
| Rimewatch | Stonebound | Defeat | 13 | 0 | 48 |
| Rimewatch | Volt Vanguard | Win | 20 | 30 | 57 |

Raw purchases, upgrades and every wave: COMPACT-NORMAL.json.

## Strategy follow-ups

- Simple cost-efficient compact spending filled Prism with 48 cheap Shade Cadets and lost on wave 13 with 2,740 gold unspent. The slot-aware strategy bought a mixed roster including champions and won with all 30 lives. This demonstrates why a weak bot is not enough evidence to change enemy stats.
- Rime basic compact spending reached wave 20 but lost; slot-aware direct-damage scoring lost on wave 18.
- Accounting conservatively for splash/chain/slow roles let Stonebound win with 11 lives. Rime still lost on wave 18, including a follow-up that values slowing nearby friendly fire and avoids stacking the same slow bonus.
- Normal two-player Rime Covenant + Stonebound won with 19 lives. Gravity Works + Scrap Frontier won with 24 lives. Each player built at most 24 towers; every wallet and the fixed team budget were audited.

Across these diagnostic strategies, 11 of 12 factions have a compact solo win. Solo Rime remains an open compact-defense case. All twelve already had wins with the larger adaptive strategy. The two compact mixed-team wins are additional evidence for the user’s one/two-player economy requirement. Raw comparisons: COMPACT-FOLLOWUPS.json.

## Reproduce and interpret

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance compact.json Normal 1 compact-value
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance mixed.json Normal 2 compact-roles mixed Rimewatch 0
```

Available additional harness strategies:

- `compact`: existing cost-efficient coverage spending, then upgrades, within 48 team slots.
- `compact-value`: divides placement score by the square root of cost to value a scarce slot more highly.
- `compact-roles`: adds conservative splash/chain/self-slow damage estimates to the slot-aware score.
- `compact-support`: also estimates additional friendly firing time from a stronger slow at a covered sample; repeated equal/weaker slows add no bonus.

These are deliberately simple heuristics. Role estimates are not actual promised DPS. The bots know whole-map empty-route samples, do not deliberately design a shared maze and do not sell/rebuild their late layout. The 48-slot restriction is arbitrary and exposes poor spending choices as well as real defense weaknesses. Do not label this beginner-friendly balance or nerf waves solely to make this matrix green.

The final harness retains the older adaptive strategy exactly: the Hard two-player Gravity/Scrap rerun matches the entire saved ledger, including all purchases, upgrades, wave ticks and wallets. See COMPACT-ADAPTIVE-COMPARISON.json. The pure simulation suite also passed 61/61 before the scoring-only follow-ups.
