# Exit, terrain and wave-planning pass

Rimewatch exit target moves from (31.5, 8.5) to (31, 8.5), the center of its six-cell walkable corridor. Source masks and checkpoint protections remain unchanged. Both the runtime asset and pure simulation reference use the same target.

Terrain materials gain original procedural cliff-edge shading and broad cap color variation. No geometry, colliders or build restrictions are added.

Wave details now offer faction-specific air, rush and swarm suggestions. A team-wide count shows how many armed towers can target the incoming movement type. This count does not promise range coverage or enough damage. Suggestions respect faction availability and champion prerequisites.

## Paid campaign diagnostics

Normal solo, compact-roles strategy, at most 48 team towers. Every purchase, upgrade and wallet is audited. This arbitrary diagnostic restriction is not a game rule.

| Map | Faction | Result | Lives | Waves |
|---|---|---|---:|---:|
| Rimewatch | Rime Covenant | Defeat | 0 | 18 |
| Rimewatch | Stonebound | Win | 4 | 20 |
| Rimewatch | Ember Assembly | Win | 8 | 20 |
| Rimewatch | Volt Vanguard | Win | 30 | 20 |
| Ironfold | Pulse Foundry | Win | 30 | 20 |
| Ironfold | Blast Circuit | Defeat | 0 | 18 |
| Ironfold | Prism Division | Win | 30 | 20 |
| Ironfold | Horizon Guild | Win | 30 | 20 |
| Ironfold | Gravity Works | Win | 7 | 20 |
| Ironfold | Scrap Frontier | Win | 27 | 20 |
| Ironfold | Overdrive Order | Win | 30 | 20 |
| Ironfold | Tidal Array | Win | 30 | 20 |

Rime Covenant purchased 25 Shard Sentries, 15 Aurora Needles and 8 Hail Bells. It cleared waves 1–15 without a leak, then leaked 6, 10 and 14 on ground waves 16–18. No simulation balance stats were changed to force this particular bot to win. Blast Circuit also lost on wave 18 with this role-scoring strategy; the matrix therefore has 10/12 wins. A fresh compact-value Blast follow-up won with 8 lives, 6,700 gold spent and 41 upgrades. This is a separate strategy, not an eleventh win in the role-scoring matrix.

Two-player compact mixed runs: Rime Covenant + Stonebound won with 15 lives; Gravity Works + Scrap Frontier won with 24. Independent wallets and the fixed 1,200 team budget passed ledger checks.

A separate solo Rime maze replay won all twenty waves with 30 lives, spending 5,925 gold on 266 purchases with 303 gold remaining. This is an automated paid campaign, not a manual playtest.

## Verification

62/62 pure simulation cases passed, including centered exit geometry, all lanes, targeting guidance across all twelve factions, wall seams, ownership and crowd routing. Unity compilation passed. The full Unity suite passed 77/77, individually confirmed in XML. A subsequent material-only correction normalizes the shading falloff and removes temporary arrays from the texture-generation loop; final compilation and native 1920×884 visual checks passed on both maps after that correction. Both scenes had zero scenery colliders and valid texture/UV setup. The marker is centered and the sidebar fits. The full suite predates only this material-only correction. Existing packages still contain afe6680.
