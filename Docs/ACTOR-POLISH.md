# Tower and builder presentation — 2026-10-09

This presentation pass applies to all 76 tower designs and the builders for all twelve factions. It retains the established weapon silhouettes, targeting/recoil transforms, gameplay footprints, economy and unlock requirements.

## Towers

Box-shaped parts use a shared chamfered mesh with the same half-unit bounds. Generated profiles now have UV coordinates. The five shared faction materials use subtle original stone, brushed-metal or woven surface textures, with different metallic/roughness treatment and winter faction body colors. Foundations gain rim trim, inset faction crests and small service panels or carved brackets. Rigid details remain combined into shared per-design meshes; upgrade markers and aiming/recoil pivots stay separate. Tower portraits use the same models automatically.

## Builders

Rimewatch wardens now have grounded boots, sleeves and gloves, chest and shoulder armor, a belted mantle, a satchel, hooded eyes and a detailed staff. Rime has a frost collar; Stonebound has root antlers; Ember has a forge mask and ember pack; Volt has a storm crest and conductor staff.

Ironfold builders are hovering robotic artisans with a readable head/visor, chest, vented tool backpack, articulated gripper arms, stabilizers and engines. Each faction has a distinct crest/tool configuration and uses its actual tower palette. The builder's owner ring is colored independently of faction to distinguish co-op players choosing the same roster.

Walking/facing and hovering use simulation ticks and actual authoritative movement. Paused ticks retain the pose, and resetting a match or reassigning the local builder to another player resets stale facing. The presentation does not add colliders, movement commands or simulation randomness. Body parts are grouped into shared meshes by material; limbs and staff remain independent for animation. Generated textures and meshes belong to the map and are disposed with it.

This is a procedural art polish pass, not a claim of a finished sculpted/rigged character library or verified crowded-scene FPS. 

## Verification

Final compilation reports zero errors/warnings. `CompleteRosterAndBuildersHaveStableDetailedPresentation` passes (139.43 seconds including Play Mode transitions; CLI aggregate 12.72 seconds). It checks all 76 designs and twelve builders across both maps: mesh UVs, foundation markings, faction palette assignment, no colliders, owner-color changes, facing resets, movement and frozen poses. Cosmetic roster fixtures spend no currency and do not count as campaign evidence. The same test separately purchases a real tower on each map, checks its gold deduction, moves the actual builder and verifies its authoritative position and paused pose.

All twelve final faction lineups and both in-map captures were inspected after lifting the dark metal and adding rear-facing foundation markings. These are actual Unity camera renders. This is focused presentation coverage; no fresh campaign balance, network interoperability or crowded-battle FPS claim is made.
