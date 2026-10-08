# Placement hints

A small map tooltip shows the selected tower's cost before purchase, or the existing authoritative build-validation reason when a footprint is blocked. Hovering an existing tower instead says it can be inspected. The hint uses the validation already performed for the placement ghost; it does not issue orders, change terrain or add another navigation query. It hides during camera gestures, setup, finished matches and while the pointer is over UI. Move/sell modes keep their existing behavior.

The tooltip is bounded to the visible world area and moved above the minimap when needed. It draws only during repaint. This is a UI aid, not a change to placement legality, checkpoint protection, wall clearance, wallets or construction timing.

Compilation passed. Native 1920×884 screenshots were inspected for an actual blocked-checkpoint reason and a legal wall-adjacent Shard Sentry price. These are staged read-only hover fixtures; no new click-through or full regression run is claimed for this text-only follow-up. Packages still contain the separately verified camera checkpoint 9368630.
