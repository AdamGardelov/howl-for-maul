# Recognize the defender before reading its name

2026-10-10. The player wants every member of a faction recognizable at a quick glance, as well as strong differences between factions. This pass revises the proportions of all 76 current designs and removes repeated framing that was overpowering individual weapons. It builds on the [living-order charter](FACTION-IDENTITY-CHARTER.md); it does not complete the remaining creature redesigns.

## Shape first

Short starters, broad heavy defenders, tall air weapons and distinctive special pieces should have different dominant shapes. Family identity comes from shared materials and anatomy, not an identical large ring, crane or wing around every member. Names, small ornament and color alone are insufficient.

Rootbound now contrasts a small forked hazel, a squat broad oak, an exposed spiky hawthorn, a tall tiered pine and a trailing willow. Oldbark has a broader trunk and lower canopy; Skybough gains an original layered needle crown; Worldroot has longer hanging leaves. Briar Elder retains its thorn-throwing identity.

The other orders retain their current art and roles while making their existing distinctive parts easier to see:

| Family | Readability cues in this pass |
|---|---|
| Rime Covenant | Plain short sentry, low rounded wall, branching binder, arched bell and tall needle |
| Ember Assembly | Low starter, boxy heavy wall, chimney furnace, tall air lance and raised crucible |
| Volt Vanguard | Short gunner, armored block, electrical cage, long paired barrels and elevated core |
| Pulse Foundry | Lantern, cast press, exposed wheel, long sight, tall beacon, vessel and wardbell |
| Blast Circuit | Different stance heights; Heatkeeper's single tall offset exhaust separates it from the square press |
| Prism Division | Petals reserved for the air specialist and champion; only the focus design carries the orbit |
| Horizon Guild | Compact starter and sight, broad crossbow, standing scope, tall air instrument and raised champion |
| Gravity Works | Rotor, anchor, ringed sphere, stacked resonator, fork, broad shoulders and crossed-halo elder |
| Scrap Frontier | Tool heads remain visible; the crane belongs to one design instead of outlining every cart |
| Overdrive Order | Square jaw, prongs, low round spitter, raised fist, tall wings, paired guns and horned champion; wings on two designs only |
| Tidal Array | Shell cradle, reed fork, spread arms, reservoir, tall spear, pearl branches and shellback; waterwheel on the reservoir only |

Portraits, construction previews and gameplay all use these same models. Projectile release height follows each design's new proportion. No stats, costs, map masks, targeting, build footprint, ownership or sound changes.

## Keep shapes inside the maze

The taller Rime air model exposed an existing fitting error: compressing a slanted static part in its own local X/Z axes did not correctly compress its rotated height. The batched static geometry now receives the width fit in tower axes. All 76 designs at level three pass diagonal cell-envelope checks, including the animated tree family. The models must continue to seal against walls without suggesting walkable gaps through their occupied cells.

## Review at real size

The new Unity audit renders actual 64-pixel portraits and white silhouettes from two fixed playing angles. Every within-faction pair is compared at consistent framing. The near-duplicate threshold is 90% average silhouette overlap: the previous source had 38 of 208 pairs above it; the final pass has none. This is a regression guard, not a guarantee of instant human recognition. Small-sheet and gameplay inspection remains required; later creature models must improve anatomy as well as pass this check.

[Verification, all twelve small sheets and actual game captures](Verification/Defender-Readability/README.md). The focused Unity suite passes 8/8. Both local desktop candidates are updated; Windows is cross-built only. Published Playtest 1 is unchanged.
