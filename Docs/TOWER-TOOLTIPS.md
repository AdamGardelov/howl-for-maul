# Compact tower information

Hovering a tower portrait now shows the same damage, interval, direct DPS, range, health and special-effect information used by Details. Unarmed maze pieces explicitly say they have no weapon. The command grid stays the same size and the centre stays clear until a tooltip or selection is needed.

Locked champions name the specific regular towers the current player still needs to own. A teammate's tower does not satisfy the requirement. Buying the prerequisite removes its name, and selling it restores the missing entry. Affordability text reports the exact additional gold needed. Tooltip text is read-only and does not reserve queued money or change selection, ticks or wallets.

The compact grid and Details share the stat formatter so their numbers and effect descriptions stay consistent. The tooltip width is capped to the viewport and its top stays on screen; no new interaction or build-blocking panel is introduced.

Unity compilation and the all-roster CompactTowerTooltipsTrackOwnedRequirementsAndExactShortfall test pass (1.79 seconds). It checks all twelve factions / 76 tower tooltips, armed/unarmed targeting, state immutability, each robot faction's owned unlocks, teammate exclusion, exact gold shortage and sale relocking. This is UI-only runtime work: map masks, navigation, economy, combat values and networking are unchanged. Packaged verification follows below.

## Packaged checks

Runtime source db0369c. Linux-TowerInfo and Windows-TowerInfo builds succeeded with zero errors. Linux emitted one Pipeline-disabled warning; Windows emitted nineteen Pipeline/ray-tracing warnings, retained in Howl-Tower-Tooltip-Packages.json. Music attribution and source provenance are included beside both executables. The editor target was restored to Linux after packaging.

On the owned isolated 1440×900 llvmpipe display, actual Linux input completed staged Ironfold / Pulse Foundry / first-start / Normal setup. Echo Champion initially listed six missing towers. A normal paid Fuse Cadet purchase, including builder travel, changed gold from 1,200 to 1,190; the live champion tooltip then listed only the other five. The regular tower tooltip showed targeting once and the expected health, damage, interval, direct DPS and range. Final captures were inspected. Esc → Quit exited zero, with no game exceptions in the player log. Both-map data-only smoke also exited zero.

An earlier packaged visual check showed repeated targeting text in the initial tooltip. The final formatter removes duplicate role/description lines, and the focused Unity case and both packages were run again after that cleanup. Raw final evidence is saved in Howl-Tower-Tooltip-Unity.json, Howl-Tower-Tooltip-Packages.json and Howl-Tower-Tooltip-Smoke.txt.

The focused ownership regression disables builder travel to make its paid prerequisite fixture quick; it is not a new paid-travel campaign. Actual packaged purchasing uses normal builder travel. Windows runtime, the user's desktop backend and networking were not retested. There is no new full-suite or performance claim. Existing user player, editor work and previous packages were preserved.
