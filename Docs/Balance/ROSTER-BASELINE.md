# Paid roster and upgrade campaigns

This checkpoint fixes Blast Circuit's Quicksilver: it was labeled an air interceptor but had both targeting flags disabled. It now damages flying targets with same-flight splash, leaving ground units untouched. Both the factory and serialized Ironfold asset are fixed. A second fix bounds the visual shot history at 128 events even when a volley creates multiple chain events. Two new regressions failed before the fixes and pass afterward; Unity additionally checks targeting in the packaged Ironfold roster.

## Method

The optional `roster` strategy buys one of every faction design in roster order, satisfying champion prerequisites through normal owned construction. It waits when the next purchase is unaffordable, then upgrades owned towers in descending direct-DPS order. After all standing owned towers reach level three, remaining funds go to the existing route-coverage strategy. Subsequent rounds also upgrade those newer towers. Purchases use real builder travel and completion-time charges; upgrades use the public paid upgrade API. No free money, teleports, damage overrides, or disabled lanes are used.

Placement searches the legal odd-cell grid for the most unobstructed route samples in range (air samples weighted 1.5). Unlike the coverage strategy, the initial roster placement does not discount overlapping coverage. It does not deliberately construct mazes, reposition/sell towers, adapt to leaks, or optimize utility effects. These limitations make defeats diagnostic of this strategy, not proof that a faction is unwinnable.

Two-player runs pair each faction with the next faction in its map roster, wrapping the last back to the first. Both use separate wallets, builders and owned unlocks, with the same fixed team budget. These are local simulations, not networking tests.

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-roster-solo.json Normal 1 roster
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-roster-mixed.json Normal 2 roster mixed
```

Omitting the last two arguments retains the original coverage strategy and same-faction teams. A zero exit code means no stalls/accounting failures; read `Won` to distinguish legitimate defeats. Raw JSON records every paid placement, upgrade, owner, purchase wave and wave outcome. Total spending includes upgrades; final team gold plus spending is checked against starting gold, kill income and completed-wave rewards.

## Results

24 completed campaigns: **19 victories and 5 defeats**, no stalls or accounting failures. All 24 robot champions were purchased through owned prerequisites; 636 paid upgrades succeeded. Solo won 8/12; mixed two-player won 11/12.

| Map | Solo faction | Solo outcome / lives | Two-player pairing | Pair outcome / lives |
|---|---|---|---|---|
| Rimewatch | Rime Covenant | Win / 30 | Rime Covenant + Stonebound | Win / 30 |
| Rimewatch | Stonebound | Win / 30 | Stonebound + Ember Assembly | Win / 29 |
| Rimewatch | Ember Assembly | Win / 30 | Ember Assembly + Volt Vanguard | Win / 30 |
| Rimewatch | Volt Vanguard | Win / 30 | Volt Vanguard + Rime Covenant | Win / 30 |
| Ironfold | Pulse Foundry | Win / 30 | Pulse Foundry + Blast Circuit | Win / 18 |
| Ironfold | Blast Circuit | Defeat at wave 6 / 0 | Blast Circuit + Prism Division | Win / 24 |
| Ironfold | Prism Division | Win / 25 | Prism Division + Horizon Guild | Win / 30 |
| Ironfold | Horizon Guild | Defeat at wave 6 / 0 | Horizon Guild + Gravity Works | Win / 29 |
| Ironfold | Gravity Works | Defeat at wave 6 / 0 | Gravity Works + Scrap Frontier | Defeat at wave 8 / 0 |
| Ironfold | Scrap Frontier | Win / 13 | Scrap Frontier + Overdrive Order | Win / 26 |
| Ironfold | Overdrive Order | Defeat at wave 6 / 0 | Overdrive Order + Tidal Array | Win / 2 |
| Ironfold | Tidal Array | Win / 15 | Tidal Array + Pulse Foundry | Win / 27 |

## Implications and next comparison

The earlier 36 coverage campaigns all won without leaks, but that strategy mostly bought cheap starters and predates the Quicksilver fix. These new runs show that a champion and its prerequisites are not automatically a sound defense: concentrated coverage, early specialist spending, savings for expensive upgrades and air-wave preparation all matter. Solo Blast Circuit, Horizon Guild, Gravity Works and Overdrive Order lost on wave six after air-wave-five leaks. Mixed Gravity Works + Scrap Frontier lost on wave eight; Overdrive Order + Tidal Array finished with only two lives.

The Quicksilver targeting repair is a correctness fix, not a broad balance adjustment. No health, cost, reward, range, lane or wave-count tuning changed. Do not raise global difficulty solely because the previous coverage driver dominated. Next compare deliberate ground mazes and distributed air coverage, and test adaptive spending before lengthening the campaign. These results still cover only ten waves and two scripted strategies.
