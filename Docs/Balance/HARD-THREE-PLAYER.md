# Three-player Hard campaigns and independent income audits

Verified 2026-10-09 on the gameplay source in b18e1e6 (runtime db0369c), with the new headless wave-income audit in this checkpoint. No tower, enemy, route, economy or presentation values changed. Player packages remain Linux-TowerInfo / Windows-TowerInfo.

Five mixed teams cover all twelve factions. Each team starts with three 400-gold wallets and selected starts 7, 0 and 4. All three Rimewatch / four Ironfold lanes stay active. Both strategies use normal paid purchases, builder travel, upgrades and rewards. The compact-invest diagnostic caps the team at 48 standing towers, sixteen per player; ordinary play has no such cap. Adaptive has no tower cap and uses a different spending heuristic, so this comparison does not isolate tower count.

| Team | Compact-invest | Adaptive | Adaptive purchases / upgrades |
|---|---|---|---:|
| Rime Covenant / Stonebound / Ember Assembly | Defeat on 18 | Win, 30 lives | 218 / 0 |
| Volt Vanguard / Rime Covenant / Stonebound | Defeat on 18 | Win, 30 lives | 229 / 0 |
| Pulse Foundry / Blast Circuit / Prism Division | Win, 30 lives | Win, 30 lives | 336 / 242 |
| Horizon Guild / Gravity Works / Scrap Frontier | Win, 7 lives | Win, 30 lives | 347 / 269 |
| Overdrive Order / Tidal Array / Pulse Foundry | Win, 30 lives | Win, 30 lives | 336 / 234 |

Both compact Rimewatch teams keep all 30 lives until the wave-18 rush, then lose. The compact Horizon/Gravity/Scrap team leaks fourteen on wave 18 and nine on the finale. Defeat stops the simulation; thirty terminal leaks do not imply that all remaining enemies would have been killed. These failures are retained in the records rather than tuned away.

The ten Hard campaigns attempt 196 waves and clear 194. Compact runs buy 240 towers and 429 upgrades, spending 29,071 gold. Adaptive runs buy 1,466 towers and 745 upgrades, spending 31,986. None stalls or fails accounting. Adaptive uses 218–347 purchases per run, global route knowledge and unlimited preparation time. These results do not establish human difficulty, real-time building speed, deliberate advanced maze quality or graphical performance. The teams are local simulations, not network sessions.

## Income and receipt verification

BalanceSweep now records the reward constants, summary presence, cleared/defeat outcome, team income and each player's wave income. Every wave compares the same WaveSummary data used by the HUD with combat results and wallet deltas. All preparation orders complete before combat in this driver, so combat wallet deltas exclude purchases/upgrades. Existing regressions separately cover spending during combat. The driver also retains the independent cumulative grant/spending audit.

Headless/audit_campaigns.py independently reads the saved JSON without running Unity or the C# simulation. It recomputes cumulative grant distribution and owner-specific purchases, upgrades and sale refunds at every wave. Integer reward remainders rotate across owners continuously; the auditor checks that exact distribution rather than merely comparing team totals.

Across the ten Hard runs, all 588 player-wallet and 588 player-income checks pass. Sixty-two waves have unequal three-player income, exercising remainder distribution. Both defeats correctly omit the completion reward. A separate Normal solo Stonebound compact-transition control verifies five paid sales totaling 873 gold and paid replacements before the final flying wave. It wins with sixteen lives; its 20 wave records bring the complete audit to 608 wallets and 608 incomes. This control is separate from the three-player Hard comparison.

All 69 pure gameplay regressions pass. Eight independent-auditor tests pass against saved real campaigns, including deliberate same-team-total wallet transfers, income transfers, reassigned purchase ownership, defeat incorrectly marked cleared, a missing summary, refund counted as income and a removed refund. Each corrupted record is rejected. This makes future saved campaign evidence repeatably auditable. Older JSON without the new income metadata is rejected rather than silently treated as verified.

This is a headless harness / evidence checkpoint. No new Unity suite, rendered performance, Windows runtime or networking test is claimed; no player rebuild is needed. Existing editor and running player were untouched.

## Reproduce

From the repository root:

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release

dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release --no-build -- --balance /tmp/rime-hard-three.json Hard 3 compact-invest mixed Rimewatch 0 7,0,4

python3 Headless/audit_campaigns.py Docs/Balance/HARD-THREE-PLAYER-COMPACT.json Docs/Balance/HARD-THREE-PLAYER-ADAPTIVE.json Docs/Balance/HARD-THREE-PLAYER-REFUND-CONTROL.json
python3 -m unittest discover -s Headless -p test_audit_campaigns.py -v
```

Repeat the balance command for Rimewatch faction 3 and Ironfold factions 0, 3 and 6, then repeat all five with `adaptive`. Use different output filenames. Exit zero means no stall/accounting error; inspect `Won` for victory. The refund control uses `Normal 1 compact-transition mixed Rimewatch 1 7`.

Raw records, counts and test output are HARD-THREE-PLAYER-{COMPACT,ADAPTIVE,REFUND-CONTROL,SUMMARY,AUDIT}.json and HARD-THREE-PLAYER-{PURE,AUDITOR-TESTS}.txt.

Next useful balance work: compare deliberate shared maze layouts with compact open coverage at the Rimewatch wave-18 threshold. Keep ground and flight coverage separate and retain paid construction and the same team budget. More bot victories alone are not grounds for claiming beginner-friendly balance.
