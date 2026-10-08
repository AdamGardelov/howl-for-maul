# Prism Division model pass

Seven original procedural models replace the generic Ironfold role shapes for faction three:

| Design | Readable feature |
|---|---|
| Shade Cadet | Faceted hood, narrow visor, compact blaster |
| Spark Herald | Paired raised conductors and bright terminals |
| Lodestone | Broad magnet poles joined by a low bridge |
| Coil Serpent | Rising segmented coil and forward head |
| Prism Twin | Paired sky lances and targeting lenses |
| Needle Guard | Three rear needles, helmet and shield |
| Shell Champion | Broad carapace, paired cannons and central prism |

All use the existing shared material/mesh batching and shot-facing weapon pivot. Cosmetic parts add no colliders; supplied terrain masks, tower footprints, pathfinding and prices are unchanged. Spark Herald retains three chain targets; Prism Twin remains air-only. Champion progression still requires all six regular designs, with paid upgrades.

Verification: zero compilation errors/warnings; 65/65 Unity tests passed. The extended paid progression test buys all seven models, checks cosmetic collider absence, chain count and air-only targeting, then upgrades the champion to level 2 with 245 gold remaining. Native 1920×884 captures were inspected at orthographic zoom 7 and normal zoom 11. These are paused lineup fixtures, not a live battle or final-art claim. Packages remain the preceding quality checkpoint; Prism is verified in the editor but is not in those packages yet.
