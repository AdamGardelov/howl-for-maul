# Maul research and implementation decisions

## Evidence, not a claim of one definitive version

The 2003 [Tower Defense FAQ](https://gamefaqs.gamespot.com/pc/256222-warcraft-iii-reign-of-chaos/faqs/23533) describes Wintermaul's selectable elements, cheap technology mazes and anti-air, earth splash, ice slowing, and expensive fire damage. Its routing discussion emphasizes successive defense areas and mandatory checkpoints. This supports distinct builder rosters and meaningful downstream roles. Its Undead tower list duplicates the electrical list, so that list is not treated as reliable data.

The author description of [Mega Man Maul 3.0 Final](https://wc3maps.com/map/2823), credited to Aestul and AtreyuRock, documents 35 levels, flying enemies every fifth level, lumber at level 25, and an area-protection toggle. Direct inspection of its builder/object text subsequently confirmed eight selectable builders, grouped by games I–VIII. Each lists six ordinary towers plus a signature Buster requiring those six. Builder themes include speed, splash, balance, range, specialty effects, cheap mazing, heavy damage and versatility. The downloadable reference was inspected only as data; none of its code, object files or assets are included in the game.

[Wintermaul Redux 1.08g](https://www.hiveworkshop.com/threads/wintermaul-redux-1-08g.121401/) describes race revisions, upgradeable towers, armor variation, shared bounty and leaver redistribution. Those are evidence for that derivative, not proof of the original Wintermaul rules.

## Implemented direction

Rimewatch offers four original builder factions before spawning. Rime Covenant offers slowing and ice splash; Stonebound emphasizes durable maze pieces and ground area damage; Ember Assembly trades cost for rapid fire and artillery; Volt Vanguard uses arm-cannon sentries, chaining electricity and precise air defense. Each has five exclusive tower designs, including a cheap maze piece and an air specialist. Ironfold separately offers eight robot factions with seven designs each. Each final champion requires the owner to have all six regular designs standing; selling a prerequisite locks future champion construction, without removing already-built champions. Slow effects expire and do not stack multiplicatively. Upgrades retain the purchased tower's identity.

All three Rimewatch lanes and all four Ironfold lanes remain active. One shared team budget is split among the active player slots. Towers can be built on any legal open terrain. Complete blockage triggers enemy siege, while congestion alone does not. Flying waves occur every fifth round.

The current playable campaign is twenty waves, with the original opening ten preserved. Thirty-five-wave progression, armor tables and hero leveling are **not implemented**. Milestone wood now unlocks extra Rimewatch rosters or Ironfold champions; see MAUL-ECONOMY-IMPLEMENTED.md. They need deliberate balancing and clearer version evidence; no hidden assumptions are presented as historical facts. The latest user request supersedes the original online deferral; see ONLINE-PLAY.md.

## Layout interpretation awaiting later user review

- The supplied ASCII maps are the source of truth for terrain; visual scenery is an independent layer.
- Rimewatch keeps all blocked cells, including the lower plug. Its exit sits in the last reachable central channel immediately above that plug. Confirm the intended terminal area later.
- Ironfold uses half-unit source cells, preserving its 128×128 geometry in a 64×64 world. Player tower footprints remain one world unit.
- Sealed top and side pockets are scenery. Spawns occupy the walkable tops of the three/four lanes, following the user's explicit direction rather than guessed marker meanings.
- Both maps have eight selectable builder starts. Solo receives the full team wallet and automatically starts at Last Stand; lane selection is for multiplayer. Some diagnostic fixtures deliberately choose other starts.
- Character-like technology towers are original silhouettes and names; no Warcraft or Mega Man models, sounds, icons or extracted map data are shipped.
- Area protection is not imposed: the user authorized building throughout the shared map. Local slot switching remains a testing option; direct host/join sessions now bind players to authenticated slots (see ONLINE-PLAY.md).

## Next fidelity work

Test longer progression and faction combinations before expanding to 30–35 waves. Add clearer upgrade identities, boss waves and enemy armor counterplay only with readable previews. The verified Mega Man reference is 3.0 Final; do not assume it exactly matches the user’s remembered revision. Preserve freeform mazing and downstream recovery as the center of play.

Research archive SHA-256: `305e39b0029636b85005cc15ccdfff7ad7012658c52ea29fe350fbe2662a240b`. Inspected `war3map.wts` and `war3map.w3u` with a scratch-only MPQ reader. Only paraphrased findings and original game data are committed.

## Economy audit — 2026-10-09

[Historical economy comparison](MAUL-ECONOMY.md) records inspected archive values and per-wave bounty tables: Wintermaul X5 starts at 60 gold per player; the archived Mega Man 3.0 Final file starts at 550. Both use native killer bounty and increasing completion rewards, with extra income for the final defender. Versions differ. That initial audit preceded the implemented economy pass. Current team openings are 240 for Rimewatch and 2,200 for Ironfold; bounty rises from 1 to 5 and milestone wood is implemented. See MAUL-ECONOMY-IMPLEMENTED.md. Preserve the user's four-player-equivalent shared economy when testing any future rebalance.
