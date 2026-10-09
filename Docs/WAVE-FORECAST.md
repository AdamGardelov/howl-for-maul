# Preparation forecast and final-air planning

The compact HUD now shows the next wave before launch: wave number, AIR/GROUND, difficulty-scaled health, total enemies across every active lane, and the number of team weapons that can target that type. Clicking the card opens Details with wave advice expanded. Targeting count is not a promise of route coverage or adequate damage.

The card disappears during combat, setup, the menu and finished matches. Its visible rectangle blocks world construction and camera gestures through the panel; when hidden it leaves no click-blocking rectangle. Health bars respect its occupied space. The rest of the central play area remains unchanged.

When only flying waves remain, expanded preparation advice counts the current player's ground-only weapons and sums their actual sale refunds, including paid upgrades. It does not include teammates' towers, unarmed walls or removed towers. It gives no liquidation suggestion before a flying wave followed by ground waves. Robot maps also warn that selling prerequisites can lock new champion construction. During an active wave, the general combat advice remains in place.

This follows the matched paid-rebuild diagnostic in Balance/FINALE-REBUILD.md, where Stonebound improved from two to sixteen lives and Ember from five to fifteen without stat changes. The UI offers information; it does not sell towers or automate purchases. Masks, lane activation, team economy, faction ownership and all simulation combat values remain unchanged.

Verification: Unity compilation passed; all 68 pure cases passed, including paid upgrade/refund ownership, earlier-flight safety, removed towers and champion warning. The focused Unity case PreparationForecastBlocksClicksOnlyWhileVisible passed in 100.90 seconds, covering setup/details/menu/combat visibility, click-through blocking, release when hidden and no wallet/tick mutation. Packaged visual verification follows separately; no new full-suite or platform claim.

## Packaged source f9a858d

Linux-WavePreview and Windows-WavePreview builds succeeded with zero errors. Linux emitted two warnings (Pipeline disabled, plus an uncompiled-code-change warning about postprocessing/import code); Windows emitted the nineteen recorded Pipeline/ray-tracing warnings. No postprocessing/import code changed. Actual Linux input confirms the new runtime card is present and functional. Exact records are in Howl-Wave-Forecast-Packages.json.

On the owned isolated 1440×900 llvmpipe display, staged solo Stonebound setup showed 24 ground enemies, 35 HP and all three Rimewatch lanes. Clicking the forecast opened expanded advice without building or spending. A paid Pebble Warden changed gold from 1,200 to 1,175 and targeting count from zero to one. Enter launched wave 1 and removed the card; game-menu Quit exited zero with no game exception in the player log. Both-map data smoke also exited zero. Unity target was restored to Linux. Existing player/package directories were preserved. Final-air refund text is verified in the pure regression, not in a manually played final-wave screen. Windows runtime and current native desktop/internet compatibility remain untested.
