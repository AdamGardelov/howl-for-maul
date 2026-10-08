# HUD readability pass

The match HUD now keeps gold, team lives, wave progress, launch, pause, speed, setup controls and action/error messages above the scrolling content. Construction cards show hotkey, name, price, combat role, ground/air targeting, and a text indication for locked or unaffordable options. Selecting an unavailable design still allows inspection of its stats and prerequisites.

Selecting a built tower scrolls its inspector into view near the top, showing current health, level, combat stats, the next upgrade and its cost. Upgrade, sale and deselection are grouped there. The existing simulation APIs still enforce ownership, affordability and prerequisites.

The sidebar is slightly wider and uses a flat dark palette, muted green selection states and warm resource values. Its input exclusion and camera viewport use the same updated width. Setup choices share the new button styling. Wave details and advanced inspection are collapsible; the reset action lives under advanced inspection rather than alongside routine wave controls.

## Verification scope

Actual 1920×884 Game-view captures were inspected for setup, Ironfold's seven-card construction roster, and the selected-tower upgrade panel. The fixture purchased a real tower. The screenshots show readable cards, a visible locked champion, and the upgrade action above the roster. Content below the available area remains scrollable; the match controls stay fixed.

A GUI event injection attempt did not activate the upgrade button. The Pipeline pointer command reported that legacy input injection is unsupported and the Input System package is absent. No input package or project input settings were changed. A native mouse fallback also failed its window validation before sending a click. Do not describe this as an end-to-end mouse-click test. Existing Play-mode regressions exercise match/setup state, tower upgrades and rendering, but do not click every HUD control. Smaller desktop resolutions have not been visually inspected in this checkpoint.

No simulation, map, economy, roster, pathfinding or art assets changed in this HUD pass. The prior wall-placement and campaign results remain their original evidence.
