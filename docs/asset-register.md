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

## STD-01: code analyzer

Status: in use for development only; excluded from every build platform, so it never ships in the game.

| Source | Creator | License / evidence | Used for |
| --- | --- | --- | --- |
| [Microsoft.Unity.Analyzers 1.27.0](https://www.nuget.org/packages/Microsoft.Unity.Analyzers/1.27.0) | Microsoft | MIT (NuGet licence expression); text from the [repository LICENSE](https://github.com/microsoft/Microsoft.Unity.Analyzers/blob/main/LICENSE) kept beside the DLL | Unity-specific compiler warnings in new code |

File: `PrisonGame/Assets/PrisonGame/Scripts/Core/Runtime/Analyzers/Microsoft.Unity.Analyzers.dll`, the unmodified `analyzers/dotnet/cs` DLL from the NuGet package (SHA-256 begins `da18985c3216a485`), retrieved 2026-09-26. It is built against Roslyn 3.11; Unity 6.3 compiles with Roslyn 4.3.

## ENCOUNTER-01: placeholder hit sounds

Status: placeholder for the step-3 feel review; replaceable once the hit feel is approved. Unmodified files taken from the Kenney pack already recorded under AUDIO-01, renamed to the naming standard.

| Source | Creator | License / evidence | Used for |
| --- | --- | --- | --- |
| [Impact Sounds 1.0](https://kenney.nl/assets/impact-sounds) | Kenney | [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/); `audio-source/impact.zip`, `audio-source/impact-LICENSE.txt` | Quick punches (`impactPunch_medium_000-004`), charged punches (`impactPunch_heavy_000-004`), charged body-thud layer (`impactSoft_heavy_000-003`), shove (`impactSoft_medium_000-003`) |

Files: `PrisonGame/Assets/PrisonGame/Audio/Combat/ImpactPunch_Medium_000.ogg` and the other 17 clips, byte-identical to the pack members, stored through Git LFS.

