# Mobile status and future work

Status checked 2026-10-09. The initial brief names Windows and Ubuntu as primary platforms and Android/iOS as future targets, and explicitly says not to implement mobile controls yet. Desktop work has followed that scope. No Android or iOS package or device test is verified.

The pure C# simulation is separate from presentation and consumes game actions independently of desktop input. CameraIntent / ICameraInput separate camera intent from its motor; DesktopInput is the only implemented provider. This is useful groundwork, not a finished cross-platform input layer. Building, queuing, selection, inspection and shortcuts in Prototype still directly read mouse and keyboard input. The IMGUI HUD is designed and checked on desktop displays.

Before a phone version can be called playable:

- Add touch camera gestures with clear arbitration between panning and placing towers, including pinch zoom and an on-screen angle reset.
- Introduce build/selection command input that supports touch, an explicit queue toggle in place of Shift, move/cancel, upgrade and remove actions without keyboard shortcuts.
- Adapt menus, tower grid, tooltips and minimap for landscape phone screens, safe areas and readable touch targets; hovering cannot be the only way to discover tower information.
- Build and test Android/iOS separately, including audio interruptions, pause/background/resume and session reconnection behavior.
- Profile dense paid defenses on real target devices, including frame time, memory, heat and battery. Desktop render-request timings do not establish mobile performance.
- Verify multiplayer interoperability and connection UX on phones. Desktop loopback results do not establish mobile or cellular internet behavior.

Do not label an editor touch simulation or successful export as a real-device pass. Retain all-active lanes, the fixed team budget and original map masks. Input/UI adaptation should not silently simplify the rules or change ownership.

Nonblocking decisions for a future mobile implementation: which phone platform to target first, minimum representative devices, and preferred touch build/queue interaction. No decision is needed for the current desktop playtest.
