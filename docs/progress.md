# Progress and next tasks

Last updated: September 23, 2026.

## How to use this tracker - agreed

The primary development agent maintains this file as part of each work session. There is no continuously running tracking agent. Read this file and the relevant design docs before work; update it after meaningful results and leave the next action before ending a session. Specialist/reviewer agents are optional, bounded assignments when authorized, not permanent background workers.

Task states: **To do**, **In progress**, **Blocked**, **Needs your review**, **Done**. Done means the stated checks passed; user approval of feel or appearance must come from the user. Record what was checked and distinguish untested implementation from verified behavior.

The [development plan](development-plan.md) owns milestone scope and sequencing. Design docs retain agreed/proposed/open decisions. This file owns task status; add detail to the next milestone and keep later work broad.

## Current milestone

Finish setup and make the first rough room playable. Proposed task breakdown below implements the accepted broad workflow; specific room layout and visual choices remain subject to user review.

Verified baseline: `PrisonGame/` exists, its Editor version is 6000.3.24f1, and the user opened its URP SampleScene. Gameplay implementation has not started: the Assets scripts found are template Readme scripts.

## Tasks

| ID | Task | Status | Prerequisite | Done when / evidence |
| --- | --- | --- | --- | --- |
| TRACK-01 | Establish progress tracking | Done | Accepted workflow | This tracker is linked from README; AGENTS.md specifies update and handoff responsibilities. |
| SETUP-01 | Open Unity project | Done | Editor installation | User screenshot showed PrisonGame/SampleScene in Unity 6.3 LTS; `ProjectSettings/ProjectVersion.txt` confirms 6000.3.24f1. |
| SETUP-02 | Verify CLI and MCP scene reads | Done | SETUP-01 | Both CLI `get_scene_hierarchy` and MCP `mcp__unity__get_scene_hierarchy` returned Main Camera, Directional Light, and Global Volume in this conversation. This verifies reads, not edits. |
| SETUP-03 | Establish recoverable version control checkpoint | In progress | Check repository boundary | Initialized a separate local repository at `game/`; added Unity cache exclusions and text normalization. Confirmed Visible Meta Files and text serialization. Initial commit and recovery verification are next. |
| SETUP-04 | Verify a reversible MCP scene edit | To do | SETUP-03 | Create a temporary object, confirm it, remove it, and confirm scene restoration. |
| ROOM-01 | Build rough cell, corridor, and small common area | To do | SETUP-04 | Separate prototype scene has connected spaces and collisions; temporary dimensions are recorded for review. |
| ROOM-02 | Add first-person movement and looking | To do | ROOM-01 | Player can traverse the space without passing through walls; mouse sensitivity and cursor release work. |
| ROOM-03 | Add basic interactions | To do | ROOM-02 | One door opens, one item can be picked up and put down, and one placeholder inmate responds to a simple interaction; prompts are readable. |
| ART-01 | Assemble a small visual reference board | To do | Existing art direction | Character, cell, lighting, and interface references have source links; user reviews the proposed direction. Can overlap room work. |
| CHECK-01 | Playtest, adjust, and make a Windows build | To do | ROOM-03 | User reviews movement/scale; agreed adjustments are tested; standalone build launches and basic interactions work without blocking errors. |

## Decisions and later work

- Co-op release scope remains open. Resolve it before expanding inventory, economy, AI, and saving; an early solo room does not settle it.
- Once room dimensions work, begin one representative art sample alongside the small earning loop. Finished characters, rigging, animation, and a full art set are later work.
- Add an asset register when collecting production assets: source, usage terms, editable source file, Unity location, and temporary/finished status.
- Use independent review when a substantial feature or milestone warrants it. No additional agents are running or scheduled by this document.
- Keep future gameplay milestones in the development plan; move their concrete tasks here when approaching them.

## Later tasks: evaluate additional agent roles

Agreed: retain these as future evaluation tasks. Adding or running the agents is deferred; the roles below remain candidates. Begin with gameplay testing and independent review when the first playable room warrants them. The primary agent owns the shared tracker and integrates findings; the user judges gameplay feel and visual direction.

| ID | Candidate role | Status | When to consider it | Evaluation outcome |
| --- | --- | --- | --- | --- |
| AGENT-01 | Gameplay tester | To do | ROOM-03 is playable | Define a repeatable test of doors, pickups, collisions, and restarting; assess whether a separate agent produces useful reproducible findings. |
| AGENT-02 | Independent code reviewer | To do | First substantial feature or milestone review | Trial a bounded review against requirements and record actionable issues, verification, and whether to reuse the role. |
| AGENT-03 | Art and asset specialist | To do | First representative assets are imported | Assess scale, materials, collisions, and consistency with the user-approved art direction. |
| AGENT-04 | Interface reviewer | To do | Orders, deadlines, or suspicion indicators exist | Check that cause, timing, progress, and consequences are understandable; retain user playtesting. |
| AGENT-05 | Performance specialist | To do | A representative prison scene can be measured | Define performance targets, collect measurements, and assess whether specialist investigation is useful. |
| AGENT-06 | Networking specialist | To do | Co-op is selected as a likely release requirement | Review shared item ownership and simultaneous interactions during the early multiplayer test, before expanding systems. |

For each evaluation, record whether to adopt, defer, or decline the role and why. A decision to defer or decline completes the evaluation without implying an agent was installed or run. Additional roles can be added when a concrete need appears.

Patterns to consider borrowing: Unity's [inspect/change/run/verify workflow](https://unity.com/blog/meet-the-unity-cli), and the community [gamedev-ai-agents workflow](https://github.com/ilezhnin/gamedev-ai-agents) for playable milestones, placeholder assets, and test evidence. The community kit has not been tested here; its inclusion is a research reference, not a decision to install it.

## First-stage estimate - provisional

For this estimate, first stage means the remaining setup, rough connected room, movement, basic interactions, small reference board, user playtest, and an initial Windows build. This spans the remainder of development-plan milestone 1 and milestone 2, plus early visual references. It does not mean the larger earning loop or polished playable sample is finished.

Assume a solo placeholder prototype, existing Unity/CLI/MCP installation, agent-assisted implementation, and the user available for review. These are project-specific planning ranges, not measured industry averages or a delivery promise.

| Work | Focused working hours |
| --- | --- |
| Remaining setup and recoverable checkpoint | 1-3 |
| Rough room geometry and collision | 2-4 |
| Movement and camera | 2-4 |
| Door, pickup, placeholder inmate, and prompts | 3-6 |
| Playtest adjustments and Windows build | 2-4 |
| Small visual reference board | 2-3 |
| Total | 12-24 |

Working hours include implementation, checks, troubleshooting, and active review, not just user typing time. At roughly two focused hours per working day, allow 6-12 working days. Waiting for feedback, extended Unity learning, unexpected setup failures, or scope changes adds calendar time. Custom finished models, multiplayer, economy, saving, and extensive animation are outside this range. Re-estimate after ROOM-02 using actual elapsed work; record the reason when the range changes.

## Session handoff

- Latest update: added AGENT-01 through AGENT-06 as deferred evaluations with triggers and completion criteria. No additional agents were launched or installed.
- Next action: inspect repository boundaries and establish a recoverable checkpoint (SETUP-03) before modifying game scenes.
- Current blockers: none confirmed for the next action. Repository scope must be checked before creating the checkpoint.
- User reviews upcoming: room scale/movement and visual reference direction.
