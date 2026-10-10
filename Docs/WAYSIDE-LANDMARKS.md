# Working forges and sheltered ward shrines

2026-10-10. Original in-world architecture on the existing landmark sites.

Ironfold's plain furnace boxes become stepped stone forges with recessed arched hearths, copper hoods, banded masonry chimneys and open flue caps. A side anvil, bound fuel stack and broad rivets make their function readable. The painted masonry, copper and banked-ember material language comes from the existing refuge halls. Warm fire anchors and the existing sound/light budget remain in place.

Rimewatch's open shrines gain stone footings, timber braces, metal bindings, swept roof courses and shallow snow ledges. Hanging copper wardbells and clappers sit beneath the roof; small ward crystals retain variation between sites. The roof and timber remain visible beneath the snow rather than becoming an undifferentiated white shape.

Painted textures now project correctly onto both top and side faces. Separate landmark material batches keep copper off tree trunks. The old rock/core stone batch uses the existing slate artwork. Generated ember/wear textures are owned and destroyed by the scenery; materials retain the game's existing lifecycle. No new downloaded or generated reference artwork is introduced.

The common scenery box builder also had its vertical faces wound inward. Their winding is corrected so exterior faces render and light properly. This changes rendering, not the shape of the source-cell walls or any collision/navigation rule. The new reflected-vertex check exposed an old inner-canopy sway mismatch; its phase and horizontal direction now mirror across the map, like the painted foliage shader.

All landmark positions, footprint selection, scale, source masks, flying clearances, team economy, tower mechanics, ownership and online flow remain unchanged. New pieces stay within the original scenic reservation. The packaged world check now includes a close landmark view and uses Move mode to keep a placement ghost from covering the artwork.

## Verification

See [retained evidence](Verification/Wayside-Landmarks/README.md). The composition regression now covers every scenery batch from 24 onward, including the new architecture, and checks reflected vertices, textures/UVs, build-cell and flight clearance. Generated landmark textures are checked for cleanup after map changes. No native Windows, hardware frame-rate, new campaign balance or separate-network claim is made by this art pass.

The legacy grove cores and builder bodies remain visibly simpler than the newer painted architecture. These improvements are a further production step, not a claim that the commercial release bar is complete.
