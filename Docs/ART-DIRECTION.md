# Howl for Maul — visual direction

User direction: League of Legends-like readability and atmosphere, with simpler shapes and less detail. This is an aesthetic reference; all shipped geometry, colors and procedural textures are original.

## First implemented pass

- Continuous terrain replaces the fine checkerboard and stacked rectangular wall seams. The exact source masks still determine the footprint of every ledge.
- Exposed cliff edges have sloping bevels, with pale snow caps against subdued ground. Broad procedural color variation gives the ground some depth without competing with moving enemies.
- Layered snow-covered pines vary in size and placement. Small faceted rocks sit within blocked terrain. Ironfold shares the quieter geometry with a darker steel palette and warm landmark lights.
- Towers use original faceted crown/base meshes, matte materials and restrained faction colors. Winter towers have warmer stone bases and buttresses; their sentries have crenellated crowns. Robots retain their technological silhouette.
- Warm directional lighting and cool ambient fill separate top surfaces from side faces. Existing shot aiming, recoil, tier markers and combat feedback remain.

## First faction model slice

Rime Covenant now has five distinct stone-and-ice tower models. Builders are wardens or compact drones; enemies have faceted shells, swept wings and moving feet. Meshes are original and shared within each map. See RIME-MODEL-SLICE.md for scope and verification.

## What this pass does not establish

This is an in-engine art-direction prototype, not the final asset quality target. The HUD has a first flat-theme/readability pass (HUD-UPDATE.md); the enemy family has an initial silhouette/animation pass, and every faction now has its first distinct tower set. The subsequent terrain and actor material passes add surface detail, while authored sculpting and animation remain future work. We have not replaced the procedural assets with a finished character/model library or added a new animation rig. Static visual inspection does not establish crowded-battle performance on target hardware.

Next visual priorities: clearer combat outcomes, a coordinated enemy silhouette refinement, and further HUD polish after checking smaller viewports. Judge each at normal gameplay zoom before adding more surface detail. Keep silhouettes and ownership colors readable through full waves.

No map, economy, faction roster, combat balance or pathfinding changes belong to this visual pass. No copyrighted game assets were imported.

## Graphics quality follow-up

See Performance/QUALITY-PASS.md for 2× MSAA, medium soft shadows, matched measurements and the packaged 1440×900 Linux mouse check. Start/build/select/upgrade are verified on a virtual display; native desktop and Windows runtime limits remain explicit.

## Complete first roster pass

All 76 tower designs now have faction-specific procedural models. Their paid progression, collider absence and silhouettes are verified. This is a foundation for further materials, animation and composition work, not a claim of finished League-quality visuals. See IRON-MODELS.md.

## Inhabited borders

See MAUL-ECONOMY-IMPLEMENTED.md for the original lodge/foundry exterior pass, snow and copper roofs, warm windows, wind pennants, chimney particles and muted water textures. This improves environmental context; a complete authored art library and crowded-scene performance pass remain future work.

## Material identity

See TERRAIN-MATERIALS.md for snow/slate/ice shelves, mossy foundry stone, weathered cliff faces, grass tufts, slate fragments and textured distant ridges. Large material patches are emphasized over busy small-scale outlines; repeat seams are regression-tested for both palettes.

## Tower and builder detail

See ACTOR-POLISH.md for chamfered tower parts, stone/metal/cloth surfaces, faction foundation markings and twelve equipped, animated builder variants. Rimewatch wardens and Ironfold robotic artisans have separate owner rings, so shared faction colors remain usable in co-op. The original design-specific tower silhouettes and actual aiming/recoil pivots remain. Both close-up in-map rendering and the entire roster were checked; the art remains procedural and no new crowded-scene performance claim is made.
