# GitHub friends playtest releases

The owner requested GitHub downloads on 2026-10-10. Use free, clearly labeled prereleases while Windows, separate-network/full-match multiplayer and hardware performance are still being tested. GitHub source pushes do not publish playable downloads. Release assets are complete desktop archives, not standalone executables or Unity project source archives.

**Published:** [Friends Playtest 2 / v0.1.0-playtest.2](https://github.com/AdamGardelov/howl-for-maul/releases/tag/v0.1.0-playtest.2), 2026-10-10T12:06:44Z. All four public downloads (Windows/Linux archives, manifest and checksums) match the verified originals byte-for-byte by size and SHA-256. [Publication evidence](Verification/Friends-Playtest-2/Publication.json). The annotated tag points to game source `f6ca21f`; later packaging/documentation commits do not change these packages. Playtest 1 remains unchanged and available.

## Package a verified checkpoint

The current released game source is `f6ca21f87e19a60d9fa641051e90657db7392f5a`; its local players and checks are recorded in `Docs/Verification/Elemental-Factions/Verification.json`. The example below describes the already packaged release; use a new version/output for subsequent releases.

Run from the repository:

```sh
python3 Tools/package-playtest.py \
  --version v0.1.0-playtest.2 \
  --source f6ca21f \
  --verification Docs/Verification/Elemental-Factions/Verification.json
```

Outputs go under `Builds/Releases/<version>/`, which is ignored by Git. The script refuses changed/untracked game source, changed player trees, missing notices, and overwriting an existing output. Both input packages must match retained verification hashes. The packager supports both historical final-newline conventions for tree serialization, while still enforcing every file byte and the exact file count; deleted source files may not reappear. It excludes Unity `DoNotShip` folders, debug symbols and logs, preserves Linux executable permissions, and includes the launch guide, build metadata and per-file hashes. It rereads every archive member and compares its bytes. Archives use fixed source timestamps. Update the verification record after a new build rather than bypassing the mismatch guard.

Extract both archives and verify the packaged file manifest. Launch the extracted Linux game/launcher and exercise the menu. Check online play from the actual extracted package when networking changes or when preparing the first online release. Record Windows build-only status separately from native runtime testing. Do not ship test logs, private invitation codes, credentials, or debug-only Unity folders. Retain the music and font notices.

## Publish

Create a version tag at the exact game source checkpoint. Create a GitHub prerelease for that tag, copy the matching `Docs/Releases/<version>.md` notes, and attach:

- `Howl-for-Maul-<version>-Windows-x64.zip`
- `Howl-for-Maul-<version>-Linux-x64.tar.gz`
- `SHA256SUMS.txt`
- `RELEASE-MANIFEST.json`

Use a draft until all four assets have finished uploading. Then publish the prerelease and verify its download links and sizes. Update README with the actual published release link. Share the Releases page or versioned release URL; do not use `/releases/latest` for a prerelease-only repository. Do not replace earlier versioned assets silently: publish a new playtest version for new game binaries so friends can identify matching builds.

This workflow does not enable a recurring scheduler or spend on cloud builds. Unity Cloud supplies multiplayer connectivity; the GitHub release supplies the playable download. A Git push alone updates neither the distributed player nor an automatic updater.
