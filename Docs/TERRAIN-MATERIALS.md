# Terrain materials — 2026-10-09

This pass gives blocked shelves, cliff faces and the surrounding landscape more material identity while keeping the playable lanes as readable flagstone. Geometry, economy, supplied layout masks and navigation rules remain unchanged except for additional decorative grass and slate fragments inside existing safe plant footprints.

Rimewatch uses broad snow coverage broken by exposed blue-grey slate, scoured ice, pale frost rims and sheltered lichen. Cliff faces have bent sediment bands, narrow fractures, ice seepage and frosted crowns. Grass tufts are short, desaturated and tipped with frost; chips of slate catch a pale highlight.

Ironfold replaces the flat purple slate wash with weathered grey-green rock, moss/grass patches, ash-darkened areas and oxidized mineral stains. Its masonry retains readable courses with damp moss near the base and rusty streaks. Small grass tufts and stone fragments give the raised shelves depth without placing props in the lanes.

Inner shelves and exterior ground share the same periodic world-space paint function. A narrow 0.35-unit join band eases to zero slope at repeated texture borders so fine mineral detail does not create a visible join. Distant ridges now receive a weathered rock texture too. Wall UV height is normalized to each theme's actual wall height, so frost crowns appear at the top instead of cycling vertically. The wall texture clamps vertically and repeats horizontally. Fine grain remains subordinate to the larger material patches; no imported game art is used.

The first both-map scenery/atmosphere runs passed their geometry checks, including the added grass and slate fragments. The initial texture-seam regression failed on excessive fine-detail variation near a repeat boundary; the texture refinement addresses that without relaxing its tolerance. Overview inspection also prompted softer broken rock seams, less regular ice streaks and quieter distant ridges. Final seam/visual rerun and package results will be recorded below. This is original procedural prototype art, not an authored final environment library.

Final compilation reports zero errors/warnings. The unchanged strict repeat-seam test now passes for both palettes, including negative world coordinates and either side of the repeat boundaries (1.98 seconds). The scenery-clearance test passed before the final color-only refinement (142.84 seconds); its geometry was not changed by that refinement.

The final both-map atmosphere test passes (137.16 seconds including Play Mode transitions; CLI aggregate 13.96 seconds). Its actual close-up and overview camera renders were inspected. Exterior geometry remains outside the playable rectangle with no colliders or minimap contamination; the retained six-building/smoke setup and 76 distinct tower sound cues also pass. This is focused coverage, not a full suite or new performance benchmark.

Linux package: `Builds/Linux-Terrain/HowlForMaul`, runtime source `de1c49e`. Build succeeded with zero errors and one Pipeline-runtime-disabled warning. Both packaged map/resource/route smoke checks pass with a clean exit on Xvfb/OpenGL. Smoke mode skips presentation; the texture images were verified in the separate editor camera captures above. Windows-Maul remains the earlier economy/world package; it has not been rebuilt for this material/fire pass.

```sh
/home/adam/Documents/Dev/howl-for-maul/Builds/Linux-Terrain/HowlForMaul -force-wayland
```
