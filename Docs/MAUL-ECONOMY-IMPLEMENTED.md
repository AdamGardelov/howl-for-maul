# Gold, wood and a more inhabited world — 2026-10-09

## Economy

This implements a reference-inspired adaptation, not an exact reproduction of every historical maul. Sources and file hashes remain in MAUL-ECONOMY.md. The inspected Wintermaul X5 opening is 60 gold per defender; the inspected Mega Man 3.0 Final opening is 550. Four defender equivalents give these fixed team budgets:

| Map | Solo | Two players each | Three players | Four players each |
|---|---:|---:|---|---:|
| Rimewatch | 240 | 120 | 80 / 80 / 80 | 60 |
| Ironfold | 2200 | 1100 | 734 / 733 / 733 | 550 |

All lanes remain active. Gold is shared fairly through the existing integer-preserving allocator, rather than historical killer-only income. Kill bounty is 1 / 2 / 3 / 4 / 5 team gold across waves 1–4 / 5–8 / 9–12 / 13–16 / 17–20. Clearing wave n pays `4 × (12 + 2n)` team gold: 56, 64, 72… This compresses the historical progression into twenty waves. Defeat pays no clear reward; normal clearance pays even after leaks. A final successful clear pays its configured reward.

Rimewatch starter weapons cost 10 gold (Rime/Volt) or 12 (Stone/Ember); their refunds follow the existing 75% rule. Other ordinary roster prices remain unchanged. Ironfold champions cost 750 gold plus 1 wood and retain six owned, standing prerequisites. To keep the now-scarce champion purchase worthwhile, its base damage is three times the earlier 260-gold design and health doubles to 480. This is original twenty-wave tuning, not a claim about historical damage numbers. Their base sale returns 562 gold plus their wood; upgrades require gold only. Siege destruction does not refund wood.

Wood is an individual wallet with a fixed four-unit team grant, split 4 / 2+2 / 2+1+1 / 1+1+1+1. It is charged on successful construction, not when queued. If two queued champions compete for the last wood, the later unaffordable order is skipped without a charge, and subsequent orders continue. A restart clears wood and unlocks.

- Rimewatch: clear wave 9 to earn wood. Spend 1 to permanently unlock another faction's roster through Details or the clickable faction heading above the tower grid. Switching among unlocked rosters is free. Owned towers and queued designs survive a switch. The initial faction selection represents the original starting lumber choice, so the match starts with no unspent wood.
- Ironfold: clear wave 14 to earn wood for champions. These timings adapt the references' pre-wave-15/pre-wave-25 milestones to twenty waves; they are intentionally not presented as original wave numbers.

The wood grant appears in the end-of-wave summary and wallet HUD. Costs and missing wood appear on cards/tooltips. Multiplayer faction changes are authenticated commands; wood and unlocks are included in state digests. Protocol is now `howl-direct-3`; all participants need the new build.

Solo can unlock the three other Rimewatch factions and retain one spare wood. That surplus is deliberately recorded for later design rather than adding an arbitrary resource sink. Further faction options or an optional prestige use can be evaluated after player feedback. No special historical Laser Cannon is claimed to have been reproduced.

## Paid verification

- 72/72 pure simulation cases pass, including exact reward timing, all 1–4-player resource totals, no repeat grants, paid champion construction/refund, queue overspend refusal, unlock ownership, digest coverage and reset.
- TCP checks pass for synchronized milestone wood, refusing unlocks without wood, guest identity enforcement and free roster switching. Existing both-map two-process paid purchase/speed/pause/disconnect tests also pass. This is local loopback, not internet testing.
- A Normal solo compact-invest sweep completed all twelve factions without stalls: eleven victories, Stonebound lost on wave 20. Stonebound then won with 3 lives using a terminal-air sale/reinvestment strategy. Rime Covenant's first sweep win retained 1 life; this is a tight result, not a claim of comfortable balance.
- Four-player mixed Rimewatch won with 22 lives. Three-player mixed Rimewatch explicitly earned/spent wood on additional factions, used their rosters and won with 18 lives.
- An Ironfold Pulse roster campaign won with 30 lives and bought Echo Champion for 750 gold after the wood milestone, before wave 16. Its broad roster strategy used many towers; it is not a crowded-scene performance benchmark.
- These planners are diagnostics, not skilled-player or subjective fun evaluations. The generous inspected Ironfold opening makes these Normal runs substantially easier than Rimewatch. We have retained the evidenced number rather than claiming the two modes now have equal difficulty.

The independent Python auditor passes all 16 recorded campaigns (15 victories), checking 420 player wallets and 420 player income entries. Nine auditor regression tests pass, including rejection of a tampered per-wave bounty schedule.

Raw evidence: Howl-Economy-Pure.txt, Howl-Economy-Network.txt, and Balance/Economy-*.json. Existing art-only regression fixtures that require a full late-game roster now explicitly provide milestone resources; they must not be mistaken for paid campaign evidence.

## World design

Original, map-specific architecture now frames the exterior: six timber lodges around Rimewatch and six industrial workshops plus furnace stacks around Ironfold. Their roofs, foundations, windows, short approach paths, stacked supplies and pennants make the defended area feel connected to a surrounding place. Warm windows and furnace vents contrast with snow/slate or oxidized copper. Nearby buildings cast shadows; distant terrain retains its cheaper treatment.

Pennants move with ambient wind and capped chimney particles drift above the roofs. These are ambient effects and keep their real-time pace independently of match speed. All structures remain outside the playable rectangle; they have no colliders and stay out of the tactical minimap. Flat dark basins now use glacial-pool or foundry-channel textures. Ground receives restrained traffic wear and moss variation without changing geometry or the supplied masks. This remains original procedural prototype art, not final hand-painted assets.

The built-in Unity particle-system module is enabled. The particle shader is retained through a Resources material so player builds can render it. An initial compile failure before package resolution was fixed; final compilation passes with zero errors/warnings. The final both-map Unity atmosphere regression passes (124.80 seconds including Play Mode transitions; CLI aggregate 8.87 seconds). It checks exterior triangle clearance, absent colliders/minimap contamination, roof/path face directions, six buildings/smoke emitters per map, current map economy values and all 76 distinct sound cues. The rendered captures were inspected; inward-facing roofs and intersecting paths found in earlier passes were corrected. A full Unity-suite rerun and new crowded-scene performance measurement are not claimed.

Scheduler stays paused. Mobile remains on hold. No copyright reference art was imported.
