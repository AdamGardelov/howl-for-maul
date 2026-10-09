# Match speed

Runtime source ed37d93, with menu spacing corrected in 81824fe, adds 0.5×, 1×, 2× and 3×. Visible minus/plus controls sit next to wave controls, with the selected multiplier between them; settings and the details panel expose the same controls. Keyboard minus/equals and keypad minus/plus adjust one step. New matches reset to 1× and controls clamp at both ends.

Solo changes immediately. In multiplayer the host alone changes one shared speed, shown to all participants. Authenticated client speed requests are ignored. Protocol howl-direct-2 rejects old packages; everyone should use the new build. Pausing retains the chosen speed and still freezes simulation. Pause-vote timeouts use real time.

The host schedules more or fewer unchanged fixed simulation steps per real second. Builders, enemy movement, spawning, targeting, attacks and projectiles stay on the same clock, with unchanged costs, damage, rewards and routes. Clients follow ordered host frames. Unity timeScale remains 1: camera input and soundtrack pitch remain normal.

## Guest controls clarification (2026-10-09)

Guests now see only a read-only `SPEED … · HOST` indicator in the compact bar, details and settings. Minus/plus buttons and shortcut instructions are shown only to solo players and the host. Existing keyboard and server-side authority checks remain unchanged. Host changes still apply immediately; this is not a speed-voting feature.

Unity recompilation passed with zero errors or warnings. The existing network runner passed again, including both-map two-process checks rejecting guest speed changes and synchronizing host speed changes. Its .NET build reports the existing obsolete PBKDF2 constructor warning. No new graphical multiplayer or internet test is claimed.

The Linux-Speed package was refreshed from `f9f5b0c`: build succeeded with zero errors and one existing Pipeline runtime-config warning. Windows-Speed remains the earlier `81824fe` package and still shows disabled guest controls. The refreshed Linux guest UI has not received a new graphical multiplayer test.

## Verification

All 70 pure simulation cases pass. The network runner checks all four solo rates against expected tick counts, rejects invalid rates, verifies pause, and compares the same paid build/combat state at tick 300 across every rate. Both-map separate-process TCP checks verify shared 3× / 0.5× pacing, unauthorized client rejection, paid wallets, ownership, ordered state digests, majority pause/resume and disconnect recovery through 620 frames. Existing password/content refusal, three/four-player and real-time vote-expiry checks also pass. Raw evidence: Howl-Speed-Pure-Tests.txt and Howl-Speed-Network-Tests.txt.

Unity compilation succeeds. The staged-solo integration case passes: selected faction/start, all three lanes, full solo wallet, faster 3× advancement, speed changes during pause without advancing ticks, return to 1×, normal timeScale/music pitch, sound volume and menu freeze/cleanup. The test case reports 123.865 seconds including Play-mode transitions; the CLI aggregate duration reports 8.45 seconds. The earlier status request timed out during domain reload; the completed result confirms the pass. See Howl-Speed-Unity-Test.json. A new complete Unity-suite run is not claimed.

Final Linux-Speed and Windows-Speed packages build successfully from 81824fe with zero errors. Linux records two warnings (Pipeline runtime configuration absent; uncompiled editor-code warning), and Windows nineteen (Pipeline/ray-tracing). The final Linux player visibly contains the corrected 680-unit settings panel, confirming the runtime UI change is present despite that build warning. No editor post-processing code changed. Full warnings and provenance are retained in Howl-Speed-Packages.json; attribution and source files accompany both packages.

Actual isolated Linux input at 1440×900 on llvmpipe verifies staged solo, visible top controls, repeated plus reaching/clamping at 3×, launching an active wave, P pause, minus keys down to 0.5×, settings plus back to 1× and equals key to 2× while still paused. The first menu screenshot exposed a clipped final help line; 81824fe increases its height, and a fresh packaged screenshot confirms all help fits. Both graphical runs quit normally with exit zero and no logged game exceptions. Both-map data smoke passes with exit zero (Howl-Speed-Smoke.txt). Screenshots are local outputs, not included game assets.

Network evidence above is same-machine .NET loopback, not an internet, new packaged multiplayer or Windows-runtime test. Windows is build-tested only. Mobile development remains on hold. The project's overnight scheduler was paused at the user's request after this feature; do not resume it without a new request.
