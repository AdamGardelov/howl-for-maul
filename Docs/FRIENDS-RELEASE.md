# Friends testing and distribution — 2026-10-09

## Recommendation

Start with a free, restricted itch.io playtest. It supports private download keys and optional page passwords, and restricted pages are absent from browse/search. Ship matched Windows and Linux archives with a short start guide and the music notices. Do not publish the repository or a Unity Editor installation as the playable download. [Official access-control guide](https://itch.io/docs/creators/access-control).

The current implementation is direct IPv4/TCP host/join on port 27888. It has password challenges, four-player capacity, shared setup, authoritative orders/ticks, host-controlled speed and pause votes. It does not provide public discovery, relay join codes, automatic NAT traversal, reconnect or host migration. A download host distributes the files; it does not make peers reachable.

For the first remote test, use a trusted private network such as Tailscale, share only the host machine with the friends, and use its private IPv4 address in the game's Join field. Tailscale documents encrypted private game-server sharing without a public address. Configure access to TCP 27888 as appropriate; the game does not change firewalls. All testers must use the same build. This connection route is a recommendation, not a verified integration in Howl. [Official private-game guide](https://tailscale.com/docs/use-cases/personal-or-at-home-use/share-private-game-server).

For a polished consumer release, add a relay-backed create/join code flow and actionable connection errors. Unity's Multiplayer Services SDK integrates sessions/Lobby/Relay. The current custom TCP transport needs an adapter or replacement; this is not a setting that can simply be enabled. Service/project configuration and a tested relay transport are still required. [Official Unity multiplayer documentation](https://docs.unity.com/en-us/mps-sdk).

## Gates before sending a build widely

- Build both supported desktop targets from one source checkpoint; include version/source information and SHA-256 checksums. Linux runtime checks and Windows build checks are different evidence.
- On a real Windows PC, extract the entire archive and verify launch, faction preview, solo, audio, HUD and quitting. Friends need neither Unity Hub nor the Editor. Windows runtime is not currently verified here.
- Complete a two-person session on different computers/networks, ideally Windows to Linux. Check joining with the right/wrong password, visible faction choices, unique starts, difficulty, independent paid builds, shared speed, pause, waves and leaving. Same-machine loopback tests do not establish internet reliability.
- Repeat with three/four participants. Record latency, desyncs, performance and what happens when a connection drops; currently a host departure ends the session and reconnect needs a new lobby.
- Ship known limitations plus simple instructions to collect the Unity Player.log and the source checkpoint when reporting a problem. Avoid collecting passwords or personal network details in reports.

These are release readiness items; no store upload, account creation, service purchase, firewall change or invitation is performed by this documentation.

## Selling later

A paid indie release is a possible direction. The product case is easy cooperative freeform mazing, distinct rosters, good feedback and replayable challenges. More maps should add different decisions; map count by itself is not a release-quality measure. First validate with friends whether they voluntarily replay, understand leaks/building, enjoy the factions and can join without help.

Use Steam as the primary commercial storefront when the core experience and online flow are dependable; retain itch.io optionally for direct downloads. Steam Playtest is available for wider testing. Steam Direct currently costs US$100 per app, recoupable after US$1,000 adjusted gross revenue, plus applicable tax. Initial onboarding includes identity/tax/bank information, review and release lead times. These are current published terms, not costs already incurred. [Steam Direct](https://partner.steamgames.com/doc/gettingstarted/appfee), [onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding), [Steam Playtest](https://partner.steamgames.com/doc/features/playtest).

itch.io supports a minimum paid price or free downloads, and lets the creator choose its platform revenue share; payment processing/tax still applies. [Pricing](https://itch.io/docs/creators/pricing), [payments](https://itch.io/docs/creators/payments). Unity Personal permits commercial development for eligible creators; its published revenue/funding ceiling is currently US$200,000 over the preceding twelve months. Recheck the applicable entity's eligibility before release. [Unity Personal](https://unity.com/products/unity-personal).

Before a commercial release, review rights for all shipped material, including the supplied/reconstructed reference-map layouts, branding, third-party packages, music and any future purchased art. Original models/code do not by themselves establish rights to every map layout. Preserve an independent identity; do not imply Blizzard, Capcom or the composer endorses the game. This is an unresolved release check, not a claim that the existing layouts are cleared commercially.

## Scott Buckley attribution

Rimewatch: “Snowfall” by Scott Buckley. Ironfold: “Signal to Noise” by Scott Buckley. Both are released under CC BY 4.0. Include the in-game credit, THIRD-PARTY-NOTICES.md and MUSIC-SOURCES.json with every archive. The attribution notice names the composer, links the sources/license and records that recordings are unmodified apart from runtime volume/loop playback. The composer's track pages explicitly permit commercial use with attribution. Gameplay videos should also carry the credit in their descriptions. [Snowfall](https://www.scottbuckley.com.au/library/snowfall/), [Signal to Noise](https://www.scottbuckley.com.au/library/signal-to-noise/), [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
