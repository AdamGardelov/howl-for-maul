# Next verified work

Latest checkpoint: the user-requested battlefield, faction, campaign and menu pass. Read Docs/BATTLEFIELD-UPDATE.md and Docs/Balance/TWENTY-WAVE-BASELINE.md first; earlier ten-wave balance datasets are historical.

- Both maps now have twenty waves, with the opening ten preserved and air every fifth wave. Ironfold's late health budget is calibrated separately. Continue adaptive, multi-position and difficulty testing before extending toward the historical longer campaign or adding late currency/armor systems.
- Towers have distinct role silhouettes, faction palettes and visible upgrade tiers. Setup offers a map overview; match start/Home focus the builder and End restores the overview. Terrain surface detail is cosmetic and source masks remain unchanged. Further art polish is still needed.
- Robot faction tradeoffs and dedicated anti-air reach are strengthened. Existing winter roles remain distinct. Roster-first strategies still lose in Ironfold, so do not call final balance settled or tune from one bot alone.
- A captured 61-unit Ironfold corner jam is fixed with deterministic collision-checked yielding. The regression checks completion, no unit overlap, no wall penetration and no false siege. Do not remove physical collision to solve future crowd issues.
- Normal setup no longer links to the empty test arena. The editor menu is Howl for Maul > Open game. Advanced inspection is collapsed. The simulation lab remains available to tests through SwitchMap(false).

Verification: 59/59 Unity tests and 54/54 headless tests passed. Updated Linux and Windows builds succeeded. The updated Linux executable passed both packaged-map route/data checks on a temporary virtual display. Both headless and windowed native desktop launches failed at X video-mode initialization before game code on :0; use Unity Play mode here until that platform issue is addressed; Windows has not been executed on Windows. Keep these limits explicit.

Tools: the live Unity editor is intentionally left available and restored to StandaloneLinux64. Check its status before use and preserve user work. The isolated :98 smoke-test display was stopped. The source folder remains /home/adam/Documents/Dev/howl-for-maul, correctly registered in Hub. The user's earlier four-tower preview layout was saved to the chat outputs before stopping Play; it is a reference, not a save game.

Preserve all-active lanes, the fixed four-player team economy, faction ownership, freeform mazing, siege rules and exact map masks. Online networking remains deferred. No reference game assets belong in the repository. Do not create additional overnight automations; the earlier scheduled continuation is separate from this directly requested work.
