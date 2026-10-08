# Next verified work

## Latest state

A subsequent source-only follow-up adds placement cost/blocked-reason hints, compiled and visually inspected at 1920×884. Read PLACEMENT-HINTS.md. Packages remain at camera checkpoint 9368630; include the hint on the next package refresh. Linux target restored and verified.

- Source 9368630 adds the requested edge scrolling and Space + left-drag camera. Enter launches waves; Shift speeds up keyboard/edge panning. Four focused Unity checks and actual Linux mouse/keyboard input passed. Read CAMERA-CONTROLS.md.
- Source 44b2290 adds actual enemy melee poses, tower impact outlines and destruction rubble. Paid wall-seal integration and native close/normal screenshots passed. Full combined Unity suite: 74/74, individually confirmed in XML. There are now 76 available cases after the camera additions; do not claim a full 76-case pass.
- Linux and Windows packages include 9368630; both builds have zero errors. Windows runtime remains untested. Linux build and both data-only smoke checks passed; actual camera input on isolated :98 passed and normal close exited zero. Read Howl-Builds.json for finalized per-platform provenance.
- Normal solo Rime scripted maze cleared twenty waves with 30 lives, valid ledger, 266 purchases and no upgrades. This is separate from the unresolved 48-tower compact solo Rime case. Read Balance/COMPACT-DEFENSE.md and RIME-PAID-MAZE.json.

## Next priorities

1. Preserve user Play sessions and unsaved scenes; packages and metadata are current through 9368630.
2. Continue paid-defense progression and clearer player-facing faction/maze guidance. Compact tests show bot choices matter: 10/12 slot-aware solo wins, role-aware Stonebound wins too, and both compact mixed pairs win. Rime compact remains unresolved; no balance change solely to make that arbitrary restriction pass.
3. Continue original minimalistic, League-inspired presentation. All 76 tower models are implemented as a first procedural pass, not final art. Maintain readable silhouettes and keep every raised scenic prop on blocked mask cells.

## Verification and environment

Live Unity 6000.3.25f1 is available through the host-authorized CLI at port 7800. Sandbox-only process/network checks hide it. Editor/domain transitions can time out; inspect state and retry only idempotent actions. After a build-target transition, a stale Pipeline request may exist with no active Unity test job. Confirm no active test, Play session, compilation or update before recovering it; never duplicate an active run.

The source folder is /home/adam/Documents/Dev/howl-for-maul and is registered in Hub. No further rename is needed. Restore StandaloneLinux64 after Windows builds. The owned isolated display :98 uses llvmpipe at 1440×900. It has no window manager: keyboard tests must explicitly focus the exact Howl for Maul test window. Never direct automated input to the user's :0 desktop.

Native default X11 startup remains unresolved. Older native Wayland/OpenGL smoke reached map markers but crashed at shutdown (139); Wayland/Vulkan exited zero with a protocol warning and is not a graphical-input pass. The initial audio-package headless smoke exited 133 after its map markers. Explicit data-only smoke now skips presentation/audio and passed three repeated checks. Windows is build-tested only. Read Platform/README.md.

Read README.md, HOWL-VERIFICATION.md and MAUL-RESEARCH.md before work. Preserve supplied map masks, all-active top-to-bottom lanes, 1,200 team gold split over 1–4 wallets, independent faction ownership/builders, paid construction/upgrades and freeform mazing. Flush wall seals and half-cell Ironfold placement remain mandatory. Congestion does not trigger siege; a complete blockage does. Online networking remains deferred. No copyrighted assets or new automations. Save nonblocking questions in project docs.
