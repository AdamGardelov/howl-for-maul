# Howl for Maul verification — Pulse model checkpoint

Latest source: original bespoke models for all four winter factions and Pulse Foundry. Remaining Ironfold factions retain generic role models. This is early procedural art, not finished League-quality presentation. Read PULSE-MODELS.md and WINTER-MODELS.md.

## Automated tests

Fresh Unity 6000.3.25f1 suite: **64/64 passed**, including seven Play-mode integration cases; 24.51 seconds. Compilation: zero errors/warnings. Exact results are in Howl-Unity-Tests.json.

The new Pulse test buys all six prerequisites and the champion through normal builder travel, verifies distinct models and no cosmetic colliders, and upgrades the champion. It checks 505 then 245 gold remain. Winter paid models/upgrades, mesh sharing/cleanup, enemy animation pause/resume, shot-facing/recoil, setup flow, and map switching remain covered.

Pure simulation regressions in the same suite cover exact map masks, all-active lanes, the fixed team economy, ownership, half-cell paid construction, wall seams, blockages/siege/reopening, upgrades, slow/chain/splash targeting and the captured 61-enemy corner jam. The last standalone .NET-only run was 57/57 at the wall-placement checkpoint; presentation-only passes did not rerun it separately.

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

Fresh Pulse lineup captures at 1920×884 were inspected at zoom 7 and normal zoom 11. Seven towers were bought for 695 gold; no wave was active. Temporary input disabling hid the hover ghost during paused captures. Prior native captures cover all four winter model sets, HUD setup/construction/upgrade panels, and Ironfold wall-fit placement. These are controlled visual fixtures, not full human playthroughs or crowded-battle performance measurements.

No end-to-end mouse-click pass is claimed: editor GUI injection did not activate the control, Pipeline lacks this project's legacy input support, and the native fallback failed window validation before clicking. See HUD-UPDATE.md. Smaller resolutions were not visually checked.

## Builds and platform limits

Latest packages are the complete-winter checkpoint **107e0a7**, before Pulse's new models. Both Linux and Windows builds succeeded with zero errors. Linux had one expected Pipeline-disabled warning; Windows also reported unsupported package ray-tracing shader warnings (19 total). Standard URP rendering is used. Details: Howl-Builds.json.

The winter Linux executable passed both packaged-map route/data smoke checks on isolated virtual display :98, exit 0. The display was stopped. Earlier native :0 desktop attempts failed during X video-mode initialization before game code. Use Unity Play mode here; native standalone success is not claimed. Windows runtime remains untested. Pulse must currently be viewed in the editor.

## Preserved constraints

Exact supplied map masks and terrain collision remain unchanged. Every lane stays active. Team start is 1,200 gold, split across one to four wallets. Exclusive faction rosters, ownership, paid construction, upgrades and freeform blockade/siege remain. Online networking is deferred. No copyrighted reference game assets were imported.
