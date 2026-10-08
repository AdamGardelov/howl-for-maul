# Three- and four-player team economy

The campaign runner now checks each player's wallet after every wave and at the end, not only the team sum. Recorded purchases and upgrades must be paid by that owner. Expected grants use the cumulative starting budget, kill income and completed-wave rewards, divided with the same continuous rotating remainder policy. The check uses division/remainder accounting independently of the simulation's per-gold reward loop. A negative or mismatched wallet fails the run. New JSON fields record per-wave `PlayerGold`, `FinalWallets` and `PlayerSpending`.

This runner never sells; refund accounting remains covered by the existing simulation tests. Extending the driver with selling will require recording those credits in this audit.

## Method

All runs use Normal difficulty, the existing ten-wave campaigns, every active lane and the unchanged 1,200-gold team start. Three players start with 400 each; four start with 300 each. Each team begins at one faction and adds successive factions cyclically in that map's roster. The `Factions` arrays record exact player order. These are twelve rotations for each mode, not an exhaustive test of all possible combinations; Rimewatch four-player rotations contain the same four factions in different player order.

Coverage mode buys normal paid defenses using diminishing-return coverage of sampled ground/air paths. Roster mode buys one of every design through owned champion prerequisites, saves for the next purchase/upgrade, upgrades in descending direct DPS, and then buys coverage. The preparation driver issues purchases in player order; it does not model simultaneous human decisions. Builder travel and charges remain normal game actions. No free money, disabled lanes or balance overrides are used.

These are local simulations with multiple player slots, not online networking or a human multiplayer playtest.

```bash
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-three-mixed.json Normal 3 coverage mixed
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-four-mixed.json Normal 4 coverage mixed
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --balance normal-four-roster.json Normal 4 roster mixed
```

## Results

| Map | First faction in cyclic team | Three-player coverage | Four-player coverage | Four-player roster |
|---|---|---|---|---|
| Rimewatch | Rime Covenant | Win / 30 lives | Win / 30 lives | Win / 27 lives |
| Rimewatch | Stonebound | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Rimewatch | Ember Assembly | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Rimewatch | Volt Vanguard | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Ironfold | Pulse Foundry | Win / 30 lives | Win / 30 lives | Win / 17 lives |
| Ironfold | Blast Circuit | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Ironfold | Prism Division | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Ironfold | Horizon Guild | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Ironfold | Gravity Works | Win / 30 lives | Win / 30 lives | Win / 30 lives |
| Ironfold | Scrap Frontier | Win / 30 lives | Win / 30 lives | Win / 19 lives |
| Ironfold | Overdrive Order | Win / 30 lives | Win / 30 lives | Win / 26 lives |
| Ironfold | Tidal Array | Win / 30 lives | Win / 30 lives | Win / 17 lives |

Completed 36 campaigns: **36 wins and 0 defeats**, no stalls or individual/team accounting failures. Recorded 360 wave-end wallet snapshots, 104 paid upgrades and 28 owned champion purchases.

These runs extend the paid full-campaign evidence to all supported player counts. They validate the tested strategies and wallet conservation, not final balance. Coverage remains strong; roster-first savings and concentrated placement can leave air-defense gaps. Existing solo/two-player roster defeats remain relevant. Further work should compare adaptive spending and distributed air defenses before increasing difficulty or expanding beyond ten waves.

Only the headless verification driver and documentation changed. The existing 51-case headless suite passed again; the latest Unity suite remains 55/55 from the preceding runtime checkpoint. No Unity editor was needed, no player build was regenerated, and existing packaged-build evidence remains unchanged.
