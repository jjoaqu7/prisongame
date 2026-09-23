# Prison world

## Agreed foundation

The game takes place inside a prison that the player explores in first person. Routines and restrictions shape where the player can go. The final size and number of blocks are open.

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
| One job area | Hands-on work, resources, staff contacts |
| Corridors and checkpoints | Connections, supervision, access restrictions |
| Later wings | New communities, jobs, and opportunities when developed |

## Access - proposed rules

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

Walls are 0.2 m thick and 3.2 m high, centred on the footprint boundaries; clear internal widths are therefore slightly smaller. Floor tops are at Y=0. The cell opens into the corridor through a 1.4 m wide, 2.3 m high opening. The common area entry is 2.4 m wide and 2.5 m high. Door leaves and interactions are deferred to ROOM-03. Corridors end at solid walls in this test scene.

A bunk, desk, stool, shared table, and benches provide static scale references. The top is open to inspect the layout; there is no finished ceiling, lighting design, or artwork. The player spawn marker is at (-3.4, 0, -0.5), facing the cell exit. A player controller has not been added.

The current camera is an overhead layout preview. Open the scene from `Assets/Scenes` to inspect it; [saved overview](../PrisonGame/Assets/Screenshots/room01-overview.png). Completion evidence and next work live in [Progress](progress.md).
