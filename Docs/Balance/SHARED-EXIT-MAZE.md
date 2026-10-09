# Paid shared exit maze: ground delay versus air coverage

Verified 2026-10-09, following source 4be2563. This checkpoint adds test/planner fixtures and recorded campaigns. Runtime remains db0369c; no terrain mask, tower value, enemy value, player placement rule or team budget changes.

## Exact fixture and geometry

Rimewatch's shared exit neck is six world cells wide, x=28 through 33. Ten paid maze pieces form two alternating arms: y=12 leaves x=33 open; y=10 leaves x=28 open. The one-cell-high passage at y=11 connects them. Pieces are purchased round-robin among the active owners, using each owner's cheapest faction design. The three-player fixtures use factions 0/1/2 and 3/0/1, starts 7/0/4, and 400 gold per owner.

The new PaidSharedExitMaze regression runs both three-player rosters on the exact supplied mask. Every build uses normal orders and builder travel. After each order, it checks the tower owner and every wallet. Ground and air probes from all three lanes must exit without siege. All ground lanes take more than 30 additional ticks; all flyer times stay unchanged. Each teammate's attempted sale is rejected, then the actual owner's sale credits the exact refund. Removing all maze pieces restores every original traversal time, exercising navigation-cache invalidation.

The isolated geometric probes use weapons disabled to measure routes. Their measured 30 Hz travel times, before any further defenses:

| Lane | Empty ground ticks | Maze ground ticks | Extra ground seconds | Flight ticks, both |
|---|---:|---:|---:|---:|
| Left | 1002 | 1142 | 4.67 | 845 |
| Centre | 1240 | 1326 | 2.87 | 740 |
| Right | 1050 | 1117 | 2.23 | 874 |

These are default single-unit probe timings, not a promise that every crowded wave gains the same delay. Geometry copies in the sampling worlds are diagnostic only; campaign construction is paid and campaign weapons remain enabled.

## Paid campaign comparison

The new `compact-shared-maze` planner pays for the ten pieces, samples the resulting routes, then uses the existing compact-invest purchase/upgrade scoring. Its 48-standing-tower team limit includes the maze pieces (sixteen total per owner). A planner-only reserved rectangle x=28..33, y=8..13 keeps the fixture's passage clear. It imposes no restriction in the game. Other purchased towers can still alter the route outside that rectangle.

`compact-shared-transition` makes exactly the same decisions until every remaining wave flies. Before wave 20 it sells only each owner's ground-only towers and walls, then uses the normal refunds to buy and upgrade air defenses. It retains the same team limit. The saved comparisons verify every pre-wave-20 purchase, travel time, upgrade, sale and wave result is identical between these two policies.

| Difficulty / team | Open compact-invest | Shared maze | Shared maze + final-air rebuild |
|---|---|---|---|
| Hard: Rime / Stonebound / Ember | Defeat 18 | Defeat 20 | Win, 3 lives |
| Hard: Volt / Rime / Stonebound | Defeat 18 | Defeat 19 | Defeat 19 |
| Normal: Rime / Stonebound / Ember | Win, 25 lives | Defeat 20 | Win, 28 lives |
| Normal: Volt / Rime / Stonebound | Win, 30 lives | Win, 10 lives | Win, 27 lives |

The first Hard maze team leaks seventeen on wave 18 and ten on wave 19, reaching the finale with three lives. Selling eighteen ground-only pieces/towers refunds 1,511 gold; paid replacements remove all finale leaks. The second Hard team leaks twenty-nine on wave 18 and its final life on wave 19, before the air-rebuild rule can apply. That loss is retained.

The Normal Rime team improves ground leaks from five to two but loses to air if it keeps the maze layout; the final-air rebuild preserves 28 lives. The Normal Volt team still does better with the original open-coverage strategy. More maze pieces are therefore not automatically better, particularly when an artificial tower cap displaces air weapons. Buying the maze and resampling routes also changes later spending, so this is not a controlled experiment isolating path length alone.

Ten new campaigns were run: four maze, four maze/rebuild, two matching Normal controls. Two Hard open controls are reused unchanged from HARD-THREE-PLAYER-COMPACT.json. The twelve comparison records cover 234 attempted waves, with 702 independently checked player wallets and 702 incomes. There are no stalls or accounting errors. Four matched pairs confirm identical history before the final-air branch; one pair loses before reaching it. The real game has no 48-tower cap, and these scripted global-knowledge policies are not evidence of beginner-friendly human balance or graphical performance. No online sessions were run.

## Verification and reproduction

All 70 pure gameplay cases pass. The focused Unity EditMode case `Paid shared exit maze delays every ground lane and reopens by owner` passes in 1.246 seconds after successful compilation. An initial test-registration comma error was corrected before either successful run. Final headless build has zero errors and the existing .NET SYSLIB0060 deprecation warning in the networking password-derivation code; that unrelated code was not changed.

From the repository root:

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release

dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/shared-maze.json Hard 3 compact-shared-maze mixed Rimewatch 0 7,0,4

dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/shared-rebuild.json Hard 3 compact-shared-transition mixed Rimewatch 0 7,0,4

python3 Headless/audit_campaigns.py Docs/Balance/SHARED-EXIT-MAZE-CAMPAIGNS.json
```

Repeat with faction 3 and Normal difficulty, retaining separate output paths. The two new policies explicitly require the Rimewatch map filter; they do not invent an equivalent Ironfold fixture. Normal controls use `compact-invest` with the same starts. Exit zero means no stall/accounting failure; inspect Won for victory.

Full records and exact failure points: SHARED-EXIT-MAZE-CAMPAIGNS.json and SHARED-EXIT-MAZE-SUMMARY.json. Test/build outputs: SHARED-EXIT-MAZE-{PURE,UNITY,BUILD}. No runtime code changed, so Linux-TowerInfo / Windows-TowerInfo remain current; no rebuild, new Windows runtime test or rendered playthrough is claimed.

Next useful work: evaluate deliberate upstream ground defense for the remaining Volt/Rime/Stonebound Hard failure, while preserving an air-defense budget. Do not tune game values just to make this bot pass.
