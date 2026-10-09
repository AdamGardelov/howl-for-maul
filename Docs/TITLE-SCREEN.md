# Title screen — 2026-10-09

The game now boots into a dedicated title screen with the original wolf-and-towers logo, a gently orbiting close view of the current map, Play, Settings, Credits and Quit. The old setup panel is behind Play. Map cards identify each setting and lane count; the selected map is still framed beside setup. Gameplay route markers are hidden only on the title screen. Music attribution remains readable over a dark footer and the Credits page includes both Scott Buckley tracks, source and license links. Sound preferences are saved on map unload/exit and loaded on startup. Returning from the title to a paused local match restores its camera; no title-screen simulation ticks are consumed.

## Verification

Two focused Unity integration cases passed: title/world freeze and gameplay-camera restoration, plus both-map setup fit/centring. Linux player built successfully. The final standalone --howl-menu-check fixture passed title/settings/credits, 1440×900 and 960×600 rendering, switching maps, entering solo faction selection and Quit. Actual captures inspected. The fixture calls the same navigation methods as the buttons; this is not a new physical mouse-input test. Final polish hides route markers and improves credit contrast. This is targeted coverage, not a full-suite rerun.

Headless network checks were rerun and passed: both-map automatic waves, all solo factions, wood, shared speeds, password/data rejection, player capacity/votes and two-process paid sessions. No internet relay is implemented or verified yet. No Windows build/runtime claim for this checkpoint. Local Linux player: Builds/Linux-Title/HowlForMaul. Includes automatic waves and protocol howl-direct-4; older Gallery builds are incompatible.

Publishing is deliberately deferred per the user's latest instruction: complete the menu, then relay online flow before distributing to friends. Existing GitHub Releases page has no releases; browser authentication was unavailable during inspection. No files were uploaded and no account or hosting settings changed.
