# Rimewatch terrain readability

The snow on blocked ledges and exterior terrain now uses broad, low-contrast blue-grey drifts and a subtle grain. This replaces the high-contrast near-white cloud patches visible in the preceding packaged screenshots, so terrain detail competes less with tower silhouettes, plants and lane edges. The rendered north-up minimap inherits the same surface treatment.

Both inner ledges and exterior terrain sample one shared color function. Ironfold retains its existing slate/copper color calculation. This pass changes texture colors only: meshes, collision, supplied masks, buildable cells, wall seals, lane routes, lighting, tower materials and simulation values are untouched.

Unity compilation and all 68 existing pure simulation cases passed. The both-map scenery clearance regression passed in 105.58 seconds before the periodic-noise refinement; it verifies every decorative triangle stays on blocked cells. No geometry changed in the refinement. Packaged checks follow below. This remains procedural prototype art; no performance or final-art claim is implied.

The first packaged inspection revealed visible repeat boundaries in the exterior snow. The refinement makes every snow noise layer periodic over the exterior's 64-unit repeat, with smooth blending across both axes. A focused Unity test covers matching opposite edges, translated samples and continuity on either side of seams, including negative world coordinates.
