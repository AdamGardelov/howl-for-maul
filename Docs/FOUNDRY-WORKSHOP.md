# Pulse Foundry belongs to Ironfold

2026-10-10. The player liked the improved map but identified a mismatch between its inhabited, earthy setting and the robotic tower roster. This is the first faction correction. It does not claim the other eleven families have been reworked or that the game has reached the selected concept's finish.

Pulse now uses reclaimed workshop instruments on worn slate beds. Worked copper bodies, brass lips, subdued verdigris and small cyan glass cores echo Ironfold's roofs and hearth-forges. The shared palette also brings its existing wardwright's clothing and tools into that material family. Ownership rings and projectile identities keep their existing colors.

| Existing design | New silhouette |
|---|---|
| Fuse Cadet | Small carrying lantern with a short cast bellmouth |
| Ironhand | Open hammer press, hanging ram and anvil |
| Shear Sentinel | Exposed rotating cutting wheel with forged teeth |
| Arc Ranger | Long sighting instrument, cast cradle and elevation handwheel |
| Flare Keeper | Three-pronged sky beacon with a glass heart |
| Rime Runner | Frost vessel, copper bands and paired return pipes |
| Echo Champion | Great wardbell in sweeping arches with paired lower bellmouths |

The first render still exposed angular joints in the frames. Those were replaced with continuous curved tube meshes and smooth normals. Meshes are cached per shape and combined into the existing rigid material batches. The wheel and wardbell use the existing simulation-tick motion path; pause and match speed continue to apply. Actual-model portraits and construction previews inherit the geometry. No new lights or scenic blockers are introduced.

Costs, faction prerequisites, targeting, damage, projectile/sound identities, occupied cells, map masks, economy and networking are unchanged. Level-three models remain within their cells when aimed diagonally. The original source and licenses remain ours; no reference-game models or textures were imported. Scott Buckley attribution is retained.

## Camera correction

R and the tactical-map north button now face north **and align horizontally with the map's symmetry plane**. Previously R only reset yaw, so a sideways perspective view could still make the map spine lean. The new command keeps the player's zoom and position up/down the map. Home still returns to the builder; End fits the whole map. Settings/help describe the distinction.

A new projection regression reproduces the old failure and checks the map spine and mirrored points after resets from both sides and both rotation directions, at three zoom levels on both maps. It also checks unchanged zoom, depth, scenery transforms and wallet. Existing drag, setup preview and title-camera tests remain in force.

## Evidence and next work

See [verification](Verification/Foundry-Workshop/README.md) for final test/build status and real game captures. The published `v0.1.0-playtest.1` assets are immutable and contain the previous tower/camera behavior; these changes require a newer local build or a later versioned release.

Remaining faction work should follow each order's place in the world: signal-glass shrines for Prism, channel-driven mechanisms for Tidal, stone-moving tools for Gravity and repaired implements for Scrap. Keep strong large shapes and material relationships; adding more tiny colored parts will not solve the mismatch. Mixed-defense listening, native Windows, hardware performance and full online matches across separate networks remain separate checks.
