# Next verified work

## Latest state

- Packages updated through ad8bd73: both builds succeed, Linux both-map smoke and actual paid build with wave advice pass. All checks have explicit source provenance in Howl-Builds.json. Earlier inspector/camera checks below retain their historical source checkpoints.

- Current exit/terrain/progression checkpoint: Rimewatch exit centered at (31, 8.5), original terrain washes, faction-aware wave advice and team targeting counts. 62 pure checks and all 77 Unity cases passed; final material-only falloff correction separately compiled and visually inspected on both maps. Paid compact matrix: 10/12 wins; alternative Blast strategy wins, Rime solo maze wins with 30 lives, both mixed pairs win. No balance stats changed. Read EXIT-TERRAIN-PROGRESSION.md.

- Source afe6680 adds owner/refund inspector guidance and disabled foreign/unaffordable actions. Compilation and all 61 pure simulation cases passed. Native two-player inspector fixtures passed. Actual Linux input verified build/select/U-upgrade/sell, with gold 1200 → 1180 → 1160 → 1190 and refund quotes 15 → 30. Normal close exited zero. Read TOWER-OWNERSHIP-UI.md.
- Linux and Windows packages include afe6680, including placement hints from e4cf5e6. Both builds have zero errors; Windows runtime remains untested. Both Linux map/data smoke checks pass. Live inspect/checkpoint hints were checked in the package. Read Howl-Builds.json for per-check source provenance.
- Source 9368630 introduced requested edge scrolling and Space + left-drag. Enter launches waves; Shift speeds keyboard/edge pan. Four focused Unity checks and actual Linux camera input passed. Read CAMERA-CONTROLS.md.
- Source 44b2290 added melee poses, tower impact outlines and destruction rubble. Paid wall-seal integration and native screenshots passed. Last complete Unity suite: 74/74 individually confirmed in XML. There are 76 available cases after camera additions; no full 76-case pass is claimed.
- Normal solo Rime scripted maze cleared twenty waves with 30 lives and a valid ledger using 266 purchases. The arbitrary 48-tower compact solo Rime case remains unresolved. Read Balance/COMPACT-DEFENSE.md.

## Next priorities

1. Preserve user Play sessions and unsaved scenes; packages and metadata are current through afe6680.
2. Continue paid-defense progression and clearer player-facing faction/maze guidance. Compact tests show bot choices matter: 10/12 slot-aware solo wins, role-aware Stonebound wins too, and both compact mixed pairs win. Rime compact remains unresolved; no balance change solely to make that arbitrary restriction pass.
3. Continue original minimalistic, League-inspired presentation. All 76 tower models are implemented as a first procedural pass, not final art. Maintain readable silhouettes and keep every raised scenic prop on blocked mask cells.

## Verification and environment

Live Unity 6000.3.25f1 is available through the host-authorized CLI at port 7800. Sandbox-only process/network checks hide it. Editor/domain transitions can time out; inspect state and retry only idempotent actions. After a build-target transition, a stale Pipeline request may exist with no active Unity test job. Confirm no active test, Play session, compilation or update before recovering it; never duplicate an active run.

The source folder is /home/adam/Documents/Dev/howl-for-maul and is registered in Hub. No further rename is needed. Restore StandaloneLinux64 after Windows builds. The owned isolated display :98 uses llvmpipe at 1440×900. It has no window manager: keyboard tests must explicitly focus the exact Howl for Maul test window. Never direct automated input to the user's :0 desktop.

Native default X11 startup remains unresolved. Older native Wayland/OpenGL smoke reached map markers but crashed at shutdown (139); Wayland/Vulkan exited zero with a protocol warning and is not a graphical-input pass. The initial audio-package headless smoke exited 133 after its map markers. Explicit data-only smoke now skips presentation/audio and passed three repeated checks. Windows is build-tested only. Read Platform/README.md.

Read README.md, HOWL-VERIFICATION.md and MAUL-RESEARCH.md before work. Preserve supplied map masks, all-active top-to-bottom lanes, 1,200 team gold split over 1–4 wallets, independent faction ownership/builders, paid construction/upgrades and freeform mazing. Flush wall seals and half-cell Ironfold placement remain mandatory. Congestion does not trigger siege; a complete blockage does. Online networking remains deferred. No copyrighted assets or new automations. Save nonblocking questions in project docs.
