# Prison world

## Agreed foundation

The game takes place inside a prison that the player explores in first person. Routines and restrictions shape where the player can go. The final size and number of blocks are open.

For the interior direction, the user selected an open common area connected to more enclosed corridors, combining reference-board B1/B2. Lighting and interiors should usually feel "eerily cozy/comfy" while retaining the prison setting. [Art and tone](art-and-tone.md) owns the mood and visual test brief. Exact heights, materials, lighting, and the larger layout remain open.

## What one cell block means

One cell block describes the proposed first playable development scope. The fictional prison is larger. That version also needs shared spaces, such as a cafeteria, yard, and one work area, so the player has somewhere to go and reasons to meet people.

Other wings can be established through exterior building shapes, fenced views, signs, announcements, and inmates who mention them. We do not need to build complete interiors behind every locked door before testing the game.

Players should understand which areas are reachable, which are temporarily restricted, and which are outside the current playable area. A permanently unavailable wing should not advertise a mission or reward the player cannot reach.

## Proposed layout

| Area | Purpose |
| --- | --- |
| Home block and cell | Storage, cellmate, nearby contacts, personal improvements |
| Cafeteria | Meetings, trading opportunities, shared routine |
| Yard | Social groups, recreation, visible tension |
| One job area | Assigned duties, resources, contacts and opportunities; see [duty discussion](gameplay.md#duty-direction---user-priority-proposed-mechanics-september-25) |
| Corridors and checkpoints | Connections, supervision, access restrictions |
| Later wings | New communities, jobs, and opportunities when developed |

## Access - proposed rules

September 25 clarification: the user wants various rooms and several new characters and has now approved trying the laundry-duty scenario with an adjacent supply room, inmate contact and supervising guard. The broader room/cast count remains open. A connecting corridor and the agreed second inmate contact support this test. **Proposed construction approach:** reusable room prefabs (saved groups of room objects), consistent doorway connections, and room-relative interaction/patrol markers. Start with rough connected spaces and a small cast to test the duty opportunities before finishing many interiors. Duty01 now implements a connected laundry/supply prefab with room-relative work zones and guard markers. Its three named people are placeholder variants; broader room and character expansion remains open. The existing prototype/save data includes world positions; relocating rooms requires checking routes, doors, sightlines, lighting and saved positions rather than assuming everything follows automatically.

See the temporary prototype dimensions below for the current scene; the wider layout above remains proposed.

- Routine access changes through the day: yard time, work assignments, and evening return to the block.
- Automatically display relevant access times, upcoming closures, and reminders. Ordinary movement should not require remembering an exact timetable. Window lengths, consequences of lateness, and the detailed time model remain open.
- Earned access comes through a job, an approved assignment, or a relationship that creates a specific opportunity.
- Temporary restrictions follow events such as fights or inspections, with a visible reason and a way to learn when they end.

The larger design can eventually support several playable blocks. Expanding the map should introduce useful differences in people, goods, jobs, and relationships.

## Design checks

Distances should allow decisions and encounters without turning the day into prolonged commuting. Players need time to notice the environment and improvise within the routine.

Access changes should create understandable choices and preserve a path forward. A closed yard might shift social life into the home block. If a closure affects a mandatory task or delivery, mark the affected objective and explain what happens to its deadline and consequences. The player should not have to discover those dependencies manually. Avoid long stretches with no useful or enjoyable activity.

The open world inside the prison can be bounded and scheduled while still offering several choices about what to do next.

## ROOM-01: temporary prototype layout

Implemented for scale testing in `PrisonGame/Assets/Scenes/Room01_Blockout.unity`. These dimensions and furniture choices are proposals awaiting playtest feedback, not a final prison design. One Unity unit represents one metre in this prototype.

| Area | Floor footprint | Position in scene (X/Z) |
| --- | --- | --- |
| Cell | 4 x 5 m | X -6 to -2; Z -3 to 2 |
| Corridor | 3 x 14 m | X -2 to 1; Z -7 to 7 |
| Common area | 7 x 7 m | X 1 to 8; Z 0 to 7 |

Walls are 0.2 m thick and 3.2 m high, centred on the footprint boundaries; clear internal widths are therefore slightly smaller. Floor tops are at Y=0. The cell opens into the corridor through a 1.4 m wide, 2.3 m high opening. The common area entry is 2.4 m wide and 2.5 m high. ROOM-03 adds a temporary sliding barred cell door, with a fixed control on each side. Its movement and controls are prototype choices for review, not final prison access rules. Corridors end at solid walls in this test scene.

A bunk, desk, stool, shared table, and benches provide static scale references. The top is open to inspect the layout; there is no finished ceiling, lighting design, or artwork. The player spawn marker is at (-3.4, 0, -0.5), facing the cell exit. ROOM-02 adds a player at 0.05 m above that marker, with a 1.8 m tall, 0.6 m wide collision capsule and camera 1.7 m above the player's feet. Gravity settles the player onto the floor.

The Game camera is now first-person; the Scene view can still be used to inspect the whole layout. Open the scene from `Assets/Scenes`; [saved ROOM-01 overview](../PrisonGame/Assets/Screenshots/room01-overview.png). Completion evidence, controls, and next work live in [Progress](progress.md).


## Neighboring cell - agreed addition, September 25

User selected a new cell beside the player's, opened with the key obtained by blackmailing Harris. Duty01 now contains a 4 x 5 m prototype cell immediately north of the existing cell: X -6 to -2, Z 2 to 7, sharing the existing dividing wall at Z=2. Its entrance opens from the same corridor at X=-2, Z=4.5. Reuses the current warm lighting, bunk/desk furnishings and barred-door style. The user subsequently selected another inmate here whom the player kills to close the prototype loop; the inmate and encounter are not yet implemented. Encounter mechanics and consequences remain open in [Gameplay](gameplay.md#neighboring-cell-inmate-encounter---agreed-purpose-mechanics-open). The cell is grouped in a reusable NeighboringCell prefab. Unlock and door position are saved; access rules live in [Gameplay](gameplay.md#six-item-inventory-and-dexs-secret---september-25).
