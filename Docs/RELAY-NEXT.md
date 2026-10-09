# Relay online flow — next milestone

User requested relay-based online play before publishing to friends. Existing sessions use direct TCP 27888. Preserve their host-authoritative simulation, lobby/faction/lane/difficulty stages, authenticated ownership, four-player cap, password challenge, shared speed and pause votes. Offline solo must remain available without any service sign-in.

Unity recommends Multiplayer Services SDK for Unity 6. Use Unity Relay allocation/join codes with Unity Transport, through a reliable ordered packet adapter for the existing Session protocol. Do not silently replace it with UDP lacking reliability/order. Protocol packets need bounded framing/fragmentation and backpressure, matching the current Connection limits. Separate Session from concrete TCP peers first so the direct transport and deterministic tests remain usable.

Intended UI: Play → Multiplayer → Host online / Join with code; visible pending/cancel/retry states, copyable join code and optional password. Keep direct LAN under an advanced option. Signing in anonymously should happen only when Online is chosen. A host departure still ends the game until host migration is explicitly implemented.

## External setup required

ProjectSettings currently has empty cloudProjectId and organizationId. No Unity Cloud project or Relay service is configured. The owner must link the intended Unity Cloud project and enable the required Authentication/Relay services (and review any applicable terms/usage limits) before live service validation. Do not invent an ID, embed account credentials in builds or claim localhost tests establish public relay reliability.

## Verification before distribution

Test adapter packet ordering, fragmentation, overflow, disposal/cancellation and failed allocations. Then use two real Relay clients on separate networks to check code/password failure, four-player capacity, complete setup, paid builds, automatic waves, speed/pause, disconnect and cleanup. Build matching Linux and Windows archives with music notices and version information; Windows launch remains a required real-device check.

Primary references checked 2026-10-09:
- https://docs.unity.com/relay/allocating-binding-joining
- https://docs.unity.com/en-us/mps-sdk/tutorials/relay-and-ngo
- https://docs.unity.com/en-us/mps-sdk/networking/relay-servers
- https://docs.unity.com/relay/networking
