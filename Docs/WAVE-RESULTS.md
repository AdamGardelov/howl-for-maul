# Wave results and match endings

The sidebar now shows a persistent completed-wave recap: defeated enemies, leaks and income. Local players see their own share plus the team total. Income counts actual bounty and completion rewards as distributed; construction, upgrades and refunds are excluded. Starting gold is excluded. A defeated match reports its partial wave without granting a completion bonus, even while spawns remain queued. Immutable snapshots prevent the next wave from rewriting the previous result.

The next launch clears the current recap. Victory and defeat receive distinct headings and notices. Finished matches show MATCH COMPLETE before considering remaining enemy queues, offer New match, and hide world placement previews. Camera controls and minimap navigation remain available.

Verification: 61/61 pure simulation cases and 70/70 Unity cases passed. New cases cover one through four players, odd reward remainders, spending/upgrades/refunds during combat, snapshot independence, repeat steps, restart, survived leaks, defeat with pending spawns and no-income free build. The Hard mixed Gravity/Scrap twenty-wave campaign was rerun and every recorded field exactly matched Docs/Balance/GRAVITY-SCRAP-HARD.json (30 lives, no stalls or wallet changes).

A live-editor Rimewatch first wave used six normal paid towers, three per player across Rime Covenant and Stonebound. All 24 enemies were defeated, no leaks, 168 team income split 84 each; wallets 624 and 609 after different tower costs. Native 1920×884 recap inspected. Separate one-life defeat and one-wave victory fixtures exercise the terminal presentation; those captures are not claims of full graphical twenty-wave playthroughs. The two small terminal-label/preview corrections followed the full suite and were compiled and visually rechecked.

Current packaged builds still correspond to scenery checkpoint 465974e and predate this recap until the next package refresh. No game economy, wave tuning or routing rules changed.
