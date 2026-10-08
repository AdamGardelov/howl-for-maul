# Paid campaigns from selectable starting positions

Verified 2026-10-09 against runtime source 0ca580c (preceding verification commit 6379e60), with the headless harness changes in this checkpoint. No runtime scripts, stats, terrain masks, music or player packages changed.

Twenty Normal campaigns complete all twenty waves: sixteen solo runs cover all eight starting indices on each map and all twelve factions; four mixed-team runs use nondefault, distinct starts. Rimewatch keeps three active lanes and Ironfold four. Team starting gold stays 1,200, split into 1,200 / 600 / 400 / 300 wallets. There are no stalls or accounting failures.

The harness now accepts an optional comma-separated list of zero-based start indices, checks exact spawn and restart positions, records initial coordinates/wallets and each paid construction's travel ticks, and audits wallets both after preparation and after combat. Completed waves must account for the full enemy count across every active lane. It also rejects invalid filters/settings before running or overwriting results: unknown map/faction filters previously could report success after zero campaigns.

## Results

| Map | Factions | Start indices | Lives | Upgrades | Gold spent |
|---|---|---|---:|---:|---:|
| Rimewatch | Rime Covenant | 0 | 12 | 86 | 5915 |
| Rimewatch | Stonebound | 1 | 2 | 86 | 5885 |
| Rimewatch | Ember Assembly | 2 | 13 | 50 | 5925 |
| Rimewatch | Volt Vanguard | 3 | 30 | 28 | 5910 |
| Rimewatch | Rime Covenant | 4 | 12 | 86 | 5915 |
| Rimewatch | Stonebound | 5 | 2 | 86 | 5885 |
| Rimewatch | Ember Assembly | 6 | 13 | 50 | 5925 |
| Rimewatch | Volt Vanguard | 7 | 30 | 28 | 5910 |
| Ironfold | Pulse Foundry | 0 | 30 | 42 | 6730 |
| Ironfold | Blast Circuit | 1 | 30 | 43 | 6730 |
| Ironfold | Prism Division | 2 | 30 | 96 | 5760 |
| Ironfold | Horizon Guild | 3 | 30 | 32 | 6740 |
| Ironfold | Gravity Works | 4 | 30 | 65 | 6738 |
| Ironfold | Scrap Frontier | 5 | 30 | 87 | 6666 |
| Ironfold | Overdrive Order | 6 | 30 | 91 | 6740 |
| Ironfold | Tidal Array | 7 | 30 | 96 | 5760 |
| Rimewatch | Rime Covenant / Stonebound | 7,0 | 12 | 87 | 5860 |
| Ironfold | Gravity Works / Scrap Frontier | 7,0 | 30 | 90 | 6492 |
| Rimewatch | Volt Vanguard / Rime Covenant / Stonebound | 7,4,1 | 30 | 87 | 5870 |
| Ironfold | Pulse Foundry / Blast Circuit / Prism Division / Horizon Guild | 7,5,3,1 | 30 | 87 | 6040 |

Across the twenty runs: 400 completed waves, 960 paid purchases, 1,403 paid upgrades and 123,396 gold spent. Before/after-wave checks cover 1,080 individual wallet balances. Builders travel normally and pay only when construction succeeds. Starting from different regions changes first-purchase travel time; no builder teleport or free defense is used.

Both Stonebound solo runs finish with only two lives. These wins do not establish beginner-friendly difficulty. Stonebound uses the previously saved `compact-roles` strategy; all other runs use `compact-invest`. This is not a universal winning strategy. The bot knows whole-map route samples, uses an arbitrary 48-tower team cap, and prepares between waves. It does not defend only its selected lane, test human build speed, deliberately weave mazes, or perform live rebuilding. Starting location therefore primarily tests correct spawn/travel and setup compatibility, not lane-specific strength. All twelve factions retain their distinct rosters and the player can build throughout the shared map.

The legacy invocation without a start argument also passes. Its Rime Covenant run matches the explicit upper-left run's purchases, upgrades, combat-wave ticks, lives and wallet ledger exactly after excluding starting coordinates and recorded travel time. In saved results, `StartingPositions: [-1]` identifies the legacy solo downstream default, not a legal UI selection.

## Reproduce

Build once with `dotnet build Headless/HowlForMaul.Headless.csproj -c Release`. Run from the repository root:

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/rime-start.json Normal 1 compact-invest mixed Rimewatch 0 0
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/iron-team.json Normal 4 compact-invest mixed Ironfold 0 7,5,3,1
```

For the sixteen solo runs, use start indices 0–7. Rimewatch's faction is `start % 4`; use `compact-roles` for faction 1, otherwise `compact-invest`. Ironfold's faction equals the start index. Team runs are Rimewatch faction 0 with two players at 7,0; Ironfold faction 4 with two at 7,0; Rimewatch faction 3 with three at 7,4,1; and Ironfold faction 0 with four at 7,5,3,1. `mixed` advances subsequent players through each map's roster. Use separate output files.

Twelve rejected CLI cases cover invalid map/faction filters, missing/extra/duplicate/out-of-range starts, malformed/overflowing numbers, difficulty and player count. All exit 2 with a concise reason and preserve an existing output file. See SELECTED-STARTS-ARGUMENTS.json. All 67 pure simulation tests pass; the headless build has zero errors and the existing .NET PBKDF2 deprecation warning. No new Unity, network, graphics, Windows runtime or internet test is claimed. Existing verified player packages remain source 0ca580c.

Full ledgers: SELECTED-STARTS.json and SELECTED-STARTS-DEFAULT.json. Pure test log: SELECTED-STARTS-PURE.txt. Exit 0 indicates no stall/accounting failure; inspect `Won` for victory, since a legitimate defeat is a balance result.
