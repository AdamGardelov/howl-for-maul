# Rimewatch terrain readability

The snow on blocked ledges and exterior terrain now uses broad, low-contrast blue-grey drifts and a subtle grain. This replaces the high-contrast near-white cloud patches visible in the preceding packaged screenshots, so terrain detail competes less with tower silhouettes, plants and lane edges. The rendered north-up minimap inherits the same surface treatment.

Both inner ledges and exterior terrain sample one shared color function. Ironfold retains its existing slate/copper color calculation. This pass changes texture colors only: meshes, collision, supplied masks, buildable cells, wall seals, lane routes, lighting, tower materials and simulation values are untouched.

Verification is recorded below after the existing clearance regression and packaged visual checks. This remains procedural prototype art; no performance or final-art claim is implied.
