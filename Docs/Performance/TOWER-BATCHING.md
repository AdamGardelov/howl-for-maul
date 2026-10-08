# Rigid tower rendering batches

Tower base and weapon geometry now combine rigid pieces by material into map-owned, per-design meshes. Repeated towers reuse the same combined meshes. The weapon pivot still turns/recoils independently; upgrade markers remain separate. Original construction nodes remain for inspection but their renderers are disabled. No gameplay, footprints or collision changed.

## Measured editor comparison

Ironfold, native 1920×884 maximized Game view, whole-map overview, paused/no enemies, 60 warmup frames then 180 unique frames sampled from UnityEditor.UnityStats. The crowd fixture reconstructs all 324 positions, designs and visual upgrade levels from the Hard solo Pulse campaign. It bypasses economy for profiling only; the separate paid campaign is unchanged.

| Fixture | Draw calls | Median reported render time | P95 render time | Median reported frame time |
|---|---:|---:|---:|---:|
| Empty before | 731 | 5.67 ms | 6.09 ms | 7.59 ms |
| Empty after | 731 | 5.72 ms | 6.12 ms | 7.67 ms |
| 324 towers before | 6,206 | 11.99 ms | 13.25 ms | 14.01 ms |
| 324 towers after | 5,021 | 10.92 ms | 11.52 ms | 12.94 ms |

Crowd draw calls fell 19.1%; median reported render time fell about 9.0%. These short editor samples do not establish standalone FPS, GPU timings, busy-wave performance or other-machine results. A first after-empty sample used the test runner's smaller Game view and was discarded/repeated at 1920×884. Raw valid samples and summary are beside this file. Submitted triangle counts vary slightly because grouped bounds affect culling; tests verify the underlying triangle count is preserved exactly.

## Verification

64/64 Unity tests passed (25.02 seconds), compilation clean. Extended tests check every rigid triangle is retained once, source renderers are disabled, per-design meshes are shared, restarting a match preserves the cache and map unload destroys it. Existing weapon turning/recoil, pause, upgrades and paid faction tests pass.

A recreated paid seven-tower Pulse lineup was inspected against its pre-batching image. Only 48 of 1,697,280 pixels differed, each by one channel value. This is a controlled visual equivalence check, not a claim about all camera angles or effects.

Linux and Windows rebuilt successfully with zero errors (1 and 19 warnings respectively). Linux packaged both-map smoke passed on temporary :98, exit 0; that display was stopped. Windows runtime remains untested; the prior native Linux :0 X video-mode startup failure remains unresolved.

Next: the minimap still draws static terrain as many individual rectangles. Cache that layer while preserving exact mask pixels and dynamic markers, then repeat matched measurements.
