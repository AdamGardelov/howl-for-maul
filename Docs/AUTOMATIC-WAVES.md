# Automatic wave pacing — 2026-10-09

The first wave waits for an explicit launch. After its last enemy dies or leaks, a full 30 simulation seconds are available to build before the next wave starts automatically. No wave overlaps the previous one. Send Now / Enter can launch early. No timer is scheduled after victory or defeat; a new match starts unarmed again.

The countdown is a fixed simulation deadline (900 ticks at 30 Hz), so pause stops it and all four shared speeds affect it. The host sends authoritative ticks; every peer runs the same schedule. NextWaveTick is part of the state digest. Protocol howl-direct-4 rejects older Gallery clients whose wave rules differ. Everyone needs the same new build.

The top forecast shows the remaining seconds; its launch button becomes Send Now. Details has the same timer and explains speed scaling. Auto pacing is always enabled in ordinary solo and multiplayer setup. MatchOptions.AutomaticWaves=false exists only for historical balance/replay diagnostics: those fixtures buy large batches between waves and must preserve their original timing. They are not evidence that a human can buy those defenses within 30 seconds. Future balance work must distinguish timed play from those historical ledgers.

All 73 pure simulation cases pass, including the exact timer boundary, no automatic first launch, no duplicate clear reward, early launch, restart and terminal behavior. Headless host/client checks pass on both map fixtures: all four speeds, a frozen countdown while paused, identical deadlines/digests, automatic next wave, early final send and victory. Other existing ownership/password/wood/capacity/two-process checks pass too. See Howl-Auto-Waves-Tests.txt and Howl-Auto-Waves-Network.txt. Package/UI verification is recorded after the final build.

Linux-Preview build includes this change. See SETUP-PREVIEW.md for actual standalone checks and remaining verification.
