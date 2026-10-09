#!/bin/sh
# Run from any directory. Default uses null graphics; set HOWL_SMOKE_GRAPHICS=1 for X11/OpenGL.
set -eu
repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
player=${HOWL_PLAYER:-"$repo_dir/Builds/Linux/HowlForMaul"}
log_file=${1:-"$repo_dir/Logs/smoke-linux.log"}
if [ ! -x "$player" ]; then
    printf '%s\n' "Build the Linux player first: $player" >&2
    exit 2
fi
mkdir -p -- "$(dirname -- "$log_file")"
log_dir=$(CDPATH= cd -- "$(dirname -- "$log_file")" && pwd)
log_file="$log_dir/$(basename -- "$log_file")"
: >"$log_file"
# Some Unity/Linux setups crash during native null-device initialization before managed code.
# The explicit graphical mode still skips game presentation/audio; it needs a working X display.
run_player() {
    if [ "${HOWL_SMOKE_GRAPHICS:-0}" = 1 ]; then
        timeout 120s env -u WAYLAND_DISPLAY "$player" -batchmode -force-glcore --howl-smoke-test -logFile "$log_file"
    else
        timeout 120s env -u DISPLAY -u WAYLAND_DISPLAY "$player" -batchmode -nographics --howl-smoke-test -logFile "$log_file"
    fi
}
if run_player >"$log_file.stdout" 2>&1; then
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
