# Hearth and Orders verification

Final-source focused run: 9 of 11 cases passed. Two old Rimewatch coordinate fixtures instead opened the Ironfold default. Explicit map selection and fresh match options fixed their setup; both then passed. See Integration-Initial.xml and Map-Fixtures-Corrected.xml. No gameplay validation was weakened. This is a focused set, not a new full-suite claim.

The all-76-model case verifies paid rosters/builders, collider absence, level-3 diagonal footprint bounds, shared batching and cleanup. Combat recoil/pause, projectile identity, owned champion progression, camera/HUD shielding, title state, portraits and preparation forecast are covered by the recorded cases.

Both twenty-wave Hard campaign replays reproduced their existing paid ledgers exactly. The captures contain 339 towers on Rimewatch and 336 on Ironfold. The report's llvmpipe timings are from a software renderer, and are not a target-hardware FPS benchmark. The warmed view synchronization allocates zero managed bytes. These are deterministic replay checks, not fresh human balance judgments.

Screenshots named Order are staged model reviews. Title/HUD/world screenshots come from the actual packaged Linux renderer. Small-window examples are 960×600; ordinary examples are 1440×900. Reference-game assets were not imported.

The isolated build runner initially exited 143 after the Linux build logged success, before the menu smoke began. Subsequent package checks were launched separately. Earlier chained jobs also hit a virtual-display shutdown race; the runner now waits for its own Xvfb process to exit before reusing display :98. Neither is being counted as a successful full pipeline.

See Verification.json for final package/input status, ../Living-World for environment checks, and ../../LIVING-WORLD-ASSETS.json for original texture provenance.

A second focused run covered the new transition input guard: camera/modal and title tests passed. The old solo test assumed three lanes while opening Ironfold; explicitly selecting Rimewatch fixed it, and its pause/music/speed/solo assertions then passed. The initial compact difficulty render inherited a fixed-height button style and clipped its two-line text. The final choices use their own flexible-height style; keep only the final inspected difficulty captures.

Earlier world smoke checks set music to zero and saved shared preferences on teardown. The final source does not save preferences for explicit QA or batch-test runs. The initial real-input session restored the values it observed (sound enabled, effects 1, music 0); later test sessions use a private XDG configuration. The original pre-QA music level was not recorded. If this user's older profile is silent, Music volume in Settings can restore it; do not silently reset all preferences.
