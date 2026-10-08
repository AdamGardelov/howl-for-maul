# Camera traversal

Both requested gestures are supported: move the pointer to the outer 16 pixels of the game window to scroll, or hold Space and drag with the left mouse button to grab the map. Middle-drag, WASD/arrows and minimap navigation remain available. Enter replaces Space as the wave-launch shortcut so camera gestures cannot accidentally launch a wave.

Keyboard/edge speed is 28 world units per second at normal zoom, scales with zoom within a bounded range, and doubles while Shift is held. Dragging follows pixel displacement with the camera pitch accounted for. The first drag frame and first frame after regaining focus have zero displacement.

Edge scrolling ignores positions outside the game window, stops when unfocused, and is suppressed during a left-button interaction. Camera gestures suppress world building, selling, selection and builder orders. Drags beginning on the sidebar or minimap cannot transfer into world panning; wheel input over UI is ignored. Setup freezes camera movement. Map bounds and zoom limits remain enforced. Pause still permits camera inspection.

Unity compilation passed. All four Camera-filtered checks passed, with each case confirmed in NUnit XML: gesture sampling, camera motor/UI/setup/bounds, existing tower/camera behavior, and camera-local audio. The preceding complete suite passed 74/74 before these camera changes; a complete 76-case run is not claimed. The 9368630 Linux package passed an actual input sequence on isolated Xvfb :98 at 1440×900: Start Match, Home, Space + left-drag, top-edge scrolling, then Enter. Captures confirm the first two gestures moved the map while wave stayed 0, gold stayed 1,200 and builder orders stayed 0; Enter started wave 1 across all three lanes. Normal window close exited zero. The window-manager-free test display initially had no keyboard focus (XGetInputFocus returned 0); explicit focus was applied to the owned test window before the successful sequence. This verifies input and rendering on llvmpipe, not native desktop compatibility or GPU performance.

## Camera rotation

Hold Q/E to orbit left/right at 55 degrees per second around the ground focus. Camera pitch now changes smoothly with zoom from 62 degrees at overview distances to 38 degrees close up. Keyboard, edge and drag movement follow screen directions at the current yaw. Home restores the default north-facing orientation at the active builder; End fits the rotated map in the overview. Focus loss prevents rotation, and setup/the Esc menu freeze it. The north-up minimap draws the rotated camera footprint.

The later depth pass uses perspective throughout and ground-ray dragging; see DEPTH-AND-TERRAIN.md for projection details and current validation.
