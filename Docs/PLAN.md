# Implementation plan and architecture

## Initial inspection

The requested parent directory contained another unrelated project, but no existing project for this brief. No inherited AGENTS.md was found in the target's parent chain. .NET SDK 10.0.112 was available; Unity was initially absent. A new independent project was created as `FrostMaze`, targeting Unity 6000.3.25f1 LTS and URP 17.3.0. The user subsequently installed Unity Hub and initiated editor installation.

## Milestones

1. Pure C# grid, exact swept clearance, cached distance fields; automated path/diagonal/reopening tests.
2. Fixed-step units, siege, tower combat, explicit wave/route data, local separation; dynamic maze and crowd tests.
3. Unity procedural scene, RTS camera, construction UI, selection and diagnostic layers.
4. Unity import/compilation and Linux/Windows build validation completed.
5. Shared-defense expansion: connected areas, hovering builder, economy, lives and ten-wave match; simulation and real Play-mode integration tests.

## Boundaries and decisions expensive to reverse

- Simulation owns towers, enemies, health, wave progression, topology revision, and positions. It has **no UnityEngine reference**.
- Grid coordinates and V2 use X/Y; presentation maps simulation Y to Unity Z. Placement uses world units: one-unit cells on Rimewatch and half-unit positions on Ironfold.
- Tower footprints are axis-aligned rectangles, units are discs. Exact swept-disc clearance determines traversability, not Unity physics callbacks.
- Navigation uses a tunable sampled lattice with eight-direction edges and shared destination/radius fields. It can conservatively miss a sub-sample passage; it never authorizes an edge that intersects a tower's clearance boundary.
- All changes rebuild affected cached fields lazily by global topology revision. There is no independent A* per enemy.
- World.Step advances a fixed 1/30-second tick. The same input sequence produces repeatable results on the same runtime. IEEE float operations are **not** a promise of bit-identical cross-platform lockstep.
- World.OrderBuild/MoveBuilder/Build/Sell/StartWave are the local command boundary. Future authoritative networking can validate and order these commands without moving gameplay rules into MonoBehaviours. No networking scaffolding is included.
- Maps and waves are authored through a ScriptableObject containing plain serializable simulation data. The presentation copies it when entering Play.
- Camera intent is abstracted behind ICameraInput for later touch input. Desktop uses Unity's built-in legacy input to avoid an extra package.

## Current scope

Two supplied map masks; all three/four lanes active; one to four local player slots with independent owners/builders and a fixed team economy; twelve exclusive factions and 76 original procedural tower designs; paid construction/upgrades, champion prerequisites, slow/splash/chain combat, ground and flying enemies, twenty waves and three difficulties. World feedback, wave-income recaps and off-camera breach warnings are implemented. The original diagnostic lab remains available.

The folder is now howl-for-maul. Online networking, armor tables, research/lumber and save/load remain deferred. Read README.md for current controls, HOWL-VERIFICATION.md for evidence and limits, MAUL-RESEARCH.md for reference decisions, and QUESTIONS.md for nonblocking open decisions. Earlier milestone text above is historical, not a current feature inventory.
