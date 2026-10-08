# Close inspection camera and themed terrain

Mouse-wheel zoom eases between a steep overview and a lower, perspective close view. At the closest zoom the camera pitch is 38 degrees and vertical field of view is 46 degrees; at overview distances these become 62 and 16 degrees. Perspective stays enabled throughout, avoiding a projection-mode switch. The narrow overview lens keeps the map readable, while close views reveal model depth. This is an RTS inspection camera, not first-person/free flight.

Q/E orbits around the current ground focus. Terrain, tower positions and navigation remain fixed, and the rendered minimap stays north-up. Home restores north-facing builder focus; End fits all four projected map corners with UI margins. Space + left-drag and middle-drag compare two ground-plane ray intersections, preserving the grabbed point at different viewing angles. Edge/keyboard panning follows camera orientation, respects bounds and retains Shift acceleration. Setup/menu input protections remain. Health bars now measure projected world size in perspective.

A proportional near clipping distance avoids loss of placement-ray precision at distant overview positions. The internal Camera.orthographicSize value is retained as a focus-plane half-height/zoom scale for existing inspector fixtures; the rendering itself is perspective.

## Original surface treatment

Both maps use deterministic 1024×1024 painted ground and cap textures with mipmaps, plus a small repeating wall texture. The textures are generated from noise, offset paving courses, worn joints and the source-mask edge distance. The ground stays flat: seams, frost and moss are paint, not additional obstacles. Wall faces now have a lower bevel, upright stone and worn crown, with a thin inset cap trim. All new wall geometry remains within blocked cells.

Rimewatch uses frost-worn green-blue flagstone, snow over blue slate, pale edge trim and cool daylight. Ironfold uses muted green-grey paving, plum-grey masonry, copper-toned trim and warmer daylight. Its former plain posts are now tapered octagonal forge pillars with footings, crowns and thin bronze bands. Existing frost plants, snow trees and cyan lanterns remain distinct from Ironfold's sparse dry scrub and orange braziers. Leaf veins improve close-up plants. Small seals on winter tower foundations and fasteners on robotic foundations reuse existing base materials and batched meshes.

Navigation masks, all-active lanes, freeform building, collision rules, costs and tower stats are unchanged. All generated textures and meshes follow map cleanup. This remains original procedural prototype art, not a finished hand-painted asset set or a claim of League-quality production art.

## Verification

All 64 pure simulation tests passed. Three focused Unity integration checks passed (complete NUnit per-case results in Howl-Depth-Tests.json): camera picking/dragging/rotation/zoom/UI protection, both-map scenery footprint clearance, and both-map minimap plus all 76 tower portraits with cleanup and unchanged match state. The first camera pass exposed a 0.00269-unit overview ray error; proportional near clipping fixed it and the unchanged 0.002 tolerance passed. The earlier three other Camera-filtered cases passed before this clipping correction. No new full-suite claim.

The subsequent Ironfold pillar/palette refinement compiled and passed the both-map clearance case again (Howl-Depth-Clearance-Tests.json). Editor close/overview views were inspected on both maps; the staged Rime lineup paid 190 gold for five towers, and Ironfold paid 435 for six. Final player inspection follows the pillar refinement. Historical package checks retain their source provenance in Howl-Builds.json.
