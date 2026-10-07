# FrostMaze — shared-defense prototype

An original cooperative-maul prototype for Unity 6.3 LTS, C#, and URP. The default map now includes three connected defense areas, a controllable hovering builder, gold, ten waves, shared lives and victory/defeat. Networking and final art remain future work; the original unlimited-construction Maze Lab is also available. All geometry is procedural. There are no Warcraft or Mega Man assets.

## Start playing

1. Install Unity **6000.3.25f1** through Unity Hub. Linux uses the included Mono backend; add **Windows Build Support (Mono)** for Windows builds.
2. In Hub, choose **Projects → Add → Add project from disk** and select this folder.
3. Let Unity resolve its official URP, Test Framework, and editor-only Pipeline automation packages. First import creates the map asset, URP renderer/pipeline, and shader-backed materials.
4. Choose **FrostMaze → Open maze lab**, then press **Play**. The scene is intentionally empty in edit mode; it constructs Frostline Crossing on entering Play.
5. Left click to dispatch the builder to construct towers, then select **Launch Wave**. Right click moves the builder. Use the sidebar to switch to **Maze Lab** for unlimited construction and the sample zig-zag maze. Switching maps resets the match.

### Controls

| Action | Control |
|---|---|
| Dispatch builder to construct | Left click in Build mode |
| Move builder / cancel pending construction | Right click, or M then left click |
| Cancel pending construction | Escape |
| Sell tower | X then left click (right click also sells in Maze Lab) |
| Build mode | B |
| Inspect enemy | Shift + left click near enemy |
| Launch next wave | Space |
| Pause | P |
| Pan | WASD / arrows / middle mouse drag |
| Zoom | Mouse wheel |
| Toggle grid | G |
| Toggle clearance, low tower visuals, footprints and enemy radii | F |

The sidebar exposes speed, reset, navigation diagnostics, weapon toggling, and selected-enemy state. Frostline Crossing starts with 300 gold and 30 lives. Towers cost 20, refund 15 on sale, and kills/waves reward 2/30. Construction is unlimited only in Maze Lab. The builder holds one order at a time; a new valid build replaces it, moving cancels it, and payment occurs on successful arrival. Full route blockage is allowed. Placement overlapping a ground enemy, an occupied tower footprint, the spawn or a ground checkpoint is rejected; those constraints do not check route availability.

## Tune the experiment

Choose **FrostMaze → Select map parameters**, edit the asset, then restart Play. Runtime settings are copied from the asset to avoid mutating authored data.

- `Width`, `Height`: shared rectangular map, one world unit per placement cell.
- `NavigationStep`: sample spacing, default 0.5. Lower values resolve narrower physical passages at greater cost.
- `Tower.Width/Height/Fill`: grid footprint and collision shape. Fill 0.86 leaves 0.14 units between adjacent single-cell towers. Wider footprints subtract this margin from the overall rectangle, not every cell.
- `Waves[].Radius`, `Speed`: enemy clearance and speed. Radius is authoritative for both navigation and swept movement collision.
- `Separation`, `Acceleration`: local steering. Separation is capped relative to forward speed to avoid force cancellation in crowds.
- `CheckpointRadius`: physical trigger radius. A unit reaches it when its disc touches the trigger, avoiding crowds trying to occupy one exact point.
- `BreachCost`, `AttackReach`: obstacle-route weighting and melee reach.
- `GroundRoute`, `FlightRoute`: ordered checkpoints on the **same shared map**. Add downstream defenses by extending the route. Ground units can leak through successive areas; they are not isolated lanes.
- `Waves`: explicit data. The shared map has ten entries, with 5 and 10 marked flying; the simulation reads the flag on each entry rather than inferring movement from the wave number.
- `Tower`: health, damage, attack interval, range, ground/air targeting.

See [shared-defense rules and reference layout](Docs/SHARED-DEFENSE.md).

## Verification

Run the rendering-independent tests with the installed .NET 10 SDK:

```bash
dotnet run --project Headless/FrostMaze.Headless.csproj -c Release
```

Or use **Window → General → Test Runner → EditMode → Run All** in Unity. Both runners execute the same 29 simulation cases; Unity additionally runs a Play-mode integration test for the builder visuals and switching maps. No external test package is needed for the .NET runner.

Build with **FrostMaze → Build Linux** or **FrostMaze → Build Windows**. Output goes under `Builds/`. Batch entry points are `FrostMaze.Editor.ProjectSetup.BuildLinux` and `FrostMaze.Editor.ProjectSetup.BuildWindows`.

```bash
/path/to/Unity -batchmode -nographics -projectPath /path/to/FrostMaze \
  -runTests -testPlatform EditMode -testResults /tmp/FrostMaze-tests.xml \
  -logFile /tmp/FrostMaze-tests.log

/path/to/Unity -batchmode -nographics -quit -projectPath /path/to/FrostMaze \
  -executeMethod FrostMaze.Editor.ProjectSetup.BuildLinux \
  -logFile /tmp/FrostMaze-build.log
```

Do not run a second Unity editor against the same project while it is already open.

If you edit scripts during Play, stop and restart Play after recompilation; the authoritative simulation is deliberately not serialized across editor domain reloads.

See [navigation decisions](Docs/NAVIGATION.md), [implementation plan](Docs/PLAN.md), [manual acceptance](Docs/ACCEPTANCE.md), [deferred questions](Docs/QUESTIONS.md), and [verification status](Docs/VERIFICATION.md).
