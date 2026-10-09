# World cohesion — 2026-10-09

This pass addresses the six approved priorities together: sheltered lane composition, surface transitions, warm/cool lighting, landmark places, character/combat weight, and quiet environmental audio. It retains the supplied source masks, all active lanes, the team economy and simulation rules.

## Environment

Broad blocked shelves now carry deliberately placed wayshrines on Rimewatch and banked furnaces/workbenches on Ironfold. Each map has three landmark groups chosen near its bends, with a hearth and darkened/weathered ground. Grouped branching trees with gentle wind motion, rounded stone banks, layered canopies, snow crowns and low vegetation replace the old regular tree/post scatter. Rimewatch also has an open bell gate and a taller crystal shrine. Ironfold scales entire groups to its narrower shelves. Every complete grove footprint is validated against the blocked source cells before generation; the triangle-bound regression includes all six new geometry batches. The nearby exterior gains denser silhouettes and reclaimed foundry greenery.

Snow and moss shoulders blend into the worn lane paving through paint, without raised lane geometry, colliders or build reservations. Warm amber braziers replace the broad cyan wash on Rimewatch; both maps retain cool ambient fill and warm directional light. Fire halos are smaller, point-light intensity/range lower, and no more than four nearby visible shadowless lights run at once. The original shrines/furnaces, foliage and material treatments are generated in project code; no reference-game assets were imported.

## Actors and combat

Builders now turn toward travel over several simulation ticks and make a short work gesture when their actual paid tower construction completes. The gesture faces the new tower, observes its owner and freezes with the simulation. It adds no construction delay. Infantry reactions dip and tilt the visible body after damage without changing the collision center or navigation. Tower bases gain sloped braces; artillery rear magazines and interceptor fins strengthen role silhouettes inside the existing footprints.

Direct projectiles now have faceted solid bodies with short tapered wakes and impact fragments. Shards, shells, spears, embers, rings, stars, orbs and bolts retain their per-design colors. Body/wake/fragments share one mesh renderer; chains retain one line renderer. The shared 64-effect limit, off-camera suppression, pause/speed behavior and immediate authoritative damage remain. These are cosmetic travel cues, not a delayed-damage system.

## Ambient mix

Four bounded voices provide a quiet wind bed, the nearest hearth's crackle, nearby creaking timber or distant machinery, and builder footsteps/hover movement. The environmental clips are original deterministic stereo noise/friction synthesis, separate from the existing weapon bank and licensed soundtrack. Local levels and stereo position follow the ground under the RTS camera, avoiding altitude-dependent silence. Footsteps require actual builder movement and are rate-limited in real time.

Sound Effects volume and the existing sound toggle control all environmental voices. Ambient loops remain quieter during pause/setup/menu so the world stays alive; footsteps stop. Changing maps releases the owned clips. No downloads, new services or licenses are required. Synthetic sounds still benefit from a human listening pass on speakers/headphones.

## Verification

The first new integration case caught that Ironfold's narrow ledges fitted only two full-size landmark groups. Scaling complete groups to fit the source mask resolved that case. The first complete five-case run then passed actor/roster, exterior, triangle-clearance and projectile regressions; the combined ambience/landmark case reached Ironfold and found only seven groves against the intended minimum of nine. The final art pass uses denser candidate sampling on Ironfold, still accepting only whole safe footprints, and broadens the tree crowns after inspecting the actual game-camera captures.

Passed before the final foliage refinement:

- All 76 towers and twelve builders, textures, ownership, smooth turning, paused pose and actual paid construction (153.95 s).
- Both-map exterior triangle boundaries, six settlement buildings, roofs, reset-angle behavior and weapon bank identity (156.61 s).
- Both-map scenery triangle bounds, no colliders, fire particles and four-light limit (165.58 s).
- All 72 armed projectile signatures, real paid shots, flying height, paused geometry, chains, speed-scaled expiry, offscreen suppression, 64-renderer cap and reset cleanup (157.24 s).

