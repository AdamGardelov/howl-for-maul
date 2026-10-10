# Twelve elemental factions

Implemented 2026-10-10. The roster is still **12 factions and 76 defenders**, with the same 4/8 split between the two maps. The selector now leads with the plain theme and shows the named order below it. Existing role balance and ownership remain tied to the original numeric IDs.

| Map | Theme | Order | Visual family |
|---|---|---|---|
| Rimewatch | Ice | Rime Covenant | Winter wildlife: an ear-tufted owl, low sleeping bear, antlered deer, broad crouched toad and long-necked crane. |
| Rimewatch | Nature | Rootbound | Living hazel, oak, thorn, pine and willow guardians. Briar Elder throws hooked thorn clusters. |
| Rimewatch | Fire | Ember Assembly | A sailed newt, clawed coalback, furnace toad, feathered phoenix and long-armed ash elder. |
| Rimewatch | Storm | Stormcallers | A spark jay, curled-horn ram, antlered thunderhart, long-legged heron and broad-winged roc. |
| Ironfold | Tech | Pulse Foundry | Refuge workshop instruments: lantern, press, shearwheel, survey lens, beacon, cooling vessel and wardbell. |
| Ironfold | Beasts | Shellbrood | An insect colony: beetle, stag beetle, burrback, hooked mantis, dragonfly, curved-sting scorpion and brood colossus. |
| Ironfold | Magic | Starweavers | Runic familiars: fox, floating wisps, living moonstone, coiled serpent, moth, pointed-hood wayseer and winged sphinx. |
| Ironfold | Air | Skyward Guild | Pass-dwelling wildlife and wind spirits: owl, open wind spiral, long-eared hare, crane, hawk, windchime and gryphon. |
| Ironfold | Earth | Stonewake | Mountain creatures: low stone tortoise, upright curled-horn ram, boulder-bearing ape, cairn elder, tall ibex, mossbear and mountain ox. |
| Ironfold | Spirits | Lantern Court | Masked guardians with distinct carried objects: hearth wisp, staff walker, umbrella keeper, bell keeper, luna moth, antler guide and arched elder lantern. |
| Ironfold | Dragons | Cinderwing Brood | Hatchling, coiled frost drake, heavy-bellied spitter, tall battering jaw, winged wyvern, two-headed drake and old wyrm. |
| Ironfold | Water | Tidekin | Spring creatures: eye-stalked crab, spiral snail, three-necked hydra, squat toad, kingfisher, trailing jelly and ridged shellback. |

## What is implemented

64 former mechanical designs now have original procedural creature or spirit anatomy, expressive heads and a separate gesture joint. Nature keeps its five previously implemented tree guardians; Tech keeps its seven workshop mechanisms. This is a first complete thematic pass, not final sculpted character art.

Species, body height, posture, horns, wings, tails and carried objects distinguish members within a faction. Creature bodies use matte hide, cloth, bone, chitin or stone palettes instead of universal metal. Faction-colored upgrade markers and ownership cues remain. Model portraits and construction previews render the same bodies as gameplay.

Heads and gestures react to actual shot events and idle on simulation time. Pause freezes them. Animated level-three bounds stay inside the occupied cell at diagonal aim. Cosmetic bodies never add colliders. Projectile forms and release heights follow the new families; Tech and Nature retain their instrument and seed/thorn signatures.

The weapon sound bank gives the living families softer material, breath, wind, stone and water profiles; metallic synthesis is restricted to Tech. All three variations of each design are still synthesized and checked for finite samples and bounded peaks. Music and Scott Buckley attribution are unchanged. These automated checks do not replace a listening/mix session.

Builders receive the new family palettes but retain their preceding wardwright anatomy. Fully bespoke keeper bodies and richer attack-specific limbs remain future art work.

## Defender names, in roster order

- **Ice / Rime Covenant:** Snowcap Owl, Slumberbear, Rimehart, Hailtoad, Aurora Crane.
- **Nature / Rootbound:** Seedling Warden, Oldbark, Briar Elder, Skybough, Worldroot.
- **Fire / Ember Assembly:** Cinder Newt, Coalback, Furnace Toad, Flarewing, Ash Elder.
- **Storm / Stormcallers:** Spark Jay, Grounding Ram, Thunderhart, Storm Heron, Tempest Roc.
- **Tech / Pulse Foundry:** Copper Lantern, Ironhand Press, Shearwheel, Surveyor Lens, Flare Beacon, Cooling Vessel, Hearthbell.
- **Beasts / Shellbrood:** Pebble Beetle, Staghorn, Burrback, Reed Mantis, Glasswing, Briar Scorpion, Brood Colossus.
- **Magic / Starweavers:** Runefox, Wispkeeper, Moonstone, Spellcoil, Starmoth, Wayseer, Astral Sphinx.
- **Air / Skyward Guild:** Breeze Owl, Gust Dancer, Cloudhare, Reed Crane, Gale Hawk, Windchime, Sky Gryphon.
- **Earth / Stonewake:** Pebbleback, Anchor Ram, Boulderhand, Cairn Elder, Crag Ibex, Mossbear, Mountain Ox.
- **Spirits / Lantern Court:** Hearth Wisp, Lantern Walker, Rainkeeper, Bell Keeper, Moon Moth, Antler Guide, Elder Lantern.
- **Dragons / Cinderwing Brood:** Hatchling, Frostcoil, Kiln Spitter, Ramjaw, Dusk Wyvern, Twinflame, Hearth Wyrm.
- **Water / Tidekin:** Brook Crab, Pearl Snail, Reed Hydra, Pool Toad, River Skimmer, Lantern Jelly, Ancient Shellback.

## Existing saves and terminology

The eight Ironfold faction indices remain: Pulse Foundry → Tech; Blast Circuit → Beasts; Prism Division → Magic; Horizon Guild → Air; Gravity Works → Earth; Scrap Frontier → Spirits; Overdrive Order → Dragons; Tidal Array → Water. Rimewatch remains Rime Covenant → Ice; Rootbound → Nature; Ember Assembly → Fire; Volt Vanguard → Storm. Historical balance ledgers retain their old labels; the replay loader resolves both sets to the same indices.

No extra factions, roster slots, summons, roaming units or abilities are introduced. The creature theme describes the defender, not a changed combat rule. Rimewatch retains unarmed maze pieces, while Ironfold retains six regular designs plus a champion. Prices, wood, prerequisites, refunds, damage, targeting, waves, masks and route data are unchanged. Both clients should use matching builds as usual.

## Verification and remaining polish

See [recorded checks and screenshots](Verification/Elemental-Factions/README.md). The small-image test compares all 208 within-faction pairs from two fixed playing angles, alongside actual gallery and map renders. It rejects near-duplicate silhouettes; it is not a human recognition study.

Next: more sculpted anatomy and varied facial expressions, proper keeper silhouettes, coordinated creature-specific attack gestures, and a listening/mix pass under a dense real match. Current families are much more explicit, but some shared face and joint construction is still visible up close. Do not label these procedural models final commercial art.
