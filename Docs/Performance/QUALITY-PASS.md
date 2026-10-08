# Graphics quality checkpoint

Enabled 2x MSAA and medium soft shadows in the URP pipeline, with the active quality setting matching 2x anti-aliasing. Original geometry, map masks, collision and economy are unchanged.

Inspected native editor captures on both maps, including rebuilt render-pipeline state (the live pipeline initially retained old settings). Fresh Linux and Windows packages succeeded with zero errors; warning counts were 1 and 19 respectively. Windows has not been run. Linux package route/data smoke passed both maps on isolated display :98, exit 0.

An actual Linux package mouse sequence at 1440x900 on :98 started a match, purchased a Shard Sentry (1200 to 1180 gold), selected it and upgraded it to level 2 (1160 gold). This verifies those specific HUD controls. A follow-up sale click returned 30 gold, leaving 1190. Restricted tool visibility initially hid the running display; authorized host access restored the connection. Wave controls and a complete human playthrough are not claimed. The earlier native :0 video-mode failure remains unresolved.

## Matched editor measurement

1920x884 Ironfold overview, paused, no enemies; 60 warm-up and 180 unique sampled frames. Crowd fixture has 324 towers and bypasses purchases for profiling only.

| Fixture | Before median render | Quality median render | Before P95 render | Quality P95 render |
|---|---:|---:|---:|---:|
| Empty | 2.820 ms | 2.968 ms | 3.156 ms | 3.344 ms |
| 324 towers | 8.293 ms | 8.335 ms | 9.214 ms | 9.956 ms |

Draw calls remain 115 / 4405. These short editor-reported timings show a modest median cost and somewhat slower tails. They are not standalone FPS, a busy-wave GPU benchmark, or evidence for other machines. Raw quality samples and summary accompany this note. The last full Unity suite was 65/65 at the preceding minimap checkpoint; it was not repeated for these quality settings.
