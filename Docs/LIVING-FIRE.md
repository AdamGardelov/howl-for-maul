# Living map fire — 2026-10-09

Map braziers now have independently swaying and stretching flame silhouettes, additive soft halos, rising fading embers, and flickering point lights. Rimewatch retains cyan-blue magical fire; Ironfold uses orange-gold forge fire. Effects are original procedural geometry and a generated radial texture using the already-retained particle shader.

Ambient fire uses unscaled real time, including during pause and in menus, so changing combat speed does not turn torches into fast-forward animations. Flame deformation stays within the existing blocked-terrain placement margin. Decorative effects add no colliders and issue no simulation commands. Halos and particles stay out of the layer-30 tactical map capture.

The renderer selects at most four nearby on-screen braziers for actual lighting, with shadow casting disabled. Every brazier has at most ten ember particles; scenery generation already caps braziers at 24. Glow is local additive rendering, not a global bloom filter over gameplay/UI. This is a bounded budget, not an FPS claim.

Unity compilation reports zero errors/warnings. The updated MapLandmarksNeverCoverWalkableCells integration test passes on both maps (132.07 seconds including Play Mode transitions; CLI aggregate 12.75 seconds). It checks paused-world animation without advancing simulation, live ember emission, the four-light/ten-particle limits, no scenery colliders, and every landmark triangle against the supplied walkability masks. Both actual camera captures were inspected. An initial run used the old test DLL and was not counted as fire-specific evidence; the new assertion string was confirmed in the compiled DLL before the recorded run. A full test-suite rerun and FPS benchmark are not claimed.


Linux package: `Builds/Linux-Fire/HowlForMaul`, source `8b587e6`. Build succeeded with zero errors and one warning (Pipeline runtime API disabled). The packaged resource/route smoke passes on the isolated Xvfb/OpenGL display with clean exit; smoke mode skips presentation, so the visual evidence is the separate Unity camera captures and integration test. Windows-Maul remains the preceding economy/world package and does not include this fire pass.

```sh
/home/adam/Documents/Dev/howl-for-maul/Builds/Linux-Fire/HowlForMaul -force-wayland
```
