# Howl for Maul verification — tower firing feedback checkpoint

The user-requested four-part pass is implemented: clearer tower/enemy presentation and camera, stronger faction tradeoffs, twenty-wave campaigns, and a normal menu flow without the empty test arena. See BATTLEFIELD-UPDATE.md for implementation details.

## Automated tests

- Unity 6000.3.25f1: **60/60 passed**, including six actual Play-mode integration tests. Exact results: Howl-Unity-Tests.json.
- Previous checkpoint standalone .NET suite: **54/54 passed**. It was not rerun for this presentation-only patch. This includes the captured 61-unit Ironfold corner jam, which failed before the movement fix and now verifies that all enemies finish without overlap, terrain/tower penetration or false siege.
- New coverage checks paid tower role silhouettes, upgrade markers, cosmetic collider isolation, builder camera/overview, packaged twenty-wave data, robot strengths and paid Scrap refunds, difficulty-scaled late previews, and heavy/runner silhouettes including Relaxed siege damage.
- Existing mask fidelity, all-active lanes, fixed team economy, ownership, upgrades, paid queues, freeform maze/reopening, siege, combat effects, pause and return-to-match regressions remain passing.

The new Play-mode regression uses a paid tower and real combat hit to verify shot-facing, recoil, pause/setup freezing, recovery after simulation resumes, and ignoring chain-bounce origins. No simulation or balance files changed.

## Paid full campaigns

Previous checkpoint evidence (not rerun for this presentation-only patch): **36 final campaigns completed: 27 wins, 9 defeats, no stalls and no individual/team wallet errors.** These are Normal-difficulty simulations with real travel, purchases and unchanged income:

| Policy | Campaigns | Wins | Defeats |
|---|---:|---:|---:|
| Solo coverage followed by affordable upgrades | 12 | 12 | 0 |
| Solo paid zig-zag maze followed by coverage, no upgrades | 12 | 11 | 1 |
| Two-player mixed factions, roster/champion/upgrade-first | 12 | 4 | 8 |

Every faction won the full twenty-wave campaign under adaptive spending, with all 30 lives. The runs recorded 2,057 paid upgrades. The formerly stalled Prism/Horizon roster campaign now ends in a normal defeat rather than hanging. Detailed outcomes, exact purchases, per-wave wallets and strategy limits are in Balance/TWENTY-WAVE-BASELINE.md and its raw JSON files. Earlier ten-wave datasets remain historical evidence; they do not establish current balance.

These policies do not exhaust human maze designs, difficulty choices or multiplayer combinations. Roster-first robot defenses remain weak in these tests. Final balance is not settled, and no online multiplayer is implemented.

## Native visual inspection

Inspected actual Game-view captures of Rimewatch's five tower roles and upgrade tiers, and Ironfold's seven-design paid robot lineup. Original snow/metal surface panels, faction colors and minimap viewport outlines render correctly. The pictures are controlled paid showcases, not full human playthroughs. Art remains procedural prototype art.

## Builds and platform limits

Updated Linux and Windows builds succeeded with zero build errors. Howl-Builds.json records the final build summaries. Linux has the expected editor-automation-disabled warning; Windows additionally reports unsupported package ray-tracing shader warnings. Standard URP rendering is used.

The actual final Linux executable passed the packaged smoke test on an isolated virtual display, exiting 0 and confirming both twenty-wave maps, faction data and every ground/flying lane route. At the previous checkpoint, both headless and ordinary windowed attempts on this machine's native :0 desktop failed during X video-mode initialization before game code. **Use Unity Play mode on this machine for now.** Native standalone desktop launch is not claimed to work. The temporary :98 display was stopped after verification.

Windows has been built, but has not been executed on Windows. Neither smoke tests nor automated campaigns constitute a full human desktop playthrough.

## Preserved constraints

The supplied layout masks and terrain collision geometry are unchanged. Every lane remains active at every player count. The team starts with 1,200 gold, split across one to four wallets. Ownership, exclusive faction rosters, paid construction, upgrades and freeform blockade/siege behavior remain intact. No reference Warcraft III or Mega Man assets were imported.
