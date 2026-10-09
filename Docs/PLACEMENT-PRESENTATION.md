# Tower portraits and construction preview

2026-10-10. A bounded presentation follow-up to [Hearth and Orders](HEARTH-INTERFACE-AND-ORDERS.md).

## Changes

The faction gallery and command grid now render all 76 actual tower models with consistent camera-relative studio lighting. Dark metal and timber shapes remain legible without changing the map sun, shared tower materials or world lighting. The portraits retain their original material colors, textures and luminous details. They remain lazy, cached 160×160 renders of the real geometry.

While building, the selected design now appears at its snapped footprint as a translucent model. A restrained mint tint marks an available placement; red marks a rejected one, alongside the existing reason text. The footprint and range remain visible. Hovering an existing tower hides the copy so inspection is unambiguous. Menus, setup, chat, results, camera gestures, Move and Remove hide it through the existing hover/input gates.

The construction copy has no simulation ID, occupancy, ownership, collider, attack or order. Each visited design is built once and reused; moving it reuses its materials and geometry. Preview materials are owned independently and released with the map. Previews use a static pose and never animate in response to live shots or recoil. Ordinary placed towers retain their animation and rendering.

Queued orders still use their numbered footprints rather than displaying a crowd of models. Gold is still charged when construction succeeds. Costs, routes, factions, map masks, waves, multiplayer and soundtrack attribution are unchanged.

## Evidence

See [the retained evidence](Verification/Placement-Preview/README.md) and [four passing Unity checks](Verification/Placement-Preview/Focused-Tests.xml).

- Both maps, all 76 designs: preview creation leaves gold, cells, shots, tick and orders unchanged. Each copy uses separate materials, casts no shadow and has no enabled collider. Owned materials are cleaned up.
- Repeated movement of a warmed preview: zero managed bytes across 300 updates per map. This is a narrow allocation check, not a frame-rate benchmark.
- Both portrait sheets: nonblank lit model pixels, no missing-shader magenta, cache reuse and unchanged map lighting/materials. Full sheets were visually inspected.
- Existing real-shot recoil/pause and portrait/minimap lifecycle regressions pass.
- Actual packaged Linux pointer/keyboard flow: start with 2200 gold and zero orders; a Fuse Cadet costs 10; three Shift-clicked orders remain visible during pause; Move hides the cursor model; resume completes all three, leaving four towers, 2160 gold and zero queue. The player closed normally with exit 0.

The first interactive run was interrupted by the private display shutting down (wrapper 143, no Unity exception). It is not counted as successful gameplay. The repeated bounded sequence above passed. Private XDG preferences were used throughout.

Linux and Windows builds passed. The packaged Linux menu/state check passed at 1440×900 and 960×600; the small HUD was visually inspected. The isolated editor target was restored to Linux. Native Windows, target-hardware performance and new full separate-network matches remain untested in this pass. This does not claim a full-suite or new campaign balance run.

## Next art work

The clearer portraits make the remaining primitive geometry and over-saturated repeated accents easier to see. Improve weapon proportions and authored material/color choices within each order next. The fresh Ironfold concept discussed in chat is an art-direction exploration, not an exact map plan or an implemented scene; preserve playable masks when translating its landscape composition into the game. The Unity MCP recommendation has not installed a package or altered the tool configuration.
