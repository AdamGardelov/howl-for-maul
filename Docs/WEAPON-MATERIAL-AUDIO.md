# Material-based weapon audio — 2026-10-09

The earlier weapon bank differentiated towers mainly through synthesized register and pitch sweeps. This pass replaces the pitched arcade character with original short material transients: a crisp attack, filtered/noisy body, quickly decaying inharmonic resonances, restrained low-end weight and small stereo early reflections. There are no imported recordings or external sound licenses.

Rime weapons emphasize brittle high-frequency cracks, Stonebound lower dusty impacts, Ember rough combustion and crackle, and Volt dry electrical snaps. Ironfold uses mechanical attacks, vented noise and brief metallic body resonances, with parameters differing by faction, weapon role and roster slot. Artillery has a longer low-frequency body; control and chain weapons have textured envelopes. These are authored procedural sounds rather than a sampled Foley library.

Every design has three deterministic variations, selected cyclically per tower design. The public one-argument bank lookup remains stable for callers needing the base clip. Clips are generated lazily at 48 kHz stereo and owned/disposed by the map. DC is removed, energy is normalized with a peak cap, and clip edges fade to prevent boundary clicks. The bank can produce 228 clips across 76 designs; four unarmed maze designs do not emit weapon shots in gameplay, leaving 216 actively used weapon variations.

The 120 ms real-time sound gate remains. In mixed volleys, the least recently heard visible weapon design takes priority; splash breaks ties. This lets multiple weapon types contribute without artillery always replacing the other sounds. Chain links do not add voices. Off-camera suppression, pause/menu/setup/mute handling, alert priority, and no replay of stale shots remain. This pass changes weapon sounds; music and the existing global breach warning remain unchanged.

## Verification

The first live combat audio regression passed (142.16 seconds including Play Mode transitions). It covers the retained volley gate, off-camera/chain suppression, pause/setup/mute, no stale replay, breach priority, and the new three-variation cycle and fair selection between two competing tower designs.

The new bank test initially failed because its editor-only preview called the runtime deferred-destruction API outside Play Mode. Disposal now destroys generated preview clips immediately in Edit Mode while retaining deferred destruction during play. The original test report is retained as `Howl-Material-Audio-InitialTests.json`. The waveform and disposal rerun follows below.

Audition exports are the actual generated game clips: each faction's opener and slot-three weapon, with silence between samples, at 0.65 reference gain. The separate five-second volley files overlay generated samples at the combat source's 0.12 gain; they are synthetic mix stress previews, not recordings of actual battles. Automated waveform checks do not establish subjective audio quality or native-device loudness.

The corrected bank test passes in 8.27 seconds: all 228 stereo clips and variants have unique sample hashes, bounded peak/RMS/DC, finite samples and silent edges. Repeated lookups reuse clips; out-of-range designs return null; all owned clips are released. Both synthetic volley exports remain below full scale. Final compilation reports zero errors/warnings. The disposal change affects Edit Mode only; the previously passed live integration result remains applicable to runtime playback.

## Playable package

`Builds/Linux-MaterialAudio/HowlForMaul` contains runtime source `009ddd9`, including the preceding projectile, actor and environment changes. Linux build succeeded with zero errors and one Pipeline-runtime-disabled warning. Both packaged map/resource/route smoke checks passed with a clean exit on isolated Xvfb/OpenGL. Smoke mode skips audio/presentation and therefore does not verify native audio-device playback; audio integration was checked separately in the live editor and actual bank samples exported. Windows was not rebuilt or runtime-tested for this pass.

```sh
/home/adam/Documents/Dev/howl-for-maul/Builds/Linux-MaterialAudio/HowlForMaul -force-wayland
```
