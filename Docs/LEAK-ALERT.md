# Off-camera leak warning

The sidebar groups recent exits into an **EXIT BREACHED** warning, highlights team lives and offers **View exit**. The button centers the bottom goal at gameplay zoom without changing the match. It remains above the scrollable build roster, so a player building in the upper lanes can notice a breach below.

The warning reads the authoritative cumulative leak counter. It therefore also catches an enemy that reaches the exit entirely between rendered frames, when no EnemyView exists to emit a world-space ring. Bursts aggregate until four seconds of active real play time after the latest leak; speed 2× does not shorten that window. Pause and setup freeze it, and starting a new match clears it. Lives remain highlighted when five or fewer are left, even after the temporary warning expires.

## Checks

Clean Unity compilation. The new OffscreenLeakAlertAggregatesPausesExpiresAndFocusesExit integration test passed (1/1); raw result: Howl-Leak-Alert-Test.json. It stages two real exits before rendering, checks aggregation, camera focus and unchanged gold/lives/pause, checks double-speed duration, pause and setup across 4.1-second waits, expiry, a new burst and reset.

The prior full suite remains 71/71 at combat-cue checkpoint 05cad27. The new case increases the suite to 72 cases, but only the focused addition was rerun for this UI follow-up; do not report a fresh 72/72 full-suite result.

Native 1920×884 sidebar capture inspected while focused on upper lanes away from the exit. A separate camera-focus capture inspected the bottom goal. These use a staged three-enemy leak in a launched first wave; they are not a natural campaign or a mouse click verification. Packages and platform coverage are tracked separately in Howl-Builds.json.

## Packaged mouse verification

Fresh source 303f2a0 Linux package on isolated :98 at 1440×900 also passed actual mouse input. Started the match, launched an undefended first wave, selected speed 2× and panned to upper lanes using the minimap. Wave one finished with 24 leaks, six lives and 1,320 gold (120 completion bonus). On wave two the warning grouped four recent leaks; pausing held it with two lives and 26 enemies remaining. Clicking View exit moved the camera to the goal while retaining those values and pause. Normal window close exited zero. This deliberately undefended alert check is separate from paid campaign verification. Native desktop and Windows runtime limitations remain unchanged.

Later combined checkpoint: all 72 Unity cases passed after the final silhouette bevels, including leak alerts, combat cues and enemy animation/cleanup. See Howl-Unity-Tests.json for the current full result. Earlier focused-run notes above describe their historical verification order.
