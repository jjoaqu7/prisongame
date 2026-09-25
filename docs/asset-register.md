# Asset register

## AUDIO-01: first sound palette

Status: implemented candidates, awaiting user listening review. The user requested comfy/cozy, satisfying, "umami" sounds and authorized licensed external sources. All source licenses below were checked as CC0 1.0; no attribution required, but credit and provenance are retained. No paid pack or account signup was used.

Sources retrieved: 2026-09-25T03:28:59.173683+00:00. Source-page HTML and included pack licenses are retained in `audio-source/`. Freesound inputs are the publicly served high-quality MP3 previews, not the original WAV downloads. Source files stay outside Unity; only selected processed WAVs ship.

| Source | Creator | License / evidence | Used for |
| --- | --- | --- | --- |
| [Impact Sounds 1.0](https://kenney.nl/assets/impact-sounds) | Kenney | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/); `audio-source/impact-source.html`; `impact-LICENSE.txt` | Concrete footsteps, soft placement, ingredient taps, metal door, shelf |
| [Interface Sounds 1.0](https://kenney.nl/assets/interface-sounds) | Kenney | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/); `audio-source/interface-source.html`; `interface-LICENSE.txt` | Soft sale pluck |
| [paper shuffle.wav](https://freesound.org/people/alec_mackay/sounds/463682/) | alec_mackay | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/); `audio-source/paper-source.html` | Pickup, wrapping, restock rustle |
| [Room Tone In An Apartment Living Room 2](https://freesound.org/people/leonelmail/sounds/329533/) | leonelmail | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/); `audio-source/room-source.html` | Quiet looped room ambience |

## Files and edits

All playable files are under `PrisonGame/Assets/Prototype/Audio01/`. The [manifest](../audio-source/manifest.json) maps each output to the exact source archive member or recording segment, with processing details, durations, peaks and SHA-256 hashes. [Preparation script](../tools/audio/prepare-palette.py) reproduces the edits using numpy, scipy and soundfile; temporary decoder dependencies are under ignored `PrisonGame/Temp/audio-python/`.

Processing: mono conversion, rumble removal, softened upper frequencies, gentle saturation and short edge fades. The room recording has a 750 ms overlap at its loop boundary. No music or intelligible dialogue was added. Sound quality and the subjective cozy/umami fit are not approved by automated waveform checks.

An unused Freesound concrete-footsteps candidate by SoftDistortionFX (https://freesound.org/people/SoftDistortionFX/sounds/465299/, CC0) remains as `audio-source/steps.mp3` with its source page; it is not imported or shipped. No Sonniss assets were needed for this first palette.

Palette preview: [WAV](../audio-source/palette-preview.wav). Order: four footsteps, pickup, wrap, restock, placement, crackers, fruit, door, shelf, sale, then room tone. This is an isolated preview, not a recording of runtime spatial mixing.
