# Permanent minimap terrain cache

The HUD now draws permanent terrain using one cached point-filtered texture instead of a rectangle per terrain block. Rimewatch uses 64×64 pixels, Ironfold 128×128, matching their source cell sizes. Dynamic tower/enemy/builder markers, camera outline and click-to-pan remain separate and unchanged. Match replacement/map changes release the old texture. Tower placement does not invalidate permanent terrain.

## Verification

Final compilation clean; final full Unity run **65/65 passed** (25.07 seconds). The new regression compares all 20,480 pixels directly against the two authoritative ASCII masks, including north/south orientation, checks cache reuse after building, and checks texture destruction on map replacement. Intermediate test-authoring errors (an integer-only helper call, then a zero-radius probe) were corrected before this final run. No source masks or collision rules changed.

Native 1920×884 Ironfold and Rimewatch captures were inspected. The cached shapes match both layouts, retain live markers and remove thin seams from the old individually rasterized rectangles. The Ironfold capture recreates the paid Pulse lineup, paused, with its hover ghost hidden; the winter capture shows the full map. These are visual fixtures, not human playthroughs or mouse-input automation.

## Matched editor measurements

Same 1920×884 Ironfold overview and paused 324-tower rendering fixture as TOWER-BATCHING.md; 60 warmup frames, 180 measured frames. The fixture uses campaign positions/designs/levels but bypasses economy for profiling only.

| Fixture | Draw calls | Median reported render time | P95 render time | Median reported frame time |
|---|---:|---:|---:|---:|
| Empty, before cache | 731 | 5.72 ms | 6.12 ms | 7.67 ms |
| Empty, cached | 115 | 2.82 ms | 3.16 ms | 4.87 ms |
| 324 towers, before cache | 5,021 | 10.92 ms | 11.52 ms | 12.94 ms |
| 324 towers, cached | 4,405 | 8.29 ms | 9.21 ms | 10.33 ms |

The terrain cache removes 616 draw calls; median crowd render time fell about 24%. Across tower batching and this cache, the original 6,206-draw-call crowd fixture is down to 4,405 and median reported render time from 11.99 to 8.29 ms. This does not establish standalone FPS, GPU timing or crowded-combat performance. Raw samples and summary are beside this document.

Packages remain the preceding tower-batching checkpoint aa500eb, which includes all winter sets plus Pulse and Blast. This minimap change is source/editor-only until the next build. Windows runtime is untested; the prior native Linux X video-mode issue remains unresolved.

Next: test modest anti-aliasing and soft shadows against the same performance fixture, then continue the six remaining Ironfold model sets and broader paid-defense coverage.
