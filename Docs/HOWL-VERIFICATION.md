# Howl for Maul verification

Current source checkpoint: Unity 6000.3.25f1 compiles with zero errors and warnings. The complete Unity suite passed **42/42 tests** in 20.78 seconds, including the real Play-mode integration test. Results are in Howl-Unity-Tests.json.

The suite covers all four lanes at every player count, independent spawn queues, permanent terrain in normal and breach navigation, shared bottom-exit routing, conserved player economy, chosen starts, independent builders/ownership, difficulty, tower roles, splash targeting, upgrades/refunds and queued construction. Original movement/diagonal/crowd/siege tests remain included.

The normal-difficulty reference defense completes all ten waves (680 total enemies) using paid builder construction. Real Play-mode checks cover map setup, two-player local control, chosen starting positions, tower catalog, rendered builders/towers and switching maps.

The headless project has been renamed to Headless/HowlForMaul.Headless.csproj. Its 41 simulation cases are included in the passing Unity suite.

This checkpoint is being pushed at the user's request while final visual review and refreshed desktop builds are in progress. Earlier FrostMaze executable builds predate this revision. Do not treat historical build reports as verification of these new binaries.

Online networking remains intentionally unimplemented. Multiple player slots are locally controlled. Final difficulty/art/audio and standalone-platform play testing remain open.
