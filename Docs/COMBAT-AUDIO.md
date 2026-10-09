# Combat audio pacing

Current weapon timbres and mixed-volley selection: see [material-based weapon audio](WEAPON-MATERIAL-AUDIO.md). The initial implementation/history below predates the replacement of oscillator sweeps and the fair per-design selection gate.

Weapon sound now uses a 120 ms real-play-time gate instead of allowing two voices on every rendered frame. The strongest visible splash cue takes priority within a volley. A shot is audible when its source or target is in the camera view; chained arcs do not trigger additional weapon sounds. The rate limit stays the same at speed 2×.

A new original descending tone marks leaks, even outside the camera view. Leaks in one update produce one cue, with at least 600 ms before another breach cue. It interrupts weapon audio and reserves 220 ms before the next weapon cue. Pause, setup and the combat-sound toggle mute the source; consumed old events do not replay after resuming. New matches stop old voices and reset the gates.

All clips are synthesized locally and owned by the current map. Existing bolt/cannon sounds remain original; the breach chirp falls from roughly 660 to 220 Hz over 220 ms with a squared decay envelope. No external samples were imported.

## Verification

Clean Unity compilation. CombatAudioLimitsBurstsFollowsCameraAndPrioritizesLeaks passed (1/1): a 100-shot volley dispatches one prioritized cannon cue; a timed stream remains within the cadence bound; off-camera and chained shots stay quiet; pause/setup/mute suppress dispatch without replay; three real exits produce one global breach cue ahead of a visible weapon; reset clears audio state. The test checks dispatch and AudioSource mute behavior, not a subjective listening assessment. Raw result: Howl-Audio-Test.json.

The preceding combined Unity run passed 72/72 and is fully recorded from NUnit XML. The available suite now has 73 cases; only the focused audio addition was run for this follow-up. Do not claim a fresh 73/73 full run.

A WAV preview was exported from the actual generated Unity clips (bolt, cannon, breach, separated by silence) at the source's 0.12 volume. Sample values are finite and bounded; export metadata is in Howl-Audio-Clips.json. This is a dry cue preview, not a recording of a full battle mix or a perceptual audio-quality certification. Current build/platform evidence remains in Howl-Builds.json.

Package follow-up: an initial display-free smoke reached both map markers but crashed during native audio shutdown (133). The explicit smoke path now skips presentation/audio initialization. Three repeated runs passed with clean exit; normal Play-mode audio integration was rerun separately. See Platform/headless-audio-shutdown.json and Howl-Builds.json.

The fresh focused audio integration rerun passed 1/1 after recovering a stale, inactive Pipeline request left by the build-target transition. Recovery first confirmed Unity had no active test job, Play session, compilation or update. No user scene was closed. The last complete suite remains 72/72; no 73-case full-suite claim is made.

The normal 75ab3ae Linux package was separately driven with actual mouse input on isolated display :98: Start Match, a paid Shard Sentry, wave 1 and 2× speed. It completed with 5 kills, 19 leaks, 11 lives and 1,310 gold (1,200 − 20 + 10 + 120). The process later exited zero without recorded ADTM or exception messages, but it had already exited before our close helper ran; the cause of that clean exit is not established. This is not a subjective listening check or a native-display compatibility pass.
