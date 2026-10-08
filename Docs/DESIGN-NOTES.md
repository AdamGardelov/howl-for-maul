# Howl for Maul — design direction

The user's reference images and corrections are the primary direction:

- Enemies spawn from every upper lane, regardless of active player count.
- The map flows from top to bottom. Surviving enemies enter connected downstream defenses and converge on a shared exit.
- Terrain defines defense areas; players build their own routes within those areas. Mazing, tight turns and congestion matter physically.
- A full blockage remains legal. Ground enemies siege player-built obstructions; permanent terrain is never a siege target.
- Choose difficulty before a match. Fewer players receive larger shares of a fixed four-player team economy, not fewer spawn lanes.
- Multiple players choose starting positions. Positions do not isolate ownership regions or prevent building downstream.
- Add maps through authored map assets, not through a separate game implementation.

## Implemented direction

Rimewatch and Ironfold follow the user-supplied winter and robot map masks, with three and four upper lanes respectively and one bottom exit. All lanes remain active. Both maps now have twenty authored waves; air arrives every fifth wave. The second half tests fast rushes, swarms and siege units.

Four winter factions provide twenty designs, and eight robot factions provide fifty-six designs with owned-roster champion prerequisites. Roles include cheap maze pieces, direct fire, slowing, splash, chaining and dedicated anti-air. Towers can be upgraded twice. Original procedural role silhouettes, faction palettes, tier markers, surface detail and a builder-focused camera improve readability. A minimap, wave forecasts and pre-purchase stats support planning. See BATTLEFIELD-UPDATE.md for this pass and Balance/TWENTY-WAVE-BASELINE.md for strategy-specific outcomes.

The current setup supports solo and local control of up to four player slots. It is NOT online multiplayer. Networking remains explicitly deferred by the original brief. Simulation state, ownership and player commands are ready for later authoritative-host work.

## Research used as context

[Magi Maul's author description](https://www.hiveworkshop.com/threads/magi-maul-v7-7.259682/) describes setup difficulty, differentiated tower choices/upgrades and informative tower/wave descriptions. Those are useful usability references, not a requirement to copy its heroes, races or rules. The user's preferred free-form mazing and shared downstream defenses remain central.

[Wintermaul One's creator discussion](https://us.forums.blizzard.com/en/warcraft3/t/hi-im-the-creator-of-wintermaul-one-and-this-my-opinion/19128) establishes creator context, but does not provide enough technical gameplay detail to infer missing rules. A fetch of the project's main site was unavailable. No reference game assets are included in the project. Later archive research was isolated in scratch space; see MAUL-RESEARCH.md.

## Deferred choices

- Final difficulty and wave count; current 70/100/140% health and siege damage are initial tuning.
- Whether four is the final maximum player count. The current budget is based on four players, as requested.
- Whether kill income should remain equally split. Current total kill and wave income is conserved, with integer remainder rotation so three-player games lose no gold.
- Further roster tuning, armor/status interactions, bosses and longer-term progression.
- Final art/audio direction. Current visuals are original procedural placeholders and sound is synthesized.
- Match saving, persistence, lobbies, reconnects and online multiplayer.
