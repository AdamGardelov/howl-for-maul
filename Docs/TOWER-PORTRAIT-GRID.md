# Maul command HUD, portraits and rendered minimap

The HUD follows the supplied WC3 reference with original, restrained stone-and-brass styling: minimap at bottom left, tower commands at bottom right, matching top controls and modal menus. There is no inventory and no permanent central portrait or information block. A small upgrade/refund panel appears only for a selected built tower.

Winter rosters use six slots (five tower portraits plus Remove); Ironfold uses eight (seven portraits plus Remove). Every tile shows its number shortcut and cost or locked state. Selection is highlighted; hovering gives the name, role, description and affordability/unlock guidance. The pending queue count and Cancel remain above the command grid. Remove [X] enters removal mode; hovering an owned tower shows its refund before the click. Selecting a portrait returns to construction. Existing ownership/refund rules are unchanged.

Portraits are cached 160×160 renders of the same TowerView geometry used in play. At most one missing current-roster portrait is prepared per frame. Temporary preview objects never enter World or charge gold. Caches and temporary cameras are cleaned up. Unity's standard render request API is used for URP (https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rendering/renderpipeline/submitrenderrequest).

The minimap background is a cached 512×512 top-down render of the actual map terrain, colours and original scenery. Static scenery uses presentation layer 30; units, construction ghosts, checkpoint markers and UI are excluded. Live unit/tower dots and a clipped rotated camera footprint are drawn separately. The map stays north-up while the main camera rotates. The original exact-mask texture remains as a fallback and retains its earlier independent mask tests. No navigation mask or collision geometry changed.

Validation is recorded in HOWL-VERIFICATION.md and the accompanying focused test results. Historical packaged checks retain their own source checkpoints in Howl-Builds.json.
