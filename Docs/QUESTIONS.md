# Design decisions for later

No answers are needed to run the current prototype. This replaces the obsolete single-lane, ten-wave baseline notes. Implemented facts live in README.md and MAUL-RESEARCH.md; open decisions below do not override them.

## Current assumptions

- Hovering wardens/drones travel at 9 world units/sec and build within 3 units. They ignore maze obstacles, preventing a trapped builder. Normal clicks replace orders; Shift queues up to 128. Construction is charged only when it succeeds at arrival. Moving or Cancel Orders cancels the queue; Escape opens the paused menu and preserves it.
- Every map lane is always active. The fixed 1,200 starting team budget is split among one to four independently owned local slots. Kill bounty and wave bonuses split without losing integer remainders. Online networking remains deferred.
- The campaign has twenty waves, flying every fifth wave, with thirty shared lives. Each exit costs one life. Players launch waves manually and can build during combat. Three difficulties change health and siege damage, not lane counts.
- Both supplied source masks determine terrain. Towers can seal against walls, including Ironfold half-cell positions. Complete blockage triggers siege; crowd congestion does not. Cosmetic props and effects cannot add collision obstacles.
- Four Rimewatch factions have five designs each. Eight Ironfold factions have six regular designs plus a champion requiring that owner's six prerequisite towers. All have original procedural models.
- Reset, map switching and restarting Play discard the current match. Save/load is not implemented. Same-runtime fixed-step repeatability is tested; cross-platform bit-identical lockstep is not promised.

## Nonblocking questions

1. Should builders eventually walk through their maze, or keep the current hovering freedom? A ground builder needs explicit self-trap recovery rules.
2. Is the twenty-wave match the desired default duration, or should a later extended mode approach Mega Man Maul's documented 35 waves? Lumber/research and extra factions are not silently assumed.
3. Keep a single timeless manual launch, or offer an optional ready timer/early-wave bonus? Multiplayer readiness should be decided together with online design.
4. Should mid-match saving be a priority before online play? It needs versioned simulation snapshots, including queues, targeting, wallets and RNG state.
5. Are the current original roster identities and simplified silhouettes the right direction? The requested League-inspired readability is a target; the procedural assets are an initial pass.
6. Should eventual multiplayer allow explicit gold transfers, shared construction permissions or only separate ownership? Current local slots keep wallets and tower sale/upgrade ownership separate.
7. After human play sessions, does the opening budget feel generous enough across all always-active lanes? Bot wins rely heavily on cheap towers and route knowledge, so they do not establish beginner balance.

8. Camera follow-up: the user says only the camera should rotate and the map should stay still. Current Q/E orbits the camera around its ground focus; terrain coordinates do not move. A gentler limited viewing angle or side lean may be intended, but is not confirmed. Preserve the current tested controls until this visual preference is clarified; do not silently substitute roll or pitch.
