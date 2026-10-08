# Active expanded playtest request — 2026-10-08

The user's latest instruction explicitly brings online multiplayer into scope; earlier notes saying networking is deferred are superseded. Finish the in-progress Quit/queue/checkpoint package first, then continue these tasks in the same work stream. Do not discard user work or claim unfinished features are available.

- Camera: improve the enclosed-world feeling, replace black outside-map space with themed scenery, and provide a visible/reset shortcut restoring orientation without moving the focus. Home currently resets and jumps to the builder, which is not enough.
- Visuals: refine each map's ground, boundary scenery and tower presentation in the agreed restrained stylized fantasy direction. Decorative geometry must never reduce legal construction or navigation space.
- Audio: distinct original sound identities for tower designs; investigate legally usable instrumental cinematic music, add credit/license records and separate music/effects volume controls. Official Scott Buckley CC BY 4.0 library licensing is a possible source; remixes are excluded. No track has been selected or imported yet.
- Online sequence: host chooses map and creates game; others join/invite into a lobby; host begins setup; everyone picks faction with all choices visible; everyone chooses a unique starting position; everyone votes for difficulty; match begins.
- Match pause/resume: anyone can initiate a vote. Implement clear vote status and a consistent majority rule, with immediate solo behavior. Votes must be tied to connected participants, expire/reset, and not become a permanent pause after a disconnect.
- Lobby essentials: optional password, player names, capacity four, ready states, map/version agreement, host start controls, leave/kick and disconnect handling. Do not treat a local player-slot switcher as online multiplayer.
- Real online connections require transport and authoritative ownership/commands, plus real multi-process validation. Direct hosting can be implemented without a service account; relay/public matchmaking or Steam invites need an actual configured service and cannot be claimed without one. Record tested LAN/loopback/internet scopes separately.

User asked to do all of these and continue autonomously. Save nonblocking design questions here rather than stopping progress. Preserve original source masks, all-active lanes, fixed 1,200 team starting economy, faction ownership and freeform paid maze construction throughout.

Music license references inspected:
- https://www.scottbuckley.com.au/library/using-this-music/
- https://www.scottbuckley.com.au/library/faq/
