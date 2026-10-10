#!/usr/bin/env python3
"""Package verified desktop players without Unity, debug symbols or local logs."""
import argparse
import gzip
import hashlib
import io
import json
import re
import stat
import subprocess
import tarfile
import time
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]


def git(*args):
    return subprocess.check_output(["git", *args], cwd=REPO, text=True).strip()


def sha(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def payload_files(folder):
    files = []
    for path in sorted(folder.rglob("*")):
        if path.is_symlink():
            raise ValueError(f"Unexpected symlink in player: {path}")
        if not path.is_file():
            continue
        relative = path.relative_to(folder)
        if any("DoNotShip" in part or part.endswith(".dSYM") for part in relative.parts):
            continue
        if path.suffix.lower() in {".pdb", ".mdb", ".log"}:
            continue
        files.append((relative.as_posix(), path))
    return files


def verify_player(folder, expected):
    entries = []
    for path in sorted(folder.rglob("*")):
        if path.is_symlink():
            raise ValueError(f"Unexpected symlink: {path}")
        if path.is_file():
            entries.append(path.relative_to(folder).as_posix() + "\0" + sha(path))
    serialized = "\n".join(entries).encode()
    # Historical verification records use both final-line conventions. All
    # paths, bytes and the file count must still match one complete record.
    digests = {hashlib.sha256(serialized).hexdigest(),
               hashlib.sha256(serialized + b"\n").hexdigest()}
    if len(entries) != expected["fileCount"] or expected["treeSha256"] not in digests:
        raise ValueError(f"{folder.name} differs from its verified player; rebuild/reverify before packaging")
    for required in ["THIRD-PARTY-NOTICES.md", "MUSIC-SOURCES.json", "ThirdParty/Fonts/SOURCES.json"]:
        if not (folder / required).is_file():
            raise ValueError(f"Missing distribution notice: {required}")


def archive(root, entries, target, epoch):
    expected = {}
    if target.name.endswith(".zip"):
        with zipfile.ZipFile(target, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as output:
            for name, content, mode in entries:
                data = content.read_bytes() if isinstance(content, Path) else content
                member = root + "/" + name
                info = zipfile.ZipInfo(member, time.gmtime(max(epoch, 315532800))[:6])
                info.create_system = 3
                info.external_attr = (stat.S_IFREG | mode) << 16
                info.compress_type = zipfile.ZIP_DEFLATED
                output.writestr(info, data)
                expected[member] = hashlib.sha256(data).hexdigest()
        with zipfile.ZipFile(target) as check:
            assert set(check.namelist()) == set(expected)
            for name, digest in expected.items():
                assert hashlib.sha256(check.read(name)).hexdigest() == digest, name
    else:
        with target.open("wb") as raw, gzip.GzipFile(filename="", mode="wb", fileobj=raw, mtime=0) as compressed:
            with tarfile.open(fileobj=compressed, mode="w", format=tarfile.PAX_FORMAT) as output:
                for name, content, mode in entries:
                    data = content.read_bytes() if isinstance(content, Path) else content
                    member = root + "/" + name
                    info = tarfile.TarInfo(member)
                    info.size = len(data)
                    info.mode = mode
                    info.mtime = epoch
                    output.addfile(info, io.BytesIO(data))
                    expected[member] = hashlib.sha256(data).hexdigest()
        with tarfile.open(target, "r:gz") as check:
            assert set(check.getnames()) == set(expected)
            for name, digest in expected.items():
                member = check.getmember(name)
                assert member.isfile() and not member.name.startswith("/") and ".." not in Path(member.name).parts
                assert hashlib.sha256(check.extractfile(member).read()).hexdigest() == digest, name
    return expected


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--version", required=True)
    parser.add_argument("--source", required=True, help="Exact verified game source revision")
    parser.add_argument("--verification", type=Path, required=True)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    if not re.fullmatch(r"v\d+\.\d+\.\d+-playtest\.\d+", args.version):
        parser.error("Use vMAJOR.MINOR.PATCH-playtest.N")
    source = git("rev-parse", "--verify", args.source + "^{commit}")
    # Packaging/documentation can change; the game and project configuration may not.
    subprocess.run(["git", "diff", "--exit-code", source, "--", "Assets", "Packages", "ProjectSettings"], cwd=REPO, check=True, stdout=subprocess.DEVNULL)
    untracked = git("ls-files", "--others", "--exclude-standard", "Assets", "Packages", "ProjectSettings")
    if untracked:
        raise ValueError("Untracked game files must be verified and committed first")
    verification_path = (REPO / args.verification).resolve()
    verification = json.loads(verification_path.read_text())
    for name, digest in verification["sources"].items():
        if sha(REPO / name) != digest:
            raise ValueError(f"Source differs from verification: {name}")
    for name in verification.get("deletedSources", []):
        if (REPO / name).exists():
            raise ValueError(f"Deleted source has reappeared since verification: {name}")
    output = args.output.resolve() if args.output else REPO / "Builds/Releases" / args.version
    if output.exists() and any(output.iterdir()):
        raise ValueError(f"Refusing to overwrite existing release files: {output}")
    # Validate both players before producing either archive.
    for platform in ("Linux", "Windows"):
        verify_player(REPO / "Builds" / (platform + "-World"), verification["packages"][platform])
    output.mkdir(parents=True, exist_ok=True)
    epoch = int(git("show", "-s", "--format=%ct", source))
    release = {"release": args.version, "gameSourceCommit": source,
               "verification": args.verification.as_posix(), "verificationSha256": sha(verification_path),
               "archives": {}}
    guide = (REPO / "Docs/PLAYTEST-START-HERE.txt").read_bytes()
    for platform in ("Linux", "Windows"):
        folder = REPO / "Builds" / (platform + "-World")
        root = f"Howl-for-Maul-{args.version}-{platform}-x64"
        entries = [(name, path, 0o755 if path.stat().st_mode & 0o111 else 0o644)
                   for name, path in payload_files(folder)]
        build = {"game": "Howl for Maul", "release": args.version, "gameSourceCommit": source,
                 "platform": platform + " x64", "unity": verification["unity"], "networkProtocol": "howl-direct-5",
                 "verification": {"Linux": "packaged runtime/render checks passed",
                                  "Windows": "cross-build passed; native Windows runtime not verified",
                                  "online": "earlier two-process live Relay checks passed on one computer/network; separate-network and full live matches remain unverified"},
                 "originalVerifiedPlayerTreeSha256": verification["packages"][platform]["treeSha256"]}
        entries += [("BUILD-INFO.json", (json.dumps(build, indent=2) + "\n").encode(), 0o644),
                    ("START-HERE.txt", guide, 0o644)]
        if platform == "Linux":
            launcher = b'#!/bin/sh\nset -eu\ncd -- "$(dirname -- "$0")"\nexec ./HowlForMaul "$@"\n'
            entries.append(("Start-Howl-for-Maul.sh", launcher, 0o755))
        payload = {name: sha(value) if isinstance(value, Path) else hashlib.sha256(value).hexdigest()
                   for name, value, mode in entries}
        entries.append(("FILE-HASHES.json", (json.dumps(payload, indent=2, sort_keys=True) + "\n").encode(), 0o644))
        target = output / (root + (".tar.gz" if platform == "Linux" else ".zip"))
        contents = archive(root, entries, target, epoch)
        release["archives"][target.name] = {"sha256": sha(target), "bytes": target.stat().st_size,
                                         "files": len(contents), "platform": platform, "rootFolder": root}
        print(f"Verified {target.name}: {len(contents)} files, {target.stat().st_size / 1048576:.1f} MiB", flush=True)
    manifest = output / "RELEASE-MANIFEST.json"
    manifest.write_text(json.dumps(release, indent=2) + "\n")
    sums = [(name, value["sha256"]) for name, value in release["archives"].items()] + [(manifest.name, sha(manifest))]
    (output / "SHA256SUMS.txt").write_text("".join(digest + "  " + name + "\n" for name, digest in sums))
    print(f"Release files: {output}")


if __name__ == "__main__":
    main()
