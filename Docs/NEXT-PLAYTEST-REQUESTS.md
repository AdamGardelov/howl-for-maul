# Expanded playtest request — delivered checkpoint

Runtime source 0ca580c implements the latest requested camera, atmosphere, audio and direct online features. The user's online request supersedes all earlier networking deferral notes.

- Camera: themed exterior terrain replaces the black immediate surround. R and the minimap north button restore orientation while preserving focus and zoom. Home returns to the builder; End fits the map.
- Visuals: snow ridges and tiered pines for Rimewatch, rock and copper outcrops for Ironfold. Exterior geometry has no colliders and does not enter the playable rectangle. Original masks, flush wall placement and navigation remain unchanged. Faction attack colors improve tower identity; this is still procedural prototype art.
- Audio: 76 distinct original synthesized tower cues with existing voice limits and leak priority. Independent effects/music sliders. Scott Buckley's Snowfall and Signal to Noise are imported under CC BY 4.0 with in-game/distributed attribution and recorded source hashes. No remixes or copyrighted reference-game assets.
- Setup: host chooses map, friends join an optional-password lobby, everyone readies, then visible faction selections, unique starts and difficulty votes. Solo follows the same selection sequence without a network listener.
- Pause/resume: any connected player can initiate a strict-majority vote; votes expire after 20 seconds. Solo is immediate. Disconnect pauses; remaining players can resume.
- Lobby: names, four-player capacity, ready states, map/rules fingerprint, host start/kick, leave and disconnect handling. Paid commands are authenticated to their player's wallet and towers. Clients follow the host's fixed tick stream.

Verification: 67 pure cases; real TCP checks including separate .NET processes on both maps; capacity/password/ownership/vote checks; focused Unity atmosphere, audio, solo setup and remote-client scene/clock regressions. Final Linux and Windows packages built with zero errors. Two packaged Unity processes per map passed purchases, combat ticks/digests, pause/resume and departure recovery. Isolated Linux graphical input passed staged setup, camera reset, numbered queued construction and Quit. See ONLINE-PLAY.md, WORLD-ATMOSPHERE.md and Howl-Builds.json for exact evidence and limits.

## Remaining product decisions

Direct hosting requires a reachable TCP endpoint (default port 27888). Copy LAN Invite is available; public matchmaking, relay/NAT traversal and Steam invites require a configured provider and are not implemented. Reconnect, host migration and leaver-wallet redistribution remain future work. No cross-device internet or Windows runtime pass is claimed. Nonblocking preferences are in QUESTIONS.md.

The next useful human check is camera/scenery feel and listening to the tower/music mix. Automated checks verify safety, identity and playback configuration, not aesthetic approval. Existing user desktop game remains untouched; launch Builds/Linux-Online/HowlForMaul for the new version.
