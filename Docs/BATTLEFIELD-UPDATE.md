# Battlefield, factions, campaign and menu pass

## What changed

- Towers now use separate original silhouettes for walls, sentries, artillery, control crystals, chain relays, sky interceptors and champions. Two illuminated tier markers show paid upgrades; weapon assemblies grow slightly with upgrades. Cosmetic shapes have no physics colliders. Faction palettes are shared to avoid allocating materials with every tower purchase.
- Setup keeps a map overview. Starting a match focuses the builder at a useful construction zoom. Home returns to that view, End shows the whole map, and the minimap outlines the current ground viewport. Winter paths have subtle snow variation; Ironfold uses inset metal panels. Geometry and navigation masks are unchanged.
- Robot factions have explicit strengths and costs: Pulse rate/control, Blast area damage, Prism three-link chains, Horizon reach, Gravity strong but costly control, Scrap cheap maze pieces/base reclamation, Overdrive heavy short-range shots, Tidal combined specialist support. Air-only towers gained reach. Existing winter factions retain their distinct slow, durable wall, fire and chain roles.
- Fixed a reproduced 61-enemy jam at two Ironfold corners. Brief deterministic yielding remains collision-checked; it does not turn congestion into siege or let enemies cross towers.
- Both maps have twenty waves. The original ten-wave opening remains. The second half introduces faster rushes, dense swarms, tougher siege units and a flying finale. Air remains every fifth wave. Siege enemies have shields/beacons and runners have fins. Enemy collision sizes remain unchanged.
- The editor menu now says Open game. Normal setup and match menus no longer offer the test arena. Advanced inspection is collapsed and weapon disabling is restricted to the test arena.

## Scope and limits

This is an original procedural art pass, not finished production art or an exact transcription of either historical mod. There is no online multiplayer. The new campaign is a tested balance draft, not final balance; automated purchase policies are limited and may lose. The 1,200-gold team budget, all-active lanes, map masks, ownership, paid construction and freeform maze/siege rules remain in place. Scrap's 90% recovery, rounded down, applies to base tower cost; upgrades retain the standard 75% refund.

The user's four-tower preview layout was saved before exiting Play mode in Before-visual-update-layout.txt. It is a layout reference, not a resumable saved game.

## Verification

59/59 Unity tests and 54/54 headless tests passed. Native visual inspection covers the tower showcase and map presentation. The twenty-wave paid campaign outcomes, including losses, are recorded in Docs/Balance/TWENTY-WAVE-BASELINE.md. Linux/Windows build and runtime status are recorded separately in HOWL-VERIFICATION.md.
