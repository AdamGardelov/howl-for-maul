#!/usr/bin/env python3
"""Run two packaged Linux processes via loopback, or opt into real cloud Relay with --relay."""
import argparse
import os
from pathlib import Path
import re
import subprocess
import time


def run_map(player, folder, map_name, relay=False):
    host_log = folder / f"{map_name}-host.log"
    client_log = folder / f"{map_name}-client.log"
    processes = []
    env = dict(os.environ)
    env.pop("WAYLAND_DISPLAY", None)
    base = [str(player), "-batchmode", "-force-glcore"]
    try:
        host_log.unlink(missing_ok=True)
        host_args = (["--howl-relay-host", "--howl-auth-profile", "relayhost"] if relay else
                     ["--howl-utp-host", "--howl-network-port", "0"])
        host = subprocess.Popen(base + host_args + ["--howl-network-map", map_name, "-logFile", str(host_log)],
                                env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        processes.append(host)
        deadline = time.monotonic() + (60 if relay else 30)
        match = None
        while time.monotonic() < deadline:
            text = host_log.read_text(errors="replace") if host_log.exists() else ""
            match = re.search(r"HOWL_RELAY_INVITE ([A-Z0-9]+)" if relay else r"HOWL_NETWORK_LISTEN (\d+)", text)
            if match or host.poll() is not None:
                break
            time.sleep(.1)
        if not match:
            raise RuntimeError(f"{map_name}: host did not listen; see {host_log}")
        client_args = (["--howl-relay-client", "--howl-relay-code", match[1], "--howl-auth-profile", "relayguest"] if relay else
                       ["--howl-utp-client", "--howl-network-port", match[1]])
        client = subprocess.Popen(base + client_args + ["-logFile", str(client_log)],
                                  env=env, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        processes.append(client)
        for process in (client, host):
            if process.wait(timeout=100) != 0:
                raise RuntimeError(f"{map_name}: process failed; inspect {folder}")
        for path, expected in ((host_log, "HOWL_NETWORK_HOST_PASS"),
                               (client_log, "HOWL_NETWORK_CLIENT_PASS")):
            lines = path.read_text(errors="replace").splitlines()
            matches = [line for line in lines if expected in line]
            if not matches:
                raise RuntimeError(f"Missing {expected}; see {path}")
            print(matches[-1], flush=True)
    finally:
        for process in processes:
            if process.poll() is None:
                process.terminate()
                try:
                    process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    process.kill()
                    process.wait()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("player", type=Path)
    parser.add_argument("--logs", type=Path, default=Path("Logs/RelayLocal"))
    parser.add_argument("--relay", action="store_true", help="Use actual Unity Relay allocations and anonymous identities (requires linked project and internet)")
    args = parser.parse_args()
    args.logs.mkdir(parents=True, exist_ok=True)
    for name in ("Rimewatch", "Ironfold"):
        run_map(args.player.resolve(), args.logs.resolve(), name, args.relay)
