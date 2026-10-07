# Automated acceptance — shared defense

These checks have been run by the agent; user testing is optional.

- Original 22 movement, siege, collision and crowd regression cases pass.
- Purchases reject invalid placement and insufficient funds without charging.
- Selling refunds exactly once; destroyed and unpaid test towers cannot produce refunds.
- Builder orders travel, cancel, replace and revalidate at completion.
- Kill and wave rewards pay once, victory prevents another wave and defeat freezes the match.
- Ground enemies traverse all four ordered checkpoints across the connected areas.
- An actual paid 15-tower construction sequence completes all ten waves, accounting for 255 enemies.
- Unity Play-mode integration enters the real scene, creates a tower via builder order, checks drone/tower GameObjects, switches to Maze Lab, loads its sample maze, returns to the shared map, and verifies a fresh treasury and a single controller.
- Rendered victory view inspected; ground overexposure and faint map labels corrected.
- Live Unity match: 255 kills, zero leaks, 30 lives, 810 gold after ten waves.

The tests establish functional behavior and regression coverage. Difficulty, aesthetics, long-term progression and multiplayer are future work. Standalone build and native-player limitations are recorded in VERIFICATION.md.
