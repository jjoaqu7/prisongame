# ART-01 - first art reference board

**Status: broad direction agreed; ART-01 complete.** Updated September 23, 2026. Specific assets and lighting treatments will be reviewed in the next sample. Open this file in VS Code and press **Ctrl+Shift+V** to see the pictures.

Agreed starting point: simple cartoon appearance, slightly grimy prison, dark humor, eccentric characters, and clear first-person interaction. User feedback settled the character direction, sufficient surface-detail level, occasional dramatic lighting, and the B1/B2 combination. The usual lighting and interiors should feel "eerily cozy/comfy" while still feeling like a prison. Exact treatments need an in-game test. The images are design references stored in documentation. Owning decisions are in [Art and tone](art-and-tone.md#reference-board-decisions---agreed).

## A. Characters and dialogue

![Schedule I character and dialogue at conversation distance](art-references/schedule-i-4.jpg)

**Source:** Schedule I, TVGS, [official Steam gallery](https://store.steampowered.com/app/3164500/Schedule_I/). Original screenshot URL is in [the source list](art-references/steam-sources.json).

**Study:** a large, simple head; readable eyes and eyebrows; plain clothing shapes; short dialogue with clearly separated choices.

**Proposed application:** distinct head shapes, hair, brows, posture, and uniform fit would help distinguish inmates. Keep the important facial features visible from our usual conversation distance. Use clear dialogue text and readable choices when a conversation needs them.

**Agreed from your review:** proportions close to this, with funny, cute faces, but original designs rather than a direct copy. Our capsule inmate is still a placeholder. **Proposed execution:** develop our own head silhouettes, eye/brow shapes, noses, hair, and expressions within that level of exaggeration. The dialogue styling shown here remains a reference proposal; your approval concerned the characters.

## B. Prison architecture and everyday light

### B1. Tall, open block with daylight

![Alcatraz cellhouse corridor with barred cells, railings and overhead daylight](art-references/alcatraz-cellhouse.jpg)

**Source:** National Park Service / Dave Rauenbuehler, "Looking down Broadway in the Alcatraz Cellhouse," [Alcatraz Island](https://www.nps.gov/alca/index.htm). [Original image](https://www.nps.gov/common/uploads/structured_data/5482A294-DB42-56E0-FCCCD03C986AE1DC.jpg?maxHeight=800&maxWidth=1200&quality=90).

**Study:** repeated barred fronts, metal railings, cell numbers, painted masonry, and overhead daylight. The warm cell fronts and cooler walkway remain distinguishable.

**Proposed application:** simplify those shapes into reusable wall, door, and railing pieces. Keep everyday routes and faces well lit. Apply wear around hinges, handles, wall bases, and frequently touched edges. This photo supplies architectural details; our prison's era, geography, and final layout remain open.

### B2. Enclosed corridor with fluorescent fixtures

![Narrow prison corridor in Bautzen with a continuous ceiling, cell doors, exposed pipes and overhead fluorescent lights](art-references/bautzen-fluorescent-corridor.jpg)

**Source:** Jasper Kortmann, [Corridor in Prison, Pexels](https://www.pexels.com/photo/corridor-in-prison-19526327/), photographed in Bautzen, Germany. [Downloaded image](https://images.pexels.com/photos/19526327/pexels-photo-19526327/free-photo-of-corridor-in-prison.jpeg?auto=compress&dpr=1&h=750&w=1260). Added for this comparison; reference use only.

**What I meant by enclosed:** a continuous ceiling close above the corridor, doors along solid walls, and visible overhead fixtures providing pools of light. B1 instead has an open space several storeys tall with daylight overhead. B2's photograph has cool tones and deep shadows; we can adjust colour and brightness independently of the architecture in our game.

| Choice | Features to compare | Possible application - proposed |
| --- | --- | --- |
| B1 | Tall space, visible tiers, daylight, longer views | Main shared block |
| B2 | Close ceiling, solid walls, overhead fixtures, shorter enclosed views | Cell corridor or service area |
| Combination | Different light and enclosure in different spaces | Open common area connected to more enclosed corridors |

**Agreed from your review:** the combination: an open common area connected to more enclosed corridors. The lighting and interior should usually feel "eerily cozy/comfy," with the prison setting still evident. Exact brightness, colours, ceiling treatment, and fixtures remain to be tested. These references do not change our approved prototype dimensions.

## C. Props and surface detail

![Schedule I close-up of simple containers and equipment on a worn work surface](art-references/schedule-i-8.jpg)

**Source:** Schedule I, TVGS, [official Steam gallery](https://store.steampowered.com/app/3164500/Schedule_I/). Original screenshot URL is in [the source list](art-references/steam-sources.json).

**Study:** simple container shapes, large labels, a worn tabletop, and small variations in otherwise plain surfaces.

**Proposed application:** build a mug, parcel, toiletries, and bunk with clear shapes and restrained wear. Test each at the distance where the player actually uses it. Pick a few recognizable details for each object. Selection of a production or earning activity remains deferred.

**Agreed from your review:** Schedule I's level of surface detail is good enough, and extensive manual detailing is not a priority. **Proposed execution:** reuse materials/textures across props and room pieces, with a few shared wear treatments. A material needs initial setup, then it can serve many objects. Exact asset sources and tools are still open.

## D. Strong lighting accents

![Schedule I interior with bright local purple lighting and dark surrounding surfaces](art-references/schedule-i-0.jpg)

**Source:** Schedule I, TVGS, [official Steam gallery](https://store.steampowered.com/app/3164500/Schedule_I/). Original screenshot URL is in [the source list](art-references/steam-sources.json).

**Study:** local coloured light makes one area visually distinct; much of the surrounding room is in shadow.

**Agreed from your review:** occasional lighting this dramatic is welcome; you described its feeling as cozy and comfy. Your later clarification makes "eerily cozy/comfy" the usual atmosphere of the lighting and interiors as well. Occasional stronger accents can sit within that everyday mood.

**Proposed application:** try a personal lamp, a warmly lit cell corner, or a small area with stronger colour. Review it from the player camera so usable objects and faces remain readable. Specific fixtures, colours, locations, and frequency remain proposed.

## Proposed first sample after review

Make **one inmate and one cell corner** using the approved choices: a wall, barred door, bunk, small personal prop, and representative lighting. Put them in the running Unity scene and review them from the player camera. Interface styling can be tried on the existing interaction prompt at the same time.

Use the agreed funny/cute character direction, Schedule I-level surface detail, combined open/enclosed interiors, and usual "eerily cozy/comfy" atmosphere for that sample. Original character designs and the sample itself still need review.

**Next proposed task:** make two lighting/material treatments of the same cell corner and adjoining corridor in Unity, with a view into the open common area. Judge the mood and readability while walking through it. Then develop the original inmate design. The detailed test brief is in [Art and tone](art-and-tone.md#first-atmosphere-test---proposed-execution); progress is tracked under ART-02/03. The reference-board choices do not approve a finished art sample on the user's behalf.
