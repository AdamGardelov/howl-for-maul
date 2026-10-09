# Relay online flow — remaining work

The code/SDK integration is implemented; see [RELAY-IMPLEMENTATION.md](RELAY-IMPLEMENTATION.md) for design and exact verification. The pending gate is Unity Cloud account/project linkage and live-service verification, not another transport rewrite.

1. Owner signs into Unity Cloud and creates/selects Howl for Maul. Link through Unity Project Settings → Services and enable Authentication/Relay. Check the intended organization/environment and usage plan before enabling any paid plan. No project/account secrets belong in source.
2. Rebuild matching desktop players with the saved public project ID.
3. Use two distinct identities on separate networks to host/join by code, then verify wrong password/expired code, full lobby, faction/start/difficulty, paid commands, speed/pause, auto waves and disconnect.
4. Complete a twenty-wave multiplayer match and repeat with four participants. Windows needs an actual runtime check.
5. Only then package the friends release, with Scott Buckley attribution, known limitations, source metadata and checksums.

Offline solo and direct LAN remain available. Scheduling/mobile stay paused. Host migration, reconnect and public server discovery remain deferred. Do not describe loopback UDP tests as live Relay tests.
