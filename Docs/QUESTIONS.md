# Design decisions for later

No answers are needed to run this build. Current assumptions are implemented and configurable in SharedDefense.asset.

1. **Builder movement:** a hovering drone ignores towers and enemies. It travels at 9 cells/sec and constructs within 3 cells. This prevents self-trapping during maze construction. Should the eventual builder be a ground unit instead?
2. **Orders:** one pending build at a time. A new valid build replaces it; move or Escape cancels it. Invalid clicks keep the existing order. Gold is charged only on successful construction, after placement is checked again on arrival. Should shift-click queue multiple orders?
3. **Economy:** 300 starting gold; tower 20; sale refund 15; kill reward 2; completed wave reward 30. Destroyed towers do not refund. Remote sale is allowed. Prices are initial tuning, not a final balance claim.
4. **Lives:** 30 shared lives, one per enemy reaching the final exit. Intermediate checkpoints continue the same unit into downstream defenses. Zero lives freezes the match; clearing ten waves with lives left wins.
5. **Map:** Frostline Crossing is a continuous 42×24 field with three tinted defense areas and four ordered ground checkpoints. Area boundaries are visual, not walls or ownership restrictions. Should routes branch or combine in a later map?
6. **Cooperation:** the local prototype has one builder and one treasury. Ownership, permissions, multiple players and network synchronization remain future work.
7. **Waves:** ten explicitly authored entries, with flying waves at 5 and 10. Flying units follow their own central route. Start each wave manually; no time pressure between waves yet.
8. **Siege choice:** blocked enemies use a weighted route through obstacles and attack its first obstruction. Prices, owner and remaining tower health do not influence that route.
9. **Clearance:** tower fill 0.86, enemy radius 0.20, flow-field spacing 0.50. These remain configurable; the low-tower F overlay shows the actual collision footprint and radius.
10. **Difficulty/art:** a verified starter layout can win all ten waves without reinvestment. This is a functional baseline; future work can add tower choices, tougher waves, sound and original art.
11. **Persistence:** reset, switching maps and restarting Play discard the current match. Saving and loading matches is not implemented.
12. **Determinism:** same-runtime fixed-step repeatability is tested. Cross-platform bit-identical lockstep is not promised; an authoritative server remains the intended direction.
