# Mirrored maps and clear base approaches

The requested October 9 layout revision mirrors the left half of both maps onto the right. Ironfold's isolated base blocks and wall-end projections are removed, leaving a flat wall on either side of the centered exit. The supplied masks are retained unchanged in `ReferenceLayouts/`; the active `LayoutSources` and serialized map assets contain the new geometry.

Ground terrain, pit outlines, static scenery and outside scenery are reflected together. Triangle clipping at the center avoids overlapping center-spanning surfaces; reflected triangle winding preserves outward faces. Texture sampling is mirrored too. Fire and landmark anchors are paired; ambient flames and foliage still animate naturally. These are real terrain changes, so building and navigation see the same cleared areas as the player.

Both exits now sit at world X=32. Opposite lanes and starting positions are reflected, and Rimewatch's center route is centered. All three/four lanes remain active. Factions, waves, rewards, team budgets and tower stats are unchanged. The explicit editor method `FrostMaze.Editor.LayoutRefresh.Refresh` updates only geometry and starts in the existing assets; it never runs automatically.

## Verification

- Linux build succeeded. Packaged `--howl-map-check` passed and all four overview/exit captures were inspected. Both-map packaged data/route smoke passed with a clean exit. Updated local player: `Builds/Linux-Title/HowlForMaul`.
- 73/73 pure simulation cases pass, including every mask cell, exact horizontal symmetry, the clear Ironfold approach, every lane's traversal, paid defenses, wall seams and the wider shared Rimewatch exit maze.
- Two focused Unity integration cases pass across both maps: scenery/buildable-ground clearance and composition/flight clearance, paired landmarks, bounded living fires, paid construction and ambient behavior. These passed in separate final runs (scenery in attempt 3; cohesion in attempt 5), not a complete Unity suite.
- The captured historical 61-enemy corner regression intentionally uses the archived original Ironfold geometry and routes. Current-layout lane/maze checks use the revised maps.

Earlier fixture failures exposed stale exact landmark/grove counts and camera/timing assumptions. Fixtures now verify paired landmarks, tolerate the revised grove count and step movement explicitly. Bounded ember emitters now simulate while culled, preventing paused/offscreen fires from remaining empty. One invocation selected PlayMode for editor-hosted tests and ran zero cases; it is excluded from passing evidence.

Full twenty-wave faction balance has not been rerun after this layout change. Older balance ledgers describe their original geometry, not this revision. Windows runtime and internet/relay play are not newly verified. Players must use matching builds/map data.
