# Direct online play

The latest user request explicitly supersedes the original networking deferral. Direct IPv4/TCP host/join is implemented, without third-party networking packages or a service account.

## Join a game

1. Host chooses Rimewatch or Ironfold in the main setup screen, opens Play Online, enters a name and optional password, then Host. Default TCP port: 27888.
2. Copy LAN Invite copies the host's local address and port. Send it to friends yourself. A joiner can paste `address:port` in Host address, enter the same password, and Join.
3. Everyone marks Ready; host begins setup. Everyone chooses a faction and confirms, then chooses a unique starting position and confirms. All choices are visible.
4. Everyone votes difficulty. Most votes wins; Normal wins a tied Normal vote, otherwise Relaxed wins the tie. The match starts automatically after all votes.
5. P starts or joins a pause/resume vote. A strict majority of connected players is required. Votes expire after 20 seconds. One player can pause/resume immediately.

Solo Start Match uses the same faction → start → difficulty sequence without opening a network listener. Multiple local slots remain a development/testing option, clearly labelled.

Internet play requires the host address and TCP port to be reachable. A copied LAN address works only on that network. For remote friends use a reachable public endpoint with router/firewall configuration, or a private network/VPN. No router or firewall settings are changed automatically. There is no public lobby browser, relay, Steam invite integration, automatic NAT traversal, reconnect or host migration. These need a separate service/UX pass. Do not describe same-machine tests as verified internet play.

## Match rules and ownership

The host orders actions and fixed simulation ticks. Clients apply that stream, never their own simulation clock. Commands use the authenticated peer's player slot; client-supplied player numbers are ignored. Existing World validation protects faction rosters, gold, build legality and tower ownership. Local toolbar selection remains local; build orders carry their chosen design. Map/rules fingerprints reject mismatches before play. Periodic state digests stop a desynchronized client rather than silently continuing.

All lanes remain active. 1,200 starting gold is split over one to four players. Choosing a solo starting position now takes effect. Camera, menus, sound settings and tower selection stay local. An online settings menu does not pause everyone; use the vote button. The solo menu freezes the solo simulation.

A disconnected participant is marked absent and the match pauses. The remaining connected players can vote to resume. Existing towers and wallets stay assigned to their original slots; there is no leaver redistribution or replacement joining. If the host exits, clients stop and return through Leave Match. During setup, a departure resets the group to the lobby. Host can kick; late joins are refused after setup begins.

## Transport scope

This is a friends-session prototype, not a hardened public game service. Versioned messages have bounded size and queues, with connection/handshake timeouts and input-rate limits. Passwords use a per-connection nonce and a derived-key proof; the password itself is not sent. Gameplay traffic is not encrypted and there is no verified host identity. Use a trusted host/private network. No account secrets or cloud credentials are required or stored.

## Verification

`dotnet run --project Headless/HowlForMaul.Headless.csproj -c Release -- --network-check`

The recorded runner uses actual TCP connections, including separate .NET host/client processes on both maps. It checks paid purchases, independent wallets, rejected foreign upgrades, ordered ticks and state digests, pause/resume and disconnect recovery. Additional three-client checks cover unique starts, difficulty tie policy, 400-gold wallets, majority and expired votes; password and content mismatch joins are refused. See Howl-Online-Network-Tests.txt. This is loopback verification; cross-machine latency, internet traversal and Windows execution are not tested. Packaged Unity evidence is recorded separately when complete.

Four-player follow-up passes capacity/late-join rejection, 300-gold wallets, non-finite/out-of-map command rejection and three-of-four pause. Unity staged solo setup passes with the selected starting position and session cleanup.

Initial packaged probe failed because its artificial fixed increment advanced connection timeouts faster than wall time in an uncapped player. The opt-in probe now uses actual unscaled elapsed time and a 60 FPS target. This was a probe-only correction; the normal game already used unscaled elapsed time. Packaged verification must be rerun before claiming success.
