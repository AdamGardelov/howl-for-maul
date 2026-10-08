# Howl for Maul verification — Stonebound model checkpoint

The user-requested four-part pass is implemented: clearer tower/enemy presentation and camera, stronger faction tradeoffs, twenty-wave campaigns, and a normal menu flow without the empty test arena. See BATTLEFIELD-UPDATE.md for implementation details. The new art-direction pass adds continuous softly varied ground, beveled mask edges, layered pines, original faceted tower meshes, winter stonework and warm/cool matte lighting; see ART-DIRECTION.md.

The latest model slice gives Rime Covenant five distinct stone-and-ice designs, replaces builder capsules with wardens/drones, and adds faceted enemy shells, swept wings and tick-driven walking feet. See RIME-MODEL-SLICE.md.

The latest Stonebound pass adds five distinct models. Its paid builder integration and full suite passed 63/63; see STONEBOUND-MODELS.md. A fresh 1920×884 paid lineup capture was inspected at zoom 7. Builds and campaign results below belong to preceding checkpoints, not this Stonebound source update.

## Automated tests

- Unity 6000.3.25f1: **63/63 passed**, including six actual Play-mode integration tests. Exact results: Howl-Unity-Tests.json.
- Prior wall-placement checkpoint standalone .NET suite: **57/57 passed**; not rerun for this presentation-only patch. This includes the captured 61-unit Ironfold corner jam, which failed before the movement fix and now verifies that all enemies finish without overlap, terrain/tower penetration or false siege.
- New coverage checks paid tower role silhouettes, upgrade markers, cosmetic collider isolation, builder camera/overview, packaged twenty-wave data, robot strengths and paid Scrap refunds, difficulty-scaled late previews, and heavy/runner silhouettes including Relaxed siege damage.
- Existing mask fidelity, all-active lanes, fixed team economy, ownership, upgrades, paid queues, freeform maze/reopening, siege, combat effects, pause and return-to-match regressions remain passing.

The new Play-mode regression uses a paid tower and real combat hit to verify shot-facing, recoil, pause/setup freezing, recovery after simulation resumes, and ignoring chain-bounce origins. The current placement fix preserves fractional source-cell positions through construction and selection. Three new pure-simulation regressions cover map-wall seams, paid half-cell queues and sealed-corridor siege/reopening; see WALL-PLACEMENT.md.

Extended Play-mode assertions passed for shared tower/wing meshes, cleanup on map unload, distinct Rime geometry, builder material separation, cosmetic collider isolation, and walking animation pause/resume. No simulation code changed.

## Paid full campaigns

At the wall-placement checkpoint, two Normal solo adaptive campaigns, Rime Covenant and Pulse Foundry, won all twenty waves with 30 lives and no wallet errors or stalls. These retain the bot's whole-unit placement policy; targeted tests cover new half-cell placements. See Balance/WALL-FIT-CAMPAIGNS.json.

Previous checkpoint evidence (the full matrix was not rerun for this placement fix): **36 final campaigns completed: 27 wins, 9 defeats, no stalls and no individual/team wallet errors.** These are Normal-difficulty simulations with real travel, purchases and unchanged income:

| Policy | Campaigns | Wins | Defeats |
|---|---:|---:|---:|
| Solo coverage followed by affordable upgrades | 12 | 12 | 0 |
| Solo paid zig-zag maze followed by coverage, no upgrades | 12 | 11 | 1 |
| Two-player mixed factions, roster/champion/upgrade-first | 12 | 4 | 8 |

Every faction won the full twenty-wave campaign under adaptive spending, with all 30 lives. The runs recorded 2,057 paid upgrades. The formerly stalled Prism/Horizon roster campaign now ends in a normal defeat rather than hanging. Detailed outcomes, exact purchases, per-wave wallets and strategy limits are in Balance/TWENTY-WAVE-BASELINE.md and its raw JSON files. Earlier ten-wave datasets remain historical evidence; they do not establish current balance.

These policies do not exhaust human maze designs, difficulty choices or multiplayer combinations. Roster-first robot defenses remain weak in these tests. Final balance is not settled, and no online multiplayer is implemented.

## Native visual inspection

Fresh 1920×884 Rimewatch captures were inspected at orthographic sizes 7 and the normal 11. Five towers were bought through the builder, a sentry upgraded, and ground/flying enemies staged for the visual fixture. The builder was moved clear of the lineup. The simulation/input component was temporarily disabled after view synchronization to hide the cursor placement ghost in the paused captures; the normal editor setup was restored afterward. These are in-engine staged captures, not full human playthroughs or crowded-battle performance measurements.

Prior HUD captures at 1920×884 verify setup, the seven-card Ironfold roster and selected-tower inspector. Resources, wave controls and action/error notices stay above the scroll. The sidebar camera/input exclusion uses the new width. No end-to-end mouse-click pass is claimed: editor GUI injection did not activate the control, Pipeline does not support this project's legacy input, and the native fallback failed window validation before clicking. See HUD-UPDATE.md. Smaller resolutions were not visually checked.

Prior wall-fit capture: a paid tower at (26, 10.5) sits against an Ironfold half-unit wall edge, with the placement grid visible and wallet 1,190. The following art-pass captures are prior-checkpoint evidence.

Inspected fresh 1920×884 Game-view captures on both maps. Rimewatch shows five paid tower roles, an upgraded sentry, and staged ground/flying enemies at normal zoom 11 and closer zoom 7. Ironfold shows seven paid robot designs at zoom 9. The first capture exposed overly blocky ground variation and uniform tree placement; both were revised and recaptured. The final views show continuous ground color, clean ledge surfaces, varied layered pines and visible faceted tower bases. These are controlled visual fixtures, not human playthroughs or performance benchmarks. Art remains an early procedural interpretation of the requested simpler League-like direction; The HUD and enemy models subsequently received the passes documented above.

## Builds and platform limits

At the preceding Rime checkpoint, Linux and Windows builds succeeded with zero build errors. Howl-Builds.json records the final build summaries. Linux has the expected editor-automation-disabled warning; Windows additionally reports unsupported package ray-tracing shader warnings. Standard URP rendering is used.

The actual final Linux executable passed the packaged smoke test on an isolated virtual display, exiting 0 and confirming both twenty-wave maps, faction data and every ground/flying lane route. At the previous checkpoint, both headless and ordinary windowed attempts on this machine's native :0 desktop failed during X video-mode initialization before game code. **Use Unity Play mode on this machine for now.** Native standalone desktop launch is not claimed to work. The temporary :98 display was stopped after verification.

Windows has been built, but has not been executed on Windows. Neither smoke tests nor automated campaigns constitute a full human desktop playthrough.

## Preserved constraints

The supplied layout masks and terrain collision geometry are unchanged. Every lane remains active at every player count. The team starts with 1,200 gold, split across one to four wallets. Ownership, exclusive faction rosters, paid construction, upgrades and freeform blockade/siege behavior remain intact. No reference Warcraft III or Mega Man assets were imported.
