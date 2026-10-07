# Shared-defense milestone

The default map is Frostline Crossing: 42×24 cells, one spawn, three connected defense areas, four ordered ground checkpoints and a separate flight route. Tinted areas have no collision boundaries. Gold dashed lines indicate checkpoint order; enemies may detour from them through player-built mazes. Purple lines show the flying route.

## Construction

Left click in Build mode dispatches the hovering builder. It moves freely above obstacles. Once within build range it attempts construction, checks current funds and occupancy, and charges only if the tower is created. One pending order is supported. Right click moves the drone, B selects Build, M selects Move, X selects Sell, and Escape cancels the pending order. Orders wait while paused. Selling is immediate anywhere on the map and pays the configured refund for purchased towers only. Full blockage remains legal; enemies attack their chosen obstruction.

The preview turns red for invalid or unaffordable placement and shows the tower's weapon range. It does not promise that a cell will remain clear until the builder arrives.

## Match rules

Start with 300 gold and 30 lives. Towers cost 20, sell for 15, and target ground and air. Each kill gives 2 gold; each completed wave gives 30 once. Each final-exit leak costs one life. Intermediate checkpoints cost no lives. Waves 5 and 10 fly. Launch waves manually; the game ends in victory after wave 10 with surviving lives, or defeat at zero lives. Reset starts fresh. The Maze Lab button switches to the original unlimited-construction test map; switching discards the current match.

## Automated reference defense

The full-match regression builds these cells using actual builder orders and the entire 300-gold budget:

(5,6), (9,6), (13,8), (15,11), (17,14), (19,17), (23,16), (25,13), (27,10), (31,8), (34,8), (36,10), (10,11), (20,11), (30,11).

It then plays all ten waves to a terminal outcome, checking no wave stalls, the player survives, and all 255 enemies are accounted for. This is a reproducible baseline, not a difficulty benchmark or a substitute for future player feedback.

## Architecture

Economy, builder orders and motion, construction checks, rewards, lives and match outcome live in pure C# World. Unity displays simulation state and submits commands. Scenario configures all values. SharedDefense.asset is generated only if absent, preserving later edits. TestMap.asset remains the original lab. There is no networking, ownership, persistence or final art in this milestone.
