# Construction during combat: freeze fix

The player reported intermittent freezes while building during an active wave. Routing reproduced the problem: every completed build, sale or destroyed tower discarded all flow fields. Each rebuilt destination scanned every tower for every navigation edge. A single edit therefore repeated the same collision work across the whole map and generated large temporary route arrays.

## Change

The navigation graph now caches permanent terrain clearance, neighbor indices and exact edge lengths. Each enemy radius shares this graph across destinations and normal/breach routes. A tower edit refreshes collision counts only around the changed footprint, using the existing spatial buckets. Distance/next buffers and heap storage are reused; typed cache keys remove string allocations from each enemy query. Seed searches visit a conservative rectangle around the destination.

Full distance propagation still runs synchronously when a field is requested after an edit. Enemies never use a deliberately stale route. First use of a radius, or a permanent terrain edit, still performs full collision preprocessing. This trades persistent per-radius graph memory for much less repeated CPU work and garbage collection. The topology cache lives with its simulation world.

Breach costs, route tie-breaking and floating-point edge distances retain the original behavior. An independent full-scan reference checks exact path/cost equality across tower construction, destruction, sale, large footprints, multiple enemy radii, diagonal gaps and permanent terrain changes. Existing wall-tight placement and maze rules remain unchanged.

## Measured result

Synthetic routing stress test: 160 towers, all unique route destinations (13 Ironfold / 9 Rimewatch), radius 0.2, eight warmed tower edits. Unity's bundled Mono runtime, same machine:

| Map | Previous median per edit | Updated median per edit |
|---|---:|---:|
| Ironfold | 1,856.64 ms | 17.41 ms |
| Rimewatch | 1,240.56 ms | 12.49 ms |

This is a CPU routing comparison, not a whole-frame or FPS measurement. The independent paid gameplay check builds during the real first wave: Ironfold grows from 48 to 56 paid towers with 32 active enemies, and Rimewatch from 8 to 11 with 24 active enemies. Construction simulation steps measure 17.96–18.91 ms and 6.82–10.48 ms respectively; view synchronization adds 1.91–2.41 ms. These figures do not certify arbitrary large crowds or target hardware.

Warm flow queries allocate zero in the 10,000-call regression. The external Mono benchmark includes its own stopwatch allocations (40–100 bytes/edit), versus roughly 1.2–1.7 MB/edit before. The checked-in benchmark reuses its stopwatch.

## Reproduce

```sh
dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --navigation-benchmark
```

Unity: run `ConstructionHitchTests.PaidConstructionDuringLiveWavesKeepsRoutesAndViewsWorking`. Its JSON CPU samples appear in `Logs/ConstructionHitch/report.json`. The test also checks charged gold including combat income, live enemy clearance, bounded collision work and actual rendered tower identities. It deliberately has no fragile universal millisecond threshold.

[Verification, raw results and limits](Verification/Construction-Hitches/README.md). Local Linux/Windows candidates and [Friends Playtest 3](https://github.com/AdamGardelov/howl-for-maul/releases/tag/v0.1.0-playtest.3) include this fix; earlier releases remain unchanged. Use matching builds for multiplayer.
