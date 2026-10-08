# Themed plants and braziers

Rimewatch has low, broad frost ferns with pale tips and small icy blooms, plus stone lantern bowls with cyan flames. Ironfold has copper seed heads and dry scrub with warm orange braziers. These are original faceted meshes generated in code; no reference-game assets are imported.

Dressing is sparse and deterministic, with varied plant sizes and jittered positions to avoid straight rows. Candidate locations must have their entire 1.52-world-unit square on raised blocked terrain, excluding water and depressions. Existing rocks, trees and landmark posts are avoided. Most clusters sit near lanes, with a few deeper plants breaking up large wall caps. A map has at most 90 plant clusters and 24 braziers.

All five new material groups are batched across the map. There are no per-prop GameObjects, colliders, dynamic lights or particle systems. Two flame layers use unlit materials; the outer layer has a subtle bounded brightness pulse that stops when the game is paused. Materials remain owned by Prototype and meshes by MapScenery, so map changes use the existing cleanup path.

Source masks, construction validation, routes, economy, tower stats and enemy simulation are unchanged. The extended MapLandmarksNeverCoverWalkableCells regression requires the new batches to exist and tests every triangle's horizontal bounds against both authoritative masks. Even a decorative mesh spanning a walkable cell fails this check.

Final compilation passed. The extended clearance regression passed on both maps after the position/size refinements, confirmed in Unity XML. Native 1920×884 overview and close-up captures were inspected. Rimewatch contains 58 plant clusters and 9 lanterns; Ironfold has 8 scrub clusters and 5 braziers, with vegetation intentionally sparse among the existing industrial posts. Live fixtures on both maps confirmed zero scenery colliders/lights, changing flame brightness while unpaused, and no brightness change during pause. The previous full suite was 77/77 before this scenery-only change; it is not a new full-suite claim.
