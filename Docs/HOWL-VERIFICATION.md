# Howl for Maul verification — Horizon model checkpoint

Latest source: original bespoke models for all four winter factions and Pulse Foundry/Blast Circuit/Prism Division/Horizon Guild. Remaining Ironfold factions retain generic role models. This is early procedural art, not finished League-quality presentation. Read PULSE-MODELS.md and WINTER-MODELS.md.

## Automated tests

Fresh Unity 6000.3.25f1 suite: **65/65 passed**, including seven Play-mode integration cases; 25.50 seconds. Compilation: zero errors/warnings. Exact results are in Howl-Unity-Tests.json.

The new Pulse test buys all six prerequisites and the champion through normal builder travel, verifies distinct models and no cosmetic colliders, and upgrades the champion. It checks 505 then 245 gold remain. Winter paid models/upgrades, mesh sharing/cleanup, enemy animation pause/resume, shot-facing/recoil, setup flow, and map switching remain covered.

Pure simulation regressions in the same suite cover exact map masks, all-active lanes, the fixed team economy, ownership, half-cell paid construction, wall seams, blockages/siege/reopening, upgrades, slow/chain/splash targeting and the captured 61-enemy corner jam. The last standalone .NET-only run was 57/57 at the wall-placement checkpoint; presentation-only passes did not rerun it separately.

## Paid full campaigns

Fresh expanded coverage: four additional adaptive campaigns (Hard solo on both maps; Normal mixed two-player teams on both maps) won with 30 lives and no stalls/wallet errors. See Balance/HARD-AND-MIXED.md and its raw ledgers. These runs do not resolve the older roster-first defeats.


At the wall-placement checkpoint, two Normal solo adaptive campaigns, Rime Covenant and Pulse Foundry, won all twenty waves with 30 lives and no wallet errors or stalls. These retain the bot's whole-unit placement policy; targeted tests cover new half-cell placements. See Balance/WALL-FIT-CAMPAIGNS.json.

Previous checkpoint evidence (the full matrix was not rerun for this placement fix): **36 final campaigns completed: 27 wins, 9 defeats, no stalls and no individual/team wallet errors.** These are Normal-difficulty simulations with real travel, purchases and unchanged income:

| Policy | Campaigns | Wins | Defeats |
|---|---:|---:|---:|
| Solo coverage followed by affordable upgrades | 12 | 12 | 0 |
| Solo paid zig-zag maze followed by coverage, no upgrades | 12 | 11 | 1 |
| Two-player mixed factions, roster/champion/upgrade-first | 12 | 4 | 8 |

Every faction won the full twenty-wave campaign under adaptive spending, with all 30 lives. The runs recorded 2,057 paid upgrades. The formerly stalled Prism/Horizon roster campaign now ends in a normal defeat rather than hanging. Detailed outcomes, exact purchases, per-wave wallets and strategy limits are in Balance/TWENTY-WAVE-BASELINE.md and its raw JSON files. Earlier ten-wave datasets remain historical evidence; they do not establish current balance.

These policies do not exhaust human maze designs, difficulty choices or multiplayer combinations. Roster-first robot defenses remain weak in these tests. Final balance is not settled, and no online multiplayer is implemented.

The final minimap test independently verifies all 20,480 cells against the supplied ASCII masks, cache reuse after building, and texture disposal on map replacement.

## Rendering performance

Rigid tower pieces now use shared per-design batches. A controlled 324-tower editor overview reduced draw calls from 6,206 to 5,021 and median reported render time from 11.99 to 10.92 ms, with a stable empty-map control. See Performance/TOWER-BATCHING.md for raw samples, visual comparison and limitations. No standalone FPS or busy-wave performance claim is made.

The permanent minimap cache removes another 616 draw calls. With both optimizations, the 324-tower fixture reports 4,405 draw calls and 8.29 ms median render time, compared with 6,206 and 11.99 ms originally. See Performance/MINIMAP-CACHE.md.

## Native visual inspection

Fresh Blast (zoom 7) and Pulse lineup captures at 1920×884 were inspected at zoom 7 and normal zoom 11. Seven towers were bought for 695 gold; no wave was active. Temporary input disabling hid the hover ghost during paused captures. Prior native captures cover all four winter model sets, HUD setup/construction/upgrade panels, and Ironfold wall-fit placement. These are controlled visual fixtures, not full human playthroughs or crowded-battle performance measurements.

A packaged Linux mouse check on isolated :98 at 1440×900 verified Start Match, buying/selecting a Shard Sentry, and its upgrade button (1200 → 1180 → 1160 gold, level 2). Sale also passed (30 gold refund, 1190 balance). Full wave play was not checked. Earlier editor injection failures remain historical; this is a separate packaged check. See Performance/QUALITY-PASS.md.

## Builds and platform limits

Fresh packages now include minimap caching, 2× MSAA and medium soft shadows, plus all four winter sets, Pulse, Blast and rigid tower batching. Both Linux and Windows builds succeeded with zero errors; Linux reported one expected Pipeline-disabled warning, Windows 19 including unsupported package ray-tracing shaders. See Howl-Builds.json.

The Linux executable passed both packaged-map route/data smoke checks on isolated :98, exit 0. The display was stopped. Earlier native :0 desktop attempts failed in X video-mode initialization before game code; use Unity Play here. Windows runtime remains untested. Smoke checks do not verify graphical performance.

## Preserved constraints

Exact supplied map masks and terrain collision remain unchanged. Every lane stays active. Team start is 1,200 gold, split across one to four wallets. Exclusive faction rosters, ownership, paid construction, upgrades and freeform blockade/siege remain. Online networking is deferred. No copyrighted reference game assets were imported.

## Quality follow-up

Both map captures inspected with the new quality settings. Matched editor measurements and limitations are in Performance/QUALITY-PASS.md. Median render time changed from 8.29 to 8.33 ms for the paused 324-tower fixture; slower tail samples prevent a no-cost claim.

Prism follow-up: seven paid models plus champion upgrade passed the extended integration test. Lineups inspected at zoom 7 and 11. See PRISM-MODELS.md. The packaged builds predate Prism.

Horizon follow-up: clean compile and targeted paid-model integration case passed 1/1; last full suite remains 65/65 at Prism. Close/normal native captures inspected. See HORIZON-MODELS.md. Hard solo Prism adaptive campaign won all 20 waves with 30 lives, no stalls or wallet errors; see Balance/PRISM-HARD.md. Packages predate Prism and Horizon.

Crowded combat follow-up: native 1920×884 diagnostic with 324 injected towers and 74 injected high-health enemies inspected. Ordinary health bars now shrink in the overview while selected enemies retain full-size bars. Clean compilation and close/overview visual checks; no additional gameplay-suite run for this HUD-only change. See Performance/LIVE-COMBAT.md.
