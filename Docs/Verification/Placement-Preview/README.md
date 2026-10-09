# Placement and portrait evidence

All images are rendered from the actual Unity models. The two portrait sheets come from the focused Unity render check. Ironfold gameplay/gallery screenshots come from the packaged Linux player using real pointer/keyboard events on a private X display, not concept art.

`Focused-Tests.xml`: four cases passed on the source for this checkpoint. The new cases cover all 76 designs on both maps; the retained cases exercise ordinary shot/pause feedback and portrait/minimap caching and cleanup. Read [the change note](../../PLACEMENT-PRESENTATION.md) for scope.

Screenshot order:
1. Ironfold-Gallery: the chosen faction and the brighter actual-model roster.
2. Ironfold-Valid: 2200 gold, no builds or queued orders, translucent model at cursor.
3. Ironfold-Blocked: rejected terrain, red model and reason; wallet unchanged.
4. Ironfold-Paid: one completed purchase, 2190 gold.
5. Ironfold-Queue: paused, three Shift-clicked orders with all three numbered footprints visible, wallet unchanged.
6. Ironfold-Move: same three orders; moving mode hides the construction model.
7. Ironfold-Queue-Complete: four completed towers, no queued orders, 2160 gold.

The initial interactive display process ended unexpectedly; only the successful repeated sequence above is retained as gameplay evidence. The successful player closed through the normal window close event, exit 0. The renderer is llvmpipe, so these captures do not establish target-hardware frame rate. No new native Windows or separate-network multiplayer claim.
