# Prison game - design notes

Last updated: September 25, 2026.

## Agreed direction

A first-person game where the player walks around and builds a life inside a prison. Schedule I is a reference for the feel of walking around, interacting with people, growing an operation, and the simple cartoon appearance.

- Stylized, slightly grimy visuals, dark humor, and eccentric characters.
- Lighting and interiors usually feel "eerily cozy/comfy" while retaining the prison setting; open common areas connect to more enclosed corridors. Detailed visual decisions live in [Art and tone](art-and-tone.md).
- An approachable experience with clear, simple management of potentially complex systems. The game handles tracking and bookkeeping so the player can concentrate on current actions and decisions.
- Daily routines affect access to people and places. Assigned duties should create contacts, tools and exploration opportunities; free looking, nearby conversation and observation remain possible while work progresses. Laundry is the selected first test; detailed rules live in [Gameplay](gameplay.md#duty-direction---user-priority-proposed-mechanics-september-25).
- Trading, favors, prison jobs, relationships, and cell improvements create progression.
- Gangs and changing rivalries affect prison life. Unpredictable gang fights can substantially change the playing dynamic.
- Guard assignments should vary while retaining some consistency. The exact rules are open.
- Life inside the prison must be compelling; escape can become a later major goal.

The working player role is an inmate. Detailed systems below are proposals unless explicitly marked as agreed. Liking a broad idea does not settle its exact rules or numbers.

The latest clarification explicitly allows deadlines, guard suspicion, supply disruptions, and substantial systemic complexity. When something applies to the player, the game must make its cause, current state, timing, and consequences easy to understand and track. Earlier assistant proposals that removed ordinary deadlines or made all pressure optional were an overcorrection, not agreed requirements. Exact difficulty and pacing remain open. Detailed feedback rules live in [Gameplay](gameplay.md).

## Agreed development approach

The user accepted the development sequence: establish the basics, build a rough playable room, develop gameplay and feedback together, explore a small art sample alongside that work, polish one integrated section, and then expand.

The current project uses Unity 6.3 LTS and URP. Agreed on September 23: release 1 is single-player, with co-op planned for release 2. Develop the solo systems with later co-op in mind; this reduces avoidable rework but does not remove later networking work. Reasons, boundaries, and remaining choices are recorded in [Development plan](development-plan.md).

Start each work session at [Progress and next tasks](progress.md). It owns current task status; the [progress log](progress-log.md) keeps each task's verification details and history. The [development plan](development-plan.md#execution-checklist) owns the long-term milestones and completion criteria.

The agreed tracking workflow makes the primary development agent responsible for keeping progress current. Temporary reviewers or specialists can be used for bounded work when needed; a continuously running tracking agent is not required. Operational rules are in `AGENTS.md`.

## Documents

| Document | Owns these details |
| --- | --- |
| [Gameplay](gameplay.md) | Daily loop, economy, relationships, jobs, cell, and escape |
| [Prison world](prison-world.md) | Layout, movement, restricted areas, and expansion |
| [Factions and events](factions-and-events.md) | Gangs, rivalries, fights, guards, and consequences |
| [Art and tone](art-and-tone.md) | Visual style, character presentation, and humor |
| [Asset register](asset-register.md) | Sources, licenses and edits for imported production candidates |
| [Art reference board](art-reference-board.md) | Labelled reference pictures and proposed visual choices for review |
| [Development plan](development-plan.md) | Next design step, a proposed short session, first playable version, and unresolved decisions |
| [Progress and next tasks](progress.md) | Current state, task status, items awaiting review, blockers, and session handoff |
| [Progress log](progress-log.md) | Dated history: each task's verification details, evidence links and handoff notes |
| [Research](research/reports/) | Sourced research reports used as references for decisions; findings are not decisions |
| [Original ideas](history/original-ideas.md) | History. The user's original ideas in their own wording; some are still open |
| [First conversation](history/first-conversation.md) | History. Raw early chat log; its suggestions are not decisions |

## How to maintain these notes

- Keep one document per major area. Add sections before adding more files.
- Mark ideas as agreed, proposed, or open. Update their status when a decision is made.
- Put each detailed rule in its owning document and link to it elsewhere.
- Give proposed mechanics a player action, a consequence, and a reason to include them.
- Record important decisions briefly with their reason. Split a document when a topic becomes substantial enough to need its own specification.
- Keep engine and code details separate from player-facing design once implementation begins.

These notes describe the current direction and candidates for development. They are not a commitment to implement every feature.
