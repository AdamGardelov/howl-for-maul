# Howl for Maul verification — maps and factions checkpoint

Unity 6000.3.25f1 passed **47/47 tests**, including the real Play-mode integration case. The standalone .NET simulation suite passed **46/46**. Exact results are in Howl-Unity-Tests.json.

New checks compare every source layout cell with collision, compare the terrain spatial index with exhaustive collision, traverse every upper lane, enforce faction rosters, expire slowing, limit chain targets, and verify all eight robot champion prerequisite sets and ownership. Integration covers both maps, setup, wallets, paid construction, faction selection and switching/restarting scenes.

The older synthetic-map test still completes ten waves using paid construction. This is not a claim that every new map/faction combination has been fully balance-tested. Those playthroughs are the next priority.

Fresh Linux and Windows builds succeeded. See Howl-Builds.json. Linux has one expected warning that editor automation is disabled in player builds. Windows additionally reports unsupported ray-tracing shader compilation warnings from Unity packages; the game uses the standard URP renderer, not ray tracing. Windows execution has not been tested on Windows.

The actual Linux executable passed its explicit `--howl-smoke-test`, exiting 0 after loading both packaged map assets and simulating ground and flying enemies through every lane. Rimewatch reported 3 lanes, 4 factions, 20 towers; Ironfold reported 4 lanes, 8 factions, 56 towers. The desktop display reported 0×0 and failed before game initialization, so the successful run used an isolated Xvfb display. This proves packaged-resource/runtime execution, not a full human desktop playthrough.

Live editor visual checks covered map geometry, faction setup and actual paid technology towers. Lighting and overlay defaults were corrected after inspection. Art is original procedural prototype art and still needs polish. The final UI keeps Start Match below the scrolling setup controls.

Networking remains intentionally deferred. Local multi-player slots are not online co-op. The current campaign remains ten waves; longer historical progression, research/lumber, enemy armor and hero systems are not implemented.
