# World overlays only repaint

Minimap dots, health bars and map labels now skip GUI layout/input events. They contain no controls or GUILayout calls. The interactive sidebar still receives all events; minimap panning remains in Prototype input handling.

Native 1920×884 paused Ironfold fixture: 324 injected towers, 74 injected enemies, tick 0, 60 warmup + 360 unique frames. Both screenshots are pixel-identical across all 1,697,280 pixels. Compilation passed without errors. No new simulation change or additional full-suite claim.

HUD median was 3.431 ms before and 3.394 ms after; editor frame median 12.148 vs 11.968 ms. This small difference is within plausible local variation, not a substantial performance claim. Global per-frame allocated-byte median fell from 62,271 to 61,139; this includes editor and sampling allocations. The change avoids unnecessary scans/projections while preserving the image. CSVs and summaries are included.
