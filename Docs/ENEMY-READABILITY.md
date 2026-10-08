# Enemy silhouette refinement

Ground roles keep their existing behavior and classification. Ordinary crawlers retain their rounded orange crest; fast runners have a narrower shell and two swept crystal fins. Slow/heavy siege units have a broader, taller bevelled shell, a flat crest and raised front shield. Flyers retain their violet swept wings and flight height. No renderer count was added per unit; the new armor mesh is shared in the map-owned mesh cache.

This is a restrained original procedural pass, not a finished character library. Collision discs, health, speed, damage, routes and difficulty scaling are unchanged. The slow-speed classification keeps a heavy silhouette on Relaxed difficulty even when siege damage is reduced.

## Verification

The existing EnemyPresentationTracksSimulationAndResets case passed (1/1). It checks ordinary, runner, Relaxed-heavy and flying subjects, walking/flapping pause/resume, facing, slow status, flight height and new-match cleanup. Collider checks now explicitly include the runner and heavy. Raw result: Howl-Enemy-Presentation-Test.json.

Visual inspection then prompted a final cosmetic refinement from a plain box hull to the cached bevelled armor mesh. That mesh-only follow-up compiled cleanly and was inspected at native 1920×884 close and normal gameplay zoom; the focused test was not rerun after the bevel adjustment. The last complete regression suite remains 71/71 at 05cad27, with later focused UI/enemy results recorded separately.

The captures are staged four-enemy lineups, not wave playthroughs. No FPS gain or newly verified platform is claimed. Package source provenance is in Howl-Builds.json.
