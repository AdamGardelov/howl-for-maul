# Construction feedback and open intermediate checkpoints

User testing identified an invisible intermediate waypoint blocking otherwise open Ironfold ground. Placement now protects only lane spawns and final exits. Intermediate route hints may be covered by paid towers. When such a point is occupied, ground navigation uses a local area within 1.25 units plus unit radius, with terrain line-of-sight and normal tower clearance. Unoccupied hints retain exact routing. A fully sealed area still uses siege; selling restores the original route. Flight routes, source terrain masks, tower footprints, prices and team income are unchanged.

Every pending construction order now has a full ground footprint and a number in FIFO order. Gold marks the current order; teal marks later orders. Local teams include player labels. Outlines use each queued design's actual dimensions, never become colliders and never reserve gold. They update after completion, skipping, cancellation or a new match, and stay visible while paused. Labels avoid the command HUD. A rejected click leaves its reason visible for five seconds; hovering protected spawn/exit space reveals the nearby protected circle.

QUIT GAME is available in the initial setup and Esc game menu. In a standalone player it exits the process; in Unity it stops Play without closing the editor.

## Verification

All 67 pure simulation cases pass. The new real-map regression purchases towers over intermediate route points on both maps, verifies all ground lanes continue without false siege or clipping, then sells the towers and compares every ground/flight traversal time with the original map. Another regression completely seals a checkpoint area and verifies siege, destruction and eventual exit. Existing wall-seam, half-cell, crowd, queue and economy regressions pass.

The paid compact-invest Normal solo sweep remains 11 wins and one defeat across twelve factions, without stalls or accounting errors. Stonebound still loses on final air wave 20 under this particular strategy. Rime Covenant wins with 12 lives; Ember with 13; Volt and all eight Ironfold factions with 30. Newly legal building sites change the planner's choices; no balance stats were tuned. Full ledgers: Balance/CHECKPOINT-PLACEMENT.json. This is automated paid simulation, not a human campaign or proof of final balance.

Unity compilation and the focused Play-mode queue integration pass. It checks all three queued designs, numbered footprint meshes, pause/menu preservation, completion renumbering, cancellation, rejection feedback and new-match cleanup. The initial assertion inspected the floor mesh by index; it was corrected to select the named queue meshes. Packaged input verification follows separately.

## Packaged construction feedback — source 945da86

Linux-Next and Windows-Next builds succeeded with zero errors. Actual isolated Linux input showed three numbered queued footprints, then three paid towers (1200 → 1185 gold) and no remaining markers. Both game-menu Quit and initial-setup Quit exited zero. Both-map data smoke passed on isolated display :98. Two no-display starts crashed in native PlayerMain before game initialization (139); these are failures, not passes. Windows runtime is untested. The user’s existing player and standard package directory were preserved. New executable: Builds/Linux-Next/HowlForMaul.
