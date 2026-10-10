# Relay online flow — remaining work

The cloud project is created, linked and enabled. Matching local Linux/Windows candidates are built; two distinct packaged Linux profiles pass real Relay/DTLS sessions on both maps. See [RELAY-LIVE.md](RELAY-LIVE.md) and [RELAY-IMPLEMENTATION.md](RELAY-IMPLEMENTATION.md).

1. Use the matching desktop candidates on separate networks to host/join by code. Verify wrong password/expired code, faction/start/difficulty choices, paid commands, speed/pause, automatic waves and disconnect. Current live-service checks ran on one computer/network.
2. Complete a twenty-wave multiplayer match and repeat with four participants, including full-lobby refusal. The four-player/loss/delay regressions passed locally, not on live Relay.
3. Verify Windows launch, actual graphics/audio/UI, solo and online on a real Windows machine. Windows is build-tested only.
4. The owner requested a labeled GitHub friends prerelease on 2026-10-10. `v0.1.0-playtest.1` is packaged with credits, known limits, source metadata and checksums, and the extracted Linux player passes fresh live Relay checks on both maps. Publication is awaiting an authenticated GitHub session; see [release workflow](GITHUB-RELEASES.md) and [verification](Verification/Friends-Playtest-1/Verification.json). The Windows and separate-network tests above remain open and are disclosed in the playtest notes.

Offline solo and direct LAN remain available. Scheduling/mobile stay paused. Host migration, reconnect and public server discovery remain deferred. No paid plan was enabled. Keep temporary invitation codes and private player logs out of source.