Unity's CLI aggregate duration omits Play Mode transitions, so the figures above come from the individual XML test cases. Initial failed results are retained rather than presented as passes. The final scenery rerun passed in 160.84 s, including all new canopy/landmark triangles, colliders, fire animation and lights. The combined ambience/construction case passed on both maps in 168.54 s: three landmarks per map, at least nine groves, unchanged source masks, wind during combat pause, four audio voices with finite bounded stereo samples, effects-volume/toggle muting, owner-paid construction and frozen work gesture, visible enemy hit reaction, movement-only footsteps and clip cleanup on map changes. Final close/overview/exterior captures were inspected.

The older crowded-scene fixture failed because it still expected the previous 600-gold wallets and 20-gold Rime opener; the current opener costs 10. The first headless generation attempt also revealed an outdated cached Release executable and was discarded. The harness was rebuilt from current source before recording replacement campaigns. No game economy or balance was changed to make the replay pass.

Fresh two-player Hard campaigns now use Ember/Volt on Rimewatch and Pulse/Blast on Ironfold, starts 7/0. Starting wallets are exactly 120/120 and 1,100/1,100. Both cleared twenty waves with 30 lives: 311 purchases / 130 upgrades on Rimewatch, 336 purchases / 396 upgrades on Ironfold. The independent Python auditor passed all 80 wallet and 80 income checks. The Unity replay fixture now rejects mismatched starting wallets and reward schedules up front. The corrected Unity replay passed in 239.29 s. All purchases, builder travel, upgrades, kills/leaks and end-of-wave gold reproduce the fresh ledgers; both campaigns win and restart clears their unit views. The late-wave captures contain 311 towers / 29 enemies on Rimewatch and 336 / 47 on Ironfold. Warmed stable view synchronization allocates zero managed bytes on both maps. Offscreen render-call median/max were 9.38/14.00 ms and 6.53/8.81 ms on the editor GTX 1080/OpenGL at 1280×720. These are scoped render-call timings, not full combat/HUD frame rates, and the two scenes have different armies from the old report, so they are not an A/B performance comparison.

Final route inspection found that a tall Rimewatch shrine intersected the middle flying route at enemy height. Tall landmarks now stay outside swept flight corridors, and grove positions beneath them use purpose-built low shrubs and rocks. The independent regression checks every new composition triangle against both the blocked-cell mask and expanded flight segments above 1.35 units. The first air-clearance rerun passed Rimewatch but found eight Ironfold groves after a relocated furnace displaced one; denser safe candidate sampling restores at least nine groups without relaxing either clearance check. The final both-map combined rerun passed in 169.01 s, including ambience, actual paid construction, reactions and cleanup. Captures were refreshed and inspected. The paid campaign replay above preceded this cosmetic placement follow-up; simulation and economy source did not change. See Howl-Cohesion-AirClearance.json and the retained initial failed result.

Six distinct focused integration cases now pass, with the affected environment checks rerun after the final foliage refinement and the paid replay rerun after its fixture correction. Final Unity compilation succeeded. The CLI reports zero errors/warnings; the compiler log still contains the existing TowerView.light hiding-member warning. The headless harness rebuild has zero errors and one existing .NET PBKDF2-constructor deprecation warning. Native audio-device listening, Windows runtime, internet networking and a full Unity-suite rerun are not claimed for this pass. The fresh Linux package is `Builds/Linux-Cohesion/HowlForMaul`, containing runtime source `2d37f0f` and the music/source notices. Build succeeded with zero errors and one expected Pipeline-runtime-disabled warning. Both packaged-map data/resource/route smoke checks passed with a clean exit on the isolated Xvfb/OpenGL display. Smoke mode skips presentation/audio; the separate live-editor integration cases and inspected renders provide that presentation evidence. No new Windows package was built.

```sh
/home/adam/Documents/Dev/howl-for-maul/Builds/Linux-Cohesion/HowlForMaul -force-wayland
```

Current build and smoke evidence: Howl-Cohesion-LinuxBuild.json and Howl-Cohesion-Smoke.txt. The live editor was reused and left available; older player builds were preserved. Scheduler remains paused and mobile remains on hold.
