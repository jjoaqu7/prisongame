# Progress and next tasks

Last updated: September 25, 2026.

## How to use this tracker - agreed

The primary development agent maintains this file as part of each work session. There is no continuously running tracking agent. Read this file and the relevant design docs before work; update it after meaningful results and leave the next action before ending a session. Specialist/reviewer agents are optional, bounded assignments when authorized, not permanent background workers.

Task states: **To do**, **In progress**, **Blocked**, **Needs your review**, **Done**. Done means the stated checks passed; user approval of feel or appearance must come from the user. Record what was checked and distinguish untested implementation from verified behavior.

The [development plan](development-plan.md) owns milestone scope and sequencing. Design docs retain agreed/proposed/open decisions. This file owns task status; add detail to the next milestone and keep later work broad.

**Two files - agreed September 25.** This file holds current state only: current state, items awaiting review, one short row per task, and the handoff. Keep it under about 150 lines, and replace outdated status instead of adding new "latest" paragraphs. Write each task's verification details, evidence links and handoff notes as a dated entry at the end of the [progress log](progress-log.md), and link the task row to that entry. Read the log only when a task's history is needed; search it by task ID.

## Current state

- **Milestone:** closing the prototype chain with ENCOUNTER-01, the neighboring-cell fight. Real combat is agreed. Combat research is done: [report](research/reports/Unity%20first%20person%20melee%20combat.md). Release 1's broad scope is agreed: [Release 1 scope](development-plan.md#release-1-scope---agreed-september-25).
- **Next action:** the user settles the report's open decisions, at least 1 (what a weapon changes), 4 (what zero health does to the inmate) and 5 (what happens when the player loses). Record them in Gameplay, then plan ENCOUNTER-01 (**High**). After that, REL1-01: the release 1 milestone list (**High**).
- **Blockers:** none for development. Publishing: local commits are not on GitHub yet; the last recorded push attempt failed on GitHub authentication.
- **Latest builds** (ignored by Git): `PrisonGame/Builds/WindowsLaundry/PrisonGame.exe` has laundry duty, HUD, inventory and the neighboring cell (scene `Duty01_Laundry`). `PrisonGame/Builds/WindowsSample/PrisonGame.exe` has the combined earning sample with sound (scene `Save01_Progress`).

## Awaiting your review

Implemented and technically checked; only the user can judge feel and appearance. Report problems, or accept an item as good enough for the prototype.

