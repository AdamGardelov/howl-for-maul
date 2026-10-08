# Linux runtime evidence

Source 465974e: Linux and Windows builds succeeded with zero errors. Linux has one expected Pipeline-disabled warning; Windows has nineteen warnings including eighteen unsupported package ray-tracing shaders. Windows has not been run.

The Linux package passes route/data smoke checks for both maps with a Null graphics device, no display addresses and exit zero. `Tools/smoke-linux.sh` runs this from any directory, bounds the process to two minutes and checks all three success markers as well as the actual exit code. Its default log is `Logs/smoke-linux.log`. This is package-data/simulation verification, not rendering or input coverage.

The same package passed actual mouse Start Match/build/select/upgrade/sell on isolated Xvfb :98 at 1440×900. The tower stood beside the visible exit wall. Gold was 1200 → 1180 → 1160 → 1190. Normal window close returned exit zero. This rendering uses Mesa llvmpipe and does not establish GPU performance or a full-wave human playthrough.

Native desktop limits remain: older default X11 launches failed in XF86VidModeGetModeLine before game initialization. A source 9e656e8 native Wayland/OpenGL probe on the GTX 1080 reached both smoke markers but crashed during shutdown (exit 139, attached Wayland proxies warning). It is explicitly a failed process despite the success messages. No saved graphics defaults were changed. Unity documents Wayland support as experimental; see the [Unity 6.3 player command-line reference](https://docs.unity3d.com/6000.3/Documentation/Manual/PlayerCommandLineArguments.html).

A subsequent source 465974e Wayland/Vulkan smoke reached both map checks and exited zero, but logged a DRM Syncobj surface protocol error. This verifies the route/data path only, not a working native graphical window or input. Saved graphics defaults remain unchanged.
