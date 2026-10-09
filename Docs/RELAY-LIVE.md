# Live Relay and compact connection dialog — 2026-10-09

The owner-authorized **Howl for Maul** Unity Cloud project is created, linked and enabled. Its public ID is saved in ProjectSettings. Anonymous Authentication and Relay/DTLS succeed from the packaged Linux game. No paid upgrade was selected and no account secrets are included in source.

## Connection dialog

Pending/error screens now use a centered, content-sized panel (maximum 480 UI units), a smaller logo, readable status, a capped password field, and side-by-side Retry/Back actions. Pending Cancel uses the same compact layout. Long errors/account notices scroll while footer actions remain available. Direct LAN connecting also gets a status dialog. The main lobby/faction gallery is unchanged.

The packaged render fixture passes at 1440×900 and 960×600. Captures of retry at both sizes were visually inspected; the panel no longer stretches down the screen and buttons no longer span its full width. The fixture also returns to the entry screen and starts offline solo. These are scripted render/state checks, not a new manual mouse-input test.

## Dialog typography follow-up

The connection dialog now bundles Cinzel Bold for its heading/actions and Alegreya Sans Medium for status text, captions and password input. The heading/body/input sizes are 18/16/16 UI units, with 14-unit captions and 13-unit action labels; the body remains mixed-case and wraps. Font files are embedded in player data, not resolved from installed OS fonts. Other menus and the HUD retain their existing typefaces in this focused change.

The packaged Linux connection fixture passes again at 1440×900 and 960×600; both retry captures were visually inspected for clipping, wrapping, alignment and text contrast. Linux and matching Windows builds pass; Windows runtime remains untested. This typography-only follow-up does not rerun the earlier live-network tests. The ordinary builder now copies soundtrack credits plus full font licenses/source hashes to both platform outputs. Font binaries are unmodified; see ThirdParty/Fonts/SOURCES.json and its SIL OFL notices.

## Real cloud checks

`Tools/check-multiplayer.py --relay` explicitly opts into real allocations; without it, the existing loopback checks remain local. Two separate packaged Linux processes use distinct `relayhost` and `relayguest` profiles. On both Rimewatch and Ironfold they pass create/code/join, shared lobby/faction/unique lane/difficulty flow, two owner-paid purchases and exact wallets, 420 synchronized ticks, majority pause/resume, client departure pause and remaining-player recovery. All four processes exit zero. The dashboard records two hosted allocations.

The test connects through the actual service using DTLS, but both processes run on the same computer and internet connection. Separate-network reliability, four-player live Relay, full twenty-wave live matches and Windows runtime remain unverified. The prior four-player and delay/loss tests remain local evidence.

## Builds

Updated local candidates: `Builds/Linux-Relay/HowlForMaul` and `Builds/Windows-Relay/HowlForMaul.exe`. Both builds pass and include the public cloud linkage, compact UI and Scott Buckley attribution. Linux is runtime-tested; Windows is build-only. The isolated editor target is restored to Linux. Existing title/older candidate folders are preserved.

The initial Linux attempt stopped before building because the previous virtual display was no longer running. Restoring the isolated display resolved it. Successful builds retain existing Pipeline warnings; ServicesCore also warns because its build check requires a signed-in editor, while this CLI editor has no login token. The saved project ID is present in the built player and both live sign-ins/Relay sessions succeeded. Windows additionally reports existing TowerView.light shadowing and unsupported ray-tracing shader warnings. These were not counted as runtime failures or hidden.

Exact sanitized results: [live-service.txt](Verification/Relay/live-service.txt), [linked-checks.json](Verification/Relay/linked-checks.json). Raw local service logs and temporary invite codes are not committed.

## Next playtest

Run the updated candidate, choose **Play → Multiplayer → Host online**. A guest with the same build chooses **Join with code**. Share the generated code and optional password yourself. Online players do not need Unity accounts. Solo remains offline.

Before publishing broadly, complete the [remaining friends checks](RELAY-NEXT.md), especially an actual Windows/Linux session on separate networks. Host departure ends the match; reconnect and host migration are deferred. Scheduler and mobile stay paused.
