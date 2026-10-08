# Wall-adjacent construction checkpoint

The user's requirement is that map walls must support sealed mazes: towers can be placed against them, and scenery must not prevent legal construction or let ground enemies slip through a visually closed join.

## Fix

Ironfold's source cells and wall edges are half a world unit apart, but the old placement API, mouse snapping and queued builder completion truncated positions to whole units. The nearest legal tower could therefore leave a half-unit strip beside a wall.

Placement now follows each map's source-cell spacing: 1 on Rimewatch, 0.5 on Ironfold. Tower footprints remain the same size. Fractional coordinates survive purchases, builder queues, overlap checks, point selection, upgrades and sales. The optional grid and hover coordinates show the finer placement spacing. Off-grid requests and partially overlapping footprints are rejected.

Terrain collision, source masks, enemy radii, income and tower statistics are unchanged. Cosmetic meshes have no placement authority: the cursor intersects the ground plane and legality uses simulation footprints. The beveled art remains inset within the blocked mask. A small cosmetic inset around tower models does not create a traversable ground-unit gap.

## Fresh verification

- 57/57 headless simulation tests and 63/63 Unity tests passed.
- A source-map sweep checks hundreds of legal wall-adjacent placements on each map, including Ironfold half-cell boundaries and stepped/corner regions, without terrain overlap and without enough space for the current ground enemy radius at the join.
- Paid half-cell construction and queued construction retain coordinates; duplicate orders, partial overlap, off-grid placement, upgrades and point-based sales are exercised.
- A half-cell corridor fixture buys its sealing tower, checks that the enemy cannot penetrate or pass it, confirms actual siege damage, then sells the tower and checks that the enemy reaches the exit.
- An actual Game-view fixture built a paid Fuse Cadet at (26, 10.5), against an Ironfold wall edge that cannot be reached with whole-unit placement. The wallet fell from 1,200 to 1,190. This was a controlled placement inspection, not a human campaign.
- Two fresh Normal solo adaptive campaigns (Rime Covenant and Pulse Foundry) completed all twenty waves with 30 lives, no stalls and no individual/team wallet errors. These bots still choose whole-unit placements; they check prior strategy compatibility. The new half-cell behavior is covered by the targeted regressions above. Raw results: Balance/WALL-FIT-CAMPAIGNS.json.

The earlier 36-campaign baseline remains historical evidence for the other factions and policies. New half-cell maze designs have not been exhaustively balanced. Online networking remains deferred.
