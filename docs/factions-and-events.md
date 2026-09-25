# Factions and events

## Agreed direction

Include gangs and rivalries. Gang fights should be unpredictable and capable of changing the playing dynamic, including trade between groups. Guard rotations should involve variation and consistency; the balance is open.

The latest clarification allows substantial complexity and pressure, provided the game makes their application clear and handles tracking. Rivalries, fights, and guard behavior can affect the player directly. Relevant effects must appear in the feedback system described in [Gameplay](gameplay.md), with understandable causes, timing, and consequences.

## Gangs and rivalries - proposed rules

Give each gang recognizable members, interests, relationships, and influence over an activity or place. Specific gang identities and membership rules have not been chosen.

Randomness can determine starting rivalries and influence when tensions escalate. Rivalries should then persist and change through events rather than being completely rerolled each day.

For a first implementation, two groups can have a small set of relationship states: trading, tense, and hostile. A broken deal, dispute, or player decision can increase tension; restitution or an agreement can reduce it. Random incidents can occur between NPCs without player involvement.

## Fights and consequences - proposed rules

Use tension, opportunities for contact, and guard presence to influence the chance of a fight. Include enough visible or overheard clues that players can sometimes anticipate trouble, while allowing surprises.

A possible sequence:

1. A dispute worsens relations between two groups.
2. A confrontation during a shared activity turns into a fight.
3. Guards separate the groups and temporarily close an area.
4. Trading between them stops; a supplier becomes unavailable.
5. The game marks the blocked supplier and affected orders. The player can compare deadlines and consequences before finding another supplier, mediating, waiting, or doing something else.

Consequences should last long enough to matter. Injured or disciplined NPCs may be absent, relationships may change, and an alternative supplier may gain business. Keep the first version's aftermath manageable: one closure and one disrupted trade connection are enough to test the idea.

Tune the frequency and overlap of incidents through playtesting. If an event interrupts a favor or delivery, automatically update its status and show whether the deadline continues, changes, or pauses under the chosen rules. Do not silently change those rules or require the player to discover every affected order manually.

How dangerous incidents are to uninvolved players, whether they can threaten belongings, and how losses work remain open. Consequences should be understandable and the player should have a recoverable path forward. Rivalries can produce lasting changes in who cooperates, what is available, and which opportunities emerge.

## Guard rotations - proposed rules

Use a persistent cast of guards with recognizable personalities and ordinary recurring shifts. Randomized substitutions, patrol assignments, and responses to incidents add variation. This is a proposed game system, not a claim about real prison staffing.

A player might usually encounter the same officer at morning work detail, then discover a stricter replacement today. Announcements, a duty board, or observation can communicate the change. Decide how much notice to provide during playtesting.

Guard differences can affect routines, enforcement, and risky activities. Show any relevant assignment or rule change where the player needs it. When a guard becomes suspicious, the player should know who is watching, what caused it, how it is changing, and what may happen next. Remembering every patrol or manually checking every roster should not be required to understand an active threat.

Guard memory should persist across shifts. Suspicion should follow something witnessed, reported, or discovered, with enough feedback for the player to understand the consequence.

## Guard suspicion test - implemented, provisional rules

The user authorized the first guard-suspicion implementation on September 24. `Guard01_Suspicion` adds one controlled scenario to the timed snack-request scene. The following choices are provisional for review; they do not establish final guard enforcement or character design.

- A marked staff-only corner occupies the far right/back of the common room. Officer Harris is a temporary guard using the existing M5 rig with a navy uniform and cap, not a new approved character design. The sign and HUD identify him and the boundary.
- Suspicion rises only when Harris sees the player inside the corner. The test uses a six-metre sight range, a 120-degree field of view and solid-geometry occlusion. Ordinary common-area activity does not raise suspicion.
- Suspicion rises by 12.5 points per gameplay second, warns at 40, and caps at 100 after eight visible seconds. At 100 Harris orders the player to leave. The HUD names him, explains what he saw, shows whether suspicion is rising or falling, and tells the player how to respond.
- Leaving the marked area or breaking his view lowers suspicion by 20 points per gameplay second. At zero, a message announces resolution. Re-entering in view starts a new episode; warnings are not repeated every frame.
- Suspicion is personal to the player and this guard; it is not a prison-wide alert. The guard's position and restricted boundary are world state. Settings/focus pause freezes suspicion with the prison clock. An active snack deadline otherwise continues, and both statuses remain visible.

This tests detection, escalation feedback and resolution. Harris is stationary and does not pursue, search, arrest, confiscate items or impose a lasting penalty; behavior beyond the order to leave remains open. Guard memory across shifts, rotations, multiple-guard coordination and saving are not implemented. These limits do not remove those proposals from the design. Actual captures: [marked corner](images/guard01-overview.png), [active order to leave](images/guard01-alert.png). Verification and next work live in [Progress](progress.md).

## Supply interruption test - implemented, provisional rules

The user authorized continuing with the next planned disruption test on September 24. `Supply01_Inspection` extends Guard01 with a controlled stock inspection. The trigger and duration below are provisional playtest choices, not final event rules.

- The first successful snack-pack sale closes the common-table supply box for ten game minutes (50 unpaused real seconds). This happens once per play session. Later sales cannot restart or extend it.
- The box refuses all collections and purchases during inspection without charging money. Existing ingredients, unfinished work and finished packs remain usable. A physical closed notice and the HUD identify the cause and reopening time.
- The request deadline continues. Its panel reports how many remaining packs require more supplies, accounting for raw ingredients, finishable work and carried/stored packs. If existing stock covers the request, the panel says so instead of marking it blocked.
- At the reopening time, the notice disappears, restocking resumes and the blocked-request message clears automatically. Reopening does not extend the request or undo late pricing. Settings/focus pause freezes the inspection with the existing prison clock.
- Closure is shared world state; its controlled trigger observes the solo player's first sale. Supply coverage and request progress are personal. This is not multiplayer support.

This tests an interruption and its consequences, without an animated inspection, fight simulation, route closure, alternative supplier, recurring event schedule or saving. The stationary guard scenario remains available alongside it. Actual captures: [closed](images/supply01-closed.png), [reopened](images/supply01-reopened.png). [Gameplay](gameplay.md#request-feedback-during-supply-interruption---implemented-provisional) owns the player guidance; [Progress](progress.md) records verification and review status.

## Open decisions

- How often major fights should happen and how dangerous they should be to uninvolved players.
- Whether the player can join, lead, or create a gang, and at what stage.
- The neighboring-cell killing is now selected as the prototype loop's endpoint. On September 25 the user selected real combat for it (not a staged interaction), with the inmate fighting back; how guards/witnesses respond remains open. See [encounter scope](gameplay.md#neighboring-cell-inmate-encounter---agreed-purpose-and-real-combat-details-open).
- Whether initial relationships are fixed or randomized between playthroughs.
- How much influence the player has over reconciliation and guard assignments.
