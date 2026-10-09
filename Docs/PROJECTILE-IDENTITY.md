# Tower projectile identity — 2026-10-09

The 72 armed designs now have distinct shape/color signatures. The four unarmed Rimewatch maze pieces do not fire. Existing faction colors remain the starting point, with weapon-specific tints and silhouettes rather than a shared straight beam for every weapon.

Rimewatch uses cyan shards, blue frost rings, pale hail shells and aurora spears; Stonebound uses ochre pebbles, amber quake shells, lime sky spears and green root stars. Ember weapons use orange comet-like embers, yellow furnace shells, pale flare spears and red meteor stars. Volt weapons use violet bolts, blue branching arcs, lilac sky spears and magenta heavy pulses. Ironfold's seven slots have bolt/orb/shell/shard/spear/ring/star silhouettes and distinct weapon tints within each faction palette. Chained hits keep the originating design's color and branch through a jagged line.

Each shot uses one LineRenderer. It animates a short trail and outlined projectile head, then expands at the impact; splash rings retain the actual hit radius and matching color. Motion follows the existing pause/setup/menu and match-speed effect clock. These are cosmetic tracers for already-resolved combat events, not simulated ballistic objects: damage, targeting, economy, network events and travel rules are unchanged. Existing sound cues remain. Unknown/debug design IDs retain the generic tracer.

The existing shared 64-effect budget, leak priority and offscreen-event consumption remain. Frustum bounds include projectile tips and elevated shell trajectories. Materials are cached per design and owned by the map; the renderer creates no physics/colliders or per-frame style arrays. Distinct signatures do not imply that every color is individually recognizable without its shape, especially in dense volleys or for color-vision differences.

## Verification

Compilation passes with zero errors/warnings. `ProjectileRosterRetainsIdentityFlightPauseAndBounds` passes (143.66 seconds including Play Mode transitions; CLI aggregate 13.48 seconds). It checks unique shape/color signatures for all 72 armed designs, one renderer/no colliders per preview projectile, valid geometry, and captures all twelve faction lineups. The lineups were visually inspected. Those staged shots are cosmetic fixtures, not campaign evidence.

On each actual map, the test separately orders paid construction, starts a wave, confirms a real hit damages a flying enemy and produces the expected design/color/height, checks frozen projectile geometry in pause, checks colored chain endpoints from air to ground, then checks expiry at 3× speed. An offscreen burst consumes no effect slots, an on-screen 100-shot burst remains capped at 64 renderers, and starting a new match removes old effects without changing gold/ticks through the cosmetic fixtures. This focused test is not a new networking, campaign-balance or frame-rate benchmark.
