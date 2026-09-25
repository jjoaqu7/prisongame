# Development plan

## Status

The user accepted the broad development sequence on September 23, 2026. The current project uses Unity 6.3 LTS (6000.3.24f1) and URP. Release 1 is single-player; co-op is planned for release 2. Player count, hosting, schedule, and detailed prototype content remain open. Release 1's broad scope is agreed; see [Release 1 scope](#release-1-scope---agreed-september-25). Documentation does not mean a feature is implemented. Current task status and verification evidence live in [Progress](progress.md).

The agreed experience allows complex systems, deadlines, suspicion, and disruptions. The game handles bookkeeping and clearly communicates what applies to the player, so the player can focus on actions and decisions. Evaluate the clarity of those systems in the prototype, alongside whether the core activity feels enjoyable.

## Release 1 scope - agreed September 25

The user answered the release 1 scope questions on September 25. Release 1 (single-player, Windows PC) includes:

- **Escape.** Escaping is part of release 1. Milestone 7's order still applies: escape paths are developed after prison life is compelling, and are required before release 1 ships.
- **Gangs/factions** with conflicts.
- **Several prison wings.**
- **Real combat.** Fist fighting in which opponents fight back, plus improvised weapons picked up nearby. The first case is the [neighboring-cell encounter](gameplay.md#neighboring-cell-inmate-encounter---agreed-purpose-and-real-combat-details-open).
- **Distribution on Steam.**

**Open:** how many wings, gangs and escape routes; combat depth (controls, weapons, lethal and non-lethal outcomes, guard response); Steam features such as achievements, cloud saves and Steam Deck support; price, store-page timing and release date. These quantities determine most of the remaining size. [First playable version](#first-playable-version), [faction behavior](#then-expand-disruption-into-faction-behavior) and [Expand after the loop works](#expand-after-the-loop-works) remain the source material. Deriving a release 1 milestone list from them is the next planning task (REL1-01 in [Progress](progress.md)).

## Execution checklist

This is the milestone roadmap. The rough room, movement, and basic interactions are implemented and user-approved for this prototype. The lists below describe required work; [Progress](progress.md) is authoritative for individual task status. Windows PC is the current build target; the first release is agreed single-player, with co-op planned for the second release.

First target: walk from a cell into a shared area, interact with an inmate, complete a small earning activity, improve the cell, and understand any active deadline or guard suspicion without the developer explaining it.

### 1. Establish the development environment

- Verify the installed Unity Editor and choose a supported version compatible with the required packages.
- Create a Universal 3D / URP project at `PrisonGame/` under the workspace, keeping these design documents outside the Unity asset folders.
- Establish version control for the game after checking the existing repository boundaries. Exclude generated Unity caches and build output.
- Configure and verify the chosen Editor automation connection with a scene inspection and small reversible edit. If setup needs troubleshooting, basic Unity work can continue manually.
  - Follow [CLI setup below](#unity-cli-setup---check-the-existing-installation-first); do not reinstall an existing CLI just because the terminal cannot find `unity`.
- Save a test scene, enter Play mode, and produce a small Windows test build.

Done when the project opens reliably, the test build launches, and there are no blocking console errors.

### 2. Make the first room playable

- Sketch a cell, corridor, and small common area, then construct them with simple shapes.
- Add first-person looking and movement, collision, one opening door, one item to pick up and put down, and one placeholder inmate.
- Add a consistent interaction prompt and basic mouse sensitivity settings.
- Have the user play the scene and adjust room scale, walking speed, camera feel, and interaction distance.

Done when walking and interacting feel comfortable and it is clear which objects can be used. Start the small art study in milestone 5 once this scene establishes useful dimensions.

### 3. Keep later co-op in mind as solo systems grow

- Release direction is agreed: single-player first release; co-op second release.
- Separate world rules from local controls and presentation. Specify the acting player for interactions, and distinguish shared world state from personal inventory, objectives, and settings as those systems are designed.
- Centralize checks and changes for important actions such as taking an item or completing a sale so a later host can validate them. Introduce stable identities and versioned data when saving is implemented; avoid using camera references or scene names as persistent ownership.
- Plan a bounded two-player feasibility test before the larger inventory/economy/save design becomes expensive to change. Its timing is proposed and it is not part of the current Windows build task. Shipping co-op remains release 2.

The release decision is recorded. Apply these implementation boundaries as each system is added and revisit them at milestone reviews. No networking package or shared-world test has been implemented. Separating code does not establish multiplayer readiness.

### 4. Build one earning loop with its feedback

- First prototype activity selected September 24: assemble and sell snack packs. The supply/assembly/sale/restock/shelf loop is implemented and technically checked, awaiting user playtesting. Money is the announced prototype assumption following "proceed" with the recommended option; the final exchange system and prices remain open. See [Gameplay](gameplay.md#snack-pack-earning-test---implemented-awaiting-review).
- Implement obtaining supplies, carrying out the activity, making one sale, and buying one visible cell improvement.
- Add inventory, money or barter, and objective progress only as needed for that loop, with immediate visible updates.
- Introduce one timed request with a due time, remaining time, progress, and stated consequence.
  - Earn02_Requests retains the untimed comparison. Earn03_Deadline implements the selected clock, one-hour deadline, warnings, pause behavior and $2 late deliveries. Technical checks pass; user review of pacing and clarity remains. See [Gameplay](gameplay.md#timed-request-and-prison-clock---agreed-prototype-rules-awaiting-play-review).
- Add one guard whose suspicion has a visible source, cause, changing state, and resolution.
  - Guard01_Suspicion implements one stationary guard watching a marked staff-only corner, with sight checks, warnings, an order to leave and cooldown feedback. Technical checks pass; the user still needs to review clarity and pacing. Further enforcement remains open.
- Trigger one supply interruption deliberately; update the affected order and explain the cause automatically.
  - Supply01_Inspection implements a temporary stock inspection after the first sale, with reopening time, usable-stock guidance and affected-request feedback. Closure/reopening, late-delivery overlap and guard/earning regressions pass. Trigger and duration are provisional; combined user playtesting remains.
- Play each condition separately, then together, to check whether the player can choose what to do next without consulting several screens.

Done when the loop can be repeated and the player understands what applies, why, what can wait, and what needs action. Revise confusing feedback or uninteresting actions before adding more content.

### 5. Develop a small art sample alongside the prototype

Current status: M5's integrated sample is accepted at 0.52 m/s; the player walks at 3.02 m/s. The combined Windows prototype, including saving/loading, has passed technical checks and the user reported "it all works" in the combined playtest. A first licensed cozy sound pass is now implemented and technically verified; listening review and mix tuning are next (**Medium**), alongside performance follow-up and an uncoached new-player test. Visible profiling identified presentation waiting as the dominant recurring stall; a configurable frame limit now mitigates it. Longer-session and other-PC verification remain. [Progress](progress.md) owns task status. Final art approval remains separate; the temporary guard is not a newly approved art direction.

- Collect a small set of visual references and choose provisional proportions, colors, and material treatment.
- Create one inmate design and a few reusable cell pieces, such as a wall, door, bunk, table, and shelf.
- Import them into the playable scene and test lighting, scale, animation needs, and visibility at normal gameplay distance.
- Try the interface style alongside the 3D art; keep conditions and usable objects easy to recognize.

Done when one character and one cell corner establish a coherent, repeatable style. This milestone can progress alongside milestones 3 and 4; the full prison and cast come later.

### 6. Finish a small integrated playable sample


Current status: combined gameplay, save/load and the Windows executable are implemented and functionally accepted by the user. Basic sound is now implemented: footsteps, door, handling, assembly/sale and shelf cues plus quiet room ambience. Its cozy/umami direction is user-requested; the specific sounds and mix await listening review. Visible performance has been measured and a frame-limit mitigation implemented; an isolated pause and longer-session performance remain follow-up checks. Further presentation polish and an uncoached new-player test remain. No new mechanics are selected by this proposal.

- Combine the tested interactions, interface, representative art, basic sound, and essential animation.
- Add save/load for the sample's possessions, progress, cell upgrade, and active conditions.
- Make an executable build and check performance on the intended test PC.
- Have a new player try it without coaching; observe confusion, repetition, and whether they want to continue.

Done when a short session is enjoyable and understandable, and restarting preserves the intended progress. Use what fails here to choose the next fixes.

### 7. Expand from the tested foundations

- Add the fuller daily routine, first prison job, cafeteria, and yard.
- Add more inmates and goods, then two gangs and one meaningful conflict with tracked consequences.
- Introduce persistent rivalries and random event timing after the individual events work predictably in tests.
- Expand playable wings and longer-term progression; develop escape paths after prison life is compelling.

At each milestone, the user directs the desired feel and art and plays the result. The assistant helps with setup, implementation, scene assembly where tools permit, debugging, and verification. Update these notes after meaningful decisions; use playtesting to determine which ideas deserve more development. Calendar estimates should follow the first working prototype, once the asset and iteration workload is better understood.

## Development order - agreed workflow, proposed details

A common approach is to plan a focused experience, build a rough functional prototype, test it, then improve and expand it. Unity's [prototyping curriculum](https://learn.unity.com/pathway/creative-core/unit/prototyping?version=2021.3) follows this progression. The user accepted this sequence for the project. Specific mechanics and quantities in the table remain proposals; development is iterative and steps can overlap.

| Stage | Concrete work | What it establishes |
| --- | --- | --- |
| 1. Technical and design brief | Choose initial platform, single-player or co-op intent, engine, rendering pipeline, one activity, and feedback requirements | Enough direction to make the first scene without committing to every future system |
| 2. Project setup | Create the Unity project, establish source control, connect editor tools, and confirm that a test scene runs | A working development environment |
| 3. Rough playable scene | Build a cell and corridor from simple shapes; add first-person movement, a door, item pickup, and one NPC interaction | Room scale, movement, and interaction feel |
| 4. Gameplay and feedback together | Add the small earning loop, a tracked order, guard suspicion, and clear status indicators; introduce one controlled supply interruption | Whether the player understands and manages the mechanics while playing |
| 5. Small art study alongside stages 3-4 | Try one character, a cell corner, materials, lighting, and interface styling in the engine | A practical visual direction that works at gameplay distance |
| 6. Integrated polished sample | Combine the tested gameplay with representative art, animation, sound, interface, and saving in a small section | A vertical slice: a small playable sample of the intended overall quality |
| 7. Broader production | Build more blocks, characters, goods, jobs, and faction systems using what worked | More content with less guesswork about the foundations |

Write scripts in small batches attached to working interactions. Judge them in the running scene. The initial art study can start early, while detailed production of many characters and rooms waits for scale and interactions to stabilize.

## Tool recommendation - not yet selected

- Engine and rendering: a supported Unity 6 Editor with the Universal 3D / URP setup. URP is Unity's broadly scalable rendering pipeline; it is a reasonable starting point for this stylized first-person project. Select the exact Editor and package versions together. [Unity URP documentation](https://docs.unity.com/en-us/engine/6000.5/manual/render-pipelines/universal-render-pipeline), [Unity release support](https://unity.com/releases/unity-6/support).
- Models: Blender is a candidate for reusable room pieces, props, and characters. Import small tests into Unity early so dimensions, materials, and animation can be checked in play.
- Editor automation: assess Unity's current official CLI/Pipeline integration, which supports direct Editor commands and an MCP mode. The CLI replaces the older MCP server inside Unity's AI Assistant package. Unity documents the CLI as experimental, so verify the chosen version with a small scene inspection and edit before relying on it. [Current integration guidance](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli), [CLI status](https://docs.unity.com/en-us/unity-cli).
- The Unity Pipeline automation package and the Universal Render Pipeline renderer serve different purposes: editor control and game graphics, respectively.

Direct CLI commands can use the explicit PrisonGame project path. `unity status` returned no instances despite successful scene queries, so an empty status listing alone does not prove the connection is unavailable. MCP was also successfully verified with a scene read in this conversation. See [Progress](progress.md) for verification evidence and remaining setup tasks.

Schedule I remains a visual reference. A February 27, 2026 [Unity rendering discussion post](https://discussions.unity.com/t/render-pipelines-strategy-for-2026/1710004/221) lists it among games using Built-in. This is not a direct verification of the current build by its developer; its exact current setup remains unconfirmed here. The recommendation above is based on this project's needs; comparable stylization requires deliberate models, materials, lighting, and animation.

## Unity CLI setup - check the existing installation first

Verified on September 23, 2026: Unity CLI is already present on this PC. The executable bundled with Hub at `C:\Program Files\Unity Hub\resources\cli\unity.exe` successfully reports `1.0.0-beta.8`. A second executable exists at `C:\Users\jjoaq\AppData\Local\Unity\bin\unity.exe`.

The earlier instruction to run `winget install Unity.CLI` unconditionally was unnecessary. Skip that installation step on this PC. A terminal failing to recognize `unity` can mean its folder is missing from that terminal's PATH (the list of folders searched for commands); it does not prove the CLI is absent.

1. Verify the existing CLI in PowerShell using its full path:

   ```powershell
   & 'C:\Program Files\Unity Hub\resources\cli\unity.exe' --version
   ```

2. Use that full executable path for subsequent CLI commands if plain `unity` is not recognized. In an MCP configuration, use the full executable path as `command` as well. Restarting VS Code after an installation can refresh its inherited PATH; reinstalling is not the first troubleshooting step.
3. Open the actual `PrisonGame` project in the Unity Editor before installing or checking the project's Pipeline integration. A Hub project listing alone does not verify that project creation finished. Confirm that the project folder contains `Assets`, `Packages`, and `ProjectSettings`.
4. Verify the Unity Pipeline package with a read-only scene command, then configure/test MCP if needed. The direct CLI connection is now verified; use the explicit project path:

   ```powershell
   & 'C:\Users\jjoaq\AppData\Local\Unity\bin\unity.exe' command get_scene_hierarchy --project-path 'C:\Users\jjoaq\vscode-python\game\PrisonGame'
   ```

   Direct CLI commands already allow Editor automation. MCP is a separate integration step, not a prerequisite for using that working connection.

For a different PC, check for an existing CLI first. Use `winget install Unity.CLI` only if it is missing and a standalone installation is needed. Unity documents that current Hub versions automatically install the CLI. [Unity CLI documentation](https://docs.unity.com/en-us/unity-cli).

## Why recommend Unity and URP?

Unity is a reasonable candidate for this 3D project and the intended editor automation workflow. It is not established as the universally simplest engine for every solo developer. Prior experience, desired assets, tools, and game requirements affect that choice.

A rendering pipeline controls how the engine draws models, materials, lights, shadows, and effects. Unity can use different pipelines; these are separate from the MCP connection used to operate the editor.

The current recommendation is URP because the project needs readable stylized 3D, ordinary lighting and shadows, and room to tune performance across PCs. This is a project assessment, not a measured performance comparison or a claim that URP reproduces Schedule I automatically.

Unity's [2026 rendering strategy](https://unity.com/topics/render-pipelines-strategy-for-2026) prioritizes URP, keeps HDRP in maintenance, and begins deprecating Built-in in Unity 6.5 while continuing support for existing projects. It explicitly discourages Built-in for new games. That makes URP a sensible starting point for a new project without an existing collection of incompatible assets.

HDRP remains capable, but we have not identified a visual requirement that justifies selecting it for this game. An older renderer can still support a successful released game; that alone does not make it the best starting choice today. Switching pipelines later can require material and shader changes, so test our first character and cell art in the selected pipeline before producing many assets.

## Single-player first release, co-op second release - agreed

On September 23, the user clarified that co-op belongs in the second game release, not the first, and requested development choices that reduce later rework. This is a planned second release of this project; whether it is an update or a separately distributed release is not specified. Networking implementation is deferred. Player count, hosting, joining/leaving behavior, shared progression, and old-save compatibility remain open.

Co-op is substantially more implementation and testing work than the same world in single-player. No reliable percentage or time multiplier can be given before the features and connection model are defined. Unity provides networking and session tools, but gameplay still needs explicit synchronization and rules about which machine decides shared results. [Unity casual co-op quickstart](https://docs.unity.com/en-us/multiplayer/quickstarts/casual-co-op-quickstart).

For this prison game, additional design and implementation would include:

- Resolving two players trying to take the same item, spend shared money, or use the same workbench.
- Keeping guard positions, gang incidents, doors, stock, and the prison clock consistent for everyone.
- Deciding which reputation, suspicion, objectives, and possessions are personal or shared.
- Defining pause, sleep, time skipping, imprisonment, and escape when players want different things.
- Deciding who owns the world save and what joining, leaving, reconnecting, and host departure do to progress.
- Testing multiple clients, delayed messages, dropped connections, and adverse network conditions. Unity explicitly recommends testing under simulated latency and packet loss during development. [Unity multiplayer testing guidance](https://docs-multiplayer.unity3d.com/netcode/2.3.2/tutorials/testing/testing_with_artificial_conditions/).

Implementation recommendation: continue the solo prototype, separating player requests, world-state changes, and local presentation as systems grow. Important interactions should identify the acting player; world rules should not read the local keyboard or depend on the main camera. Document which state is shared or personal. Unity's [ownership and authority guidance](https://docs-multiplayer.unity3d.com/netcode/2.3.2/basics/ownership/) explains why networked state later needs explicit ownership and permission to change it; the specific architecture above is our project recommendation, not a selected networking library.

These boundaries reduce avoidable coupling but cannot guarantee a cheap co-op conversion. Synchronization, simultaneous item use, latency, disconnects, UI/session flow, and multiplayer testing remain additional work. The current movement and pickup scripts are solo prototypes, not a verified networking foundation. Proposed later risk check: use an isolated two-player test before inventory, economy, and saving become large. Do not delay the current standalone build for this test or build a generic networking framework in advance.

A possible limited co-op scope is a private game hosted by one player, beginning with two players and a save kept by the host. If the host leaves, the session ends. This avoids needing seamless transfer of the host role initially, but still requires connection handling, state synchronization, persistence rules, and multiplayer testing. Player count and hosting choices are proposals only.

The release-order decision is settled. The next networking decisions are the second release's player count, hosting model, and shared/personal progression rules; those can be addressed separately from this Windows prototype build.

## Next step - design and test one short session

The broad concept is sufficient to begin focused experience design. Draft a session that might take roughly 10-15 minutes at an ordinary pace; this is a planning estimate. Define its actions, rewards, small layout, and the feedback for any timed obligations or threats. Then build it using simple placeholder geometry and characters.

The following is one provisional session to evaluate. Snack packs are selected for the first prototype, and its basic earning loop now uses temporary cash prices. The final money/barter choice, shelf and exact mechanics remain provisional, not settled final-game rules.

| Moment | Player action | Feedback or purpose |
| --- | --- | --- |
| Arrive in the cell | Look around and notice a possible storage upgrade | Establish a small visible goal without a timer |
| Meet a nearby inmate | Learn that they buy simple snack packs | Introduce a repeat customer; make the request's requirements, reward, and any due time clear |
| Visit the shared table | Combine two supplied packaged goods into a bundle | Test a short hands-on routine with no speed requirement or failure timer |
| Make a sale | Carry the pack to the customer and exchange it | Give clear earnings and a friendly reaction |
| Repeat or explore | Restock through a nearby, dependable source when desired | Test whether repeating the activity is pleasant; set provisional prices so restocking remains affordable |
| Improve the cell | Buy and place a small shelf after a few sales | Show progress physically and make storage easier |

After the basic interaction works, use controlled test scenarios to apply a deadline, raise one guard's suspicion, and interrupt a supply source. Add their indicators with the mechanics. Test each scenario on its own, then combine them to find out whether priority and dependencies remain understandable. These scenarios precede a full gang or economy simulation.

Provide enough starting supplies to complete the first sale and an accessible way to earn replacement stock if the player uses their supplies elsewhere. Exact quantities and prices should be adjusted during playtesting.

For this earliest test, connect a cell, shared landing, and small common room with the table and supplier. A few placeholder characters suffice. This precedes the larger first playable prison day below. It does not require a functioning whole prison, combat, a gang simulation, or finished artwork.

Unity and the release order are selected. Use the implementation boundaries above as the earning loop develops. The user deferred earning-loop work on September 23 and resumed it on September 24 by selecting snack packs. Earn01_SnackPacks contains the earning/shelf loop; Earn02_Requests adds an untimed request; Earn03_Deadline adds the selected clock, deadline and late-payment consequence; Guard01_Suspicion adds guard observation and escalation/resolution feedback; Supply01_Inspection adds the controlled disruption and affected-request feedback. Milestone 4's functional scenarios are implemented and technically checked together, but its clarity/enjoyment criteria still require user playtesting. Save01_Progress now adds manual save/load following the user's authorization to proceed; see [saving behavior](gameplay.md#saving-the-sample---implemented-provisional-controls) and [verification](progress-log.md#save-01-manual-sample-persistence---september-24). The user subsequently confirmed that the combined sample works; its functional playtest is accepted. The combined Windows sample is now built and verified under [CHECK-02](progress-log.md#check-02-combined-windows-sample---september-24), including save/load across separate executable launches. The first sound pass is now implemented and verified; next is listening review/mix tuning (**Medium**), alongside an uncoached new-player test and longer-session performance observation. PERF-02 records the measured frame-limit mitigation and remaining isolated pause. Final polish remains.

After playing the test, ask:

- Is walking, handling objects, and completing the sale satisfying?
- Is the next action understandable without many prompts or status displays?
- Can the player explain which conditions currently apply, why they began, what they affect, and what happens next?
- Does the game automatically update inventory progress, deadlines, and affected orders when something changes?
- Can the player distinguish something that needs attention now from something that can wait?
- Does the shelf feel worth earning, and does the player want to repeat the loop?

If the activity feels dull, revise it before expanding the system list. Continue fleshing out details that affect this session; defer distant systems until the basic experience works.

## First playable version

Build one connected prison day with:

- First-person movement and basic object and conversation interactions.
- A home block, player cell, cafeteria, yard, and one job area.
- A small cast of recognizable inmates and guards; roughly a dozen inmates is a starting estimate.
- A readable routine with work, social time, and return to the block, with relevant times automatically tracked.
- A few tradable goods and one favor with clearly stated requirements, timing, benefit, and consequences.
- The tested interface for objectives, deadlines, suspicion, and supply changes.
- One short work activity and one useful cell improvement.

The first question is whether walking around, learning the routine, meeting people, and completing a small personal objective is enjoyable.

## Then expand disruption into faction behavior

Introduce two groups and one confrontation that changes a trading relationship and temporarily affects access. Initially trigger the event deliberately during development so its consequences can be checked. Add random timing and persistent tension after that behavior works.

Give guards recurring assignments and one possible substitution. Test whether the player can recognize the change and adapt.

Also test delaying a response: the player should understand whether this is safe, costly, or dangerous under the chosen rules. The interface must identify which objectives are affected and update them automatically. Balance the frequency and overlap of problems through playtesting.

Real player combat is agreed for release 1 ([Release 1 scope](#release-1-scope---agreed-september-25)). Whether this first gang confrontation starts as a short staged event or as simulated NPC fighting remains open.

## Expand after the loop works

Candidates include more jobs, deeper favors, cellmates, recruited traders, gang progression, additional playable wings, and multiple escape plans. Prioritize whichever adds useful choices to the proven loop.

The larger prison can be sketched now; only the spaces needed for the current playable version need full interiors and functioning systems.

## Local display settings - implemented provisional default

The Windows sample now offers 60 FPS, 120 FPS and Unlimited under Escape. It starts at 60 FPS when no valid saved choice exists, based on the measured reduction in presentation stalls on the test laptop. The choice is local and persistent; it does not change prison-clock or simulation rules. This default is a provisional technical implementation, not a user-approved final performance target or minimum specification. [Progress](progress-log.md#perf-02-visible-profiling-and-frame-limit---september-25) owns evidence and outstanding checks.

## Decisions still needed

1. Laundry is selected for the first supervised-duty prototype. The two-minute shift/one-minute work requirement, first warning and tool-for-introduction favor are selected for testing. Free looking, nearby conversation and observation during work are agreed; ignoring the warning or being caught away again fails only this shift, preserving unrelated progress. Remaining details stay provisional in [Gameplay](gameplay.md#duty-direction---user-priority-proposed-mechanics-september-25).
2. Does everyday trade primarily use money, goods, or both?
3. Final day pacing and later waiting/sleep/time skipping remain open. The current prototype's clock rate, deadline and pause rules were selected September 24; see [Gameplay](gameplay.md#timed-request-and-prison-clock---agreed-prototype-rules-awaiting-play-review).
4. Which conditions impose deadlines, suspicion, or penalties, and how does the interface communicate their onset, cause, progress, and resolution?
5. How do the earlier labor quotas, multipliers, and global upgrades fit the sandbox?
6. What does gang progression allow, and can it support a long-term goal beyond escape?
7. Which asset tools should be selected? The current project uses Unity 6.3 LTS and URP; the asset creation workflow remains open.
8. For release 2 co-op, what player count, hosting model, shared/personal progression, and save compatibility should be supported? Release 1 single-player / release 2 co-op is already agreed.
9. Release 1 quantities and depth: how many wings, gangs and escape routes, and how deep combat goes. The broad scope is agreed; see [Release 1 scope](#release-1-scope---agreed-september-25).

The immediate development milestone is a rough Unity room with movement, interaction, a small earning loop, and understandable feedback for a deadline and rising guard suspicion. A small visual study can proceed alongside that work. A full prison day follows once those interactions are enjoyable and clear.


## Near-term sequence after the laundry prototype - proposed

1. Implement the selected Minimal text HUD and consistent contextual notifications; test warning priority and readability in play (**Medium**).
2. Add a small inventory using existing objects, with pocket/held distinctions, storage and save compatibility. Decide capacity and stacking before changing carrying rules (**High**).
3. Provide a compact on-demand view of tasks and known contacts, building on existing favor/request state (**Medium**).
4. Connect one short repeatable prison-day sequence: assigned duty, free time and an existing contact's useful service. Scheduling, service/reward and any additional room are proposals requiring selection (**High**).

Judge this small sequence in play before adding many rooms, characters or unrelated systems. Existing broader ideas remain recorded, not discarded.


### Next step after the Harris/cell chain - proposed

HUD, notifications, a six-slot inventory, Dex's secret, Harris's key and a neighboring cell are now implemented in the prototype (technical results in Progress; user review remains). The user has selected the cell's purpose: another inmate is there, and the player kills him to close this progression loop. On September 25 the user selected real combat for it: the inmate fights back and nearby objects can be picked up as weapons. Combat details, motivation and aftermath remain open; [Gameplay](gameplay.md#neighboring-cell-inmate-encounter---agreed-purpose-and-real-combat-details-open) owns those decisions, and the [combat research](research/reports/Unity%20first%20person%20melee%20combat.md) lists the open combat decisions. Recommended reasoning: **High**, because this introduces combat, death, persistent character state and encounter completion.

**Proposed milestone order:** finish that bounded encounter and play the whole chain from its objective through its outcome; then put it inside a short duty/free-time/return-to-cell day sequence. Add a compact on-demand task/contact journal when tracking the chain needs it (**Medium**), as supporting interface work rather than the main progression milestone. A full day/night simulation is not required to test this first complete chain; the encounter does require a first real combat implementation. Broader rooms, cast, factions and release 2 co-op remain later milestones.
