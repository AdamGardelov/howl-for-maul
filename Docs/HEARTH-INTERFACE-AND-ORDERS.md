# Hearth interface and faction silhouettes

2026-10-10. Continues the [living-world pass](LIVING-WORLD.md), following the request for a warmer, professional presentation and factions recognizable by shape rather than a colored band. This is an implemented iteration, not a declaration that the commercial release bar has been reached.

## Interface

The title camera now looks into Ironfold's defended settlement: the lit refuge hall, planted stone garden, blossom trees and open road. Its slow drift remains temporary and never changes the player's saved camera position, rotation or zoom.

The shared interface uses muted green slate, restrained brass rims, clipped corners and warm text. Heavy nested frame outlines have been removed. Buttons retain separate idle, hover, focus, pressed and selected states. Primary actions have warmer surfaces; selected tower cards and speed choices also have an underline. The minimap has one frame with a readable title, north/reset control and travel hint. The center of the screen stays clear unless a tower is selected.

Resource counters now have small original symbols, separators, larger values and enough room for the opening wallet at 960×600. Top actions separate their label from a compact shortcut key. Settings have explicit ON/OFF controls; sliders, text entry and scroll thumbs use the same visual materials. Tooltips have more readable text and an opaque inset surface. Online forms, chat, faction selection, pause and results share the skin. The existing host-only speed authority, chat capture and modal input protections remain in force.

Difficulty selection now uses a centered, content-sized panel with three descriptive challenge choices. Solo does not show a redundant pause-vote button. Closing menus or adopting a newly started match waits for a neutral input frame before accepting world commands; the click used to start a match can no longer become a build order. This issue was discovered during actual mouse-driven testing after the staged render checks passed.

Scott Buckley's credits remain visible on the title screen and in settings, with the bundled license notices. No soundtrack recording was changed in this pass. Automated QA runs now skip preference saving, so their temporary mute/volume changes cannot become a player setting. Further isolated checks use their own XDG configuration directory.

## Twelve visual families

The old common pedestal, rear buttresses, skirt and magazines were making unrelated units look alike. Those have been replaced by faction-specific supporting forms. Existing weapon identities are retained and reshaped around them:

| Order | Shape and material direction |
|---|---|
| Pulse Foundry | Superseded by the [workshop pass](FOUNDRY-WORKSHOP.md): copper instruments, presses and wardbells on slate beds |
| Blast Circuit | Squat copper siege beetles with six articulated legs and broad bodies |
| Prism Division | Ivory glass shrines, split petals, suspended crystals and turning orbit rings |
| Horizon Guild | Timber survey tripods, brass fittings, strung ballista arms and long cradles |
| Gravity Works | Open bronze gimbals, dark anchor stones and turning counterweights |
| Scrap Frontier | Rusted salvage carts, four wheels, crane jibs and improvised tools |
| Overdrive Order | Iron drakes with folded furnace wings, haunches, claws and dorsal spines |
| Tidal Array | Patinated shell cradles, pearl-colored petals and working tidewheels |
| Rime Covenant | Snowbound ritual stone, swept ice petals and a suspended hail-bell frame |
| Stonebound | Mossy mountain guardians with heavy feet, fists and branching worldroots |
| Ember Assembly | Brick hearths, boilers, wrought handles and copper forge rings |
| Volt Vanguard | Conductors with earthing feet, copper cables, rails and induction hoops |

Builders inherit the material families and carry larger recognizable tools: pressure packs, ivory crests, survey cloth, gravity halos, salvage crates, drake horns or shell crests. Round shells and cloth now have smoother profiles; crystals, stone and armor keep deliberate hard edges. Tower portraits are rendered from these actual models, not separate illustrations.

Geometry remains original repository-authored work. Rigid parts still share combined meshes and materials by design. Moving rings and wheels are small separate batches driven by simulation ticks, so pause and game speed apply. Towers retain their original damage, targeting, shot colors, sounds, cost, prerequisites and occupied cells. Horizontal silhouettes are fitted before batching; upgrades grow vertically rather than expanding over neighboring cells. Brief combat recoil remains a visual effect and never changes occupancy.

The ground exit marker is now a quiet ring with “REFUGE · EXIT.” Intermediate route annotations stay available through route guides and setup, without scattering numbered checkpoints over ordinary play.

## Verification and remaining release work

The structured record and retained screenshots are in [Verification/Hearth-Orders](Verification/Hearth-Orders). Verification is performed against actual Unity models and packaged Linux renders. Read the record for completed runs and platform limits.

The artwork remains stylized procedural geometry. This pass makes factions easier to distinguish, but bespoke hero-quality meshes, richer character animation and an independent first-time-player review are still commercial-quality gates. Native Windows, mixed-hardware performance and full matches between friends on separate networks must be validated before treating the build as a finished paid release. Reference-game screenshots inform composition; no Warcraft, League or Dota assets are shipped.

Nonblocking follow-up: retain these silhouette families when sculpted models replace procedural parts. A full-model placement preview, brighter portrait staging and stronger role differentiation within each faction are useful next priorities; do not compensate by enlarging the HUD or obscuring maze cells.
