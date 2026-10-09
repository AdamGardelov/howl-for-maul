# Faction gallery, classic HUD and solo start — 2026-10-09

Runtime source: 6a4abd9. Both new desktop packages contain this checkpoint.

## Changes

- Faction cards show an actual tower portrait, role and opening cost. Hovering previews without
  changing the choice; clicking chooses and a separate confirm button commits readiness.
- The preview exposes the whole five/seven-tower roster, prices, wood, targeting, health,
  damage/interval, direct DPS, range and special effects before choosing. Champions list their
  six named owned prerequisites and the wave awarding wood. One portrait is warmed per frame.
- Solo and a one-person hosted match go directly from faction to difficulty at Last Stand.
  Multiplayer retains distinct starting reservations. Every lane and the full team economy stay active.
- A single four-segment speed selector replaces the separated minus/value/plus UI. The active
  speed is highlighted. Solo/host control it; guests see the shared speed with the HOST label.
  Keyboard minus/equals still works. Resource counters have separate inset gold/wood/lives/wave
  cells. Stone borders, brass trim and corner plates give the HUD a restrained classic RTS treatment.
- Game-menu credits identify the current Scott Buckley track, composer, CC BY 4.0 and website.
  Music & License Credits opens the track's official page. Every new package includes the notices.

## Verification

- Unity recompile succeeded. The headless project built with zero errors and its existing
  SYSLIB0060 password API deprecation warning. Network checks pass all twelve solo factions,
  Last Stand/skip-lane behavior, budgets/all lanes, one-person hosts, two-process paid simulation,
  four speeds, ownership, capacity, passwords and votes. See Howl-Gallery-Headless.txt and
  Howl-Gallery-Network.txt. These are not new twenty-wave balance or full Unity-suite results.
- Focused Unity case SoloStagedSetupUsesLastStandMusicAndAuthoritativeTicks passed. NUnit case
  duration is 156.6634185 seconds; the CLI's 10.22 summary is not total wall time. It checks real
  setup/world creation, speed/pause/music and cleanup. See Howl-Gallery-Solo.json.
- Both builds succeeded with zero errors. Linux: 36.921 seconds, one expected disabled-Pipeline
  warning. Windows: 68.267 seconds, nineteen recorded Pipeline/unsupported ray-tracing shader
  warnings. Editor target restored successfully to StandaloneLinux64. Exact reports and archive
  checksums are in Howl-Gallery-Packages.json.
- Owned Xvfb :98, OpenGL/llvmpipe, actual packaged mouse input: at 1440x900 all Rimewatch cards
  and tower portraits appear; choosing Rime then hovering Volt changes only the preview. Confirm
  enters difficulty directly, starts Rime at Last Stand with 240 gold/three lanes. Clicking 3x,
  then pausing and selecting 0.5x, updates the selector without clearing pause. Esc shows Snowfall
  attribution. Leave/map-switch opens Ironfold's eight cards and seven portraits. Champion hover
  shows 750 gold + one wood, weapon stats and all six prerequisite names.
- At 960x600 the gallery scrolls its body and keeps Confirm/Leave reachable; card selection and
  confirmation work. Normal difficulty starts Pulse Foundry at Last Stand with 2,200 gold/four
  lanes; compact top controls/resource cells and the command grid fit. At restored 1440x900 the
  menu shows Signal to Noise attribution. Quit exits zero. No game exceptions/assertions in the
  completed graphical session log. A first preliminary session exited zero before all intended
  input checks; its cause is not established and it is not counted as the completed sequence.
- Both-map packaged data/route smoke passes with clean exit. Two actual Linux player processes
  pass host/client setup, separate paid purchases, pause/resume and disconnect handling on each
  map at tick 420. This is same-machine loopback, not a different-house internet test.

## Packages and remaining release checks

Builds/Linux-Gallery/HowlForMaul and Builds/Windows-Gallery/HowlForMaul.exe contain the same
runtime source, BUILD-INFO.json, README-PLAYTEST.md, THIRD-PARTY-NOTICES.md and MUSIC-SOURCES.json.
The compressed friends archives exclude debug symbols and DoNotShip directories. Every archived file matches its built source byte-for-byte; required notices and Linux executable permissions are verified. Prior builds
and the user's own player/editor work are preserved.

Windows execution, real remote 2–4-computer sessions, native desktop/driver coverage, audio
listening and guest HUD visual inspection were not performed in this pass. The game remains a
direct-connect prototype with no relay, NAT traversal, reconnect or host migration. Follow
FRIENDS-RELEASE.md before broader distribution. No store upload or service purchase was made.
Mobile and the scheduler remain paused. Map masks, collision, prices and combat stats are unchanged.
