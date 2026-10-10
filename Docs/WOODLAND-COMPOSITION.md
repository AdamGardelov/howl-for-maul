# Woodland banks and quieter greens

2026-10-10. A bounded landscape iteration on the living-world pass.

Ironfold's painted foliage now uses shaded green pigments instead of bright yellow-green accents. Its meadow pigment and exterior close-view detail are quieter too, giving the amber hearths, road and paid defenses more room to stand out. The original source artwork remains unchanged; the new surface color is baked with the updated paint revision.

Isolated little shrubs are replaced by grouped planting: two weathered stones and five overlapping patches of low fronds. Wider shelves get larger groups; narrow shelves get compact versions. A broad planting field leaves open spaces between groups. New eligible tree sites use larger crowns. Four authored targets per half of Ironfold frame the lower bends and base approach: each searches only within 1.5 units for a valid blocked-terrain site, and is omitted if no safe site exists. The same grouping reaches the refuge grounds and Rimewatch, which retains its separate snowy spruce palette. Flowers are rarer accents.

Each entire planting group is reserved before its meshes are generated. The source masks, lanes, economy, tower rules and online flow are unchanged. Wind margins, roof/road exclusions and flying corridors still govern placement, and both halves mirror. No scenery colliders are added. The low frond and stone heights are capped so groups beneath flight paths cannot hide enemies.

This pass does not remodel the refuge halls or legacy forge landmarks. Their remaining simple forms, tower/builder artistry and human first-play/audio review remain priorities; these changes do not establish commercial-release readiness.

## Verification

The first geometry run passed the exterior/baked-map check but failed the existing minimum planting-content assertion: Ironfold had only eight garden elements after oversized groups were rejected by narrow shelves. Compact group placement fixed the content loss. The same unchanged tests then passed 2/2. A packaged review found the base too bare; after the authored tree targets were added, both tests passed again, including full triangle/wind/build-cell checks, tall-flight clearance, roof exclusions, mirrored vertices, actual paid construction, exact wallets, unchanged masks, baked-paint validity and material cleanup across map switches.

Packaged render evidence and build results are recorded in [the verification folder](Verification/Woodland-Composition/README.md).
