# GPU instancing experiment — default unchanged

Unity 6.3 documents that identical meshes/materials can sometimes benefit from GPU instancing, but recommends profiling rather than assuming it is faster: [official Unity manual](https://docs.unity3d.com/6000.3/Documentation/Manual/SRPBatcher-Incompatible.html).

On the local GTX 1080, runtime-only material instancing was enabled and SRP batching disabled for a reversible comparison. Both fixtures used native 1920×884, 60 warm-up frames and 180 unique samples per stage. No settings or material assets were changed. Each sequence used SRP, instancing, then SRP again to expose timing drift.

| Fixture | SRP median render | Instanced median render | SRP repeat median render | SRP / instanced draws |
|---|---:|---:|---:|---:|
| 324 paused towers | 8.389 ms | 8.486 ms | 8.792 ms | 4410 / 486 |
| Same towers + frozen 74-enemy combat scene | 10.552 ms | 10.160 ms | 10.175 ms | 5764 / 763 |

The combat fixture was allowed to generate shots, then paused and view updates disabled so both renderers saw identical geometry/effects. It is not the earlier live-moving combat diagnostic. Both screenshot pairs were pixel-identical across 1,697,280 pixels; triangle counts matched. Draw-call reductions did not establish a clear frame-time win, especially against the repeated baseline. Therefore the existing SRP batching configuration is retained. These short editor samples are not standalone FPS or other-platform evidence.

Next: separately measure simulation, view synchronization, input and combat effects. Do not infer the current bottleneck merely from draw counts or the difference between moving and frozen fixtures.