| ID | What it is | Where to try it | What to judge |
| --- | --- | --- | --- |
| ART-02 | Lighting and materials: amber cell, cream and blue-grey shared areas | Unity: `Assets/Scenes/Art02_Combined.unity` | The look while walking; whether doors, items, the inmate and prompts are easy to see |
| AUDIO-01 | First sound pass: footsteps, doors, handling, sales, room ambience | WindowsSample; Escape shows two volume sliders | Anything harsh, thin, too quiet or repetitive |
| DUTY-01 | Laundry duty: two-minute shift, one minute of work, Vale's turns, warning then shift failure, Rue's tool favor | WindowsLaundry; press E at the entrance board | Whether Vale gives readable chances, talking while working feels natural, and the shift has enough to do. The timing change was accepted ("OK done"); overall feel is not yet confirmed. |
| HUD-01 | Minimal text HUD, warm notification panel, hold Tab for the inventory view | WindowsLaundry | Readability, and whether the screen still feels crowded |
| INV-01 | Six-slot inventory; Dex's secret leads to Harris's key | WindowsLaundry; controls in the [log entry](progress-log.md#inv-01---functional-inventory-and-officer-secret) | Whether carrying and pocketing feel right |
| CELL-01 | Dialogue appears immediately, centered; Harris's key opens the neighboring cell | WindowsLaundry | Dialogue timing and position; the new cell |

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
| ART-02 | Lighting and materials test | Needs your review | Movement and interaction checks pass in all variants. [Log](progress-log.md#art-02-atmosphere-comparison--september-24) |
| ART-03 | Original inmate design (M5) | Done | Accepted prototype; 20 behavior checks at 0.52 m/s. [Log](progress-log.md#m5-review-faster-walking-and-earn-01-assembly--september-24) |
| EARN-01 | Snack packs and cell shelf | Done | 27 loop checks; combined prototype accepted ("it all works"). [Log](progress-log.md#earn-01-complete-earning-prototype--september-24) |
| EARN-02 | One tracked customer request | Done | 23 checks. [Log](progress-log.md#earn-02-tracked-customer-request--september-24) |
| EARN-03 | Prison clock and request deadline | Done | 33 checks. [Log](progress-log.md#earn-03-timed-request-and-player-speed--september-24) |
| GUARD-01 | One guard's suspicion and resolution | Done | 32 checks; further enforcement open. [Log](progress-log.md#guard-01-observed-trespass-and-suspicion--september-24) |
| SUPPLY-01 | Supply interruption and affected request | Done | 31 + 9 checks; trigger and duration provisional. [Log](progress-log.md#supply-01-controlled-stock-inspection--september-24) |
| SAVE-01 | Save and restore the combined sample | Done | 32 checks plus fresh-session reload. [Log](progress-log.md#save-01-manual-sample-persistence---september-24) |
| CHECK-02 | Combined Windows sample | Done | 35 executable checks; other-PC testing open. [Log](progress-log.md#check-02-combined-windows-sample---september-24) |
| AUDIO-01 | First cozy sound palette | Needs your review | 37 audio, 32 save and 37 executable checks. [Log](progress-log.md#audio-01-cozy-sound-palette) |
| PERF-01 | Performance baseline | Done | [Log](progress-log.md#perf-01-windows-performance-baseline---september-25); [report](evidence/performance-summary.md) |
| PERF-02 | Frame hitches and frame limit | Done | Presentation waiting identified; 60 FPS default. [Log](progress-log.md#perf-02-visible-profiling-and-frame-limit---september-25) |
| PERF-03 | Longer performance session | To do | Watch the isolated 226 ms pause, paused-menu outliers and memory; other-PC testing open. **High** for profiling. |
| DUTY-01 | Supervised laundry duty | Needs your review | 36 rule and 25 executable checks. [Log](progress-log.md#duty-01-verification-and-review-handoff---september-25) |
| HUD-01 | Minimal HUD, notifications, Tab inventory view | Needs your review | 36 duty and 32 executable checks. [Log](progress-log.md#hud-01-and-first-inventory-view---implemented-awaiting-review) |
| INV-01 | Six-slot inventory and Dex-to-Harris key | Needs your review | 41 rule/save, 79 Editor and 79 executable checks. [Log](progress-log.md#inv-01---functional-inventory-and-officer-secret) |
| CELL-01 | Immediate dialogue, Harris's secret, neighboring cell | Needs your review | 22 new, 106 Editor and 106 executable checks. [Log](progress-log.md#cell-01---dialogue-correction-and-harriss-neighboring-cell-key) |
| PLAYTEST-01 | Uncoached new-player test | To do | Milestone 6 exit test: a new player tries the sample without coaching; note confusion, repetition and interest. **Medium**. |
| RESEARCH-01 | Combat research | Done | [Report](research/reports/Unity%20first%20person%20melee%20combat.md) with a proposed approach, build order and 13 open decisions; findings are not decisions. [Log](progress-log.md#research-01-combat-research---september-25) |
| ENCOUNTER-01 | Neighboring-cell fight | Blocked | Real combat agreed September 25: fists, the inmate fights back, nearby objects as weapons. Waits on the user's answers to the report's open decisions (at least 1, 4 and 5). Done when the fight, death, saved outcome and completion work through the full Rue/Dex/Harris chain. [Gameplay](gameplay.md#neighboring-cell-inmate-encounter---agreed-purpose-and-real-combat-details-open) |
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
| AGENT-07 | Research specialist | To do | After RESEARCH-01; research recurs (HUD, prison rules, combat; later escape, gang AI, Steam) | Assess whether a reusable research agent adds anything beyond the built-in research workflow used for RESEARCH-01. |

For each evaluation, record whether to adopt, defer, or decline the role and why. A decision to defer or decline completes the evaluation without implying an agent was installed or run. Additional roles can be added when a concrete need appears.

Patterns to consider borrowing: Unity's [inspect/change/run/verify workflow](https://unity.com/blog/meet-the-unity-cli), and the community [gamedev-ai-agents workflow](https://github.com/ilezhnin/gamedev-ai-agents) for playable milestones, placeholder assets, and test evidence. The community kit has not been tested here; its inclusion is a research reference, not a decision to install it.

## Session handoff

- **September 25:** user agreed release 1 scope (escape, gangs/factions, several wings, real combat, Steam) and real combat for ENCOUNTER-01; recorded in the development plan, Gameplay, Prison world and Factions/events. Tracker split (TRACK-02). Checkpoint commit `3b97bcf` holds all earlier uncommitted prototype work. RESEARCH-01 finished with a combat report. No game code, scenes or builds changed.
- **Next:** the user answers the report's open decisions (at least 1, 4 and 5), then plan ENCOUNTER-01 (**High**).
- **Open user decisions:** the 13 combat decisions at the end of the [report](research/reports/Unity%20first%20person%20melee%20combat.md), including what an improvised weapon changes; release 1 quantities (wings, gangs, escape routes, combat depth); the review items above.
- **Publication:** no push attempted this session; GitHub authentication must work before pushing.
