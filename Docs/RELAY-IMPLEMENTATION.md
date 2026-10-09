# Online join codes — implementation checkpoint

Online now has a Relay path in addition to direct LAN. The account/project setup and real-internet checks below are still required before publishing this as an online-ready playtest.

## Player flow

Play → choose map → Multiplayer. The host enters a name and optional password, then chooses **Host online**. Guests enter the code in **Join with code** and use the same password. The lobby displays a copyable code; names, readiness, factions, unique starts and difficulty voting use the existing shared setup. All lanes stay active and team money is split between the actual players. Solo remains completely offline. Direct LAN/IP controls are under Advanced.

The host controls speed. Pause/resume requires a majority. If a guest leaves a running game, the remaining players can vote to resume; if the host leaves, the match ends. Reconnect and host migration are not implemented. Codes are private invitations, not public matchmaking listings.

## Implementation

Pinned official SDKs: Multiplayer Services 2.4.0 and Unity Transport 2.7.4. Anonymous Authentication starts only on the online path. Existing Session logic now accepts an ordered packet transport; TCP remains available. Relay allocations permit three guests plus the host and use DTLS. No Unity access credentials are embedded in builds.

The adapter uses the same fragmentation → reliable sequenced pipeline on both ends, a 64-packet reliable window, 16 KiB message bound and 256-message incoming/outgoing queues. Send backpressure retries the oldest message without skipping ticks. Session still validates map/balance fingerprints, command ownership, packet rates, passwords and capacity. All commands and ticks share one ordered stream.

Connecting has progress text, Cancel, a 45-second deadline, clear error text and Retry. Cancelled/timed-out async allocations cannot attach late; unused service allocations expire without binding. Passwords are not retained for retries. Service account notices are retrieved and displayed with acknowledgment/copy controls, including restricted-account notices.

## Unity Cloud setup still needed

`ProjectSettings/ProjectSettings.asset` has empty cloudProjectId and organizationId. The Unity Cloud browser currently requires sign-in. The project owner must sign in, create/select **Howl for Maul**, and link this Unity project to it through Project Settings → Services. Enable Authentication and Relay for that project, review the service terms/usage plan, then save the project settings. No project ID has been invented and no account credentials have been read from disk.

After linkage, rebuild both desktop players so they use the same project and map data. Project ID is public configuration; do not paste secret service credentials into source or chat. Check actual join codes between distinct identities and separate networks before sharing a release.

Local candidates: `Builds/Linux-Relay/HowlForMaul` and `Builds/Windows-Relay/HowlForMaul.exe`. Both include the Scott Buckley music notices. These are not published downloads and still require cloud linkage/rebuild before Relay can connect.

## Verification and limits

- Existing headless direct-network checks pass: both maps, automatic wave timing/speeds/pause, paid wallets/ownership, refused passwords/data mismatch, capacities and shared lobby rules.
- Four focused Unity tests pass: 160 fragmented 4 KiB messages survive 25 ms simulated delay and 3% loss with exact ordering and replies; outgoing overflow closes cleanly; four-player setup/paid purchases/host speed/majority pause/disconnect on both maps; fifth-player/password/data refusal; cancellation and timeout discard late transports while offline solo remains usable.
- These tests use the actual Unity Transport adapter and pipeline on loopback. They do not establish public Relay service connectivity or DTLS behavior against a live allocation.
- Linux and Windows builds succeed (Windows runtime untested). Packaged Linux connection-screen/offline-solo checks pass. Two independent packaged processes pass paid builds, 420 synchronized ticks, pause/resume and disconnect on both maps via the actual adapter (loopback UDP, not cloud Relay).
- Full twenty-wave multiplayer campaigns, Windows runtime, separate-network joining and service-account notification responses remain unverified.

## Repeatable probes

`--howl-online-menu-check <output-directory>` captures the packaged connection screens using an injected gateway; it never allocates a cloud session.

Run `DISPLAY=:98 python3 Tools/check-multiplayer.py /absolute/path/to/HowlForMaul --logs /absolute/path/to/logs` on an isolated display for both-map two-process checks.

`--howl-utp-host --howl-network-map Ironfold --howl-network-port 27888` and `--howl-utp-client --howl-network-port 27888` exercise two packaged processes through the adapter locally. Run with graphics on a supported Linux display. These data-only probes exit automatically after paid purchases, launch, pause/resume and disconnect checks.

Once linked, use `--howl-relay-host --howl-network-map Ironfold --howl-auth-profile relayhost` and read `HOWL_RELAY_INVITE` from its local log. On the other machine run `--howl-relay-client --howl-relay-code CODE --howl-auth-profile relayguest`. Probe password is `smoke`; this is a QA path, not the ordinary lobby. Use distinct auth profiles when testing multiple processes on one computer. Keep invite codes/private player logs out of source commits.

## Sources

- [Unity Multiplayer SDK installation](https://docs.unity.com/en-us/mps-sdk/install-and-upgrade)
- [Unity Transport pipeline order, fragmentation and backpressure](https://docs.unity3d.com/Packages/com.unity.transport@2.6/manual/pipelines-usage.html)
- [Authentication notification API](https://docs.unity.com/en-us/authentication/dsa-notifications)
- Exact Relay allocation, join and Transport APIs checked against the pinned official package source.
