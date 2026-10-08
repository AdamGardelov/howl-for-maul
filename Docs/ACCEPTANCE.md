# Manual acceptance in Unity

Use the shipped map, enter Play, and keep the Console visible. These checks supplement the automated simulation tests.

1. Pan at all four window edges and corners, use Space + left-drag and middle-drag, use WASD/arrows with and without Shift, zoom to both limits, and try panning beyond map bounds. Space-drag must not build or launch a wave; Enter launches. Try a drag starting over UI and leaving/refocusing the game window. Camera orientation stays fixed.
2. Place and sell towers at edges and near each other. Confirm the preview and rendered footprint match the configured dimensions.
3. Toggle grid and navigation overlay. Red crosses mean insufficient clearance, green arrows mean reachable samples, amber marks mean disconnected free samples.
4. Disable tower weapons. Load the sample zig-zag, launch a wave, and watch units take the full detour. Inspect an enemy with Shift-click.
5. While the wave runs, fill a gap completely. Units should turn red, choose a blocking tower, approach and damage it. The target line should identify that tower. Tower health bars should shrink.
6. Sell a tower to reopen the wall before it is destroyed. Enemies should return to orange and resume movement immediately.
7. Repeat without selling. Enemies should destroy a tower, then continue through the opening. Other towers should remain.
8. With the route open, watch a crowd at a one-cell turn. Queueing should not turn units red or cause siege attacks.
9. Restart Play with smaller tower Fill and a smaller wave Radius. Try diagonally opposite towers. Repeat with a larger radius; units should detour rather than clip. Reduce NavigationStep for narrow-passage experiments.
10. Enable weapons. Confirm towers reduce enemy health, kills increment, and ground/air target flags affect target acquisition.
11. Reach wave five. Flying units should follow purple checkpoints across ground walls and never siege towers.
12. Add a second ground checkpoint in the asset, restart Play, and confirm the route passes through it before the final exit.
13. Test a Linux build and a Windows build on their native platforms. Confirm runtime-generated shader materials render correctly outside the editor.

Capture subjective turn/congestion observations and tuning preferences in QUESTIONS.md. Automated path tests do not prove visual quality or camera usability.
