# Readability and stonework follow-through

## Health and inspection

Health bars reserve screen space in this order: selected units, damaged towers, siege attackers, other damaged enemies. Overlapping unselected bars are suppressed by default. Selected bars keep their full width and have a pale border; selected healthy towers now have a bar too. Hold **Alt** to reveal every visible unit's health, including healthy units and overlapping bars. Selection is painted last, so it stays readable in reveal mode. The layout reuses storage and stays clear of the HUD/minimap. The help menu and README list the shortcut.

Ctrl-click enemy inspection now measures the visible model in screen space before ground picking. The old ground-plane distance check could miss hovering enemies at low camera angles and could select a tower beneath a flying unit. The new picker accounts for model height, projected size and zoom, rejects offscreen points, and preserves camera gestures and UI protection. Normal tower/build clicks retain their behavior. Hovering an existing tower for inspection no longer paints a rejected-placement red square underneath it; explicit Remove mode retains its red marker.

## Terrain materials

The preceding procedural texture pass used `Mathf.SmoothStep(low, high, value)` where it needed a threshold mask. Unity interpolates the two output values; it does not remap the input interval. That washed out the intended flagstone joints, selective frost and wall courses. `PaintMask` now normalizes through `InverseLerp` before smoothing to zero–one.

Rimewatch retains cool flagstone, snow over blue slate and frost-worn edges. Ironfold retains darker paving, weathered plum-grey masonry and sparse moss/dust. Seams and courses now have their intended local contrast. These are texture corrections: source masks, scenery footprints, geometry, colliders and build rules are unchanged. The both-map clearance and minimap tests pass. Visual inspection/package evidence follows below when completed.

## Verification

- All 64 pure simulation cases passed.
- Camera gestures, zoom/picking and protected UI passed the existing integration case.
- Scenery footprint checks passed both maps, including Ironfold half-cell edges. No scenic colliders were added.
- All 76 actual-model tower portraits, both rendered minimaps, unchanged match data and texture cleanup passed.
- The new health case passes selected healthy-tower visibility, selected-enemy priority, suppression of overlapping bars, reveal mode including healthy units, HUD clipping, offscreen rejection, and zero managed allocation in 100 warmed layout calls.
- The new inspection case passes ground and flying model picking at zoom 5, 11 and 24, before and after camera rotation. It explicitly verifies a close flying target whose ground-plane intersection is more than one unit away, preserving terrain transforms, gold, ticks, tower count and builder queue.

The initial health fixture tried to overlap enemy spawns, and the simulation correctly refused it. The corrected fixture uses legal spacing and resolution-aware camera distance to produce overlap in the projection. The final rerun passes; the original failure and all five final case outcomes remain in Howl-Readability-Unity-Tests.json. This is focused verification, not a new full-suite or frame-rate claim.

Paid-defense work is separate: COMPACT-INVESTMENT.md in Balance records 11/12 solo wins with the new strategy, including the first compact Rime win, plus two mixed-pair wins. Across the saved strategies all twelve factions now have a Normal compact solo win. No balance numbers were changed.
