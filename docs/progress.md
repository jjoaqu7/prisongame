# Progress and next tasks

Last updated: September 26, 2026.

## How to use this tracker - agreed

The primary development agent maintains this file as part of each work session. There is no continuously running tracking agent. Read this file and the relevant design docs before work; update it after meaningful results and leave the next action before ending a session. Specialist/reviewer agents are optional, bounded assignments when authorized, not permanent background workers.

Task states: **To do**, **In progress**, **Blocked**, **Needs your review**, **Done**. Done means the stated checks passed; user approval of feel or appearance must come from the user. Record what was checked and distinguish untested implementation from verified behavior.

The [development plan](development-plan.md) owns milestone scope and sequencing. Design docs retain agreed/proposed/open decisions. This file owns task status; add detail to the next milestone and keep later work broad.

**Two files - agreed September 25.** This file holds current state only: current state, items awaiting review, one short row per task, and the handoff. Keep it under about 150 lines, and replace outdated status instead of adding new "latest" paragraphs. Write each task's verification details, evidence links and handoff notes as a dated entry at the end of the [progress log](progress-log.md), and link the task row to that entry. Read the log only when a task's history is needed; search it by task ID.

## Current state

- **Milestone:** closing the prototype chain with ENCOUNTER-01, the neighboring-cell fight. Its [fight rules](gameplay.md#neighboring-cell-fight-rules---agreed-september-25) and [build plan](development-plan.md#encounter-01-build-plan---september-25) were agreed September 25. Release 1's broad scope is agreed: [Release 1 scope](development-plan.md#release-1-scope---agreed-september-25).
- **Next action:** the user plays `CombatAttacksSandbox` and judges whether hits read clearly (steps 2 and 3 together; see Awaiting your review). Then ENCOUNTER-01 step 4: the inmate fights back (**High**). After ENCOUNTER-01, REL1-01 (**High**).
- **Blockers:** none. Working locally on `encounter-01`; STD-02 is checkpointed as `3322500`. The last publication was September 25; these changes have not been pushed.
- **Latest builds** (ignored by Git): `PrisonGame/Builds/WindowsLaundry/PrisonGame.exe` has laundry duty, HUD, inventory and the neighboring cell (scene `Duty01_Laundry`). `PrisonGame/Builds/WindowsSample/PrisonGame.exe` has the combined earning sample with sound (scene `Save01_Progress`).

## Awaiting your review

Implemented and technically checked; only the user can judge feel and appearance. List each item here with what to try and what to judge, and move it to Done when the user approves it.

ENCOUNTER-01 steps 2-3, controls and hit feel. Open `Assets/PrisonGame/Scenes/Sandbox/CombatAttacksSandbox.unity`, press Play, click **Resume walking** and walk up to the orange dummy:
- **Try:** click LMB to punch, press V to kick (slower, reaches further, pushes harder), hold RMB to block, and press F to shove.
- **Judge:** whether a hit reads clearly. On each hit both fighters freeze for a split second, the dummy leans away and is pushed back (much further for a kick), your view kicks slightly, and a punch sound plays (heavier, with a thud, for a kick). Also judge whether punching feels responsive and whether the kick's slower wind-up feels right.
- **Adjust:** camera shake can be turned off in the Escape menu. Exit and re-enter Play to reset the dummy. He does not fight back yet (step 4).
- [Step-3 log](progress-log.md#encounter-01-step-3-hit-feedback---september-27).

## Tasks

Details for each row are in the [progress log](progress-log.md). Rows without their own log entry are detailed in the [pre-split task table](progress-log.md#task-table-before-the-split).

| ID | Task | Status | Evidence / notes |
| --- | --- | --- | --- |
| TRACK-01 | Establish progress tracking | Done | Linked from README; AGENTS.md sets update and handoff duties. |
| TRACK-02 | Split the tracker into current state and a log | Done | History moved unchanged to the log in date order; word count matched; links repointed. [Log](progress-log.md#track-02-tracker-split-and-release-1-scope---september-25) |
| SETUP-01 | Open Unity project | Done | Editor 6000.3.24f1 confirmed. |
| SETUP-02 | Verify CLI and MCP scene reads | Done | Both returned the scene hierarchy. |
| SETUP-03 | Recoverable version-control checkpoint | Done | Separate game repository; baseline `bae004a`; recovery compared. [Log](progress-log.md#local-checkpoint-and-recovery) |
| SETUP-04 | Reversible MCP scene edit | Done | Create, read and delete verified. |
| SETUP-05 | Connect the private GitHub repository | Done | `origin` is `jjoaqu7/prisongame`; initial push verified. Later pushes: see Current state. |
| ROOM-01 | Rough cell, corridor and common area | Done | User accepted September 23. [Log](progress-log.md#room-01-verification) |
| ROOM-02 | First-person movement and looking | Done | 14 checks; accepted. [Log](progress-log.md#room-02-controls-and-verification) |
| ROOM-03 | Basic interactions | Done | 26 checks; accepted. [Log](progress-log.md#room-03-controls-and-verification) |
| CHECK-01 | First Windows build | Done | 18 executable checks. [Log](progress-log.md#check-01-windows-build) |
| ART-01 | Visual reference board | Done | Direction selected. [Board](art-reference-board.md) |
| ART-02 | Lighting and materials test | Done | User approved for the prototype September 25. Movement and interaction checks pass in all variants. [Log](progress-log.md#art-02-atmosphere-comparison--september-24) |
| ART-03 | Original inmate design (M5) | Done | Accepted prototype; 20 behavior checks at 0.52 m/s. [Log](progress-log.md#m5-review-faster-walking-and-earn-01-assembly--september-24) |
| EARN-01 | Snack packs and cell shelf | Done | 27 loop checks; combined prototype accepted ("it all works"). [Log](progress-log.md#earn-01-complete-earning-prototype--september-24) |
| EARN-02 | One tracked customer request | Done | 23 checks. [Log](progress-log.md#earn-02-tracked-customer-request--september-24) |
| EARN-03 | Prison clock and request deadline | Done | 33 checks. [Log](progress-log.md#earn-03-timed-request-and-player-speed--september-24) |
| GUARD-01 | One guard's suspicion and resolution | Done | 32 checks; further enforcement open. [Log](progress-log.md#guard-01-observed-trespass-and-suspicion--september-24) |
| SUPPLY-01 | Supply interruption and affected request | Done | 31 + 9 checks; trigger and duration provisional. [Log](progress-log.md#supply-01-controlled-stock-inspection--september-24) |
| SAVE-01 | Save and restore the combined sample | Done | 32 checks plus fresh-session reload. [Log](progress-log.md#save-01-manual-sample-persistence---september-24) |
| CHECK-02 | Combined Windows sample | Done | 35 executable checks; other-PC testing open. [Log](progress-log.md#check-02-combined-windows-sample---september-24) |
| AUDIO-01 | First cozy sound palette | Done | User approved for the prototype September 25. 37 audio, 32 save and 37 executable checks. [Log](progress-log.md#audio-01-cozy-sound-palette) |
| PERF-01 | Performance baseline | Done | [Log](progress-log.md#perf-01-windows-performance-baseline---september-25); [report](evidence/performance-summary.md) |
| PERF-02 | Frame hitches and frame limit | Done | Presentation waiting identified; 60 FPS default. [Log](progress-log.md#perf-02-visible-profiling-and-frame-limit---september-25) |
| PERF-03 | Longer performance session | To do | Watch the isolated 226 ms pause, paused-menu outliers and memory; other-PC testing open. **High** for profiling. |
| DUTY-01 | Supervised laundry duty | Done | User approved for the prototype September 25. 36 rule and 25 executable checks. [Log](progress-log.md#duty-01-verification-and-review-handoff---september-25) |
| HUD-01 | Minimal HUD, notifications, Tab inventory view | Done | User approved for the prototype September 25. 36 duty and 32 executable checks. [Log](progress-log.md#hud-01-and-first-inventory-view---implemented-awaiting-review) |
| INV-01 | Six-slot inventory and Dex-to-Harris key | Done | User approved for the prototype September 25. 41 rule/save, 79 Editor and 79 executable checks. [Log](progress-log.md#inv-01---functional-inventory-and-officer-secret) |
| CELL-01 | Immediate dialogue, Harris's secret, neighboring cell | Done | User approved for the prototype September 25. 22 new, 106 Editor and 106 executable checks. [Log](progress-log.md#cell-01---dialogue-correction-and-harriss-neighboring-cell-key) |
| PLAYTEST-01 | Uncoached new-player test | To do | Milestone 6 exit test: a new player tries the sample without coaching; note confusion, repetition and interest. **Medium**. |
| RESEARCH-01 | Combat research | Done | [Report](research/combat/report.md) with a proposed approach, build order and 13 open decisions; findings are not decisions. [Log](progress-log.md#research-01-combat-research---september-25) |
| RESEARCH-02 | Coding, scripting and testing standards | Done | [Report](research/engineering-standards/report.md). Decisions 1 to 13 agreed and applied in STD-01 and STD-02. [Log](progress-log.md#research-02-engineering-standards---september-25) |
| STD-01 | Adopt the engineering standards | Done | [Standard](engineering-standards.md) in place September 26, enforced by `.editorconfig` + `dotnet format`, `tools/check.ps1` / `tools/format.ps1`, a blocking pre-commit hook (proven to block), and Microsoft.Unity.Analyzers with per-assembly rulesets (proven to warn). Prototype reformatted whitespace-only (`a704ac6`, in `.git-blame-ignore-revs`); 28 tests, 106 laundry and 18 smoke checks pass in the Editor. Windows builds were not rebuilt. Later: agent post-edit hooks, coverage, CI. [Log](progress-log.md#std-01-engineering-standards-adopted---september-26) |
| STD-02 | Names, builds, versions and Git | Done | Company name `Niuq`, with saves and settings copied to the new location. 14 spaced materials renamed. `CombatSandbox` moved to `Assets/PrisonGame/Scenes/Sandbox/`; template scene deleted. Git LFS for new binaries, commit and branch rules, `PrisonGame.EditorTools` (`AssetNaming`, `PrototypeChecks`). Check passes; 106 laundry, 18 smoke and 18 sandbox checks pass. Build Profiles come at ENCOUNTER-01 step 7. [Log](progress-log.md#std-02-names-builds-versions-and-git---september-26) |
| ENCOUNTER-01 | Neighboring-cell fight | In progress | Rules and 8-step plan agreed September 25; September 27 the charged punch was replaced by a kick (V, 12 damage vs the punch's 10, slower, 1.4 m reach, 0.6 m push). **Steps 1-3 implemented and technically verified** in `CombatAttacksSandbox`: punches, kick, block input, shove, placeholder arms and leg, hitstop, knockback, flinch, camera kick with an off setting, CC0 placeholder sounds. 52 edit-mode and 23 play-mode tests pass. **Needs your review:** controls and hit feel. Next: step 4. [Log](progress-log.md#encounter-01-kick-replaces-the-charged-punch---september-27). Done when the fight, death, saved outcome and completion work through the full Rue/Dex/Harris chain. [Plan](development-plan.md#encounter-01-build-plan---september-25) |
| SHOVE-01 | Reactions to shoving by character type | To do | Agreed September 25: shove anyone at any time on F; reactions depend on who (a prisoner starts a fight). ENCOUNTER-01 step 2 builds the shove itself; this task adds reactions for guards, gangs and contacts when those are developed. [Gameplay](gameplay.md#shoving---agreed-direction-and-key-reactions-open) |
| REL1-01 | Release 1 milestone list | To do | Derive from the agreed [scope](development-plan.md#release-1-scope---agreed-september-25): detailed for the next milestones, broad later, with proposed quantities (wings, gangs, escape routes) for the user to decide. **High**. |

## Later tasks: evaluate additional agent roles

Agreed: retain these as future evaluation tasks. Adding or running the agents is deferred; the roles below remain candidates. Begin with gameplay testing and independent review when the first playable room warrants them. The primary agent owns the shared tracker and integrates findings; the user judges gameplay feel and visual direction.

| ID | Candidate role | Status | When to consider it | Evaluation outcome |
| --- | --- | --- | --- | --- |
| AGENT-01 | Gameplay tester | To do | ROOM-03 is playable | Define a repeatable test of doors, pickups, collisions, and restarting; assess whether a separate agent produces useful reproducible findings. |
| AGENT-02 | Independent code reviewer | To do | First substantial feature or milestone review | Trial a bounded review against requirements and record actionable issues, verification, and whether to reuse the role. |
| AGENT-03 | Art and asset specialist | To do | First representative assets are imported | Assess scale, materials, collisions, and consistency with the user-approved art direction. |
| AGENT-04 | Interface reviewer | To do | Orders, deadlines, or suspicion indicators exist | Check that cause, timing, progress, and consequences are understandable; retain user playtesting. |
| AGENT-05 | Performance specialist | To do | A representative prison scene can be measured | Define performance targets, collect measurements, and assess whether specialist investigation is useful. |
| AGENT-06 | Networking specialist | To do | Planning the release 2 co-op feasibility test | Review shared item ownership, simultaneous interactions, and state boundaries; evaluation deferred, no networking agent running. |
| AGENT-07 | Research specialist | Adopted September 25 | Research recurs (HUD, prison rules, combat; later escape, gang AI, Steam) | User chose a reusable research agent that works in any tool. One `game-research` skill in the open Agent Skills format (`.agents/skills/game-research/`), with thin pointers for Claude Code (`.claude/skills/`, `.claude/agents/`) and Codex (`.codex/agents/`). See the [research README](research/README.md). Not yet used for a new topic; Codex agent files are checked against the docs but not run. |

For each evaluation, record whether to adopt, defer, or decline the role and why. A decision to defer or decline completes the evaluation without implying an agent was installed or run. Additional roles can be added when a concrete need appears.

Patterns to consider borrowing: Unity's [inspect/change/run/verify workflow](https://unity.com/blog/meet-the-unity-cli), and the community [gamedev-ai-agents workflow](https://github.com/ilezhnin/gamedev-ai-agents) for playable milestones, placeholder assets, and test evidence. The community kit has not been tested here; its inclusion is a research reference, not a decision to install it.

## Session handoff

- **September 25:** user agreed release 1 scope, real combat, and the combat decisions for ENCOUNTER-01 (weapon adds damage, downed then separate kill, reload on loss), plus shoving as a general action; recorded in Gameplay, Factions/events and the development plan. User approved the six review items. Tracker split (TRACK-02). RESEARCH-01 finished; research moved to `docs/research/<topic>/`; a tool-independent research skill with Claude Code and Codex agents (AGENT-07). No game code, scenes or builds changed.
- **September 25, later:** planned ENCOUNTER-01; the user agreed the fight rules (changing four defaults: add a ragdoll, no critical hits, no breaking, LMB hit / RMB block) and code organization. Recorded in Gameplay, the development plan and Factions/events.
- **September 25, step 1:** built the greybox room and combat foundations (see the ENCOUNTER-01 row). Combat foundations are committed; the standards checkpoint is covered below.
- **September 25-26, standards:** researched (RESEARCH-02); the user accepted all proposals; STD-01 set them up and verified them. Every agent now follows [Engineering standards](engineering-standards.md); run `git config core.hooksPath .githooks` once in any new clone.
- **September 26, resumed:** STD-02 independently rechecked; 28 edit-mode, 106 laundry, 18 smoke and 18 combat checks pass. [Resume log](progress-log.md#std-02-resumed-verification-and-checkpoint---september-26).
- **September 26, step 2:** player attacks implemented and verified; `CombatAttacksSandbox` is ready for an early controls review. [Log](progress-log.md#encounter-01-step-2-player-attacks---september-26). Windows builds are unchanged.
- **September 27, step 3:** hit feedback added to `CombatAttacksSandbox`, and `tools/check.ps1 -Unity` now also runs play-mode tests. The sandbox is ready for your feel review. [Log](progress-log.md#encounter-01-step-3-hit-feedback---september-27).
- **September 27, kick:** the user replaced the charged punch with a kick on V; built, tested and recorded in Gameplay. Play-mode input tests were made independent of window focus and real mouse use. [Log](progress-log.md#encounter-01-kick-replaces-the-charged-punch---september-27).
- **Next:** the user's feel review of steps 2-3 in `CombatAttacksSandbox`; then step 4, the inmate fights back (**High**).
- **Open user decisions:** the shipped product name (working title `PrisonGame`; it sets the save folder, so settle it before the Steam release); release 1 quantities (wings, gangs, escape routes). The user will merge `encounter-01` into `main` after testing it. The encounter has no story in this prototype (agreed).
- **Publication:** pushed September 25; see Current state.
