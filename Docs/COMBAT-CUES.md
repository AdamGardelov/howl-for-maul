# Combat readability follow-up

Rendered enemies now briefly flash their crest after damage. Ground defeats leave three small amber shards; air defeats use violet shards at flight height. A red ring identifies an enemy reaching the exit. These are original cosmetic effects; simulation damage, rewards, routing and footprints are unchanged.

The shared effect budget remains 64 objects, including shot traces and splash rings. A leak can replace the oldest effect when the budget is full. Defeat bursts share one cached mesh. Effects have no colliders and do not cast shadows. Hit flashes use simulation ticks; burst/ring animation follows game speed, pauses with pause/setup and clears on a new match.

## Verification

Unity 6000.3.25f1: 71/71 cases passed, including the new hit/defeat/leak Play-mode integration case. It buys a real tower, checks a real nonlethal hit and kill, stages an air unit at its final route checkpoint, verifies pause/setup/resume, saturates the budget with 100 removals, checks a leak still appears at capacity, and verifies reset cleanup. The simultaneous removal load is a diagnostic fixture, not a paid campaign.

Native 1920×884 gameplay capture inspected with a paid Rime tower and staged subjects. The ground defeat is explicitly staged by setting its health to zero; the hit and exit run through World.Step. This is a visibility check, not a natural wave playthrough. The integration case separately exercises a real tower kill.

Cues are emitted for enemies whose views existed before removal. An enemy spawned and killed entirely between presentation synchronizations has no removal view to emit a burst. No full-frame performance improvement is claimed for this follow-up. Current package source provenance remains in Howl-Builds.json.
