# Friends Playtest 2 — release verification

2026-10-10. Exact game source: `f6ca21f87e19a60d9fa641051e90657db7392f5a`. The release tag points to this game checkpoint; later packaging/documentation commits do not change its binaries. Full underlying art, campaign and build evidence: [Elemental Factions](../Elemental-Factions/README.md).

## Packaging and extracted runtime

- Both player trees and every recorded source hash match retained verification. Deleted source files remain absent. Game source/configuration has no diff from the tagged commit.
- Linux archive: 196 files. Windows archive: 197 files. Complete notices, font licenses, Scott Buckley attribution, start guide, build metadata and per-file hashes are included. Debug-only folders, symbols and logs are excluded.
- Every archived member was read back and hashed. Both archives were then extracted into new folders; every extracted payload hash, exact file count and build source revision matched. Linux launcher/executable modes are preserved.
- The extracted Linux binary passed both-map data smoke. The extracted shell launcher passed title/settings/credits, map choice, solo faction/difficulty entry and 960×600 / 1440×900 HUD checks, then exited cleanly. The small faction and HUD captures above were visually inspected. This uses software OpenGL on private Xvfb :98 with isolated preferences.
- First attempt was blocked by sandbox restrictions on the private display socket, before game verification. After closing that attempt, the same script passed with permission to create the private display. No game code was changed to pass it. All owned processes were closed.
- The packager now accepts either historical final-newline convention for complete tree hashes, while retaining exact file count and byte checks. A small fixture verified both forms and rejects wrong counts, hashes and modified content. This fixes compatibility with the Elemental verification record, not a relaxation of file integrity.

## Limits

Native Windows execution, actual GPU performance, separate-network multiplayer, four-player live sessions and full live online matches remain unverified. Earlier two-process live Relay checks are retained in [Playtest 1 evidence](../Friends-Playtest-1/README.md). Networking is unchanged since that release and was not rerun here. The source has 73 simulation regressions, 13 focused Unity cases, and one two-map historical campaign replay passing; these are not a full Unity-suite or human-playtest claim. Current creature models remain a first procedural thematic pass.

## Publication

[Friends Playtest 2](https://github.com/AdamGardelov/howl-for-maul/releases/tag/v0.1.0-playtest.2) published at 2026-10-10T12:06:44Z as a prerelease, with all four assets. Local originals remain in `Builds/Releases/v0.1.0-playtest.2`. [Manifest](RELEASE-MANIFEST.json), [checksums](SHA256SUMS.txt), and [release notes](../../Releases/v0.1.0-playtest.2.md). [Publication.json](Publication.json) records the public API status, exact tag resolution and all four anonymous downloads. Every downloaded file matches its local size and SHA-256; GitHub asset digests also agree. The signed-in release page visibly shows the correct tag, source commit and prerelease label. Browser screenshot capture was unavailable, so no publication screenshot is claimed. Playtest 1 remains immutable.
