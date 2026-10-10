# GitHub friends playtest releases

The owner requested GitHub downloads on 2026-10-10. Use free, clearly labeled prereleases while Windows, separate-network/full-match multiplayer and hardware performance are still being tested. GitHub source pushes do not publish playable downloads. Release assets are complete desktop archives, not standalone executables or Unity project source archives.

## Package a verified checkpoint

The current game source is `5d342839eff4875bb9a6a299d0350e95bb4aee3a`; its local players and checks are recorded in `Docs/Verification/Rounded-Walls/Verification.json`. The first release is `v0.1.0-playtest.1`.

Run from the repository:

```sh
python3 Tools/package-playtest.py \
  --version v0.1.0-playtest.1 \
  --source 5d34283 \
  --verification Docs/Verification/Rounded-Walls/Verification.json
```

Outputs go under `Builds/Releases/<version>/`, which is ignored by Git. The script refuses changed/untracked game source, changed player trees, missing notices, and overwriting an existing output. Both input packages must match retained verification hashes. It excludes Unity `DoNotShip` folders, debug symbols and logs, preserves Linux executable permissions, and includes the launch guide, build metadata and per-file hashes. It rereads every archive member and compares its bytes. Archives use fixed source timestamps. Update the verification record after a new build rather than bypassing the mismatch guard.

Extract both archives and verify the packaged file manifest. Launch the extracted Linux game/launcher and exercise the menu. Check online play from the actual extracted package when networking changes or when preparing the first online release. Record Windows build-only status separately from native runtime testing. Do not ship test logs, private invitation codes, credentials, or debug-only Unity folders. Retain the music and font notices.

## Publish

Create a version tag at the exact game source checkpoint. Create a GitHub prerelease for that tag, copy the matching `Docs/Releases/<version>.md` notes, and attach:

- `Howl-for-Maul-<version>-Windows-x64.zip`
- `Howl-for-Maul-<version>-Linux-x64.tar.gz`
- `SHA256SUMS.txt`
- `RELEASE-MANIFEST.json`

Use a draft until all four assets have finished uploading. Then publish the prerelease and verify its download links and sizes. Update README with the actual published release link. Share the Releases page or versioned release URL; do not use `/releases/latest` for a prerelease-only repository. Do not replace earlier versioned assets silently: publish a new playtest version for new game binaries so friends can identify matching builds.

This workflow does not enable a recurring scheduler or spend on cloud builds. Unity Cloud supplies multiplayer connectivity; the GitHub release supplies the playable download. A Git push alone updates neither the distributed player nor an automatic updater.
