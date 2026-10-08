#!/bin/sh
# Run from any directory after building the Linux player. No X11/Wayland server needed.
set -eu
repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
player="$repo_dir/Builds/Linux/HowlForMaul"
log_file=${1:-"$repo_dir/Logs/smoke-linux.log"}
if [ ! -x "$player" ]; then
    printf '%s\n' "Build the Linux player first: $player" >&2
    exit 2
fi
mkdir -p -- "$(dirname -- "$log_file")"
log_dir=$(CDPATH= cd -- "$(dirname -- "$log_file")" && pwd)
log_file="$log_dir/$(basename -- "$log_file")"
: >"$log_file"
# Unset display addresses only for the test process, not the user's desktop session.
if timeout 120s env -u DISPLAY -u WAYLAND_DISPLAY "$player" -batchmode -nographics --howl-smoke-test -logFile "$log_file" >"$log_file.stdout" 2>&1; then
    for expected in 'HOWL_SMOKE_DATA_ONLY' 'HOWL_SMOKE_PASS Rimewatch' 'HOWL_SMOKE_PASS Ironfold' 'HOWL_SMOKE_COMPLETE'; do
        if ! grep -Fq "$expected" "$log_file"; then
            printf '%s\n' "Missing smoke result: $expected (see $log_file)" >&2
            exit 1
        fi
    done
    printf '%s\n' "PASS: both packaged maps; clean player exit. Log: $log_file"
else
    result=$?
    printf '%s\n' "FAIL: player exited $result (see $log_file and $log_file.stdout)" >&2
    exit "$result"
fi
