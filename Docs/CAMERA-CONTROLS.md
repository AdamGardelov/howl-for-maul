# Camera traversal

Both requested gestures are supported: move the pointer to the outer 16 pixels of the game window to scroll, or hold Space and drag with the left mouse button to grab the map. Middle-drag, WASD/arrows and minimap navigation remain available. Enter replaces Space as the wave-launch shortcut so camera gestures cannot accidentally launch a wave.

Keyboard/edge speed is 28 world units per second at normal zoom, scales with zoom within a bounded range, and doubles while Shift is held. Dragging follows pixel displacement with the camera pitch accounted for. The first drag frame and first frame after regaining focus have zero displacement.

Edge scrolling ignores positions outside the game window, stops when unfocused, and is suppressed during a left-button interaction. Camera gestures suppress world building, selling, selection and builder orders. Drags beginning on the sidebar or minimap cannot transfer into world panning; wheel input over UI is ignored. Setup freezes camera movement. Map bounds and zoom limits remain enforced. Pause still permits camera inspection.

Unity compilation passed. All four Camera-filtered checks passed, with each case confirmed in NUnit XML: gesture sampling, camera motor/UI/setup/bounds, existing tower/camera behavior, and camera-local audio. The preceding complete suite passed 74/74 before these camera changes; a complete 76-case run is not claimed. Packaged mouse verification is pending.
