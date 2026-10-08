# Linux runtime evidence

Source 465974e: Linux and Windows builds succeeded with zero errors. Linux has one expected Pipeline-disabled warning; Windows has nineteen warnings including eighteen unsupported package ray-tracing shaders. Windows has not been run.

The Linux package passes route/data smoke checks for both maps with a Null graphics device, no display addresses and exit zero. `Tools/smoke-linux.sh` runs this from any directory, bounds the process to two minutes and checks all three success markers as well as the actual exit code. Its default log is `Logs/smoke-linux.log`. This is package-data/simulation verification, not rendering or input coverage.

The same package passed actual mouse Start Match/build/select/upgrade/sell on isolated Xvfb :98 at 1440×900. The tower stood beside the visible exit wall. Gold was 1200 → 1180 → 1160 → 1190. Normal window close returned exit zero. This rendering uses Mesa llvmpipe and does not establish GPU performance or a full-wave human playthrough.

Native desktop limits remain: older default X11 launches failed in XF86VidModeGetModeLine before game initialization. A source 9e656e8 native Wayland/OpenGL probe on the GTX 1080 reached both smoke markers but crashed during shutdown (exit 139, attached Wayland proxies warning). It is explicitly a failed process despite the success messages. No saved graphics defaults were changed. Unity documents Wayland support as experimental; see the [Unity 6.3 player command-line reference](https://docs.unity3d.com/6000.3/Documentation/Manual/PlayerCommandLineArguments.html).

A subsequent source 465974e Wayland/Vulkan smoke reached both map checks and exited zero, but logged a DRM Syncobj surface protocol error. This verifies the route/data path only, not a working native graphical window or input. Saved graphics defaults remain unchanged.

Package refresh a56b875 adds the wave recap and terminal-state corrections. Both builds have zero errors (same 1/19 warning counts), and display-free Linux route/data smoke passes with exit zero. The graphical mouse evidence above remains tied to 465974e.

## Explicit data-only smoke and audio shutdown

The fdfbde8 package completed both map smoke checks but exited 133 during native audio shutdown, with ADTM warnings about ending a manager while a mix was active. The smoke path had unnecessarily created the procedural presentation and its synthetic clips before immediately quitting. Source 75ab3ae skips presentation bootstrap only for the explicit `--howl-smoke-test` mode, then asserts no Prototype/AudioSource was created. Three consecutive Linux runs passed both maps and exited zero without those messages. `Tools/smoke-linux.sh` now also requires HOWL_SMOKE_DATA_ONLY. This is an isolated route/data check, not an audio device or graphics test. The underlying native race is not claimed to be solved for every platform.
