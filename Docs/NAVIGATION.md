# Navigation and movement decisions

## Global path availability

The tower placement grid and navigation lattice are separate. Tower placement is integer-aligned; the navigation sample spacing defaults to half a placement cell. Samples are at half-step offsets. Each sample has up to eight neighbors. Reverse Dijkstra builds a distance field toward a checkpoint, with a stable-index min-heap tie break. Each node stores its next downhill neighbor.

An edge is traversable only when the **entire swept enemy disc** clears every tower rectangle and stays within map bounds. A conservative swept-bounds rejection first discards distant rectangles. Geometry then tests segment/rectangle-edge distance, including rounded corner clearance. Checking endpoints alone, or rejecting every diagonal whenever either adjacent cell contains a tower, would give incorrect results. Tangency is permitted; penetration is not.

For two diagonally opposite unit towers of Fill 0.6, the diagonal gap is about 0.566 units wide: radius 0.25 passes, radius 0.30 does not. Actual movement and global edge validity use the same geometry function.

Fields are cached by exact destination, radius, breach mode, and tower-grid version. Build, removal and destruction increment that version. Fields are rebuilt lazily; all enemies with the same destination/radius share them. Health loss alone does not rebuild a field because breach weights currently depend on geometry, not remaining health.

Checkpoint seeding and anchoring examine multiple nearby samples and test continuous visibility. This avoids depending on a single nearest node. Lookahead follows up to ten downhill links and picks the farthest continuously clear waypoint. Units move toward that point with bounded acceleration; they do not teleport or snap between cells.

## Blocked routes and tower selection

A finite normal distance field represents an open route. Enemy occupancy is never included in this field: traffic queues cannot mark a route globally blocked.

If an enemy cannot anchor to a reachable normal field, it uses a separate **breach field**. This allows edges through destructible towers with an added configurable cost. Along its downhill route, the first swept obstruction becomes the siege target. It approaches the closest point on the tower rectangle and attacks only within radius plus melee reach. Every tick checks the normal field again, so selling a tower or opening an alternative path immediately cancels siege behavior.

This is a practical heuristic, not an exact minimum-destruction solver: a thick tower can charge multiple edge penalties, several overlapped clearances can charge more than once, and current health is not part of the cost. The chosen tower lies on a weighted route to the goal rather than being an arbitrary globally nearest tower. Target ties are stable by tower ID.

## Local movement and congestion

Velocity moves toward the route intent at configurable acceleration. Nearby same-layer units repel one another. The summed separation force is capped to half movement speed so it cannot cancel route intent in a dense crowd. Each displacement is substepped, checked against swept tower geometry and other enemy discs, then tries axis sliding if blocked. Air and ground units have separate separation layers.

Traffic can queue and lose speed at turns through collision and steering. There is no artificial maze-slow effect. Processing order is deterministic by spawn ID and has an order bias; this is basic local avoidance, not an ORCA solver.

Destinations are small configurable circular triggers. A disc touching the trigger counts as arrival, with a clear segment to the checkpoint required for ground units. Requiring all unit centers to reach a mathematical point caused a dense-crowd contact ring to jam; the trigger models a physical exit region and removes that artifact.

## Limits to evaluate before expanding scope

- The global representation is sampled. Reduce NavigationStep when a geometrically valid narrow passage is missed. A resolution sweep belongs in tuning; this is not a complete continuous visibility graph.
- Field construction checks towers directly and runs synchronously. Large mazes can cause a frame hitch on edits. The small prototype favors correctness; use measured profiling before adding incremental rebuilding or a spatial obstacle index.
- Local separation and collision currently scan the enemy list. The crowd regression exercises hundreds of units, but this is not a frame-budget guarantee on every machine.
- The topology field does not reserve slots for local traffic. Dense interactions may produce less polished motion than a production crowd solver.
- Ground checkpoints and spawn are protected from construction so the destination itself cannot be buried. Surrounding them with towers is allowed and invokes siege.
- Towers' combat targeting is range-based; projectile line of sight is not simulated.
