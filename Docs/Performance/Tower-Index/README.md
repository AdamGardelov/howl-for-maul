# Tower collision spatial index

Matched Unity editor diagnostic, native 1920×884, Ironfold, 324 injected Pulse towers and 74 injected high-health enemies (34 ground, 40 air), speed 1, combat sound off. Each run records 360 unique rendered frames after 60 warmup frames. Both runs use the same initial fixture. This bypasses economy and is not a player strategy or standalone benchmark.

| Measurement | Full tower scan | Spatial index |
| --- | ---: | ---: |
| Simulation mean per executed step | 14.548 ms | 2.914 ms |
| Simulation mean per rendered frame | 10.668 ms | 1.328 ms |
| View synchronization mean per frame | 1.013 ms | 1.010 ms |
| Combat effects mean per frame | 0.131 ms | 0.072 ms |
| HUD mean per frame | 4.239 ms | 4.274 ms |
| Editor frame median | 27.927 ms | 14.253 ms |
| Editor frame P95 | 30.017 ms | 18.561 ms |

Simulation runs at 30 Hz, so the faster render loop has more frames without a simulation step. Per-step cost is total recorded simulation time divided by tick advances, excluding the first sample. Frame windows cover ticks 80–343 before and 57–220 after; faster rendering covers less simulation time. This is a short local comparison, not an identical-tick GPU benchmark or a performance guarantee. All 74 enemies survive both windows. Profiler recorders and the editor add overhead. The global GC counter includes editor/sampling allocations and is not attributed solely to game code.

Markers now separate input, simulation, view synchronization, combat effects and HUD. The initial profile identified simulation collision queries as the larger opportunity; effects averaged only 0.131 ms per frame. Each short navigation query previously tested all towers. Two-world-unit buckets now provide candidate footprints; the existing exact swept-disc geometry still decides collision. Build, sale and destruction maintain the index. Upgrades do not change footprints. Closest-blocker selection retains its original distance and tower-ID tie break.

Verification: 58/58 pure simulation cases and 66/66 Unity cases passed, including wall seams, half-cell sealing/reopening and the captured 61-unit jam. The new independent oracle compares clearance and blocker selection against the original full scan for 20,670 queries across build, removal, destruction and rebuilding, including bucket boundaries, varied footprints/radii, stationary and long segments. Compilation had zero errors/warnings.

Raw before/after CSVs and summary JSON are alongside this note. Hard solo Prism and Hard mixed Gravity/Scrap were rerun through all twenty waves. Every recorded field matches the previous full-scan ledgers exactly: purchases, upgrades, per-wave kills/leaks/ticks/wallets and final totals. See campaign-comparison.json and the unchanged baseline ledgers in Docs/Balance. Packaged builds at source 9e656e8 predate this optimization.
