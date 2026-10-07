# Adding maps

Map geometry and gameplay data are authored in `MapDefinition` ScriptableObjects. The default map is `Assets/Game/Maps/Resources/Frostfall.asset`. The original `TestMap.asset` remains the unrestricted lab; `SharedDefense.asset` is a historical prototype and is not offered in the multi-lane selector.

1. Duplicate Frostfall.asset into a Resources directory and give the asset a unique name.
2. Set Settings.Name, Width and Height. Grid coordinates use X/Y; simulation Y becomes Unity Z, so high Y appears toward the top of the map.
3. Author Lanes. Every lane has its own Spawn, ordered GroundRoute and ordered FlightRoute. All lanes stay active; each Wave.Count is the number spawned PER LANE. A blocked spawn delays only its own queue.
4. Route lanes through defense areas and toward the shared bottom exit. The terrain must allow a path between consecutive ground checkpoints. Flying routes ignore ground obstacles but must stay inside the map.
5. Author Terrain rectangles with integer X/Y/Width/Height. They block construction and ground movement, including breach navigation. They have no health and cannot be sold or attacked.
6. Author BuilderStarts and matching StartNames. Supply at least four unique selectable positions for the current four-player setup; more positions are supported. SoloBuilderStart is independent. Builders hover and are not ground-path blocked.
7. Author Catalog entries: display name, description, cost/refund, footprint, health, weapon interval, damage, range, ground/air targeting and optional splash radius. Catalog index order drives toolbar keys.
8. Author Waves explicitly. Flying is a flag on each wave, not a simulation rule derived from wave number. Difficulty applies runtime copies and leaves the asset untouched.
9. StartingGold and WaveReward are TEAM totals, distributed among active players. KillReward is the team reward per defeated enemy. Integer remainders rotate between wallets.
10. Enter Play. Valid multi-lane map assets in Resources appear in the setup map selector automatically. Restart after editing an asset; runtime settings are copied to protect authored data.

The legacy Spawn/GroundRoute/FlightRoute fields serve the lab and overview/debug markers. For multi-lane maps, each lane's route is authoritative. Do not remove legacy fields while old map assets still use them.

Run the simulation suite and the Unity integration tests after changes. For a new map, add a case that sends units through every lane, verifies no terrain intersection, and accounts for every final-exit leak. Add a reference-defense playthrough before calling a map balanced.

The internal C# namespace/assembly names retain FrostMaze to preserve serialized compatibility. Product name, UI, builds and repository are Howl for Maul.


## Reference layout maps

`Assets/Game/Maps/LayoutSources/Rimewatch.txt` and `Ironfold.txt` preserve the supplied ASCII masks. `ReferenceMaps` interprets rows from top to bottom and maps them to increasing world Y toward the top. Rimewatch cells are one unit; Ironfold cells are half a unit. Blocked horizontal runs are merged without deleting cells. Terrain blocks use floating point bounds and a spatial index; towers keep integer world-grid footprints.

`ProjectSetup` creates missing Rimewatch/Ironfold assets. It does not overwrite authored assets on every compilation. After deliberately changing a layout or its factory, regenerate the corresponding asset explicitly and re-run mask and route tests. `SelectableMap` controls whether it appears in the match menu. `Theme` controls original procedural scenery independently of collision.

Faction rosters are map data. Match options select one faction per player. The simulation rejects designs outside that roster; UI filtering is not the only enforcement. Ground and air targeting, splash, slow duration and chain limits live on tower specifications.
