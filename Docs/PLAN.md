# Implementation plan and architecture

## Initial inspection

The requested parent directory contained another unrelated project, but no existing project for this brief. No inherited AGENTS.md was found in the target's parent chain. .NET SDK 10.0.112 was available; Unity was initially absent. A new independent project was created as `FrostMaze`, targeting Unity 6000.3.25f1 LTS and URP 17.3.0. The user subsequently installed Unity Hub and initiated editor installation.

## Milestones

1. Pure C# grid, exact swept clearance, cached distance fields; automated path/diagonal/reopening tests.
2. Fixed-step units, siege, tower combat, explicit wave/route data, local separation; dynamic maze and crowd tests.
3. Unity procedural scene, RTS camera, construction UI, selection and diagnostic layers.
4. Unity import/compilation and build validation when editor/license becomes available; record exact limitations and manual acceptance steps.

## Boundaries and decisions expensive to reverse

- Simulation owns towers, enemies, health, wave progression, topology revision, and positions. It has **no UnityEngine reference**.
- Grid coordinates and V2 use X/Y; presentation maps simulation Y to Unity Z. One placement cell is one world unit.
- Tower footprints are axis-aligned rectangles, units are discs. Exact swept-disc clearance determines traversability, not Unity physics callbacks.
- Navigation uses a tunable sampled lattice with eight-direction edges and shared destination/radius fields. It can conservatively miss a sub-sample passage; it never authorizes an edge that intersects a tower's clearance boundary.
- All changes rebuild affected cached fields lazily by global topology revision. There is no independent A* per enemy.
- World.Step advances a fixed 1/30-second tick. The same input sequence produces repeatable results on the same runtime. IEEE float operations are **not** a promise of bit-identical cross-platform lockstep.
- World.Build/Sell/StartWave are the local command boundary. Future authoritative networking can validate and order these commands without moving gameplay rules into MonoBehaviours. No networking scaffolding is included.
- Maps and waves are authored through a ScriptableObject containing plain serializable simulation data. The presentation copies it when entering Play.
- Camera intent is abstracted behind ICameraInput for later touch input. Desktop uses Unity's built-in legacy input to avoid an extra package.

## Deliberate scope

One tower, one ground unit, a flying variant, five data-defined waves, a rectangular shared map, unlimited construction, and direct build interaction. No factions, builder character, economy, armor tables, splash, status effects, auras, lobby, persistence, or multiplayer. These were future requirements, not vertical-slice features.
