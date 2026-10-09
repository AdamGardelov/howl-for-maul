# Setup preview framing — 2026-10-09

The setup camera fits and centres the projected map bounds in the space to the right of the menu, with proportional padding. It recomputes after window resizing. This changes only the rendered setup view; the gameplay focus, rotation and zoom are restored when returning to a match. Map masks and simulation are unchanged.

Verification: focused Unity EditMode/EnterPlayMode regression passed (1/1, both maps). It checks corner containment, centring within three pixels and restoration of the gameplay camera. Linux build succeeded. Actual Linux/OpenGL player previews inspected for Rimewatch and Ironfold at 1440×900 and after resizing to 960×600; maps remain fully visible beside the menu. Quit Game exited normally. This is targeted verification, not a full-suite rerun. Windows was not rebuilt or runtime tested for this change.

Current local player: Builds/Linux-Preview/HowlForMaul. Includes the preceding original logo and automatic-wave source changes (protocol howl-direct-4). Older Gallery packages remain preserved and are incompatible online. Automatic-wave simulation/network evidence is in AUTOMATIC-WAVES.md; a full timed campaign and final timer GUI playthrough remain outstanding. Menu logo inspected in these setup captures; other logo-bearing menu screens still need final standalone inspection.
